using System.Collections.Generic;
using System.Linq;
using Sasha.Lims.Core.DTO.Common;
using Sasha.Lims.Core.DTO.Customization;
using Sasha.Lims.WebUI.Infrastructure.ViewModelHelpers;

namespace Sasha.Lims.WebUI.Controllers
{
	public abstract class CustomFieldsController : BaseController 
	{
		protected CustomFieldsController(IEnumerable<(UserFieldDTO, UserFieldTypeDTO, PropertyDTO)> customFields)
		{
			CustomFields = customFields.Select(MakeMetadata).ToList().AsReadOnly();
		}

		public IReadOnlyList<ViewModelFieldMetadata> CustomFields { get; }

		ViewModelFieldMetadata MakeMetadata((UserFieldDTO, UserFieldTypeDTO, PropertyDTO) userField)
		{
			return ViewModelFieldMetadata.FromUserFields(userField.Item1, userField.Item2, userField.Item3);
		}
	}

}