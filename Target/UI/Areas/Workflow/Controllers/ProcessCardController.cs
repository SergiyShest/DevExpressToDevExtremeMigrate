using Entity;
using Sasha.Lims.DataAccess.Entities.Entity;
using Entity.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace UI.Areas.Workflow.Controllers {

    [Area("Workflow")]
    public class ProcessCardController : GenericCardController<>
    {
        public ProcessCardController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor) { }

    }
}