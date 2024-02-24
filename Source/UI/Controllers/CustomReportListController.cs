using System.Web.Mvc;
using Sasha.Lims.Core.Interfaces.Common;
using Sasha.Lims.WebUI.Models;

namespace Sasha.Lims.WebUI.Controllers
{
    public class CustomReportListController : BaseController
	{
		private readonly IPrintService _printService;

		public CustomReportListController(IPrintService printService)
		{
			_printService = printService;
		}

		[ChildActionOnly]
		public ActionResult Index(int id, string tableName, string customParameter = null)
		{
			CustomReportListVM vm = new CustomReportListVM()
			{
				Properties = _printService.GetReportProperties(tableName),
				CustomParameter = customParameter,
				RecordId = id,
				TableName = tableName,
			};
			return PartialView(vm);
        }
    }
}