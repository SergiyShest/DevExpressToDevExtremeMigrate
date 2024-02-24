using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sasha.Lims.DataAccess.Entities.Constants;

namespace Sasha.Lims.DataAccess.Entities.Entity
{

	public interface IBaseSampleRecord : IEntity
	{

		string Barcode { get; set; }

		string Name { get; set; }

		DateTime? CollectedDate { get; set; }

		int? StatusId { get; set; }

		int? HospitalId { get; set; }

		int? LocationId { get; set; }

		int? UserId { get; set; }

		string AspNetUserId { get; set; }

		int? SampleTypeId { get; set; }

		DateTime? DateCreate { get; set; }

		DateTime? ModDate { get; set; }


		DateTime? AcceptedDate { get; set; }

		int? ParentId { get; set; }

		int? BatchId { get; set; }


		int? prop1 { get; set; }

		int? prop2 { get; set; }

		int? prop3 { get; set; }

		//int? prop4 { get; set; }

		int? prop5 { get; set; }

		int? prop6 { get; set; }

		int? prop7 { get; set; }

		int? prop8 { get; set; }

		int? prop9 { get; set; }

		int? prop10 { get; set; }


		int? ProcessId { get; set; }
	}

}
