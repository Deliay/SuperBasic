using SuperBasic.FrontEnd.Errors;
using SuperBasic.FrontEnd.Inter;
using SuperBasic.FrontEnd.Lexer;
using SuperBasic.FrontEnd.Symbols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SuperBasic.FrontEnd.Inter.IntermediateCode;

namespace SuperBasic.FrontEnd.Parser
{
    /* Program  -> Modules
     * Modules  -> Module
     * Module   -> Decl [Static]Functions([PARAMS])
     * //Class    -> [Constructor] Decls Functions([PARAMS]) Property Id As Type
     * Function -> Block
     * Block    -> Decls  Stmts
     * Decls    -> Decl | empty
     * Decl     -> Dim Id As Type, Delegate Id[PARAMS], Dynamic T([PARAMS])
     * Type     -> type[n] | BasicType
     * Stmts    -> Stmts Stms | empty
     * 
     * Stmt -> loc = bool
     *      |   If bool Then Stmt
     *      |   If bool Then Stmt Else Stmt
     *      |   While bool Stmt Wend
     *      |   Do Stmt While bool
     *      |   Break
     *      |   Continue
     *      |   block
     *      
     * loc = loc[bool] | id
     * -----------------------------
     * bool  -> bool || join | join
     * join  -> join && eqv | eqv
     * eqv   -> eqv == rel | eqv != rel | rel
     * rel   -> expr < expr | expr <= expr | expr >= expr | expr > expr | expr
     * expr  -> expr + term | expr - term | term
     * term  -> term * unary | term / unary | unary
     * unary -> !unary | -unary | factor
     * factor-> (bool) | loc | Number | Real | True | false
     */
    public class Parser
    {
        private LexicalAnalyzer lex;
        private TokenReader reader;
        private SymbolTable top = new SymbolTable(null);
        private Token look;
        private string scope_ = "";
        private string NextScopeName
        {
            get
            {
                string t = scope_;
                scope_ = "";
                return t;
            }
            set
            {
                scope_ = value;
            }
        }
        private int used = 0;
        private Stmt s;

        public Stmt CodeTree { get { return s; } }

        public SymbolTable SymbolScopes { get { return top; } }

        public Parser(LexicalAnalyzer lexical)
        {
            lex = lexical;
            reader = new TokenReader(lex.Tokens);

        }
        
        private void move()
        {
            look = reader.Read();
            while(look.TokenTag == Tag.LINE_END) look = reader.Read();
        }

        public bool Test(params Tag[] tags)
        {
            return tags.Contains(look.TokenTag);
        }

        public bool TestGrammar(params Tag[] tags)
        {
            if (tags.Contains(look.TokenTag)) return true;
            else throw new TokenWrongException(look, tags);
        }

        public bool Except(params Tag[] tags)
        {
            if (tags.Contains(look.TokenTag))
            {
                move();
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Except(params char[] tags)
        {
            if (look != null && tags.Contains((char)look.TokenTag))
            {
                move();
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool ExceptGrammar(params Tag[] tags)
        {
            if (look != null && tags.Contains(look.TokenTag))
            {
                move();
                return true;
            }
            else
            {
                throw new TokenWrongException(look, tags);
            }
        }

        public bool ExceptGrammar(params char[] tags)
        {
            if (look != null && tags.Contains((char)look.TokenTag))
            {
                move();
                return true;
            }
            else
            {
                throw new TokenWrongException(look, (Tag)tags[0]);
            }
        }

        //Program  -> Block
        public void Program()
        {
            move();
            s = Modules();
            CurrentGenerator.Start(top.SubTables.First());
            int begin = CurrentGenerator.AllocLabel(), after = CurrentGenerator.AllocLabel();
            CurrentGenerator.Label(begin);
            s.gen(begin, after);
            CurrentGenerator.Label(after);
            CurrentGenerator.End();
        }

        private Stmt Modules()
        {
            if (Except(Tag.RightBlock))
                return new Seq(new EndScope(look.Line, top), Stmt.Null);
            else if (Tag.PROGRAM_END == look.TokenTag) return Stmt.Null;

            while(Except(Tag.MODULE))
            {
                if (Test(Tag.IDENTITY))
                {
                    NextScopeName = (look as Word).lexeme;
                    ExceptGrammar(Tag.IDENTITY);
                }
                ExceptGrammar(Tag.LeftBlock);
                SymbolTable saveEnv = top;
                top = new SymbolTable(top, NextScopeName);
                Stmt s = new Seq(Module(), Modules());
                top = saveEnv;
                return s;
            }
            return new Seq(Module(), Modules());
        }

        private Stmt Module()
        {
            //Static Function & Class Only
            bool isStatic = false;
            while(Except(Tag.STATIC))
            {
                if (ExceptGrammar(Tag.FUNCTION))
                {
                    Word n = look as Word;
                    Function func = new Function(n.Line);
                    Stmt.LastFunction = func;
                    ExceptGrammar(Tag.IDENTITY);
                    NextScopeName = n.lexeme;

                    SymbolTable parent = top;
                    SymbolTable fc = new SymbolTable(top, NextScopeName);

                    //point TOP scope to func scope
                    top = fc;
                    //add param define
                    Params p = Params();

                    //set new identity to func
                    Id i = new IdFunc(func, n, parent, ReserveType.Function, used, isStatic);
                    //add to parent scope
                    parent.Add(n.lexeme, i);

                    // initial func first for call-self
                    func.Init(p, null, i);

                    //parser code-blocks
                    Stmt b = Block(false);

                    //add func-addr width
                    used += ReserveType.Function.Width;

                    //full initial func
                    func.Init(p, b, i);

                    //restore scope
                    top = parent;

                    Stmt.LastFunction = Stmt.Null;
                    return new Seq(func, new EndScope(look.Line, fc));
                }
                else
                    isStatic = true;
            }

            if(Except(Tag.CLASS))
            {
                return null;
            }

            if(isStatic)
            {
                ExceptGrammar(Tag.CLASS, Tag.FUNCTION);
            }
            return null;
        }

        private Params Params()
        {
            Symbols.Type type = ReserveType.Void;
            List<Param> param = new List<Param>();

            ExceptGrammar(Tag.LeftPar);
            Token cur = look;
            
            while(Test(Tag.IDENTITY, Tag.OPTIONAL))
            {
                Param p = Param();
                param.Add(p);
                top.Add(p.ParamName.lexeme, new Id(p, top, 0, false));
            }
            ExceptGrammar(Tag.RightPar);
            if(Except(Tag.AS))
            {
                //just support reserve type
                type = look as Symbols.Type;
                ExceptGrammar(Tag.BASICTYPE);
            }
            
            return new Params(cur.Line, param, type);

        } 

        private Param Param()
        {
            bool isOpt = Except(Tag.OPTIONAL);
            Word name = look as Word;
            ExceptGrammar(Tag.IDENTITY);
            ExceptGrammar(Tag.AS);
            Symbols.Type type = look as Symbols.Type;
            ExceptGrammar(Tag.BASICTYPE);
            if(isOpt && ExceptGrammar(Tag.Equal))
            {
                Expr initVal = Expr();
                if(!(initVal is Constant))
                {
                    throw new TokenNotConstantException(initVal);
                }
                Except(Tag.Split);
                return new OptParam(name, type, initVal as Constant);
            }
            else
            {
                Except(Tag.Split);
                return new Param(name, type);
            }
        }

        //Block    -> Decls  Stmts
        private Stmt Block(bool newScope = true)
        {
            SymbolTable saveEnv = top, curEnv = new SymbolTable(top, NextScopeName);
            if(newScope) top = curEnv;
            int line = look.Line;
            Stmt s = stmts();
            if(newScope) top = saveEnv;
            return new Seq(new Scope(line, curEnv), s);

        }

        private Stmt stmts()
        {
            if (Except(Tag.RightBlock))
                return new Seq(new EndScope(look.Line, top), Stmt.Null);
            else if (Tag.PROGRAM_END == look.TokenTag) return Stmt.Null;
            else if (Test(Tag.STATIC, Tag.FUNCTION, Tag.CLASS))
                return Stmt.Null;

            return new Seq(stmt(), stmts());
        }

        private Stmt stmt()
        {
            Expr x;
            Stmt s1, s2;
            Stmt saveStmt;
            if (Except(Tag.IF))
            {
                ExceptGrammar(Tag.LeftPar);
                x = Bool();
                ExceptGrammar(Tag.RightPar);
                s1 = stmt();

                if (!Except(Tag.ELSE)) return new If(x, s1);
                s2 = stmt();
                return new Else(x, s1, s2);
            }
            else if(Except(Tag.WHILE))
            {
                While node = new While(look.Line);
                saveStmt = Stmt.Encloseing;
                Stmt.Encloseing = node;

                ExceptGrammar(Tag.LeftPar); x = Bool(); ExceptGrammar(Tag.RightPar);
                s1 = stmt();
                node.Init(x, s1);

                Stmt.Encloseing = saveStmt;
                return node;
            }
            else if(Except(Tag.DO))
            {
                Do node = new Do(look.Line);
                saveStmt = Stmt.Encloseing;
                Stmt.Encloseing = node;
                s1 = stmt();
                ExceptGrammar(Tag.WHILE); ExceptGrammar(Tag.LeftPar); x = Bool();
                ExceptGrammar(Tag.RightPar);
                
                node.Init(s1, x);

                Stmt.Encloseing = saveStmt;
                return node;
            }
            else if(Except(Tag.DIM))
            {
                bool isStatic = Except(Tag.STATIC);
                Word curName = look as Word;
                ExceptGrammar(Tag.IDENTITY);
                ExceptGrammar(Tag.AS);
                Symbols.Type type = look as Symbols.Type;
                ExceptGrammar(Tag.BASICTYPE);
                if (Except(Tag.LeftBra))
                {
                    // is array
                    dims(ref type, curName);
                }
                Id id = new Id(curName, top, type, used, isStatic);
                top.Add(curName.lexeme, id);
                used += type.Width;

                if (Except(Tag.Equal))
                {
                    //a init val
                    x = Bool();
                    return new Set(id, x);
                }

                return Stmt.Null;
            }
            else if(Except(Tag.RETURN))
            {
                Return ret;
                if (reader.ReadLast().TokenTag != Tag.LINE_END)
                {
                    ret = new Return(look.Line, Stmt.LastFunction as Function, Bool());
                    return ret;
                }
                else
                {
                    move();
                    return new Return(look.Line);
                }
            }
            else if(Except(Tag.BREAK))
            {
                return new Break(look.Line);
            }
            else if(Except(Tag.CONTINUE))
            {
                return new Continue(look.Line);
            }
            else if(Except(Tag.LeftBlock))
            {
                return Block();
            }
            else
            {
                return Assign();
            }
        }

        private Stmt Assign()
        {
            Word t = look as Word;
            ExceptGrammar(Tag.IDENTITY);
            Stmt stmt; 
            Id id = top[t.lexeme];
            if (id == null) throw new TokenIdentityNotDeclareException(t);
            if (Except(Tag.Equal))
            {
                stmt = new Set(id, Bool());
            }
            else if(Except(Tag.LeftPar))
            {
                //func call
                stmt = new Call(new FuncCall((id as IdFunc).Func, ReadParams()));
            }
            else
            {
                ExceptGrammar(Tag.LeftBra);
                Access x = Offset(id);
                ExceptGrammar(Tag.Equal);
                stmt = new SetElem(x, Bool());
            }
            return stmt;

 
        }

        private Expr Bool()
        {
            Expr x = Join();
            Token cur = look;
            if (Except(Tag.OR))
            {
                return new Or(cur, x, Join());
            }

            return x;
        }

        private Expr Join()
        {
            Expr x = Equality();
            Token cur = look;
            if (Except(Tag.AND))
            {
                return new And(cur, x, Equality());
            }

            return x;
        }

        private Expr Equality()
        {
            Expr x = Rel();
            Token cur = look;
            if (Except(Tag.EQV, Tag.NEQV))
            {
                return new Rel(cur, x, Rel());
            }

            return x;
        }

        private Expr Rel()
        {
            Expr x = Expr();
            Token cur = look;
            if (Except(Tag.Less, Tag.LESEQ, Tag.LARGEEQ, Tag.Large)) 
            {
                return new Rel(cur, x, Expr());
            }

            return x;
        }

        private Expr Expr()
        {
            Expr x = Term();
            Token cur = look;
            while (Except('+') || Except('-'))
            {
                x = new Arith(cur, x, Term());
            }
            return x;
        }

        private Expr Term()
        {
            Expr x = Unary();
            Token cur = look;
            while(Except('*') || Except('/') || Except('%'))
            {
                x = new Arith(cur, x, Unary());
            }
            return x;
        }

        private Expr Unary()
        {
            Token cur = look;
            if (Except('-'))
            {
                return new Unary(ReserveWord.Minus, Unary());
            }
            else if (Except('!'))
            {
                return new Not(cur, Unary());
            }
            else return Factor();
        }

        private Expr Factor()
        {
            Expr x = null;
            switch (look.TokenTag)
            {
                case Tag.LeftPar:
                move(); x = Bool(); ExceptGrammar(Tag.RightPar);
                return x;

                case Tag.NUMBER:
                x = new Constant(look, ReserveType.Int32);
                move(); return x;

                case Tag.REAL:
                if ((look as Real).IsFloat)
                    x = new Constant(look, ReserveType.Single);
                else
                    x = new Constant(look, ReserveType.Double);
                move(); return x;

                case Tag.TRUE:
                x = Constant.True;
                move(); return x;

                case Tag.FALSE:
                x = Constant.False;
                move(); return x;

                case Tag.IDENTITY:
                string s = (look as Word).lexeme;
                Id id = top[s];
                if (id == null) throw new TokenIdentityNotDeclareException(look as Word);
                if ((!(id is IdFunc)) && (!(id.Type is Symbols.Array)) && id.Offset!=0 && !id.IsInitial) throw new IdentityNotInitialException(look.Line, id);
                move();
                if (id.Type.TokenTag == Tag.FUNCTION)
                {
                    ExceptGrammar(Tag.LeftPar);
                    return new FuncCall((id as IdFunc).Func, ReadParams());
                }
                if(Except(Tag.LeftBra))
                {
                    return Offset(id);
                }
                else
                {
                    return id;
                }
                default:
                throw new TokenWrongException(look, Tag.IDENTITY);
            }
        }

        private IEnumerable<PassParam> ReadParams()
        {
            //pass params
            List<PassParam> p = new List<PassParam>();
            while (!Except(Tag.RightPar))
            {
                p.Add(ReadParam());
                Except(Tag.Split);
            }
            return p;
        }

        private PassParam ReadParam()
        {
            return new PassParam(Bool());
        }

        private Access Offset(Id a)
        {
            Expr i, t1, t2, loc;
            Symbols.Type type = a.Type;
            i = Bool();
            ExceptGrammar(Tag.RightBar);
            type = (type as Symbols.Array).Type;
            t1 = new Constant(type.Width);
            loc = t1;
            while(Except(Tag.LeftBra))
            {
                i = Bool();ExceptGrammar(Tag.RightBar);
                t1 = new Constant(type.Width);
                t2 = new Arith(new Token('+', a.lexline, 0), loc, t1);
            }
            return new Access(a, loc, type);
        }

        void dims(ref Symbols.Type p, Word name)
        {
            Number size = look as Number;
            if (ExceptGrammar(Tag.NUMBER))
            {
                if (ExceptGrammar(Tag.RightBar))
                {
                    if(Except(Tag.LeftBra))
                    {
                        dims(ref p, name);
                    }
                    p = new Symbols.Array(p, size.value);
                }
                else
                {
                    throw new TokenNotCloseArrayException(name);
                }
            }
        }


    }

    class TokenReader
    {
        List<Token> tokens;
        int currentIndex = 0;

        public TokenReader(IEnumerable<Token> tokens)
        {
            this.tokens = new List<Token>(tokens);
        }

        public Token ReadLast()
        {
            return tokens[currentIndex - 2];
        }

        public Token Read()
        {
            return tokens[currentIndex++];

        }

        public Token ReadNext()
        {
            return tokens[currentIndex];
        }

        public void Reset()
        {
            currentIndex = 0;
        }
    }
}
