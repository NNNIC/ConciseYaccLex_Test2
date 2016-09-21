using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lextool
{
    public class YDEF_DEBUG
    {
        public static string NL { get {return Environment.NewLine; } }

        public static bool IsExecutable(List<VALUE> list, out int errorline)
        {
            errorline = -1;
            if (list.Count==1 && list[0].IsType(YDEF.get_type("sx_main_block")))
            {
                return true;
            }
            int _el = -1;
            Action<VALUE> check_sentence = null;
            check_sentence = (v) =>
            {
                if (_el>=0) return;
                if (v.type == YDEF.BOF || v.type == YDEF.EOF) return;

                if (v.type == YDEF.get_type("sx_sentence_list"))
                {
                    if (v.list!=null) v.list.ForEach(i=>check_sentence(i));
                }
                if (v.type == YDEF.get_type("sx_sentence_block"))
                {
                    if (v.list!=null && v.list.Count>=2) check_sentence(v.list[1]);
                }
                if (v.type != YDEF.get_type("sx_sentence"))
                {
                    _el = v.get_dbg_line();
                }
            };

            list.ForEach(i=>check_sentence(i));

            errorline = _el;

            return errorline < 0;
        }


        #region Dump
        public static void DumpList(List<List<VALUE>> list, bool bOmitTerminalType = false)
        {
            foreach(var l in list)
            {
                DumpLine_detail(l,bOmitTerminalType);
            }
        }
        public static void DumpLine_detail(List<VALUE> l,bool bOmitTerminalType=false)
        {
            // [type|?|0[]1[]2[]
            string s =null;
            Action<VALUE> work = null;
            work = (v) => {
                var tm = v.GetTerminal();
                var tn = v.get_type_name();
                if (v.type < (int)TOKEN.MAX && bOmitTerminalType) tn = "";

                if (tn=="sx_sentence") s+=Environment.NewLine;
                s += "[";
                s +=  tn.Replace("sx_","") /*+ ">"*/ + (tm!=null ? ("`" + tm + "`") :"");

                if (v.list!=null)
                {
                    for(int i = 0; i<v.list.Count; i++)
                    {
                        if (v.list.Count>1) s+=i.ToString();
                        work(v.list[i]);
                    }
                }

                s +="]";
                if (tn=="sx_sentence") s+=Environment.NewLine;
            };
            
            l.ForEach(i=>work(i));

            sys.logline(s);
        }
        #endregion

        #region Print
        public static void PrintListValue(List<VALUE> l)
        {
            var s = "";
            l.ForEach(v=>s+=PrintValue(v)); 
            sys.logline(s);
        }
        public static string PrintValue(VALUE v)
        {
            foreach(var e in Enum.GetValues(typeof(TOKEN)))
            {
                var i = (int)e;
                if (i>0 && i<(int)TOKEN.MAX)
                {
                    VALUE find = v.IsType(i) ? v.FindValueByTravarse(i) : null;
                    if (find!=null)
                    {
                        if (i==(int)TOKEN.BOF || i==(int)TOKEN.EOF) return "----" + NL; 
                        return find.o.ToString();
                    }
                }
            }

            Func<string,string> conv = (j) => {
                if (j==";") return j  + NL;
                if (j=="{") return NL + j + NL;
                if (j=="}") return j  + NL;
                if (string.IsNullOrEmpty(j)) return j;
                return  j.StartsWith(" ") ? j : " " + j ; 
            };

            string s = "";
            if (v.list!=null)
            {
                v.list.ForEach(i=>s+=conv(PrintValue(i)));
            }
            return s;
        }
        #endregion
    }
}
