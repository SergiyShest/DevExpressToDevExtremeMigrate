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
    public class HospitalJournalController : UniversalController<vHospital,c_client>
	{
		public HospitalJournalController() : base(TableType.Hospital)
		{

		}

	}
}