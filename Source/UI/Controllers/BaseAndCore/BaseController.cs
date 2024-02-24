using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using DevExpress.DataProcessing;
using DevExpress.Web;
using Microsoft.AspNet.Identity.Owin;
using Sasha.Lims.Core.Consts;
using Sasha.Lims.Core.DTO.Access;
using Sasha.Lims.Core.DTO.Laboratory;
using Sasha.Lims.Core.Helpers;
using Sasha.Lims.Core.Interfaces.Access;
using Sasha.Lims.Core.Interfaces.Laboratory;
using Sasha.Lims.Core.Services.Common;
using Sasha.Lims.WebUI.Infrastructure.Helpers;
using Sasha.Lims.Core.Settings;
using Sasha.Lims.DataAccess.Repositories;
using Sasha.Lims.WebUI.Models;
using NLog;
using System.Dynamic;
using Newtonsoft.Json;
using Sasha.Lims.Core.BusinessObjects;

namespace Sasha.Lims.WebUI.Controllers
{
	public abstract class BaseController : Controller, IBaseController
	{
		protected Logger log=  LogManager.GetCurrentClassLogger();



		private IUnitOfWorkEx _unitOfWork;
		public IUnitOfWorkEx UOW
		{
			get
			{
				if (_unitOfWork == null)
				{
					_unitOfWork = new UnitOfWork();
				}
				return new UnitOfWork();
			}

		}

		private bool _isSessionInitialized { get; set; }

		protected BaseController()
		{
			ViewBag.getUser = new Func<UserDTO>(GetCurrentUser);
		}
		#region Locations

		protected ILocationsService LocationsService;
		public IEnumerable<LocationViewModel> GetLocations(int? locationId = -1)
		{
			IEnumerable<LocationViewModel> result = null;
			var tmpLocationId = locationId ?? -1;

			if (null == LocationsService)
				LocationsService = DependencyResolver.Current.GetService<ILocationsService>();

			result = LocationsService.GetLocations(tmpLocationId)?.Select(p => new LocationViewModel() { Id = p.LocationId, ShortPath = p.ShortPath });

			return result;
		}

		public IEnumerable<LocationViewModel> GetSortLocations(int? shortLocationId = -1)
		{
			IEnumerable<LocationViewModel> result = null;
			var tmpLocationId = shortLocationId ?? -1;

			if (null == LocationsService)
				LocationsService = DependencyResolver.Current.GetService<ILocationsService>();


			result = LocationsService.GetShortLocations(tmpLocationId)?.Select(buildLocationModel);

			return result;
		}

		static LocationViewModel buildLocationModel(LocationDTO p) => new LocationViewModel() { ShortLocationId = p.ShortLocationId, ShortPath = p.ShortPath, Id = p.LocationId };

		public IEnumerable<LocationViewModel> GetLocation(ListEditItemsRequestedByFilterConditionEventArgs args) => GetSortLocations();

		public LocationViewModel GetLocation(ListEditItemRequestedByValueEventArgs args)
		{
			if (args.Value == null || !int.TryParse(args.Value.ToString(), out int id))
				return null;

			if (null == LocationsService)
				LocationsService = DependencyResolver.Current.GetService<ILocationsService>();

			return buildLocationModel(LocationsService.Get(id));
		}

		#endregion
		#region User
		private IUserService _userServiceVar { get; set; }

		private IUserService _userService => _userServiceVar
											?? (_userServiceVar = HttpContext.GetOwinContext()
												.GetUserManager<IUserService>());
		//todo old method need use GetSystemUsers, GetAspNetUsers to remove
		public IEnumerable<UserViewModel> GetAspNetUsers(string userGuid = "")
		{
			UserViewModel makeUser(UserDTO item) => new UserViewModel
			{
				Id = item.Id,
				DisplayText = ((item.FirstName ?? string.Empty) + " " + (item.LastName ?? string.Empty)).Trim()
			};

			var UserService = DependencyResolver.Current.GetService<IUserServiceCreator>().CreateUserService(SettingsConsts.IdentityDbConfigSection);

			IEnumerable<UserViewModel> result = string.IsNullOrWhiteSpace(userGuid)
				? UserService.Users.OrderBy(i => i.FirstName + i.LastName).Select(makeUser)
				: UserService.Users.Where(u => u.Id == userGuid).Select(makeUser);
			return result;
		}

		public IEnumerable<UserViewModel> GetSystemUsers(int? userId = 0)
		{
			UserViewModel makeUser(UserDTO item) => new UserViewModel
			{
				Id = item.Id,
				UserId = item.UserId ?? 0,
				DisplayText = ((item.FirstName ?? string.Empty) + " " + (item.LastName ?? string.Empty)).Trim()
			};

			var UserService = DependencyResolver.Current.GetService<IUserServiceCreator>().CreateUserService(SettingsConsts.IdentityDbConfigSection);

			userId = userId ?? 0;
			IEnumerable<UserViewModel> result = userId < 1
				? UserService.Users.OrderBy(i => i.FirstName + i.LastName).Select(makeUser)
				: UserService.Users.Where(u => u.UserId == userId).Select(makeUser);
			return result;
		}

		private UserDTO _currentUser;

		protected virtual UserDTO GetCurrentUser()
		{
			if (_currentUser == null)
			{
				//todo:save data to session variable
				_currentUser=  _userService.FindByName(User?.Identity?.Name);

				if(_currentUser==null)
				{
					_currentUser = new UserDTO() { FirstName = "Anonim" };
				}
			}
			return _currentUser;
		}

		#endregion
		#region Execute

		protected void SafeExecute(Action method)
		{
			try
			{
				method();
			}
			catch (Exception e)
			{
				SetErrorText(e);
			}
		}

		protected object SafeExecute(Func<object> method)
		{
			try
			{
				return method();
			}
			catch (Exception e)
			{
				SetErrorText(e);
			}

			return null;
		}


	   protected	JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings();

		public string GetJsonSafe(Func<object> function)
		{
			dynamic expOb = new ExpandoObject();
			try
			{
				expOb.Item = function();
			}
			catch (Exception ex)
			{
				expOb.Errors = new List<ObjectError>() { new ObjectError("", ex.GetAllMessages()) };
				log.Error(ex);
			}
			var	json = JsonConvert.SerializeObject(expOb, JsonSerializerSettings);
			return json;
		}

		public string GetJsonSafe(Action function)
		{
			dynamic expOb = new ExpandoObject();
			try
			{
				function();

			}
			catch (Exception ex)
			{
				expOb.Errors = new List<ObjectError>() { new ObjectError("", ex.GetAllMessages()) };
				log.Error(ex);

			}
			var	json = JsonConvert.SerializeObject(expOb, JsonSerializerSettings);
			return json;
		}

		protected virtual void SetErrorText(Exception ex)
		{
			ViewBag.GeneralError = ex.Message;
		}

		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			if (!filterContext.IsChildAction)
			{
				InitContextData(filterContext.HttpContext);
			}

			base.OnActionExecuting(filterContext);
		}

		#endregion
		#region init contex

		protected void InitContextData(HttpContextBase httpContext, bool renewData = false)
		{
			if (httpContext?.Session != null && (!_isSessionInitialized || renewData))
			{
				//IClientDataProvider clientDataProvider =
				//		(IClientDataProvider) DependencyResolver.Current.GetService(typeof(IClientDataProvider));

				////init is admin group
				//if (ContextDataHelper.CurrentClient != null)
				//{
				//	ContextDataHelper.CurrentClient.IsAdminInSubClientsGroup = clientDataProvider.IsClientAdminInGroup(ContextDataHelper.CurrentClient.Id);
				//}

				var langLocStr = WebContextHelper.GetCookie(httpContext, "Lang");

				if (string.IsNullOrWhiteSpace(langLocStr))
				{
					var request = httpContext.Request;

					if (request.UserLanguages?.Length > 0)
					{
						langLocStr = Request.UserLanguages[0];
					}
				}

				SetCultureLanguage(httpContext, langLocStr);
				_isSessionInitialized = true;
			}
		}

		#endregion
		#region set culture

		private void SetCultureLanguage(HttpContextBase httpContext, string langForUse)
		{
			//Set language culture context
			if (ConfigVariables.AvailableLanguages?.Count > 0)
			{
				var langForSet =
					ConfigVariables.AvailableLanguages.FirstOrDefault(x => x.Equals(langForUse));

				if (langForSet != null)
				{
					try
					{
						var culture = new CultureInfo(langForSet);
						Thread.CurrentThread.CurrentCulture = culture;
						Thread.CurrentThread.CurrentUICulture = culture;
					}
					catch
					{
						;
					}
				}
			}

			ViewBag.CurrentCulture = Thread.CurrentThread.CurrentCulture;
			Response.AppendCookie(new HttpCookie("Lang", Thread.CurrentThread.CurrentCulture.Name) { Expires = DateTime.Now.AddYears(1) });
		}

		#endregion
		#region ClientLayout

		ClientLayoutService _clientLayoutService = new ClientLayoutService();

		public string LayoutId { get; set; }

		bool dontSaveLayout = false;
		internal void DontSaveLayaut()
		{
			dontSaveLayout = true;
		}

		public void ClientLayout(object sender, ASPxClientLayoutArgs asPxClientLayoutArgs)
		{
			if(LayoutId == null||dontSaveLayout)return;
			//todo сделать сохранение настроек с использованием не id а имени пользователя
			var layoutData = Session[LayoutId]?.ToString();
		
			switch (asPxClientLayoutArgs.LayoutMode)
			{
				case ClientLayoutMode.Loading:
					if (!string.IsNullOrWhiteSpace(layoutData))
					{
						asPxClientLayoutArgs.LayoutData = layoutData;
					}
					else
					{
						asPxClientLayoutArgs.LayoutData = _clientLayoutService.Load(LayoutId, (int) GetCurrentUser().UserId);
					}
					break;
				    case ClientLayoutMode.Saving:

					Session[LayoutId] = asPxClientLayoutArgs.LayoutData;//настройки сохраняются в сессию и в базу параллельно
					_clientLayoutService.SaveAsinc(LayoutId, asPxClientLayoutArgs.LayoutData, (int) GetCurrentUser().UserId);//в базу в другом потоке.
					break;
			}
		}

		#endregion

		protected static string CreateExpeession(string fieldName, object fieldValue, string filterExpesson, ExpressionType exprType=ExpressionType.StringContains,string expr="and")
		{
			if (fieldValue==null || fieldValue.ToString() == "null"||fieldValue.ToString()==String.Empty) return filterExpesson;

			if (!string.IsNullOrEmpty(filterExpesson)) filterExpesson += " "+expr+" ";

			switch (exprType)
			{
				case ExpressionType.StringContains:filterExpesson += $"Contains([{fieldName}],'{fieldValue}')"; break;

				case ExpressionType.NumberEquals:filterExpesson += $" [{fieldName}] = {fieldValue} ";break;

				case ExpressionType.NumberNotEquals: filterExpesson += $" [{fieldName}] != {fieldValue} ";break;

				case ExpressionType.DateEquals: filterExpesson += $"[{fieldName}] ='{fieldValue}'";break;
			}

			return filterExpesson;
		}

		protected static void NullTest(object type, string name)
		{
			if (type == null) { throw new ArgumentNullException($"Parametr {name} can not be null"); }
		}



	}

	public  enum ExpressionType
	{
		StringContains,
		DateEquals,
		NumberEquals,
        NumberNotEquals,

	}
}