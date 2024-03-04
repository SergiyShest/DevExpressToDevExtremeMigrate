using Entity;
using Sasha.Lims.DataAccess.Entities.Entity;
using Sasha.Lims.DataAccess.Entities.Entity;
using Entity.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace UI.Areas.Common.Controllers {

    [Area("Common")]
    public class PatientCardController : GenericCardController<vPatient>
    {
        public PatientCardController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor) { }

    }
}