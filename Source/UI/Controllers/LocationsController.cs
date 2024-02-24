using AutoMapper;
using DevExpress.Web;
using DevExpress.Web.Mvc;
using Sasha.Lims.Core.DTO.Laboratory;
using Sasha.Lims.Core.Infrastructure.Classes;
using Sasha.Lims.Core.Interfaces.Laboratory;
using Sasha.Lims.WebUI.Infrastructure.Attributes;
using Sasha.Lims.WebUI.Infrastructure.ViewModelHelpers;
using Sasha.Lims.WebUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Sasha.Lims.WebUI.Controllers
{
	public class LocationsController : GridBasedController<LocationViewModel, LocationDTO, ILocationsService>
	{
		public LocationsController(ILocationsService mainService) : base(mainService)
		{
		}

		public override string GetGridViewName() => "LocationsGridView";

		protected override GridViewModel CreateGridViewModelImpl()
		{
			throw new NotImplementedException();
		}

		[LimsReferenceProvider(typeof(LocationViewModel))]
		public sealed class Reader : ComboBoxDataProvider<LocationViewModel>
		{
			private Lazy<ILocationsService> Service => new Lazy<ILocationsService>(() => DependencyResolver.Current.GetService<ILocationsService>());
			public override IComboBoxColumnCollection Columns => SuggestedColumns;

			public override string TextFormat => "{1}";

			public override IEnumerable<LocationViewModel> Get(ListEditItemsRequestedByFilterConditionEventArgs args)
			{
				var devExFilterStr = string.Empty;
				if (!string.IsNullOrWhiteSpace(args?.Filter))
				{
					devExFilterStr = $"Contains([LocationName], '{args.Filter}')";
				}

				var skip = args.BeginIndex;
				var take = args.EndIndex - args.BeginIndex + 1;
				var result = Service.Value.Get(new PageParams { RowCount = take, StartRowIndex = skip }, new SortParams { Ascending = true, OrderBy = "LocationName" }, devExFilterStr);
				return result.Select(BuildViewModel).ToList();
			}

			private static LocationViewModel BuildViewModel(LocationDTO dto)
			{
				return Mapper.Map<LocationViewModel>(dto);
			}

			public override LocationViewModel Get(ListEditItemRequestedByValueEventArgs args)
			{
				return args.Value == null || !int.TryParse(args.Value.ToString(), out int id) ? null : BuildViewModel(Service.Value.Get(id));
			}

			public static readonly ComboBoxColumnCollectionBase<LocationViewModel, int> SuggestedColumns =
				ComboBoxColumnCollection<LocationViewModel>.Create(m => m.Id, m => m.LocationName, m => m.FullPath);

		}
	}
}