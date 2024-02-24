using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using Sasha.Lims.Core.Consts;
using Sasha.Lims.Core.DTO.Access;
using Sasha.Lims.Core.DTO.Common;
using Sasha.Lims.Core.Interfaces.Access;
using Sasha.Lims.Core.Interfaces.Common;
using Sasha.Lims.Core.Interfaces.Laboratory;
using Sasha.Lims.WebUI.Models;

namespace Sasha.Lims.WebUI.Controllers
{
	public abstract class BaseSamplesController : BaseController
	{
		protected IPropertiesService PropertiesService;

		protected IUserService UserService;

		protected ISamplesApprovedService SamplesApprovedService;

		public IEnumerable<PropertyViewModel> GetProperties(string propertyType, int? propertyId = -1)
		{
			IEnumerable<PropertyDTO> result = null;

			if (propertyId.HasValue)
			{
				if (propertyId < 1)
					result = GetKnownProperty(propertyType, result);
				else
					switch (propertyType)
					{
						case "OneLevelProperties":
							result = PropertiesService.GetOneLevelProperties((int) propertyId);
							break;
						default:
							PropertyDTO propertyDto = PropertiesService.Get(propertyId.Value);
							if (propertyDto != null)
								result = new[] { propertyDto };
							break;
					}
			}

			return result?.OrderBy(x => x.Code)
				.Select(item =>
					new PropertyViewModel
					{
						Id = item.Id,
						Code = item.Code,
						Value = item.Value,
						Path = CutRootNodePath(item.Path),
						Name = item.Name,
						DisplayText = item.Name + (item.Value != null ? " (" + item.Value + ")" : "")
					});
		}

		private IEnumerable<PropertyDTO> GetKnownProperty(string propertyType, IEnumerable<PropertyDTO> result)
		{
			switch (propertyType)
			{
				case "TubeType": return PropertiesService.GetTubeTypes();
				case "SampleType": return PropertiesService.GetSampleTypes();
				case "SampleStatus": return PropertiesService.GetSampleStatuses();
				case "ProcessStatus": return PropertiesService.GetWorkflowStatus();
				case "WorkflowType": return PropertiesService.GetWorkflowType();
				case "Location": return PropertiesService.GetLocations();
				case "ValueUnit": return PropertiesService.GetValueUnits();
				case "Models": return PropertiesService.GetModels();
			}

			return result;
		}

		private string CutRootNodePath(string path)
		{
			var rootIndex = path?.IndexOf('/');
			if (rootIndex > -1)
				path = path.Substring(rootIndex.Value + 1);

			return path;
		}

		public IEnumerable<EquipmentViewModel> GetEquipments(int? equipmentId = -1)
		{
			IEnumerable<EquipmentViewModel> result = null;

			if (equipmentId.HasValue)
			{
				if (equipmentId.Value < 1)
				{
					//todo get all equipments
					//result = all equipments
				}
			}

			return result;
		}
	}
}