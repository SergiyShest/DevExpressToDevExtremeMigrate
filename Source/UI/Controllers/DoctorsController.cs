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
	public class DoctorsController : GridBasedController<DoctorViewModel, DoctorDTO, IDoctorsService>
	{

		public const string GridViewName = "DoctorsGridView";

		public DoctorsController(IDoctorsService doctorsService) : base(doctorsService)
		{
		}

		public ActionResult Index() => View();


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
			viewModel.Columns.Add(nameof(DoctorViewModel.Id));
			viewModel.Columns.Add(nameof(DoctorViewModel.FirstName));
			viewModel.Columns.Add(nameof(DoctorViewModel.LastName));
			viewModel.Columns.Add(nameof(DoctorViewModel.Title));

			return viewModel;
		}


		public ActionResult _ComboBoxPartial() => PartialView("_ComboBoxPartial");

		public override string GetGridViewName() => GridViewName;

		public class ComboBoxVM
		{
			public ComboBoxVM(DoctorViewModel doctorViewModel, ClientViewModel clientViewModel)
			{
				Doctor = doctorViewModel;
				Client = clientViewModel;
			}

			public DoctorViewModel Doctor { get; }
			public ClientViewModel Client { get; }
		}

		[LimsReferenceProvider(typeof(DoctorViewModel))]
		public sealed class Reader : ComboBoxDataProvider<ComboBoxVM>
		{
			public override IEnumerable<ComboBoxVM> Get(ListEditItemsRequestedByFilterConditionEventArgs args)
			{
				IDoctorsService _service = DependencyResolver.Current.GetService<IDoctorsService>();

				var devExFilterStr = string.Empty;
				if (!string.IsNullOrWhiteSpace(args?.Filter))
				{
					// TODO: Enable filter by client name
					devExFilterStr = $"Contains([FirstName], '{args.Filter}') Or Contains([LastName], '{args.Filter}')";
				}

				var skip = args.BeginIndex;
				var take = args.EndIndex - args.BeginIndex + 1;
				var result = _service.GetWithRel(new PageParams { RowCount = take, StartRowIndex = skip }, new SortParams { Ascending = true, OrderBy = "FirstName" }, devExFilterStr);
				return result.Select(d => BuildViewModel(d.doctor, d.client)).ToList();
			}

			public override ComboBoxVM Get(ListEditItemRequestedByValueEventArgs args)
			{
				IDoctorsService _service = DependencyResolver.Current.GetService<IDoctorsService>();
				if (args.Value == null || !int.TryParse(args.Value.ToString(), out int id))
					return null;
				var (doctor, client) = _service.GetWithRel(id);
				return BuildViewModel(doctor, client);
			}

			private static ComboBoxVM BuildViewModel(DoctorDTO doctor, ClientDTO client) => new ComboBoxVM(Mapper.Map<DoctorViewModel>(doctor), Mapper.Map<ClientViewModel>(client));

			public static readonly ComboBoxColumnCollectionBase<ComboBoxVM, int> SuggestedColumns = ComboBoxColumnCollection<ComboBoxVM>.Create(m => m.Doctor.Id, m => m.Doctor.Title, m => m.Doctor.FirstName, m => m.Doctor.LastName, m => m.Client.Name);

			public override IComboBoxColumnCollection Columns => SuggestedColumns;

			public override string TextFormat => "{0} {1} {2} ({3})";
		}
	}
}