using System;
using MPI;

class Pi
{
    static void main(string[] args)
    {
        int dartsPerProcessor = 10000;
        using (new MPI.Environment(ref args))
        {
            if (args.Length > 0)
                dartsPerProcessor = Convert.ToInt32(args[0]);

            Intracommunicator world = Communicator.world;
            Random random = new Random(5 * world.Rank);
            int dartsInCircle = 0;
            for (int i = 0; i < dartsPerProcessor; ++i)
            {
                double x = (random.NextDouble() - 0.5) * 2;
                double y = (random.NextDouble() - 0.5) * 2;
                if (x * x + y * y <= 1.0)
                    ++dartsInCircle;
            }

            if (world.Rank == 0)
            {
                int totalDartsInCircle = world.Reduce<int>(dartsInCircle, Operation<int>.Add, 0);
                System.Console.WriteLine("Pi is approximately {0:F15}.",
                    4 * (double)totalDartsInCircle / (world.Size * (double)dartsPerProcessor));
            }
            else
            {
                world.Reduce<int>(dartsInCircle, Operation<int>.Add, 0);
            }
        }
    }
}

