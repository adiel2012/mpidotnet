using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPI;

namespace hpc1.BasicExamples
{
    class Barrier
    {
        static void main(string[] args)
        {
            using (new MPI.Environment(ref args))
            {
                Intracommunicator comm = Communicator.world;
                for (int i = 1; i <= 5; ++i)
                {
                    comm.Barrier();
                    if (comm.Rank == 0)
                        Console.WriteLine("Everyone is on step " + i + ".");
                }
            }
        }
    }
}
