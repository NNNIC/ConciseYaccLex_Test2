using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lextool.runtime
{
    public class CFG //Config
    {
        public const int    LOOPMAX      = 9999;
    }

    public enum BREAKTYPE
    {
        NONE,
        BREAK,
        CONTINUE,
        RETURN
    }

    public class desc
    {
        public Hashtable    m_root_dic;           //ルート
        public Hashtable    m_front_dic;          //フロント
        public Hashtable    m_func_dic;           //ファンクション格納
        public Hashtable    m_funcwork_dic;       //ファンクション実行用

        public object m_cur;

        public BREAKTYPE m_breakType;

        #region api

        public const string KEY_PARENT   = "!PARENT!";
        public const string KEY_CHILD    = "!CHILD!";
        public const string KEY_FUNCTION = "!FUNCTION!";
        public const string KEY_FUNCWORK = "!FUNCWORK!"; //ファンクション実行時のワークエリア。
        public const string KEY_FUNCSAVE = "!FUNCSAVE!"; //ファンクション実行時の前ワークエリア。

        public desc()
        {
            m_root_dic = new Hashtable();
            m_root_dic[KEY_PARENT] = null;
            m_front_dic = m_root_dic;

            m_func_dic = new Hashtable();
            m_root_dic[KEY_FUNCTION] =m_func_dic;

            m_funcwork_dic  = new Hashtable();
            m_root_dic[KEY_FUNCWORK] = m_funcwork_dic;

            m_cur = null;
            m_breakType = BREAKTYPE.NONE;
        }

        public void push_blk()
        {
            var newdic = new Hashtable();
            newdic[KEY_PARENT] = m_front_dic;
            m_front_dic[KEY_CHILD] = newdic;
            m_front_dic = newdic;            
        }
        public void pop_blk()
        {
            var p = m_front_dic[KEY_PARENT];
            if (p==null) throw new SystemException("block underflow");
            m_front_dic = (Hashtable)p;
            m_front_dic.Remove(KEY_CHILD);
        }
        public void set_funcwork()
        {
            m_funcwork_dic.Clear();
            m_funcwork_dic[KEY_PARENT] = m_root_dic;    //ファンクションはグローバルを参照するため
            m_funcwork_dic[KEY_FUNCSAVE] = m_front_dic; //復帰時用
            m_front_dic = m_funcwork_dic;
        }
        public void reset_funcwork()
        {
            m_front_dic = (Hashtable)m_funcwork_dic[KEY_FUNCSAVE];
        }
        public void add_func(string name,VALUE v)
        {
            name = name.ToUpper();
            if (m_front_dic!=m_root_dic) throw new SystemException("root level can declear function");
            m_func_dic[name] = v;
        }
        public object get_func(string name)
        {
            name = name.ToUpper();
            return m_func_dic[name];
        }
        public object get(string name)
        {
            name = name.ToUpper();
            Func<Hashtable,object> _get = null;
            _get = (d)=> {
                if (d.ContainsKey(name)) return d[name];
                var p = d[KEY_PARENT];
                if (p==null) throw new SystemException(name + " is not defined");
                return _get((Hashtable)p);
            };
            return _get(m_front_dic);
        }
        public void find_and_set(string name, object o)
        {
            name = name.ToUpper();
            Action<Hashtable> _findset = null;
            _findset = (d)=> {
                if (d.ContainsKey(name)) {
                    d[name] = o;
                }
                else
                {
                    var p = d[KEY_PARENT];
                    if (p==null) throw new SystemException(name + " is not defined");
                    _findset((Hashtable)p);
                }
            };
            _findset(m_front_dic);
        }
        public void define(string name, object o)
        {
            name = name.ToUpper();
            if (m_front_dic.ContainsKey(name)) throw new SystemException("Multiple defined");
            m_front_dic[name] = o;
        }
        public desc curnull()
        {
            m_cur = null;
            return this;
        }
        public desc breaknone()
        {
            m_breakType = BREAKTYPE.NONE;
            return this;
        }
        
        public bool get_bool_cur()
        {
            if (m_cur!=null)
            { 
                if (m_cur.GetType()==typeof(bool))
                {
                    return (bool)m_cur;
                }
                else if (m_cur.GetType()==typeof(double))
                {
                    return (double)m_cur == 1;
                }
                else
                {
                    return true;
                }
            }
            return false;
        }
        public double get_number_cur()
        {
            if (m_cur!=null)
            {
                if (m_cur.GetType()==typeof(double))
                {
                    return (double)m_cur;
                }
            }
            return double.NaN;
        }
        public string get_string_cur()
        {
            if (m_cur==null) return null;
            return m_cur.ToString();
        }
        #endregion
    }

    public class processfunc
    {
        public static void Run(VALUE v)
        {
            var d  = new desc();
            var nd = run(v,d);  
        }

        private static desc run(VALUE v, desc d)
        {
            var nd =d;
            if (v.type == YDEF.get_type(YDEF.sx_main_block))
            {
                return run(v.list_at(1),nd);
            }
            if (v.type == YDEF.get_type(YDEF.sx_sentence_list))
            {
                for(int i = 0; i<v.list.Count; i++)
                {
                    nd = run(v.list_at(i),nd.curnull());
                    if (nd.m_breakType != BREAKTYPE.NONE)
                    {
                        break;
                    }
                }
                return nd;
            }
            if (v.type == YDEF.get_type(YDEF.sx_sentence_block))
            {
                if (v.list.Count==3)
                {
                    nd.push_blk();
                    nd = run(v.list_at(1),nd);
                    nd.pop_blk();
                    return nd;
                }
                throw new SystemException("unexpected");
            }
            if (v.type == YDEF.get_type(YDEF.sx_sentence))
            {
                return run(v.list[0],nd);
            }
            //--
            if (v.type == YDEF.get_type(YDEF.sx_expr_clause))
            {
                if (v.list.Count == 4)
                { 
                    var name = v.list_at(0).GetString();
                    nd = run(v.list_at(2),nd.curnull());
                    nd.find_and_set(name,nd.m_cur);
                }
                else
                {
                    nd = run(v.list_at(0),nd.curnull());
                }
                return nd;
            }
            if (v.type == YDEF.get_type(YDEF.sx_def_var_clause))
            {
                var name = v.list_at(1).GetString();
                object obj  = null;
                if (v.list.Count==5)
                {
                    nd  = run(v.list_at(3),nd.curnull());
                    obj = nd.m_cur; 
                }
                nd.define(name,obj);
                return nd;
            }
            if (v.type == YDEF.get_type(YDEF.sx_def_func_clause))
            {
                var name = v.list_at(1).list_at(0).ToString();
                nd.add_func(name,v);
            }
            if (v.type == YDEF.get_type(YDEF.sx_if_clause))
            {
                nd = run(v.list_at(1),nd.curnull());
                bool b = nd.get_bool_cur();
                if (b)
                {
                    nd = run(v.list_at(2),nd.curnull());
                }
                else if (v.list_at(3)!=null)
                {
                    nd = run(v.list_at(3),nd.curnull());
                }
                return nd;
            }   
            if (v.type == YDEF.get_type(YDEF.sx_else_clause))
            {
                nd = run(v.list_at(1),nd.curnull());
            }
            if (v.type == YDEF.get_type(YDEF.sx_for_clause))
            {
                var bkt = v.list_at(1);
                var sts = v.list_at(2);

                nd.push_blk();
                nd = run(bkt.list_at(1),nd.curnull());
                for(var loop = 0; loop<=CFG.LOOPMAX; loop++)
                {
                    nd = run(bkt.list_at(2),nd.curnull());
                    if (nd.get_bool_cur())
                    {
                        nd = run(sts,nd.curnull());                                                
                        if (nd.m_breakType == BREAKTYPE.BREAK)
                        {
                            nd.breaknone();
                            break;
                        }
                        if (nd.m_breakType == BREAKTYPE.RETURN)
                        {
                            break;
                        }
                        if (nd.m_breakType == BREAKTYPE.CONTINUE)
                        {
                            nd.breaknone();                            
                        }
                    }
                    else
                    {
                        break;
                    }
                    
                    var name = bkt.list_at(3).GetString();
                    nd = run(bkt.list_at(5),nd.curnull());
                    nd.find_and_set(name,nd.m_cur);
                }
                nd.pop_blk();
                return nd;
            }            
            if (v.type == YDEF.get_type(YDEF.sx_while_clause))
            {
                var expr = v.list_at(1);
                var sbk  = v.list_at(2);
                for(var loop = 0; loop<CFG.LOOPMAX; loop++)
                {
                    nd = run(expr,nd.curnull());
                    if (nd.get_bool_cur())
                    {
                        nd = run(sbk,nd.curnull());
                        if (nd.m_breakType == BREAKTYPE.BREAK)
                        {
                            nd.breaknone();
                            break;
                        }
                        if (nd.m_breakType == BREAKTYPE.RETURN)
                        {
                            break;
                        }
                        if (nd.m_breakType == BREAKTYPE.CONTINUE)
                        {
                            nd.breaknone();
                            continue;
                        }
                    }
                }
                return nd;
            }
            if (v.type == YDEF.get_type(YDEF.sx_switch_clause))
            {
                var sx_expr_blanket   = v.list_at(1);
                var sx_sentence_block = util.check_switch_sentence_block(v.list_at(2));

                nd = run(sx_expr_blanket,nd.curnull());
                var x = nd.m_cur;
                
                var sx_sentence_list = sx_sentence_block.FindValueByTravarse(YDEF.sx_sentence_list);
                bool b = false;
                for(int i = 0; i<sx_sentence_list.list.Count;i++)
                {
                    var vc = sx_sentence_list.list_at(i);
                    if (b)
                    {
                        nd = run(vc,nd.curnull());
                        if (nd.m_breakType == BREAKTYPE.BREAK)
                        {
                            nd.breaknone();
                            break;
                        }
                    }
                    else
                    { 
                        if (vc.IsType(YDEF.sx_case_clause))
                        {
                            var sx_case_clause = vc.FindValueByTravarse(YDEF.sx_case_clause);
                            nd = run(sx_case_clause.list_at(1),nd.curnull());
                            if (x!=null && nd.m_cur!=null && x.Equals(nd.m_cur))
                            {
                                b = true;
                            }
                        }
                        else if (vc.IsType(YDEF.sx_default_clause))
                        {
                            var sx_default_clause = vc.FindValueByTravarse(YDEF.sx_default_clause);
                            b = true;
                        }
                    }
                }
                return nd;
            }
            if (v.type == YDEF.get_type(YDEF.sx_break_clause))
            {
                nd.m_breakType = BREAKTYPE.BREAK;
                return nd;
            }
            if (v.type == YDEF.get_type(YDEF.sx_continue_clause))
            {
                nd.m_breakType = BREAKTYPE.CONTINUE;
                return nd;
            }
            if (v.type == YDEF.get_type(YDEF.sx_return_clause))
            {
                if (v.list.Count == 3)
                {
                    nd = run(v.list_at(1),nd.curnull());
                    nd.m_breakType = BREAKTYPE.RETURN;
                    return nd;
                }
                nd.m_breakType = BREAKTYPE.RETURN;
                return nd;
            }
            if (v.type == YDEF.get_type(YDEF.sx_expr_clause))
            {
                if (v.list.Count == 4)
                {
                    var name = v.list_at(0).ToString();
                    nd = run(v.list_at(2),nd.curnull());
                    nd.find_and_set(name,nd.m_cur);                    
                }
                else
                {
                    nd = run(v.list_at(0),nd.curnull());      
                }
            }
            if (v.type == YDEF.get_type(YDEF.sx_expr))
            {
                if (v.list.Count==1)
                { 
                    if (v.IsType(YDEF.NAME))
                    {
                        nd.m_cur = nd.get(v.GetString());
                        return nd;
                    }
                    if (v.IsType(YDEF.QSTR))
                    {
                        nd.m_cur = util.DelDQ(v.GetString());
                        return nd;
                    }
                    if (v.IsType(YDEF.NUM))
                    {
                        nd.m_cur = v.GetNumber();
                        return nd;
                    }
                    nd = run(v.list_at(0),nd.curnull());
                    return nd;
                }
                if (v.list.Count==3)
                {
                    nd = run(v.list_at(0),nd.curnull());
                    var a = nd.m_cur;
                    nd = run(v.list_at(2),nd.curnull());
                    var b = nd.m_cur;

                    nd.m_cur = util.Calc_op(a,b,v.list_at(1).GetString());

                    return nd;
                } 
            }
            if (v.type == YDEF.get_type(YDEF.sx_expr_blanket))
            {
                if (v.list.Count==3)
                {
                    nd = run(v.list_at(1),nd.curnull());
                }
                return nd;
            }
            if (v.type == YDEF.get_type(YDEF.sx_func))
            {
                var name = v.list_at(0).GetString();
                nd = run(v.list_at(1),nd.curnull());

                List<object> ol = null;
                if (nd.m_cur!=null)
                { 
                    if (nd.m_cur.GetType()==typeof(List<object>))
                    { 
                        ol = (List<object>)nd.m_cur;
                    }
                    else
                    {
                        ol = new List<object>();
                        ol.Add(nd.m_cur);
                    }
                }
                else
                {
                    ol = new List<object>();
                }

                var fv = (VALUE)nd.get_func(name);
                if (fv==null)
                {
                    if (predefinedfunc.IsFunc(name))
                    {
                        nd.m_cur = predefinedfunc.Run(name,ol.ToArray(),nd.curnull());
                        return nd;
                    }
                    throw new SystemException("function is not defined:" + name);
                }

                nd.set_funcwork();
                {
                    var fvbk = util.normalize_func_blancket(fv.list_at(1).list_at(1)); //ファンクション定義部の引数部分
                    int n = 0;
                    if (fvbk!=null) for(int i = 0; i<fvbk.list.Count; i+=2)
                    {
                        var varname = fvbk.list_at(i).GetString();//定義側の変数名
                        object o = ol!=null && n < ol.Count ? ol[n] : null;
                        nd.define(varname, o);
                        n++;
                    }
                    nd = run(fv.list_at(2),nd);
                    nd.breaknone();
                }
                nd.reset_funcwork();
                return nd;
            }
            if (v.type == YDEF.get_type(YDEF.sx_def_func_clause))
            {
                var n = v.list_at(1).list_at(0).GetString();
                nd.add_func(n,v);
                return nd;
            }
            if (v.type == YDEF.NAME)
            {
                var n = v.GetString();
                nd.m_cur = nd.get(n);
                return nd;            
            }
            if (v.type == YDEF.NUM)
            {
                var n = v.GetNumber();
                nd.m_cur = n;
                return nd;            
            }
            if (v.type == YDEF.QSTR)
            {
                var n = v.GetString();
                nd.m_cur = n;
                return nd;            
            }
            return nd;            
        }
    }

    public class util
    {
        public static bool is_paramlist(VALUE v)
        {
            if (v.type == YDEF.get_type(YDEF.sx_expr))
            {
                if (v.list.Count>=3)
                { 
                    for(int i = 1; i < v.list.Count; i+=2)
                    {
                        if (v.list_at(i).GetString()!=",") return false;            
                    }
                    return true;
                }
            }
            return false;
        }
        public static VALUE normalize_func_blancket(VALUE v)
        {
            if (v.type != YDEF.get_type(YDEF.sx_expr_blanket)) throw new SystemException("unexpected");

            Func<VALUE,VALUE> comb = null;
            comb = (w) => {
                if (!is_paramlist(w))
                {
                    return w;                    
                }
                var x = comb(w.list_at(0));
                var c = w.list_at(1); //, comma
                var y = w.list_at(2); //
                if (is_paramlist(x))
                {
                    w.list.Clear();
                    w.list.Add(x.list_at(0));
                    w.list.Add(x.list_at(1));
                    w.list.Add(x.list_at(2));
                    w.list.Add(c);
                    w.list.Add(y);
                }
                return w;
            };
            
            if (v.list.Count==3)
            {
                var nv = comb(v.list_at(1));
                return nv;    
            }
            return null;
        }

        public static VALUE check_switch_sentence_block(VALUE v)
        {
            if (v.type != YDEF.get_type(YDEF.sx_sentence_block)) throw new System.Exception("unexpected switch block #1");
            var inblock = v.list_at(1);
            if (inblock.type == YDEF.get_type(YDEF.sx_sentence))
            {
                check_case(inblock);
                return v;
            }
            if (inblock.type == YDEF.get_type(YDEF.sx_sentence_list))
            {
                var list = inblock.list_at(0);
                for(int i = 0; i<list.list.Count; i++)
                {
                    check_case(list.list_at(i));
                }
                return v;
            }
            throw new SystemException("unexpected switch block #2");
        }
        private static void check_case(VALUE v)
        {
            if (v.IsType(YDEF.sx_case_clause))
            {
                var expr = v.list_at(1);                
                if (expr.IsType(YDEF.QSTR) || expr.IsType(YDEF.NUM))
                {
                    ;//ok
                }
                else
                {
                    throw new SystemException("unexpected case sentence");
                }
            }
            else if (v.IsType(YDEF.sx_default_clause))
            {
                ; //ok
            }
            else if (v.IsType(YDEF.sx_sentence))
            {
                ;//ok
            }
            else
            {
                throw new SystemException("unexpected switch senetence");
            }
        }
        public static string DelDQ(string i)
        {
            var s = i;
            if (string.IsNullOrEmpty(i)) return "";
            if (s.StartsWith("\"")) s=s.Substring(1);
            if (s.EndsWith("\""))   s=s.Substring(0,s.Length-1);
            return s;
        }
        public static object Calc_op(object a, object b, string op)
        {
            if (op==",")
            {
                List<object> l = null;
                if (a.GetType()==typeof(List<object>))
                {
                    l = (List<object>)a;
                }
                else
                {
                    l = new List<object>();
                    l.Add(a);
                }
                if (b.GetType()==typeof(List<object>))
                {
                    var nl = (List<object>)b;
                    l.AddRange(nl);
                }
                else
                {
                    l.Add(b);
                }
                return l;
            }

            if (a.GetType()==typeof(string))
            {
                var x = a.ToString();
                var y = b.ToString();
                switch(op)
                {
                    case "+":   return x + y;
                    case "==":  return (bool)(x==y);
                    case "!=":  return (bool)(x!=y);
                    default:    throw new SystemException("unexpected string operaion");                   
                }
            }
            else if (a.GetType()==typeof(double))
            {
                var x = (double)a;
                var y = (double)b;
                
                switch(op)
                {
                    case "+":   return x+y;
                    case "-":   return x-y;
                    case "*":   return x*y;
                    case "/":   return x/y;
                    case "%":   return x%y;
                    case "==":  return (bool)(x==y);
                    case "!=":  return (bool)(x!=y);
                    case ">":   return (bool)(x>y);
                    case ">=":  return (bool)(x>=y);
                    case "<":   return (bool)(x<y);
                    case "<=":  return (bool)(x<=y);
                    default:    throw new SystemException("unexpected number operaion");                   
                }
            }
            else if (a.GetType()==typeof(bool))
            {
                var x = (bool)a;
                var y = (bool)b;

                switch(op)
                {
                    case "==":  return (bool)(x==y);
                    case "!=":  return (bool)(x!=y);
                    default:    throw new SystemException("unexpected bool operaion");                   
                }
            }
            throw new SystemException("unexpected calc op");                   
        }
    }
}
