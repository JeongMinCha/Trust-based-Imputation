using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trust_based_Imputation
{
	class UniqueList : List<int>
	{
		public UniqueList()
			: base()
		{
		}

		public UniqueList(int capacity)
			: base(capacity)
		{
		}

		/* customized add */
		public virtual void Add(int item)
		{
			if (!base.Contains(item))
				base.Add(item);
		}
	}
}