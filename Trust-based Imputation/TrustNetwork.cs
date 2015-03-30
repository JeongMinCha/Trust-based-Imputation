using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trust_based_Imputation
{
    class TrustNetwork
    {
        public int userNum;
        public ALGraph forwardGraph;   // graph with edges of forward direction
        public ALGraph backwardGraph;  // graph with edges of backward direction

        public TrustNetwork()
        {
            this.userNum = 0;
            forwardGraph = null;
            backwardGraph = null;
        }

        public TrustNetwork(DataSet dataSet)
        {
            this.userNum = dataSet.userNum;

            forwardGraph = new ALGraph(userNum);
            backwardGraph = new ALGraph(userNum);

			ConstructGrpahs(dataSet.trustNetworkFile);
            Console.WriteLine("2. Trust Network Graph is made");
        }

		private void ConstructGrpahs (string fileName)
		{
			string[] lines = System.IO.File.ReadAllLines(fileName);
			foreach (string line in lines)
			{
				// words[0] = trustor, words[1] = trustee, words[2] = true or false
				string[] words = line.Split('\t');
				if (words[0] != null && words[1] != null)
				{
					int fromV = Convert.ToInt32(words[0]);
					int toV = Convert.ToInt32(words[1]);
					forwardGraph.InsertEdge(fromV, toV);    // edge 'trustor -> trustee'
					backwardGraph.InsertEdge(toV, fromV);   // edge 'trustee -> trustor'
				}
			}
		}


		public BitArray GetForwardAllNeighbors (int userID, int distanceThreshold)
		{
			if (distanceThreshold < 1)
				return null;
			
			BitArray neighborArray = new BitArray(userNum);
			HashSet<int> oldSet = null;
			for(int hop=1; hop<=distanceThreshold; ++hop)
			{
				HashSet<int> newSet = new HashSet<int>();
				if (hop == 1)
					newSet = oldSet = new HashSet<int>(forwardGraph.AdjNodes(userID));
				else
				{
					foreach (int fromV in oldSet)	// for all old neighbors
					{
						// find new neighbors from the old neighbors.
						ISet<int> iSet = new HashSet<int>(forwardGraph.AdjNodes(fromV));
						foreach (int newNeighbor in iSet)
							newSet.Add(newNeighbor);
					}
					oldSet = newSet;
				}
				foreach (int item in newSet)
					neighborArray.Set(item-1, true);
			}
			// One Person cannot be a his/her neighbor.
			neighborArray.Set(userID-1, false);
			return neighborArray;
		}

		// TODO: FIX IT
		public BitArray GetBackwardAllNeighbors (int userID, int distanceThreshold)
		{
			if (distanceThreshold < 1)
				return null;

			BitArray neighborArray = new BitArray(userNum);
			List<int> oldList = null;
			for(int hop=1; hop<=distanceThreshold; ++hop)
			{
				List<int> newList = new List<int>();
				if (hop == 1)
					newList = oldList = backwardGraph.AdjNodes(userID);
				else
				{
					foreach (int oldNeigbor in oldList)	// for all old neighbors
					{
						// find new neighbors from the old neighbors.
						IList<int> list = backwardGraph.AdjNodes(oldNeigbor);
						foreach (int newNeighbor in list)
							newList.Add(newNeighbor);
					}
					oldList = newList;
				}

				foreach (int item in newList)
					neighborArray.Set(item-1, true);
			}

			// One Person cannot be a his/her neighbor.
			neighborArray.Set(userID-1, false);
			return neighborArray;
		}

		// TODO: FIX IT
		public BitArray GetBidirectedAllNeighbors (int userID, int distanceThreshold)
		{
			if (distanceThreshold < 1)
				return null;

			BitArray neighborArray = new BitArray(userNum);
			List<int> oldList = null;
			for(int hop=1; hop<=distanceThreshold; ++hop)
			{
				List<int> newList = new List<int>();
				IList<int> forwardList;
				IList<int> backwardList;

				if (hop == 1)
				{
					forwardList = forwardGraph.AdjNodes(userID);
					backwardList = backwardGraph.AdjNodes(userID);
					oldList = newList = forwardList.Union(backwardList).ToList();
				}
				else
				{
					foreach (int oldNeighbor in oldList)	// for all old neighbors
					{
						forwardList = forwardGraph.AdjNodes(oldNeighbor);
						backwardList = backwardGraph.AdjNodes(oldNeighbor);
						// find new neighbors from the old neighbors.
						foreach (int newNeighbor in forwardList.Union(backwardList))
							newList.Add(newNeighbor);
					}
					oldList = newList;
				}

				foreach (int item in newList)
					neighborArray.Set(item-1, true);
			}

			// One Person cannot be a his/her neighbor.
			neighborArray.Set(userID-1, false);
			return neighborArray;
		}
    }
}
