using Entity;
using Sasha.Lims.DataAccess.Entities.Entity;
using Entity.Controllers;
using Microsoft.AspNetCore.Mvc;
namespace UI.Areas.Common.Controllers{
    [Area("Common")]
    public class TmpJournalController : GenericJournalController<vPatient>
    {
        public TmpJournalController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor) { }

        protected override IQueryable<vPatient> FilterAction(IQueryable<vPatient> answers) {
            var dateFrom = HttpContext.Request.Query["from"];
            var dateTo = HttpContext.Request.Query["to"];           
            if (!string.IsNullOrEmpty(dateFrom) && DateTime.TryParse(dateFrom, out DateTime dateFr))
            {   
                answers = answers.Where(x => x.dob > dateFr);
            }
            if (!string.IsNullOrEmpty(dateTo) && DateTime.TryParse(dateTo, out DateTime dateT))
            {
                answers = answers.Where(x => x.dob < dateT);
            }
            return answers;
        }
    }
}