using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SuperMvcDemo;
using SuperMvcDemo.Controllers;
using System.Web;
using System.Web.Routing;

namespace SuperMvcDemo.Tests.Controllers
{
	[TestClass]
	public class HomeControllerTest
	{
		[TestInitialize]
		public void Initialize()
		{
			string[] roles =
				{
					 "Admin",
					 "User",
					 "Local Office"
				};
			HttpContextHelper.SetCurrentContext(new FakePrincipal("Fake", "testuser", true, roles));
		}

		[TestMethod]
		public void Index()
		{
			// Arrange
			HomeController controller = new HomeController();

			// Act
			ViewResult result = controller.Index() as ViewResult;

			// Assert
			ViewDataDictionary viewData = result.ViewData;
			Assert.AreEqual("Welcome to ASP.NET MVC!", viewData["Message"]);
		}

		[TestMethod]
		public void About()
		{
			// Arrange
			HomeController controller = new HomeController();

			// Act
			ViewResult result = controller.About() as ViewResult;

			// Assert
			Assert.IsNotNull(result);
		}

		//[SuperAuthorize(SuperRoles = "!Guest")]
		//public ActionResult AboutTestOne()
		[TestMethod]
		public void AboutTestOne()
		{
			// Arrange
			ControllerContext context;
			var invoker = GetInvoker<RedirectToRouteResult>(out context);

			// Act
			var invokeResult = invoker.InvokeAction(context, "AboutTestOne");

			// Assert
			Assert.IsTrue(invokeResult);
		}

		//[SuperAuthorize(SuperRoles = "Guest")]
		//public ActionResult AboutTestTwo()
		[TestMethod]
		public void AboutTestTwo()
		{
			// Arrange
			ControllerContext context;
			var invoker = GetInvoker<HttpUnauthorizedResult>(out context);

			// Act
			var invokeResult = invoker.InvokeAction(context, "AboutTestTwo");

			// Assert
			Assert.IsTrue(invokeResult);
		}

		//[SuperAuthorize(SuperRoles = "Admin & Local Office")]
		//public ActionResult AboutTestThree()
		[TestMethod]
		public void AboutTestThree()
		{
			// Arrange
			ControllerContext context;
			var invoker = GetInvoker<RedirectToRouteResult>(out context);

			// Act
			var invokeResult = invoker.InvokeAction(context, "AboutTestThree");

			// Assert
			Assert.IsTrue(invokeResult);
		}

		//[SuperAuthorize(SuperRoles = "Admin & Local Office Supervisor")]
		//public ActionResult AboutTestFour()
		[TestMethod]
		public void AboutTestFour()
		{
			// Arrange
			ControllerContext context;
			var invoker = GetInvoker<HttpUnauthorizedResult>(out context);

			// Act
			var invokeResult = invoker.InvokeAction(context, "AboutTestFour");

			// Assert
			Assert.IsTrue(invokeResult);
		}

		//[SuperAuthorize(SuperRoles = "Admin & Local Office | Local Office Admin")]
		//public ActionResult AboutTestFive()
		[TestMethod]
		public void AboutTestFive()
		{
			// Arrange
			ControllerContext context;
			var invoker = GetInvoker<RedirectToRouteResult>(out context);

			// Act
			var invokeResult = invoker.InvokeAction(context, "AboutTestFive");

			// Assert
			Assert.IsTrue(invokeResult);
		}

		//[SuperAuthorize(SuperRoles = "!Guest & !(SuperUser | DaemonUser) & ((Admin & Local Office | Local Office Admin) & !Local Office Supervisor)")]
		//public ActionResult AboutCrazyOne()
		[TestMethod]
		public void AboutCrazyOne()
		{
			// Arrange
			ControllerContext context;
			var invoker = GetInvoker<RedirectToRouteResult>(out context);

			// Act
			var invokeResult = invoker.InvokeAction(context, "AboutCrazyOne");

			// Assert
			Assert.IsTrue(invokeResult);
		}

		//[SuperAuthorize(SuperRoles = "!Guest & !(SuperUser | DaemonUser) & ((Admin & Local Office | Local Office Admin) & !User)")]
		//public ActionResult AboutCrazyTwo()
		[TestMethod]
		public void AboutCrazyTwo()
		{
			// Arrange
			ControllerContext context;
			var invoker = GetInvoker<HttpUnauthorizedResult>(out context);

			// Act
			var invokeResult = invoker.InvokeAction(context, "AboutCrazyTwo");

			// Assert
			Assert.IsTrue(invokeResult);
		}

		private FakeControllerActionInvoker<TExpectedResult> GetInvoker<TExpectedResult>(out ControllerContext context) where TExpectedResult : ActionResult
		{
			HomeController controller = new HomeController();
			var httpContext = new HttpContextWrapper(HttpContext.Current);
			context = new ControllerContext(httpContext, new RouteData(), controller);
			controller.ControllerContext = context;
			var invoker = new FakeControllerActionInvoker<TExpectedResult>();
			return invoker;
		}
	}
}
