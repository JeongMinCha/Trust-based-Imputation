using System;
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

            // make new instances of ALGraph class
            // This class and ALGraph are composition-related.
            forwardGraph = new ALGraph(userNum);
            backwardGraph = new ALGraph(userNum);

            string[] lines = System.IO.File.ReadAllLines(dataSet.trustNetworkFile);
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
            Console.WriteLine("2. Trust Network Graph is made");
        }

        /* Returns list of users who trust the user. (backward graph) */
        public UniqueList GetTrustorList(int userID, int hop)
        {
            UniqueList trustorList = null;
            if (hop == 1)
                trustorList = backwardGraph.AdjNodes(userID);
            else
            {
                trustorList = new UniqueList();
                UniqueList prevList = GetTrustorList(userID, hop - 1);
                foreach (int fromV in prevList)
                {
                    UniqueList tmp = backwardGraph.AdjNodes(fromV);
                    foreach (int v in tmp)
                        trustorList.Add(v);
                }
            }
            return trustorList;
        }

        /* Returns list of users who are trusted by the user. (forward graph) */
        public UniqueList GetTrusteeList(int userID, int hop)
        {
            UniqueList trusteeList = null;

            if (hop == 1)
                trusteeList = forwardGraph.AdjNodes(userID);
            else
            {
                trusteeList = new UniqueList();
                UniqueList prevList = GetTrusteeList(userID, hop - 1);
                foreach (int fromV in prevList)
                {
                    UniqueList tmp = forwardGraph.AdjNodes(fromV);
                    foreach (int v in tmp)
                        trusteeList.Add(v);
                }
            }
            return trusteeList;
        }

        /* Returns union of trustor and trustee list.*/
        public UniqueList GetTrustorAndTrusteeList(int userID, int hop)
        {
            UniqueList trustorList = GetTrustorList(userID, hop);
            UniqueList trusteeList = GetTrusteeList(userID, hop);
            UniqueList list = new UniqueList(trustorList.Count + trusteeList.Count);

            // get elements from trustor list
            for (int i = 0; i < trustorList.Count; i++)
                list.Add(trustorList[i]);

            // get elements from trustee list.
            for (int i = 0; i < trusteeList.Count; i++)
                list.Add(trusteeList[i]);

            // return the union of both list.
            return list;
        }

        /* It nullifies all instance elements and calls GC.Collect() */
        public void Destroy()
        {
            forwardGraph.Destroy();
            backwardGraph.Destroy();
            GC.Collect();
        }
    }
}
