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
    public class Node
    {
        public static int labels = 0;
        public int lexline { get; protected set; }

        public Node(int line)
        {
            lexline = line;
        }

        //public int newlabel()
        //{
        //    return CurrentGenerator.AllocLabel();
        //}

        //public void emitLabel(int i)
        //{
        //    CurrentGenerator.Label("L" + i.ToString());
            
        //}

        //public void emit(string s)
        //{
        //    CurrentIO.EmitLine(s);
        //}

        //public void emitSrc(string s)
        //{
        //    CurrentIO.EmitSource(s);
        //}

        public void emitfunSet(Expr exp, Expr toVar)
        {
            FuncCall c = exp as FuncCall;

            if (c.FunParams?.Count() > 0)
            {
                foreach (PassParam item in c.FunParams)
                {
                    if (item.Param is FuncCall)
                    {
                        Temp t = new Temp(toVar.Type);
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
            CurrentGenerator.Call(toVar, c.Func);
            CurrentGenerator.CallEnd();
        }

    }

    public class Expr : Node
    {
        public Token Op { get; protected set; }
        public Symbols.Type Type { get; protected set; } 

        public Expr(Token tok, Symbols.Type p) : base(tok.Line)
        {
            Op = tok;
            Type = p;
        }

        public virtual Expr gen()
        {
            return this;
        }

        public virtual Expr reduce()
        {
            return this;
        }

        public virtual void jumping(int t, int f)
        {
            emitjumps(this, t, f);
        }

        public virtual void emitjumps(Expr s, int truejump, int falsejump)
        {
            if (truejump != 0 && falsejump != 0)
            {
                CurrentGenerator.If(s, truejump);
                CurrentGenerator.Goto(falsejump.ToString());
            }
            else if (truejump != 0) CurrentGenerator.If(s, truejump);
            else if (falsejump != 0) CurrentGenerator.IfFalse(s, falsejump);
            
        }

        public override string ToString()
        {
            return Op.ToString();
        }
    }


    public class Id : Expr
    {
        public Id(Word id, SymbolTable t, Symbols.Type p, int b, bool s) : base(id, p)
        {
            Offset = b;
            Scope = t;
            IsStatic = s;
            Identity = id;
            IsInitial = false;
        }

        public Id(Param p, SymbolTable t, int b, bool s) : this(p.ParamName, t, p.ParamType, b, s)
        {
            Offset = b;
            Scope = t;
        }
        public int Offset { get; protected set; }
        public SymbolTable Scope { get; protected set; }
        public bool IsStatic { get; protected set; }
        public Word Identity { get; protected set; }
        public bool IsInitial { get; protected set; }
        public void setInit()
        {
            IsInitial = true;
        }
        public override string ToString()
        {
            return base.ToString();
        }
    }

    public class IdFunc : Id
    {
        public Function Func { get; protected set; }
        public IdFunc(Function func, Word id, SymbolTable t, Symbols.Type p, int b, bool s) : base(id, t, p, b, s)
        {
            Func = func;
        }
    }

    public class Op : Expr
    {
        public Op(Token tok, Symbols.Type p) : base(tok, p)
        {

        }

        public override Expr reduce()
        {
            Expr x = gen();
            Temp t = new Temp(Type);
            CurrentGenerator.Temp(t);
            CurrentGenerator.Set(t, x);
            return t;
        }
    }

    public class Temp : Expr
    {
        public static int count { get; protected set; }
        int number = 0;

        public Temp(Symbols.Type p) : base(Word.Temp, p)
        {
            number = ++count;
        }

        public override string ToString()
        {
            return "t" + number;
        }
    }

    public class Arith : Op
    {
        public Expr First { get; protected set; }
        public Expr Second { get; protected set; }
        public Arith(Token tok, Expr x1, Expr x2) : base(tok, null)
        {
            First = x1;
            Second = x2;
            Type = ReserveType.Max(First.Type, Second.Type);
            if (Type == null) throw new TypeCovertException(tok.Line, First.Type, Second.Type);
        }

        public override Expr gen()
        {
            return new Arith(Op, First.reduce(), Second.reduce());
        }

        public override string ToString()
        {
            return First.reduce() + " " + ((int)Op.TokenTag < 255 ? (char)Op.TokenTag + "" : Op.TokenTag.ToString()) + " " + Second.reduce();
        }

    }

    public class Unary : Op
    {
        public Expr Expr { get; protected set; }
        public Unary(Token tok, Expr exp) : base(tok, null)
        {
            Expr = exp;
            Type = ReserveType.Max(ReserveType.Int32, Expr.Type);
            if (Type == null) throw new TypeCovertException(tok.Line, Type, Expr.Type);
        }

        public override Expr gen()
        {
            return new Unary(Op, Expr.reduce());
        }

        public override string ToString()
        {
            return Op + " " + Expr;
        }
    }

    public class Constant : Expr
    {
        public static Constant True = new Constant(ReserveWord.True, ReserveType.Bool8);
        public static Constant False = new Constant(ReserveWord.False, ReserveType.Bool8);

        public Constant(Token tok, Symbols.Type p) : base(tok, p)
        {
        }

        public Constant(int i) : base(new Number(i, 0, 0), ReserveType.Int32)
        {
        }

        public override void jumping(int t, int f)
        {
            if (this == True && t != 0) CurrentGenerator.Goto(t);
            else if (this == False && f != 0) CurrentGenerator.Goto(f);
        }
    }

    public class Logical : Expr
    {
        public Expr First { get; protected set; }
        public Expr Second { get; protected set; }
        public Logical(Token tok, Expr x1, Expr x2) : base(tok, ReserveType.Bool8)
        {
            First = x1; Second = x2;

            if(!check())
                throw new TypeCovertException(tok.Line, First.Type, Second.Type); 
        }

        public virtual bool check()
        {
            if (!ReserveType.IsBoolean(First.Type) || !ReserveType.IsBoolean(Second.Type))
            {
                return false;
            }
            return true;
        }

        public override Expr gen()
        {
            int f = CurrentGenerator.AllocLabel();
            int a = CurrentGenerator.AllocLabel();
            Temp t = new Temp(Type);
            CurrentGenerator.Temp(t);
            jumping(0, f);
            CurrentGenerator.Set(t, Constant.True);
            CurrentGenerator.Goto(a);
            CurrentGenerator.Label(f);
            CurrentGenerator.Set(t, Constant.False);
            CurrentGenerator.Label(a);
            return t;
        }

        public override string ToString()
        {
            return First + " " + Op + " " + Second;
        }

    }

    public class Or : Logical
    {
        public Or(Token tok, Expr x1, Expr x2) : base(tok, x1, x2)
        {
        }

        public override void jumping(int t, int f)
        {
            int label = t != 0 ? t : CurrentGenerator.AllocLabel();

            First.jumping(label, 0);
            Second.jumping(t, f);

            if (t == 0) CurrentGenerator.Label(label);
        }
    }

    public class And : Logical
    {
        public And(Token tok, Expr x1, Expr x2) : base(tok, x1, x2)
        {
        }

        public override void jumping(int t, int f)
        {
            int l = f != 0 ? f : CurrentGenerator.AllocLabel();
            First.jumping(0, l);
            Second.jumping(t, f);
            if (f == 0) CurrentGenerator.Label(l);
        }
    }

    public class Not : Logical
    {
        public Not(Token tok, Expr x2) : base(tok, x2, x2)
        {
        }

        public override void jumping(int t, int f)
        {
            Second.jumping(f, t);
        }

        public override string ToString()
        {
            return Op + " " + Second;
        }
    }

    public class Rel : Logical
    {
        public Rel(Token tok, Expr x1, Expr x2) : base(tok, x1, x2)
        {
        }

        public override void jumping(int t, int f)
        {
            Expr a = First.reduce();
            Expr b = Second.reduce();

            emitjumps(this, t, f);
        }

        public override bool check()
        {
            if (First.Type is Symbols.Array || Second.Type is Symbols.Array) return false;
            else if (First.Type == Second.Type) return true;
            else if (ReserveType.IsNumberic(First.Type) && ReserveType.IsNumberic(Second.Type))
            {
                if(ReserveType.ConvertableT1T2(First.Type, Second.Type))
                {
                    return true;
                }
                return false;
            }
            else return false;
        }
    }

    public class Access : Op
    {
        public Id array { get; protected set; }
        public Expr index { get; protected set; }
        public Access(Id a, Expr i, Symbols.Type p) : base(new Word(Tag.ARRAY, "[]", a.Op.Line, a.Op.Pos), p)
        {
            array = a;
            index = i;
        }

        public override Expr gen()
        {
            return new Access(array, index.reduce(), Type);
        }

        public override void jumping(int t, int f)
        {
            emitjumps(reduce(), t, f);
        }

        public override string ToString()
        {
            return array + " [ " + index + " ]";
        }

    }

    public class PassParam : Expr
    {
        public Expr Param { get; protected set; }

        public PassParam(Expr p) : base(p.Op, p.Type)
        {
            Param = p;
        }

        public override string ToString()
        {
            //Expr t = ;
            return (Param.gen().ToString());
        }

    }

    public class FuncCall : Expr
    {
        public Function Func { get; protected set; }
        public IEnumerable<PassParam> FunParams { get; protected set; }
        public FuncCall(Function fun, IEnumerable<PassParam> param) : base(fun.Id.Op, fun.Params.ReturnType)
        {
            Func = fun;
            FunParams = param;
        }

        public override Expr reduce()
        {
            foreach (var item in FunParams)
            {
                CurrentGenerator.Param(item);
            }
            Temp t = new Temp(Func.Params.ReturnType);
            CurrentGenerator.Temp(t);
            CurrentGenerator.Call(t, Func);
            CurrentGenerator.CallEnd();
            return t;
        }

        public override string ToString()
        {
            return "call " + Func;
        }
    }

}
