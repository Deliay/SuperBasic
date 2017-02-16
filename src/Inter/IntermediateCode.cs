using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperBasic.FrontEnd.Inter
{
    public class Code
    {
        public enum Operator_
        {
            ifFalse,
            ifTrue,
            @goto,
            def,
            param,
            call,
            add, sub, mul, div, mod,
            band, bor, bxor, bnot,
            and, or, not,
            eq, neq,
            shl, shr,
            label,
        }

        public class Operator
        {
            Operator_ value;

            public Operator(Operator_ v)
            {
                value = v;
            }

            public static implicit operator Operator_ (Operator opt)
            {
                return opt.value;
            }

            public static implicit operator Operator(Operator_ opt)
            {
                return new Operator(opt);
            }

            public static implicit operator int(Operator opt)
            {
                return (int)opt.value;
            }
        }

        protected Operator op;
        protected Node arg1;
        protected Node arg2;
        protected int result;
        protected int line;
        protected int pos;

        public Code(int line, int pos)
        {
            this.line = line;
            this.pos = pos;
        }

        public Code(int line, int pos, Operator op, Node arg1, Node arg2, int result) : this(line, pos)
        {
            this.op = op;
            this.arg1 = arg1;
            this.arg2 = arg2;
            this.result = result;
        }

    }

    public class IntermediateCode 
    {
        public static IntermediateCode CurrentInter { get { return curInter; } }
        public static InnerIO CurrentIO { get { return currentIO; } }
        private static InnerIO currentIO = new InterDefaultTextIO();
        private static IntermediateCode curInter = null;
        private List<Code> lstCode;
        public static void SetIO(InnerIO IO)
        {
            currentIO = IO;
        }

        public void addCode(Code c)
        {

        }

        public IntermediateCode()
        {
            lstCode = new List<Code>();
            curInter = this;
        }

        public void Attach(IEnumerable<Code> codes)
        {

        }


    }
}
