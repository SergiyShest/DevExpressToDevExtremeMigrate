using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using DevExpress.XtraReports.UI;
using DevExpress.XtraReports.Web;
using DevExpress.XtraReports.Web.Extensions;
using Newtonsoft.Json;
using Sasha.Lims.Core.Interfaces.Common;
using Sasha.Lims.Core.Interfaces.Laboratory;
using Sasha.Lims.Core.Services.Common.Logic;
using Sasha.Lims.Core.Services.Laboratory.Logics;
using Sasha.Lims.WebUI.Infrastructure.Helpers;
using Sasha.Lims.WebUI.Models;
using Sasha.Lims.WebUI.Reports.BatchSchema;

namespace Sasha.Lims.WebUI.Controllers
{
	public class PrintController : BaseController
	{
		private ReportStorageWebExtension _reportStorage;
		private readonly ISamplesApprovedService _samplesApprovedService;

		private readonly IPrintService _printService;

		private const string BookUrl = "Book_Label_Dymo_30252";
		private const string JournalUrl = "Journal_Label_Dymo_30252";
		private const string CustodyUrl = "Custody_Log";
		private const string SettingsExportUrl = "Settings_Export";

		public PrintController(ReportStorageWebExtension reportStorage,
								ISamplesApprovedService samplesApprovedService,
								IPrintService printService)
		{
			_reportStorage = reportStorage;
			_samplesApprovedService = samplesApprovedService;

			_printService = printService;
		}

		public ActionResult Book(string ids, bool? dontShowMainMenu)
		{
			ViewBag.DontShowMainMenu = dontShowMainMenu;
			var report = GetReport(BookUrl);
			var parameters = report.Report.Parameters["Ids"];
			if (!string.IsNullOrEmpty(ids) && parameters != null)
			{
				var idArray = ids.Split(',').Select(x => Convert.ToInt32(x)).ToArray();
				parameters.Value = idArray;
				parameters.Visible = false;
			}
			return View(report);
		}

		public ActionResult Journal(string ids)
		{
			var report = GetReport(JournalUrl);
			var parameters = report.Report.Parameters["Ids"];
			if (!string.IsNullOrEmpty(ids) && parameters != null)
			{
				var idArray = ids.Split(',').Select(x => Convert.ToInt32(x)).ToArray();
				parameters.Value = idArray;
				parameters.Visible = false;
			}

			return View(report);
		}

		public ActionResult Custody(int id)
		{
			var report = GetReport(CustodyUrl);

			var parameter = report.Report.Parameters["ID"];
			parameter.Value = id;
			parameter.Visible = false;
			return View(report);
		}

		public ActionResult Batch(int id, bool dontShowMainMenu = false)
		{
			ViewBag.DontShowMainMenu = dontShowMainMenu;
			var dimensionDescription = _samplesApprovedService.GetBatchNumberingScheme(id);
			var batch = _samplesApprovedService.Get(id);

			if (string.IsNullOrEmpty(dimensionDescription)) // todo: log error
			{
				return View("Error", new List<string> { "A report for this batch cannot be built." }); // todo: localize
			}

			WellDimension[] dimensions;

			try
			{
				dimensions = dimensionDescription.Split(',').Select(d => new WellDimension(d)).ToArray();
			}
			catch (InvalidOperationException) // todo: log error
			{
				return View("Error", new List<string> { "A report for this batch cannot be built." }); // todo: localize
			}

			var samplesInBatch = Mapper.Map<IEnumerable<BatchWell>>(_samplesApprovedService.GetPlatesItems(id));
			var reportModel = new BatchReportViewModel()
			{
				BatchName = batch.Name,
				BatchBarcode = batch.Barcode,
				Model = batch.ModelId.ToString(),
				BatchDimensions = dimensions,
				Items = samplesInBatch,
			};
			var report = new BatchSchemaReport(reportModel);
			var parameter = report.Parameters["BatchId"];
			parameter.Value = id;
			parameter.Visible = false;

			return View(report);
		}

		public ActionResult CustomReport(int? id, int? reportId, bool dontShowMainMenu = false)
		{
			ViewBag.DontShowMainMenu = dontShowMainMenu;
			if (id == null || reportId == null)
			{
				return HttpNotFound("File not found");
			}
			var reportProperties = _printService.GetReportProperty((int) reportId);

			if (reportProperties.ReportSource == CustomReportSource.Report || reportProperties.ReportSource == CustomReportSource.Subreport)
			{
				ViewBag.TitleLocalizationKey = reportProperties.LocalizationNameKey;
				var report = GetReport(reportProperties.ReportFileName);

				foreach (var parameter in report.Report.Parameters)
				{
					if (parameter.Name.Equals("ID", StringComparison.OrdinalIgnoreCase))
					{
						parameter.Value = id;
						parameter.Visible = false;
					} else if (parameter.Name.Equals("IDS", StringComparison.OrdinalIgnoreCase))
					{
						parameter.Value = new[] { id };
						parameter.Visible = false;
					} else if (parameter.Name.Equals("ReportName", StringComparison.OrdinalIgnoreCase))
					{
						parameter.Value = reportProperties.ReportFileName;
						parameter.Visible = false;
					} else if (parameter.Name.Equals("LocalServer", StringComparison.OrdinalIgnoreCase))
					{
						var url = Request.Url;
						var apiLink = Url.Action("Index", "Home");
						parameter.Value = $"{url.Scheme}://{url.Host}:{url.Port}{apiLink}";
						parameter.Visible = false;
					}
				}

				int? patientId = null;
				bool isTCR = false;
				if (reportProperties.TableName == "wf_result")
				{
					patientId = GetPatientId(id);
					isTCR = reportProperties.ReportFileName == "TCR_MAIN";
				}
				ViewBag.ReportExtraData = JsonConvert.SerializeObject(
					new
					{
						tableId = GetTableId(reportProperties.TableName),
						rowId = id,
						patientId = patientId,
						isTCR = isTCR
					});
				return View(report);
			}

			if (reportProperties.ReportSource == CustomReportSource.File)
			{
				return OpenReportFile((int) id, reportProperties);
			}

			return HttpNotFound("File not found");
		}

		private int? GetTableId(string tableName)
		{
			if (string.IsNullOrEmpty(tableName)) return null;
			return _printService.GetTableIdByName(tableName);
		}

		//private int? GetSampleId(int? resultId)
		//{
		//	if (resultId == null) return null;
		//	return _printService.WfResultGetSampleId((int) resultId);
		//}

		private int? GetPatientId(int? resultId)
		{
			if (resultId == null) return null;
			return _printService.WfResultGetPatientId((int) resultId);
		}


		public ActionResult CustomReportWithIds(string ids, int? reportId, bool dontShowMainMenu = false)
		{
			ViewBag.DontShowMainMenu = dontShowMainMenu;
			if (string.IsNullOrEmpty(ids) || reportId == null)
			{
				return HttpNotFound("File not found");
			}

			var idArray = ids.Split(',').Select(x => Convert.ToInt32(x)).ToArray();
			var reportProperties = _printService.GetReportProperty((int) reportId);

			if (reportProperties.ReportSource == CustomReportSource.Report || reportProperties.ReportSource == CustomReportSource.Subreport)
			{
				ViewBag.TitleLocalizationKey = reportProperties.LocalizationNameKey;
				var report = GetReport(reportProperties.ReportFileName);

				foreach (var parameter in report.Report.Parameters)
				{
					if (parameter.Name.Equals("IDS", StringComparison.OrdinalIgnoreCase))
					{
						parameter.Value = idArray;
						parameter.Visible = false;
					} else if (parameter.Name.Equals("ReportName", StringComparison.OrdinalIgnoreCase))
					{
						parameter.Value = reportProperties.ReportFileName;
						parameter.Visible = false;
					} else if (parameter.Name.Equals("LocalServer", StringComparison.OrdinalIgnoreCase))
					{
						var url = Request.Url;
						var apiLink = Url.Action("Index", "Home");
						parameter.Value = $"{url.Scheme}://{url.Host}:{url.Port}{apiLink}";
						parameter.Visible = false;
					}
				}
				ViewBag.ReportExtraData = JsonConvert.SerializeObject(
					new
					{
						isTCR = false
					});

				return View("CustomReport", report);
			}
			return HttpNotFound("File not found");
		}


		public ActionResult ExportSettings(IEnumerable<int> keys)
		{
			var ids = _printService.GetAllChildren(keys);
			var report = GetReport(SettingsExportUrl);

			if (ids?.Count() > 0)
			{
				var parameters = report.Report.Parameters["Ids"];
				if (parameters != null)
				{
					parameters.Value = ids;
					parameters.Visible = false;
				}
			}

			return View(report);
		}

		private ActionResult OpenReportFile(int id, ReportProperty reportProperties)
		{
			// only wf_result supported
			if (reportProperties.TableName != "wf_result")
				return HttpNotFound("File not found");

			(string attachedText, string sampleName) = _printService.GetReportPathParts(id);

			if (string.IsNullOrEmpty(attachedText) || string.IsNullOrEmpty(sampleName))
				return HttpNotFound("File not found");
			(string inDir, var outDir) = _printService.GetWorkflowSubdirectory(id);
			string subfolder = reportProperties.ReportSource == CustomReportSource.File ? inDir : outDir;
			string fileName = CustomReportListHelper.GetReportPath(attachedText, sampleName, reportProperties.FilePattern, subfolder);

			if (fileName == null)
				return HttpNotFound("File not found");

			return File(
				fileName,
				System.Web.MimeMapping.GetMimeMapping(fileName),
				Path.GetFileName(fileName));
		}

		private CachedReportSourceWeb GetReport(string url)
		{
			XtraReport report;

			if (string.IsNullOrEmpty(url) || !_reportStorage.IsValidUrl(url))
			{
				report = new XtraReport();
			} else
			{
				using (var ms = new MemoryStream(_reportStorage.GetData(url)))
				{
					report = XtraReport.FromStream(ms);
				}
				report.Tag = url;
				ViewBag.ReportTag = url;
			}
			return new CachedReportSourceWeb(report);
		}
	}
}