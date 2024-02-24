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
using Box = DevExpress.XtraCharts.Native.Box;

namespace Sasha.Lims.WebUI.Areas.SamplesEdit.Controllers
{
	public class ContainerJournalController : BaseController
	{
		private IRepos<vContaiter> _containerRepos;
		private IRepos<vSamplesBook> _samplesRepos;
		private IRepos<vContaiterDraft> _draftRepos;

		private ISamplesService _samplesService;
		public ContainerJournalController(ISamplesService samplesService) : base()
		{
			_samplesService = samplesService;
			IUnitOfWorkEx uow = new UnitOfWork();
			_containerRepos = uow.GetRepository<vContaiter>();
			_samplesRepos = uow.GetRepository<vSamplesBook>();
			_draftRepos = uow.GetRepository<vContaiterDraft>();

		}
		public ActionResult CallbackRouteValues(string customAction, string selectedIds, string newValue,bool? groupMode)
		{
			var ids = CoreHelper.ConvertIdsToArr(selectedIds);
			var sb = new StringBuilder();
			ViewBag.GroupMode = groupMode;
			UserDTO user = GetCurrentUser();
			switch (customAction)
			{
				case "changeStatus":
					foreach (var id in ids)
					{
						try
						{
							var sample = SampleManager.Current.GetSample(id);
							sample.ChangeStatus(newValue);
							sample.Save(user);
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
							var draft = SampleManager.Current.GetSample(id);
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
					foreach (var id in ids)
					{
						try
						{
							var sample = SampleManager.Current.GetSample(id) as RBox;
							sample.MakeAliquote(user);
						//	sample.Save(user);
						}
						catch (Exception ex)
						{
							sb.AppendLine(ex.Message);
						}
					}
					break;
				case "commitEdit":
					foreach (var id in ids)
					{
						try
						{
						
                            var draft = SampleManager.Current.GetDraftBySample(id,JournalType.Box) as BoxDraft;
							draft.ApplyEdit(user);

						}
						catch (Exception ex)
						{
							sb.AppendLine(ex.Message);
						}
					}
					break;
				case "undoEdit":
					foreach (var id in ids)
					{
						try
						{
							var draft = SampleManager.Current.GetDraftBySample(id,JournalType.Box) as BoxDraft;
						    draft.Delete(user);
						}
						catch (Exception ex)
						{
							sb.AppendLine(ex.Message);
						}
					}
					break;

				case "sendReport":
					SafeExecute(() => _samplesService.SendReport(newValue, ids));
					break;
				case "delete":
					foreach (var id in ids)
					{
						try
						{
							var sample = SampleManager.Current.GetSample(id) ;
							sample.Delete(user);
						}
						catch (Exception ex)
						{
							sb.AppendLine(ex.Message);
						}
					}
					break;
			}

			return PartialView("_GridViewPartial", GetModel(false));
		}

		public ActionResult ContainerJournal()
		{
			return View("ContainerJournal", GetModel(true));
		}

	
		public ActionResult ContainerDraftPartial(int id)
		{
			ViewData["ContainerId"] = id;
			var model= _draftRepos.GetAll().Where(x => x.SampleId==id && x.TypeId == (int) JournalType.Box );
			return View("_ContainerDraftPartial", model);
		}

		object GetModel(bool returnNull = false)
		{
			if (returnNull) return null;
			return _containerRepos.GetAll().Where(x => (x.ModelId == (int) SampleModel.Box || x.ModelId == (int) SampleModel.Plate));//&&x.UserId==usId
		}
	}


}