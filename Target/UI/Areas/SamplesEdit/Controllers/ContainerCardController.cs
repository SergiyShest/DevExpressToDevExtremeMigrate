using Entity;
using Sasha.Lims.DataAccess.Entities.Entity;
using Entity.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace UI.Areas.SamplesEdit.Controllers {

    [Area("SamplesEdit")]
    public class ContainerCardController : GenericCardController<vContaiter>
    {
        public ContainerCardController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor) { }

    }
}