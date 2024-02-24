using System.Web.Mvc;
using DevExpress.Web.Mvc;
using Sasha.Lims.Core.BusinessObjects;
using FileManagerSettings = DevExpress.Web.Mvc.FileManagerSettings;

namespace Sasha.Lims.WebUI.Controllers
{

	[Authorize]
	public class FileManagerController : Controller
	{
		public ActionResult FileManager(int tableId, int rowId)
		{
			ViewBag.Table = tableId;
			ViewBag.Id = rowId;
			return PartialView("FileManager");
		}

		FilesManager _objectFilesModel = new FilesManager();

		[ValidateInput(false)]
		public ActionResult FileManagerPartial(int tableId, int rowId)
		{
			ViewBag.tableId = tableId;
			ViewBag.rowId = rowId;
			_objectFilesModel.Set(tableId, rowId);
			var result= PartialView("_FileManagerPartial", _objectFilesModel);
           _objectFilesModel.RefreshDataBase();
			return result;
		}

		public FileStreamResult FileManagerPartialDownload(int tableId, int rowId)
		{
			_objectFilesModel.Set(tableId, rowId);
			var settings = new FileManagerSettings();
			settings.SettingsEditing.AllowDownload = true;
			settings.Name = "FileManager";
			var result= FileManagerExtension.DownloadFiles(settings, _objectFilesModel.DirName);
           _objectFilesModel.RefreshDataBase();
			return result;
		}

	}
}

