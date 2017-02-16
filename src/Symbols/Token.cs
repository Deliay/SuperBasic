using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperBasic.FrontEnd.Symbols
{

    public enum VarType
    {
        Void,
        Bool8,
        Bool16,
        Bool32,
        Int8,
        Int16,
        Int32,
        Int64,
        UInt8,
        UInt16,
        UInt32,
        UInt64,
        Half,
        Single,
        Double,
        DoubleEx,
        IntPtr,
        UIntPtr,
        Char,
        Byte,
        Array,
        Type,
        Class,
        Interface,
        Function,
        Delegate,
        Module,
        Dynmaic,
        Property,
        Typedef,
        String,
        Const
    }

    public enum Tag
    {
        PROGRAM_END = 0,
        BitAnd = '&',
        Split = ',',
        BitOr = '|',
        Equal = '=',
        Not = '!',
        Less = '<',
        Large = '>',
        BitNot = '~',
        Add = '+',
        Sub = '-',
        Multi = '*',
        Div = '/',
        Mod = '%',
        LeftBra = '[',
        RightBar = ']',
        LeftPar = '(',
        RightPar = ')',
        LeftBlock = '{',
        RightBlock = '}',
        TAG_START = 256, PROGRAM_TEMP, LINE_END,
        NUMBER,             //NUMBER
        REAL,               //Floats
        ARRAY,              //Array
        IDENTITY,           //IDENTITIES
        TRUE,               //TRUE Flag
        FALSE,              //FALSE Flag
        MODULE,
        IF, ELSE, ELSEIF, THEN,
        DO, WHILE, UNTIL, WEND, BREAK, CONTINUE,
        STATIC, CLASS, FUNCTION, SUB, RETURN, OPTIONAL,
        AND, OR, EQV, NEQV, LESEQ, LARGEEQ,
        MINUS,
        BASICTYPE,
        LITERAL,            //"" for 
        DIM,                //Dim Keyword
        AS,                 //As Keyword
        END,                //End Keyword
        TAG_END,
    }

    public enum Operators
    {
        Group3 = 30,
        Mod,
        Div = Mod,
        Mul = Mod,
        Add,
        Sub = Add,
        SHL,
        SHR = SHL,
        Group4 = 40,
        Less,
        LessEq = Less,
        Large = Less,
        LargeEq = Less,
        LogicEq,
        LogicNEq = LogicEq,
        Group8 = 80,
        Equal,

    }

    public class Array : Type
    {
        private Type type;
        private int width = 0;
        public Array(Type t, int size) : base("[]", Tag.ARRAY, t.Width * size, 0, 0)
        {
            type = t;
            width = size;
        } 

        public Type Type
        {
            get { return type; }
        }

        public int Widht
        {
            get { return width; }
        }

        public override string ToString()
        {
            return "[" + width + "] " + type.lexeme;
        }

        public static bool IsArray(Type t)
        {
            return t is Array;
        }
    }

    public class ReserveType : Type
    {
        public static Type Int8 = new ReserveType(VarType.Int8, Tag.BASICTYPE, 1);
        public static Type Int16 = new ReserveType(VarType.Int16, Tag.BASICTYPE, 2);
        public static Type Int32 = new ReserveType(VarType.Int32, Tag.BASICTYPE, 4);
        public static Type Int64 = new ReserveType(VarType.Int64, Tag.BASICTYPE, 8);
        public static Type UInt8 = new ReserveType(VarType.UInt8, Tag.BASICTYPE, 1);
        public static Type UInt16 = new ReserveType(VarType.UInt16, Tag.BASICTYPE, 2);
        public static Type UInt32 = new ReserveType(VarType.UInt32, Tag.BASICTYPE, 4);
        public static Type UInt64 = new ReserveType(VarType.UInt64, Tag.BASICTYPE, 8);
        public static Type Half = new ReserveType(VarType.Half, Tag.BASICTYPE, 2);
        public static Type Single = new ReserveType(VarType.Single, Tag.BASICTYPE, 4);
        public static Type Double = new ReserveType(VarType.Double, Tag.BASICTYPE, 8);
        public static Type IntPtr = new ReserveType(VarType.IntPtr, Tag.BASICTYPE, 4);
        public static Type UIntPtr = new ReserveType(VarType.UIntPtr, Tag.BASICTYPE, 4);
        public static Type Bool8 = new ReserveType(VarType.Bool8, Tag.BASICTYPE, 1);
        public static Type Bool16 = new ReserveType(VarType.Bool16, Tag.BASICTYPE, 2);
        public static Type Bool32 = new ReserveType(VarType.Bool32, Tag.BASICTYPE, 4);
        public static Type Char = new ReserveType(VarType.Char, Tag.BASICTYPE, 2);
        public static Type Byte = new ReserveType(VarType.Byte, Tag.BASICTYPE, 1);
        public static Type Void = new ReserveType(VarType.Void, Tag.BASICTYPE, 0);
        public static Type Function = new ReserveType(VarType.Function, Tag.FUNCTION, 4);

        public ReserveType(VarType s, Tag t, int size) : base(s, t, size, 0, 0)
        {

        }


        public static bool Except(Type t, params Type[] ts)
        {
            return ts.Contains(t);
        }

        public static bool IsBoolean(Type t)
        {
            return Except(t, Bool8, Bool16, Bool32);
        }

        public static bool IsNumberic(Type t)
        {
            return Except(t, Int16, Int32, Int64, UInt16, UInt32, UInt64, UIntPtr, IntPtr, Char, Byte, Half, Single, Double);
        }

        public static bool IsInteger(Type t)
        {
            return Except(t, Int16, Int32, Int64, UInt16, UInt32, UInt64, UIntPtr, IntPtr, Char, Byte);
        }

        public static bool IsFloat(Type t)
        {
            return Except(t, Half, Single, Double);
        }

        public static Type Max(Type p1, Type p2)
        {
            if (p1 == p2) return p1;

            if (p1.Width == p2.Width)
            {
                return IsFloat(p1) ? p1 : p2;
            }
            else
            {
                return p1.Width > p2.Width ? p1 : p2;
            }
        }

        public static bool ConvertableT1T2(Type t1, Type t2)
        {
            if (t1 == t2) return true;
            if (IsNumberic(t1) && IsNumberic(t2) && Max(t1, t2) == t1)
                return true;
            else if (IsBoolean(t1) && IsBoolean(t2))
                return true;

            return false;
        }
    }

    public class Type : Word
    {


        private int width = 1;
        public Type(string s, Tag t, int size, int line, int pos) : base(t, s, line, pos)
        {
            width = size;
        }
        public Type(VarType s, Tag t, int size, int line, int pos) : this(s.ToString(), t, size, line, pos)
        {

        }

        public int Width
        {
            get { return width; }
        }

    }

    public class Real : Token
    {
        double value = 0.0d;
        public bool IsFloat { get; protected set; }
        public Real(double val, int line, int pos, bool isFloat = false) : base(Tag.REAL, line, pos)
        {
            IsFloat = isFloat;
            value = val;
        }

        public override string ToString()
        {
            return "" + value;
        }
    }

    public class EndOFLine : Token
    {
        public EndOFLine(int line, int pos) : base(Tag.LINE_END, line, pos)
        {

        }
    }

    public class Operator : Word
    {
        private Operators lvl;
        public Operator(Tag t, string s, Operators Level) : base(t, s, 0, 0)
        {
            lvl = Level;
        }

        public Operators Level
        {
            get { return lvl; }
        }
    }

    public class ReserveWord : Word
    {
        public static ReserveWord
            True = new ReserveWord(Tag.TRUE, "True"),
            False = new ReserveWord(Tag.FALSE, "False"),
            And = new ReserveWord(Tag.AND, "&&"),
            Or = new ReserveWord(Tag.OR, "||"),
            Eqv = new ReserveWord(Tag.EQV, "=="),
            NEqv = new ReserveWord(Tag.NEQV, "!="),
            LessEq = new ReserveWord(Tag.LESEQ, "<="),
            LargeEq = new ReserveWord(Tag.LARGEEQ, ">="),
            Minus = new ReserveWord(Tag.MINUS, "minus");

        public ReserveWord(Tag t, string s) : base(t, s, 0, 0)
        {
            
        }
    }

    public class Word : Token
    {
        public static Word Temp = new Word(Tag.PROGRAM_TEMP, "t", 0, 0);

        public string lexeme;
        public Word(Tag t, string s, int line, int pos) : base(t, line, pos)
        {
            lexeme = s;
        }

        public override string ToString()
        {
            return lexeme;
        }
    }

    public class Number : Token
    {
        public int value;
        public Number(int v, int line, int pos) : base(Tag.NUMBER, line, pos)
        {
            value = v;
        }

        public override string ToString()
        {
            return "" + value;
        }
    }

    public class Token
    {
        public Tag TokenTag;
        public readonly int Line;
        public readonly int Pos;
        public Token(Tag t, int line, int pos)
        {
            TokenTag = t;
            Line = line;
            Pos = pos;
        }

        public Token(char t, int line, int pos)
        {
            TokenTag = (Tag)t;
            Line = line;
            Pos = pos;
        }

        public override string ToString()
        {
            return TokenTag.ToString();
        }
    }
}
