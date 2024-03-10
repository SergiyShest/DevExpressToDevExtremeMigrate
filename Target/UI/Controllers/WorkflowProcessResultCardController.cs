using Entity;
using Sasha.Lims.DataAccess.Entities.Entity;
using Entity.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace UI.Controllers {

    
    public class WorkflowProcessResultCardController : GenericCardController<vProcessResult>
    {
        public WorkflowProcessResultCardController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor) { }

    }
}