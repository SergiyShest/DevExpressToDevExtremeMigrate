using Sasha.Lims.DataAccess.Entities.Entity;
using Sasha.Lims.DataAccess.Repositories;
using Sasha.Lims.WebUI.Controllers;
using System.Linq;
using System.Web.Mvc;

namespace Sasha.Lims.WebUI.Areas.Reagents.Controllers
{
    public class ReagentLogJournalController : BaseController
    {

        public ActionResult Index()
		{
			return View("Index",null);
		}

		private static IQueryable<vReagentLog> getModel()
		{
			IUnitOfWorkEx uow = new UnitOfWork();
			var ret = uow.GetRepository<vReagentLog>().GetAll();
			return ret;
		}

		public ActionResult CallbackRouteValues(string filterExpression, string selectedIds)
		{


			ViewData["filterExpression"] = filterExpression;
			return PartialView("_GridViewPartial", getModel());
		}

	}
}