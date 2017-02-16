using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperBasic.FrontEnd.Inter
{
    public interface InnerIO
    {
        void EmitLabel(string label);
        void EmitLine(string s);
        void EmitSource(string s);
        void If(Expr t, int Line);
        void IfFalse(Expr t, int Line);
        void Goto(int Line);
        void Equal(Expr t, Expr v);

    }

    public class InterDefaultTextIO : InnerIO
    {
        public string content = string.Empty;

        public void EmitLabel(string label)
        {
            content +=  label + ":\n";
        }

        public void EmitLine(string s)
        {
            content += "\t" + s + "\n";
        }

        public void EmitSource(string s)
        {
            content += "//" + s + "\n";
        }

        public void Equal(Expr t, Expr v)
        {
            throw new NotImplementedException();
        }

        public void Goto(int Line)
        {
            throw new NotImplementedException();
        }

        public void If(Expr t, int Line)
        {
            throw new NotImplementedException();
        }

        public void IfFalse(Expr t, int Line)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return content;
        }
    }
}
