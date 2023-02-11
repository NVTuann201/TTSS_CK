using System;
using System.Collections.Generic;
using System.Text;
using Alchemi.Core;
using Alchemi.Core.Owner;


namespace PrimeNumber
{
    class PrimeNumber : GApplication
    {
        public static GApplication App = new GApplication();
        private static int NumPerThread;//Số lượng số trong 1 thread
        private static int NumThreads;//số luồng
        private static DateTime start;

        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("[Integral Calculator Grid Application]\n--------------------------------\n");
            Console.WriteLine("Press <enter> to start ...");
            Console.ReadLine();
            //input
            int n;
            Console.Write("Enter n: ");
            n = Int32.Parse(Console.ReadLine());
            Console.Write("Enter number per thread: ");
            NumPerThread = Int32.Parse(Console.ReadLine());
            Console.WriteLine(" ");

            //tính số luồng
            NumThreads = (Int32)Math.Floor((double)n / NumPerThread);
            if (NumPerThread * NumThreads < n)
            {
                NumThreads++;
            }

            //chia các đoạn vào các luồng, thêm vào lưới
            for (int i = 0; i < NumThreads; i++)
            {
                int StartDigitNum = 0 + (i * NumPerThread);
                int NumForThisThread = Math.Min(NumPerThread, n - i * NumPerThread);

                //Console.WriteLine("starting a thread to calculator integral from {0} to {1}",StartDigitNum,StartDigitNum + NumForThisThread);

                Integral thread = new Integral(
                    StartDigitNum,
                    NumForThisThread + StartDigitNum
                    );

                App.Threads.Add(thread);
            }

            //Khởi tạo lưới
            App.Connection = new GConnection("localhost", 9000, "user", "user");
            App.Manifest.Add(new ModuleDependency(typeof(Integral).Module));

            App.ThreadFinish += new GThreadFinish(App_ThreadFinish);
            App.ApplicationFinish += new GApplicationFinish(App_ApplicationFinish);

            //Thực thi
            start = DateTime.Now;
            App.Start();

            Console.ReadLine();
        }

        private static void App_ThreadFinish(GThread thread)
        {
            //   Console.WriteLine("Grid thread # {0} finished executing", thread.Id);
        }

        private static void App_ApplicationFinish()
        {
            double I = 0;
            for (int i = 0; i < App.Threads.Count; i++)
            {
                Integral pcgt = (Integral)App.Threads[i];
                I += pcgt.result();
            }
            Console.WriteLine($"\n===\nResult: {I}");
            Console.WriteLine("\n===\nTotal time:\t {0} seconds.", DateTime.Now - start);
        }
    }

    [Serializable]
    class Integral : GThread
    {
        public double x1, x2;
        public double I;
        public int n = 50;//n theo công thức simpson


        public Integral(int x1, int x2)
        {
            this.x1 = x1;
            this.x2 = x2;
        }
        //hàm f(x)
        public double f(double x)
        {
            return x * x + 1;
        }
        public override void Start()
        {
            double h = (x2 - x1) / (2 * n); //2n=10
            for (int i = 0; i <= 2 * n; i++)
            {
                I += (i == 0 || i == 2 * n) ? f((double)x1 + i * h) : (((i % 2) == 1) ? (4 * f((double)x1 + i * h)) : (2 * f((double)x1 + i * h)));
            }
            I = I * h / 3;
        }

        public double result()
        {
            return I;
        }
    }


}
