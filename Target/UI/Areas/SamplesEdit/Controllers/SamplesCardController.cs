using Entity;
using Sasha.Lims.DataAccess.Entities.Entity;
using Sasha.Lims.DataAccess.Entities.Entity;
using Entity.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace UI.Areas.SamplesEdit.Controllers {

    [Area("SamplesEdit")]
    public class SamplesCardController : GenericCardController<vJourLine>
    {
        public SamplesCardController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor) { }

    }
}