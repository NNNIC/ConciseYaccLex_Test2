using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lextool.runtime
{
    public static class FuncUtil
    {
        public static string get_funcname(VALUE v)
        {
            //if (v.type == get_type(YDEF.sx_function))
            //{
            //    if (v.list != null && v.list.Count > 0)
            //    {
            //        return v.list[0].GetString();
            //    }
            //}
            sys.error("Runtime/get_funcname", v);
            return null;
        }
        public static object[] get_parameters(VALUE v)
        {
            //if (v.type == get_type(YDEF.sx_function))
            //{
            //    VALUE find = v.FindValueByTravarse(get_type(YDEF.sx_param_list));
            //    if (find != null && find.list != null)
            //    {
            //        List<object> olist = new List<object>();
            //        foreach (var i in find.list)
            //        {
            //            var o = i.GetTerminalObject_ascent();
            //            if (o == null) sys.error("Runtime/get_parameters");
            //            olist.Add(o);
            //        }
            //        return olist.ToArray();
            //    }
            //    find = v.FindValueByTravarse(get_type(YDEF.sx_param));
            //    if (find != null)
            //    {
            //        var o = find.GetTerminalObject_ascent();
            //        if (o == null) sys.error("Runtime/get_parameters");
            //        return new object[1] { o };
            //    }
            //}
            sys.error("Runtime/get_funcname", v);
            return null;
        }

        public static string del_dq(string s)
        {
            return s.Replace("\"", "");
        }

        public static int get_type(object[] o)
        {
            return YDEF.get_type(o);
        }
    }
}
