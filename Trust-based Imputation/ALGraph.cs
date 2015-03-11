using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trust_based_Imputation
{
    class ALGraph
    {
        private int nodeNum;
        private UniqueList[] graph;      // array of list

        public ALGraph(int nodeNum)
        {
            this.nodeNum = nodeNum;
            graph = new UniqueList[nodeNum];

            // allocate instances for each entries
            for (int i = 0; i < nodeNum; ++i)
                graph[i] = new UniqueList();
        }

        /* Insert the edge from 'fromV' to 'toV' */
        public void InsertEdge(int fromV, int toV)
        {
            graph[fromV - 1].Add(toV);
        }

        /* Remove the edge from 'fromv' to 'toV' */
        public void RemoveEdge(int fromV, int toV)
        {
            List<int> list = graph[fromV - 1];
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == toV)
                    list.Remove(toV);
            }
        }

        /* Returns list of nodes who is adjacent to the node 'fromV'. */
        public UniqueList AdjNodes(int fromV)
        {
            return graph[fromV - 1];
        }

        /* It nullifies all instance elements and calls GC.Collect() */
        public void Destroy()
        {
            foreach (UniqueList node in graph)
                node.Clear();

            graph = null;
            GC.Collect();
        }
    }
}
