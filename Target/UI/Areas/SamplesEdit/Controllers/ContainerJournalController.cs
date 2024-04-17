using Entity;
using Sasha.Lims.DataAccess.Entities.Entity;
using Entity.Controllers;
using Microsoft.AspNetCore.Mvc;
namespace UI.Areas.SamplesEdit.Controllers{
    [Area("SamplesEdit")]
    public class ContainerJournalController : GenericJournalController<vContaiter>
    {
        public ContainerJournalController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor) { }

        protected override IQueryable<vContaiter> FilterAction(IQueryable<vContaiter> answers) {
            var dateFrom = HttpContext.Request.Query["from"];
            var dateTo = HttpContext.Request.Query["to"];           
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