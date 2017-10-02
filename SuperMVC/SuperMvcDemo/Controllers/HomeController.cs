using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SuperMvc;

namespace SuperMvcDemo.Controllers
{
	[HandleError]
	public class HomeController : Controller
	{
		public ActionResult Index()
		{
			ViewData["Message"] = "Welcome to ASP.NET MVC!";

			return View();
		}

		public ActionResult About()
		{
			return View();
		}

		[SuperAuthorize(SuperRoles = "!Guest")]
		public ActionResult AboutTestOne()
		{
			return RedirectToAction("About");
		}

		[SuperAuthorize(SuperRoles = "Guest")]
		public ActionResult AboutTestTwo()
		{
			return RedirectToAction("About");
		}

		[SuperAuthorize(SuperRoles = "Admin & Local Office")]
		public ActionResult AboutTestThree()
		{
			return RedirectToAction("About");
		}

		[SuperAuthorize(SuperRoles = "Admin & Local Office Supervisor")]
		public ActionResult AboutTestFour()
		{
			return RedirectToAction("About");
		}

		[SuperAuthorize(SuperRoles = "Admin & Local Office | Local Office Admin")]
		public ActionResult AboutTestFive()
		{
			return RedirectToAction("About");
		}

		[SuperAuthorize(SuperRoles = "!Guest & !(SuperUser | DaemonUser) & ((Admin & Local Office | Local Office Admin) & !Local Office Supervisor)")]
		public ActionResult AboutCrazyOne()
		{
			return RedirectToAction("About");
		}

		[SuperAuthorize(SuperRoles = "!Guest & !(SuperUser | DaemonUser) & ((Admin & Local Office | Local Office Admin) & !User)")]
		public ActionResult AboutCrazyTwo()
		{
			return RedirectToAction("About");
		}
	}
}
