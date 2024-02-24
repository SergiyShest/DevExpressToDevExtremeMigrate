using Sasha.Lims.Core.Helpers;
using Sasha.Lims.DataAccess.Entities.Constants;
using Sasha.Lims.DataAccess.Entities.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Sasha.Lims.WebUI.Controllers
{
	public abstract class UniversalController<T,S> : BaseController where T : class ,IEntity where S : class ,IEntity
	{
		protected int _typeId;
		protected TableType? _type;

		string _name;
		protected string TypeName
		{
			get
			{
				if (_name == null)
				{
					if (_type == null)
					{
						var typeDescr = DataHelper.Current.GetTypeDescriptions().FirstOrDefault(x => x.Id == _typeId);
						if(typeDescr != null)
						{_name = typeDescr.Name; } else
						{
							_name = "Unknown type " + _typeId;
						}

					} else
					{
						_name = _type.ToString();
					}
				}
				return _name;
			}
			set { _name = value; }
		}

		#region Constructors
		protected UniversalController(int typeId)
		{
			_typeId = typeId;
		}
		protected UniversalController(TableType type) : this((int) type)
		{
			_type = type;
		} 
		#endregion

		public virtual ActionResult Index()
		{
			ViewBag.TypeId = _typeId;
			ViewBag.TypeName = TypeName;
			return View("Index", null);

		}

		public virtual ActionResult Select(string varName)
		{
			base.LayoutId = TypeName+".Select" + varName;
			ViewBag.Title = "Select "+TypeName;
			ViewBag.DontShowMainMenu = true;
			ViewBag.ShowSelectCancelButtons = true;
			ViewBag.varName = varName ?? "DoctorId";
			return Index();
		}

		protected virtual  IQueryable<T> getModel()
		{
			return UOW.GetRepository<T>().GetAll();
		}

		public virtual ActionResult CallbackRouteValues(string selectedIds,	string customAction,string filterExpression)
		{

			ViewData["filterExpression"] = filterExpression;
			Action(selectedIds, customAction);

			return PartialView("_GridViewPartial", getModel());
		}

		protected virtual void Action(string selectedIds, string customAction)
		{
			if (customAction == "delete")
			{
				SafeExecute(() =>
				{
					if (!string.IsNullOrEmpty(selectedIds))
					{
						var ids = CoreHelper.ConvertIdsToArr(selectedIds);
						var patRep = UOW.GetRepository<S>();
						foreach (var id in ids)
						{
							patRep.Delete(id);
						}
					}
				});
			}
		}
	}

}