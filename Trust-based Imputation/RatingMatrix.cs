using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trust_based_Imputation
{
    class RatingMatrix
    {
        public int userNum;        // row
        public int itemNum;        // column
        NeighborsMatrix nm;
        CandidateItemSets cim;
		String outputFile;
        private Dictionary<int, int>[] matrix;
		private double[] averageScores;

        public RatingMatrix()
        {
            this.userNum = 0;
            this.itemNum = 0;
            this.matrix = null;
			this.averageScores = null;
        }

		public RatingMatrix(DataSet dataSet)
        {
            this.userNum = dataSet.userNum;
            this.itemNum = dataSet.itemNum;

            matrix = new Dictionary<int, int>[userNum];
            for (int i = 0; i < userNum; i++)
                matrix[i] = new Dictionary<int, int>();

			averageScores = new double[userNum];

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

			for (int u=1; u<=userNum; ++u)
				averageScores[u-1] = AverageScoreFor(u);
			
            Console.WriteLine("1. Rating Matrix is made from the file.");
			Console.WriteLine("Time: " + DateTime.Now.ToString("O"));
        }

		public Dictionary<int, int> GetScoreInfo (int userID)
		{
			return matrix[userID-1];
		}

		public bool HasRating (int userID, int itemID)
		{
			return matrix[userID-1].ContainsKey(itemID);
		}

        /* Returns the rating score of the item ITEMID rated by the user USERID. 
         * Returns -1 if the score doesn't exist. */
        public int Score(int userID, int itemID)
        {
//			try {
//				return matrix[userID-1][itemID];
//			} catch (KeyNotFoundException) {
//				return -1;
//			} catch (ArgumentOutOfRangeException) {
//				return -1;
//			}
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
		public void MatrixImputation(NeighborsMatrix nm, CandidateItemSets cim, String outputFile)
        {
            Console.WriteLine("MatrixImputation() starts...");

			StreamWriter sw = new StreamWriter(outputFile);
			sw.Close();

            this.nm = nm;
            this.cim = cim;
			this.outputFile = outputFile;

			int sum=0;
            for (int u=1; u<=userNum; ++u)			// for all user u
			{
				List<int> neighborList = nm.NeighborList(u);
				sum += RowImputation(u, neighborList);
			}
			Console.WriteLine(sum);
            Console.WriteLine("MatrixImputation() ends...");
        }

        /* This method imputates a row of the rating matrix, which means the rating scores of
         * all items rated by neighbors of user USERID. */
		private int RowImputation(int userID, List<int> neighborList)
        {
			BitArray candidateItemArray = cim.CandidateItemBitArray(userID);

			int count = 0;
			int score=0;
			for (int itemID=1; itemID<=itemNum; ++itemID)
			{
				score = Score(userID, itemID);
				if (score != -1)
				{
					OutputFileWrite(userID, itemID, score);
					count++;
				}
				// For candidate items, predict the score and write it in output file.
				else if (candidateItemArray[itemID-1] == true)
				{
					double prediction = RatingScorePrediction(userID, itemID, neighborList);
					OutputFileWrite(userID, itemID, (int) prediction);
					count++;
				}
			}
			return count;
        }

		/* Write (userID, itemID, score) pair into the output file. */
		private void OutputFileWrite(int userID, int itemID, int score)
		{
			StringBuilder sb = new StringBuilder();
			sb.Clear();
			sb.Append((userID).ToString()).Append("\t");
			sb.Append((itemID).ToString()).Append("\t");
			sb.Append((score).ToString()).Append("\r\n");

			File.AppendAllText(outputFile, sb.ToString());
		}

        /* Returns the prediction of the rating score which the user USERID
         * would have rated the item ITEMID. */
		private double RatingScorePrediction(int userID, int itemID, List<int> neighborList)
        {
            int count = 0;
            double total = 0;
            double average = 0;
			int score = 0;

			foreach (int neighbor in neighborList)	// for all neighbors
			{
				if ((score = Score(neighbor, itemID)) != -1)
				{
					total += (score - averageScores[neighbor-1]);
					++count;
				}
			}
            if (count > 0)
				average = total / (double)count;
			average += averageScores[userID-1];

            return average;
        }
    }
}
