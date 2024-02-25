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
	//public class HospitalEditController : BaseController
	//{
	//	#region fields

	//	private UnitOfWork _unitOfWork;


	//	private readonly ISamplesService _samplesService;

	//	private readonly ISamplesApprovedService _samplesApprovedService;

	//	#endregion

	//	#region Constructor

	//	public HospitalEditController()
	//	{
	//		_unitOfWork = new UnitOfWork();
	//	}

	//	#endregion

	//	public ActionResult EditHospital(int? id)
	//	{
 //           ViewBag.HospitalId = id;

	//		return View("Index");
	//	}




	//	public ActionResult Get(int? id)
	//	{
	//		string json;
	//		dynamic expOb = new ExpandoObject();
	//		try
	//		{
	//			var model = new HospitalEditModel(id);
	//			json = model.GetJson();
	//		}
	//		catch (Exception ex)
	//		{
	//			expOb.Errors = new List<ObjectError>() { new ObjectError("", ex.GetAllMessages()) };
	//			LogManager.GetLogger("Error HospitalEditModel ids= " + id).Error(ex);
	//			json = JsonConvert.SerializeObject(expOb);
	//		}
	//		return Content(json, "application/json");
	//	}





	//	public ActionResult Save(int? id)
	//	{
	//		var model = new HospitalEditModel(id);
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