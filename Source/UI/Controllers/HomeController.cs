using System.Threading.Tasks;
using Sasha.Lims.WebUI.Models;
using System.Web.Mvc;
using Sasha.Lims.Core.BusinessObjects;
using Sasha.Lims.DataAccess.Entities.Constants;

namespace Sasha.Lims.WebUI.Controllers
{
    [Authorize]
    public class HomeController : BaseController
	{

        public HomeController()
        {

		}

        // GET: Home
        public async Task<ActionResult>  Index()
		{
			var fm = new FileUpdater();//обновление ссылок на файлы возможно не лучшее место но не придумал пока ни чего более.
			//правильней всего сделать службу которая бы крутилась постоянно и синхронизировала данные
			await fm.RefreshDataBaseAsinc();

			//return Redirect("SamplesEdit/SamplesJournal/SamplesJournal?editMode=" + JournalType.Receiving);
			// return Redirect("SamplesEdit/ContainerJournal/ContainerJournal");
			return View( "Index");
		}

    }
}