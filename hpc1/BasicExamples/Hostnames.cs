
using System;
using MPI;

class Hostnames
{
    static void main(string[] args)
    {
        using (new MPI.Environment(ref args))
        {
            string[] hostnames = null;
            Communicator.world.Gather(MPI.Environment.ProcessorName, 0, ref hostnames);
            if (Communicator.world.Rank == 0)
            {
                System.Array.Sort(hostnames);
                foreach (string host in hostnames)
                    System.Console.WriteLine(host);
            }
        }
    }
}

