using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trust_based_Imputation
{
    class CandidateItemSet
    {
        private int userNum = 0;
        private int candidateThreshold = 0;
        private NeighborsMatrix nm = null;
        private RatingMatrix rm = null;

        // Imputation 후보가 된 (u, i)pairs
		public List<int>[] sets = null;

        // key = 이웃평가횟수,
        // value = 그 이웃평가횟수가 몇번있었나
        // 이웃평가횟수: 특정 유저의 이웃들이 특정 아이템에 대해서 평가한 횟수
		AugmentingTable countTable;

    	double limit = 0;
		double remains = 0;
        int currentCount = 0;
        int oldCount = 0;
        int candidateBase = 0;  // Imputation 후보가 될 (u, i) pair가 넘어야 할 최소한의 rating 횟수

        public CandidateItemSet()
        {
            this.userNum = 0;
            this.rm = null;
            this.nm = null;
            this.sets = null;
        }

        public CandidateItemSet(NeighborsMatrix nm, 
                        RatingMatrix rm, int candidateThreshold)
        {
            this.nm = nm;
            this.rm = rm;
            this.userNum = nm.userNum;
            this.candidateThreshold = candidateThreshold;

			countTable = new AugmentingTable();

			sets = new List<int>[userNum];
			for(int i=0; i<userNum; ++i)
				sets[i] = new List<int>();

            FindCandidateLimit();
            FindCandidates();

            Console.WriteLine("4. Candidate Item Matrix is made.");
        }
			
        // key = 아이템 번호, value = 이웃들이 그 아이템을 평가한 횟수
        // 인 Key-Value pair CountTable 자료구조를 반환함.
		private AugmentingTable GetRatingCountTable (int userID)
        {
			AugmentingTable itemRatingCountTable = new AugmentingTable();
			foreach(int neighbor in nm.NeighborList(userID))	// for all neighbors n
			{
				Dictionary<int, int> dict = rm.GetScoreInfo(neighbor);
				foreach (int item in dict.Keys)		// for all items the neighbor has rated
					itemRatingCountTable.AugmentValue(item);
			}
			return itemRatingCountTable;
        }

		/* First taverse */
		private void FindCandidateLimit()
		{
			for (int u=1; u<=userNum; ++u)
			{
				AugmentingTable itemRatingCountTable = GetRatingCountTable(u);
				foreach (int ratingCount in itemRatingCountTable.Values)
					countTable.AugmentValue(ratingCount);
			}

			Console.WriteLine(DateTime.Now.ToString("O"));
			// sort the table by key(# of neighbors who has rated).
			List<KeyValuePair<int, double>> countTableList = countTable.ToList();
			countTableList.Sort((firstPair, nextPair) => 
				nextPair.Key.CompareTo (firstPair.Key)
			);

			double total = 0;
			foreach (int value in countTable.Values)
				total += value;
			limit = (int)(total * candidateThreshold / 100);
			Console.WriteLine("total: " + total);
			Console.WriteLine("limit: " + limit);

			foreach (KeyValuePair<int, double> pair in countTableList)
			{
				currentCount = pair.Key;
				if ((limit - pair.Value) > 0)
				{
					limit -= pair.Value;
					oldCount = pair.Key;
				}
				else break;
			}
			candidateBase = oldCount;
			remains = limit;

			Console.WriteLine("candidate base: " + candidateBase);
			Console.WriteLine("경계 남아 있는 숫자: " + remains);
			Console.WriteLine(DateTime.Now.ToString("O"));
		}
        /* Second Traverse */
        private void FindCandidates()
        {
            for (int u = 1; u <= userNum; u++)  // for all user u
            {
				AugmentingTable itemRatingCountTable = GetRatingCountTable(u);
				foreach (int key in itemRatingCountTable.Keys)
				{
					double value = itemRatingCountTable[key];
					if (value >= candidateBase)
						sets[u-1].Add(key);
					else if (value == currentCount && remains > 0)
					{
						sets[u-1].Add(key);
						--remains;
					}
				}
//				foreach (KeyValuePair<int, double> pair in ratingCountTableList[u-1])
//				{
//					double value = pair.Value;
//					if (value >= candidateBase)
//						sets[u-1].Add(pair.Key);
//					else if (value.Equals(currentCount) && remains > 0)
//					{
//						sets[u-1].Add(pair.Key);
//						-- remains;
//					}
//				}
            }
			Console.WriteLine(DateTime.Now.ToString("O"));
        }

        // 후보 기준에 만족하는 아이템 번호 집합을 반환함. 
        // 인자로 받는 사용자번호와 반환되는 아이템 번호 pair가 후보 기준을 만족하는 것.
		public List<int> CandidateItemForUser(int userID)
        {
			return sets[userID - 1];
        }
    }
}
