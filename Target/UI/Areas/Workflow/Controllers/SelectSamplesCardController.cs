using Entity;
using Sasha.Lims.DataAccess.Entities.Entity;
using Entity.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace UI.Areas.Workflow.Controllers {

    [Area("Workflow")]
    public class SelectSamplesCardController : GenericCardController<vSamplesBook>
    {
        public SelectSamplesCardController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor) { }

    }
}