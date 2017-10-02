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
	class Match : IRule
	{
		public string Role { get; set; }

		public bool Evaluate(Func<string, bool> matcher)
		{
			return matcher(Role);
		}

		public string ShowRule(int pad)
		{
			return new String(' ', pad * 4) + "Match: " + Role + "\r\n";
		}
	}
}
