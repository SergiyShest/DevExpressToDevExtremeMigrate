using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Validation;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Sasha.Lims.Core.BusinessObjects;
using Sasha.Lims.Core.DTO.Access;
using Sasha.Lims.Core.DTO.Laboratory;
using Sasha.Lims.Core.Helpers;
using Sasha.Lims.Core.Interfaces.Customization;
using Sasha.Lims.Core.Interfaces.Laboratory;
using Sasha.Lims.DataAccess.Entities.Constants;
using Sasha.Lims.DataAccess.Entities.Entity;
using Sasha.Lims.DataAccess.Repositories;
using Sasha.Lims.DataAccess.Repositories.Abstract;
using Sasha.Lims.WebUI.Controllers;
using Sasha.Lims.WebUI.Infrastructure.Helpers;


namespace Sasha.Lims.WebUI.Areas.SamplesEdit.Controllers
{
	public class ContainerEditController : BaseController
	{
		// GET: SamplesEdit/PlateJournal
		public ActionResult Index(int? id, int? sampleId, string containerType)
		{
			IUnitOfWorkEx uow = new UnitOfWork();
			var repos = uow.GetRepository<s_jourLine>();
			if (sampleId != null)
			{
				var draftRec = repos.GetAll().FirstOrDefault(x => x.sample_id == sampleId);
				if (draftRec != null)
				{
					id = draftRec.id;
				} else
				{
					UserDTO user = GetCurrentUser();
					var sample = SampleManager.Current.GetSample((int) sampleId);
					var draft = sample.BeginEdit(user, JournalType.Box);

					draft.Save(user);
					id = draft.Id;
				}
			}
			if (id != null && string.IsNullOrWhiteSpace(containerType))
			{
				var model = repos.GetAll().FirstOrDefault(x => x.id == id)?.model_id;
				containerType = model.ToString();
				if (string.IsNullOrWhiteSpace(containerType))
				{
					model = uow.GetRepository<s_sample>().GetAll().FirstOrDefault(x => x.id == sampleId)?.model_id;
					containerType = model.ToString();
				}
			}
			ViewBag.ContainerType = containerType;
			ViewBag.ContainerId = id;
			return View();
		}


		/// <summary>
		/// Добавление в контейнер анализов по баркоду
		/// </summary>
		/// <param name="containerId"></param>
		/// <param name="barcode"></param>
		/// <returns></returns>
		public ActionResult AddByBarCode(int containerId, string barcode)
		{

			dynamic dynamic = new ExpandoObject();
			try
			{
				UserDTO user = GetCurrentUser();
				var containerDraft = SampleManager.Current.GetDraft((int) containerId) as BoxDraft;
				BaseDraft sample = containerDraft.AddByBarCode(user, barcode);
				dynamic.Sample = sample;
			}
			catch (Exception ex)
			{
				dynamic.Error = ex.Message;
			}

			//var sampleJson = JsonConvert.SerializeObject(dynamic);
			var sampleJson = JsonConvert.SerializeObject(dynamic, Formatting.Indented,
					new JsonSerializerSettings { ContractResolver = new DynamicContractResolver(new List<string>() { "Id", "Name", "Barcode", "Well" }) });
			return Content(sampleJson, "application/json");
		}

		/// <summary>
		/// чтение контейнера
		/// </summary>
		/// <param name="containerId"></param>
		/// <returns></returns>
		public ActionResult GetContainer(int containerId)
		{
			dynamic dynamic = new ExpandoObject();
			try
			{
				var containerDraft = SampleManager.Current.GetDraft((int) containerId) as BoxDraft;
				dynamic.Container = containerDraft;
			}
			catch (Exception ex)
			{
				dynamic.Error = ex.Message;
			}
			var sampleJson = JsonConvert.SerializeObject(dynamic, Formatting.Indented,
				new JsonSerializerSettings { ContractResolver = new DynamicContractResolver(new List<string>() { "Id", "Name", "Barcode", "Well", "ContentList", "TubeTypeId", "SampleTypeId", "WellDescription" }) });
			return Content(sampleJson, "application/json");
		}

		/// <summary>
		/// Сохранение контейнера
		/// </summary>
		/// <param name="containerId"></param>
		/// <param name="containerType"></param>
		/// <param name="commit"></param>
		/// <returns></returns>
		public ActionResult SaveContainer(int? containerId, string containerType, bool? commit)
		{
			dynamic dynamic = new ExpandoObject();
			dynamic.Error = string.Empty;
			dynamic.ContainerId = string.Empty;

			try
			{
				UserDTO user = GetCurrentUser();
				BoxDraft containerDraft = null;
				if (containerId == null)
				{
					switch (containerType)
					{
						case "Box": containerDraft = new BoxDraft(); break;
						case "Plate": containerDraft = new PlateDraft(); break;
						default: throw new NotImplementedException("A 'Box' or 'Plate' was expected but there was a '" + containerType + "'");
					}
				} else
				{
					containerDraft = SampleManager.Current.GetDraft((int) containerId) as BoxDraft;
				}

				var bodyStream = new StreamReader(HttpContext.Request.InputStream);
				bodyStream.BaseStream.Seek(0, SeekOrigin.Begin);
				var bodyText = bodyStream.ReadToEnd();
				if (!string.IsNullOrWhiteSpace(bodyText))
				{
					if (containerDraft.ContentList != null)
						containerDraft.ContentList.Clear();
					JsonConvert.PopulateObject(bodyText, containerDraft);
					if (containerDraft.ContentList != null)
						foreach (var cont in containerDraft.ContentList)
						{
							var sItem = cont as BaseDraft;
							if (sItem != null)
							{
								cont.Record.well = sItem.Well;
							} else
							{
								cont.Record.well = null;
							}
						}
				}
				containerDraft.Save(user);
				if (commit == true)
				{
					containerDraft.ApplyEdit(user);
				}
				dynamic.ContainerId = containerDraft.Id;
				dynamic.Barcode = containerDraft.Barcode;
				dynamic.WellDescription = containerDraft.WellDescription;
			}
			catch (DbEntityValidationException validationErr)
			{
				StringBuilder sb = new StringBuilder();
				foreach (DbEntityValidationResult validationError in validationErr.EntityValidationErrors)//Заполнение массива ошибками
				{
					foreach (DbValidationError err in validationError.ValidationErrors)//Заполнение массива ошибками
					{
						sb.AppendLine(err.ErrorMessage);
					}
				}
				dynamic.Error = sb.ToString();
			}
			catch (Exception ex)
			{
				dynamic.Error = ex.Message;
			}
			var sampleJson = JsonConvert.SerializeObject(dynamic);
			return Content(sampleJson, "application/json");
		}
		/// <summary>
		/// Удаление черновика
		/// </summary>
		/// <param name="containerId"></param>
		/// <returns></returns>
		public ActionResult CancelEdit(int? containerId)
		{
			dynamic dynamic = new ExpandoObject();
			dynamic.Error = string.Empty;
			dynamic.Меssage = string.Empty;
			dynamic.ContainerId = string.Empty;
			try
			{
				var containerDraft = SampleManager.Current.GetDraft((int) containerId) as BoxDraft;
				if (containerDraft == null)
				{
					dynamic.Меssage = "";

				} else
				{
					UserDTO user = GetCurrentUser();
					containerDraft.Delete(user);
				}

			}
			catch (Exception ex)
			{
				dynamic.Error = ex.Message;
			}
			var sampleJson = JsonConvert.SerializeObject(dynamic);
			return Content(sampleJson, "application/json");
		}

		/// <summary>
		/// Удаление анализа из контейнера
		/// </summary>
		/// <param name="containerId"></param>
		/// <returns></returns>
		public ActionResult DeleteItemFromContainer(int containerId, string selectedIds)
		{
			dynamic dynamic = new ExpandoObject();
			dynamic.Error = string.Empty;
			dynamic.Меssage = string.Empty;
			dynamic.ContainerId = string.Empty;
			try
			{
				var containerDraft = SampleManager.Current.GetDraft(containerId) as BoxDraft;
				if (containerDraft == null)
				{
					dynamic.Меssage = "";

				} else
				{
					UserDTO user = GetCurrentUser();
					var ids = CoreHelper.ConvertIdsToArr(selectedIds);
					containerDraft.DeleteItems(ids, user);
				}

			}
			catch (Exception ex)
			{
				dynamic.Error = ex.Message;
			}
			var sampleJson = JsonConvert.SerializeObject(dynamic);
			return Content(sampleJson, "application/json");
		}


		/// <summary>
		/// получение типов пробирок допустимых для данного типа
		/// </summary>
		/// <param name="containerType"></param>
		/// <returns></returns>
		public ActionResult GetTubeTypes(string containerType)
		{
			Type contType;

			switch (containerType)
			{
				case "Box": contType = typeof(BoxDraft); break;
				case "Plate": contType = typeof(PlateDraft); break;
				default: throw new NotImplementedException("A 'Box' or 'Plate' was expected but there was a '" + containerType + "'");
			}

			var model = SampleManager.Current.GetAvaiableContainerTypes(contType);

			var json = JsonConvert.SerializeObject(model);

			return Content(json, "application/json");
		}



		/// <summary>
		/// Получение типов анализов
		/// </summary>
		/// <returns></returns>
		public ActionResult GetSampleTypes()
		{
			var model = SampleManager.Current.GetProperties(PropsType.SampleTypes);
			var json = JsonConvert.SerializeObject(model);
			return Content(json, "application/json");
		}


	}
}