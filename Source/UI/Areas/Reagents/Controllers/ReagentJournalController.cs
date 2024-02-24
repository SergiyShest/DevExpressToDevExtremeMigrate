using DevExpress.Web.Mvc;
using Sasha.Lims.Core.BusinessObjects;
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
    public class ReagentJournalController : BaseController
    {

        public ActionResult Index()
		{
			return View("Index",null);
		}

		[ValidateInput(false)]
		public ActionResult BatchEditingUpdateModel(MVCxGridViewBatchUpdateValues<vReagent, int> updateValues)
		{

			foreach (var view in updateValues.Update)
			{
				if (updateValues.IsValid(view))
				{
					var item = new  ReagentBo(view.Id);
					item.Barcode=view.Barcode;
					item.Quantity=view.Quantity;
					item.Save(GetCurrentUser());
				}
			}

			return CallbackRouteValues(null,null,null);
		}

		//	public ActionResult Select(
		//int? patientId,
		//int? doctorId,
		//int? parentId,
		//int? locationId,
		//bool? showSelectCancelButtons, string varName, bool multiSelect = false)
		//	{
		//		base.LayoutId = "SamplesBook.Select" + varName;
		//		ViewBag.DontShowMainMenu = true;
		//		var filterExpesson = CreateExpeession("PatientId", patientId, string.Empty, ExpressionType.NumberEquals);
		//		filterExpesson = CreateExpeession("DoctorId", doctorId, filterExpesson, ExpressionType.NumberEquals);
		//		filterExpesson = CreateExpeession("HospitalId", hospitalId, filterExpesson, ExpressionType.NumberEquals);

		//		if (parentlocationId != null)
		//		{
		//			var chailds = DataHelper.Current.Properties.GetDescendants((int) parentlocationId);

		//			foreach (var child in chailds)
		//			{
		//				filterExpesson = CreateExpeession("LocationId", child.id, filterExpesson, ExpressionType.NumberEquals, "or");
		//			}
		//		}
		//		ViewData["filterExpression"] = filterExpesson;
		//		ViewBag.MultiSelect = multiSelect;
		//		ViewBag.Title = "Select Sample";
		//		ViewBag.ShowSelectCancelButtons = showSelectCancelButtons == true;
		//		ViewBag.varName = varName ?? "SampleId";
		//		return View("Index", null);
		//	}



		public ActionResult SelectReagent(string varName, int? kitId, int? locationId)
		{
			ViewBag.Title = "Select reagent";
			ViewBag.GroupMode = false;
			ViewBag.DontShowMainMenu = true;
			ViewBag.ShowSelectCancelButtons = true;
			ViewBag.varName = varName ?? "ReagentId";
			ViewBag.DontCloseAfterSelect = true;
			var filterExpression = CreateExpeession("LocationId", locationId, string.Empty, ExpressionType.NumberNotEquals);
	
			 filterExpression = CreateExpeession("BatchId", kitId, filterExpression, ExpressionType.NumberNotEquals);//исключаю те которые есть
			filterExpression = CreateExpeession("Id", kitId, filterExpression, ExpressionType.NumberNotEquals);//исключаю себя

			ViewData["filterExpr"] = filterExpression;
			return View("Index", null);
		}

		private static IQueryable<vReagent> getModel()
		{
			IUnitOfWorkEx uow = new UnitOfWork();
			var ret = uow.GetRepository<vReagent>().GetAll();
			return ret;
		}

		public ActionResult CallbackRouteValues(string filterExpression, string customAction, string selectedIds)
		{

			if (customAction == "delete")
			{
				SafeExecute(() =>
				{
					if (!string.IsNullOrEmpty(selectedIds))
					{
						var ids = CoreHelper.ConvertIdsToArr(selectedIds);
						
						foreach (var id in ids)
						{
							var reatent = new ReagentBo(id);
							reatent.Discard(GetCurrentUser());
						}
						UOW.Save();
					}
				});
			}

			ViewData["filterExpression"] = filterExpression;
			return PartialView("_GridViewPartial", getModel());
		}

	}
}