using Entity;
using Sasha.Lims.DataAccess.Entities.Entity;
using Entity.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace UI.Areas.Workflow.Controllers {

    [Area("Workflow")]
    public class RunCardController : GenericCardController<>
    {
        public RunCardController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor) { }

    }
}