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
	public class EditSampleController : BaseSamplesController
	{
		#region fields

		private UnitOfWork _unitOfWork;

		private readonly IPropertiesService _propertiesService;
		private readonly ISamplesService _samplesService;
		private readonly ISamplesApprovedService _samplesApprovedService;

		#endregion
		#region Constructor

		public EditSampleController(ISamplesService samplesService, IPropertiesService propertiesService, ISamplesApprovedService samplesApprovedService)
		{
			_samplesApprovedService = samplesApprovedService;
			_propertiesService = propertiesService;
			_samplesService = samplesService;
			_unitOfWork = new UnitOfWork();

		}

		#endregion

		#region GroupInput

		public ActionResult GroupInput(string selectedIds, int journalType)
		{
			var ids = CoreHelper.ConvertIdsToArr(selectedIds);

			ViewBag.SelectedIds = selectedIds;
			ViewBag.EditingRowCount = ids.Count;
			ViewBag.JournalType = journalType;
			return View("GroupInput");
		}

		public JsonResult GroupInputSave(string selectedIds, int journalType)
		{
			var usr = GetCurrentUser();
			var bodyStream = new StreamReader(HttpContext.Request.InputStream);
			bodyStream.BaseStream.Seek(0, SeekOrigin.Begin);
			var bodyText = bodyStream.ReadToEnd();

			var dto = Newtonsoft.Json.JsonConvert.DeserializeObject<GroupInputDto>(bodyText);
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

				res.Add(dto);
			}

			res=res.OrderBy(x => x.Name).ToList();

			return Json(res, JsonRequestBehavior.AllowGet);
		}

		#endregion

		#region Edit

		public ActionResult SampleEdit(int? id, JournalType editMode)
		{
			ViewBag.EditMode = editMode;
			var sample = id == null ? null : _samplesService.Get((int) id);
			@ViewBag.Id = id;
			ViewBag.ExistsSample = false;

			if (sample == null)
				return View("NewSample", null);

			JournalType jourType = _samplesService.GetSampleJourType(id.Value);
			ViewBag.EditMode = jourType;
			if (jourType != JournalType.Receiving && !ConfigVariables.AllowEditingExistsSamples)
				ViewBag.ExistsSample = true;

			int? batchId = sample?.BatchId;
			if (null != batchId)
			{
				var batch = _samplesApprovedService.Get(batchId.Value);
				ViewBag.ContainerModel = batch?.ModelId;
			}

			ViewBag.Close = false;
			return View("SampleEdit", Mapper.Map<SampleViewModel>(sample));
		}

		public ActionResult SampleSave(JournalType editMode, SampleViewModel sample)
		{
			ViewBag.EditMode = editMode;

			ViewBag.ExistsSample = false;
			@ViewBag.Id = sample.Id;
			UserDTO usr = GetCurrentUser();
			sample.AspNetUserId = usr?.Id ?? "zero";
			sample.UserId = usr?.UserId.GetValueOrDefault(0) ?? 0;

			if (editMode != JournalType.Receiving && !ConfigVariables.AllowEditingExistsSamples)
				ViewBag.ExistsSample = true;

			int? batchId = sample?.BatchId;
			if (null != batchId)
			{
				var batch = _samplesApprovedService.Get(batchId.Value);
				ViewBag.ContainerModel = batch?.ModelId;
			}

			if (sample.Id == null)
			{

				return UpdateModelWithDataValidation(Mapper.Map<SampleDTO>(sample), usr, (a, u) => { _samplesService.CreateSample(a, u); });
			} else
			{
				return UpdateModelWithDataValidation(Mapper.Map<SampleDTO>(sample), usr, _samplesService.EditSample);
			}

		}

		private ActionResult UpdateModelWithDataValidation(SampleDTO sample, UserDTO usr, Action<SampleDTO, UserDTO> updateMethod)
		{

			ViewBag.Close = true;
			if (ModelState.IsValid)
			{
				SafeExecute(() => updateMethod(sample, usr));
				if (!string.IsNullOrWhiteSpace(ViewBag.GeneralError))
				{
					ViewBag.Close = false;
				}
			} else
			{
				ViewBag.GeneralError = "Please, correct all errors.";
			}
			return View("SampleEdit", Mapper.Map<SampleViewModel>(sample));
		}

		#endregion

		#region Locations

		public ActionResult LocationsTreeList(int? draftId, int? sampleId, int? locationId)
		{
			int? id = null;
			if (locationId != null)
			{
				id = locationId;
			} else
			  if (draftId != null)
			{
				var draft = _samplesService.Get((int) draftId);
				id = draft.LocationId;
			} else
			  if (sampleId != null)
			{
				var sample = _samplesApprovedService.Get((int) sampleId);
				id = sample.LocationId;
			}
			ViewBag.LocationSelectedId = id;
			return View("LocationsTreeList");
		}

		public PartialViewResult LocationsTreeListPartial(int? selectedId)
		{
			var locations = _propertiesService.GetLocations(); //.Select(x=> new PropertyViewModel(){ ParentId = x.ParentId, Id = x.Id, Code = x.Code, Value = x.Value, Name = x.Name});

			//init short path
			var rootNode = locations.FirstOrDefault(x => x.ParentId == null);
			rootNode.Path = string.Empty;
			InitPropertyShortPath(rootNode.Id, locations, 0);

			ViewBag.LocationSelectedId = selectedId;
			return PartialView("_LocationTreeListPartial", locations);
		}

		public ActionResult GpLocationTreeListPartial(int? selectedId)
		{
			var locations = _propertiesService.GetLocations();

			//init short path
			var rootNode = locations.FirstOrDefault(x => x.ParentId == null);
			rootNode.Path = string.Empty;
			InitPropertyShortPath(rootNode.Id, locations, 0);

			ViewBag.LocationSelectedId = selectedId;
			return PartialView("_GpLocationTreeListPartial", locations);
		}

		protected static void InitPropertyShortPath(int nodeId, IEnumerable<PropertyDTO> props, int level)
		{
			const int shortLevel = 10;
			if (props != null)
			{
				var curNode = props.FirstOrDefault(x => x.Id == nodeId) ?? new PropertyDTO();
				var childNodes = props.Where(x => x.ParentId == nodeId);

				foreach (var childNode in childNodes)
				{
					childNode.Path = level <= shortLevel ? curNode.Path + (string.IsNullOrWhiteSpace(curNode.Path) ? "" : "->") + childNode.Name : curNode.Path;
					InitPropertyShortPath(childNode.Id, props, level++);
				}
			}

		}
		#endregion

		#region new Edit
		public ActionResult Edit(int? id, SampleModel? modelId, JournalType editMode)
		{
			ViewBag.EditMode = editMode;


			if (modelId == null && id != null)
			{
				IUnitOfWorkEx uow = new UnitOfWork();
				var rep = uow.GetRepository<s_jourLine>();
				var line = rep.GetAll().FirstOrDefault(x => x.id == id);
				modelId = line.model_id;
			}

			ViewBag.modelId = modelId;
			ViewBag.rowId = id;
			BaseDraft model = null;
			switch (modelId)
			{
				case SampleModel.Sample:
					model = SampleManager.Current.GetSampleDraft(id);
					return View("Edit/SampleDtaft", model as Sasha.Lims.Core.BusinessObjects.SampleDraft);// 
					break;
				case SampleModel.Aliquot:
					model = SampleManager.Current.GetAliquotDraft(id);
					return View("Edit/AliquotDtaft", model as Sasha.Lims.Core.BusinessObjects.AliquoteDraft);
					break;
				case SampleModel.Plate:
					//					model = _draftManager.GetPlate(id);

					return View("Edit/PlateDtaftVue");
					break;
				case SampleModel.Box:
					model = SampleManager.Current.GetBoxDraft(id);
					return View("Edit/BoxDtaft", model as Sasha.Lims.Core.BusinessObjects.BoxDraft);
					break;

			}

			string err = modelId + "  not implemented";
			throw new NotImplementedException(err);


		}

		public ActionResult GetDraft(int id)
		{
			BaseDraft model = SampleManager.Current.GetDraft(id);

			var json = JsonConvert.SerializeObject(model);

			return Content(json, "application/json");
		}

		public ActionResult SaveDraftVue(int id)
		{
			var bodyStream = new StreamReader(HttpContext.Request.InputStream);
			bodyStream.BaseStream.Seek(0, SeekOrigin.Begin);
			var bodyText = bodyStream.ReadToEnd();
			var changed = false;
			string res = null;
			List<DbEntityValidationResult> errors = new List<DbEntityValidationResult>();
			try
			{

				BaseDraft model = SampleManager.Current.GetDraft(id);
				JsonConvert.PopulateObject(bodyText, model);
				changed = _unitOfWork.DbContext.ChangeTracker.HasChanges();
				if (changed)
				{
					UserDTO user = GetCurrentUser();
					model.Save(user);
					changed = false;
				}
			}
			catch (DbEntityValidationException ex)
			{
				errors.AddRange(ex.EntityValidationErrors);
			}
			catch (ValidationException ex)
			{
				res = GetErrorsAndChanged(null, ex, changed);
			}
			catch (Exception e)
			{
				res = GetErrorsAndChanged(null, e, changed);
			}
			if (res == null)
			{
				res = GetErrorsAndChanged(errors, null, changed).ToString();
			}
			return Content(res, "application/json");

		}



		private string GetErrorsAndChanged()
		{
			var changed = _unitOfWork.DbContext.ChangeTracker.HasChanges();
			var errors = _unitOfWork.DbContext.GetValidationErrors();
			return GetErrorsAndChanged(errors, null, changed);
		}

		private static string GetErrorsAndChanged(IEnumerable<DbEntityValidationResult> errors, Exception ex, bool changed)
		{
			dynamic dynamic = new ExpandoObject();
			dynamic.IsChanged = changed;//Создание свойства IsChanged
			var errProperty = new Dictionary<string, object>();//Создание массива с будущими свойсвтвами ошибки
			dynamic.Errors = new DynObject(errProperty);//Создание объекта у которого свойства задаются в массиве
			if (errors != null)
				foreach (DbEntityValidationResult validationError in errors)//Заполнение массива ошибками
				{
					foreach (DbValidationError err in validationError.ValidationErrors)//Заполнение массива ошибками
					{
						errProperty.Add(err.PropertyName, err.ErrorMessage);
					}
				}
			else if (ex != null)
			{
				errProperty.Add("GlobalErrors", ex.Message);
			}

			var json = JsonConvert.SerializeObject(dynamic); return json;
		}

		public ActionResult GetTubeTypes(string modelId)
		{
			var model = DataHelper.Current.TubeTypes;

			var json = JsonConvert.SerializeObject(model);

			return Content(json, "application/json");
		}


		#region old


		public ActionResult SaveSampleDraft(Sasha.Lims.Core.BusinessObjects.SampleDraft sample, JournalType editMode)
		{
			return SaveDraft(sample, editMode);
		}
		public ActionResult SaveAliquoteDraft(Sasha.Lims.Core.BusinessObjects.AliquoteDraft sample, JournalType editMode)
		{
			return SaveDraft(sample, editMode);
		}
		public ActionResult SaveBoxDraft(Sasha.Lims.Core.BusinessObjects.BoxDraft sample, JournalType editMode)
		{
			return SaveDraft(sample, editMode);
		}
		public ActionResult SavePlateDraft(Sasha.Lims.Core.BusinessObjects.PlateDraft sample, JournalType editMode)
		{
			return SaveDraft(sample, editMode);
		}


		public ActionResult SaveDraft(BaseDraft requestDraft, JournalType editMode)
		{
			ViewBag.EditMode = editMode;
			if (!ModelState.IsValid)
			{
				StringBuilder errors = new StringBuilder();
				foreach (var val in ModelState.Values)
				{
					foreach (var error in val.Errors)
					{
						errors.AppendLine(error.ErrorMessage);
					}
				}
				ViewBag.GeneralError = errors.ToString();
			} else
			{
				try
				{
					requestDraft.JourTypeId = editMode;
					var user = GetCurrentUser();
					if (requestDraft.Id != null)
					{

						var draft = SampleManager.Current.GetDraft((int) requestDraft.Id);

						Debug.WriteLine("Mapping:");
						CoreHelper.MapRequestProperties(requestDraft, draft, Request.Params.Keys);
						requestDraft = draft;
					}

	                 requestDraft.Save(user);

				}
				catch (System.Data.Entity.Validation.DbEntityValidationException ex)
				{
					StringBuilder sb = new StringBuilder();

					foreach (var errors in ex.EntityValidationErrors)
					{
						foreach (var error in errors.ValidationErrors)
						{
							sb.AppendLine(error.PropertyName + " " + error.ErrorMessage);
						}
					}

					ViewBag.GeneralError = sb.ToString();
				}

				catch (Exception ex)
				{
					var t = ex.GetType();
					ViewBag.GeneralError = ex.Message;
				}

			}
			if (!string.IsNullOrWhiteSpace(ViewBag.GeneralError))
			{
				ViewBag.Close = false;
			} else
			{
				ViewBag.Close = true;
			}

			@ViewBag.Id = requestDraft.Id;
			switch (requestDraft.ModelId)
			{
				case SampleModel.Sample:

					return View("Edit/SampleDtaft", requestDraft as Sasha.Lims.Core.BusinessObjects.SampleDraft);
					break;
				case SampleModel.Aliquot:

					return View("Edit/AliquotDtaft", requestDraft as Sasha.Lims.Core.BusinessObjects.AliquoteDraft);
					break;
				case SampleModel.Plate:

					return View("Edit/PlateDtaft", requestDraft as Sasha.Lims.Core.BusinessObjects.PlateDraft);
					break;
				case SampleModel.Box:

					return View("Edit/BoxDtaft", requestDraft as Sasha.Lims.Core.BusinessObjects.BoxDraft);
					break;
				default:
					throw new NotImplementedException();

			}
		}



	#endregion
		#endregion

		public JsonResult Delete(string selectedIds)
		{
			string res = "";
			try
			{
				var ids = CoreHelper.ConvertIdsToArr(selectedIds);
				_samplesService.DeleteSamples(ids, GetCurrentUser().Id);
			}
			catch (Exception ex)
			{
				res = ex.Message;
			}
			return Json(res, JsonRequestBehavior.AllowGet);
		}

		#region Batch


		public IEnumerable<SampleApprovedViewModel> GetBatchesRangeNonDisposed(ListEditItemsRequestedByFilterConditionEventArgs args) => GetBatchesRange(args);

		public IEnumerable<SampleApprovedViewModel> GetBatchesRangeNonDisposedBoxes(ListEditItemsRequestedByFilterConditionEventArgs args) => GetBatchesRange(args, new SampleModel?[] { SampleModel.Box });


		private IEnumerable<SampleApprovedViewModel> GetBatchesRange(ListEditItemsRequestedByFilterConditionEventArgs args, IEnumerable<SampleModel?> validContainers = null, bool includeDisposed = false)
		{
			var devExFilterStr = string.Empty;

			if (!string.IsNullOrWhiteSpace(args?.Filter))
			{
				devExFilterStr = $"Contains([Name], '{args.Filter}') Or Contains([Barcode], '{args.Filter}')";
			}

			var skip = args.BeginIndex;
			var take = args.EndIndex - skip + 1;

			if (skip > 0)
				skip--;
			else
				take--;

			IEnumerable<SampleApprovedDTO> containers = _samplesApprovedService.GetContainers(new PageParams { RowCount = take, StartRowIndex = skip },
				new SortParams { Ascending = true, OrderBy = "Name" }, includeDisposed, validContainers, devExFilterStr);

			var result = Mapper.Map<IEnumerable<SampleApprovedViewModel>>(containers);
			if (skip == 0)
				result = Enumerable.Prepend(result, emptyBatchViewModel);

			return result;
		}


		private static readonly SampleApprovedViewModel emptyBatchViewModel = new SampleApprovedViewModel() { Name = "None", Barcode = "not in batch", Id = null };
		public SampleApprovedViewModel GetBatchById(ListEditItemRequestedByValueEventArgs args)
		{
			if (args.Value == null || !int.TryParse(args.Value.ToString(), out var id))
				return null;
			return id == 0 ? emptyBatchViewModel : Mapper.Map<SampleApprovedViewModel>(_samplesApprovedService.Get(id));
		}

		#endregion


		public JsonResult GetBarcode(string model, int? sampleTypeId, int? hospitalId)
		{
			SampleModel m;
			SampleModel.TryParse(model, out m);
			var userId = DataHelper.GetUserId(User?.Identity?.Name);
			var barcode = DataHelper.Current.GetBarcode(userId, 0, m, sampleTypeId, TableType.JourLine);
			return Json(barcode, JsonRequestBehavior.AllowGet);
		}
	}


}