using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SuperBasic.FrontEnd.Lexer;
using SuperBasic.FrontEnd.Parser;
using SuperBasic.FrontEnd.Inter;
using System.Diagnostics;

namespace SuperBasicFornt
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string baseDirectory = string.Empty;

#if (NOTCORE)
            baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
#endif

#if (NETCORE)
            baseDirectory = AppContext.BaseDirectory;
#endif

            Console.WriteLine("Super Basic Front-End Executor");

            if (args.Length != 3)
            {
                Console.WriteLine("\n\nWrong param passed");
                Console.WriteLine("Param : SuperBasic [file] [gen] [genopt]");
            }
            else
            {
                string filePath = args[0];
                string gen = args[1];
                string genopt = args[2];
                Stopwatch time = new Stopwatch();
                LexicalAnalyzer la = new LexicalAnalyzer(baseDirectory);

                Console.WriteLine("Loading file....");
                string codeLoaded = File.ReadAllText(filePath);
                Console.WriteLine("Readed " + codeLoaded.Length + " words.\n\n");

                Console.WriteLine("Lexeical analyzing..");
                time.Start();
                if(!la.LoadSource(codeLoaded))
                {
                    Console.WriteLine("Source code analysis failed");
                    Console.ReadLine();
                    return;
                }
                time.Stop();
                Console.WriteLine("Elapsed : " + time.ElapsedMilliseconds + " ms");
                Console.WriteLine("Readed " + la.Tokens.Count() + " Tokens\n\n");
                Console.WriteLine("Parser to: " + gen);
                if (gen == "cpp")
                {
                    IntermediateCode.SetIO(new CppTextIO());
                    IntermediateCode.SetGenerator(new CppGenerator(genopt));
                }
                Console.WriteLine("Generator Param: " + genopt);
                Parser p = new Parser(la);
                Console.WriteLine("\n\nParsering...");
                try
                {
                    time.Reset(); time.Start();
                    p.Program();
                    time.Stop();
                    Console.WriteLine("Elapsed : " + time.ElapsedMilliseconds + " ms");
                    Console.WriteLine("Intermedia Code Generated : " + IntermediateCode.CurrentIO.ToString().Length + " Bytes\n\n");

                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception raised while parsering");
                    Console.WriteLine(e.Message);
                    return;
                }

                string filename = Path.GetFileNameWithoutExtension(filePath);
                string output = Path.Combine(baseDirectory, filename + "." + IntermediateCode.CurrentIO.FileExt());

                Console.WriteLine("Output to :" + output + "\n\n");
                File.WriteAllText(output, IntermediateCode.CurrentIO.ToString());
                Console.WriteLine("Done.");
            }
        }
    }
}
