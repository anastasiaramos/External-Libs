/*	* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * Contributed by and used with permission from Nick Muhonen of 
 * Useable Concepts Inc. (http://www.useableconcepts.com/).
 * Copyright of this code is incorporated under the license terms
 * indicated in the AssemblyInfo.cs file of this project.
 *	* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperMvc.RoleRules
{
	class Or : IComparisonRule
	{
		public IRule LValue { get; set; }
		public IRule RValue { get; set; }

		public bool Evaluate(Func<string, bool> matcher)
		{
			return LValue.Evaluate(matcher) || RValue.Evaluate(matcher);
		}

		public string ShowRule(int pad)
		{
			String padStr = new string(' ', pad * 4);

			return
				 padStr + "Begin Or\r\n" +
				 LValue.ShowRule(pad + 1) +
				 RValue.ShowRule(pad + 1) +
				 padStr + "End Or\r\n";
		}
	}
}
