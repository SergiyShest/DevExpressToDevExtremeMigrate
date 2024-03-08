using Entity;
using Sasha.Lims.DataAccess.Entities.Entity;
using Entity.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace UI.Areas.Common.Controllers {

    [Area("Common")]
    public class DoctorCardController : GenericCardController<vDoctor>
    {
        public DoctorCardController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor) { }

    }
}