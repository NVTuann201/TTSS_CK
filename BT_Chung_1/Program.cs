using System;
using System.Collections.Generic;
using System.Text;
using Alchemi.Core;
using Alchemi.Core.Owner;

namespace ListPerfectSquare
{
    class ListPerfectSquare : GApplication
    {
        public static GApplication App = new GApplication();
        private static int NumberOfThread;
        private static DateTime start;
        private static List<int> ListPerfectSquareNumber = new List<int>();

        [STAThread]
        static void Main(string[] args)
        {
            int n;
            int firstNumber = 1;
            int lastNumber;
            int numberOfElement;
            string host;
            Console.Write("Host[localhost]: ");
            host = Console.ReadLine();
            if (host.Length < 1)
                host = "localhost";
            Console.Write("Enter the natural number n: ");
            n = Convert.ToInt32(Console.ReadLine());
            Console.Write("Enter the number of threads: ");
            NumberOfThread = Convert.ToInt32(Console.ReadLine());

            numberOfElement = Convert.ToInt32(n / NumberOfThread);

            for (int i = 0; i < NumberOfThread - 1; i++)
            {
                lastNumber = firstNumber + numberOfElement;
                App.Threads.Add(new PerfectSquareNumberCheck(firstNumber, lastNumber - 1));
                firstNumber = lastNumber;
            }
            App.Threads.Add(new PerfectSquareNumberCheck(firstNumber, n));

            //------------------------------------------------------------------------
            App.Connection = new GConnection(host, 9000, "user", "user");
            App.Manifest.Add(new ModuleDependency(typeof(PerfectSquareNumberCheck).Module));
            App.ThreadFinish += new GThreadFinish(App_ThreadFinish);
            App.ApplicationFinish += new GApplicationFinish(App_ApplicationFinish);
            start = DateTime.Now;
            Console.WriteLine("Thread started!");
            Console.WriteLine("\n-------------------------------------------------------");
            App.Start();
            Console.ReadLine();
        }
        private static void App_ThreadFinish(GThread thread)
        {
            PerfectSquareNumberCheck PSNC = (PerfectSquareNumberCheck)thread;
            Console.WriteLine("\nThe numbers from {0} to {1} have been checked.", PSNC.firstNumber, PSNC.lastNumber);
            Console.Write("The perfect squares are: ");
            for (int i = 0; i < PSNC.ListPerfectSquare.Count; i++)
            {
                Console.Write(PSNC.ListPerfectSquare[i] + ", ");
                ListPerfectSquareNumber.Add(PSNC.ListPerfectSquare[i]);
            }
            Console.WriteLine();
        }
        private static void App_ApplicationFinish()
        {
            Console.WriteLine("\n-------------------------------------------------------\n");
            Console.Write("The list of perfect squares is: ");
            for (int i = 0; i < ListPerfectSquareNumber.Count; i++)
                Console.Write(ListPerfectSquareNumber[i] + ", ");

            Console.WriteLine("\n\n-------------------------------------------------------");
            Console.WriteLine("\nThe program execution time is (seconds): {0}", DateTime.Now - start);
        }
    }

    //Class that checks if a natural number is a perfect square
    [Serializable]
    class PerfectSquareNumberCheck : GThread
    {
        public int firstNumber;
        public int lastNumber;
        public List<int> ListPerfectSquare = new List<int>();
        public PerfectSquareNumberCheck(int firstNumber, int lastNumber)
        {
            this.firstNumber = firstNumber;
            this.lastNumber = lastNumber;
        }
        public override void Start()
        {
            int temp;
            for (int i = firstNumber; i <= lastNumber; i++)
            {
                temp = Convert.ToInt32(Math.Sqrt(i));
                if (temp * temp == i)
                    ListPerfectSquare.Add(i);
            }
        }
    }
}