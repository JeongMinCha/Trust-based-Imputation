using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trust_based_Imputation
{
    /* Because a sealed class can never be used as a base class, some run-time 
     * optimizations can make calling sealed class members slightly faster. */

    /* This class makes CountTable class have its elements in reverse order. */
    public sealed class ReverseComparer<T> : IComparer<T>
    {
        private readonly IComparer<T> original;

        public ReverseComparer(IComparer<T> original)
        {
            // TODO: Validation
            this.original = original;
        }

        public int Compare(T left, T right)
        {
            return original.Compare(right, left);
        }
    }

    class CountTable : SortedDictionary<int, int>
    {
        public CountTable()
            : base(new ReverseComparer<int>(Comparer<int>.Default))
        {
        }

        /* If key already exists, the value at key will increase.
        / Otherwise, key is inserted with value 1. */
        public void AddCount(int key)
        {
            if (ContainsKey(key))
                this[key] = (int)this[key] + 1;
            else
                Add(key, 1);
        }
    }
}
