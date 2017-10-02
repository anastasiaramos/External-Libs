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
	public interface IRule
	{
		bool Evaluate(Func<string, bool> matcher);
		string ShowRule(int pad);
	}

	public interface IComparisonRule : IRule
	{
		IRule LValue { get; set; }
		IRule RValue { get; set; }
	}
}
