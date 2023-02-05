using System;
using System.Collections.Generic;
using System.Text;
using Alchemi.Core;
using Alchemi.Core.Owner;

namespace IntegralValue
{
    class IntegralValue : GApplication
    {
        public static GApplication App = new GApplication();
        private static DateTime start;

        private static long NumberOfThread;
        private static long NumberOfElementThreads;
        private static long NumberOfLoop;
        private static double integralResult = 0;

        public static double Function(double x)
            => Math.Pow(x, 2) + 1;

        [STAThread]
        static void Main(string[] args)
        {
            double n;
            double lengthOfSubdivisons;
            long firstNumber = 0;
            long lastNumber;
            string host;
            Console.Write("Host[localhost]: ");
            host = Console.ReadLine();
            if (host.Length < 1)
                host = "localhost";
            Console.Write("Enter the upper bround n = ");
            n = Convert.ToDouble(Console.ReadLine());
            Console.Write("Enter the number of subdivisions: ");
            NumberOfLoop = Convert.ToInt64(Console.ReadLine());
            Console.Write("Enter the number of threads: ");
            NumberOfThread = Convert.ToInt64(Console.ReadLine());

            lengthOfSubdivisons = n / NumberOfLoop;
            NumberOfElementThreads = Convert.ToInt64(NumberOfLoop / NumberOfThread);

            for (long i = 0; i < NumberOfThread - 1; i++)
            {
                lastNumber = firstNumber + NumberOfElementThreads;
                App.Threads.Add(new CalculateIntegral(firstNumber, lastNumber - 1, lengthOfSubdivisons));
                firstNumber = lastNumber;
            }
            App.Threads.Add(new CalculateIntegral(firstNumber, NumberOfLoop, lengthOfSubdivisons));

            //------------------------------------------------------------------------
            App.Connection = new GConnection(host, 9000, "user", "user");
            App.Manifest.Add(new ModuleDependency(typeof(CalculateIntegral).Module));
            App.ThreadFinish += new GThreadFinish(App_ThreadFinish);
            App.ApplicationFinish += new GApplicationFinish(App_ApplicationFinish);
            start = DateTime.Now;
            Console.WriteLine("Thread started!");
            Console.WriteLine("\n------------------------------------------------------------\n");
            App.Start();
            Console.ReadLine();
        }

        private static void App_ThreadFinish(GThread thread)
        {
            CalculateIntegral CI = (CalculateIntegral)thread;
            Console.WriteLine("The division from {0} to {1} has been calculated.", CI.firstNumber * CI.lengthOfSubdivisons, CI.lastNumber * CI.lengthOfSubdivisons);
            Console.WriteLine("The value on the division is: {0}", CI.result);
            integralResult += CI.result;
            Console.WriteLine();
        }
        private static void App_ApplicationFinish()
        {
            Console.WriteLine("\n------------------------------------------------------------");
            Console.WriteLine("\nIntegral value is: {0}", integralResult);
            Console.WriteLine("\nThe program execution time is (seconds): {0}", DateTime.Now - start);
        }
    }

    [Serializable]
    class CalculateIntegral : GThread
    {
        public long firstNumber;
        public long lastNumber;
        public double lengthOfSubdivisons;
        public double result;
        public CalculateIntegral(long FirstNumber, long LastNumber, double lengthOfSubdivisons)
        {
            this.firstNumber = FirstNumber;
            this.lastNumber = LastNumber;
            this.lengthOfSubdivisons = lengthOfSubdivisons;
        }
        public override void Start()
        {
            result = 0;
            for (long i = firstNumber; i <= lastNumber; i++)
                result += IntegralValue.Function(i * lengthOfSubdivisons);
            result = result * lengthOfSubdivisons;
        }
    }
}