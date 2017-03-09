using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using MPI;

namespace hpc1
{
    class Program
    {
        static void Main(string[] args)
        {
            //using (new MPI.Environment(ref args))
            //{
            //    // MPI program goes here!
            //    //Console.WriteLine("Hello, World! from rank " + Communicator.world.Rank
            //    //  + " (running on " + MPI.Environment.ProcessorName + ")");
            //    Console.WriteLine(Communicator.world.Rank + "-" + Communicator.world.Size);
               
            //}

            EvolutiveParallelAlgotithm.main(args);
        }
    }
}
