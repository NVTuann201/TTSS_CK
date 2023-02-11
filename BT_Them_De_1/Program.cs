using System;
using System.Collections.Generic;
using System.Text;
using Alchemi.Core;
using Alchemi.Core.Owner;
namespace eNumber
{
    class eNumber : GApplication
    {
        public static GApplication App = new GApplication();
        private static int n;
        private static int NumPerThread;//Số lượng số trong 1 thread
        private static int NumThreads;//số luồng
        private static DateTime start;

        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("[e Calculator Grid Application]\n------------------------------- -\n");
           
            Console.WriteLine("Press <enter> to start ...");
            Console.ReadLine();
            //input
            int n;
            Console.Write("Enter n digits to calculate: n = ");
            n = Int32.Parse(Console.ReadLine());
            Console.Write("Enter number per thread: ");
            NumPerThread = Int32.Parse(Console.ReadLine());

            //tính số luồng
            NumThreads = (Int32)Math.Floor((double)n / NumPerThread);
            if (NumPerThread * NumThreads < n)
            {
                NumThreads++;
            }
            Console.WriteLine("\n ");
            //chia thành các đoạn thêm vào lưới
            for (int i = 0; i < NumThreads; i++)
            {
                int StartDigitNum = 1 + (i * NumPerThread);
                int NumForThisThread = Math.Min(NumPerThread, n - i *
                NumPerThread);
                //Console.WriteLine("starting a thread to calculate the digits of e from { 0}to { 1}",StartDigitNum,StartDigitNum + NumForThisThread-1);
           

                E thread = new E(StartDigitNum, NumForThisThread);
                App.Threads.Add(thread);
            }
            //Khởi tạo
            App.Connection = new GConnection("localhost", 9000, "user", "user");
            App.Manifest.Add(new ModuleDependency(typeof(E).Module));
            App.ThreadFinish += new GThreadFinish(App_ThreadFinish);
            App.ApplicationFinish += new GApplicationFinish(App_ApplicationFinish);
            //Thực thi
            start = DateTime.Now;
            App.Start();
            Console.ReadLine();
        }
        private static void App_ThreadFinish(GThread thread)
        {
            // Console.WriteLine("Grid thread # {0} finished executing", thread.Id);
        }
        private static void App_ApplicationFinish()
        {
            //nối chuỗi tạo thành chuỗi các số sau dấu thập phân
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < App.Threads.Count; i++)
            {
                E pcgt = (E)App.Threads[i];
                result.Append(pcgt.Result());
            }
            Console.WriteLine("\n===\nResult: 2.{0}", result);
            Console.WriteLine("\n===\nTotal time:\t {0} seconds.", DateTime.Now - start);
        }
    }
    [Serializable]
    class E : GThread
    {
        public int StartNum, NumForThisThread;
        public int[] Nums;
        public E(int StarNum, int NumForThisThread)
        {
            this.StartNum = StarNum;
            this.NumForThisThread = NumForThisThread;
        }
        public override void Start()
        {

            Nums = new int[NumForThisThread];
            for (int i = 0; i < NumForThisThread; i++)
            {
                //tìm số thứ n sau dấu thập phân 
                int n = StartNum + i;
                int N = n + 3;
                int q = 2;
                int[] e = new int[N];
                for (int j = 0; j < N; j++)
                {
                    e[j] = 1;
                }
                for (int j = 0; j < n; j++)
                {
                    for (int k = 0; k < N; k++)
                    {
                        int temp = q;
                        q = (int)((10 * e[k] + q) / (N - k + 1));
                        e[k] = (10 * e[k] + temp) % (N - k + 1);
                    }
                }
                Nums[i] = q;
            }
        }
        public String Result()
        {
            //tạo chuỗi gồm NumForThisThread số sau dấu thập phân từ số thứ StartNum
            StringBuilder temp = new StringBuilder();
            for (int i = 0; i < NumForThisThread; i++)
            {
                temp.Append(Nums[i]);
            }
            return temp.ToString();
        }
    }


}
