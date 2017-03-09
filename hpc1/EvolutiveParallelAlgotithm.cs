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
        
        public static void main(string[] args)
        {
            int num_individuals = 100;
            int num_variables = 7;
            int num_epoch = 100;
            Population population = new Population(num_individuals, num_variables);
            EvolutiveParallelAlgotithm algorithm  = new EvolutiveParallelAlgotithm();
            algorithm.run(population, num_epoch, args);

        }

        private void run(Population population, int numEpoch, string[] args)
        {
           // throw new NotImplementedException();

            using (new MPI.Environment(ref args))
            {
                int valreceived = -1000;
                Intracommunicator comm = Communicator.world;
                int num_nodes = Communicator.world.Size;
                int node_index = Communicator.world.Rank;
                int num_partitions = num_nodes-1;
                int range = num_nodes / num_partitions;

                if (node_index == 0)
                {
                    // send its partition to every body
                    for (int i = 1; i < num_nodes; i++)
                    {
                        //comm.Send(population.CutPopulation(i*(range),0), i, 0);
                        int val = i;
                        Console.WriteLine("Sending from " + 0 +" to "+i);
                        comm.Send(val,i,0);
                    }
                }
                else
                {
                    // wait for partition from  controller node
                    //Population partition = null;
                    //evolve(partition);

                    valreceived = comm.Receive<int>(0, 0);
                    Console.WriteLine("Received:  " + valreceived);
                    
                    valreceived++;
                    Thread.Sleep(1000);

                    // send the best element
                    //comm.Send(population.Individuals.ElementAt(0), 0, 0);
                    

                }

                if (node_index > 0) {
                  // Console.WriteLine("llegue a la barrera:  " + node_index);
                  //// comm.Barrier();
                  // Console.WriteLine("pase a la barrera:  " + node_index);

                   comm.Send(valreceived, 0, 0);
                }
                else
                {
                    //--------------
                    //procces in node 0
                    //-------------

                    int sum = 0;
                    for (int i = 1; i < num_nodes; i++)
                    {
                        int rec = comm.Receive<int>(i, 0);
                        sum += rec;
                    }
                    Console.WriteLine("Sum: " + sum);
                    
                }
               

            }

        }

        private void evolve(Population partition)
        {
           // throw new NotImplementedException();
        }
    }

    public interface IFuntion
    {
        double evaluate(double[] values);
    }

    [Serializable]
    public class Population
    {
        public Population CutPopulation(int begin, int end)
        {
            Population res = new Population();
            res.individuals = individuals.GetRange(begin, end - begin + 1);
            return res;
        }

        private Population()
        {
        }

        private List<Individual> individuals = new List<Individual>();

        public Population(int num_individuals, int num_variables)
        {

            Initialize(num_individuals, num_variables);
        }

        private void Initialize(int num_individuals, int num_variables)
        {
            double interval = 100;
            Random random = new Random();
            for (int i = 0; i < num_individuals; i++)
            {
                Individual ind = new Individual(num_variables);
                for (int j = 0; j < num_variables; j++)
                {
                    ind.Values[j] = 2*interval * random.NextDouble() - interval;
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

        public Individual(int cant)
        {
            values = new double [cant];
        }

        public double[] Values
        {
            get { return values; }
        }
    }
}
