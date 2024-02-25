using System;
using System.Collections.Generic;
using Sasha.Lims.Core;
using Sasha.Lims.Core.BusinessObjects;
using Sasha.Lims.Core.DTO.Access;
using Sasha.Lims.Core.Helpers;
using Sasha.Lims.DataAccess.Entities.Constants;
using Sasha.Lims.DataAccess.Entities.Entity;

namespace GroupChange
{
	public class DoctorEditModel:EditModelEntity<c_doctor>
	{
		public DoctorEditModel(int? id):base(id,TableType.Doctor)
		{

		}
}





}
