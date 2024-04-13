using Entity;
using Sasha.Lims.DataAccess.Entities.Entity;
using Entity.Controllers;
using Microsoft.AspNetCore.Mvc;
namespace UI.Areas.Reagents.Controllers{
    [Area("Reagents")]
    public class ReagentLogJournalController : GenericJournalController<vReagentLog>
    {
        public ReagentLogJournalController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor) { }

        protected override IQueryable<vReagentLog> FilterAction(IQueryable<vReagentLog> answers) {
            var dateFrom = base.HttpContext.Session.GetString(ClassName + "from");
            var dateTo = base.HttpContext.Session.GetString(ClassName + "to");
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