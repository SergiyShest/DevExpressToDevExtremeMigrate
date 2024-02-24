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
	public class ReagentTypeEditController : BaseController
	{
		#region fields

		private UnitOfWork _unitOfWork;

		#endregion

		#region Constructor

		public ReagentTypeEditController()
		{
			_unitOfWork = new UnitOfWork();
		}

		#endregion

		public ActionResult Index(int? id,bool? copy)
		{
            ViewBag.Id = id;
            ViewBag.ForCopy = copy==true;
			return View("Index");
		}

		public ActionResult Get(int? id)
		{
			string json;
			dynamic expOb = new ExpandoObject();
			try
			{
				var model = new ReagentTypeEditModel(id);
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

		public ActionResult GetReagentType(int? id)
		{
			string json;
			dynamic expOb = new ExpandoObject();
			try
			{
				expOb.Item = UOW.GetRepository<vReagentType>().FirstOrDefault(x=>x.Id==id);
			}
			catch (Exception ex)
			{
				expOb.Errors = new List<ObjectError>() { new ObjectError("", ex.GetAllMessages()) };
				LogManager.GetLogger("Error ReagentEditModel ids= " + id).Error(ex);

			}
			json = JsonConvert.SerializeObject(expOb);
			return Content(json, "application/json");
		}

		public ActionResult GetChaild( int parentId)
		{
			var links = UOW.GetRepository<r_reagentTypeLink>().GetAll(x=>x.parentReagentType_id==parentId).Select(x=>x.reagentType_id).ToList();

			var chaildReagents = UOW.GetRepository<vReagentType>().Where(x => links.Contains(x.Id));
			return Content(JsonConvert.SerializeObject(chaildReagents), "application/json");
		}

		public ActionResult AddChaild( int parentId,string ids)
		{

			return SafeExecuteJson(AddReagentChaild, parentId, ids);
		}

		private object AddReagentChaild(object parentId, object ids)
		{
			var idsArr = CoreHelper.ConvertIdsToArr((string)ids);
			var rep = UOW.GetRepository<r_reagentTypeLink>();

			foreach (var reagentTypeId in idsArr)
			{
				rep.Create(new r_reagentTypeLink() {reagentType_id=reagentTypeId,parentReagentType_id =(int)parentId});
			}
			rep.Save();
			return null;
		}
		private object RemoveReagentChaild(object parentId, object ids)
		{
			var idsArr = CoreHelper.ConvertIdsToArr((string) ids);
			var rep = UOW.GetRepository<r_reagentTypeLink>();
			var chaildRecordsForRemove = rep.Where(x => x.parentReagentType_id == (int) parentId && idsArr.Contains((int) x.reagentType_id));
			foreach (var chaild in chaildRecordsForRemove)
			{
				rep.Delete(chaild.id,false);
			}
			rep.Save();
			return null;
		}
		public ActionResult RemoveChaild(int parentId,string ids)
		{
				return SafeExecuteJson(RemoveReagentChaild,parentId, ids);
		}

		public ActionResult Save(int? id)
		{
			var model = new ReagentTypeEditModel(id);
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
			var model = new ReagentTypeEditModel(id);
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