using System;
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
        public UniqueList[,] matrix;

        /* Base Constructor */
        public NeighborsMatrix()
        {
            this.distanceThreshold = 0;
            this.userNum = 0;
            this.direction = Globals.FORWARD;
			this.matrix = null;
        }

        public NeighborsMatrix(int distanceThreshold,
                        int direction, TrustNetwork tn)
        {
            this.distanceThreshold = distanceThreshold;
            this.userNum = tn.userNum;
            this.direction = direction;

            matrix = new UniqueList[distanceThreshold, userNum];

            switch (direction)
            {
                case Globals.FORWARD:
                    for (int h = 0; h < distanceThreshold; ++h)
                    {
                        for (int u = 0; u < userNum; ++u)
                        {
                            matrix[h, u] = tn.GetTrusteeList(u + 1, h + 1);
                            for (int e = 0; e < h; e++)
                                excludeList(matrix[h, u], matrix[e, u]);
                        }
                    }
                    break;
                case Globals.BACKWARD:
                    for (int h = 0; h < distanceThreshold; ++h)
                    {
                        for (int u = 0; u < userNum; ++u)
                        {
                            matrix[h, u] = tn.GetTrustorList(u + 1, h + 1);
                            for (int e = 0; e < h; e++)
                                excludeList(matrix[h, u], matrix[e, u]);
                        }
                    }
                    break;
                case Globals.BIDIRECTED:
                    for (int h = 0; h < distanceThreshold; ++h)
                    {
                        for (int u = 0; u < userNum; ++u)
                        {
                            matrix[h, u] = tn.GetTrustorAndTrusteeList(u + 1, h + 1);
                            for (int e = 0; e < h; e++)
                                excludeList(matrix[h, u], matrix[e, u]);
                        }
                    }
                    break;
                default:
                    break;
            }
            Console.WriteLine("3. Reliable Neighbors Matrix is made.");
        }

        /* This method exclude elements in mainList which
         * are same as elements in exclusionList */
        private void excludeList(UniqueList mainList, UniqueList exclusionList)
        {
            foreach (int exElem in exclusionList)
                mainList.Remove(exElem);
        }

        /* Returns array of list, which each list has users' list at each hop. */
        public UniqueList[] HoppedNeighborsFor(int userID)
        {
            UniqueList[] list = new UniqueList[distanceThreshold];

            for (int hop = 1; hop <= distanceThreshold; hop++)
                list[hop - 1] = matrix[hop - 1, userID - 1];

            return list;
        }

        /* Returns a list of all neighbors */
        public UniqueList NeighborsFor(int userID)
        {
            UniqueList neighborList = new UniqueList();

            for (int hop = 1; hop <= distanceThreshold; hop++)
            {
                UniqueList curList = matrix[hop - 1, userID - 1];  // neighbor list at hop
                for (int i = 0; i < curList.Count; i++)
                    neighborList.Add(curList[i]);
            }

            return neighborList;
        }
    }
}
