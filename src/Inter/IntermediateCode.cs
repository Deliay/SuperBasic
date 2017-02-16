using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperBasic.FrontEnd.Inter
{
    public class IntermediateCode 
    {
        public static IntermediateCode CurrentInter { get { return curInter; } }
        public static IGenerator CurrentGenerator { get { return currentGen; } }
        public static InnerIO CurrentIO { get { return currentIO; } }

        private static InnerIO currentIO = new InterDefaultTextIO();
        private static IGenerator currentGen = new DefaultGenerator();
        private static IntermediateCode curInter = null;

        public static void SetIO(InnerIO IO)
        {
            currentIO = IO;
        }

        public static void SetGenerator(IGenerator GEN)
        {
            currentGen = GEN;
        }

        public IntermediateCode()
        {
            curInter = this;
        }


    }
}
