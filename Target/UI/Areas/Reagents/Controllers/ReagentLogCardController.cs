using Entity;
using Sasha.Lims.DataAccess.Entities.Entity;
using Sasha.Lims.DataAccess.Entities.Entity;
using Entity.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace UI.Areas.Reagents.Controllers {

    [Area("Reagents")]
    public class ReagentLogCardController : GenericCardController<vReagentLog>
    {
        public ReagentLogCardController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor) { }

    }
}