using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyScript
{
    class Program
    {
        static void Main(string[] args)
        {
            lextool.process.Run(args[0]);

            System.Diagnostics.Debugger.Break();
        }
    }
}
