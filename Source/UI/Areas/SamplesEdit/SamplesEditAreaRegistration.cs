using System.Web.Mvc;

namespace Sasha.Lims.WebUI.Areas.SamplesEdit
{
    public class SamplesEditAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "SamplesEdit";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "SamplesEdit_default",
                "SamplesEdit/{controller}/{action}/{id}",
                new {controller= "EditSample",  action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}