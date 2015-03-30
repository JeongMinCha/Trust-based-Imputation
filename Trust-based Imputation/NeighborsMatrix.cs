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

        /* Base Constructor */
        public NeighborsMatrix()
        {
            this.distanceThreshold = 0;
            this.userNum = 0;
            this.direction = Globals.FORWARD;
			this.matrix = new List<BitArray>();
        }

        public NeighborsMatrix(int distanceThreshold,
                        int direction, TrustNetwork tn)
        {
            this.distanceThreshold = distanceThreshold;
            this.userNum = tn.userNum;
            this.direction = direction;

			matrix = new List<BitArray>();
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
            Console.WriteLine("3. Reliable Neighbors Matrix is made.");
        }

		public bool CheckNeighboorhood(int userID, int targetUserID)
		{
			return matrix[userID-1].Get(targetUserID-1);
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
    }
}
