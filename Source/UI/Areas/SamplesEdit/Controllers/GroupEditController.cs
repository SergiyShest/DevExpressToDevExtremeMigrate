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
using System.Web.Mvc;
using DataHelper = Sasha.Lims.Core.Helpers.DataHelper;

namespace Sasha.Lims.WebUI.Areas.SamplesEdit.Controllers
{
	public class GroupEditController : BaseSamplesController
	{
		#region fields

		private UnitOfWork _unitOfWork;


		private readonly ISamplesService _samplesService;

		private readonly ISamplesApprovedService _samplesApprovedService;

		#endregion

		#region Constructor

		public GroupEditController(ISamplesService samplesService, ISamplesApprovedService samplesApprovedService)
		{
			_samplesApprovedService = samplesApprovedService;

			_samplesService = samplesService;
			_unitOfWork = new UnitOfWork();

		}

		#endregion

		public ActionResult EditJournal(string ids, JournalType? draftMode)
		{
			ViewBag.draftMode = (int?)draftMode;
			ViewBag.IsEditDraft = true;
			ViewBag.ids = ids;
			return View("Index");
		}

		public ActionResult EditSample(string ids)
		{
			ViewBag.draftMode = null;
			ViewBag.IsEditDraft = false;
			var usr = base.GetCurrentUser();

			var samplesApprovedIds = CoreHelper.ConvertIdsToArr(ids).ToList();
			var serv = GroupSave.ToEdit(samplesApprovedIds,_unitOfWork, usr);

			ids = string.Join(",", serv);
			ViewBag.ids = ids;
			return View("Index");
		}

		public ActionResult Get(string ids)
		{
			string json;
			dynamic expOb = new ExpandoObject();
			try
			{
				var model = new GroupEditModel(ids, _unitOfWork);
				json = model.GetJson();
			}
			catch (Exception ex)
			{
				expOb.Errors = new List<ObjectError>() { new ObjectError("", ex.GetAllMessages()) };
				LogManager.GetLogger("Error GroupEditModel ids= " + ids).Error(ex);
				json = JsonConvert.SerializeObject(expOb);
			}
			return Content(json, "application/json");
		}

		public ActionResult Save(string ids, bool isEditDraft, JournalType? draftMode)
		{
			var model = new GroupEditModel(ids, _unitOfWork);
			var bodyStream = new StreamReader(HttpContext.Request.InputStream);
			bodyStream.BaseStream.Seek(0, SeekOrigin.Begin);
			var bodyText = bodyStream.ReadToEnd();
			var errors = model.SetJson(bodyText);
			if (errors.Count == 0)
			{

				errors = model.Save(GetCurrentUser(),draftMode);
				if (errors.Count == 0  && !isEditDraft)
				{
					errors = model.ApplayEdit(GetCurrentUser());
				} 
			};

			var json = JsonConvert.SerializeObject(new { Errors = errors });
			return Content(json, "application/json");
		}

		/// <summary>
		/// Получение всех докторов
		/// </summary>
		/// <returns></returns>
		public ActionResult GetDoctors()
		{
			var doctors = _unitOfWork.GetRepository<c_doctor>().GetAll().Select(x => new
			{
				Id = x.id,
				Name = x.firstName + " " + x.lastName
			});
			var json = JsonConvert.SerializeObject(doctors);
			return Content(json, "application/json");
		}

		/// <summary>
		/// получение типов пробирок 
		/// </summary>
		/// <returns></returns>
		public ActionResult GetTubeTypes()
		{
			var json = DataHelper.Current.GetTubeTypesJson();


			return Content(json, "application/json");
		}

		/// <summary>
		/// получение типов пробирок 
		/// </summary>
		/// <returns></returns>
		public ActionResult GetVolumeUnits()
		{
			var json = DataHelper.Current.GetVolumeUnitsJson();


			return Content(json, "application/json");
		}


		/// <summary>
		/// получение типов  
		/// </summary>
		/// <returns></returns>
		public ActionResult GetModels()
		{

			var json = DataHelper.Current.GetModelsJson();
			return Content(json, "application/json");
		}

		/// <summary>
		/// получение статусов  
		/// </summary>
		/// <returns></returns>
		public ActionResult GetStatuses()
		{
            var json = DataHelper.Current.GetStatusesJson();
			return Content(json, "application/json");
		}
		/// <summary>
		/// получение госпиталей
		/// </summary>
		/// <returns></returns>
		public ActionResult GetHospitals()
		{
			var items = _unitOfWork.GetRepository<c_client>().GetAll().Select(x => new
			{
				Id = x.id,
				Name = x.name
			});
			var json = JsonConvert.SerializeObject(items);
			return Content(json, "application/json");
		}

		/// <summary>
		/// получение Patients
		/// </summary>
		/// <returns></returns>
		public ActionResult GetPatients(string email, string patientNr,string firstName,string lastName)
		{
			var items = _unitOfWork.GetRepository<c_patient>().GetAll().Select(x => new
			{
				Id = x.id,
				Name = x.firstName+" "+x.lastName,
				FirstName = x.firstName,
				LastName = x.lastName,
                Email = x.email,
				No=x.no,
				BirthDay=x.dob,
				DoctorId=x.doctor_id
			});
			var json = JsonConvert.SerializeObject(items);
			return Content(json, "application/json");
		}

		/// <summary>
		/// получение Patients
		/// </summary>
		/// <returns></returns>
		public ActionResult GetPatient(int? patientId)
		{
			dynamic expOb = new ExpandoObject();
			try
			{
             var patient = _unitOfWork.GetRepository<c_patient>().GetAll().FirstOrDefault(x => x.id == patientId);
		      expOb.Patient= patient;
			}
			catch (NamedException ex)
			{
				expOb.Errors = new List<ObjectError>() { new ObjectError(ex.Name, ex.GetAllMessages()) };
				LogManager.GetLogger("Error GetPatient patientId= " + patientId).Error(ex);
			}
				var json = JsonConvert.SerializeObject(expOb);
			return Content(json, "application/json");
		}

		/// <summary>
		/// получение Расположения
		/// </summary>
		/// <returns></returns>
		public ActionResult GetLocationDiscr(int locationId)
		{
			var location = _unitOfWork.GetRepository<vPropLocation>().GetAll().FirstOrDefault(x => x.id == locationId);
			string json;
			if (location == null)
			{
				json = JsonConvert.SerializeObject(new { Name = "error", Path = "Location id=" + locationId + " not found!" });
			} else
			{
				json = JsonConvert.SerializeObject(new { Name = location.name, Path = location.treepath });
			}

			return Content(json, "application/json");
		}


		/// <summary>
		/// получение места
		/// </summary>
		/// <returns></returns>
		public ActionResult GetBatches()
		{
			var items = _unitOfWork.GetRepository<s_sample>().GetAll().Where(x => (x.model_id == SampleModel.Box || x.model_id == SampleModel.Plate) && x.status_id != SampleStatus.Disposed)
				.Select(x => new
				{
					Id = x.id,
					Name = x.name,
					Barcode = x.barcode,
					Type = x.Discriminator
				});
			var json = JsonConvert.SerializeObject(items);
			return Content(json, "application/json");
		}

		public JsonResult GetBarcode(int modelId, int? sampleTypeId)
		{
			SampleModel m = (SampleModel) modelId;

			var userId = DataHelper.GetUserId(User?.Identity?.Name);
			var barcode = DataHelper.Current.GetBarcode(userId, 0, m, sampleTypeId, TableType.JourLine);
			return Json(barcode, JsonRequestBehavior.AllowGet);
		}


	}

}