using Sasha.Lims.Core.Helpers;
using Sasha.Lims.DataAccess.Entities.Entity;
using Sasha.Lims.DataAccess.Repositories;
using Sasha.Lims.WebUI.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sasha.Lims.WebUI.Areas.Common.Controllers
{
    public class CustodyLogJournalController : BaseController
    {

        public ActionResult Index(int? sampleId,bool? dontShowMainMenu)
		{
			if (sampleId != null)
			{
				base.LayoutId = "CustodyLogJournal.Select" + sampleId;
				var filterExpression = CreateExpeession("SampleId", sampleId, "", ExpressionType.NumberEquals);
				ViewBag.DontShowMainMenu = dontShowMainMenu == true;
				ViewData["filterExpr"] = filterExpression;
			}
			return View("Index",null);
		}


		private static IQueryable<vCustodyLog> getModel()
		{
			IUnitOfWorkEx uow = new UnitOfWork();
			var log = uow.GetRepository<vCustodyLog>().GetAll();
			return log;
		}

		public ActionResult CallbackRouteValues(string filterExpression)
		{
			ViewData["filterExpression"] = filterExpression;
			return PartialView("_GridViewPartial", getModel());//getModel()
		}

	}
}