﻿using System;
using MPI;

class Ring
{
    static void main(string[] args)
    {
        using (MPI.Environment env = new MPI.Environment(ref args))
        {
            Intracommunicator comm = MPI.Communicator.world;
            if (comm.Size < 2)
            {
                // Our ring needs at least two processes
                System.Console.WriteLine("The Ring example must be run with at least two processes.");
                System.Console.WriteLine("Try: mpiexec -np 4 ring.exe");
            }
            else if (comm.Rank == 0)
            {
                // Rank 0 initiates communication around the ring
                string data = "Hello, World!";

                // Send "Hello, World!" to our right neighbor
                comm.Send(data, (comm.Rank + 1) % comm.Size, 0);

                // Receive data from our left neighbor
                comm.Receive((comm.Rank + comm.Size - 1) % comm.Size, 0, out data);

                // Add our own rank and write the results
                data += " 0";
                System.Console.WriteLine(data);
            }
            else
            {
                // Receive data from our left neighbor
                String data;
                comm.Receive((comm.Rank + comm.Size - 1) % comm.Size, 0, out data);

                // Add our own rank to the data
                data = data + " " + comm.Rank.ToString() + ",";

                // Pass on the intermediate to our right neighbor
                comm.Send(data, (comm.Rank + 1) % comm.Size, 0);
            }
        }
    }
}

