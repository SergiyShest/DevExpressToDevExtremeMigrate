using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using AutoMapper;
using DevExpress.Web.ASPxTreeList;
using DevExpress.XtraPivotGrid.Utils.DateHelpers;

using Microsoft.AspNet.Identity.Owin;
using Sasha.Lims.Core.DTO.Common;
using Sasha.Lims.Core.Helpers;
using Sasha.Lims.Core.Infrastructure;
using Sasha.Lims.Core.Interfaces.Access;
using Sasha.Lims.Core.Interfaces.Common;
using Sasha.Lims.DataAccess.Entities.Entity;
using Sasha.Lims.DataAccess.Repositories;
using Sasha.Lims.WebUI.Models;

namespace Sasha.Lims.WebUI.Controllers
{
	
	public class SettingsController : BaseController
	{
		private IPropertiesService _service;

		public SettingsController(IPropertiesService service)
		{
			_service = service;
		}

		public ActionResult Index()
		{
			return View();
		}

		[ValidateInput(false)]
		public ActionResult TreeListPartial()
		{
			var model = new UnitOfWork().GetRepository<vProperty>().GetAll().ToList();

		//	var model = Mapper.Map<IEnumerable<Models.PropertyModel>>(_service.GetAll());

			return PartialView("_TreeListPartial", model);
		}

		[ValidateInput(false)]
		public ActionResult PropertiesTreeListPartialAddNew(Models.PropertyModel model)
		{
			if (ModelState.IsValid)
			{
				var result = _service.Create(Mapper.Map<PropertyDTO>(model));

				if (!result.Succeeded)
				{
					AddErrorsFromResult(result);
				} else
				{
					DataHelper.ResetCache();
				}
			}



			return TreeListPartial();
		}

		[ValidateInput(false)]
		public ActionResult PropertiesTreeListPartialUpdate(ExtendedPropertyModel model)
		{
			if (ModelState.IsValid)
			{
				if (model.Code == PropertyType.MenuItem.ToString())
				{
					model.MenuItem.SerializeValue();
				}
				if (model.Code == PropertyType.ObjectEditSetting.ToString())
				{
					model.EditSampleSetting.SerializeValue();
				}
				if (model.Code == PropertyType.ReagentTemplate.ToString())
				{
					model.ReagentTemplate.SerializeValue();
				}
				var result = _service.Update(Mapper.Map<PropertyDTO>(model));

				if (!result.Succeeded)
				{
					AddErrorsFromResult(result);
				} else
				{
					DataHelper.ResetCache();
				}
			}



			return TreeListPartial();
		}

		[ValidateInput(false)]
		public ActionResult PropertiesTreeListPartialDelete(int id)
		{
			var ids = _service.GetAllChildren(new List<int> { id });

			if (ids?.Count() > 0)
			{
				foreach (var idItem in ids)
				{
					var result = _service.Delete(idItem);

					if (!result.Succeeded)
					{
						AddErrorsFromResult(result);
					}
				}
			}

		

			return TreeListPartial();
		}

		public void ClearCache()
		{
			DataHelper.ResetCache();
		}


		private void AddErrorsFromResult(OperationDetails operation)
		{
			ViewData["EditError"] = string.Join("; ", operation.Messages);
		}
		public List<string> GetRoles()
		{
			IUserService userService = HttpContext.GetOwinContext().GetUserManager<IUserService>();
			var roles = userService.Roles.Select(x => x.Name).ToList();

			return roles;
		}

		public static class PropertiesReader
		{

			public static List<KeyValuePair<int, string>> GetValueNamePair(int propertyCode)
			{
				return GetPropertiesByCode(propertyCode)
					.Select(item => new KeyValuePair<int, string>(Convert.ToInt32(item.Value), item.Name))
					.ToList();
			}

			public static List<KeyValuePair<int, string>> GetProperties(string propertyType)
			{
				return GetPropertiesByType(propertyType)
					.Select(item => new KeyValuePair<int, string>(item.Id, item.Name + (item.Value != null ? " (" + item.Value + ")" : "")))
					.ToList();
			}

			public static List<KeyValuePair<int, string>> GetProperties(int rootCode)
			{
				return GetPropertiesByCode(rootCode)
					.Select(item => new KeyValuePair<int, string>(item.Id,
						item.Name + (!string.IsNullOrWhiteSpace(item.Value) ? " (" + item.Value + ")" : "")))
					.ToList();
			}

			private static IEnumerable<PropertyDTO> GetPropertiesByType(string propertyType)
			{
				IPropertiesService _propertiesService = DependencyResolver.Current.GetService<IPropertiesService>();

				switch (propertyType)
				{
					case "TubeType": return _propertiesService.GetTubeTypes();
					case "SampleType": return _propertiesService.GetSampleTypes();
					case "SampleStatus": return _propertiesService.GetSampleStatuses();
					case "ProcessStatus": return _propertiesService.GetWorkflowStatus();
					case "Location": return _propertiesService.GetLocations();
					case "Models": return _propertiesService.GetModels();
					case "WorkflowType": return _propertiesService.GetWorkflowType();
					case "ValueUnit": return _propertiesService.GetValueUnits();
					case "ReagentType": return _propertiesService.GetReagentKitTypes();
				}

				return null;
			}


			private static IEnumerable<PropertyDTO> GetPropertiesByCode(int code) =>
				DependencyResolver.Current.GetService<IPropertiesService>().GetOneLevelProperties(code);

			private static string CutRootNodePath(string path)
			{
				var rootIndex = path?.IndexOf('/');

				if (rootIndex > -1)
					path = path.Substring(rootIndex.Value + 1);

				return path;
			}

			public static IEnumerable<PropertyViewModel> GetProperties(string propertyType, int? propertyId = -1)
			{
				IEnumerable<PropertyDTO> result = null;

				if (propertyId.HasValue)
				{
					if (propertyId < 1)
						result = GetPropertiesByType(propertyType) ?? result;
					else
					{
						IPropertiesService _propertiesService = DependencyResolver.Current.GetService<IPropertiesService>();

						switch (propertyType)
						{
							case "OneLevelProperties":
								result = _propertiesService.GetOneLevelProperties((int) propertyId);

								break;

							default:
								PropertyDTO propertyDto = _propertiesService.Get(propertyId.Value);

								if (propertyDto != null)
									result = new[] { propertyDto };

								break;
						}
					}
				}

				return result?.OrderBy(x => x.Code).Select(makeVmFromDto);
			}

			private static PropertyViewModel makeVmFromDto(PropertyDTO item)
			{
				return new PropertyViewModel
				{
					Id = item.Id,
					Code = item.Code,
					Value = item.Value,
					Path = CutRootNodePath(item.Path),
					Name = item.Name,
					DisplayText = item.Name + (item.Value != null ? " (" + item.Value + ")" : "")
				};
			}


		}
	}








}