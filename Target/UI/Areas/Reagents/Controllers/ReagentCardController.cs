using Entity;
using Sasha.Lims.DataAccess.Entities.Entity;
using Entity.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace UI.Areas.Reagents.Controllers {

    [Area("Reagents")]
    public class ReagentCardController : GenericCardController<vReagent>
    {
        public ReagentCardController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor) { }

    }
}