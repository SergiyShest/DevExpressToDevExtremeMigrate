using Entity;
using Sasha.Lims.DataAccess.Entities.Entity;
using Entity.Controllers;
using Microsoft.AspNetCore.Mvc;
namespace UI.Areas.Common.Controllers{
    [Area("Common")]
    public class HospitalJournalController : GenericJournalController<vHospital>
    {
        public HospitalJournalController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor) { }

        protected override IQueryable<vHospital> FilterAction(IQueryable<vHospital> answers) {
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