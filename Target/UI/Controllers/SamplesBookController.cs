using Entity;
using Sasha.Lims.DataAccess.Entities.Entity;
using Entity.Controllers;
using Microsoft.AspNetCore.Mvc;
namespace UI.Controllers{
    
    public class SamplesBookController : GenericJournalController<vSamplesBook>
    {
        public SamplesBookController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor) { }

        protected override IQueryable<vSamplesBook> FilterAction(IQueryable<vSamplesBook> answers) {
            var dateFrom = base.HttpContext.Session.GetString("dateFrom");
            var dateTo = base.HttpContext.Session.GetString("dateTo");
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