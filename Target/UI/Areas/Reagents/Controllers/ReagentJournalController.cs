using Entity;
using Sasha.Lims.DataAccess.Entities.Entity;
using Entity.Controllers;
using Microsoft.AspNetCore.Mvc;
namespace UI.Areas.Reagents.Controllers{
    [Area("Reagents")]
    public class ReagentJournalController : GenericJournalController<vReagent>
    {
        public ReagentJournalController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor) { }

        protected override IQueryable<vReagent> FilterAction(IQueryable<vReagent> answers) {
            var dateFrom = HttpContext.Request.Query["from"];
            var dateTo = HttpContext.Request.Query["to"];           
            if (!string.IsNullOrEmpty(dateFrom) && DateTime.TryParse(dateFrom, out DateTime dateFr))
            {   
                answers = answers.Where(x => x.ExpirationDate > dateFr);
            }
            if (!string.IsNullOrEmpty(dateTo) && DateTime.TryParse(dateTo, out DateTime dateT))
            {
                answers = answers.Where(x => x.ExpirationDate < dateT);
            }
            return answers;
        }
    }
}