using System;
using System.Collections.Generic;
using System.Web;
using Sasha.Lims.Core;
using Sasha.Lims.Core.BusinessObjects;
using Sasha.Lims.Core.DTO.Access;
using Sasha.Lims.Core.Helpers;
using Sasha.Lims.DataAccess.Entities.Constants;
using Sasha.Lims.DataAccess.Entities.Entity;

namespace GroupChange
{

	public class PatientEditModel:EditModelEntity<c_patient>
	{
		public PatientEditModel(int? id):base(id,TableType.Patient)
		{

		}
	}

}
