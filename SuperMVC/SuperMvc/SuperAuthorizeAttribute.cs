/*	* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 *	Copyright (c) 2010, Tyler Jensen (http://www.tsjensen.com/blog)
 *	All rights reserved. See license terms in AssemblyInfo.cs file.
 *	* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using SuperMvc.RoleRules;

namespace SuperMvc
{
	public class SuperAuthorizeAttribute : AuthorizeAttribute
	{
		private string _superRoles;
		private IRule _superRule;

		public string SuperRoles
		{
			get
			{
				return _superRoles ?? String.Empty;
			}
			set
			{
				_superRoles = value;
				if (!string.IsNullOrWhiteSpace(_superRoles))
				{
					RoleParser parser = new RoleParser();
					_superRule = parser.Parse(_superRoles);
				}
			}
		}

		public override void OnAuthorization(AuthorizationContext filterContext)
		{
			base.OnAuthorization(filterContext);
			if (_superRule != null)
			{
				var result = _superRule.Evaluate(role => filterContext.HttpContext.User.IsInRole(role));
				if (!result)
				{
					filterContext.Result = new HttpUnauthorizedResult();
				}
			}
		}
	}
}
