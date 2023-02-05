using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Alchemi.Core;
using Alchemi.Core.Owner;

namespace MultiplyMatrixFile
{
    class Mtrix_File : GApplication
    {
        public static GApplication App = new GApplication();
        private static int NumberOfThread;
        private static DateTime start;
        private static List<char> matrixResult = new List<char>();
        private static int count = 0;
        private static int n;

        [STAThread]
        static void Main(string[] args)
        {
            int p, temp;
            string[] data;
            List<int> matrixA = new List<int>();
            List<int> matrixB = new List<int>();
            List<int> matrix1 = new List<int>();
            string fileInput = @"input.txt";
            if (System.IO.File.Exists(fileInput))
            {
                data = System.IO.File.ReadAllLines(fileInput);
                string host;
                Console.Write("Host[localhost]: ");
                host = Console.ReadLine();
                if (host.Length < 1)
                    host = "localhost";
                Console.Write("Enter the number of threads between 1 and {0}: ", data.Length);
                NumberOfThread = Convert.ToInt32(Console.ReadLine());
                if (NumberOfThread < 1 | NumberOfThread > data.Length)
                {
                    Console.WriteLine("The number of threads entered is not satisfied !!");
                    return;
                }

                #region Data pre-processing 
                n = data[0].Length;
                for (int i = 1; i < data.Length; i++)
                    if (data[i].Length > n)
                        n = data[i].Length;
                for (int i = 0; i < data.Length; i++)
                {
                    for (int j = 0; j < data[i].Length; j++)
                        matrixA.Add(Convert.ToInt32(data[i][j]));
                    if (data[i].Length < n)
                        for (int j = data[i].Length; j < n; j++)
                            matrixA.Add(0);
                }
                for (int j = 0; j < n * n; j++)
                    matrixB.Add(1);
                #endregion

                #region Split data into streams according to selected number of streams
                p = data.Length / NumberOfThread;
                temp = 0;
                for (int i = 0; i < NumberOfThread - 1; i++)
                {
                    matrix1 = matrixA.GetRange(temp, n * p);
                    App.Threads.Add(new MultiplyMatrix(matrix1, matrixB, n));
                    temp += n * p;
                }
                matrix1 = matrixA.GetRange(temp, n * data.Length - temp);
                App.Threads.Add(new MultiplyMatrix(matrix1, matrixB, n));
                #endregion

                //------------------------------------------------------------------------
                App.Connection = new GConnection(host, 9000, "user", "user");
                App.Manifest.Add(new ModuleDependency(typeof(MultiplyMatrix).Module));
                App.ThreadFinish += new GThreadFinish(App_ThreadFinish);
                App.ApplicationFinish += new GApplicationFinish(App_ApplicationFinish);
                start = DateTime.Now;
                Console.WriteLine("\nThread started!");
                Console.WriteLine("\n---------------------------------------------------");
                App.Start();
                Console.ReadLine();
            }
            else
            {
                Console.WriteLine("File does not exist");
            }
        }
        private static void App_ThreadFinish(GThread thread)
        {
            MultiplyMatrix MM = (MultiplyMatrix)thread;
            Console.WriteLine("\n{0}th thread completed.", count);
            count++;

            //Normalize data to acii normal form for display
            for (int i = 0; i < MM.matrixResultThread.Count; i++)
                matrixResult.Add(Convert.ToChar(MM.matrixResultThread[i] % 75 + 48));
        }
        private static void App_ApplicationFinish()
        {
            StreamWriter sWrite = new StreamWriter("output.txt");
            for (int i = 0; i < matrixResult.Count; i++)
            {
                if (i % n == 0 && i != 0)
                    sWrite.WriteLine();
                sWrite.Write(matrixResult[i]);
            }
            sWrite.Flush();
            Console.WriteLine("\n-----------------------------------------------------------------");
            Console.WriteLine("\nThe program execution time is (seconds): {0}", DateTime.Now - start);
        }
    }

    [Serializable]
    class MultiplyMatrix : GThread
    {
        public int n;
        public List<int> matrix1 = new List<int>();
        public List<int> matrix2 = new List<int>();
        public List<int> matrixResultThread = new List<int>();
        public MultiplyMatrix(List<int> matrix1, List<int> matrix2, int n)
        {
            this.matrix1 = matrix1;
            this.matrix2 = matrix2;
            this.n = n;
        }
        public override void Start()
        {
            int p, temp;
            p = matrix1.Count / n;
            for (int i = 0; i < p; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    temp = 0;
                    for (int h = 0; h < n; h++)
                        temp += matrix1[i * n + h] * matrix2[h * n + j];
                    matrixResultThread.Add(temp);
                }
            }
        }
    }
}