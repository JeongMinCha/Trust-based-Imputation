using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trust_based_Imputation
{
    class ALGraph
    {
        private int nodeNum;
//		private int[][] graph;      // array of list
		private List<int>[] graphs;

        public ALGraph(int nodeNum)
        {
            this.nodeNum = nodeNum;
            graphs = new List<int>[nodeNum];

            // allocate instances for each entries
            for (int i = 0; i < nodeNum; ++i)
				graphs[i] = new List<int>();
        }

        /* Insert the edge from 'fromV' to 'toV' */
        public void InsertEdge(int fromV, int toV)
        {
            graphs[fromV - 1].Add(toV);
        }

        /* Remove the edge from 'fromv' to 'toV' */
        public void RemoveEdge(int fromV, int toV)
        {
            List<int> list = graphs[fromV - 1];
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == toV)
                    list.Remove(toV);
            }
        }

        /* Returns list of nodes who is adjacent to the node 'fromV'. */
		public List<int> AdjNodes(int fromV)
        {
            return graphs[fromV - 1];
        }
    }
}
