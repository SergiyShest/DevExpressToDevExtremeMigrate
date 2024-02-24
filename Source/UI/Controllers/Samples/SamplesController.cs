using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using AutoMapper;
using DevExpress.Data;
using DevExpress.Office.Utils;
using DevExpress.Web;

using DevExpress.Web.Mvc;
using NLog;
using Sasha.Lims.Core.DTO.Access;
using Sasha.Lims.Core.DTO.Common;
using Sasha.Lims.Core.DTO.Laboratory;
using Sasha.Lims.Core.Helpers;
using Sasha.Lims.Core.Infrastructure.Classes;
using Sasha.Lims.Core.Infrastructure.Extentions;
using Sasha.Lims.Core.Interfaces.Common;
using Sasha.Lims.Core.Interfaces.Customization;
using Sasha.Lims.Core.Interfaces.Laboratory;
using Sasha.Lims.DataAccess.Entities.Constants;
using Sasha.Lims.DataAccess.Entities.Entity;
using Sasha.Lims.Core.Settings;
using Sasha.Lims.Exceptions;
using Sasha.Lims.WebUI.Infrastructure.Helpers;
using Sasha.Lims.WebUI.Models;

namespace Sasha.Lims.WebUI.Controllers
{
	[Authorize]
	public class SamplesController : GridBasedController
	{
		private static NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
		private readonly IPropertiesService _propertiesService;
		private readonly ISamplesService _samplesService;
		private readonly ISamplesApprovedService _samplesApprovedService;

		public const string GridViewName = "SamplesGridView";
		public override string GetGridViewName() => GridViewName;

		public SamplesController(ISamplesService samplesService, IPropertiesService propertiesService, ISamplesApprovedService samplesApprovedService, ILocationsService locationsService,
								 IDtoMetadataService metadataService) : base(metadataService.GetUserFieldsForViewModel<SampleDTO>())
		{
			_samplesService = samplesService;
			_samplesService.SetAccessibleJourTypes(new[] { JournalType.Management, JournalType.Receiving });
			_propertiesService = propertiesService;
			_samplesApprovedService = samplesApprovedService;
			LocationsService = locationsService;
		}
		// GET: Samples
		public ActionResult Index(JournalType editMode = JournalType.Management,bool dontShowMainMenu=false)
		{
			ViewBag.DontShowMainMenu = dontShowMainMenu;
			SetAccessibleJourTypes(editMode);
			return View("Index");
		}

		#region Setup
		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			_samplesService.SetUserFilter(GetCurrentUser().UserId.GetValueOrDefault(0));
			base.OnActionExecuting(filterContext);
		}

		private JournalType GetAccessibleJourTypesFromGridView() => Enum.TryParse(GridViewExtension.GetEditValue<string>("editMode"), out JournalType jourType) ? jourType : JournalType.Management;

		private static readonly JournalType[] noEditSampleModes = { JournalType.MassCheckOut, JournalType.MassCheckIn, JournalType.MassMove, JournalType.MassDispose, JournalType.Box };
		private void SetAccessibleJourTypes(JournalType jourType)
		{
			_samplesService.SetAccessibleJourTypes(jourType);

			ViewBag.EditMode = jourType;
			ViewBag.CanEditSamples = !noEditSampleModes.Contains(jourType);
		}
		#endregion
		#region Mass Processing: location and status
		[HttpPost]
		[ActionName("Dispose")]
		public ActionResult DisposeSamples() => AlterRecords(JournalType.MassDispose, _samplesService.SetStatusExecutor, (SampleStatus?) SampleStatus.Disposed);

		[HttpPost]
		public ActionResult CheckOut() => AlterRecords(JournalType.MassCheckOut, _samplesService.SetStatusExecutor, (SampleStatus?) SampleStatus.CheckedOut);

		[HttpPost]
		public ActionResult Move(int locationId) => AlterRecords(JournalType.MassMove, _samplesService.SetLocationExecutor, (int?) locationId);

		[HttpPost]
		public ActionResult CheckIn(int locationId) => AlterRecords(JournalType.MassCheckIn, _samplesService.CheckInExecutor, (int?) locationId);

		private ActionResult AlterRecords<T>(JournalType editMode, Action<s_jourLine, T> mutator, T value)
		{
			SetAccessibleJourTypes(editMode);
			_samplesService.AlterEditingSamples(editMode, GetCurrentUser(), mutator, value);
			return RedirectToAction("Index", new { editMode });
		}
		#endregion

		#region Mass processing from toolbar
		public ActionResult ChangeSamplesGroupDialog(string customAction, SampleStatus? sampleStatusIdForGroup, int? gpLocationId, int? batchId, bool? aliquot, bool? sendReport, JournalType editMode = JournalType.Management)
		{
			switch (customAction)
			{
				case "StatusChange":
					if (sampleStatusIdForGroup.HasValue && sampleStatusIdForGroup > 0)
						AlterRecords(editMode, _samplesService.SetStatusExecutor, sampleStatusIdForGroup);
					break;

				case "LocationChange":
					if (gpLocationId.HasValue && gpLocationId > 0)
						AlterRecords(editMode, _samplesService.SetLocationExecutor, gpLocationId);
					break;

				case "BatchChange":
					if (batchId.GetValueOrDefault(0) <= 0)
						batchId = null;
					AlterRecords(editMode, _samplesService.SetBatchExecutor, batchId);
					break;

				case "AliquoteChange":
					AlterRecords(editMode, (j, b) => j.makeAliquot = true.Equals(b), aliquot);
					break;

				case "SendReport":
					AlterRecords(editMode, (j, b) => j.emailReport = true.Equals(b), sendReport);
					break;
			}

			return RedirectToAction("Index", new { editMode });
		}
		#endregion

		#region GridView
		#region View List
		protected override GridViewModel CreateGridViewModelImpl()
		{
			var viewModel = new GridViewModel { KeyFieldName = nameof(SampleViewModel.Id) };
			viewModel.Columns.Add(nameof(SampleViewModel.Barcode));
			viewModel.Columns.Add(nameof(SampleViewModel.Name));
			viewModel.Columns.Add(nameof(SampleViewModel.CollectedDateTime));
			viewModel.Columns.Add(nameof(SampleViewModel.ModelId));
			viewModel.Columns.Add(nameof(SampleViewModel.TubeTypeId));
			viewModel.Columns.Add(nameof(SampleViewModel.SampleTypeId));
			viewModel.Columns.Add(nameof(SampleViewModel.SampleStatusId));
			viewModel.Columns.Add(nameof(SampleViewModel.LocationId));

			return viewModel;
		}

		protected override PartialViewResult GridCustomActionCore(GridViewModel gridViewModel)
		{
			SetAccessibleJourTypes(GetAccessibleJourTypesFromGridView());
			return base.GridCustomActionCore(gridViewModel);
		}
		#endregion

		#region Get Data methods
		protected override void GetDataRowCount(GridViewCustomBindingGetDataRowCountArgs e) => e.DataRowCount = _samplesService.SamplesCount();

		protected override void GetData(GridViewCustomBindingGetDataArgs e)
		{
			var paging = new PageParams { StartRowIndex = e.StartDataRowIndex, RowCount = e.DataRowCount };
			var sorting = e.State.SortedColumns.Count == 0 ? new SortParams { OrderBy = "Id", Ascending = false }
				: new SortParams
				{
					OrderBy = Mapper.Instance.GetDestinationPropertyFor<SampleViewModel, SampleDTO>(e.State.SortedColumns.FirstOrDefault().FieldName),
					Ascending = !e.State.SortedColumns.FirstOrDefault().SortOrder.HasFlag(ColumnSortOrder.Descending)
				};

			e.Data = Mapper.Map<IEnumerable<SampleViewModel>>(_samplesService.GetSamples(paging, sorting));
		}
		#endregion

		#region Populate List
		public ActionResult AddSamplesByBarCode(string samplesBarcodes, string fieldName, string selectedIds, JournalType editMode = JournalType.Management)
		{
			AddSamplesByBarCode(samplesBarcodes, selectedIds, editMode);

			return RedirectToAction("Index", new { editMode = (int) editMode });
		}

		protected void AddSamplesByBarCode(string samplesBarcodes, string selectedIds, JournalType editMode)
		{
			var barcodes = samplesBarcodes?.Replace(" ", "").Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

			if (barcodes?.Length > 0)
			{
				UserDTO usr = GetCurrentUser();
				if (string.IsNullOrWhiteSpace(selectedIds))
				{
					switch (editMode)
					{
						case JournalType.Receiving:
							_samplesService.CloneLastEditedSampleWithBarcodes(barcodes, usr)?.ToList();
							break;

						case JournalType.Aliquoting:
							_samplesApprovedService.ToAliquotingMode(barcodes, editMode, usr);
							break;

						case JournalType.Management:
						case JournalType.MassCheckIn:
						case JournalType.MassCheckOut:
						case JournalType.MassMove:
						case JournalType.MassDispose:
							_samplesApprovedService.ToEditMode(barcodes, usr, editMode);
							break;

						default:
							break;
					}
				}
			}
		}

		public ActionResult FindSampleByBarcode(string barcode, JournalType? jourType)
		{


			SampleApprovedDTO dto = FindSampleByBarcodeInternal(barcode, jourType);

			object jsonData;
			if (dto != null)
				jsonData = new { _status = 200, requestKey = barcode, barcode = dto.Barcode, model = dto.ModelId.ToString() };
			else
				jsonData = new { _status = 404, requestKey = barcode };

			return new JsonResult() { Data = jsonData, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}

		protected SampleApprovedDTO FindSampleByBarcodeInternal(string barcode, JournalType? jourType)
		{
			SampleApprovedDTO dto;
			if (jourType.HasValue)
			{
				UserDTO usr = GetCurrentUser();
				string[] barcodes = new[] { barcode };

				switch (jourType)
				{
					case JournalType.Receiving:
						dto = _samplesService.CloneLastEditedSampleWithBarcodes(barcodes, usr).FirstOrDefault();
						break;

					case JournalType.Aliquoting:
						dto = _samplesApprovedService.ToAliquotingMode(barcodes, jourType.Value, usr).FirstOrDefault();
						break;

					case JournalType.Management:
					case JournalType.MassCheckIn:
					case JournalType.MassCheckOut:
					case JournalType.MassMove:
					case JournalType.MassDispose:
						dto = _samplesApprovedService.ToEditMode(barcodes, usr, jourType.Value).FirstOrDefault();
						break;

					default:
						dto = null;
						break;
				}
			} else
				dto = _samplesApprovedService.GetByBarcode(barcode);

			return dto;
		}

		#endregion

		#region Processing
		//[ValidateAntiForgeryToken]
		public ActionResult CustomActionPartial(string customAction, string customParams)
		{
			var selectedRowIds = Request.Params["SelectedRows"];
			switch (customAction)
			{
				case "delete":
					SafeExecute(() =>
					{
						if (!string.IsNullOrEmpty(selectedRowIds))
						{
							_samplesService.DeleteSamples(selectedRowIds.Split(',').ToList().ConvertAll(id => int.Parse(id)), GetCurrentUser().Id);
						}
					});
					break;

				#region Закрыто по задаче LIMS-300
				//case "export":
				//	// I cannot query data and send it right here, because devexpress JS requires me to send a partial view with grid in response.
				//	// However I can pass an export command with parameters from here, so that it executes when partial is injected 
				//  var viewModel = GridViewExtension.GetViewModel(GetGridViewName());
				//	//ViewBag.Export viewModel.FilterExpression;
				//	//viewModel.SortedColumns
				//	break;
				#endregion

				case "process":
					SafeExecute(() => _samplesService.PostJournal(GetAccessibleJourTypesFromGridView(), GetCurrentUser(), new List<int>()));
					break;

				case "assignWells":
					SafeExecute(() => _samplesService.AssignWells(new List<int>(), (int) GetCurrentUser().UserId));
					break;

				case "print":
					if (!string.IsNullOrEmpty(selectedRowIds))
					{
						return RedirectToAction("Journal", "Print", new { ids = selectedRowIds });
					}
					break;
				case "clear":
					{
						UserDTO usr = GetCurrentUser();
						_samplesService.ClearJournal(GetAccessibleJourTypesFromGridView(), usr.UserId.GetValueOrDefault(0));
						break;
					}
			}
			return GridViewPartial();
		}


		protected override void SetErrorText(Exception ex)
		{var barcodes = new StringBuilder();
			if ((ex as LimsException)?.Code== (int)LimsExceptionType.DbUpdateConcurrencyException)
			{
				var entries = ((ex as LimsException).InnerException as DbUpdateConcurrencyException).Entries.ToList();
				
				entries.ForEach(getBarcode);
				var ends=entries.Count>1?"s":string.Empty;

				ViewBag.GeneralError = $"Sample {ends} {barcodes} has been changed by another user. Delete it and add again.";
			} else
			{
				ViewBag.GeneralError = ex.Message;
			}

			void getBarcode(DbEntityEntry sample)
			{
				var barcode = sample.Entity as s_sample;

				if (barcode != null)
				{
					if (barcodes.Length > 0) barcodes.Append(",");
					barcodes.Append(barcode.barcode);
				}
			}

		}

		public FileResult Export(string sortColumn, ColumnSortOrder? sortOrder, JournalType editMode)
		{
			var sorting = string.IsNullOrWhiteSpace(sortColumn) ? new SortParams { OrderBy = "Id" }
				: new SortParams
				{
					OrderBy = Mapper.Instance.GetDestinationPropertyFor<SampleViewModel, SampleDTO>(sortColumn),
					Ascending = sortOrder.GetValueOrDefault(ColumnSortOrder.Ascending) == ColumnSortOrder.Ascending
				};
			SetAccessibleJourTypes(editMode);
			var data = Mapper.Map<IEnumerable<SampleViewModel>>(_samplesService.GetSamples(new PageParams { RowCount = -1, StartRowIndex = 0 }, sorting));

			StringBuilder sb = new StringBuilder();
			var writer = new ChoETL.ChoCSVWriter<SampleViewModel>(sb).WithFirstLineHeader();
			writer.Write(data);

			return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "export-" + editMode + ".csv");
		}
		#endregion

		#region Edit Single
		public ActionResult SamplePartial(int? id)
		{
			var sample = id == null ? null : _samplesService.Get((int) id);

			ViewBag.ExistsSample = false;

			if (sample == null)
				return PartialView("_NewSamplePartial", null);

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

			return PartialView("_EditSamplePartial", Mapper.Map<SampleViewModel>(sample));
		}

		//[ValidateAntiForgeryToken]
		public ActionResult GridViewPartialAddNew(SampleViewModel sample)
		{
			UserDTO usr = GetCurrentUser();
			sample.AspNetUserId = usr?.Id ?? "zero";
			sample.UserId = usr?.UserId.GetValueOrDefault(0) ?? 0;
			return UpdateModelWithDataValidation(Mapper.Map<SampleDTO>(sample), usr, (a, u) => { _samplesService.CreateSample(a, u); });
		}

		//[ValidateAntiForgeryToken]
		public ActionResult GridViewPartialUpdate(SampleViewModel sample)
		{
			UserDTO usr = GetCurrentUser();
			sample.AspNetUserId = usr?.Id ?? "zero";
			sample.UserId = usr?.UserId.GetValueOrDefault(0) ?? 0;
			return UpdateModelWithDataValidation(Mapper.Map<SampleDTO>(sample), usr, _samplesService.EditSample);
		}

		private ActionResult UpdateModelWithDataValidation(SampleDTO sample, UserDTO usr, Action<SampleDTO, UserDTO> updateMethod)
		{
			if (ModelState.IsValid)
				SafeExecute(() => updateMethod(sample, usr));
			else
				ViewBag.GeneralError = "Please, correct all errors.";

			return GridViewPartial();
		}
		#endregion
		#endregion

		#region Locations
		public ActionResult LocationsTreeListPartial(int? selectedId)
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

		#region Batch Combo
		public ActionResult _BatchComboBoxPartial()
		{
			return PartialView();
		}


		public IEnumerable<SampleApprovedViewModel> GetBatchesRangeNonDisposed(ListEditItemsRequestedByFilterConditionEventArgs args) => GetBatchesRange(args);

		public IEnumerable<SampleApprovedViewModel> GetBatchesRangeNonDisposedBoxes(ListEditItemsRequestedByFilterConditionEventArgs args) => GetBatchesRange(args, new SampleModel?[] { SampleModel.Box });

		private static readonly SampleApprovedViewModel emptyBatchViewModel = new SampleApprovedViewModel() { Name = "None", Barcode = "not in batch", Id = null };

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

		public SampleApprovedViewModel GetBatchById(ListEditItemRequestedByValueEventArgs args)
		{
			if (args.Value == null || !int.TryParse(args.Value.ToString(), out var id))
				return null;
			return id == 0 ? emptyBatchViewModel : Mapper.Map<SampleApprovedViewModel>(_samplesApprovedService.Get(id));
		}
		#endregion

		#region Print
		public JsonResult GetSamples(int[] ids) => Json(_samplesService.GetSamplesByIds(ids));
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