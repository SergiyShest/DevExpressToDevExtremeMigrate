using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using ChoETL;
using Newtonsoft.Json;
using Sasha.Lims.Core;
using Sasha.Lims.Core.BusinessObjects;
using Sasha.Lims.Core.DTO.Access;
using Sasha.Lims.Core.Helpers;
using Sasha.Lims.Core.Infrastructure.Exceptions;
using Sasha.Lims.DataAccess.Entities.Constants;
using Sasha.Lims.DataAccess.Repositories.Abstract;

namespace GroupChange
{
	public class GroupEditModel
	{
		private List<int> _ids = new List<int>();
		private IUnitOfWorkEx _uow;
		private DraftList _draftsList;
		public int EditingSamplesCount { get { return _ids.Count; } }

		TypeDescriptionBo _typeDescription;
		public TypeDescriptionBo TypeDescription
		{
			get
			{
				if (_typeDescription == null)
				{
					_typeDescription = new TypeDescriptionBo((int) TableType.Sample);
				}
				return _typeDescription;
			}
		}

		List<IField> _settingList;

		public List<IField> SettingList
		{


			get
			{
				if (_settingList == null || _settingList.Count == 0)
				{
					_settingList = DataHelper.Current.GetEditSetting(PropsType.SampleEditSettings);
					if (_settingList.Count == 0)
					{
						_settingList = DefaultSettingList;
						foreach (IField set in _settingList)
						{
							var setting = set as EditSettingItem;
							setting.SerializeValue();
							setting.Save(this._uow);
						}
					}
					//дополняю кастомными значениями

					foreach (var fieldDescription in TypeDescription.Fields)
					{
						if (fieldDescription.IsVirtual && fieldDescription.ShowOnForm)
						{
							if (!_settingList.Where(x => x.Name == fieldDescription.Name).Any())
								_settingList.Add(fieldDescription);
						}
					}
				}
				return _settingList;
			}

			//}
			//return _settingList;

			set { _settingList = value; }

		}
		public List<IField> DefaultSettingList { get { return _defaultSettingList; } }
		private List<IField> _defaultSettingList = new List<IField> {
					new EditSettingItem(PropsType.SampleEditSettings,"Name","Name",1,"",UserFieldDataType.String){Rules= new List<string> { "v =>!v||(v && v.length <= 50)||'Max 50 characters'" } },
					new EditSettingItem(PropsType.SampleEditSettings,"Barcode","Barcode",1,"",UserFieldDataType.String){Rules= new List<string> { "v =>!v||(v && v.length <= 20)||'Max 20 characters'" } },
					new EditSettingItem(PropsType.SampleEditSettings,"CollectedDate","Collected Date",1,"",UserFieldDataType.Date),
					new EditSettingItem(PropsType.SampleEditSettings,"SampleTypeId","Sample Type",1,"",UserFieldDataType.Number),
					new EditSettingItem(PropsType.SampleEditSettings,"ModelId","Model ",1,"",UserFieldDataType.Number){Rules= new List<string> { "v =>!v||'Can`t be null'" } },
					new EditSettingItem(PropsType.SampleEditSettings,"StatusId","Status ",1,"",UserFieldDataType.Number),
					new EditSettingItem(PropsType.SampleEditSettings,"LocationId","Location ",1,"",UserFieldDataType.Number),
					new EditSettingItem(PropsType.SampleEditSettings,"BatchId","Batch ",1,"",UserFieldDataType.Number),

					new EditSettingItem(PropsType.SampleEditSettings,"Volume","Volume",2,"Box,Plate",UserFieldDataType.Number){Rules= new List<string> { @"v => /^\d+$/.test(v) || 'must be number'" } },
					new EditSettingItem(PropsType.SampleEditSettings,"VolumeUnitId","Volume Unit",2,"Box,Plate",UserFieldDataType.Number),
					new EditSettingItem(PropsType.SampleEditSettings,"TubeTypeId","Tube Type",2,"Box,Plate",UserFieldDataType.Number),

					new EditSettingItem(PropsType.SampleEditSettings,"Well","Well",2,"Sample,Aliquote",UserFieldDataType.String){Rules= new List<string> { "v =>!v||(v && v.length <= 10)||'Max 10 characters'" } },
					new EditSettingItem(PropsType.SampleEditSettings,"PatientId","Patient Id",2,"Sample,Aliquote",UserFieldDataType.Number),
					new EditSettingItem(PropsType.SampleEditSettings,"PatientNo","Patient No",2,"Sample,Aliquote",UserFieldDataType.String),

					new EditSettingItem(PropsType.SampleEditSettings,"PatientEmail","Patient Email",2,"Sample,Aliquote",UserFieldDataType.String){Rules= new List<string> { @"v => /^\w+([.-]?\w+)*@\w+([.-]?\w+)*(\.\w{2,3})+$/.test(v) || 'E-mail must be valid'" } },
					new EditSettingItem(PropsType.SampleEditSettings,"PatientSendEmail","Patient Send Email",2,"Sample,Aliquote",UserFieldDataType.Boolean),

					new EditSettingItem(PropsType.SampleEditSettings,"PatientFirstName","Patient First Name",2,"Sample,Aliquote",UserFieldDataType.String),
					new EditSettingItem(PropsType.SampleEditSettings,"PatientLastName","Patient Last Name",2,"Sample,Aliquote",UserFieldDataType.String),
					new EditSettingItem(PropsType.SampleEditSettings,"PatientDateOfBirth","Patient Date Of Birth",2,"Sample,Aliquote",UserFieldDataType.Date),
					new EditSettingItem(PropsType.SampleEditSettings,"DoctorId","Doctor",2,"Sample,Aliquote",UserFieldDataType.Number),
					new EditSettingItem(PropsType.SampleEditSettings,"HospitalId","Hospital",2,"Sample,Aliquote",UserFieldDataType.Number),

		};

		public List<GroupPropertyVm> ChangedFreePropertyList = new List<GroupPropertyVm>();

		private List<GroupPropertyVm> _propertyList;

		public List<GroupPropertyVm> PropertyList
		{
			get
			{
				if (_propertyList == null)
				{
					_propertyList = new List<GroupPropertyVm>();

					foreach (var property in SettingList)
					{
						var pr = new GroupPropertyVm(ObjectList, property);

						_propertyList.Add(pr);

					}
				}
				return _propertyList;
			}
			set
			{
				_propertyList = value;
			}
		}

		public DraftList ObjectList
		{
			get
			{
				if (_draftsList == null)
				{
					_draftsList = new DraftList(_uow);
					_draftsList.Ids = _ids;
					_draftsList.BaseLoad();
				}
				return _draftsList;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="idsStr">Ids string, if empty mean create new</param>
		/// <param name="uow"></param>
		public GroupEditModel(string idsStr, IUnitOfWorkEx uow)
		{
			_ids = CoreHelper.ConvertIdsToArr(idsStr);
			_uow = uow;
		}

		public string GetJson()
		{
			dynamic expOb = new ExpandoObject();
			expOb.Item = GetDTOs();
			expOb.Errors = LastErrors;
			expOb.EditingSamplesCount = EditingSamplesCount;
			var json = JsonConvert.SerializeObject(expOb);
			return json;
		}

		public List<ObjectError> LastErrors { get; } = new List<ObjectError>();


		private IDictionary<string, object> GetDTOs()
		{
			LastErrors.Clear();
			var expOb = new ExpandoObject() as IDictionary<string, object>;
			foreach (var property in PropertyList)
			{
				try
				{
					expOb.Add(property.Name, new
					{
						Values = property.Values,
						ValuesType = property.ValuesType,
						Column = property.Column,
						Row = property.Row,
						Text = property.Caption,
						TargetTypes = property.TargetTypes,
						Rules = property.Rules,
						AvaiableValues = property.AvaiableValues,
						ControlType = property.ControlType?.ToString(),
						DataType = property.DataType?.ToString(),
					});
				}
				catch (Exception ex)
				{
					//throw new NamedException(property.Name, $"For settings with the id = {property.Row} got error,possible  more than one record with the Name = {property.Name} was found in the child settings");
					LastErrors.Add(new ObjectError(property.Name, $"For settings with the id = {property.Row} got error,possible  more than one record with the Name = {property.Name} was found in the child settings {Environment.NewLine}{ex.Message}")); ;

				}
			}
			return expOb;
		}

		public List<ObjectError> SetJson(string json)
		{
			List<ObjectError> errorList = new List<ObjectError>();
			try
			{
				var obj = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
				if (obj != null)
				{
					if (_ids.Count == 0) //case when new object
					{
						if (!obj.ContainsKey("ModelId"))
						{
							throw new ApplicationException("Didn't set Model property");
						}
						var modelId = (int) Convert.ChangeType(obj["ModelId"], typeof(int));
						AddNew(modelId);
					}
					foreach (var key in obj.Keys)
					{
						var pr = PropertyList.FirstOrDefault(x => x.Name == key);
						if (pr == null)
						{
							errorList.Add(new ObjectError("des object", $"Not found property {key}"));
							continue;
						}
						pr.Value = obj[key];
						if (pr.Setting.IsVirtual && pr.Setting.FieldNumber == null)
						{
							ChangedFreePropertyList.Add(pr);
						}
					}
				}
			}
			catch (Exception es)
			{
				errorList.Add(new ObjectError(" Exception", es.Message));
			}

			return errorList;
		}

		public void AddNew(int modelId)
		{
			ObjectList.Add(this.CreateNew(modelId));
			_propertyList = null;
		}


		private BaseDraft CreateNew(int modelId)
		{
			BaseDraft draft;
			switch ((SampleModel) modelId)
			{
				case SampleModel.Plate:
					draft = new PlateDraft();
					break;
				case SampleModel.Box:
					draft = new BoxDraft();
					break;
				case SampleModel.Aliquot:
					draft = new AliquoteDraft();
					break;
				case SampleModel.Sample:
					draft = new SampleDraft();
					break;
				default: throw new NotImplementedException($"For {modelId} not implemented now");
			}
			draft.Record.model_id = draft.ModelId;//убиваю двух зайцев
												  //создаю Record и присваюваю ему ModelId

			return draft;
		}

		public List<ObjectError> Save(UserDTO user, JournalType? draftMode)
		{
			if (draftMode != null)
				foreach (var obj in ObjectList)
				{
					obj.JourTypeId = draftMode;
					if (obj.ModelId == null)
					{
						obj.ModelId = SampleModel.Sample;
					}

				}
			var err = ObjectList.Save(user);
			var errId = err.Select(x => x.Id).ToList();
			var savedId = ObjectList.Select(x => x.Id).ToList();

			foreach (var freeProp in this.ChangedFreePropertyList)
			{
				freeProp.Save(user, savedId);
			}
			return err;
		}

		public List<ObjectError> ApplayEdit(UserDTO user)
		{
			foreach (var obj in ObjectList)
			{
				obj.Record.aspNetUser_Id = user.Id;
				obj.Record.user_id = user.UserId;
			}
			return ObjectList.ApplayEdit();
		}
	}





}
