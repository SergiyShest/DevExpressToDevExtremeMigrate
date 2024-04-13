using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Core;
using DevExtreme.AspNet.Data;
using System.Reflection;
using DAL.Core;
using BLL;


namespace Entity.Controllers
{
    public class GenericJournalController<T> : BaseController where T : class, IEntity
    {
        string? _name;
        public virtual string ClassName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_name))
                {
                    _name = GetType().Name.Replace("Controller",string.Empty);
                }
                return _name;

            }
            set { _name = value; }
        }

        #region BaseJournalService
        BaseJournalService<T> _baseJournalService;

        public BaseJournalService<T> BaseJournalService
        {
            get
            {
                if (_baseJournalService == null)
                {
                    _baseJournalService = new BaseJournalService<T>();
                }
                return _baseJournalService;
            }

            set { _baseJournalService = value; }
        }

        #endregion

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
                    setFilter(dateFromTo.from,"from");
                    setFilter(dateFromTo.to,"to");
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

        private void setFilter(string value,string paramName)
        {
            if (value == null || value == "null")
            {
                base.HttpContext.Session.Remove(ClassName + paramName);
            }
            else
            {
                base.HttpContext.Session.SetString(ClassName + paramName, value);
            }
        }

        public virtual async Task<IActionResult> Get(DataSourceLoadOptions loadOptions)
        {

            IQueryable<T> answers = await BaseJournalService.Get();
            answers = FilterAction(answers);
            loadOptions.PrimaryKey = new[] { "Id" };
            loadOptions.PaginateViaPrimaryKey = true;
            return Content(JsonConvert.SerializeObject(answers), "application/json");
        }

        protected virtual IQueryable<T> FilterAction(IQueryable<T> answers)
        {
            return answers;
        }
    }


}

