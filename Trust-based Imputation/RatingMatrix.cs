using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trust_based_Imputation
{
    class RatingMatrix
    {
        int userNum;        // row
        int itemNum;        // column
        NeighborsMatrix nm;
        CandidateItemSet cim;
        public Dictionary<int, int>[] matrix;

        public RatingMatrix()
        {
            this.userNum = 0;
            this.itemNum = 0;
            this.matrix = null;
        }

        public RatingMatrix(DataSet dataSet)
        {
            this.userNum = dataSet.userNum;
            this.itemNum = dataSet.itemNum;

            matrix = new Dictionary<int, int>[userNum];
            for (int i = 0; i < userNum; i++)
                matrix[i] = new Dictionary<int, int>();

            string[] lines = File.ReadAllLines(dataSet.ratingMatrixFile);
            foreach (string line in lines)
            {
                // words[0] = user, words[1] = item, words[2] = rating score
                string[] words = line.Split('\t');
                if (words[0] != null && words[1] != null && words[2] != null)
                {
                    int user = Convert.ToInt32(words[0]);
                    int item = Convert.ToInt32(words[1]);
                    int score = Convert.ToInt32(words[2]);
                    Insert(user, item, score);
                }
            }
            Console.WriteLine("1. Rating Matrix is made from the file.");
        }

        /* Returns the rating score of the item ITEMID rated by the user USERID. 
         * Returns -1 if the score doesn't exist. */
        public int Score(int userID, int itemID)
        {
            Dictionary<int, int> dict = null;
            if (matrix[userID - 1] != null)
                dict = matrix[userID - 1];

            if (dict != null && dict.ContainsKey(itemID))
                return (int)dict[itemID];
            else
                return -1;
         }

        /* Inserts the rating of the item ITEMID rated by the user USERID */
        public void Insert(int userID, int itemID, int score)
        {
            if (matrix[userID - 1] != null &&
                matrix[userID - 1].ContainsKey(itemID) == false)
            {
                matrix[userID - 1].Add(itemID, score);
            }
        }

        /* Returns the average value of rating scores which the user USERID rated items at. 
         * Returns -1 if the user USERID has rated no item. */
        public double AverageScoreFor(int userID)
        {
            Dictionary<int, int> dict = matrix[userID - 1];
            double sum = 0;
            double count = 0;
            double average = 0;

            foreach (int itemID in dict.Keys)
            {
                sum = sum + Convert.ToDouble(dict[itemID]);
                count++;
            }
            if (count > 0)
                average = sum / count;
            else
                average = -1;

            return average;
        }

        /* This method imputates the rating marix */
        public void MatrixImputation(NeighborsMatrix nm, CandidateItemSet cim)
        {
            Console.WriteLine("MatrixImputation() starts...");

            this.nm = nm;
            this.cim = cim;

            for (int u=1; u<=userNum; ++u)
                RowImputation(u);

            Console.WriteLine("MatrixImputation() ends...");

        }

        /* This method imputates a row of the rating matrix, which means the rating scores of
         * all items rated by neighbors of user USERID. */
        private void RowImputation(int userID)
        {
            UniqueList candidateItemList = cim.CandidateItemForUser(userID);
            foreach (int itemID in candidateItemList)
            {
                if (Score(userID, itemID) == -1)
                {
                    double prediction = RatingScorePrediction(userID, itemID);
                    Insert(userID, itemID, (int)prediction);
                }
            }
        }

        /* Returns the prediction of the rating score which the user USERID
         * would have rated the item ITEMID. */
        private double RatingScorePrediction(int userID, int itemID)
        {
            int count = 0;      // count of ratings of neighbors
            double total = 0;
            double average = 0;
            UniqueList neighborList = nm.NeighborsFor(userID);

            for(int v = 0; v < neighborList.Count; ++v)
            {
                double score = 0;
                int neighbor = neighborList[v];

                if ((score = Score(neighbor, itemID)) != -1)
                {
                    total += (score - AverageScoreFor(neighbor));
                    ++count;
                }
            }

            if (count > 0)
                average = total / count;
            average += AverageScoreFor(userID);

            return average;
        }

        /* The data in rating matrix will be saved into the file. */
        public void SaveToFile(string newFileName)
        {
            Console.WriteLine("SaveToFile() starts... ");
            string line = null;
            string[] token = new string[3]; // token[0]: user, token[1]: item, token[2]: score
            StringBuilder sb = new StringBuilder();

            for (int user = 1; user <= userNum; user++)
            {
                token[0] = (user).ToString();

                Dictionary<int, int> dict = matrix[user - 1];  // key = item, value = rating score.
                foreach (int item in dict.Keys)
                {
                    sb.Clear();
                    // token[0]: user number
                    sb.Append(token[0]).Append("\t");

                    // token[1]: item number 
                    token[1] = (item).ToString();
                    sb.Append(token[1]).Append("\t");

                    // token[2]: rating score
                    token[2] = ((int)dict[item]).ToString();
                    sb.Append(token[2]).Append("\n");

                    line = sb.ToString();
                    System.IO.File.AppendAllText(newFileName, line);
                }
            }
            Console.WriteLine("SaveToFile() ends... ");
        }
    }
}
