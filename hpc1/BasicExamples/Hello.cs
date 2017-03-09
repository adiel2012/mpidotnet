using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.Text;
using MPI;

class Hello
{
    public static void main(string[] args)
    {
        using (new MPI.Environment(ref args))
        {
            System.Console.WriteLine("Hello, from process number "
                + MPI.Communicator.world.Rank.ToString() + " of "
                + MPI.Communicator.world.Size.ToString());

        }
    }
}

