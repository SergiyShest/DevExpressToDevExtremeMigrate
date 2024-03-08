using Entity;
using Sasha.Lims.DataAccess.Entities.Entity;
using Entity.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace UI.Controllers {

    
    public class SamplesBookCardController : GenericCardController<vSamplesBook>
    {
        public SamplesBookCardController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor) { }

    }
}