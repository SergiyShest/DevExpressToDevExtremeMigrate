using Entity;
using Entity.Controllers;
using Microsoft.AspNetCore.Mvc;
namespace UI.Areas.Reagents.Controllers{
    [Area("Reagents")]
    public class ReagentTypeJournalController : GenericJournalController<vAnswer2>
    {
        public ReagentTypeJournalController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor) { }

        protected override IQueryable<vAnswer2> FilterAction(IQueryable<vAnswer2> answers) {
            var dateFrom = base.HttpContext.Session.GetString("dateFrom");
            var dateTo = base.HttpContext.Session.GetString("dateTo");
            if (!string.IsNullOrEmpty(dateFrom) && DateTime.TryParse(dateFrom, out DateTime dateFr))
            {   
                answers = answers.Where(x => x. > dateFr);
            }
            if (!string.IsNullOrEmpty(dateTo) && DateTime.TryParse(dateTo, out DateTime dateT))
            {
                answers = answers.Where(x => x. < dateT);
            }
            return answers;
        }
    }
}