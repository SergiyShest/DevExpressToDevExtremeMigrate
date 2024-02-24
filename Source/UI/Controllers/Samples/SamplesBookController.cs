using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using DevExpress.Web;
using DevExpress.Web.Mvc;
using Newtonsoft.Json;
using Sasha.Lims.Core.DTO.Access;
using Sasha.Lims.Core.DTO.Laboratory;
using Sasha.Lims.Core.Helpers;
using Sasha.Lims.Core.Infrastructure.Classes;
using Sasha.Lims.Core.Interfaces.Access;
using Sasha.Lims.Core.Interfaces.Customization;
using Sasha.Lims.Core.Interfaces.Laboratory;
using Sasha.Lims.Core.Services.Common;
using Sasha.Lims.DataAccess.Entities.Constants;
using Sasha.Lims.WebUI.Infrastructure.Attributes;
using Sasha.Lims.WebUI.Infrastructure.Classes;
using Sasha.Lims.WebUI.Infrastructure.ViewModelHelpers;
using Sasha.Lims.WebUI.Models;

namespace Sasha.Lims.WebUI.Controllers
{
	[Authorize]
	public class SamplesBookController : CustomFieldsController
	{
		private ISamplesBookService _samplesService;

		private ISamplesApprovedService MainService { get; }
		public SamplesBookController(ISamplesBookService samplesService, ISamplesApprovedService samplesApprovedService, IDtoMetadataService metadataService) : base(metadataService.GetUserFieldsForViewModel<SampleApprovedDTO>())
		{
			_samplesService = samplesService;
			MainService = samplesApprovedService;
		}


		public ActionResult CallbackRouteValues(string customAction, string customParams, JournalType editMode = JournalType.Management, string newValue = "", string ids = "")
		{
			UserDTO usr = GetCurrentUser();
			switch (customAction)
			{
				case "MoveToEdit":
					{
						var samplesApprovedIds = customParams.Split(',').Select(RecVersionId<int>.Parse).ToArray();
						MainService.ToEditMode(samplesApprovedIds, usr.Id, editMode, usr.UserId.GetValueOrDefault(0));

						if (editMode == JournalType.Management)
						{
							return RedirectToAction("SamplesJournal", "SamplesJournal", new { area = "SamplesEdit", editMode = editMode });
						}
						else
						{
                            return RedirectToAction("Index", "Samples", new { editMode });//старый интерфейс
						}
						
					}
				case "MoveToAliquoting":
					{
						if (!string.IsNullOrEmpty(customParams))
						{
						List<int> samplesApprovedIds = customParams.Split(',').Select(int.Parse).ToList();
						MainService.ToAliquotingMode(samplesApprovedIds, JournalType.Aliquoting, usr);
						}
						return RedirectToAction("SamplesJournal", "SamplesJournal", new { area = "SamplesEdit", editMode = (int) JournalType.Aliquoting });
					}
				case "MoveToPlate":
					{
						var samplesApprovedIds = customParams.Split(',').Select(RecVersionId<int>.Parse).ToArray();

						MainService.ToPlateMode(samplesApprovedIds,
							GetCurrentUser().Id,
							JournalType.Plate,
							usr.UserId.GetValueOrDefault(0));
						return RedirectToAction("SamplesJournal", "SamplesJournal", new { area = "SamplesEdit", editMode = (int) JournalType.Plate });
						//return RedirectToAction("Index", "SamplesPlate", new { editMode = (int) JournalType.Plate });
					}
				case "MoveToBox":
					{
						var samplesApprovedIds = customParams.Split(',').Select(RecVersionId<int>.Parse).ToArray();

						MainService.ToBoxMode(samplesApprovedIds,
							GetCurrentUser().Id,
							JournalType.Box,
							usr.UserId.GetValueOrDefault(0));
						return RedirectToAction("SamplesJournal", "SamplesJournal", new { area = "SamplesEdit", editMode = (int) JournalType.Box });
					}
				case "Print":
					if (!string.IsNullOrEmpty(customParams))
					{
						var id_s = string.Join(",", customParams.Split(',').Select(x => RecVersionId<int>.Parse(x).Id));
						return RedirectToAction("Book", "Print", new { id_s });
					}
					break;

			}
			return PartialView("_GridViewPartial", _samplesService.Data());
		}

		public ActionResult Index()
		{
		
			return View("Index", null);
		}

	public ActionResult Select(
	int? patientId,
	int? doctorId,
	int? hospitalId,
	int? parentlocationId,
	bool? showSelectCancelButtons,string varName,bool multiSelect=false)
		{
			base.LayoutId = "SamplesBook.Select"+varName;
			ViewBag.DontShowMainMenu =true; 
			var filterExpesson = CreateExpeession("PatientId", patientId, string.Empty,ExpressionType.NumberEquals);
			filterExpesson = CreateExpeession("DoctorId", doctorId, filterExpesson,ExpressionType.NumberEquals);
			filterExpesson = CreateExpeession("HospitalId", hospitalId, filterExpesson,ExpressionType.NumberEquals);

			if (parentlocationId != null)
			{
				var  chailds= DataHelper.Current.Properties.GetDescendants((int)parentlocationId);

				foreach (var child in chailds)
				{
					filterExpesson = CreateExpeession("LocationId", child.id, filterExpesson, ExpressionType.NumberEquals, "or");
				}
			}
			ViewData["filterExpression"] = filterExpesson;
			ViewBag.MultiSelect = multiSelect;
			ViewBag.Title = "Select Sample";
			ViewBag.ShowSelectCancelButtons=showSelectCancelButtons == true;
			ViewBag.varName = varName??"SampleId";
			return View("Index", null);
		}
	}
}