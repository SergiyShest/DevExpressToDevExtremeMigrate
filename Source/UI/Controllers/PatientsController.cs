using AutoMapper;
using DevExpress.Web;
using DevExpress.Web.Mvc;
using Sasha.Lims.Core.DTO.Laboratory;
using Sasha.Lims.Core.Infrastructure.Classes;
using Sasha.Lims.Core.Interfaces.Laboratory;
using Sasha.Lims.WebUI.Infrastructure.Attributes;
using Sasha.Lims.WebUI.Infrastructure.ViewModelHelpers;
using Sasha.Lims.WebUI.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Sasha.Lims.WebUI.Controllers
{
	[Authorize]
	public class PatientsController : GridBasedController<PatientViewModel, PatientDTO, IPatientsService>
	{
		private IDoctorsService _doctorsService;
		public const string GridViewName = "PatientsGridView";

		public PatientsController(IPatientsService patientsService, IDoctorsService doctorsService) : base(patientsService)
		{
			_doctorsService = doctorsService;
		}
		// GET: Clients
		public ActionResult Index() => View();
		public override string GetGridViewName() => GridViewName;

		#region GridView
		public override ActionResult GridViewPartialAddNew(PatientViewModel patient)
		{
			patient.ClientId = _doctorsService.Get(patient.DoctorId ?? 0)?.ClientId;
			return UpdateModelWithDataValidation(Mapper.Map<PatientDTO>(patient), MainService.Create);
		}

		//[ValidateAntiForgeryToken]
		public override ActionResult GridViewPartialUpdate(PatientViewModel patient)
		{
			patient.ClientId = _doctorsService.Get(patient.DoctorId ?? 0)?.ClientId;
			return UpdateModelWithDataValidation(Mapper.Map<PatientDTO>(patient), MainService.Edit);
		}

		public ActionResult CustomActionPartial(string customAction)
		{
			switch (customAction)
			{
				case "delete":
					SafeExecute(() => PerformDelete());
					break;
			}

			return GridViewPartial();
		}

		protected override GridViewModel CreateGridViewModelImpl()
		{
			var viewModel = new GridViewModel { KeyFieldName = "Id" };
			viewModel.Columns.Add(nameof(PatientViewModel.Id));
			viewModel.Columns.Add(nameof(PatientViewModel.FirstName));
			viewModel.Columns.Add(nameof(PatientViewModel.LastName));

			return viewModel;
		}
		#endregion


		#region Patient Combo
		public ActionResult _ComboBoxPartial() => PartialView();

		[LimsReferenceProvider(typeof(PatientViewModel))]
		public sealed class Reader : ComboBoxDataProvider<PatientViewModel>
		{
			public override IEnumerable<PatientViewModel> Get(ListEditItemsRequestedByFilterConditionEventArgs args)
			{
				IPatientsService _service = DependencyResolver.Current.GetService<IPatientsService>();

				var devExFilterStr = string.Empty;
				if (!string.IsNullOrWhiteSpace(args?.Filter))
				{
					// TODO: Enable filter by client name
					devExFilterStr = $"Contains([FirstName], '{args.Filter}') Or Contains([LastName], '{args.Filter}')";
				}

				var skip = args.BeginIndex;
				var take = args.EndIndex - args.BeginIndex + 1;
				var result = _service.Get(new PageParams { RowCount = take, StartRowIndex = skip }, new SortParams { Ascending = true, OrderBy = "FirstName" }, devExFilterStr);
				return result.Select(BuildViewModel).ToList();
			}

			public override PatientViewModel Get(ListEditItemRequestedByValueEventArgs args)
			{
				IPatientsService _service = DependencyResolver.Current.GetService<IPatientsService>();
				if (args.Value == null || !int.TryParse(args.Value.ToString(), out int id))
					return null;
				var data = _service.Get(id);
				return BuildViewModel(data);
			}

			private static PatientViewModel BuildViewModel(PatientDTO patient) => Mapper.Map<PatientViewModel>(patient);

			public static readonly ComboBoxColumnCollectionBase<PatientViewModel, int> SuggestedColumns = ComboBoxColumnCollection<PatientViewModel>.Create(m => m.Id, m => m.FirstName, m => m.LastName);

			public override IComboBoxColumnCollection Columns => SuggestedColumns;

			public override string TextFormat => "{0} {1}";
		}

		public IEnumerable<PatientDTO> GetPatientsRange(ListEditItemsRequestedByFilterConditionEventArgs args)
		{
			var devExFilterStr = string.Empty;

			if (!string.IsNullOrWhiteSpace(args?.Filter))
			{
				devExFilterStr = $"Contains([FirstName], '{args.Filter}') Or Contains([LastName], '{args.Filter}')";
			}

			var skip = args.BeginIndex;
			var take = args.EndIndex - args.BeginIndex + 1;

			return MainService.Get(new PageParams { RowCount = take, StartRowIndex = skip },
				new SortParams { Ascending = true, OrderBy = "FirstName" },
				devExFilterStr);
		}

		public PatientDTO GetPatientById(ListEditItemRequestedByValueEventArgs args)
		{
			return args.Value == null || !int.TryParse(args.Value.ToString(), out var id) ? null : MainService.Get(id);
		}

		#endregion

	}
}