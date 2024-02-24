using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Core;
using DevExtreme.AspNet.Data;
using System.Reflection;
using DAL.Core;


namespace Entity.Controllers
{
    public class GenericJournalController<T> : BaseController where T : class, IEntity
	{
		public GenericJournalController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor) { }

		public virtual IActionResult Index()
		{
			return View();
		}

		public virtual IActionResult SetFilter()
		{
			try
			{
				var dateFromTo = JsonConvert.DeserializeObject<DateFromTo>(base.Body());
				if (dateFromTo != null)
				{
					if (dateFromTo.from == null || dateFromTo.from == "null")
					{
						base.HttpContext.Session.Remove("dateFrom");
					}
					else
					{
						base.HttpContext.Session.SetString("dateFrom", dateFromTo.from);
					}
					if (dateFromTo.to == null || dateFromTo.to == "null")
					{
						base.HttpContext.Session.Remove("dateTo");
					}
					else
					{
						base.HttpContext.Session.SetString("dateTo", dateFromTo.to);
					}
				}
				return Ok(new { Reload = true });
			}
			catch (Exception ex)
			{
				return BadRequest(new
				{
					error = ex.GetBaseException()
				});
			}
		}

		public virtual async Task<IActionResult> Get(DataSourceLoadOptions loadOptions)
		{
			IQueryable<T> answers = TestDataHelper.GetFilledEntities<T>(16);
            answers = FilterAction(answers);
			loadOptions.PrimaryKey = new[] { "Id" };
			loadOptions.PaginateViaPrimaryKey = true;
			//var result = await DataSourceLoader.LoadAsync(answers, loadOptions, cancellationToken: HttpContext.RequestAborted);
			return Content(JsonConvert.SerializeObject(answers), "application/json");
		}

		protected virtual IQueryable<T> FilterAction(IQueryable<T> answers)
		{
			return answers;
		}
    }
}

