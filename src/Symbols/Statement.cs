using SuperBasic.FrontEnd.Errors;
using SuperBasic.FrontEnd.Symbols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SuperBasic.FrontEnd.Inter.IntermediateCode;

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

    }

    public class Scope : Stmt
    {
        public SymbolTable Symbols { get; protected set; }
        public Scope(int line, SymbolTable curTable) : base(line)
        {
            Symbols = curTable;
        }

        public override void gen(int begin, int after)
        {
            CurrentGenerator.EnterScope(Symbols);
        }
    }

    public class EndScope : Scope
    {
        public EndScope(int line, SymbolTable curTable) : base(line, curTable)
        {

        }

        public override void gen(int begin, int after)
        {
            CurrentGenerator.EscapeScope(Symbols);
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
            int label = CurrentGenerator.AllocLabel();
            Expr.jumping(0, after);
            CurrentGenerator.Label(label);
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
            int label1 = CurrentGenerator.AllocLabel();
            int label2 = CurrentGenerator.AllocLabel();

            Expr.jumping(0, label2);

            CurrentGenerator.Label(label1); Stmt1.gen(label1, after); CurrentGenerator.Goto(after);
            CurrentGenerator.Label(label2); Stmt2.gen(label2, after);

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
            int label = CurrentGenerator.AllocLabel();

            CurrentGenerator.Label(label); Stmt.gen(label, begin);
            CurrentGenerator.Goto(begin);
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
            int label = CurrentGenerator.AllocLabel();

            Stmt.gen(label, begin);
            CurrentGenerator.Label(label);
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
                        CurrentGenerator.Temp(t);
                        emitfunSet(item.Param, t);
                        CurrentGenerator.Param(t);
                    }
                    else
                    {
                        CurrentGenerator.Param(item);
                    }
                }
            }
            CurrentGenerator.Label(CurrentGenerator.AllocLabel());
            CurrentGenerator.Call(Caller);
            CurrentGenerator.CallEnd();
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
            Id.setInit();
            if (!ReserveType.ConvertableT1T2(i.Type, x.Type))
            {
                throw new TypeCovertException(lexline, Id.Type, Expr.Type);
            }
        }

        public Set(Temp i, Expr x) : base(x.lexline)
        {
            t = i;
            Expr = x;

            if (!ReserveType.ConvertableT1T2(i.Type, x.Type))
            {
                throw new TypeCovertException(lexline, Id.Type, Expr.Type);
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
                if (t == null) CurrentGenerator.Set(Id, Expr.gen());
                else CurrentGenerator.Set(t, Expr.gen());
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
            Expr s1 = Index.reduce();
            Expr s2 = Expr.reduce();

            CurrentGenerator.SetArr(Array, s1, s2);
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
                int label = CurrentGenerator.AllocLabel();
                First.gen(begin, label);
                CurrentGenerator.Label(label);
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
            CurrentGenerator.Goto(Stmt.after);
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
            CurrentGenerator.Goto(Stmt.begin);
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
            
            if (Parent.Params.ReturnType != ReserveType.Void)
            {
                if (!ReserveType.ConvertableT1T2(Parent.Params.ReturnType, ReturnVal.Type)) throw new TypeMisMatchException(this.lexline, ReturnVal.Type, Parent.Params.ReturnType.ToString());
                Temp t = new Temp(Parent.Params.ReturnType);
                CurrentGenerator.Temp(t);
                new Set(t, ReturnVal).gen(begin, after);
                CurrentGenerator.Return(t);

            }
            else
            {
                CurrentGenerator.Return(null);
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
        public override string ToString()
        {
            return ParamName.ToString();
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
            if (ReturnType == null) ReturnType = ReserveType.Void;
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
        public bool ScopeFlag = false;

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
            CurrentGenerator.Goto(after);
            foreach (var item in Params.ParamList)
            {
                if(item is OptParam)
                {
                    CurrentGenerator.OptParam(item as OptParam, (item as OptParam).DefaultValue, item.ParamType);
                }
                else
                {
                    CurrentGenerator.Param(item, item.ParamType);
                }
            }
            
            int label = CurrentGenerator.AllocLabel();
            int label2 = CurrentGenerator.AllocLabel();
            ExitLabel = label2;
            CurrentGenerator.Label(label);
            CurrentGenerator.LabelFunc(this);
            Stmt.gen(begin, after);
            CurrentGenerator.LabelFuncEnd(this);
            CurrentGenerator.Label(ExitLabel);
            //CurrentGenerator.Return(null);
            
        }

        public override string ToString()
        {
            return Id.ToString();
        }
    }





}
