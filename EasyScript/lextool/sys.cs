using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lextool
{
    public static class sys
    {
        public static void error(string s, VALUE v = null)
        {
            int line = -1;
            if (v!=null) line = v.get_dbg_line();
            
            string es = "ERROR"+ (line>=0 ? "(L:" + (line+1).ToString() + ")" : "") + ":" + s;

            Console.WriteLine(es);
            throw new SystemException(es);
        }

        public static void log(string s)
        {
            Console.Write(s);
        }

        public static void logline(string s=null)
        {
            if (s==null)
            {
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine(s);
            }
        }
    }
}
