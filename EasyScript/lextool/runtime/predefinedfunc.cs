using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lextool.runtime
{
    class predefinedfunc
    {
        public class item
        {
            public string name;
            public Func<object[],object> func;
        }

        public static Hashtable m_hash;
        
        public static void Init()
        {
            m_hash = new Hashtable();

            Action<string,Func<object[],object>> set = (n,f) => { 
                m_hash[n] = new item() { name = n, func = f };
            };

            set("PRINT",ConsoleWrite);
            set("CONSOLEWRITE",ConsoleWrite);
            set("CONSOLEWRITELINE",ConsoleWriteLine);
        }
        public static bool IsFunc(string name)
        {
            var i = (item)m_hash[name.ToUpper()];
            return (i!=null);
        }
        public static object Run(string name, object[] ol,desc d)
        {
            var i = (item)m_hash[name.ToUpper()];
            if (i ==null) return null;

            return i.func(ol);
        }
                
        //--- 組み込み関数
        static object Print(object[] ol)
        {
            var o = ol_at(ol,0);
            Console.Write(o);
            return null;
        }
        static object ConsoleWrite(object[] ol)
        {
            var o = ol_at(ol,0);
            Console.Write(o);
            return null;
        }
        static object ConsoleWriteLine(object[] ol)
        {
            var o = ol_at(ol,0);
            Console.WriteLine(o);
            return null;
        }

        //--- このクラス用のtool
        static object ol_at(object[] ol,int n)
        {
            if (ol==null || n < 0 || ol.Length<=n ) return null;
            return ol[n];
        }
    }
}
