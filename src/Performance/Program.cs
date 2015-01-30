using System;
using System.Diagnostics;
using System.IO;

namespace Performance
{
    class Program
    {
        static void Main(string[] args)
        {
            int iterations = 500;
            if (args.Length >= 1)
            {
                Console.WriteLine("usage: {0} numerOfIterations",
                                  Path.GetFileName(typeof(Program).Assembly.ManifestModule.FullyQualifiedName));
                iterations = int.Parse(args[0]);
                
            }

            UseCase jit = new PlainUseCase();
            jit.Run();

            var loops = 10;
            // includes only fast containers http://www.palmmedia.de/blog/2011/8/30/ioc-container-benchmark-performance-comparison
            for (int i = 0; i < loops; i++)
            {
                Console.WriteLine("Test #{0}:", i);

                
                Console.WriteLine("Running {0} iterations for each use case.", iterations);

                int padding = 50;
                UseCase uc = new PlainUseCase();
                Console.WriteLine(Pad(padding, "Plain no-DI: {0}"), Measure(uc.Run, iterations));

                uc = new HiroUseCase();
                Console.WriteLine(Pad(padding, "Hiro: {0}"), Measure(uc.Run, iterations));

                uc = new FunqUseCase();
                Console.WriteLine(Pad(padding, "Funq: {0}"), Measure(uc.Run, iterations));

                uc = new DryIocUseCase();
                Console.WriteLine(Pad(padding, "DryIoc: {0}"), Measure(uc.Run, iterations));
                
                uc = new LightInjectUseCase();
                Console.WriteLine(Pad(padding, "LightInject: {0}"), Measure(uc.Run, iterations));

            }
            
            Console.ReadKey();
        }

        private static string Pad(int count, string value)
        {
            return value + new string(' ', count - value.Length);
        }

        private static long Measure(Action action, int iterations)
        {
            GC.Collect();
            var watch = Stopwatch.StartNew();

            for (int i = 0; i < iterations; i++)
            {
                action();
            }

            return watch.ElapsedTicks;
        }
    }
}
