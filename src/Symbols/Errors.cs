using SuperBasic.FrontEnd.Inter;
using SuperBasic.FrontEnd.Symbols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperBasic.FrontEnd.Errors
{

    public class SourceBlocksNotCloseException : Exception
    {
        public SourceBlocksNotCloseException(int line) : base("Ln " + line + ", Block not close.")
        {

        }
    }

    public class TypeMisMatchException : Exception
    {
        public TypeMisMatchException(int line, Symbols.Type T, string except) : base("Ln " + line + ", Type " + T + " not match covert rule (except: " + except.ToString() + ")")
        {

        }
    }

    public class TypeCovertException : Exception
    {
        public TypeCovertException(int line, Symbols.Type T1, Symbols.Type T2) : base("Ln " + line + ", Type " + T1 + " cannot convert to " + T2)
        {

        }
    }

    public class TokenWrongException : Exception
    {
        public TokenWrongException(Token token, params Tag[] except) : base("Ln " + token.Line + ", " + token.TokenTag.ToString() + " should " + except[0])
        {

        }
    }

    public class TokenPreserveException : Exception
    {
        public TokenPreserveException(int line, Word token) : base("Ln " + line + ", Cannot use preserve keyword: " + token.lexeme)
        {

        }
    }

    public class TokenIdentityNotDeclareException : Exception
    {
        public TokenIdentityNotDeclareException(Word token) : base("Ln " + token.Line + ", Syntax not defined: " + token.lexeme)
        {

        }
    }

    public class TokenNotCloseArrayException : Exception
    {
        public TokenNotCloseArrayException(Word token) : base("Ln " + token.Line + ", Bracket not close")
        {

        }
    }

    public class TokenNotConstantException : Exception
    {
        public TokenNotConstantException(Expr token) : base("Ln " + token.lexline + ", Cannot initial Optioanl param without Constant")
        {

        }
    }

    public class IdentityNotInitialException : Exception
    {
        public IdentityNotInitialException(int line, Id id) : base("使用了未初始化的变量 " + id.Identity)
        {

        }
    }

}
