using SuperBasic.FrontEnd.Symbols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperBasic.FrontEnd.Inter
{
    public interface IGenerator
    {
        void Start(SymbolTable t);
        void End();
        void EnterScope(SymbolTable t);
        void EscapeScope(SymbolTable t);
        /// <summary>
        /// 设置标签
        /// </summary>
        /// <param name="tag">字符</param>
        void Label(string tag);
        void Label(int tag);
        /// <summary>
        /// 传递参数
        /// </summary>
        /// <param name="t">参数T</param>
        void Param(Expr t);
        /// <summary>
        /// 获得参数
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        void Param(Param id, Symbols.Type t);
        /// <summary>
        /// 语句跳转
        /// </summary>
        /// <param name="tag">指定TAG</param>
        void Goto(string tag);
        void Goto(int tag);
        /// <summary>
        /// 函数调用
        /// </summary>
        /// <param name="caller">调用体</param>
        void Call(FuncCall caller);
        void Call(Id ret, Function caller);
        void Call(Expr ret, Function caller);
        void CallEnd();
        /// <summary>
        /// 赋值
        /// </summary>
        /// <param name="t">临时变量</param>
        /// <param name="exp2">表达式</param>
        void Set(Temp t, Expr exp2);
        /// <summary>
        /// 赋值
        /// </summary>
        /// <param name="exp1">变量</param>
        /// <param name="exp2">表达式</param>
        void Set(Id exp1, Expr exp2);
        void Temp(Temp t);
        /// <summary>
        /// 数组赋值
        /// </summary>
        /// <param name="arr">数组</param>
        /// <param name="index">序号</param>
        /// <param name="value">值</param>
        void SetArr(Id arr, Expr index, Expr value);
        ///// <summary>
        ///// 中断语句执行
        ///// </summary>
        ///// <param name="s">目标语句</param>
        //void Break(Stmt s, int begin, int after);
        ///// <summary>
        ///// 中断语句当前执行
        ///// </summary>
        ///// <param name="s">目标语句</param>
        //void Continue(Stmt s, int begin, int after);
        /// <summary>
        /// 函数返回返回值
        /// </summary>
        /// <param name="t">值</param>
        void Return(Expr t);
        /// <summary>
        /// 标记函数开始
        /// </summary>
        /// <param name="func">函数</param>
        void LabelFunc(Function func);
        /// <summary>
        /// 标记函数结束
        /// </summary>
        /// <param name="func">函数</param>
        void LabelFuncEnd(Function func);
        /// <summary>
        /// if判断
        /// </summary>
        /// <param name="exp1">表达式1</param>
        /// <param name="op">判断符</param>
        /// <param name="exp2">表达式2</param>
        void If(Expr exp, int jump);
        /// <summary>
        /// ifFalse判断
        /// </summary>
        /// <param name="exp1">表达式1</param>
        /// <param name="op">判断符</param>
        /// <param name="exp2">表达式2</param>
        void IfFalse(Expr exp1, int jump);
        /// <summary>
        /// 创建一个匿名Label以供跳转
        /// </summary>
        /// <returns>Label</returns>
        int AllocLabel();
        void OptParam(OptParam item, Constant defaultValue, Symbols.Type paramType);
    }

    public class DefaultGenerator : IGenerator
    {
        private int alabel = 0;
        public DefaultGenerator()
        {

        }

        public void CallEnd()
        {
            IntermediateCode.CurrentIO.EmitLine("retcall");
        }

        public void Label(string tag)
        {
            IntermediateCode.CurrentIO.EmitLabel(tag);
        }

        public int AllocLabel()
        {
            return (alabel++);
        }

        public void Label(int tag)
        {
            Label("L" + tag);
        }

        public void Param(Expr t)
        {
            IntermediateCode.CurrentIO.EmitLine("param " + t);
        }

        public void Param(Param id, Symbols.Type t)
        {
            IntermediateCode.CurrentIO.EmitLine(id + " = param " + t);
        }

        public void Goto(string tag)
        {
            IntermediateCode.CurrentIO.EmitLine("goto " + tag);
        }

        public void Goto(int tag)
        {
            Goto("L" + tag);
        }

        public void Call(FuncCall caller)
        {
            IntermediateCode.CurrentIO.EmitLine(caller.ToString());
        }

        public void Call(Id ret, Function caller)
        {
            IntermediateCode.CurrentIO.EmitLine(ret + " = " + caller);
        }

        public void Call(Expr ret, Function caller)
        {
            IntermediateCode.CurrentIO.EmitLine(ret + " = call " + caller);
        }

        public void Set(Temp t, Expr exp)
        {
            IntermediateCode.CurrentIO.EmitLine(t + " = " + exp);
        }

        public void Set(Id t, Expr exp)
        {
            IntermediateCode.CurrentIO.EmitLine(t + " = " + exp);
        }

        public void SetArr(Id arr, Expr index, Expr value)
        {
            IntermediateCode.CurrentIO.EmitLine(arr + " [ " + index + " ] = " + value);
        }

        public void Return(Expr t)
        {
            IntermediateCode.CurrentIO.EmitLine("ret " + t);
        }

        public void LabelFunc(Function func)
        {
            IntermediateCode.CurrentIO.EmitLabel("_func_" + func);
        }

        public void LabelFuncEnd(Function func)
        {
            IntermediateCode.CurrentIO.EmitLabel("_end_func_" + func);
        }

        public void If(Expr exp, int jump)
        {
            IntermediateCode.CurrentIO.EmitLine("if " + exp + " goto " + jump);
        }

        public void IfFalse(Expr exp, int jump)
        {
            IntermediateCode.CurrentIO.EmitLine("iffalse " + exp + " goto " + jump);
        }

        public void OptParam(OptParam item, Constant defaultValue, Symbols.Type paramType)
        {
            IntermediateCode.CurrentIO.EmitLine(item + " = " + defaultValue + " param " + paramType);
        }

        public void EnterScope(SymbolTable t)
        {
            IntermediateCode.CurrentIO.EmitLabel(t.ToString());
            foreach (var item in t.Members)
            {
                IntermediateCode.CurrentIO.EmitLine("db " + item.Type.Width + " " + item.Identity);
            }
        }

        public void EscapeScope(SymbolTable t)
        {

        }

        public void Temp(Temp t)
        {
            
        }

        public void Start(SymbolTable t)
        {

        }

        public void End()
        {

        }
    }

    public class CppGenerator : IGenerator
    {

        private int alabel = 0;
        private string peekParam = string.Empty;
        private string peekPassParam = string.Empty;
        private bool infunc = false;
        private string entry = string.Empty;

        public CppGenerator(string arg)
        {
            entry = arg;
            IntermediateCode.CurrentIO.EmitLine(@"#include ""superbasic_cpp.h""");
            IntermediateCode.CurrentIO.EmitLine(@"#include <iostream>");
        }

        public int AllocLabel()
        {
            return (alabel++);
        }

        public void Call(FuncCall caller)
        {
            IntermediateCode.CurrentIO.Emit("\t" + caller.Func + "(");
        }

        public void Call(Id ret, Function caller)
        {
            IntermediateCode.CurrentIO.Emit("\t" + ret + " = " + caller + "(");
        }

        public void Call(Expr ret, Function caller)
        {
            IntermediateCode.CurrentIO.Emit("\t" + ret + " = " + caller + "(");
        }

        public void CallEnd()
        {
            IntermediateCode.CurrentIO.Emit((peekPassParam.Length > 0 ? peekPassParam.Substring(0, peekPassParam.Length - 1) : "") + ");\n");
            peekPassParam = string.Empty;
        }

        public void EnterScope(SymbolTable t)
        {
            IntermediateCode.CurrentIO.EmitBlock("{");
            foreach (var item in t.Members)
            {
                if(item.Type.TokenTag == Tag.ARRAY)
                {
                    IntermediateCode.CurrentIO.EmitLine((item.Type as Symbols.Array).Type + " " + item.Identity + "[" + (item.Type as Symbols.Array).Width + "]");
                }
                else
                {
                    IntermediateCode.CurrentIO.EmitLine(item.Type + " " + item.Identity);
                }
                
            }
        }

        public void EscapeScope(SymbolTable t)
        {
            IntermediateCode.CurrentIO.EmitBlock("}");
        }

        public void Goto(int tag)
        {
            if (infunc)
                Goto("L" + tag);
        }

        public void Goto(string tag)
        {
            if (infunc)
                IntermediateCode.CurrentIO.EmitLine("goto " + tag);
        }

        public void If(Expr exp, int jump)
        {
            IntermediateCode.CurrentIO.EmitLine("if(" + exp + ") goto L" + jump);
        }

        public void IfFalse(Expr exp, int jump)
        {
            IntermediateCode.CurrentIO.EmitLine("if(!(" + exp + ")) goto L" + jump);
        }

        public void Label(int tag)
        {
            if(infunc)
                Label("L" + tag);
        }

        public void Label(string tag)
        {
            if (infunc)
                IntermediateCode.CurrentIO.EmitLabel(tag);
        }

        public void LabelFunc(Function func)
        {
            IntermediateCode.CurrentIO.EmitBlock(func.Params.ReturnType + " " + func.Id.Scope + "::" + func.Id + "(" + (peekParam.Length > 0 ? peekParam.Substring(0, peekParam.Length - 1) : "") + ") ");
            peekParam = string.Empty;
            infunc = true;
        }

        public void LabelFuncEnd(Function func)
        {
            //IntermediateCode.CurrentIO.EmitBlock("}");
            infunc = false;
        }

        public void OptParam(OptParam item, Constant defaultValue, Symbols.Type paramType)
        {
            peekParam += paramType + " " + item + " = " + defaultValue + ",";
        }

        public void Param(Expr t)
        {
            peekPassParam += t + ",";
        }

        public void Param(Param id, Symbols.Type t)
        {
            peekParam += t + " " + id + ",";
        }

        public void Return(Expr t)
        {
            IntermediateCode.CurrentIO.EmitLine("return" + (t == null ? "" : " " + t));
        }

        public void Set(Id exp1, Expr exp2)
        {
            IntermediateCode.CurrentIO.EmitLine(exp1 + " = " + exp2);
        }

        public void Set(Temp t, Expr exp2)
        {
            IntermediateCode.CurrentIO.EmitLine(t + " = " + exp2);
        }

        public void SetArr(Id arr, Expr index, Expr value)
        {
            IntermediateCode.CurrentIO.EmitLine(arr + " [ " + index + " ] = " + value);
        }

        public void Temp(Temp t)
        {
            IntermediateCode.CurrentIO.EmitLine(t.Type + " " + t);
        }

        public void Start(SymbolTable t)
        {
            IntermediateCode.CurrentIO.EmitBlock("namespace " + t.Name + " {");
            foreach (var item in t.Members)
            {
                if (item is IdFunc)
                {
                    var k = item as IdFunc;
                    IntermediateCode.CurrentIO.Emit("\t" + k.Func.Params.ReturnType + " " + k.Func.Id + "(");
                    string param = string.Empty;
                    foreach (var i in k.Func.Params.ParamList)
                    {
                        param += (i.ParamType + " " + i.ParamName) + ",";
                    }
                    if (param.EndsWith(",")) param = param.Substring(0, param.Length - 1);
                    IntermediateCode.CurrentIO.Emit(param + ");\n");
                }
                else
                {
                    IntermediateCode.CurrentIO.Emit(item.Type + " " + item.Identity);
                }
            }
            IntermediateCode.CurrentIO.EmitBlock("}");
        }

        public void End()
        {
            IntermediateCode.CurrentIO.EmitBlock("\nint main(){ std::cout << " + entry + "() << std::endl; system(\"pause\"); return 0;}");
        }
    }
}
