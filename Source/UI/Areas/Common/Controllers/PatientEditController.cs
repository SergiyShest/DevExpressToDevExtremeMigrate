using DevExpress.DataProcessing;
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

namespace Sasha.Lims.WebUI.Areas.Common.Controllers
{
	//public class PatientEditController : BaseController
	//{
	//	#region fields

	//	private UnitOfWork _unitOfWork;

	//	#endregion

	//	#region Constructor

	//	public PatientEditController()
	//	{
	//		_unitOfWork = new UnitOfWork();
	//	}

	//	#endregion

	//	public ActionResult EditPatient(int? id)
	//	{
 //           ViewBag.PatientId = id;
	//		var patientModel = new PatientEditModel(id);
	//		//ViewBag.PatientJson = HttpUtility.HtmlEncode(patientModel.GetJson());

	//		return View("Index");
	//	}



	//	public ActionResult Get(int? id)
	//	{
	//		string json;
	//		dynamic expOb = new ExpandoObject();
	//		try
	//		{
	//			var model = new PatientEditModel(id);
	//			json = model.GetJson();
	//		}
	//		catch (Exception ex)
	//		{
	//			expOb.Errors = new List<ObjectError>() { new ObjectError("", ex.GetAllMessages()) };
	//			LogManager.GetLogger("Error PatientEditModel ids= " + id).Error(ex);
	//			json = JsonConvert.SerializeObject(expOb);
	//		}
	//		return Content(json, "application/json");
	//	}



	//	public ActionResult Save(int? id)
	//	{
	//		var model = new PatientEditModel(id);
	//		var bodyStream = new StreamReader(HttpContext.Request.InputStream);
	//		bodyStream.BaseStream.Seek(0, SeekOrigin.Begin);
	//		var bodyText = bodyStream.ReadToEnd();
	//		var errors = model.SetJson(bodyText);
	//		if (errors.Count == 0)
	//		{
	//			errors = model.Save(GetCurrentUser());
	//		};

	//		var json = JsonConvert.SerializeObject(new { Errors = errors });
	//		return Content(json, "application/json");
	//	}


	//}

}