using AutoMapper;
using DevExpress.Data;
using DevExpress.Web.Mvc;
using Sasha.Lims.Core.DTO.Common;
using Sasha.Lims.Core.DTO.Customization;
using Sasha.Lims.Core.Infrastructure.Classes;
using Sasha.Lims.Core.Infrastructure.Extentions;
using Sasha.Lims.Core.Interfaces;
using Sasha.Lims.Core.Interfaces.Customization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Sasha.Lims.WebUI.Controllers




{
	public abstract class GridBasedController : CustomFieldsController
	{
		
		protected GridBasedController(IEnumerable<(UserFieldDTO, UserFieldTypeDTO, PropertyDTO)> customFields):base(customFields)
		{
			
		}


		public abstract string GetGridViewName();
		protected GridViewModel CreateGridViewModel()
		{
			var vm = CreateGridViewModelImpl();
			foreach(var c in CustomFields)
				vm.Columns.Add(c.Name);
			return vm;
		}

		protected abstract GridViewModel CreateGridViewModelImpl();

		#region SortingFilteringPaging
		private void ClearColumnSorting(GridViewModel viewModel)
		{
			if ((viewModel?.SortedColumns?.Count ?? 0) > 0)
				viewModel.Columns.ForEach(uc => uc.SortIndex = -1);
		}

		public ActionResult GridViewPartial()
		{
			var viewModel = GridViewExtension.GetViewModel(GetGridViewName());
			if (viewModel != null)
			{
				ClearColumnSorting(viewModel);
				var column = new GridViewColumnState
				{
					FieldName = "Id",
					SortOrder = ColumnSortOrder.Descending
				};
				viewModel.SortBy(column, true);
			}
			return GridCustomActionCore(viewModel ?? CreateGridViewModel());
		}
		

		public ActionResult GridViewSortingAction(GridViewColumnState column, bool reset)
		{
			var viewModel = GridViewExtension.GetViewModel(GetGridViewName());

			viewModel.SortBy(column, reset);

			return GridCustomActionCore(viewModel);
		}

		public ActionResult GridViewPagingAction(GridViewPagerState pager)
		{
			var viewModel = GridViewExtension.GetViewModel(GetGridViewName());
			viewModel.Pager.Assign(pager);
			return GridCustomActionCore(viewModel);
		}

		public ActionResult GridViewFilteringAction(GridViewFilteringState filteringState)
		{
			var viewModel = GridViewExtension.GetViewModel(GetGridViewName());
			ClearColumnSorting(viewModel);
			viewModel.ApplyFilteringState(filteringState);
			return GridCustomActionCore(viewModel);
		}

		protected virtual PartialViewResult GridCustomActionCore(GridViewModel gridViewModel)
		{
			gridViewModel.ProcessCustomBinding(GetDataRowCount, GetData, null, null, GetUniqueHeaderFilterValuesAdvanced);
			return PartialView("GridViewPartial", gridViewModel);
		}


		public virtual void GetUniqueHeaderFilterValuesAdvanced(GridViewCustomBindingGetUniqueHeaderFilterValuesArgs e)
		{
		}
		#endregion

		#region FetchData
		protected abstract void GetDataRowCount(GridViewCustomBindingGetDataRowCountArgs e);

		protected abstract void GetData(GridViewCustomBindingGetDataArgs e);
		#endregion
	}

	public abstract class GridBasedController<TViewModel, TDTO, TService> : GridBasedController where TService : IDtoService<TDTO> where TDTO : class
	{
		protected readonly TService MainService;

		protected GridBasedController(TService mainService) : base(DependencyResolver.Current.GetService<IDtoMetadataService>().GetUserFieldsForViewModel<TDTO>())
		{
			MainService = mainService;
		}

		#region FetchData
		protected override void GetDataRowCount(GridViewCustomBindingGetDataRowCountArgs e)
		{
			e.DataRowCount = MainService.Count(e.FilterExpression);
		}

		protected override void GetData(GridViewCustomBindingGetDataArgs e)
		{
			SortParams sort = e.State.SortedColumns.Count < 1
				? new SortParams { OrderBy = "Id", Ascending = false }
				: new SortParams {
					OrderBy = Mapper.Instance.GetDestinationPropertyFor<TViewModel, TDTO>(e.State.SortedColumns.FirstOrDefault()?.FieldName),
					Ascending = !e.State.SortedColumns.FirstOrDefault().SortOrder.HasFlag(ColumnSortOrder.Descending)
				};

			PageParams page = new PageParams { StartRowIndex = e.StartDataRowIndex, RowCount = e.DataRowCount };
			e.Data = Mapper.Map<IEnumerable<TViewModel>>(MainService.Get(page, sort, e.State.AppliedFilterExpression));
		}
		#endregion



		

		#region CreateUpdateDelete
		public virtual ActionResult DetailsPartial(int? id) => base.PartialView("_DetailsPartial", Mapper.Map<TViewModel>(id == null ? null : MainService.Get(id.Value)));

		public virtual ActionResult GridViewPartialAddNew(TViewModel model) => UpdateModelWithDataValidation(Mapper.Map<TDTO>(model), MainService.Create);
		public virtual ActionResult GridViewPartialUpdate(TViewModel model) => UpdateModelWithDataValidation(Mapper.Map<TDTO>(model), MainService.Edit);

		protected ActionResult UpdateModelWithDataValidation(TDTO dto, Action<TDTO> updateMethod)
		{
			if (ModelState.IsValid)
				SafeExecute(() => updateMethod(dto));
			else
				ViewBag.GeneralError = "Please, correct all errors.";
			return GridViewPartial();
		}

		protected void PerformDelete()
		{
			var selectedRowIds = Request.Params["SelectedRows"];
			if (!string.IsNullOrEmpty(selectedRowIds))
			{
				List<int> selectedIds = selectedRowIds.Split(',').ToList().ConvertAll(id => int.Parse(id));
				MainService.Delete(selectedIds);
			}
		}
		#endregion
	}
}