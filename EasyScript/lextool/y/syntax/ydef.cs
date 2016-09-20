using System;
using System.Collections.Generic;
using System.Reflection;
namespace lextool
{
    /*
     *   Lex & Yacc の .y定義に当たる部分。 
     *　
     *　 .yに似せた構造となっている。
     *　 
     *　 文法は、ここだけを変更すればよい。
     * 
     */

    //終末トークン
    public enum TOKEN
    {
        UNKNOWN = 0,
        NUM     ,//= 1,
        NAME    ,//= 2,
        QSTR    ,//= 3,
        OTR     ,//= 4,
        SP      ,//= 5,
        EOL     ,//= 6,
        EOF     ,//= 7,
        BOF     ,//= 8,

        FUNCTION,//=9 
        VAR,     //=10
        IF,      //=11
        ELSE,    //=12
        FOR,     //=13
        WHILE,   //=14
        SWITCH,  //=15
        CASE,    //=16
        DEFAULT, //=17
        BREAK,   //=18
        CONTINUE,//=19
        RETURN,  //=20

        OP      ,// 曖昧
        OP3     ,// wikipedia:演算子の優先順位=3
        OP4     ,// wikipedia:演算子の優先順位=4
        OP6     ,// wikipedia:演算子の優先順位=6
        OP7     ,// wikipedia:演算子の優先順位=7
        OP11    ,// wikipedia:演算子の優先順位=11
        OP12    ,// wikipedia:演算子の優先順位=12
        OP14    ,// wikipedia:演算子の優先順位=14
 
        REST    ,//
        CMT     ,//

        ERROR   = -1,

        MAX     = 255
    }


    public partial class YDEF
    {
        //未定
        public static int UNKNOWN = (int)TOKEN.UNKNOWN;   //取り扱いを簡単にするために再定義

        //基本
        public static int NUM  = (int)TOKEN.NUM;          //   　　　　　〃
        public static int NAME = (int)TOKEN.NAME;         //   　　　　　〃
        public static int QSTR = (int)TOKEN.QSTR;         //   　　　　　〃
        public static int OTR  = (int)TOKEN.OTR;          //   　　　　　〃
        public static int SP   = (int)TOKEN.SP;           //   　　　　　〃
        public static int EOL  = (int)TOKEN.EOL;          //   　　　　　〃
        public static int EOF  = (int)TOKEN.EOF;          //   　　　　　〃
        public static int BOF  = (int)TOKEN.BOF;          //   　　　　　〃

        //予約語
        public static int FUNCTION = (int)TOKEN.FUNCTION;
        public static int VAR      = (int)TOKEN.VAR;     
        public static int IF       = (int)TOKEN.IF;     
        public static int ELSE     = (int)TOKEN.ELSE;
        public static int FOR      = (int)TOKEN.FOR;    
        public static int WHILE    = (int)TOKEN.WHILE;
        public static int SWITCH   = (int)TOKEN.SWITCH;
        public static int CASE     = (int)TOKEN.CASE;  
        public static int DEFAULT  = (int)TOKEN.DEFAULT;
        public static int BREAK    = (int)TOKEN.BREAK;
        public static int CONTINUE = (int)TOKEN.CONTINUE;
        public static int RETURN   = (int)TOKEN.RETURN;

        //オペレータ                                      //   　　　　　〃
        public static int OP    = (int)TOKEN.OP;          //   　　　　　〃
        public static int OP3   = (int)TOKEN.OP3;         //
        public static int OP4   = (int)TOKEN.OP4;         //
        public static int OP6   = (int)TOKEN.OP6;         //
        public static int OP7   = (int)TOKEN.OP7;         //
        public static int OP11  = (int)TOKEN.OP11;        //
        public static int OP12  = (int)TOKEN.OP12;        //
        public static int OP14  = (int)TOKEN.OP14;        //

        //特殊                                            //   　　　　　〃
        public static int REST  = (int)TOKEN.REST;        //   　　　　　〃
        public static int CMT   = (int)TOKEN.CMT;         //   　　　　　〃
        public static int ERROR = (int)TOKEN.ERROR;       //   　　　　　〃

        //構文分析用
        public static int __OR__   = 300;
        public static int __MAKE__ = 301;
 
        //文字列
        public static string CMTSTR = "//";
        public static string DQ     = "\"";

        //構文ツリー     .yの表現をコードで実装。　第１，２要素はルール名とトークンタイプ。 __OR__は選択支。__MAKE__は処理用のファンクション
        //               
        //     トークンタイプの大きい方が結びつきが強い。
        //     __MAKE__の後は 処理ファンクション、引数の番号(Base 0)
        //

        public static object[] sx_main_block      =  { "sx_main_block",
                                                       1001,
                                                            BOF,"sx_sentence_list",EOF,                       __MAKE__, YCODE.DO_NEW, 0,1,2,
                                                            __OR__,
                                                            BOF,"sx_sentence",EOF,                            __MAKE__, YCODE.DO_NEW, 0,1,2                            
                                                     };
        public static object[] sx_sentence_block =   {"sx_sentence_block",
                                                       1002,
                                                            "{","sx_sentence","}",                            __MAKE__, YCODE.DO_NEW, 0,1,2,
                                                            __OR__,
                                                            "{","sx_sentence_list","}",                       __MAKE__, YCODE.DO_NEW, 0,1,2,
                                                            __OR__,
                                                            "{","}",                                          __MAKE__, YCODE.DO_NEW, 0,1                            
                                                     };
        public static object[] sx_sentence_list   =  { "sx_sentence_list",
                                                       1003,
                                                            "sx_sentence","sx_sentence",                      __MAKE__, YCODE.DO_NEW, 0,1,
                                                            __OR__,
                                                            "sx_sentence_list","sx_sentence",                 __MAKE__, YCODE.DO_ADD, 0,1,
                                                            __OR__,
                                                            "sx_sentence","sx_sentence_list",                 __MAKE__, YCODE.DO_ADDHEAD, 1,0,
                                                            __OR__,
                                                            "sx_sentence_list","sx_sentence_list",            __MAKE__, YCODE.DO_COMBINE, 0,1,
                                                     };
        public static object[] sx_for_blanket     =  {  "sx_for_blanket",
                                                        1008,
                                                            "(","sx_def_var_clause","sx_expr_clause",NAME,"=","sx_expr",")", __MAKE__, YCODE.DO_NEW,0,1,2,3,4,5,6,
                                                            __OR__,
                                                            "(","sx_expr_clause","sx_expr_clause","sx_expr",")",    __MAKE__, YCODE.DO_NEW,0,1,2,3,4,
                                                     };
        public static object[] sx_sentence        =  { "sx_sentence",
                                                       1007,
                                                            "sx_expr_clause",                              __MAKE__, YCODE.DO_NEW, 0,
                                                            __OR__,
                                                            "sx_def_var_clause",                           __MAKE__, YCODE.DO_NEW, 0,
                                                            __OR__,
                                                            "sx_def_func_clause",                          __MAKE__, YCODE.DO_NEW, 0,
                                                            __OR__,
                                                            "sx_if_clause",                                __MAKE__, YCODE.DO_NEW, 0,
                                                            __OR__,
                                                            "sx_for_clause",                               __MAKE__, YCODE.DO_NEW, 0,
                                                            __OR__,
                                                            "sx_while_clause",                             __MAKE__, YCODE.DO_NEW, 0,
                                                            __OR__,
                                                            "sx_switch_clause",                            __MAKE__, YCODE.DO_NEW, 0,
                                                            __OR__,
                                                            "sx_case_clause",                              __MAKE__, YCODE.DO_NEW, 0,
                                                            __OR__,
                                                            "sx_default_clause",                           __MAKE__, YCODE.DO_NEW, 0,
                                                            __OR__,
                                                            "sx_break_clause",                             __MAKE__, YCODE.DO_NEW, 0,
                                                            __OR__,
                                                            "sx_continue_clause",                          __MAKE__, YCODE.DO_NEW, 0,
                                                            __OR__,
                                                            "sx_return_clause",                            __MAKE__, YCODE.DO_NEW, 0,
                                                            __OR__,
                                                            "sx_sentence_block",                           __MAKE__, YCODE.DO_NEW, 0,
                                                      };

        public static object[] sx_expr_clause      =  { "sx_expr_clause",
                                                        1009,
                                                             NAME,"=", "sx_expr", ";",                     __MAKE__, YCODE.DO_NEW, 0,1,2,3,
                                                             __OR__,
                                                             "sx_expr",";",                                __MAKE__, YCODE.DO_NEW, 0,1
                                                      };

        public static object[] sx_expr             =  { "sx_expr",
                                                        1052,
                                                             NAME,                                          __MAKE__, YCODE.DO_NEW, 0,
                                                             __OR__,
                                                             QSTR,                                          __MAKE__, YCODE.DO_NEW, 0,
                                                             __OR__,
                                                             NUM,                                           __MAKE__, YCODE.DO_NEW, 0,
                                                             __OR__,
                                                             "sx_expr",OP3,"sx_expr",                       __MAKE__, YCODE.DO_NEW, 0, 1, 2,
                                                             __OR__,
                                                             "sx_expr",OP4,"sx_expr",                       __MAKE__, YCODE.DO_NEW, 0, 1, 2,
                                                             __OR__,
                                                             "sx_expr",OP6,"sx_expr",                       __MAKE__, YCODE.DO_NEW, 0, 1, 2,
                                                             __OR__,
                                                             "sx_expr",OP7,"sx_expr",                       __MAKE__, YCODE.DO_NEW, 0, 1, 2,
                                                             __OR__,
                                                             "sx_expr",OP11,"sx_expr",                      __MAKE__, YCODE.DO_NEW, 0, 1, 2,
                                                             __OR__,
                                                             "sx_expr",OP12,"sx_expr",                      __MAKE__, YCODE.DO_NEW, 0, 1, 2,
                                                             __OR__,
                                                             "sx_expr",OP14,"sx_expr",                      __MAKE__, YCODE.DO_NEW, 0, 1, 2,
                                                             __OR__,
                                                             "sx_expr",OP,"sx_expr",                        __MAKE__, YCODE.DO_NEW, 0, 1, 2,
                                                             __OR__,
                                                             "-", "sx_expr",                                __MAKE__, YCODE.DO_NEW, 0, 1,
                                                             __OR__,
                                                             "+", "sx_expr",                                __MAKE__, YCODE.DO_NEW, 0, 1,
                                                             __OR__,
                                                             "sx_expr_blanket",                             __MAKE__, YCODE.DO_NEW, 0,
                                                             __OR__,
                                                             "sx_func",                                     __MAKE__, YCODE.DO_NEW, 0
                                                      };

        public static object[] sx_expr_blanket     =  { "sx_expr_blanket",
                                                        1053,
                                                            "(", "sx_expr", ")",                           __MAKE__, YCODE.DO_NEW, 0,1,2,
                                                            __OR__,
                                                            "(", ")",                                      __MAKE__, YCODE.DO_NEW, 0,1
                                                      };
        
        public static object[] sx_func             =  { "sx_func",
                                                        1031,
                                                            NAME, "sx_expr_blanket",                       __MAKE__, YCODE.DO_NEW, 0,1,
                                                      };

        public static object[] sx_def_func_clause  =  { "sx_def_func_clause",
                                                       1011,
                                                           FUNCTION,"sx_func", "sx_sentence_block",        __MAKE__, YCODE.DO_NEW,0,1, 2
                                                      };

        public static object[] sx_def_var_clause  =   { "sx_def_var_clause",
                                                       1012,
                                                           VAR,NAME,";",                                   __MAKE__, YCODE.DO_NEW,0,1,2,
                                                           __OR__,
                                                           VAR,NAME,"=","sx_expr",";",                     __MAKE__, YCODE.DO_NEW,0,1,2,3,4,
                                                      };

        public static object[] sx_if_clause       =   { "sx_if_clause",
                                                       1021,
                                                           IF,"sx_expr_blanket","sx_sentence",                               __MAKE__, YCODE.DO_NEW,0,1,2,
                                                           __OR__,
                                                           "sx_if_clause","sx_else_clause",                                  __MAKE__, YCODE.DO_ADD,0,1,
                                                      };
        public static object[] sx_else_clause     =   { "sx_else_clause",
                                                       1022,
                                                           ELSE,"sx_sentence",                                               __MAKE__, YCODE.DO_NEW,0,1,
                                                      };
        public static object[] sx_for_clause      =   { "sx_for_clause",
                                                       1023,
                                                           FOR,"sx_for_blanket","sx_sentence",                               __MAKE__, YCODE.DO_NEW,0,1,2,
                                                      };
        public static object[] sx_while_clause    =   { "sx_while_clause",
                                                       1024,
                                                            WHILE,"sx_expr_blanket","sx_sentence",                           __MAKE__, YCODE.DO_NEW,0,1,2
                                                      };

        public static object[] sx_switch_clause   =   { "sx_switch_clause",
                                                       1025,
                                                           SWITCH,"sx_expr_blanket","sx_sentence_block",   __MAKE__,YCODE.DO_NEW, 0,1,2
                                                      };

        public static object[] sx_case_clause     =   { "sx_case_clause",
                                                       1026,
                                                           YDEF.CASE, "sx_expr", ":",                       __MAKE__,YCODE.DO_NEW, 0,1,2,
                                                      };
        public static object[] sx_default_clause  =   { "sx_default_clause",
                                                       1027,
                                                           YDEF.DEFAULT, ":",                              __MAKE__,YCODE.DO_NEW, 0,1,
                                                      };
        public static object[] sx_break_clause    =   { "sx_break_clause",
                                                       1028,
                                                           YDEF.BREAK, ";",                                __MAKE__,YCODE.DO_NEW, 0,1,
                                                      };
        public static object[] sx_continue_clause =   { "sx_continue_clause",
                                                       1029,
                                                           YDEF.CONTINUE, ";",                             __MAKE__,YCODE.DO_NEW, 0,1,
                                                      };
        public static object[] sx_return_clause =     { "sx_return_clause",
                                                       1030,
                                                           YDEF.RETURN, "sx_expr", ";",                    __MAKE__,YCODE.DO_NEW, 0,1,2,
                                                           __OR__,
                                                           YDEF.RETURN, ";",                               __MAKE__,YCODE.DO_NEW, 0,1,
                                                      };
    }
}