using Entity;
using Sasha.Lims.DataAccess.Entities.Entity;
using Entity.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace UI.Controllers {

    
    public class WorkflowRunListCardController : GenericCardController<vWorkflowRunList>
    {
        public WorkflowRunListCardController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor) { }

    }
}