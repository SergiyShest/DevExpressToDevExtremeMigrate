using Entity;
using Sasha.Lims.DataAccess.Entities.Entity;
using Entity.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace UI.Areas.Common.Controllers {

    [Area("Common")]
    public class HospitalCardController : GenericCardController<vHospital>
    {
        public HospitalCardController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor) { }

    }
}