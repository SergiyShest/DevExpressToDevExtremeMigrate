using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sasha.Lims.Core.Helpers;

namespace Sasha.Lims.WebUI.Controllers.BaseAndCore
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using System.Web.Mvc;
	namespace ErrorHandler.Controllers
	{
		public class ErrorController : Controller
		{


			public ErrorController()
			{

			}

			public ActionResult General()
			{

				var ex = RouteData.DataTokens["exception"] as Exception;
				if (ex != null)
				{
					ViewBag.message = ex.GetAllMessages();
				}
                 ViewBag.error=	ex?.ToString();
				return View("GeneralError");
			}
			public ActionResult NotFound()
			{
				var ex = RouteData.DataTokens["exception"] as Exception;
				ViewBag.message = ex?.Message;
				return View("NotFound");
			}
			public ActionResult BadLicense()
			{	var ex = RouteData.DataTokens["exception"] as Exception;
				ViewBag.message = ex?.InnerException?.Message;
				return View("BedLicense");
			}

		}
	}
}