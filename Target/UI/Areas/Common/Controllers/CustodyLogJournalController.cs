using Entity;
using Sasha.Lims.DataAccess.Entities.Entity;
using Entity.Controllers;
using Microsoft.AspNetCore.Mvc;
namespace UI.Areas.Common.Controllers{
    [Area("Common")]
    public class CustodyLogJournalController : GenericJournalController<vCustodyLog>
    {
        public CustodyLogJournalController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor) { }

        protected override IQueryable<vCustodyLog> FilterAction(IQueryable<vCustodyLog> answers) {
            var dateFrom = base.HttpContext.Session.GetString(ClassName + "from");
            var dateTo = base.HttpContext.Session.GetString(ClassName + "to");
            if (!string.IsNullOrEmpty(dateFrom) && DateTime.TryParse(dateFrom, out DateTime dateFr))
            {   
                answers = answers.Where(x => x.DateTime > dateFr);
            }
            if (!string.IsNullOrEmpty(dateTo) && DateTime.TryParse(dateTo, out DateTime dateT))
            {
                answers = answers.Where(x => x.DateTime < dateT);
            }
            return answers;
        }
    }
}