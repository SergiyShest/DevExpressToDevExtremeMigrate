//using System;
//using System.Collections.Generic;
//using System.Data.Entity;
//using System.Data.Entity.Infrastructure;
//using System.Globalization;
//using System.Linq;
//using System.Reflection;
//using System.Text.RegularExpressions;
//using AutoMapper;
//using Sasha.Lims.Core.BusinessObjects;
//using Sasha.Lims.Core.Consts;
//using Sasha.Lims.Core.DTO.Access;
//using Sasha.Lims.Core.DTO.Common;
//using Sasha.Lims.Core.DTO.Laboratory;
//using Sasha.Lims.Core.Helpers;
//using Sasha.Lims.Core.Infrastructure.Classes;
//using Sasha.Lims.Core.Infrastructure.Exceptions;
//using Sasha.Lims.Core.Infrastructure.Extentions;
//using Sasha.Lims.Core.Interfaces.Common;
//using Sasha.Lims.Core.Interfaces.Laboratory;
//using Sasha.Lims.Core.Services.Laboratory.Logics;
//using Sasha.Lims.DataAccess.Entities.Constants;
//using Sasha.Lims.DataAccess.Entities.Entity;
//using Sasha.Lims.DataAccess.Helpers;
//using Sasha.Lims.DataAccess.Repositories;
//using Sasha.Lims.DataAccess.Repositories.Abstract;
//using IFilterConditionsHelper = Sasha.Lims.Core.Helpers.Abstract.IFilterConditionsHelper;

using Sasha.Lims.Core.BusinessObjects;
using Sasha.Lims.Core.DTO.Access;
using Sasha.Lims.Core.DTO.Common;
using Sasha.Lims.Core.Helpers;
using Sasha.Lims.Core.Interfaces.Laboratory;
using Sasha.Lims.DataAccess.Entities.Constants;
using Sasha.Lims.DataAccess.Entities.Entity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sasha.Lims
{
	public class SamplesModel
	{
		private readonly IUnitOfWorkEx _unitOfWork;

		public SamplesModel(IUnitOfWorkEx unitOfWork)
		{
			_unitOfWork = unitOfWork;

		}

		private RSample GetLastEditSampleObj(int aspNetUserId)
		{
			RSample sampleBo = null;
			var sampleRec = DataHelper.UOW.GetRepository<s_sample>().Where(x => x.user_id == aspNetUserId && x.model_id == SampleModel.Sample).OrderByDescending(x => x.ModDate).FirstOrDefault();

			if (sampleRec != null)
			{
				sampleBo = new RSample(sampleRec);
			}
			return sampleBo;
		}

		public void GroupInputSave(string selectedIds, GroupInputDto dto, UserDTO usr)
		{
			//			var ids = CoreHelper.ConvertIdsToArr(selectedIds);
			//			var samples = GetSamplesByIds(ids);
			//			int ind = 0;
			//			foreach (var sample in samples)
			//			{
			//				var dataArr = dto.GetData(ind);

			//				for (int fieldNr = 0; fieldNr < dto.Fields.Count; fieldNr++)
			//				{
			//					var fieldName = dto.Fields[fieldNr];
			//					if (string.IsNullOrWhiteSpace(fieldName)) continue;

			//					var valStr = dataArr[fieldNr];
			//					var prInfo = sample.GetType().GetProperty(fieldName);
			//					if (prInfo == null)
			//					{
			//						throw new Exception("Field " + fieldName + " not found");
			//					}
			//					object value = null;
			//					if (valStr != null)
			//					{
			//						var type = Nullable.GetUnderlyingType(prInfo.PropertyType);
			//						if (type == null)
			//						{
			//							type = prInfo.PropertyType;
			//						}
			//						if (type == typeof(DateTime))
			//						{
			//							value = CoreHelper.ParseRequestDate(valStr);
			//						} else
			//						{
			//							value = Convert.ChangeType(valStr, type);
			//						}
			//					}
			//					prInfo.SetValue(sample, value);
			//				}
			////				EditSample(sample, usr);
			//				ind++;
			//			}
		}

		public void GroupInputInsert(dynamic data, JournalType draftMode, UserDTO usr)
		{
			List<ObjectError> errorList = new List<ObjectError>();
			var lastEditedSample = GetLastEditSampleObj(usr.UserId.GetValueOrDefault(0));

			if (lastEditedSample != null)
			{
				lastEditedSample.Id = 0;
				lastEditedSample.ParentId = null;
				lastEditedSample.ProcessId = null;
				lastEditedSample.BatchId = null;
				lastEditedSample.AspNetUser_Id = null;
				lastEditedSample.Well = string.Empty;
			} else
			{
				lastEditedSample = new RSample(new s_sample
				{
					aspNetUser_Id = usr.Id,
					user_id = usr.UserId.GetValueOrDefault(0)
				});
			}

			
			for (int dd = 0; dd < data.Count; dd++)
			{

				var dataArr = data[dd];
				var model = new GroupChange.GroupEditModel(null, _unitOfWork);
				model.AddNew((int) SampleModel.Sample);

				model.PropertyList.FirstOrDefault(x => x.Name == "CollectedDate").Value = lastEditedSample.CollectedDate;
                model.PropertyList.FirstOrDefault(x => x.Name == "SampleTypeId").Value =lastEditedSample.SampleTypeId;
				model.PropertyList.FirstOrDefault(x => x.Name == "DoctorId").Value = lastEditedSample.DoctorId;
                model.PropertyList.FirstOrDefault(x => x.Name == "HospitalId").Value =lastEditedSample.HospitalId;

	

				foreach (var pr in model.PropertyList)
				{
					var fieldName = pr.Name;
					var val = dataArr[fieldName];

				    pr.Value = val;
					if (pr.Setting.IsVirtual && pr.Setting.FieldNumber == null)
					{
						model.ChangedFreePropertyList.Add(pr);
					}
				}
				model.Save(usr, draftMode);


			}
		}
	}


}