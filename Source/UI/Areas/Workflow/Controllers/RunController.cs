using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;
using Newtonsoft.Json;
using Sasha.Lims.Core.DTO.Laboratory;
using Sasha.Lims.Core.Interfaces.Customization;
using Sasha.Lims.Core.Interfaces.Laboratory;
using Sasha.Lims.WebUI.Infrastructure.Helpers;
using Sasha.Lims.DataAccess.Entities.Constants;
using Sasha.Lims.WebUI.Controllers;

namespace Sasha.Lims.WebUI.Areas.Workflow.Controllers
{
	public class RunController : CustomFieldsController
	{
		public RunController(IDtoMetadataService metadataService) : base(metadataService.GetUserFieldsForViewModel<RunListDTO>())
		{

		}

		public ActionResult CallbackRouteValues(int? rowId,string act)
		{
			ExceptionHelper.ThrowExceptionIfRowIdIsNull(rowId);
			return PartialView("_RunProcessGridPartial", GetModel(rowId));
		}

		private Core.BusinessObjects.RunBo GetModel(int? rowId)
		{
			return new Core.BusinessObjects.RunBo(rowId);
		}

		public ActionResult RunProcessView(int? rowId,string status)
		{
			ExceptionHelper.ThrowExceptionIfRowIdIsNull(rowId);
			ViewBag.RowId = rowId;
			var user = GetCurrentUser();
			var	model = GetModel(rowId);
			if(model.ChangeStatus(status, (int) user.UserId))
			{
				model.Save(user);
			}
			return View("RunProcessView", model);
		}

		public JsonResult ChangeProcessStatus(int? rowId, string status)
		{
			ExceptionHelper.ThrowExceptionIfRowIdIsNull(rowId);
			var error = string.Empty;
			try
			{
				var user = GetCurrentUser();
				var process = new Core.BusinessObjects.ProcessBo(rowId);
				process.Load();
				if (process.ChangeStatus(status, (int) user.UserId))
				{
					process.Save(user);
				}
			}
			catch (Exception ex)
			{
				error = ex.Message;
			}
			return Json(error, JsonRequestBehavior.AllowGet);
		}
	}
}
