using SuperBasic.FrontEnd.Symbols;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperBasic.FrontEnd.Lexer
{
    public class TokenTable : List<Token>
    {

    }

    public class LexicalAnalyzer
    {
        string source = string.Empty;
        public static Reader reader;
        private int currentLine = 1;
        private Dictionary<string, Token> words;
        private TokenTable tokens;
        void reserve(Word t) { words.Add(t.lexeme, t); }
        void reserve(Symbols.Type t) { words.Add(t.lexeme, t); }

        public IEnumerable<Token> Tokens
        {
            get { return tokens; }
        }

        public LexicalAnalyzer(string workPath)
        {
            tokens = new TokenTable();

            words = new Dictionary<string, Token>();
            reserve(ReserveWord.True);
            reserve(ReserveWord.False);
            reserve(ReserveWord.And);
            reserve(ReserveWord.Eqv);
            reserve(ReserveWord.LargeEq);
            reserve(ReserveWord.LessEq);
            reserve(ReserveWord.Minus);
            reserve(ReserveWord.NEqv);
            reserve(ReserveWord.Or);
            reserve(new ReserveWord(Tag.DIM, "Dim"));
            reserve(new ReserveWord(Tag.IF, "If"));
            reserve(new ReserveWord(Tag.ELSE, "Else"));
            reserve(new ReserveWord(Tag.ELSEIF, "ElseIf"));
            reserve(new ReserveWord(Tag.THEN, "Then"));
            reserve(new ReserveWord(Tag.END, "End"));
            reserve(new ReserveWord(Tag.AS, "As"));
            reserve(new ReserveWord(Tag.DO, "Do"));
            reserve(new ReserveWord(Tag.BREAK, "Break"));
            reserve(new ReserveWord(Tag.CONTINUE, "Continue"));
            reserve(new ReserveWord(Tag.WHILE, "While"));
            reserve(new ReserveWord(Tag.WEND, "Wend"));
            reserve(new ReserveWord(Tag.FUNCTION, "Function"));
            reserve(new ReserveWord(Tag.SUB, "Sub"));
            reserve(new ReserveWord(Tag.RETURN, "Return"));
            reserve(new ReserveWord(Tag.OPTIONAL, "Optional"));
            reserve(new ReserveWord(Tag.CLASS, "Class"));
            reserve(new ReserveWord(Tag.MODULE, "Module"));
            reserve(new ReserveWord(Tag.STATIC, "Static"));
            reserve(new ReserveWord(Tag.Split, ","));
            reserve(new Operator(Tag.Equal, "=", Operators.Equal));
            reserve(new Operator(Tag.Add, "+", Operators.Add));
            reserve(new Operator(Tag.Sub, "-", Operators.Sub));
            reserve(new Operator(Tag.Multi, "*", Operators.Mul));
            reserve(new Operator(Tag.Div, "/", Operators.Div));
            reserve(new Operator(Tag.Mod, "%", Operators.Mod));
            reserve(new Operator(Tag.Less, "<", Operators.Less));
            reserve(new Operator(Tag.Large, ">", Operators.Large));
            reserve(ReserveType.Bool8);
            reserve(ReserveType.Bool16);
            reserve(ReserveType.Bool32);
            reserve(ReserveType.Int8);
            reserve(ReserveType.Int16);
            reserve(ReserveType.Int32);
            reserve(ReserveType.Int64);
            reserve(ReserveType.UInt8);
            reserve(ReserveType.UInt16);
            reserve(ReserveType.UInt32);
            reserve(ReserveType.UInt64);
            reserve(ReserveType.Half);
            reserve(ReserveType.Single);
            reserve(ReserveType.Double);
            reserve(ReserveType.IntPtr);
            reserve(ReserveType.UIntPtr);
            reserve(ReserveType.Char);
            reserve(ReserveType.Byte);
            reserve(ReserveType.Void);
        }

        public Token Scan()
        {
            char peek = ' ';
            for(; ; peek = reader.Read())
            {
                if (!reader.EOF()) return new Token('\0', currentLine, reader.CurrentPosition);
                if (peek == ' ' || peek == '\t' || peek == '\r') continue;
                else if (peek == '\n')
                {
                    currentLine++;
                    return new Token(Tag.LINE_END, currentLine, reader.CurrentPosition);
                }
                else break;
            }

            if(char.IsDigit(peek))
            {
                int v = 0;
                do
                {
                    v = 10 * v + System.Convert.ToInt32(char.GetNumericValue(peek));
                    peek = reader.Read();
                } while (char.IsDigit(peek));
                if (peek != '.')
                {
                    reader.Back();
                    return new Number(v, currentLine, reader.CurrentPosition);
                }
                double x = v, d = 10;
                for(;;)
                {
                    peek = reader.Read();
                    if (!char.IsDigit(peek)) break;
                    x = x + char.GetNumericValue(peek) / d; d *= 10;
                }
                if(peek == '!' || peek == 'f')
                {
                    return new Real(x, currentLine, reader.CurrentPosition, true);
                }
                reader.Back();
                return new Real(x, currentLine, reader.CurrentPosition);
            }
            
            if(char.IsLetter(peek))
            {
                string b = string.Empty;
                do
                {
                    b += peek;
                    peek = reader.Read();

                } while (char.IsLetterOrDigit(peek));
                reader.Back();
                if (words.ContainsKey(b)) return words[b];
                Word w = new Word(Tag.IDENTITY, b, currentLine, reader.CurrentPosition);
                words.Add(b, w);
                return w;
            }

            if(reader.isTerminalSymbol(peek))
            {
                string b = string.Empty;
                do
                {
                    b += peek;
                    peek = reader.Read();

                } while (reader.isTerminalSymbol(peek));
                reader.Back();
                if (words.ContainsKey(b)) return words[b];
                Word w = new Word(Tag.IDENTITY, b, currentLine, reader.CurrentPosition);
                return w;
            }

            if(peek == '"')
            {
                string b = string.Empty;
                do
                {
                    b += peek;
                    peek = reader.Read();
                } while (peek != '"');

                Word w = new Word(Tag.LITERAL, b.Substring(1), currentLine, reader.CurrentPosition);
                return w;
            }

            Token t = new Token(peek, currentLine, reader.CurrentPosition);
            peek = ' ';
            return t;

        }

        public bool ExistInTokens(int id)
        {
            return Enum.GetName(typeof(Tag), id) != null;
        }

        public bool LoadSource(string sourceCode)
        {
            bool succ = true;
            source = sourceCode;
            reader = new Reader(source);
            Token t = Scan();
            do
            {
                if (!ExistInTokens((int)t.TokenTag))
                {
                    Console.WriteLine("Ln " + currentLine + ", Cannot read: " + reader.CurrentChar);
                    succ = false;
                }
                tokens.Add(t);
                t = Scan();

            } while (t.TokenTag != Tag.PROGRAM_END);
            tokens.Add(new Token(Tag.PROGRAM_END, currentLine + 1, reader.CurrentPosition));
            return succ;
        }
    }
}
