using DevExpress.DataProcessing;
using DevExtreme.AspNet.Data;
using GroupChange;
using Newtonsoft.Json;
using NLog;
using Sasha.Lims.Core.BusinessObjects;
using Sasha.Lims.Core.Helpers;
using Sasha.Lims.Core.Infrastructure.Exceptions;
using Sasha.Lims.Core.Interfaces.Laboratory;
using Sasha.Lims.DataAccess.Entities.Constants;
using Sasha.Lims.DataAccess.Entities.Entity;
using Sasha.Lims.DataAccess.Repositories;
using Sasha.Lims.WebUI.Controllers;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DataHelper = Sasha.Lims.Core.Helpers.DataHelper;

namespace Sasha.Lims.WebUI.Areas.Reagents.Controllers
{
	public class ReagentEditController : BaseController
	{
		#region fields

		private UnitOfWork _unitOfWork;


		#endregion

		#region Constructor

		public ReagentEditController()
		{
			_unitOfWork = new UnitOfWork();
		}

		#endregion

		public ActionResult Index(int? id,int? reagentTypeId,bool? copy)
		{
            ViewBag.Id = id;
            ViewBag.ForCopy = copy==true;
			ViewBag.TypeId = (int)TableType.Reagent;
			ViewBag.ReagentTypeId = reagentTypeId;
			//ViewBag.GetPath = "/Reagents/ReagentEdit/Get?reagentTypeId="+reagentTypeId;
			//ViewBag.SavePath = "/Reagents/ReagentEdit/Save?reagentTypeId="+reagentTypeId;
            //ViewBag.StringValuePath = "/Reagents/ReagentEdit/GetStringValue?";
			return View("Index");
		}

		public ActionResult Get(int? id,int? reagentTypeId)
		{
			string json;
			dynamic expOb = new ExpandoObject();
			try
			{
				var model = new ReagentEditModel(id,reagentTypeId);

				json = model.GetJson();
			}
			catch (Exception ex)
			{
				expOb.Errors = new List<ObjectError>() { new ObjectError("", ex.GetAllMessages()) };
				LogManager.GetLogger("Error ReagentEditModel ids= " + id).Error(ex);
				json = JsonConvert.SerializeObject(expOb);
			}
			return Content(json, "application/json");
		}






		public ActionResult GetChaild(int parentId)
		{
			var chaildReagents = UOW.GetRepository<vReagent>().Where(x => x.BatchId == parentId);
			//var loadResult = DataSourceLoader.Load(chaildReagents, loadOptions);
			return Content(JsonConvert.SerializeObject(chaildReagents), "application/json");
		}

		public ActionResult AddChaild( int parentId, string ids)
		{
			return SafeExecuteJson(SetReatentParent, parentId, ids);
		}

		private object SetReatentParent(object parentId, object ids)
		{
			var idsArr = CoreHelper.ConvertIdsToArr((string) ids);
			var rep = UOW.GetRepository<r_reagent>();
			var chaildReagents = rep.Where(x => idsArr.Contains(x.id));
			foreach (r_reagent reagent in chaildReagents)
			{
				var reagentBo = new ReagentBo(reagent);
				reagentBo.ParentId = (int?) parentId;
				reagentBo.Save(GetCurrentUser());
			}
			
			return null;
		}

		public ActionResult RemoveChaild(string ids)
		{
				return SafeExecuteJson(SetReatentParent,null, ids);
		}

		public ActionResult Save(int? id, int? reagentTypeId)
		{
			var model = new ReagentEditModel(id,reagentTypeId);
			var bodyStream = new StreamReader(HttpContext.Request.InputStream);
			bodyStream.BaseStream.Seek(0, SeekOrigin.Begin);
			var bodyText = bodyStream.ReadToEnd();
			var errors = model.SetJson(bodyText);
			if (errors.Count == 0)
			{
				errors = model.Save(GetCurrentUser());
			};

			var json = JsonConvert.SerializeObject(new { Errors = errors });
			return Content(json, "application/json");
		}

		public ActionResult Copy(int? id)
		{
			var model = new ReagentEditModel(id);
			var bodyStream = new StreamReader(HttpContext.Request.InputStream);
			bodyStream.BaseStream.Seek(0, SeekOrigin.Begin);
			var bodyText = bodyStream.ReadToEnd();
			var errors = model.SetJson(bodyText);
			if (errors.Count == 0)
			{
				errors = model.Copy(GetCurrentUser());
			};

			var json = JsonConvert.SerializeObject(new { Errors = errors });
			return Content(json, "application/json");
		}

		public ActionResult GetStringValue(int? id, string varName, int? typeId)
		{
			string json = null;
			dynamic expOb = new ExpandoObject();
			try
			{
				NullTest(typeId, nameof(typeId));
				var strVal = string.Empty;

				if(varName == "LocationId")
				{
					strVal = this.UOW.GetRepository<a_prop>().FirstOrDefault(x => x.id == id)?.path;
				}
				else
				{ 
				var type = new TypeDescriptionBo(typeId);
				var property = type.Fields.FirstOrDefault(x => x.Name == varName);
				var propertyF = type.Fields;
				if (property == null) throw new InvalidOperationException("Not find Field named " + varName);
				strVal = property.GetListStringValue(id);
				
                }
				expOb.Item = strVal;
			}
			catch (Exception ex)
			{
				expOb.Errors = new List<ObjectError>() { new ObjectError("", ex.GetAllMessages()) };
				log.Error(ex);
			}
			json = JsonConvert.SerializeObject(expOb);
			return Content(json, "application/json");
		}

		protected ActionResult SafeExecuteJson(Func<object,object,object> method, object param1,object param2)
		{
			string json = String.Empty;
			dynamic expOb = new ExpandoObject();
			try
			{
				var ret = method(param1,param2);
				if (ret != null)
				{
					expOb.Item = ret;
					json = JsonConvert.SerializeObject(expOb);
				}
			}
			catch (Exception ex)
			{
				expOb.Errors = new List<ObjectError>() { new ObjectError("", ex.GetAllMessages()) };
				LogManager.GetCurrentClassLogger().Error(ex);
				json = JsonConvert.SerializeObject(expOb);
			}

			return Content(JsonConvert.SerializeObject(json), "application/json");
		}

	}

}