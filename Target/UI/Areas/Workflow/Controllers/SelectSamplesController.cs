using Entity;
using Sasha.Lims.DataAccess.Entities.Entity;
using Entity.Controllers;
using Microsoft.AspNetCore.Mvc;
namespace UI.Areas.Workflow.Controllers{
    [Area("Workflow")]
    public class SelectSamplesController : GenericJournalController<vSamplesBook>
    {
        public SelectSamplesController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor) { }

        protected override IQueryable<vSamplesBook> FilterAction(IQueryable<vSamplesBook> answers) {
            var dateFrom = HttpContext.Request.Query["from"];
            var dateTo = HttpContext.Request.Query["to"];           
            if (!string.IsNullOrEmpty(dateFrom) && DateTime.TryParse(dateFrom, out DateTime dateFr))
            {   
                answers = answers.Where(x => x.DateCreate > dateFr);
            }
            if (!string.IsNullOrEmpty(dateTo) && DateTime.TryParse(dateTo, out DateTime dateT))
            {
                answers = answers.Where(x => x.DateCreate < dateT);
            }
            return answers;
        }
    }
}