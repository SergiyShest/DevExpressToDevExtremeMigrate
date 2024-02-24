using AutoMapper;
using DevExpress.DataProcessing;
using DevExpress.Web;
using GroupChange;
using Newtonsoft.Json;
using Sasha.Lims.Core.BusinessObjects;
using Sasha.Lims.Core.DTO.Access;
using Sasha.Lims.Core.DTO.Common;
using Sasha.Lims.Core.DTO.Laboratory;
using Sasha.Lims.Core.Helpers;
using Sasha.Lims.Core.Infrastructure.Classes;
using Sasha.Lims.Core.Interfaces.Common;
using Sasha.Lims.Core.Interfaces.Laboratory;
using Sasha.Lims.Core.Settings;
using Sasha.Lims.DataAccess.Entities.Constants;
using Sasha.Lims.DataAccess.Entities.Entity;
using Sasha.Lims.DataAccess.Repositories;
using Sasha.Lims.WebUI.Controllers;
using Sasha.Lims.WebUI.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using DataHelper = Sasha.Lims.Core.Helpers.DataHelper;


namespace Sasha.Lims.WebUI.Areas.SamplesEdit.Controllers
{
	public class GroupInputController : BaseSamplesController
	{
		#region fields

		private UnitOfWork _unitOfWork;

		private readonly IPropertiesService _propertiesService;
		private readonly ISamplesService _samplesService;
		private readonly ISamplesApprovedService _samplesApprovedService;

		#endregion
		#region Constructor

		public GroupInputController(ISamplesService samplesService, IPropertiesService propertiesService, ISamplesApprovedService samplesApprovedService)
		{
			_samplesApprovedService = samplesApprovedService;
			_propertiesService = propertiesService;
			_samplesService = samplesService;
			_unitOfWork = new UnitOfWork();

		}

		#endregion

		#region GroupInput

		public ActionResult Index(string selectedIds, int journalType)
		{
			var ids = CoreHelper.ConvertIdsToArr(selectedIds);

			ViewBag.SelectedIds = selectedIds;
			ViewBag.EditingRowCount = ids.Count;
			ViewBag.JournalType = journalType;
			return View("Index");
		}

		public JsonResult GroupInputSave(string selectedIds, int journalType)
		{
			var usr = GetCurrentUser();
			var bodyStream = new StreamReader(HttpContext.Request.InputStream);
			bodyStream.BaseStream.Seek(0, SeekOrigin.Begin);
			var bodyText = bodyStream.ReadToEnd();

			dynamic dto = JsonConvert.DeserializeObject(bodyText);
			string res = "";

			try
			{
				if (string.IsNullOrWhiteSpace(selectedIds))
				{
//					_samplesService.GroupInputInsert(dto, (JournalType) journalType, GetCurrentUser());
					SamplesModel sm = new SamplesModel(UOW);
					sm.GroupInputInsert(dto, (JournalType)journalType, usr);
				} else
				{
					_samplesService.GroupInputSave(selectedIds, dto, usr);
				}
			}
			catch (Exception ex)
			{
				res = ex.Message;
			}
			return Json(res, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// поля доступные для редактирования в интерфейсе GroupInput
		/// </summary>
		/// <returns></returns>
		public JsonResult GetAvailableFields()
		{
			var res = new List<NamedValueDto>();
			var model =  new GroupEditModel(null,this.UOW);
			var availableProperties = model.SettingList; ;

			var remProperty = new List<string>() {"Id" ,"SampleId", "SampleRowVersion", "IsChild", "SourceId", "ProcessId","RowVersion","UserId", "AspNetUserId", "EquipmentId", "SampleWorkflowStatusId", "MakeAliquot", "ModelId" };

			foreach (var property in availableProperties)
			{
				if (remProperty.Contains(property.Name)) continue;
				var dto = new NamedValueDto() { Name = property.Name};
				dto.Value = property.Text;
				switch (property.DataType)
				{
					case UserFieldDataType.Date: dto.DataType = "date"; break;
					case UserFieldDataType.Number: dto.DataType = "number"; break;
					case UserFieldDataType.Boolean: dto.DataType = "booleak"; break;
					case UserFieldDataType.List: dto.DataType = "string"; break;
				}
                
				res.Add(dto);
			}

			res=res.OrderBy(x => x.Name).ToList();

			return Json(res, JsonRequestBehavior.AllowGet);
		}

		#endregion


	}


}