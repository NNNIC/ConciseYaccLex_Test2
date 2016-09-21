using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lextool
{
    public class yanalyzer
    {
        public static bool Analyze(List<VALUE> src, out List<VALUE> dst)
        {
            const int LOOPMAX = 10000;

            dst = new List<VALUE>(src);

            var list = new List<VALUE>(src);

            for(int loop = 0; loop <= LOOPMAX; loop++)
            {
                YDEF_DEBUG.DumpLine_detail(dst);

                if (loop == LOOPMAX) sys.error("yanalyzer Analyze LoopMax:1"); 

                var syntax_order = YDEF.get_syntax_order();

                bool bNeedLoop=false;
                for(int i = 0; i < syntax_order.Count; i++)
                {
                    var syntax = syntax_order[i];
                    var tslist = YDEF.get_syntax_set(syntax);

                    foreach (var ts in tslist)
                    {
                        if (_check_syntax(dst,ts))
                        {
                            bNeedLoop = true;
                            break;
                        }
                    }
                    if (bNeedLoop) break; //最初から
                }

                if (bNeedLoop)
                { 
                    continue;
                }
                else
                {
                    break;
                }
            }

            return true;
        }
        private static bool _check_syntax(List<VALUE> list, YDEF.TreeSet ts)
        {
            int start = find_deepest_bracket(list);                          //括弧の中を優先処理
            int end   = start >=0 ? list.FindIndex(start,v=>v.s==")") : -1;  //括弧の中を優先処理
            if (start < 0) start = 0;
            if (end < 0) end = list.Count;
            for(var i = start; i<end; i++)
            {
                if (_isMatchAndMake(list,i,ts))
                {
                    sys.logline("\n match ..." + ": list[" + i + "] " + ts.ToString() +">" + YDEF_DEBUG.PrintValue(list[i]));
                    return true;
                }
            }
            return false;
        }
        private static bool _isMatchAndMake(List<VALUE> list, int index, YDEF.TreeSet ts)
        {
            Func<int,VALUE> get = (n) => {
                if (n >= list.Count) return null;
                return list[n];
            };
            List<VALUE> args = new List<VALUE>();
            int removelength = ts.list.Count;

            for (int i = 0; i<ts.list.Count; i++)
            {
                var v = get(index + i);
                if (v==null) return false;

                if (i==0 && ts.list.Count==1 && v.IsType(ts.type)) return false; //既に変換済み

                int tstype = 0;
                var o = ts.list[i];
                VALUE nv = null;
                if (o.GetType() == typeof(string))
                {
                    if (v.GetString() == (string)o)
                    { 
                        nv = v.GetTerminalValue_ascent();
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    tstype = (int)o;
                    if (tstype == YDEF.REST) // ※RESTは特殊処理。EOLまでのすべて(除EOL)が入る
                    {
                        removelength--; //本VALUE分を事前に引く

                        var restv = new VALUE();
                        restv.type = YDEF.REST;
                        restv.list = new List<VALUE>();

                        for (int j = index + i; j < list.Count; j++)
                        {
                            var v2 = get(j);
                            if (v2 != null && !v2.IsType(YDEF.EOL))
                            {
                                restv.list.Add(v2);
                                removelength++;
                            }
                            else
                            {
                                break;
                            }
                        }

                        args.Add(restv);
                        break;
                    }
                    else
                    {
                        if (v.IsType(tstype))
                        { 
                            nv = v.FindValueByTravarse(tstype);
                        }
                        else
                        {
                            return false;
                        }
                    }
                }

                if (nv!=null)
                {
                    args.Add(nv);
                }
                else
                {                 
                    args.Add(v);
                }
            }

            //Yes, they match! then Make it.

            var makefunc = (Func<int, VALUE[], int[], VALUE>)ts.make_func;

            var newv = ts.make_func(ts.type,args.ToArray(),ts.make_index.ToArray());

            list.RemoveRange(index, removelength);
            list.Insert(index,newv);

            return true;
        }

        // --- tool for this class
        private static int find_deepest_bracket(List<VALUE> l)
        {
            int find = -1;
            int max_nextcount = 0;
            int nestcount = 0;
            for(int n = 0; n<l.Count; n++)
            { 
                var i = l[n];
                var str = i.GetString();
                if (str=="(")
                {
                    nestcount++;
                    if (max_nextcount<nestcount)
                    {
                        max_nextcount = nestcount;
                        find = n;
                    }
                }
                else if (str == ")")
                {
                    nestcount--;
                }
            }
            return find;
        }
    }
}
