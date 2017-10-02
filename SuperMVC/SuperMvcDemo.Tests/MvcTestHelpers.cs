/*	* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 *	Copyright (c) 2010, Tyler Jensen (http://www.tsjensen.com/blog)
 *	All rights reserved. See license terms in AssemblyInfo.cs file.
 *	* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Principal;
using System.Web;
using System.Threading;
using System.IO;
using System.Reflection;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SuperMvcDemo.Tests
{
	public static class HttpContextHelper
	{
		public static void SetCurrentContext()
		{
			SetCurrentContext("index.aspx", "http://tempuri.org/index.aspx", null, true);
		}

		public static void SetCurrentContext(bool setCurrentUser)
		{
			SetCurrentContext("index.aspx", "http://tempuri.org/index.aspx", null, setCurrentUser);
		}

		public static void SetCurrentContext(string fileName, string url, string queryString, bool setCurrentUser)
		{
			var context = CreateHttpContext(fileName, url, queryString);
			if (setCurrentUser)
			{
				context.User = new WindowsPrincipal(WindowsIdentity.GetCurrent());
			}
			var result = RunInstanceMethod(Thread.CurrentThread, "GetIllogicalCallContext", new object[] { });
			SetPrivateInstanceFieldValue(result, "m_HostContext", context);
		}

		public static void SetCurrentContext(FakePrincipal principal)
		{
			SetCurrentContext("index.aspx", "http://tempuri.org/index.aspx", null, principal);
		}

		public static void SetCurrentContext(string fileName, string url, string queryString, FakePrincipal principal)
		{
			var context = CreateHttpContext(fileName, url, queryString);
			context.User = principal;
			var result = RunInstanceMethod(Thread.CurrentThread, "GetIllogicalCallContext", new object[] { });
			SetPrivateInstanceFieldValue(result, "m_HostContext", context);
		}

		private static HttpContext CreateHttpContext(string fileName, string url, string queryString)
		{
			var sb = new StringBuilder();
			var sw = new StringWriter(sb);
			var hres = new HttpResponse(sw);
			var hreq = new HttpRequest(fileName, url, queryString);
			var httpc = new HttpContext(hreq, hres);
			return httpc;
		}

		private static object RunInstanceMethod(object source, string method, object[] objParams)
		{
			var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			var type = source.GetType();
			var m = type.GetMethod(method, flags);
			if (m == null)
			{
				throw new ArgumentException(string.Format("There is no method '{0}' for type '{1}'.", method, type));
			}

			var objRet = m.Invoke(source, objParams);
			return objRet;
		}

		private static void SetPrivateInstanceFieldValue(object source, string memberName, object value)
		{
			var field = source.GetType().GetField(memberName, BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance);
			if (field == null)
			{
				throw new ArgumentException(string.Format("Could not find the private instance field '{0}'", memberName));
			}

			field.SetValue(source, value);
		}
	}

	public class FakePrincipal : IPrincipal
	{
		IEnumerable<string> Roles { get; set; }
		FakeIdentity identity;

		public FakePrincipal(string authenticationType, string name, bool isAuthenticated, IEnumerable<string> roles)
		{
			this.Roles = roles;
			identity = new FakeIdentity(authenticationType, name, isAuthenticated);
		}

		public IIdentity Identity
		{
			get { return identity; }
		}

		public bool IsInRole(string role)
		{
			return Roles.Any(x => x.ToLowerInvariant() == role.ToLowerInvariant());
		}
	}

	public class FakeIdentity : IIdentity
	{
		string authenticationType;
		string name;
		bool isAuthenticated;

		public FakeIdentity(string authenticationType, string name, bool isAuthenticated)
		{
			this.authenticationType = authenticationType;
			this.name = name;
			this.isAuthenticated = isAuthenticated;
		}

		public string AuthenticationType
		{
			get { return authenticationType; }
		}

		public bool IsAuthenticated
		{
			get { return isAuthenticated; }
		}

		public string Name
		{
			get { return name; }
		}
	}

	public class FakeControllerActionInvoker<TExpectedResult> : ControllerActionInvoker where TExpectedResult : ActionResult
	{
		protected override void InvokeActionResult(ControllerContext controllerContext, ActionResult actionResult)
		{
			Assert.IsInstanceOfType(actionResult, typeof(TExpectedResult));
		}
	}
}
