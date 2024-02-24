using System.Linq;
using System.Web.Mvc;
using Portable.Licensing;
using Portable.Licensing.Validation;
using Sasha.Lims.Core.Helpers;
using Sasha.Lims.Core.Interfaces.Common;
using Sasha.Lims.DataAccess.Entities.Constants;

namespace Sasha.Lims.WebUI.Controllers.BaseAndCore
{
	public class CheckLicenseController : BaseController
	{
		private IPropertiesService _propertiesService;

		public CheckLicenseController(IPropertiesService propertiesService)
		{
			_propertiesService = propertiesService;
		}

		[ChildActionOnly]
		public ActionResult Index()
		{
			return View(model: DataHelper.Current.CheckLicense());
		}


	}
}