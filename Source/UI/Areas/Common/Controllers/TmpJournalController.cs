using Sasha.Lims.Core.DTO.Laboratory;
using Sasha.Lims.Core.Helpers;
using Sasha.Lims.Core.Interfaces.Customization;
using Sasha.Lims.DataAccess.Entities.Constants;
using Sasha.Lims.DataAccess.Entities.Entity;
using Sasha.Lims.DataAccess.Repositories;
using Sasha.Lims.WebUI.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sasha.Lims.WebUI.Areas.Common.Controllers
{
	public class PatientJournalController : UniversalController<vPatient, c_patient>
	{
		public PatientJournalController():base (TableType.Patient)
		{

		}

		public ActionResult SelectPatient(
			string patientFirstName,
			string patientLastName,
			string patientDateOfBirth,
			string patientEmail,
			string patientNo, bool? showSelectCancelButtons, string varName)
		{
			base.LayoutId = "SamplesBook.Select" + varName;
			var filterExpression = CreateExpeession("firstName", patientFirstName, string.Empty);
			filterExpression = CreateExpeession("lastName", patientLastName, filterExpression);
			filterExpression = CreateExpeession("email", patientEmail, filterExpression);
			filterExpression = CreateExpeession("no", patientNo, filterExpression);

			ViewData["filterExpr"] = filterExpression;

			ViewBag.Title = "Select Patient";
			ViewBag.DontShowMainMenu = true;
			ViewBag.ShowSelectCancelButtons = true;
			ViewBag.varName = varName ?? "PatientId";
			return Index();
		}



		//private static IQueryable<vPatient> vPatient(
		//	string patientFirstName,
		//	string patientLastName,
		//	string patientDateOfBirth,
		//	string patientEmail,
		//	string patientNo)
		//{
		//	IUnitOfWorkEx uow = new UnitOfWork();
		//	var patients = uow.GetRepository<vPatient>().GetAll().Where(x =>
		//	(patientFirstName==null || x.firstName.Contains(patientFirstName)) &&
		//          (patientLastName==null || x.lastName.Contains(patientLastName)) &&
		//	(patientEmail == null || x.email.Contains(patientEmail)) &&
		//          (patientNo == null || x.no.Contains(patientNo))
		//	);

		//	return patients;
		//}
	}
}