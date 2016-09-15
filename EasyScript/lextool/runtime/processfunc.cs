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
        public Hashtable    m_func_dic;

        public object m_cur;

        public BREAKTYPE m_breakType;

        #region api

        public const string KEY_PARENT   = "!PARENT!";
        public const string KEY_CHILD    = "!CHILD!";
        public const string KEY_FUNCTION = "!FUNCTION!";

        public desc()
        {
            m_root_dic = new Hashtable();
            m_root_dic[KEY_PARENT] = null;
            m_front_dic = m_root_dic;

            m_func_dic = new Hashtable();
            m_root_dic[KEY_FUNCTION] =m_func_dic;

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
        public void add_func(string name,VALUE v)
        {
            if (m_front_dic!=m_root_dic) throw new SystemException("root level can declear function");
            m_func_dic[name] = v;
        }
        public object get(string name)
        {
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
            Action<Hashtable> _findset = null;
            _findset = (d)=> {
                if (d.ContainsKey(name)) d[name] = o;
                var p = d[KEY_PARENT];
                if (p==null) throw new SystemException(name + " is not defined");
                _findset((Hashtable)p);
            };
            _findset(m_front_dic);
        }
        public void define(string name, object o)
        {
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
                    var name = v.list_at(0).ToString();
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
                var name = v.list_at(1).ToString();
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
                bool b = false;
                if (nd.m_cur!=null)
                {
                    if (nd.m_cur.GetType()==typeof(bool) && (bool)nd.m_cur == true) b = true;
                    if (nd.m_cur.GetType()==typeof(double) && (double)nd.m_cur != 0) b = true;
                }
                if (b)
                {
                    nd = run(v.list_at(2),nd);
                }
                else if (v.list_at(3)!=null)
                {
                    nd = run(v.list_at(3),nd);
                }
                return nd;
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
                            continue;
                        }
                    }
                    else
                    {
                        break;
                    }
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
                            nd = run(vc.list_at(1),nd.curnull());
                            if (x == nd.m_cur)
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

            return nd;            
        }


    }

    public class util
    {
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
    }
}
