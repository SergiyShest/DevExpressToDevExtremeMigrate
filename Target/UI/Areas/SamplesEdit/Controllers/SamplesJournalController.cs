using Entity;
using Sasha.Lims.DataAccess.Entities.Entity;
using Entity.Controllers;
using Microsoft.AspNetCore.Mvc;
namespace UI.Areas.SamplesEdit.Controllers{
    [Area("SamplesEdit")]
    public class SamplesJournalController : GenericJournalController<vJourLine>
    {
        public SamplesJournalController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor) { }

        protected override IQueryable<vJourLine> FilterAction(IQueryable<vJourLine> answers) {
            var dateFrom = base.HttpContext.Session.GetString(ClassName + "from");
            var dateTo = base.HttpContext.Session.GetString(ClassName + "to");
            if (!string.IsNullOrEmpty(dateFrom) && DateTime.TryParse(dateFrom, out DateTime dateFr))
            {   
                answers = answers.Where(x => x.CollectedDate > dateFr);
            }
            if (!string.IsNullOrEmpty(dateTo) && DateTime.TryParse(dateTo, out DateTime dateT))
            {
                answers = answers.Where(x => x.CollectedDate < dateT);
            }
            return answers;
        }
    }
}