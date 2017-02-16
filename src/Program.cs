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

            if (args.Length != 1)
            {
                Console.WriteLine("\n\nNo param passed");
                Console.WriteLine("Call Param : sbf [file]");
                Console.ReadLine();
            }
            else
            {
                string filePath = args[0];
                Stopwatch time = new Stopwatch();
                LexicalAnalyzer la = new LexicalAnalyzer(baseDirectory);
                IntermediateCode inter = new IntermediateCode();

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

                Parser p = new Parser(la, inter);
                Console.WriteLine("Parsering...");
                //try
                //{
                    time.Reset(); time.Start();
                    p.Program();
                    time.Stop();
                    Console.WriteLine("Elapsed : " + time.ElapsedMilliseconds + " ms");
                    Console.WriteLine("Intermedia Code Generated : " + IntermediateCode.CurrentIO.ToString().Length + " Bytes\n\n");

                //}
                //catch (Exception e)
                //{
                //    Console.WriteLine("Exception raised while parsering");
                //    Console.WriteLine(e.Message);
                //    Console.ReadLine();
                //    return;
                //}

                string filename = Path.GetFileNameWithoutExtension(filePath);
                string output = Path.Combine(baseDirectory, filename + ".tsb");

                Console.WriteLine("Output to :" + output + "\n\n");
                File.WriteAllText(output, IntermediateCode.CurrentIO.ToString());
                Console.WriteLine("Done.");
                Console.WriteLine("Press enter to exit..");
                Console.ReadLine();
            }
        }
    }
}
