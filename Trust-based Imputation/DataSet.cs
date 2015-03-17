using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trust_based_Imputation
{
    class DataSet
    {
        public string trustNetworkFile { get; private set; }
        public string ratingMatrixFile { get; private set; }

        public int itemNum;
        public int userNum;

        public DataSet()
        {
            this.itemNum = 0;
            this.userNum = 0;
            trustNetworkFile = null;
            ratingMatrixFile = null;
        }

        public DataSet(int itemNum, int userNum)
        {
            this.itemNum = itemNum;
            this.userNum = userNum;
            this.trustNetworkFile = null;
            this.ratingMatrixFile = null;
        }

        public DataSet(string trustNetworkFile, string ratingMatrixFile)
        {
            this.trustNetworkFile = trustNetworkFile;
            this.ratingMatrixFile = ratingMatrixFile;
            this.itemNum = MaxItemNum();
            this.userNum = MaxUserNum();
        }

        /* Returns max numbers of items.
         * Need to analyze only rating matrix file. */
        private int MaxItemNum()
        {
            int max = 0;
            /* Analyze the rating matrix file. */
            string[] lines = File.ReadAllLines(ratingMatrixFile);
            foreach(string line in lines)
            {
                // words[0] = user, words[1] = item, words[2] = rating score
                string[] words = line.Split(Globals.delimiter);
                try {
                    if (words[1] != null)
                    {
                        int cur = Convert.ToInt32(words[1]);
                        if (cur > max) max = cur;
                    }
                } catch (IndexOutOfRangeException e) {
                    Console.WriteLine(e.StackTrace);
                    Console.WriteLine("current statement: {0}", line);
                }
            }
            return max;
        }

        /* Returns max number of users.
         * Need to analyze both rating matrix and trust network file. */
        private int MaxUserNum()
        {
            int max = 0;
            /* Analyze the rating matrix file. */
            string[] lines = File.ReadAllLines(ratingMatrixFile);
            foreach (string line in lines)
            {
                // words[0] = user, words[1] = item, words[2] = rating score
                string[] words = line.Split(Globals.delimiter);
                try
                {
                    if (words[0] != null)
                    {
                        int cur = Convert.ToInt32(words[0]);
                        if (cur > max) max = cur;
                    }
                } catch (IndexOutOfRangeException e)
                {
                    Console.WriteLine(e.StackTrace);
                    Console.WriteLine("current statement: {0}", line);
                }
            }
            /* Analyze the trust network file. */
            lines = File.ReadAllLines(trustNetworkFile);
            foreach (string line in lines)
            {
                // words[0] = user, words[1] = user, words[2] = yes or no
                string[] words = line.Split(Globals.delimiter);
                try
                {
                    if (words[1] != null)
                    {
                        int cur = Convert.ToInt32(words[1]);
                        if (cur > max) max = cur;
                    }
                } catch (IndexOutOfRangeException e) {
                    Console.WriteLine(e.StackTrace);
                    Console.WriteLine("current statement: {0}", line);
                }
            }
            return max;
        }
    }
}
