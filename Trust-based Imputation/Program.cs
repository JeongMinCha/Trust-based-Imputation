using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trust_based_Imputation
{
    public static class GlobalVar
    {
        public const int FORWARD = 1;
        public const int BACKWARD = 2;
        public const int BIDIRECTED = 3;

        public const char delimiter = '\t';
    }

    class Program
    {
        static int ARGS_NUM = 6;
        static void Main(string[] args)
        {
            Console.WriteLine("현재시각: " + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"));
            DataSet dataSet = null;         // input data set
            string newFileName = null;      // output file name
            int direction = 0;              // direction of edges in trust network

            /* threshold arguments δ, θ */
            int candidateThreshold = 0;     // theta θ
            int distanceThreshold = 0;      // delta δ

            try 
            {
                if (args.Length != ARGS_NUM)
                    PrintUsage();
                else if (args[2] != null)
                {
                    direction = Convert.ToInt32(args[2]);
                    if (1 > direction && 3 < direction)
                        PrintUsage();
                }
                
                // 신뢰 네트워크나 평점 파일이 없다면, 예외 처리
                if(File.Exists(args[0]) == false || File.Exists(args[1]) == false)
                    throw new FileNotFoundException();

                // 인자 할당하기
                dataSet = new DataSet(args[0], args[1]);
                candidateThreshold = Convert.ToInt32(args[3]);
                distanceThreshold = Convert.ToInt32(args[4]);
                newFileName = String.Copy(args[5]);
            }
            catch (FormatException fe) {
                Console.WriteLine(fe.StackTrace);
                PrintUsage();
            }
            catch (FileNotFoundException fnfe) {
                Console.WriteLine(fnfe.StackTrace);
                FileNotFoundWarning();
            }

            Console.WriteLine("거리 threshold: {0}", distanceThreshold);
            Console.WriteLine("후보 threshold: {0}", candidateThreshold);
            if (direction == GlobalVar.FORWARD) Console.WriteLine("방향: Forward");
            if (direction == GlobalVar.BACKWARD) Console.WriteLine("방향: Backward");
            if (direction == GlobalVar.BIDIRECTED) Console.WriteLine("방향: bidirected");

            /* Making 4 main data structure for data imputation. */
            RatingMatrix rm = new RatingMatrix(dataSet);
            TrustNetwork tn = new TrustNetwork(dataSet);
            NeighborsMatrix nm = new NeighborsMatrix(distanceThreshold, direction, tn);
            CandidateItemSet cis = new CandidateItemSet(nm, rm, candidateThreshold);

            ///* Data Imputation */
            rm.MatrixImputation(nm, cis);
            rm.SaveToFile(newFileName);
            Console.WriteLine("Data Imputation Complete. \nThe imputed rating matrix file is made.");
            Console.WriteLine("현재시각: " + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"));
            Console.WriteLine();
        }

        /* Prints usage command */
        private static void PrintUsage()
        {
            Console.WriteLine("\nUsage: Trust-based Imputation [args]\n");
            Console.WriteLine(" -args[0]: input file name representing trust network");
            Console.WriteLine(" -args[1]: input file name representing rating matrix");
            Console.WriteLine(" -args[2]: direction of edges of trust network \n\t(Forward=1, Backward=2, Bidirected=3)");
            Console.WriteLine(" -args[3]: candidate threshold \n\t(theta, integer number, e.g. 10,20, ...)");
            Console.WriteLine(" -args[4]: distance threshold \n\t(delta, integer number, e.g. 1,2,3,...)");
            Console.WriteLine(" -args[5]: output file name to save imputated rating matrix\n");
            Environment.Exit(0);
        }

        private static void FileNotFoundWarning()
        {
            Console.WriteLine("\nThe input files are not found.\n");
        }
    }
}
