using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace lextool
{
    public class process
    {
        public static void Run(string file)
        {
            var src = File.ReadAllText(file);

            var engine = new yengine();

            // 終末記号に分類
            var lex_output = engine.Lex(src);

            //スペース・コメント削除。"文字列"以外大文字化。
            engine.Normalize(ref lex_output);                             sys.logline("\n*lex_output");           YDEF_DEBUG.DumpList(lex_output, true);

            //１行化
            var one_line = engine.Make_one_line(lex_output);

            //実行用リスト作成(解析)
            var executable_value_list = engine.Interpret(one_line);       sys.logline("\n*executable_value_list"); YDEF_DEBUG.DumpList(executable_value_list, true);

            //ダンプ
            sys.logline("\n[executable_value_list]\n");
            YDEF_DEBUG.PrintLiterally(executable_value_list[0]);
            sys.logline("\n");

            //リストの整合性テスト
            int errorline;
            if (YDEF_DEBUG.IsExecutable(executable_value_list[0],out errorline))
            {
                sys.logline("This script has been pass the first check.");
            }
            else
            {
                sys.error("Not executable. Check Line " + (errorline + 1));
            }

            //実行
            sys.logline("\n\n*Execute! \n");
            runtime.predefinedfunc.Init();
            runtime.processfunc.Run(executable_value_list[0][0]);
#if x

            //実行
            sys.logline("\n\n*Execute! \n");
            foreach (var l in executable_value_list)
            {
                runtime.MainProcessFunction.ExecuteSentence(l);
            }
#endif



            Console.WriteLine("\nend");
        }
    }
}
