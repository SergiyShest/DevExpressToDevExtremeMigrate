using Sasha.Lims.Core.Helpers;
using Sasha.Lims.DataAccess.Entities.Entity;
using Sasha.Lims.DataAccess.Repositories;
using Sasha.Lims.WebUI.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sasha.Lims.WebUI.Areas.Reagents.Controllers
{
	public class ReagentTypeJournalController : BaseController
	{

		public ActionResult Index()
		{
			ViewBag.GroupMode = true;
			return View("Index", null);
		}

		public ActionResult SelectReagentTypeForAdd(int? kitId)
		{
			ViewBag.Title = "Select reagent types";
			ViewBag.DontShowMainMenu = true;
			ViewBag.ShowSelectCancelButtons = true;
			ViewBag.varName = "SelectedIdsForAdd";

			var filterExpression = CreateExpeession("Id", kitId, String.Empty, ExpressionType.NumberNotEquals);//исключаю себя
			ViewBag.GroupMode = true;
			ViewData["filterExpr"] = filterExpression;
			return View("Index", null);
		}


		public ActionResult SelectReagentType(string varName,bool? dontCloseAfterSelect=true)
		{
			ViewBag.Title = "Select reagent type";
			ViewBag.GroupMode  = false;
			ViewBag.DontShowMainMenu = true;
			ViewBag.ShowSelectCancelButtons = true;
			ViewBag.varName = varName ?? "ReagentTypeId";
			ViewBag.DontCloseAfterSelect= dontCloseAfterSelect!=false;
			return View("Index", null);
		}


		private static IQueryable<vReagentType> getModel()
		{
			IUnitOfWorkEx uow = new UnitOfWork();
			var ret = uow.GetRepository<vReagentType>().GetAll();
			return ret;
		}

		public ActionResult CallbackRouteValues(string filterExpression, string customAction, string selectedIds)
		{
			ViewBag.GroupMode = true;
			if (customAction == "delete")
			{
				SafeExecute(() =>
				{
					if (!string.IsNullOrEmpty(selectedIds))
					{
						var ids = CoreHelper.ConvertIdsToArr(selectedIds);
						var patRep = DataHelper.UOW.GetRepository<r_reagentType>();
						foreach (var id in ids)
						{
							patRep.Delete(id);
						}
					}
				});
			}
			ViewData["filterExpression"] = filterExpression;
			return PartialView("_GridViewPartial", getModel());
		}

	}
}