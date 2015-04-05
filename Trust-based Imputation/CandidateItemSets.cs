using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trust_based_Imputation
{
    class CandidateItemSets
    {
        private int userNum = 0;
		private int itemNum = 0;
        private int candidateThreshold = 0;
        private NeighborsMatrix nm = null;
        private RatingMatrix rm = null;

        // Imputation 후보가 된 (u, i)pairs
		public List<BitArray> sets = null;
//		public List<int>[] sets = null;

        // key = 이웃평가횟수,
        // value = 그 이웃평가횟수가 몇번있었나
        // 이웃평가횟수: 특정 유저의 이웃들이 특정 아이템에 대해서 평가한 횟수
		AugmentingTable countTable;

		long limit = 0;
		long remains = 0;
        long currentCount = 0;
        long oldCount = 0;
        long candidateBase = 0;  // Imputation 후보가 될 (u, i) pair가 넘어야 할 최소한의 rating 횟수

        public CandidateItemSets()
        {
            this.userNum = 0;
            this.rm = null;
            this.nm = null;
			this.sets = null;
        }

        public CandidateItemSets(NeighborsMatrix nm, 
                        RatingMatrix rm, int candidateThreshold)
        {
            this.nm = nm;
            this.rm = rm;
            userNum = nm.userNum;
			itemNum = rm.itemNum;
            this.candidateThreshold = candidateThreshold;

			countTable = new AugmentingTable();

			sets = new List<BitArray>(userNum);
			for (int i=0; i<userNum; ++i)
				sets.Add(new BitArray(itemNum));

            FindCandidateLimit();
            FindCandidates();

            Console.WriteLine("4. Candidate Item Matrix is made.");
			Console.WriteLine("Time: " + DateTime.Now.ToString("O"));
        }

        // key = 아이템 번호, value = 이웃들이 그 아이템을 평가한 횟수
        // 인 Key-Value pair 자료구조를 반환함.
		private Dictionary<int, long> GetRatingCountTable (int userID)
        {
			Dictionary<int, long> itemRatingCountTable = new Dictionary<int, long>();
			foreach(int neighbor in nm.NeighborList(userID))	// for all neighbors n
			{
				Dictionary<int, int> dict = rm.GetScoreInfo(neighbor);
				foreach (int item in dict.Keys)		// for all items the neighbor has rated
				{
					if (itemRatingCountTable.ContainsKey(item))
						itemRatingCountTable[item] = itemRatingCountTable[item] + 1;
					else
						itemRatingCountTable.Add(item, 1);
				}
			}
			return itemRatingCountTable;
        }

		/* First taverse */
		private void FindCandidateLimit()
		{
			for (int u=1; u<=userNum; ++u)
			{
				Dictionary<int, long> itemRatingCountTable = GetRatingCountTable(u);
				foreach (long ratingCount in itemRatingCountTable.Values)
					countTable.AugmentValue(ratingCount);
			}
			GC.Collect();

			// sort the table by key(# of neighbors who has rated).
			List<KeyValuePair<long, long>> countTableList = countTable.ToList();
			countTableList.Sort((firstPair, nextPair) => 
				nextPair.Key.CompareTo (firstPair.Key)
			);

			long total = 0;
			foreach (long value in countTable.Values)
				total += value;
			limit = (int)(total * candidateThreshold / 100);
			Console.WriteLine("total: " + total);
			Console.WriteLine("limit: " + limit);

			foreach (KeyValuePair<long, long> pair in countTableList)
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
			Console.WriteLine("After first traverse, Time: " + DateTime.Now.ToString("O"));
		}
        /* Second Traverse */
        private void FindCandidates()
        {
            for (int u = 1; u <= userNum; u++)  // for all user u
            {
				Dictionary<int, long> itemRatingCountTable = GetRatingCountTable(u);
				foreach (int item in itemRatingCountTable.Keys)
				{
					long value = itemRatingCountTable[item];
					if (value >= candidateBase)
						sets[u-1].Set(item-1, true);
					else if (value == currentCount && remains > 0)
					{
						sets[u-1].Set(item-1, true);
						--remains;
					}
				}
            }
			GC.Collect();
			Console.WriteLine("After second traverse, Time: " + DateTime.Now.ToString("O"));
        }

        // 후보 기준에 만족하는 아이템 번호 집합을 반환함. 
        // 인자로 받는 사용자번호와 반환되는 아이템 번호 pair가 후보 기준을 만족하는 것.
		public BitArray CandidateItemBitArray(int userID)
        {
			return sets[userID - 1];
        }
    }
}
