using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using AutoMapper;
using DevExpress.Web;
using GroupChange;
using Newtonsoft.Json;
using Sasha.Lims.Core.BusinessObjects;
using Sasha.Lims.Core.DTO.Access;
using Sasha.Lims.Core.DTO.Laboratory;
using Sasha.Lims.Core.Helpers;
using Sasha.Lims.Core.Infrastructure.Classes;
using Sasha.Lims.Core.Interfaces.Customization;
using Sasha.Lims.Core.Interfaces.Laboratory;
using Sasha.Lims.Core.Services.Common;
using Sasha.Lims.DataAccess.Entities.Constants;
using Sasha.Lims.DataAccess.Entities.Entity;
using Sasha.Lims.DataAccess.Repositories;
using Sasha.Lims.DataAccess.Repositories.Abstract;
using Sasha.Lims.WebUI.Controllers;
using Sasha.Lims.WebUI.Models;

namespace Sasha.Lims.WebUI.Areas.SamplesEdit.Controllers
{
	[Authorize]
	public class SamplesJournalController : CustomFieldsController
	{
		private IRepos<vJourLine> _jourLineRepos;

		private ISamplesApprovedService _samplesApprovedService;

		private ISamplesService _samplesService;

		public SamplesJournalController(ISamplesService mainService, ISamplesApprovedService samplesApprovedService, IDtoMetadataService metadataService) : base(metadataService.GetUserFieldsForViewModel<SampleApprovedDTO>())
		{
			 IUnitOfWorkEx  uow= new UnitOfWork();
			_jourLineRepos= uow.GetRepository<vJourLine>();
			_samplesService = mainService;
			_samplesApprovedService = samplesApprovedService;
		}

		private void PostJournal(string  ids)
		{
			var model = new GroupEditModel( ids, new UnitOfWork());
			 var errors=model.ApplayEdit(GetCurrentUser());
			if (errors.Any())
			{
				var resError = string.Join(";",errors);
				throw new ApplicationException(resError);
			}
		}

		public ActionResult CallbackRouteValues(string customAction, string selectedIds, string newValue, JournalType editMode = JournalType.Management)
		{
		var ids= 	CoreHelper.ConvertIdsToArr(selectedIds);

		UserDTO user;
			switch (customAction)
			{
				case "process":
					SafeExecute(() => this.PostJournal(selectedIds));
					break;
				case "assignWells":
                    user = GetCurrentUser();
					SafeExecute(() => _samplesService.AssignWells(ids,(int) user.UserId));
					break;
				case "changeStatus":
					SafeExecute(() => _samplesService.ChangeStatus(newValue, ids));
					break;
				case "changeBatch":
					SafeExecute(() => _samplesService.ChangeBatch(newValue, ids));
					break;
				case "assignLocation":
					SafeExecute(() => _samplesService.ChangeLocation(newValue, ids));
					break;
				case "makeAliquot":
					SafeExecute(() => _samplesService.MakeAliquot(newValue, ids));
					break;
				case "sendReport":
					SafeExecute(() => _samplesService.SendReport(newValue, ids));
					break;
			}

			return PartialView("_GridViewPartial", GetModel(editMode));
		}

		public ActionResult SamplesJournal(JournalType? editMode)
		{
			if (editMode == null)
			{
				editMode = JournalType.Management;
			}


			return View("SamplesJournal", GetModel((JournalType)editMode,true) );
		}

		object GetModel(JournalType editMode,bool returnNull=false)
		{			JournalType[] noEditSampleModes = { JournalType.MassCheckOut, JournalType.MassCheckIn, JournalType.MassMove, JournalType.MassDispose, JournalType.Box };

			ViewBag.EditMode = editMode;
			ViewBag.CanEditSamples = !noEditSampleModes.Contains(editMode);
			var usId = GetCurrentUser().UserId;
			return _jourLineRepos.GetAll().Where(x => x.TypeId == (int) editMode && x.UserId == usId);
		}

		public ActionResult AddSamplesByBarCode(string samplesBarcodes, string fieldName, string selectedIds, JournalType editMode = JournalType.Management)
		{
			var barcodes = samplesBarcodes?.Replace(" ", "").Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

			if (barcodes?.Length > 0)
			{
				UserDTO usr = GetCurrentUser();
			//	if (string.IsNullOrWhiteSpace(selectedIds))
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
			return RedirectToAction("SamplesJournal", new { editMode = (int) editMode });
		}

		public ActionResult AddSamplesByJJIBarcode(string barcode)
		{
			List<string> errors = new List<string>();
			dynamic jsonData= new ExpandoObject();		 
			try
			{
				var error = _samplesService.AddSamplesByJJIBarcode(barcode, base.GetCurrentUser(),UOW);
				jsonData._status = 200;
				if (!string.IsNullOrWhiteSpace(error)) {
					errors.Add(error);
				jsonData._status = 404;
				}

			}
			catch (Exception ex) 
			{
				jsonData._status = 404;
				errors.Add(ex.GetAllMessages());

			}
            jsonData.Errors=errors;
            jsonData.requestKey = barcode ;

			var json = JsonConvert.SerializeObject(jsonData);

			return Content(json, "application/json");
		}

		#region Populate List


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

	}
}