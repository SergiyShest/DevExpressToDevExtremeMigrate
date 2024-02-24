using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;
using ChoETL;
using Newtonsoft.Json;
using Sasha.Lims.Core.DTO.Laboratory;
using Sasha.Lims.Core.Helpers;
using Sasha.Lims.Core.Interfaces.Customization;
using Sasha.Lims.Core.Interfaces.Laboratory;
using Sasha.Lims.WebUI.Infrastructure.Helpers;
using Sasha.Lims.DataAccess.Entities.Constants;
using Sasha.Lims.DataAccess.Entities.Entity;
using Sasha.Lims.WebUI.Controllers;

namespace Sasha.Lims.WebUI.Areas.Workflow.Controllers
{
	public class ProcessController : CustomFieldsController
	{

		public ProcessController(IDtoMetadataService metadataService) : base(
			metadataService.GetUserFieldsForViewModel<RunListDTO>())
		{

		}

		public ActionResult ProcessView(int? rowId, string status, string resultPath, string act)
		{
			ExceptionHelper.ThrowExceptionIfRowIdIsNull(rowId);
			ViewBag.RowId = rowId;
			var model = GetModel(rowId);
			if (act == "save")
			{
				var changed = false;
				if (model.ChangeStatus(status, (int) GetCurrentUser().UserId))
				{
					changed = true;
				}
				if (model.ResultPath != resultPath)
				{
					model.ResultPath = resultPath;
					changed = true;
				}
				if (changed)
				{
					var user = GetCurrentUser();
					model.Save(user);
				}
			}
			return View("ProcessSamplesView", model);
		}

		public ActionResult CallbackRouteValues(int? rowId)
		{
			ExceptionHelper.ThrowExceptionIfRowIdIsNull(rowId);
			return PartialView("_ProcessSamplesGridPartial", GetModel(rowId));
		}

		private Core.BusinessObjects.ProcessBo GetModel(int? rowId)
		{
			return new Core.BusinessObjects.ProcessBo(rowId);
		}


		public JsonResult ChangeSampleResultStatus(string selectedIds, string status)
		{
			var error = string.Empty;
			try
			{
				var ids = CoreHelper.ConvertIdsToArr(selectedIds);
				var user = GetCurrentUser();
				foreach (var sampleResId in ids)
				{
					var process = new Core.BusinessObjects.SampleInProcessBo(sampleResId);
					process.ChangeResult(status, (int) user.UserId);
					process.Save(user);
				}
			}
			catch (Exception ex)
			{
				error = ex.Message;
			}
			return Json(error, JsonRequestBehavior.AllowGet);
		}

		public JsonResult RemoveSamples(int? processId, string selectedIds)
		{
			ExceptionHelper.ThrowExceptionIfRowIdIsNull(processId);
			var error = string.Empty;
			try
			{
				var user = GetCurrentUser();
				var process = new Core.BusinessObjects.ProcessBo(processId);
				process.Load();
				process.RemoveSamples(user,selectedIds);
				process.Save(user);
			}
			catch (Exception ex)
			{
				error = ex.Message;
			}
			return Json(error, JsonRequestBehavior.AllowGet);
		}

        public JsonResult PublishResult(int? processId, string selectedIds)
		{
			var error = string.Empty;
			try
			{
				var ids = CoreHelper.ConvertIdsToArr(selectedIds);
				var repos = UOW.GetRepository<wf_result>();
			    var res= repos.GetAll(false).Where(x=>x.process_id==processId && ids.Contains((int)x.id));

				foreach (var wfResult in res)
				{ 
					wfResult.Sended = 1;
				}
				repos.Save();
			}
			catch (Exception ex)
			{
				error = ex.GetAllMessages();
			}
			return Json(error, JsonRequestBehavior.AllowGet);
		}


	}
}
