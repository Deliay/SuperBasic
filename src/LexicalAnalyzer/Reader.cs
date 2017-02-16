using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperBasic.FrontEnd.Lexer
{
    public class Reader
    {
        string codes;
        string[] codesLine;
        int curPos = 0, curLine = 0;
        public Reader(string code)
        {
            codes = code;
            codesLine = code.Split('\n');
        }

        public int CurrentPosition
        {
            get { return curPos; }
        }

        public string CurrentLine
        {
            get { return codesLine[curLine]; }
        }

        public void Back()
        {
            curPos--;
        }

        public char CurrentChar
        {
            get { return codes[curPos - 1]; }
        }

        public char Read()
        {
            
            if (curPos < codes.Length) return codes[curPos++];
            else return '\0';
        }

        public bool EOF()
        {
            return curPos + 1 <= codes.Length;
        }

        public bool isTerminalSymbol(char ch)
        {
            
            return "+-*/%=><!&|".Contains(ch);
        }
    }
}