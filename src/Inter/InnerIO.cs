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
        void EmitBlock(string s);
        void Emit(string s);
        string FileExt();
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

        public void EmitBlock(string s)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return content;
        }

        public void Emit(string s)
        {
            throw new NotImplementedException();
        }

        public string FileExt()
        {
            return "tsb";
        }
    }

    public class CppTextIO : InnerIO
    {
        public string content = string.Empty;

        public void EmitLabel(string label)
        {
            content += label + ":{}\n";
        }

        public void Emit(string s)
        {
            content += s;
        }

        public void EmitBlock(string s)
        {
            content += "\t" + s + "\n";
        }

        public void EmitLine(string s)
        {
            content += "\t" + s + ";\n";
        }

        public void EmitSource(string s)
        {
            content += "//" + s + "\n";
        }

        public override string ToString()
        {
            return content;
        }

        public string FileExt()
        {
            return "cpp";
        }
    }
}
