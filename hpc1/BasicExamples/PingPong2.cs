using System;
using MPI;

class PingPong2
{
    public static void main(string[] args)
    {
        using (new MPI.Environment(ref args))
        {
            int cant = 2;
            Intracommunicator comm = Communicator.world;
            if (comm.Rank == 0)
            {
                Console.WriteLine("Rank 0 is alive and running on " + MPI.Environment.ProcessorName);
                for (int dest = 1; dest < comm.Size; ++dest)
                {
                    Console.Write("Pinging process with rank " + dest + "...");
                    double[] vals = {6,9};
                    comm.Send(vals, dest, 0);
                    Console.Write("Sended");

                    double[] values = new double[2];
                    comm.Receive(dest, 1, ref values);
                    Console.Write("Devolver");

                    foreach (double v in values)
                        Console.WriteLine(v);
                    
                    //Console.WriteLine(" Pong!");
                    //Console.WriteLine("  Rank " + dest + " is alive and running on " + destHostname);
                }
            }
            else
            {
                double[] values = new double[2];
                comm.Receive(0, 0,ref values);
                Console.Write("Received " + comm.Rank);
                values[0]++;
                comm.Send(values, 0, 1);
            }
        }
    }
}
