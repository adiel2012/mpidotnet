using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MPI;

namespace hpc1
{
    class EvolutiveParallelAlgotithm
    {

        static int num_individuals = 25000;
        static int num_variables = 2;
        static int num_epoch = 100;

        Random random = new Random();
        public static void main(string[] args)
        {
            Population population = new Population(num_individuals, num_variables);
            EvolutiveParallelAlgotithm algorithm = new EvolutiveParallelAlgotithm();

            algorithm.Run(population, num_epoch, args, num_epoch,
                values => -(Math.Pow(values[0] - 2.5, 2) + Math.Pow(values[1] - 1, 2) + 8));
        }

        private void Run(Population population, int numEpoch, string[] args, int epochs, Function fun)
        {


            using (new MPI.Environment(ref args))
            {
                int valreceived = -1000;
                Intracommunicator comm = Communicator.world;
                int num_nodes = Communicator.world.Size;
                int node_index = Communicator.world.Rank;
                int num_partitions = num_nodes;

                int range = population.Individuals.Count() / num_nodes;
                if (population.Individuals.Count() % num_nodes != 0)
                    range = population.Individuals.Count() / (num_nodes - 1);

                num_partitions += (population.Individuals.Count() % num_nodes != 0 ? 1 : 0);
                Population node_population = null;

                if (node_index == 0)
                {
                    node_population = population.CutPopulation(0, range - 1);
                    // send its partition to every node
                    for (int i = 1; i < num_nodes; i++)
                    {
                        int ini = (i) * (range);
                        int end = Math.Min(population.Individuals.Count() - 1, (i + 1) * (range) - 1);
                        
                        comm.Send(population.CutPopulation(ini, end).ToArray(), i, 0);
                        
                    }
                }
                else
                {
                   

                    double[] values = new double[num_nodes * num_individuals + 2];
                    comm.Receive(0, 0, ref values);
                    node_population = new Population(values);
                    


                }

                Evolve(node_population, fun, epochs, node_index);
                Console.WriteLine("Finish Node: " + node_index);

                if (node_index > 0)
                {
                    comm.Send(node_population.Individuals.ElementAt(0).Values.Clone(), 0, 0);
                }
                else
                {
                    Individual best_individual = node_population.Individuals.ElementAt(0);
                    double best_eval = fun(best_individual.Values);
                    int sum = 0;
                    for (int i = 1; i < num_nodes; i++)
                    {
                        double[] values = new double[num_nodes * num_individuals + 2];
                        comm.Receive(i, 0, ref values);

                        Individual curr = new Individual(values);
                        double eval = fun(best_individual.Values);

                        if (eval > best_eval)
                        {
                            best_eval = eval;
                            best_individual = curr;
                        }

                    }

                    Console.WriteLine("Finish Eval: " + best_eval + "  " + best_individual[0] + "  " + best_individual[1]);
                }


            }

        }


        private void Evolve(Population partition, Function f, int epochs, int node_index)
        {
           
            int N = partition.Individuals.Count();
            double granularity = 0.1;
            double[] evals = new double[N];
            for (int i = 0; i < epochs; i++)
            {
                Console.WriteLine("Node: "+node_index+" --> Epoch: "+i);
                //modify and evaluate
                for (int j = 0; j < N; j++)
                {
                    Individual ind = partition.Individuals.ElementAt(j);
                    for (int k = 0; k < ind.NumVariables; k++)
                    {
                        ind[k] = ind[k] + granularity * random.NextDouble() - (granularity/2);
                    }
                    // evaluate
                    evals[j] = f(ind.Values);
                }

                // arrange
                double temp = 0;
                Individual itemp;
                for (int j = 0; j < N - 1; j++)
                {
                    for (int k = j + 1; k < N; k++)
                    {
                        if (evals[j] < evals[k])
                        {  //swap
                            temp = evals[k];
                            evals[k] = evals[j];
                            evals[j] = temp;

                            itemp = partition.Individuals[k];
                            partition.Individuals[k] = partition.Individuals[j];
                            partition.Individuals[j] = itemp;
                        }
                    }

                }


                // selective preasure
                for (int j = 0; j < N/2 ; j++)
                {
                    partition.Individuals[j + N / 2] = new Individual(partition.Individuals[j].Values) ;
                    evals[j + N / 2] = evals[j];
                }

            }
        }
    }

    public delegate double Function(double[] values);



    [Serializable]
    public class Population
    {

        public double[] ToArray()
        {
            int num_individuals = Individuals.Count();
            double[] res = new double[num_individuals * num_variables + 2];
            int pos = 2;
            res[0] = num_individuals;
            res[1] = num_variables;

            for (int i = 0; i < num_individuals; i++)
            {
                Individual ind = individuals.ElementAt(i);
                for (int j = 0; j < num_variables; j++)
                {
                    res[pos++] = ind[j];
                }
            }
            return res;
        }

        public Population(double[] vals)
        {
            //int num_individuals = Individuals.Count();
            int num_individuals = (int)vals[0];
            num_variables = (int)vals[1];
            int pos = 2;
            for (int i = 0; i < num_individuals; i++)
            {
                Individual ind = new Individual(num_variables);

                for (int j = 0; j < num_variables; j++)
                {
                    ind[j] = vals[pos++];
                }
                individuals.Add(ind);
            }
        }



        public Population CutPopulation(int begin, int end)
        {
            Population res = new Population();
            res.individuals = individuals.GetRange(begin, end - begin + 1);
            res.num_variables = num_variables;
            return res;
        }

        private Population()
        {
        }

        private List<Individual> individuals = new List<Individual>();

        public Population(int num_individuals, int num_variables)
        {
            this.num_variables = num_variables;

            Initialize(num_individuals, num_variables);
        }

        Random random = new Random();
        private int num_variables;

        private void Initialize(int num_individuals, int num_variables)
        {
            double interval = 100;

            for (int i = 0; i < num_individuals; i++)
            {
                Individual ind = new Individual(num_variables);
                for (int j = 0; j < num_variables; j++)
                {
                    ind.Values[j] = 2 * interval * random.NextDouble() - interval;
                }
                individuals.Add(ind);
            }
        }

        public List<Individual> Individuals
        {
            get { return individuals; }
        }
    }

    [Serializable]
    public class Individual
    {
        private double[] values;

        public int NumVariables
        {
            get { return values.Length; }
        }

        public double this[int index]
        {
            get { return values[index]; }
            set { values[index] = value; }
        }

        public Individual(int cant)
        {
            values = new double[cant];
        }

        public Individual(double[] pvals)
        {
            // TODO: Complete member initialization
            this.values = (double[])pvals.Clone();
        }

        public double[] Values
        {
            get { return values; }
        }
    }
}
