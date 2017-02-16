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

        public int newlabel()
        {
            return ++labels;
        }

        public void emitLabel(int i)
        {
            CurrentIO.EmitLabel("L" + i.ToString());
        }

        public void emitLabel(string i)
        {
            CurrentIO.EmitLabel(i);
        }

        public void emit(string s)
        {
            CurrentIO.EmitLine(s);
        }

        public void emitSrc(string s)
        {
            CurrentIO.EmitSource(s);
        }

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
                        emitfunSet(item.Param, t);
                        emit("param " + t);
                    }
                    else
                    {
                        emit("param " + item);
                    }
                }
            }
            emit(toVar + " = call " + c.Func);
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
            emitjumps(ToString(), t, f);
        }

        public virtual void emitjumps(string s, int t, int f)
        {
            if (t != 0 && f != 0)
            {
                emit("if " + s + " goto L" + t);
                emit("goto L" + f);
            }
            else if (t != 0) emit("if " + s + " goto L" + t);
            else if (f != 0) emit("iffalse " + s + " goto L" + f);
            
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
        public override string ToString()
        {
            return Scope + "::" + base.ToString();
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
            emit(t + " = " + x);
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
            return First + " " + ((int)Op.TokenTag < 255 ? (char)Op.TokenTag + "" : Op.TokenTag.ToString()) + " " + Second;
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
            if (this == True && t != 0) emit("goto L" + t);
            else if (this == False && f != 0) emit("goto L" + f);
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
            int f = newlabel();
            int a = newlabel();
            Temp t = new Temp(Type);
            jumping(0, f);
            emit(t + " = true");
            emit("goto L" + a);
            emitLabel(f);
            emit(t + " = false");
            emitLabel(a);
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
            int label = t != 0 ? t : newlabel();

            First.jumping(label, 0);
            Second.jumping(t, f);

            if (t == 0) emitLabel(label);
        }
    }

    public class And : Logical
    {
        public And(Token tok, Expr x1, Expr x2) : base(tok, x1, x2)
        {
        }

        public override void jumping(int t, int f)
        {
            int l = f != 0 ? f : newlabel();
            First.jumping(0, l);
            Second.jumping(t, f);
            if (f == 0) emitLabel(l);
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
            string test = a + " " + Op + " " + b;

            emitjumps(test, t, f);
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
            emitjumps(reduce().ToString(), t, f);
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

        public override string ToString()
        {
            return "call " + Func;
        }
    }

}
