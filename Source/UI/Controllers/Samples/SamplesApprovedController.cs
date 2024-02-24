using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using DevExpress.Web;
using DevExpress.Web.Mvc;
using Newtonsoft.Json;
using Sasha.Lims.Core.DTO.Access;
using Sasha.Lims.Core.DTO.Laboratory;
using Sasha.Lims.Core.Infrastructure.Classes;
using Sasha.Lims.Core.Interfaces.Access;
using Sasha.Lims.Core.Interfaces.Laboratory;
using Sasha.Lims.DataAccess.Entities.Constants;
using Sasha.Lims.WebUI.Infrastructure.Attributes;
using Sasha.Lims.WebUI.Infrastructure.Classes;
using Sasha.Lims.WebUI.Infrastructure.ViewModelHelpers;
using Sasha.Lims.WebUI.Models;

namespace Sasha.Lims.WebUI.Controllers
{
	[Authorize]
	public class SamplesApprovedController : GridBasedController<SampleApprovedViewModel, SampleApprovedDTO, ISamplesApprovedService>
	{

		public SamplesApprovedController(ISamplesApprovedService samplesApprovedService, IUserServiceCreator userServiceCreator, ILocationsService locationsService) : base(samplesApprovedService)
		{
			LocationsService = locationsService;
		}

		public const string GridViewName = "gridViewSamplesApproved";
		public override string GetGridViewName() => GridViewName;

		// GET: SamplesApproved
		public ActionResult Index() => View();

		[ChildActionOnly]
		public ActionResult _SampleApprovedFilesAttachedCardViewPartial(int sampleId)
		{
			var model = MainService.GetFiles(sampleId);

			return PartialView(model);
		}

		public ActionResult GridViewCustomAction(string customAction, string customParams,
																JournalType editMode = JournalType.Management)
		{
			UserDTO usr = GetCurrentUser();

			switch (customAction)
			{
				case "MoveToEdit":
					{
						var samplesApprovedIds = customParams.Split(',').Select(RecVersionId<int>.Parse).ToArray();
						MainService.ToEditMode(samplesApprovedIds, usr.Id, editMode, usr.UserId.GetValueOrDefault(0));

						return RedirectToAction("Index", "Samples", new { editMode });
					}
				case "MoveToAliquoting":
					{
						List<int> samplesApprovedIds = customParams.Split(',').Select(int.Parse).ToList();
						MainService.ToAliquotingMode(samplesApprovedIds, JournalType.Aliquoting, usr);

						return RedirectToAction("Index", "Samples", new { editMode = (int) JournalType.Aliquoting });
					}
				case "MoveToPlate":
					{
						var samplesApprovedIds = customParams.Split(',').Select(RecVersionId<int>.Parse).ToArray();

						MainService.ToPlateMode(samplesApprovedIds,
							GetCurrentUser().Id,
							JournalType.Plate,
							usr.UserId.GetValueOrDefault(0));

						return RedirectToAction("Index", "SamplesPlate", new { editMode = (int) JournalType.Plate });
					}
				case "MoveToBox":
					{
						var samplesApprovedIds = customParams.Split(',').Select(RecVersionId<int>.Parse).ToArray();

						MainService.ToBoxMode(samplesApprovedIds,
							GetCurrentUser().Id,
							JournalType.Box,
							usr.UserId.GetValueOrDefault(0));

						return RedirectToAction("Index", "SamplesBox", new { editMode = JournalType.Box });
					}
				case "Print":
					if (!string.IsNullOrEmpty(customParams))
					{
						var ids = string.Join(",", customParams.Split(',').Select(x => RecVersionId<int>.Parse(x).Id));

						return RedirectToAction("Book", "Print", new { ids });
					}

					break;
			}

			return GridViewPartial();
		}

		protected override GridViewModel CreateGridViewModelImpl()
		{
			var viewModel = new GridViewModel();

			viewModel.Columns.Add("RowVersion");
			viewModel.KeyFieldName = "Id";
			viewModel.Columns.Add("Barcode");
			viewModel.Columns.Add("Name");
			viewModel.Columns.Add("CollectedDateTime");
			viewModel.Columns.Add("LocationId");
			viewModel.Columns.Add("TubeTypeId");
			viewModel.Columns.Add("SampleTypeId");
			viewModel.Columns.Add("SampleStatusId");
			viewModel.Columns.Add("Well");
			viewModel.Columns.Add("Volume");
			viewModel.Columns.Add("VolumeUnitId");
			viewModel.Columns.Add("AspNetUserId");
			viewModel.Columns.Add("ClientId");
			viewModel.Columns.Add("DoctorId");
			viewModel.Columns.Add("PatientId");
			viewModel.Columns.Add("EquipmentId");

			return viewModel;
		}

		//---------------------------plates --------------------------

		public ActionResult WellsDiagram()
		{
			return View();
		}

		public ActionResult WellsDiagramPartial(int? plateId)
		{
			var plateData = GetPlate(plateId);
			var plateItems = MainService.GetPlatesItems(plateData?.Id ?? 0).Select(x => new WellDataForJSDiagram() { id = x.Id, pos = x.Well, value = x.Name }).ToArray();

			var plateType = PropertiesController.PropertiesReader.GetProperties("TubeType", plateData?.TubeTypeId ?? 0).FirstOrDefault()?.Value ?? "";
			ViewBag.plateType = plateType;
			return PartialView("_WellsDiagramPartial", JsonConvert.SerializeObject(plateItems));
		}

		protected SampleApprovedDTO GetPlate(int? plateId)
		{
			SampleApprovedDTO result = null;

			if (plateId.HasValue)
			{
				result = MainService.GetPlates(plateId).FirstOrDefault();
			}

			return result;
		}

		[LimsReferenceProvider(typeof(SampleApprovedViewModel), "Batch")]
		public sealed class BatchReader : ComboBoxDataProvider<SampleApprovedViewModel>
		{
			private ISamplesApprovedService Service => DependencyResolver.Current.GetService(typeof(ISamplesApprovedService)) as ISamplesApprovedService;

			public override string TextFormat => "{1}";
			public override IComboBoxColumnCollection Columns => SuggestedColumns;
			private static readonly SampleApprovedViewModel emptyBatchViewModel = new SampleApprovedViewModel() { Name = "None", Barcode = "", Id = 0 };

			public override IEnumerable<SampleApprovedViewModel> Get(ListEditItemsRequestedByFilterConditionEventArgs args)
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

				IEnumerable<SampleApprovedDTO> containers = Service.GetContainers(new PageParams { RowCount = take, StartRowIndex = skip },
					new SortParams { Ascending = true, OrderBy = "Name" }, true, null, devExFilterStr);

				var result = Mapper.Map<IEnumerable<SampleApprovedViewModel>>(containers);
				if (skip == 0)
					result = Enumerable.Prepend(result, emptyBatchViewModel);

				return result;
			}

			public override SampleApprovedViewModel Get(ListEditItemRequestedByValueEventArgs args)
			{
				if (args.Value == null || !int.TryParse(args.Value.ToString(), out var id))
				{
					return null;
				}
				if (id == 0)
					return emptyBatchViewModel;

				return Mapper.Map<SampleApprovedViewModel>(Service.Get(id));
			}

			public static readonly ComboBoxColumnCollectionBase<SampleApprovedViewModel, int?> SuggestedColumns = ComboBoxColumnCollection<SampleApprovedViewModel>.Create(m => m.Id, m => m.Name, m => m.Barcode);
		}

		public class ComboBoxVM
		{
			public ComboBoxVM(SampleApprovedViewModel sample)
			{
				Sample = sample;
			}

			public SampleApprovedViewModel Sample { get; }
		}

		[LimsReferenceProvider(typeof(SampleApprovedViewModel))]
		public sealed class Reader : ComboBoxDataProvider<ComboBoxVM>
		{
			private ISamplesApprovedService Service => DependencyResolver.Current.GetService(typeof(ISamplesApprovedService)) as ISamplesApprovedService;

			public override IComboBoxColumnCollection Columns => SuggestedColumns;

			public override string TextFormat => null;

			public override IEnumerable<ComboBoxVM> Get(ListEditItemsRequestedByFilterConditionEventArgs args)
			{
				var devExFilterStr = string.Empty;
				if (!string.IsNullOrWhiteSpace(args?.Filter))
				{
					devExFilterStr = $"Contains([Name], '{args.Filter}')";
				}

				var skip = args.BeginIndex;
				var take = args.EndIndex - args.BeginIndex + 1;

				var result = Service.Get(new PageParams { RowCount = take, StartRowIndex = skip },
					new SortParams { Ascending = true, OrderBy = "Name" },
					devExFilterStr);
				return result.Select(BuildViewModel).ToList();
			}

			public override ComboBoxVM Get(ListEditItemRequestedByValueEventArgs args)
			{
				if (args.Value == null || !int.TryParse(args.Value.ToString(), out int id))
					return null;
				return BuildViewModel(Service.Get(id));
			}

			private static ComboBoxVM BuildViewModel(SampleApprovedDTO data)
			{
				return new ComboBoxVM(Mapper.Map<SampleApprovedViewModel>(data));
			}

			public static readonly ComboBoxColumnCollectionBase<ComboBoxVM, int?> SuggestedColumns = ComboBoxColumnCollection<ComboBoxVM>.Create(m => m.Sample.Id, m => m.Sample.Name, m => m.Sample.Barcode);
		}
	}
}