using System.Linq;
using System.Web.Mvc;
using Sasha.Lims.Core.Interfaces.Common;
using Sasha.Lims.DataAccess.Entities.Constants;

namespace Sasha.Lims.WebUI.Controllers.BaseAndCore
{
    public class HelpController : BaseController
	{
		private IPropertiesService _propertiesService;

		public HelpController(IPropertiesService propertiesService)
		{
			_propertiesService = propertiesService;
		}

		public ActionResult About()
		{
			string databaseVersion = _propertiesService.GetAll()
				.FirstOrDefault(x => x.ParentId == (int) PropsType.ProductInfo && x.Code == "DB_VERSION")
				?.Value;

			System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
			var name = assembly.GetName().Version;

			return View(model:databaseVersion);
        }
    }
}