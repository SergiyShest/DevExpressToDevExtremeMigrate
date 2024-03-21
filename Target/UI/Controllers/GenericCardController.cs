using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Core;
using DAL.Core;
using System.Dynamic;

namespace Entity.Controllers
{
    public class GenericCardController<T> : BaseController where T : class, IEntity
	{
		public GenericCardController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor) { }

		public virtual IActionResult Index(int?id)
		{
			ViewBag.Id = id;
			ViewBag.Mode = "edit";
			return View();
		}

		public virtual async Task<IActionResult> Get(int? id,string? mode)
		{ 
			T answer ;
			if (id == null) answer = Activator.CreateInstance<T>();
			else { 
			answer  = TestDataHelper.GetFilledEntities<T>(16).First(x=>x.Id==id);
            }
			return Content(JsonConvert.SerializeObject(answer ), "application/json");
		}

        public virtual async Task<IActionResult> Delete(int? id)
		{
			var answer = new { message = "Необходимо реализовать метод Delete!" };
			return Content(JsonConvert.SerializeObject(answer ), "application/json");
		}
        public virtual async Task<IActionResult> Save(int? id)
		{
			var answer = new { message = "Необходимо реализовать метод Save!" };
			return Content(JsonConvert.SerializeObject(answer ), "application/json");
		}
    }

}

