using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using DevExpress.Data;
using DevExpress.Web.Mvc;
using Newtonsoft.Json;
using Sasha.Lims.Core.Consts;
using Sasha.Lims.Core.DTO.Access;
using Sasha.Lims.Core.DTO.Common;
using Sasha.Lims.Core.DTO.Laboratory;
using Sasha.Lims.Core.Infrastructure.Classes;
using Sasha.Lims.Core.Infrastructure.Extentions;
using Sasha.Lims.Core.Interfaces.Access;
using Sasha.Lims.Core.Interfaces.Common;
using Sasha.Lims.Core.Interfaces.Laboratory;
using Sasha.Lims.DataAccess.Entities.Constants;
using Sasha.Lims.WebUI.Infrastructure.Classes;
using Sasha.Lims.WebUI.Models;

namespace Sasha.Lims.WebUI.Controllers
{
	public class SamplesBoxController : BaseSamplesController
	{
		private readonly ISamplesService _samplesService;
		public SamplesBoxController(IPropertiesService propertiesService, IUserServiceCreator userServiceCreator,
										IClientsService clientsService, IDoctorsService doctorsService,
										IPatientsService patientsService, ILocationsService locationsService,
											ISamplesApprovedService samplesApprovedService, ISamplesService samplesService)
		{
			_samplesService = samplesService;
			_samplesService.SetAccessibleJourTypes(new[] { JournalType.Box });
			SamplesApprovedService = samplesApprovedService;
			PropertiesService = propertiesService;
			UserService = userServiceCreator.CreateUserService(SettingsConsts.IdentityDbConfigSection);
			LocationsService = locationsService;
		}

        public ActionResult Index()
        {
			return View();
        }

		public ActionResult WellsDiagram()
		{
			return View();
		}

		private void SetAccessibleJourTypes(string editModeParam)
		{
			if (Enum.TryParse(editModeParam, out JournalType jourType))
			{
				_samplesService.SetAccessibleJourTypes(new[] { jourType });
			}
		}

		public IEnumerable<PropertyViewModel> GetBoxTubeTypes()
		{
			return GetProperties("TubeType").Where(x => x.Code == "BOX");
		}

		[ValidateInput(false)]
		public ActionResult SamplesGridViewBatchEditingPartial()
		{
			//return PartialView("_SamplesGridViewBatchEditingPartial", _samplesService.GetSamples(pageParams:));
			//return View("_SamplesGridViewBatchEditingPartial");
			var viewModel = GridViewExtension.GetViewModel("SamplesPlateViewModel");

			if (viewModel == null)
			{
				viewModel = CreateGridViewModel();
			}

			return GridCustomActionCore(viewModel);
		}

		protected void SaveBoxWithOutItems(SampleViewModel mainBox, bool needMoveFromEditMode)
		{
			UserDTO usr = GetCurrentUser();
			var existsMainPlate = _samplesService.Get(mainBox?.Id ?? 0);

			var mainBoxDTO = SaveMainBox(mainBox);

			var boxItems = _samplesService.GetEditingBatchItems(mainBoxDTO.Id);

			if (existsMainPlate != null && mainBoxDTO.LocationId != existsMainPlate?.LocationId)
			{
				//need update location
				foreach (var plateItem in boxItems)
				{
					plateItem.LocationId = mainBoxDTO.LocationId;
					//plateItem.TubeTypeId = PropertiesConsts.TubeTypeWell;

					plateItem.AspNetUserId = usr?.Id ?? "zero";
					plateItem.UserId = usr?.UserId.GetValueOrDefault(0) ?? 0;

					_samplesService.EditSample(plateItem,usr);
				}
			}

			if (needMoveFromEditMode)
			{
				_samplesService.FromEditModeBox(mainBoxDTO, boxItems, usr?.Id ?? "zero", usr?.UserId.GetValueOrDefault(0) ?? 0);
			} else
			{
				//init update diagram data if diagram on edit page
				//if (mainBoxDTO != null && mainBoxDTO?.Id > 0)
				//{
				//	var boxType = GetProperties("TubeType", mainBoxDTO?.TubeTypeId ?? 0).FirstOrDefault()?.Value ?? "";
				//	var boxJSItems = boxItems.Select(x => new WellDataForJSDiagram() { id = x.Id, pos = x.Well, value = x.Name }).ToArray();
				//	ViewBag.boxType = boxType;
				//	ViewBag.boxData = JsonConvert.SerializeObject(boxJSItems);
				//}
			}
		}


		protected SampleDTO SaveMainBox(SampleViewModel mainBox)
		{
			UserDTO usr = GetCurrentUser();

			SampleDTO resultMainPlateDto = null;

			//save main plate
			if (mainBox != null && mainBox.Id > 0)
			{
				//save exists
				var existsMainBox = _samplesService.Get(mainBox.Id.Value);
				existsMainBox.Barcode = mainBox.Barcode;
				existsMainBox.Name = mainBox.Name;
				existsMainBox.TubeTypeId = mainBox.TubeTypeId;
				existsMainBox.LocationId = mainBox.LocationId;
				existsMainBox.SampleStatusId = mainBox.SampleStatusId;

				existsMainBox.AspNetUserId = usr.Id;
				existsMainBox.UserId = usr.UserId.GetValueOrDefault(0);

				mainBox.AspNetUserId = usr.Id;
				mainBox.UserId = usr.UserId.GetValueOrDefault(0);

				_samplesService.EditSample(existsMainBox,usr);

				resultMainPlateDto = existsMainBox;
			} else
			{
				//create new main id
				mainBox = mainBox ?? new SampleViewModel();
				mainBox.AspNetUserId = usr.Id;
				mainBox.UserId = usr.UserId.GetValueOrDefault(0);

				var mainBoxDto = Mapper.Map<SampleDTO>(mainBox);

				_samplesService.CreateSample(mainBoxDto,usr, JournalType.Box);

				resultMainPlateDto = mainBoxDto;
			}

			return resultMainPlateDto;
		}


		[ValidateInput(false)]
		public ActionResult BatchEditingUpdateModel(MVCxGridViewBatchUpdateValues<SampleViewModel, int> updateValues, SampleViewModel mainBox, bool postAction)
		{
			UserDTO usr = GetCurrentUser();

			var mainBoxDTO = SaveMainBox(mainBox);

			var mainBoxId = mainBoxDTO?.Id ?? 0;
			var mainBoxLocationId = mainBoxDTO?.LocationId;

			//wells
			foreach (var product in updateValues.Insert)
			{
				if (updateValues.IsValid(product))
				{
					//InsertProduct(product, updateValues);
					throw new NotImplementedException();
				}
			}
			foreach (var product in updateValues.Update)
			{
				if (updateValues.IsValid(product))
				{
					var sample = _samplesService.Get(product.Id.GetValueOrDefault(0));

					sample.Well = product.Well;
					sample.Name = product.Name;
					sample.AcceptedDate = product.AcceptedDate;
					sample.Volume = product.Volume;
					sample.VolumeUnitId = product.VolumeUnitId;

					sample.BatchId = mainBoxId;
					sample.LocationId = mainBoxLocationId;
					//sample.TubeTypeId = PropertiesConsts.TubeTypeWell;

					sample.AspNetUserId = usr?.Id ?? "zero";
					sample.UserId = usr?.UserId.GetValueOrDefault(0) ?? 0;

					_samplesService.EditSample(sample,usr);
				}

			}
			foreach (var productId in updateValues.DeleteKeys)
			{
				_samplesService.DeleteSamples(new[] { productId }, GetCurrentUser().Id);
			}

			if (postAction)
			{
				return GridViewCustomAction("Process", null, Mapper.Map<SampleViewModel>(mainBoxDTO));
			} else
			{
				if (mainBoxDTO != null && mainBoxDTO?.Id > 0)
				{
					var plateType = GetProperties("TubeType", mainBoxDTO?.TubeTypeId ?? 0).FirstOrDefault()?.Value ?? "";
					var plateItems = _samplesService.GetEditingBatchItems(mainBoxDTO?.Id ?? 0).Select(x => new WellDataForJSDiagram() { id = x.Id, pos = x.Well, value = x.Name }).ToArray();
					ViewBag.plateType = plateType;
					ViewBag.plateData = JsonConvert.SerializeObject(plateItems);
				}

				return SamplesGridViewBatchEditingPartial();
			}
		}

		public ActionResult SamplesEditorGridViewSortingAction(GridViewColumnState column, bool reset)
		{
			var viewModel = GridViewExtension.GetViewModel("gridViewSamplesEditor");
			viewModel.SortBy(column, reset);

			return GridCustomActionCore(viewModel);
		}

		public ActionResult SamplesEditorGridViewPagingAction(GridViewPagerState pager)
		{
			var viewModel = GridViewExtension.GetViewModel("gridViewSamplesEditor");
			viewModel.Pager.Assign(pager);

			return GridCustomActionCore(viewModel);
		}

		public ActionResult SamplesEditorGridViewFilteringAction(GridViewFilteringState filteringState)
		{
			var viewModel = GridViewExtension.GetViewModel("gridViewSamplesEditor");
			viewModel.ApplyFilteringState(filteringState);

			return GridCustomActionCore(viewModel);
		}

		private PartialViewResult GridCustomActionCore(GridViewModel gridViewModel)
		{
			gridViewModel.ProcessCustomBinding(GetDataRowCountSamplesApproved,
				GetDataSamplesApproved,
				null,
				null,
				GetUniqueHeaderFilterValuesAdvanced);

			return PartialView("_SamplesGridViewBatchEditingPartial", gridViewModel);
		}

		private void GetDataRowCountSamplesApproved(GridViewCustomBindingGetDataRowCountArgs e)
		{
			e.DataRowCount = _samplesService.BatchItemsCount(GetEditingBox(null)?.Id ?? 0, e.FilterExpression);
		}
		public void GetDataSamplesApproved(GridViewCustomBindingGetDataArgs e)
		{
			e.Data = Mapper.Map<IEnumerable<SampleViewModel>>(_samplesService.GetEditingBatchItems(new PageParams
				{
					StartRowIndex = e.StartDataRowIndex,
					RowCount = e.DataRowCount
				},
				e.State.SortedColumns.Count == 0
					? new SortParams { OrderBy = "Id" }
					: new SortParams
					{
						OrderBy = Mapper.Instance.GetDestinationPropertyFor<SampleViewModel, SampleDTO>(
							e.State.SortedColumns.FirstOrDefault()?.FieldName),
						Ascending = !e.State.SortedColumns.FirstOrDefault()
							.SortOrder
							.HasFlag(ColumnSortOrder.Descending)
					}
				, e?.State?.AppliedFilterExpression ?? string.Empty
				, GetEditingBox(null)?.Id ?? 0
			));
		}

		public static void GetUniqueHeaderFilterValuesAdvanced(GridViewCustomBindingGetUniqueHeaderFilterValuesArgs e)
		{
			//e.Data = Model
			//    .ApplyFilter(e.FilterExpression)
			//    .UniqueValuesForField(e.FieldName);
		}

		private static GridViewModel CreateGridViewModel()
		{
			var viewModel = new GridViewModel();

			//viewModel.Columns.Add("RowVersion");
			viewModel.KeyFieldName = "Id";
			viewModel.Columns.Add("Barcode");
			viewModel.Columns.Add("Name");
			viewModel.Columns.Add("AcceptedDate");
			//viewModel.Columns.Add("CollectedDateTime");
			viewModel.Columns.Add("LocationId");
			viewModel.Columns.Add("TubeTypeId");
			//viewModel.Columns.Add("SampleTypeId");
			//viewModel.Columns.Add("SampleStatusId");
			//viewModel.Columns.Add("Well");
			viewModel.Columns.Add("Volume");
			viewModel.Columns.Add("VolumeUnitId");
			//viewModel.Columns.Add("AspNetUserId");
			//viewModel.Columns.Add("ClientId");
			//viewModel.Columns.Add("DoctorId");
			//viewModel.Columns.Add("PatientId");
			//viewModel.Columns.Add("EquipmentId");

			return viewModel;
		}

		public ActionResult LocationsTreeListPartial(int? selectedId)
		{
			var locations = PropertiesService.GetLocations(); 

			//init short path
			var rootNode = locations.FirstOrDefault(x => x.ParentId == null);
			rootNode.Path = string.Empty;
			InitPropertyShortPath(rootNode.Id, locations, 0);

			ViewBag.LocationSelectedId = selectedId;
			return PartialView("_LocationTreeListPartial", locations);
		}

		public ActionResult BoxPanelEditingPartial(int? plateId)
		{
			var mainBoxRecord = Mapper.Map<SampleViewModel>(
				GetEditingBox(plateId));

			mainBoxRecord = mainBoxRecord ?? new SampleViewModel();
			return PartialView("_BoxPanelEditingPartial", mainBoxRecord);
		}

		protected SampleDTO GetEditingBox(int? boxId)
		{
			SampleDTO result = null;

			if (boxId.HasValue)
			{
				result = _samplesService.GetEditingBoxes(GetCurrentUser().UserId.GetValueOrDefault(0)).FirstOrDefault(x => x.Id == boxId);
			} else
			{
				result = _samplesService.GetEditingBoxes(GetCurrentUser().UserId.GetValueOrDefault(0)).FirstOrDefault();
			}

			return result;
		}

		protected void InitPropertyShortPath(int nodeId, IEnumerable<PropertyDTO> props, int level)
		{
			var shortLevel = 10;

			if (props != null)
			{
				var curNode = props.FirstOrDefault(x => x.Id == nodeId) ?? new PropertyDTO();
				var childNodes = props.Where(x => x.ParentId == nodeId);

				foreach (var childNode in childNodes)
				{
					if (level <= shortLevel)
					{
						childNode.Path = curNode.Path + (string.IsNullOrWhiteSpace(curNode.Path) ? "" : "->") + childNode.Name;
					} else
					{
						childNode.Path = curNode.Path;
					}

					InitPropertyShortPath(childNode.Id, props, level++);
				}
			}

		}

		public ActionResult GridViewCustomAction(string customAction, string customParams, SampleViewModel mainBox)
		{
			UserDTO usr = GetCurrentUser();
			switch (customAction)
			{
				case "Process":
					{
						SaveBoxWithOutItems(mainBox, true);
						//_samplesService.FromEditMode(PropertiesConsts.JourTypeBox, usr.Id, usr.UserId.GetValueOrDefault(0));
						return RedirectToAction("Index", "SamplesApproved");
					}
				case "SaveSamples":
					{
						SaveBoxWithOutItems(mainBox, false);
						break;
					}
				case "Clear":
					{
						_samplesService.ClearJournal(JournalType.Box, usr.UserId.GetValueOrDefault(0));
						//redirect to samples
						return RedirectToAction("Index", "SamplesApproved");
					}
			}

			return SamplesGridViewBatchEditingPartial();
		}

		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			_samplesService.SetUserFilter(GetCurrentUser().UserId.GetValueOrDefault(0));
			base.OnActionExecuting(filterContext);
		}

	}
}