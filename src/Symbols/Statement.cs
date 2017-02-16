using SuperBasic.FrontEnd.Errors;
using SuperBasic.FrontEnd.Symbols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperBasic.FrontEnd.Inter
{

    public class Stmt : Node
    {
        public int after { get; protected set; }
        public int begin { get; protected set; }
        public static Stmt Null = new Stmt(0);
        public static Stmt Encloseing = Null;
        public static Stmt LastFunction = Null;
        public Stmt(int line) : base(line)
        {
        }

        public virtual void gen(int begin, int after)
        {

        }

        public virtual void genCPP()
        {

        }

    }

    public class If : Stmt
    {
        public Expr Expr { get; protected set; }
        public Stmt Stmt { get; protected set; }
        public If(Expr x, Stmt s) : base(x.lexline)
        {
            Expr = x;
            Stmt = s;

            if (!ReserveType.IsBoolean(Expr.Type))
            {
                throw new TypeMisMatchException(s.lexline, Expr.Type, "Boolean");
            }
        }

        public override void gen(int begin, int after)
        {
            int label = newlabel();
            Expr.jumping(0, after);
            emitLabel(label);
            Stmt.gen(label, after);
        }

    }

    public class Else : Stmt
    {
        public Expr Expr { get; protected set; }
        public Stmt Stmt1 { get; protected set; }
        public Stmt Stmt2 { get; protected set; }

        public Else(Expr x, Stmt s1, Stmt s2) : base(x.lexline)
        {

            Expr = x;
            Stmt1 = s1;
            Stmt2 = s2;
            if (!ReserveType.IsBoolean(Expr.Type))
            {
                throw new TypeMisMatchException(x.lexline, Expr.Type, "Boolean");
            }
        }

        public override void gen(int begin, int after)
        {
            int label1 = newlabel();
            int label2 = newlabel();

            Expr.jumping(0, label2);

            emitLabel(label1); Stmt1.gen(label1, after); emit("goto L" + after);
            emitLabel(label2); Stmt2.gen(label2, after);

        }
    }

    public class While : Stmt
    {
        public Expr Expr { get; protected set; }
        public Stmt Stmt { get; protected set; }

        public While(int line) : base(line)
        {

        }

        public void Init(Expr x, Stmt s)
        {
            Expr = x;
            Stmt = s;

            if (!ReserveType.IsBoolean(Expr.Type))
            {
                throw new TypeMisMatchException(this.lexline, Expr.Type, "Boolean");
            }
        }


        public override void gen(int begin, int after)
        {
            this.after = after;
            this.begin = begin;
            Expr.jumping(0, after);
            int label = newlabel();

            emitLabel(label); Stmt.gen(label, begin);
            emit("goto L" + begin);
        }


    }

    public class Do : Stmt
    {
        public Expr Expr { get; protected set; }
        public Stmt Stmt { get; protected set; }

        public Do(int line) : base(line)
        {

        }

        public void Init(Stmt s, Expr x)
        {
            Expr = x;
            Stmt = s;

            if (!ReserveType.IsBoolean(Expr.Type))
            {
                throw new TypeMisMatchException(this.lexline, Expr.Type, "Boolean");
            }
        }


        public override void gen(int begin, int after)
        {
            this.after = after;
            this.begin = begin;
            int label = newlabel();

            Stmt.gen(label, begin);
            emitLabel(label);
            Expr.jumping(begin, 0);
        }
    }

    public class Call : Stmt
    {
        public FuncCall Caller { get; protected set; }
        public Call(FuncCall caller) : base(caller.lexline)
        {
            Caller = caller;
        }

        public override void gen(int begin, int after)
        {
            if(Caller.FunParams.Count() > 0)
            {
                foreach (var item in Caller.FunParams)
                {
                    if (item.Param is FuncCall)
                    {
                        Temp t = new Temp(Caller.Func.Params.ReturnType);
                        emitfunSet(item.Param, t);
                        emit("param " + t);
                    }
                    else
                    {
                        emit("param " + item);
                    }
                }
            }
            emitLabel(newlabel());
            emit("call " + Caller.Func);
        }
    }

    // left = right
    public class Set : Stmt
    {
        public Id Id { get; protected set; }
        public Expr Expr { get; protected set; }
        private Temp t = null;

        public Set(Id i, Expr x) : base(x.lexline)
        {
            Id = i;
            Expr = x;

            if(!ReserveType.ConvertableT1T2(i.Type, x.Type))
            {
                throw new TypeCovertException(this.lexline, Id.Type, Expr.Type);
            }
        }

        public Set(Temp i, Expr x) : base(x.lexline)
        {
            t = i;
            Expr = x;

            if (!ReserveType.ConvertableT1T2(i.Type, x.Type))
            {
                throw new TypeCovertException(this.lexline, Id.Type, Expr.Type);
            }
        }

        public override void gen(int begin, int after)
        {
            if (Expr is FuncCall)
            {
                if (t == null) emitfunSet(Expr, Id);
                else emitfunSet(Expr, t);
            }
            else
            {
                if (t == null) emit(Id + " = " + Expr.gen());
                else emit(t + " = " + Expr.gen());
            }
        }
    }

    // left[] = right
    public class SetElem : Stmt
    {
        public Id Array { get; protected set; }
        public Expr Index { get; protected set; }
        public Expr Expr { get; protected set; }

        public SetElem(Access x, Expr y) : base(x.lexline)
        {
            Array = x.array;
            Index = x.index;
            Expr = y;

            if (!ReserveType.ConvertableT1T2(y.Type, x.Type)) throw new TypeCovertException(this.lexline, y.Type, x.Type);
        }

        public override void gen(int begin, int after)
        {
            string s1 = Index.reduce().ToString();
            string s2 = Expr.reduce().ToString();

            emit(Array + " [" + s1 + "] = " + s2);
        }


    }

    public class Seq : Stmt
    {
        public Stmt First { get; protected set; }
        public Stmt Second { get; protected set; }

        public Seq(Stmt s1, Stmt s2) : base(s2.lexline)
        {
            First = s1;
            Second = s2;
        }

        public override void gen(int begin, int after)
        {
            if (First == Null) Second.gen(begin, after);
            else if (Second == Null) First.gen(begin, after);
            else
            {
                int label = newlabel();
                First.gen(begin, label);
                emitLabel(label);
                Second.gen(label, after);
            }
        }
    }

    public class Break : Stmt
    {
        public Stmt Stmt { get; protected set; }

        public Break(int line) : base(line)
        {
            if(Encloseing == Null)
            {
                throw new SourceBlocksNotCloseException(line);
            }
            Stmt = Encloseing;

        }

        public override void gen(int begin, int after)
        {
            emit("goto L" + Stmt.after);
        }
    }

    public class Continue :Stmt
    {
        public Stmt Stmt { get; protected set; }

        public Continue(int line) : base(line)
        {
            if (Encloseing == Null)
            {
                throw new SourceBlocksNotCloseException(line);
            }
            Stmt = Encloseing;

        }

        public override void gen(int begin, int after)
        {
            emit("goto L" + Stmt.begin);
        }
    }

    public class Return : Stmt
    {
        public Expr ReturnVal { get; protected set; }
        public Function Parent { get; protected set; }
        public Return(int line) : base(line)
        {
            Parent = LastFunction as Function;
        }

        public Return(int line, Function par, Expr ret) : base(line)
        {
            Parent = par;
            ReturnVal = ret;
            Parent = LastFunction as Function;
        }


        public override void gen(int begin, int after)
        {
            
            if (Parent.Params.ReturnType != null && Parent.Params.ReturnType != ReserveType.Void)
            {
                if (!ReserveType.ConvertableT1T2(Parent.Params?.ReturnType, ReturnVal.Type)) throw new TypeMisMatchException(this.lexline, ReturnVal.Type, Parent.Params.ReturnType.ToString());
                Temp t = new Temp(Parent.Params.ReturnType);
                new Set(t, ReturnVal).gen(begin, after);
                emit("rtn " + t);

            }
            else
            {
                emit("rtn");
            }
            //emitLabel(Parent.ExitLabel);

        }
    }


    public class OptParam : Param
    {
        public Constant DefaultValue { get; protected set; }
        public OptParam(Word name, Symbols.Type type, Constant defVal) : base(name, type)
        {
            DefaultValue = defVal;
        }
    }

    public class Param : Stmt
    {
        public Word ParamName { get; protected set; }
        public Symbols.Type ParamType { get; protected set; }
        public bool IsRequireParam { get; protected set; }
        public Param(Word name, Symbols.Type type) : base(name.Line)
        {
            ParamName = name;
            ParamType = type;
        }
    }

    public class Params : Stmt
    {
        public IEnumerable<Param> ParamList { get; protected set; }
        public Symbols.Type ReturnType { get; protected set; }
        public Params(int line, IEnumerable<Param> args, Symbols.Type retType) : base(line)
        {
            ParamList = args;
            ReturnType = retType;
        }

        public override void gen(int begin, int after)
        {
            foreach (var item in ParamList)
            {
                item.gen(begin, after);
            }
        }
    }

    public class Function : Stmt
    {
        public Params Params { get; protected set; }
        public Stmt Stmt { get; protected set; }
        public Id Id { get; protected set; }
        public int ExitLabel { get; protected set; }
        public Function(int line) : base(line)
        {

        }

        public void Init(Params param, Stmt fun, Id representId)
        {
            Params = param;
            Stmt = fun;
            Id = representId;
        }

        public override void gen(int begin, int after)
        {
            emit("goto L" + after);
            emitLabel("_func_" + Id.ToString());
            foreach (var item in Params.ParamList)
            {
                if(item is OptParam)
                {
                    emit(item.ParamName + " = " + (item as OptParam).DefaultValue + " optparam " + item.ParamType);
                }
                else
                {
                    emit(item.ParamName + " = param " + item.ParamType);
                }
            }
            int label = newlabel();
            int label2 = newlabel();
            ExitLabel = label2;
            emitLabel(label);
            Stmt.gen(label, label2);
            emitLabel(ExitLabel);
            emit("rtn");
            emitLabel("_func_end_" + Id.ToString());
        }

        public override string ToString()
        {
            return Id.ToString();
        }
    }





}
