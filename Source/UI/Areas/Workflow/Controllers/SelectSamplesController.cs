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
using Sasha.Lims.Core.Services.Common;
using Sasha.Lims.WebUI.Infrastructure.Helpers;
using Sasha.Lims.DataAccess.Entities.Constants;
using Sasha.Lims.DataAccess.Entities.Entity;
using Sasha.Lims.WebUI.Controllers;

namespace Sasha.Lims.WebUI.Areas.Workflow.Controllers
{
	public class SelectSamplesController : CustomFieldsController
	{
		private ISamplesBookService _samplesService;
		public SelectSamplesController(ISamplesBookService samplesService, IDtoMetadataService metadataService) : base(
			metadataService.GetUserFieldsForViewModel<RunListDTO>())
		{
			_samplesService = samplesService;
		}

		public ActionResult CallbackRouteValues(int? processId, string filterExpression)
		{
			ViewData["filterExpression"] = filterExpression;
			ViewBag.ProcessName = processId;//todo: начитывать имя
			ViewBag.ProcessId = processId;
			return PartialView("_SelectSamplesGridViewPartial", GetModel());
		}


		private IQueryable<vSamplesBook> GetModel()
		{
			return _samplesService.Data();
		}

		public ActionResult SelectSamples(int? processId)
		{
			ViewBag.ProcessId = processId;
			ViewBag.ProcessName = processId;//todo: начитывать имя
			return View("SelectSamplesView", null);
		}

		public JsonResult AddSamples(int? processId, string selectedIds)
		{
			ExceptionHelper.ThrowExceptionIfRowIdIsNull(processId);
			var error = string.Empty;
			try
			{
				var user = GetCurrentUser();
				var process = new Core.BusinessObjects.ProcessBo(processId);
				process.Load();
				process.AddSamples(user,selectedIds);
				process.Save(user);
			}
			catch (Exception ex)
			{
				error = ex.Message;
			}
			return Json(error, JsonRequestBehavior.AllowGet);
		}

	}
}