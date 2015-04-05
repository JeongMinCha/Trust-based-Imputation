using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trust_based_Imputation
{
    class NeighborsMatrix
    {
        private int direction = 0;
        public int distanceThreshold;
        public int userNum;
		public List<BitArray> matrix;
		RatingMatrix rm;

        /* Base Constructor */
        public NeighborsMatrix()
        {
            this.distanceThreshold = 0;
            this.userNum = 0;
            this.direction = Globals.FORWARD;
			this.matrix = new List<BitArray>();
        }

        public NeighborsMatrix(int distanceThreshold,
			int direction, TrustNetwork tn, RatingMatrix rm)
        {
            this.distanceThreshold = distanceThreshold;
            this.userNum = tn.userNum;
            this.direction = direction;
			this.rm = rm;

			matrix = new List<BitArray>(userNum);
			for(int i=0; i<userNum; ++i)
				matrix.Add(new BitArray(userNum));

            switch (direction)
            {
                case Globals.FORWARD:
				for (int user=1; user<=userNum; ++user)
				{
					BitArray neighborArr = tn.GetForwardAllNeighbors(user, distanceThreshold);
					matrix[user-1] = neighborArr;
				}
				break;

                case Globals.BACKWARD:
				for (int user=1; user<=userNum; ++user)
				{
					BitArray neighborArr = tn.GetBackwardAllNeighbors(user, distanceThreshold);
					matrix[user-1] = neighborArr;
				}
				break;

                case Globals.BIDIRECTED:
				for (int user=1; user<=userNum; ++user)
				{
					BitArray neighborArr = tn.GetBidirectedAllNeighbors(user, distanceThreshold);
					matrix[user-1] = neighborArr;
				}
				break;

                default:
                    break;
            }
		
			long totalNeighborCount = 0;
			for(int u=1; u<=userNum; ++u)
			{
				totalNeighborCount += NeighborCount(u);
			}
			Console.WriteLine("Total neighbors count: " + totalNeighborCount);
			Console.WriteLine("Average neighbors count: " + (totalNeighborCount/(double)userNum).ToString("N2"));

            Console.WriteLine("3. Reliable Neighbors Matrix is made.");
			Console.WriteLine("Time: " + DateTime.Now.ToString("O"));
			GC.Collect();
        }

		/* Returns # of the user's neighbors who rated the item. */
		public int NeighborRatingCount (int userID, int itemID)
		{
			int count=0;
			BitArray neighborArr = NeighborArray(userID);
			for(int neighbor=1; neighbor<=userNum; ++neighbor)
			{
				if (neighborArr[neighbor-1].Equals(true))	// for all neighbors
				{
					if (rm.HasRating(neighbor, itemID))
						++ count;
				}
			}
			return count;
		}


		/* Returns the bit array including neighbors of 'userID' user.*/
		public BitArray NeighborArray (int userID)
		{
			return matrix[userID-1];	
		}

		/* Returns the list including neighbors of 'userID' user.*/
		public List<int> NeighborList (int userID)
		{
			List<int> neighborList = new List<int>();
			BitArray neighborArray = NeighborArray(userID);
			for (int u=1; u<=userNum; ++u)
			{
				if (neighborArray[u-1].Equals(true))
					neighborList.Add(u);
			}
			return neighborList;
		}

		private int NeighborCount (int userID)
		{
			int count = 0;
			BitArray neighborArray = NeighborArray(userID);
			for(int i=0; i<userNum; ++i)
			{
				if (neighborArray[i].Equals(true))
					++ count;
			}
			return count;
		}
    }
}