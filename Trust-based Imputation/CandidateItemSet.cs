using System;
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
        public UniqueList[] matrix = null;

        // key = 이웃평가횟수,
        // value = key의 수만큼 평가된 이웃평가횟수가 몇번있었나
        // 이웃평가횟수: 특정 유저의 이웃들이 특정 아이템에 대해서 평가한 횟수
        CountTable countTable;

    	double limit = 0;
        int currentCount = 0;
        int oldCount = 0;
        int candidateBase = 0;  // Imputation 후보가 될 (u, i) pair가 넘어야 할 최소한의 rating 횟수

        public CandidateItemSet()
        {
            this.userNum = 0;
             this.rm = null;
            this.nm = null;
            this.matrix = null;
        }

        public CandidateItemSet(NeighborsMatrix nm, 
                        RatingMatrix rm, int candidateThreshold)
        {
            this.nm = nm;
            this.rm = rm;
            this.userNum = nm.userNum;
            this.candidateThreshold = candidateThreshold;

            countTable = new CountTable();

            matrix = new UniqueList[userNum];
            for (int i = 0; i < userNum; ++i)
                matrix[i] = new UniqueList();

            FindCandidateLimit();
//            FindCandidates();

            Console.WriteLine("4. Candidate Item Matrix is made.");
        }

        /* First taverse */
        private void FindCandidateLimit()
        {
//            for (int u = 1; u <= userNum; ++u)
//            {
//                CountTable itemRatingCountTable = GetRatingCountTable(u);
//                foreach (int ratingCount in itemRatingCountTable.Values)
//                    countTable.AddCount(ratingCount);
//
//                itemRatingCountTable = null;
//            }
//            Console.WriteLine("key가 '이웃평가횟수', value가 그 이웃평가횟수 자체의 개수인 CountTable을 완성했습니다.");
//            Console.WriteLine("이웃평가횟수란 특정 사용자의 이웃들이 특정 아이템을 평가한 횟수를 의미합니다.");
//            Console.WriteLine("즉, value는 key의 수만큼의 이웃평가횟수가 나오는 user-item 페어의 수를 의미하는 것입니다.");
//
//            double total = 0;          // 모든 유저의 이웃들이 평가한 아이템의 총 수.
//
//            foreach (int value in countTable.Values)
//				total += value;
//            limit = (int)(total*candidateThreshold/100);
//
//			File.AppendAllText(@"./result.txt", "total: " + total + ", one-rating: " + countTable[1] + "\r\n");
//            Console.WriteLine("각 유저들의 이웃들이 평가한 아이템의 총 수: {0}", total);
//            Console.WriteLine("이들 중 \"{0}\"개가 Imputation의 후보입니다.", limit);

			/* warning space */
			for (int h = 1; h <= nm.distanceThreshold; ++h)
			{
				CountTable countTable = new CountTable();
				for (int u = 1; u <= userNum; ++u)
				{
					CountTable itemRatingCountTable = GetRatingCountTable(u, h);
					foreach(int ratingCount in itemRatingCountTable.Values)
						countTable.AddCount(ratingCount);
					itemRatingCountTable = null;
				}
				double total=0;
				foreach (int value in countTable.Values)
					total += value;
				File.AppendAllText(@"./result.txt", "hop: " + h + ", total: " + total + 
							", one-rating: " + countTable[1] + "\r\n");
				Console.WriteLine("각 유저들의 이웃들이 평가한 아이템의 총 수: {0}", total);
				Console.WriteLine("한 이웃이 평가한 아이템의 수: {0}", countTable[1]);

				countTable = null;
			}

//            foreach (int count in countTable.Keys)
//            {
//                currentCount = count;
//                if ((limit - countTable[count]) > 0)
//                {
//                    limit -= countTable[count];
//                    oldCount = count;
//                }
//                else
//                    break;
//            }
//            candidateBase = oldCount;
//            // 이 시점에서, limit은 경계 범위에서 남은 (u, i) pair의 수가 됨.
//
//            Console.WriteLine("Imputation 후보의 기준을 찾았습니다!");
//            Console.WriteLine("After iteration, limit: {0}", limit);
//            Console.WriteLine("The base of candidate : {0}", candidateBase);
        }

        // key = 아이템 번호, value = 이웃들이 그 아이템을 평가한 횟수
        // 인 Key-Value pair CountTable 자료구조를 반환함.
        private CountTable GetRatingCountTable (int userID)
        {
            CountTable itemRatingCountTable = new CountTable();
            UniqueList neighborList = nm.NeighborsFor(userID);
            foreach (int neighbor in neighborList)    // for all neighbors of user u
            {
                Dictionary<int, int> dict = rm.matrix[neighbor - 1];
                // add rating count of items rated by neighbor
                foreach (int item in dict.Keys)
                    itemRatingCountTable.AddCount(item);
            }
            return itemRatingCountTable;
        }

		private CountTable GetRatingCountTable (int userID, int hop)
		{
			CountTable itemRatingCountTable = new CountTable();
			UniqueList neighborList = nm.matrix[hop-1, userID-1];
			foreach (int neighbor in neighborList)    // for neighbors of user u at hop
			{
				Dictionary<int, int> dict = rm.matrix[neighbor - 1];
				// add rating count of items rated by neighbor
				foreach (int item in dict.Keys)
					itemRatingCountTable.AddCount(item);
			}
			return itemRatingCountTable;
		}

        /* Second Traverse */
        private void FindCandidates()
        {
            for (int u = 1; u <= userNum; u++)  // for all user u
            {
                CountTable itemRatingCountTable = GetRatingCountTable(u);
                foreach (int item in itemRatingCountTable.Keys)
                {
                    int value = itemRatingCountTable[item]; // how many the item is rated by neighbors
                    // 후보가 되는 이웃평가횟수 이상의 이웃평가횟수를 갖고 있는 경우 후보로 추가.
                    if (value >= candidateBase)
                        matrix[u - 1].Add(item);
                    // 경계에 걸친 이웃평가횟수에서 남은 아이템들을 후보로 추가하는 루틴...
                    else if (value == currentCount && limit > 0)
                    {
                        matrix[u - 1].Add(item);
                        limit--;
                    }
                }
                itemRatingCountTable = null;
            }
        }

        // 후보 기준에 만족하는 아이템 번호 집합을 반환함. 
        // 인자로 받는 사용자번호와 반환되는 아이템 번호 pair가 후보 기준을 만족하는 것.
        public UniqueList CandidateItemForUser(int userID)
        {
            // this list is already sorted by how many each items are rated by them.
            UniqueList itemList = matrix[userID - 1];
            return itemList;
        }
    }
}
