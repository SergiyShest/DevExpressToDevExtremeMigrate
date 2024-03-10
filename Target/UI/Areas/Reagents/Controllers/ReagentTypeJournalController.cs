using Entity;
using Sasha.Lims.DataAccess.Entities.Entity;
using Entity.Controllers;
using Microsoft.AspNetCore.Mvc;
namespace UI.Areas.Reagents.Controllers{
    [Area("Reagents")]
    public class ReagentTypeJournalController : GenericJournalController<vReagentType>
    {
        public ReagentTypeJournalController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor) { }

    }
}