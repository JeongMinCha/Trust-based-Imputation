using System;
using System.Collections;
using System.Collections.Generic;

namespace Trust_based_Imputation
{
	public class AugmentingTable : Dictionary<int, double>
	{
		public AugmentingTable () : base()
		{
		}

		public void AugmentValue (int key)
		{
			try
			{
				this[key] = this[key] + 1;
			} 
			catch (KeyNotFoundException)
			{
				Add(key, 1);
			}
		}
	}
}