using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lextool.runtime
{
    public class MainProcessFunction
    {
        public static object ExecuteSentence(List<VALUE> l)
        {
            if (l==null || l.Count!=1) return null;

            var v = l[0];

            VALUE find = null;

            //find = v.FindValueByTravarse(FuncUtil.get_type(YDEF.sx_screen_sentence));
            //if (find != null)
            //{
            //    sys.logline("Exec Screen Sentence");
            //    return null;
            //}
            //find = v.FindValueByTravarse(FuncUtil.get_type(YDEF.sx_layer_sentence));
            //if (find != null)
            //{
            //    sys.logline("Exec Layer Sentence");
            //    return null;
            //}
            //find = v.FindValueByTravarse(FuncUtil.get_type(YDEF.sx_display_sentence));
            //if (find != null)
            //{
            //    sys.logline("Exec Display Sentence");
            //    return null;
            //}

            return null;
        }
    }
}
