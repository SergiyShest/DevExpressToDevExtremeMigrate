using AutoMapper;
using DevExpress.Web;
using DevExpress.Web.Mvc;
using Sasha.Lims.Core.DTO.Laboratory;
using Sasha.Lims.Core.Infrastructure.Classes;
using Sasha.Lims.Core.Interfaces.Laboratory;
using Sasha.Lims.WebUI.Infrastructure.ViewModelHelpers;
using Sasha.Lims.WebUI.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Sasha.Lims.WebUI.Controllers
{
	[Authorize]
	public class ClientsController : GridBasedController<ClientViewModel, ClientDTO, IClientsService>
	{

		public ClientsController(IClientsService clientsService) : base(clientsService)
		{
		}

		public const string GridViewName = "ClientsGridView";

		// GET: Clients
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
			var viewModel = new GridViewModel { KeyFieldName = nameof(ClientViewModel.Id) };
			viewModel.Columns.Add(nameof(ClientViewModel.Id));
			viewModel.Columns.Add(nameof(ClientViewModel.Name));
			return viewModel;
		}
		public ActionResult _ComboBoxPartial() => PartialView("_ComboBoxPartial");

		public override string GetGridViewName() => GridViewName;

		public sealed class Reader : ComboBoxDataProvider<ClientViewModel>
		{
			private IClientsService Service => DependencyResolver.Current.GetService(typeof(IClientsService)) as IClientsService;

			public override IComboBoxColumnCollection Columns => SuggestedColumns;

			public override string TextFormat => null;

			public override IEnumerable<ClientViewModel> Get(ListEditItemsRequestedByFilterConditionEventArgs args)
			{
				var devExFilterStr = string.Empty;
				if (!string.IsNullOrWhiteSpace(args?.Filter))
				{
					// TODO: Enable filter by client name
					devExFilterStr = $"Contains([Name], '{args.Filter}')";
				}

				var skip = args.BeginIndex;
				var take = args.EndIndex - args.BeginIndex + 1;
				var result = Service.Get(new PageParams { RowCount = take, StartRowIndex = skip }, new SortParams { Ascending = true, OrderBy = "Name" }, devExFilterStr);
				return result.Select(BuildViewModel).ToList();
			}

			public override ClientViewModel Get(ListEditItemRequestedByValueEventArgs args)
			{
				if (args.Value == null || !int.TryParse(args.Value.ToString(), out int id))
					return null;
				var client = Service.Get(id);
				return BuildViewModel(client);
			}

			private static ClientViewModel BuildViewModel(ClientDTO client) => Mapper.Map<ClientViewModel>(client);

			public static readonly ComboBoxColumnCollectionBase<ClientViewModel, int> SuggestedColumns = ComboBoxColumnCollection<ClientViewModel>.Create(m => m.Id, m => m.Name);
		}
	}
}