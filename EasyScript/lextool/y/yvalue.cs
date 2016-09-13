using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lextool
{
    public class VALUE
    {
        public double n;
        public string s;
        public object o;

        public int type;
        public List<VALUE> list;
        public bool IsType(int itype)
        {
            if (itype == type) return true;
            if (list != null && list.Count == 1)//保持要素が１つの場合は、そのタイプもチェック対象
            {
                return list[0].IsType(itype);
            }
            return false;
        }
        public bool IsType(string s)
        {
            int tp = YDEF.get_type(s);
            return IsType(tp);
        }
        public string GetString()
        {
            if (s != null) return s;
            if (list != null && list.Count == 1)
            {
                return list[0].GetString();
            }
            return null;
        }
        public string GetTerminal()
        {
            if (s!=null) return s;
            if (o!=null && o.GetType()==typeof(double)) return o.ToString();
            return null;
        }
        public object GetTerminalObject()
        {
            if (o != null)
            {
                var t = o.GetType();
                if (t == typeof(string) || t==typeof(double))
                {
                    return o;
                }
            }
            return null;
        }
        public object GetTerminalObject_ascent()//遡って取得
        {
            var o = GetTerminalObject();
            if (o!=null) return o;
            if (list != null && list.Count == 1)
            {
                o = list[0].GetTerminalObject_ascent();
            }
            return o;
        }
        public VALUE GetTerminalValue_ascent()
        {
            if (list!=null && list.Count==1)
            {
                return list[0].GetTerminalValue_ascent();
            }
            return this;
        }

        #region //デバッグ
        public int dbg_line;
        public int dbg_col;

        public override string ToString()
        {
            string s = null;

            s += type.ToString() + ":" + YDEF.get_name(type);

            return s + ":" + (o != null ? o.ToString() : "null");
        }
        public string get_type_name()
        {
            return YDEF.get_name(type);
        }
        public string get_ascent_types() //タイプを遡って纏めて文字列化。listの先頭のみが対象
        {
            string s = null;
            Action<VALUE> printtype = null;
            printtype = (v) =>
            {
                if (v==null) return;
                if (s!=null) s+="-";
                s+= YDEF.get_name(v.type);
                if (v.list != null && v.list.Count > 0)
                {
                    printtype(v.list[0]);
                }
            };

            printtype(this);

            return s;
        }
        public string get_all_terminals() //全終端記号を出力
        {
            string s = null;
            Action<VALUE> print_terminals = null;
            print_terminals = (v) => {
                var n = v.GetTerminal();
                if (n != null)
                {
                    if (s != null) s += ",";
                    s += n;
                }
                else
                {
                    if (v.list != null) foreach (var v2 in v.list)
                    {
                        print_terminals(v2);
                    }
                }
            };
            print_terminals(this);
            return s;
        }
        public int get_dbg_line()
        {
            int line = -1;
            Travarse(v => {
                if (Enum.IsDefined(typeof(TOKEN),v.type))
                {
                    line = v.dbg_line;
                    return true;
                }
                return false;
            });
            return line;
        }
        #endregion

        #region //実行時用
        public VALUE FindValueByTravarse(int itype) //指定タイプをトラバースして検索　(listを辿りながら)
        {
            if (itype == type) return this;
            if (list==null) return null;

            for(int i = 0; i<list.Count;i++)//１．中のタイプのみを確認
            {
                if (list[i].type == itype) return list[i];
            }
            for(int i = 0; i<list.Count;i++)//２．一つずつ中を検索
            {
                var v = list[i].FindValueByTravarse(itype);
                if (v!=null) return v;
            }

            return null;
        }

        public bool ReplaceValueByTravarse(int itype, VALUE dst)//トラバースして、最初に見つけたのを入れ替える。
        {
            if (itype==type)//自身の入れ替えはＮＧ
            {
                sys.logline("Unexpected. Cannot replace self");
                return false;
            }
            if (list == null) return false;

            for (int i = 0; i<list.Count; i++)
            {
                if (list[i].type == itype)
                {
                    list[i] = dst;
                    return true;
                }
            }
            for(int i = 0; i<list.Count;i++)
            {
                if (list[i].ReplaceValueByTravarse(itype,dst))
                {
                    return true;
                }
            }
            return false;
        }

        public VALUE get_child(int index)
        {
            if (index >= list.Count) { sys.logline("get_child index exceeded"); return null; }
            var v = list[index];
            return v;
        }

        public void Travarse(Func<VALUE,bool> func)//汎用トラバース  funcの戻り値がtrue時は、以降の確認をしない。
        {
            bool bDone = false;
            Action<VALUE> work = null;
            work = (v) => {
                if (!bDone)
                {
                    bDone = func(v);
                    if (bDone) return;
                }
                if (v.list != null) {
                    for (int i = 0; i < v.list.Count; i++)
                    {
                        bDone = func(v.list[i]);
                        if (bDone) return;
                    }
                    for (int i = 0; i < v.list.Count; i++)
                    {
                        work(v.list[i]);
                        if (bDone) return;
                    }
                }
            };
            work(this);
        }
        #endregion

        #region static 
        public static VALUE BOF()
        {
            var v = new VALUE();
            v.type = YDEF.BOF;
            v.s = "BOF";
            v.o = v.s;
            return v;
        }
        public static VALUE EOF()
        {
            var v = new VALUE();
            v.type = YDEF.EOF;
            v.s = "EOF";
            v.o = v.s;
            return v;
        }
        #endregion
    }
}
