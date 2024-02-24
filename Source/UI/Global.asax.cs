using System;
using System.ServiceModel.Dispatcher;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using DevExpress.Web.Mvc;
using DevExpress.Web.Rendering;
using NLog;
using Sasha.Lims.Core.Helpers;
using Sasha.Lims.Exceptions;
using Sasha.Lims.WebUI.App_Start;
using Sasha.Lims.WebUI.Controllers.BaseAndCore.ErrorHandler.Controllers;
using Sasha.Lims.WebUI.Infrastructure;

namespace Sasha.Lims.WebUI
{
	public class MvcApplication : HttpApplication
	{
		internal static IDependencyResolver DataProvidersDependencyResolver;

		protected void Application_Start()
		{
			DataHelper.Current.ResetUnitOfWork();
			AreaRegistration.RegisterAllAreas();

			GlobalConfiguration.Configure(WebApiConfig.Register);

			ReportConfig.RegisterReport();
			RouteConfig.RegisterRoutes(RouteTable.Routes);
			BundleConfig.RegisterBundles(BundleTable.Bundles);
			FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);

			var kernel = NinjectStandardKernelConfig.GetKernel();
#pragma warning disable CS0618 // Тип или член устарел
			DependencyResolver.SetResolver(DataProvidersDependencyResolver = new NinjectDependencyResolver(kernel));
#pragma warning restore CS0618 // Тип или член устарел

			ModelBinders.Binders.DefaultBinder = new DevExpressEditorsBinder();
		}

		protected void Application_PreRequestHandlerExecute(object sender, EventArgs e)
		{
			DevExpressHelper.Theme = DataHelper.Current.UiTheme;
		}

		void Application_Error(object sender, EventArgs e)
		{
			Exception ex = Server.GetLastError();
			string method = String.Empty;

			RouteData routeData = new RouteData();
			routeData.Values.Add("controller", "Error");
			if (ex?.InnerException is BadLicenseException)
			{
				method = "BadLicense";

			}
			else
			{
				HttpException httpException = ex as HttpException;
				if (httpException != null)
				{

					switch (httpException.GetHttpCode())
					{
						case 404:
							// page not found
							method = "NotFound";
							break;
						case 500:
							// server error

							//	 method = "General";
							break;
						default:

							//	 method = "General";
							break;
					}
				}
			}
			Server.ClearError();
			if (string.IsNullOrEmpty(method)) method = "General";

			routeData.Values.Add("action", method);

			Response.TrySkipIisCustomErrors = true;
			IController errorController = new ErrorController();
			var context = new RequestContext(new HttpContextWrapper(Context), routeData);
			context.RouteData.DataTokens.Add("exception", ex);

			errorController.Execute(context);
			Server.ClearError();

			Logger logger = NLog.LogManager.GetLogger("Global error handler");
			logger.Error(ex);
		}

	}
}