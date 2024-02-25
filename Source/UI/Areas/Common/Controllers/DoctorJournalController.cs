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
	public class DoctorJournalController : UniversalController<vDoctor, c_doctor>
	{
		public DoctorJournalController() : base(TableType.Doctor)
		{

		}

	}
}