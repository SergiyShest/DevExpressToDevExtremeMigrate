using Entity;
using Sasha.Lims.DataAccess.Entities.Entity;
using Entity.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace UI.Areas.Common.Controllers {

    [Area("Common")]
    public class CustodyLogCardController : GenericCardController<vCustodyLog>
    {
        public CustodyLogCardController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor) { }

    }
}