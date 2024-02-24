using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using AutoMapper;
using DevExpress.Data;
using DevExpress.Web;
using DevExpress.Web.Mvc;
using Sasha.Lims.Core.DTO.Access;
using Sasha.Lims.Core.DTO.Common;
using Sasha.Lims.Core.DTO.Laboratory;
using Sasha.Lims.Core.Infrastructure.Classes;
using Sasha.Lims.Core.Infrastructure.Extentions;
using Sasha.Lims.Core.Interfaces.Common;
using Sasha.Lims.Core.Interfaces.Customization;
using Sasha.Lims.Core.Interfaces.Laboratory;
using Sasha.Lims.DataAccess.Entities.Constants;
using Sasha.Lims.DataAccess.Entities.Entity;
using Sasha.Lims.Core.Settings;
using Sasha.Lims.WebUI.Models;

namespace Sasha.Lims.WebUI.Controllers
{
	[Authorize]
	public class SamplesJournalController : GridBasedController
	{
		private readonly ISamplesService _samplesService;
		private readonly ISamplesApprovedService _samplesApprovedService;

		public const string GridViewName = "SamplesGridView";
		public override string GetGridViewName() => GridViewName;

		public SamplesJournalController(ISamplesService samplesService, IPropertiesService propertiesService, ISamplesApprovedService samplesApprovedService, ILocationsService locationsService,
										IDtoMetadataService metadataService) : base(metadataService.GetUserFieldsForViewModel<SampleDTO>())
		{
			_samplesService = samplesService;
			_samplesService.SetAccessibleJourTypes(new[] { JournalType.Management, JournalType.Receiving });
			//_propertiesService = propertiesService;
			_samplesApprovedService = samplesApprovedService;
			LocationsService = locationsService;
		}
		// GET: Samples
		public ActionResult Index(JournalType editMode = JournalType.Management)
		{
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
		public ActionResult AddSamplesByBarCode(string samplesBarcodes, JournalType editMode = JournalType.Management)
		{
			var barcodes = samplesBarcodes?.Replace(" ", "").Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

			if (barcodes?.Length > 0)
			{
				UserDTO usr = GetCurrentUser();

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

			return RedirectToAction("Index", new { editMode = (int) editMode });
		}

		public ActionResult FindSampleByBarcode(string barcode, JournalType? jourType)
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

			object jsonData;
			if (dto != null)
				jsonData = new { _status = 200, requestKey = barcode, barcode = dto.Barcode, model = dto.ModelId.ToString() };
			else
				jsonData = new { _status = 404, requestKey = barcode };

			return new JsonResult() { Data = jsonData, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
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

				case "export":
					// I cannot query data and send it right here, because devexpress JS requires me to send a partial view with grid in response.
					// However I can pass an export command with parameters from here, so that it executes when partial is injected 
					var viewModel = GridViewExtension.GetViewModel(GetGridViewName());
					//ViewBag.Export viewModel.FilterExpression;
					//viewModel.SortedColumns
					break;

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


#endregion


	}
}