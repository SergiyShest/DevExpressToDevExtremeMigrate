using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using DevExpress.Web;

namespace Sasha.Lims.WebUI.Controllers
{
    public class FilesController : Controller
    {
        // GET: Files
        public ActionResult Index()
        {
            //return View();
			return null;
		}

		public static void ucMultiSelection_FileUploadComplete(object sender, FileUploadCompleteEventArgs e)
		{
			string resultFileName = Path.GetRandomFileName() + "_" + e.UploadedFile.FileName;
			string resultFileUrl = "~/Content/UploadControl/UploadFolder/" + resultFileName;
			string resultFilePath = System.Web.HttpContext.Current.Request.MapPath(resultFileUrl);
			e.UploadedFile.SaveAs(resultFilePath);

			//UploadingUtils.RemoveFileWithDelay(resultFileName, resultFilePath, 5);

			IUrlResolutionService urlResolver = sender as IUrlResolutionService;
			if (urlResolver != null)
			{
				string url = urlResolver.ResolveClientUrl(resultFileUrl);
				e.CallbackData = GetCallbackData(e.UploadedFile, url);
			}
		}

		static string GetCallbackData(UploadedFile uploadedFile, string fileUrl)
		{
			string name = uploadedFile.FileName;
			long sizeInKilobytes = uploadedFile.ContentLength / 1024;
			string sizeText = sizeInKilobytes.ToString() + " KB";

			return name + "|" + fileUrl + "|" + sizeText;
		}
	}
}