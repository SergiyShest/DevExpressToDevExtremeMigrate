using AutoMapper;
using DevExpress.Data;
using DevExpress.Web.Mvc;
using Sasha.Lims.Core.Consts;
using Sasha.Lims.Core.DTO.Laboratory;
using Sasha.Lims.Core.Infrastructure.Classes;
using Sasha.Lims.Core.Infrastructure.Extentions;
using Sasha.Lims.Core.Interfaces.Access;
using Sasha.Lims.Core.Interfaces.Common;
using Sasha.Lims.Core.Interfaces.Laboratory;
using Sasha.Lims.WebUI.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Sasha.Lims.WebUI.Controllers
{

	//public class CustodyLogController : BaseSamplesController
	//{
 //   }
	//[Authorize]
	//public class CustodyLogController : BaseSamplesController
	//{

	//	private ICustodyLogService _custodyLogService;

	//	public CustodyLogController(ICustodyLogService custodyLogService, IPropertiesService propertiesService, IUserServiceCreator userServiceCreator, ILocationsService locationsService)
	//	{
	//		_custodyLogService = custodyLogService;
	//		PropertiesService = propertiesService;
	//		UserService = userServiceCreator.CreateUserService(SettingsConsts.IdentityDbConfigSection);
	//		LocationsService = locationsService;
	//	}

	//	// GET: CustodyLog
	//	public ActionResult Index()
 //       {
	//		// "Index.GridView" - GridView version for CustodyLog
	//		return View("Index.GridView");
 //       }

	//	//public IEnumerable<CustodyLogViewModel> GetCustodyLogViewModels()
	//	//{
	//	//	return Mapper.Map<IEnumerable<CustodyLogViewModel>>(_custodyLogService.GetCustodyLogs());
			
	//	//}

	//	#region GridView version
	//	public ActionResult CustodyLogGridViewPartial()
	//	{
	//		var viewModel = GridViewExtension.GetViewModel("CustodyLogGridView");
	//		if (viewModel == null)
	//			viewModel = CreateGridViewModel();

	//		return GridCustomActionCore(viewModel);

	//		//return PartialView("_CustodyLogGridViewPartial", GetCustodyLogViewModels());
	//	}

	//	//public ActionResult CustodyLogCustomActionPartial(string customAction)
	//	//{
	//	//	return CustodyLogGridViewPartial();
	//	//}

	//	private PartialViewResult GridCustomActionCore(GridViewModel gridViewModel)
	//	{
	//		gridViewModel.ProcessCustomBinding(
	//			GetDataRowCount,
	//			GetDataRow,
	//			null,
	//			null,
	//			GetUniqueHeaderFilterValuessAdvanced
	//		);
	//		return PartialView("_CustodyLogGridViewPartial", gridViewModel);
	//	}

	//	private void GetDataRowCount(GridViewCustomBindingGetDataRowCountArgs e)
	//	{
	//		e.DataRowCount = _custodyLogService.CustodyLogsCount(e.FilterExpression);
	//	}

	//	private void GetDataRow(GridViewCustomBindingGetDataArgs e)
	//	{
	//		e.Data = Mapper.Map<IEnumerable<CustodyLogViewModel>>(_custodyLogService.GetCustodyLogs(
	//			new PageParams
	//			{
	//				StartRowIndex = e.StartDataRowIndex,
	//				RowCount = e.DataRowCount
	//			},
	//			e.State.SortedColumns.Count == 0
	//				? new SortParams { OrderBy = "LogDateTime" }
	//				: new SortParams
	//				{
	//					OrderBy = Mapper.Instance.GetDestinationPropertyFor<CustodyLogViewModel, CustodyLogDTO>(
	//						e.State.SortedColumns.FirstOrDefault()?.FieldName),
	//					Ascending = !e.State.SortedColumns.FirstOrDefault().SortOrder
	//						.HasFlag(ColumnSortOrder.Descending)
	//				},
	//			e.State.AppliedFilterExpression
	//		));
	//	}

	//	public static void GetUniqueHeaderFilterValuessAdvanced(GridViewCustomBindingGetUniqueHeaderFilterValuesArgs e)
	//	{
	//		//e.Data = Model
	//		//    .ApplyFilter(e.FilterExpression)
	//		//    .UniqueValuesForField(e.FieldName);
	//	}

	//	private static GridViewModel CreateGridViewModel()
	//	{
	//		var viewModel = new GridViewModel();
	//		viewModel.KeyFieldName = "Id";
	//		viewModel.Columns.Add("Sample.Name");
	//		viewModel.Columns.Add("StatusId");
	//		viewModel.Columns.Add("LogDateTime");
	//		viewModel.Columns.Add("LocationId");
	//		viewModel.Columns.Add("AspNetUserId");
	//		viewModel.Pager.PageSize = 15;

	//		return viewModel;
	//	}

	//	public ActionResult CustodyLogsGridViewSortingAction(GridViewColumnState column, bool reset)
	//	{
	//		var viewModel = GridViewExtension.GetViewModel("CustodyLogGridView");
	//		viewModel.SortBy(column, reset);
	//		return GridCustomActionCore(viewModel);
	//	}

	//	public ActionResult CustodyLogsGridViewPagingAction(GridViewPagerState pager)
	//	{
	//		var viewModel = GridViewExtension.GetViewModel("CustodyLogGridView");
	//		viewModel.Pager.Assign(pager);
	//		return GridCustomActionCore(viewModel);
	//	}

	//	public ActionResult CustodyLogsGridViewFilteringAction(GridViewFilteringState filteringState)
	//	{
	//		var viewModel = GridViewExtension.GetViewModel("CustodyLogGridView");
	//		viewModel.ApplyFilteringState(filteringState);
	//		return GridCustomActionCore(viewModel);
	//	}
	//	#endregion

	//	#region CardView version
	//	//public ActionResult CustodyLogViewPartial()
	//	//{
	//	//	//var viewModel = GridViewExtension.GetViewModel("CustodyLogViewPartial");
	//	//	//if (viewModel == null)
	//	//	//	viewModel = CreateGridViewModel();

	//	//	//viewModel
	//	//	return PartialView("_CustodyLogViewPartial", GetCustodyLogViewModels());
	//	//}
	//	#endregion
	//}

}