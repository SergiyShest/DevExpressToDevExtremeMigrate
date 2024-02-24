using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using DevExpress.DataProcessing.InMemoryDataProcessor;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Sasha.Lims.Core.BusinessObjects;
using Sasha.Lims.Core.DTO.Access;
using Sasha.Lims.Core.Helpers;
using Sasha.Lims.Core.Interfaces.Laboratory;
using Sasha.Lims.DataAccess.Entities.Constants;
using Sasha.Lims.DataAccess.Entities.Entity;
using Sasha.Lims.DataAccess.Repositories;
using Sasha.Lims.DataAccess.Repositories.Abstract;
using Sasha.Lims.WebUI.Controllers;

namespace Sasha.Lims.WebUI.Areas.SamplesEdit.Controllers
{
	public class ContainerContentController : BaseController
	{
		private IRepos<vSamplesBook> _sampleRepos;
		private IRepos<vJourLine> _drartRepos;

         private ISamplesService _samplesService;
		public ContainerContentController(ISamplesService samplesService) : base()
		{
			_samplesService = samplesService;
			IUnitOfWorkEx uow = new UnitOfWork();
			_sampleRepos = uow.GetRepository<vSamplesBook>();
			_drartRepos = uow.GetRepository<vJourLine>();

		}
		public ActionResult CallbackRouteValues(int? containerId,string customAction, string selectedIds, string newValue)
		{
			var ids = CoreHelper.ConvertIdsToArr(selectedIds);
			var sb = new StringBuilder();

			UserDTO user = GetCurrentUser();
			switch (customAction)
			{

				case "process":
					
					foreach (var id in ids)
					{
						try
						{
							var draft = SampleManager.Current.GetDraft(id);
							draft.ApplyEdit(user);
						}
						catch (Exception ex)
						{
							sb.AppendLine(ex.Message);
						}
					}
					//	SafeExecute(() => _samplesService.PostJournal(editMode, user, ids));
					break;

				    case "changeStatus":

					foreach (var id in ids)
					{
						try
						{
							var draft = SampleManager.Current.GetDraft(id);
							draft.ChangeStatus(newValue) ;
                            draft.Save(user);
						}
						catch (Exception ex)
						{
							sb.AppendLine(ex.Message);
						}
					}

					break;

				case "assignLocation":
					foreach (var id in ids)
					{
						try
						{
							var draft = SampleManager.Current.GetDraft(id);
							draft.ChangeLocation(newValue);
							draft.Save(user);
						}
						catch (Exception ex)
						{
							sb.AppendLine(ex.Message);
						}
					}
					break;
				case "makeAliquot":
					//	SafeExecute(() => _samplesService.MakeAliquot(newValue, ids));
					break;
				case "sendReport":
					SafeExecute(() => _samplesService.SendReport(newValue, ids));
					break;
			}

			return PartialView("_ContainerContentPartial", GetModel(containerId, false));
		}

		public ActionResult ContainerContentJournal(int containerId,string barcode)
		{
			ViewBag.RowId = containerId;
			ViewBag.Barcode = barcode;
			return View("ContainerContentJournal", GetModel(containerId, true));
		}

		object GetModel( int? id, bool returnNull = false)
		{
			if (returnNull) return null;
			return _sampleRepos.GetAll().Where(x => x.BatchId==id);//&&x.UserId==usId
		}




	public ActionResult CallbackDraftRouteValues(int? containerId)
		{

			return PartialView("_ContainerDraftContentPartial", GetDraftModel(containerId, false));
		}

		public ActionResult ContainerDraftContentJournal(int containerId,string barcode)
		{
			ViewBag.RowId = containerId;
			ViewBag.Barcode = barcode;
			return View("ContainerDraftContentJournal", GetDraftModel(containerId, true));
		}

		object GetDraftModel( int? id, bool returnNull = false)
		{
			if (returnNull) return null;
			return _drartRepos.GetAll().Where(x => x.BatchId==id && x.TypeId==(int)JournalType.Box );//
		}


	}


}