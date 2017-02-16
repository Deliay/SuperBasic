using SuperBasic.FrontEnd.Symbols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperBasic.FrontEnd.Inter
{
    interface IGenerator
    {
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
        Expr Param(Symbols.Type t);
        /// <summary>
        /// 语句跳转
        /// </summary>
        /// <param name="tag">指定TAG</param>
        void Goto(string tag, int begin, int after);
        /// <summary>
        /// 函数调用
        /// </summary>
        /// <param name="caller">调用体</param>
        void Call(FuncCall caller);
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
        /// <summary>
        /// 数组赋值
        /// </summary>
        /// <param name="arr">数组</param>
        /// <param name="index">序号</param>
        /// <param name="value">值</param>
        void SetArr(Id arr, Expr index, Expr value);
        /// <summary>
        /// 中断语句执行
        /// </summary>
        /// <param name="s">目标语句</param>
        void Break(Stmt s, int begin, int after);
        /// <summary>
        /// 中断语句当前执行
        /// </summary>
        /// <param name="s">目标语句</param>
        void Continue(Stmt s, int begin, int after);
        /// <summary>
        /// 函数返回返回值
        /// </summary>
        /// <param name="t">值</param>
        void Return(Expr t, int begin, int after);
        /// <summary>
        /// 标记函数开始
        /// </summary>
        /// <param name="func">函数</param>
        void LabelFunc(Function func, int begin, int after);
        /// <summary>
        /// 标记函数结束
        /// </summary>
        /// <param name="func">函数</param>
        void LabelFuncEnd(Function func, int begin, int after);
        /// <summary>
        /// if判断
        /// </summary>
        /// <param name="exp1">表达式1</param>
        /// <param name="op">判断符</param>
        /// <param name="exp2">表达式2</param>
        void If(Expr exp1, Op op, Expr exp2, int begin, int after);
        /// <summary>
        /// ifFalse判断
        /// </summary>
        /// <param name="exp1">表达式1</param>
        /// <param name="op">判断符</param>
        /// <param name="exp2">表达式2</param>
        void IfFalse(Expr exp1, Op op, Expr exp2, int begin, int after);
    }

    public class DefaultGenerator : IGenerator
    {
        public DefaultGenerator()
        {

        }

        public void Break(Stmt s, int begin, int after)
        {
            throw new NotImplementedException();
        }

        public void Call(FuncCall caller)
        {
            throw new NotImplementedException();
        }

        public void Continue(Stmt s, int begin, int after)
        {
            throw new NotImplementedException();
        }

        public void Goto(string tag, int begin, int after)
        {
            throw new NotImplementedException();
        }

        public void If(Expr exp1, Op op, Expr exp2, int begin, int after)
        {
            throw new NotImplementedException();
        }

        public void IfFalse(Expr exp1, Op op, Expr exp2, int begin, int after)
        {
            throw new NotImplementedException();
        }

        public void LabelFunc(Function func, int begin, int after)
        {
            throw new NotImplementedException();
        }

        public void LabelFuncEnd(Function func, int begin, int after)
        {
            throw new NotImplementedException();
        }

        public Expr Param(Symbols.Type t)
        {
            throw new NotImplementedException();
        }

        public void Param(Expr t)
        {
            throw new NotImplementedException();
        }

        public void Return(Expr t, int begin, int after)
        {
            throw new NotImplementedException();
        }

        public void Set(Id exp1, Expr exp2)
        {
            throw new NotImplementedException();
        }

        public void Set(Temp t, Expr exp2)
        {
            throw new NotImplementedException();
        }

        public void SetArr(Id arr, Expr index, Expr value)
        {
            throw new NotImplementedException();
        }
    }
}
