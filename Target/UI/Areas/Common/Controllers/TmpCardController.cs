using Entity;
using Sasha.Lims.DataAccess.Entities.Entity;
using Sasha.Lims.DataAccess.Entities.Entity;
using Entity.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace UI.Areas.Common.Controllers {

    [Area("Common")]
    public class TmpCardController : GenericCardController<vPatient>
    {
        public TmpCardController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor) { }

    }
}