using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.Serialization;
using ChoETL;
using Newtonsoft.Json;
using Sasha.Lims.Core;
using Sasha.Lims.Core.BusinessObjects;
using Sasha.Lims.Core.DTO.Access;
using Sasha.Lims.Core.Helpers;
using Sasha.Lims.DataAccess.Entities.Constants;
using Sasha.Lims.DataAccess.Entities.Entity;
using Sasha.Lims.DataAccess.Repositories;

namespace GroupChange
{

	public interface IVueModel
	{
		string GetJson();
		List<ObjectError> SetJson(string json);
		List<ObjectError> Save(UserDTO user);
		List<ObjectError> Delete();
	}


	/// <summary>
	/// общий класс для редактирования объектов через Vue
	/// Git pass =g9M98qZ4Ju8B8WSVF8jm 
	/// Свойства объекта содержатся в коллекции PropertyList создаваемой на основе SettingList
	/// Получение свойств метод GetJson()
	/// Установка свойств метод  SetJson(string json)
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class EditModelBase<T>:IVueModel where T : class, new()
	{
		protected NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		protected IUnitOfWorkEx _uow;
		protected object _lock = new object();

		public List<ObjectError> LastErrors { get; } = new List<ObjectError>();

		#region Id

		protected int? _id;
		public int? Id {
			get => _id;
			set => _id = value;
		}

		#endregion

		#region TypeId

		protected int? _typeId;
		//id 
		public int TypeId
		{
			get
			{
				if (_typeId == null) return -1;
				return (int) _typeId;
			}
		}

		#endregion

		#region Object

		public T _object;
		public T Object
		{
			get
			{
				if (_object == null)
				{
					_object = getObject(Id);
				}
				return _object;
			}
			set{ _object = value; }
		}
		protected abstract T getObject(int? id);

		#endregion

		#region AdditionalObject

		protected a_userData _addPropertyObject;

		#endregion

		#region SettingList 

		protected List<IField> _settingList;
		/// <summary>
		/// 
		/// </summary>
		public virtual List<IField> SettingList
		{
			get
			{
				if (_settingList == null)
				{
					_settingList = new List<IField>();
					var properties = typeof(T).GetProperties();
					foreach (var property in properties)
					{
						var text = property.Name;

						var custAtt = property.CustomAttributes;
						if (property.CustomAttributes.Any(x => x.AttributeType == typeof(DataMemberAttribute)))
						{
							var dataType = CoreHelper.GetFieldType(property);

							bool readOnly = !property.CanWrite;

							var controlType = Control.TextField;
							if (dataType == UserFieldDataType.Boolean)
							{
								controlType = Control.CheckBox;
							}

							var item = new EditSettingItem(property.Name, text, 1, String.Empty, readOnly, controlType) { DataType = dataType };
							_settingList.Add(item);
						}
					}
				}
				return _settingList;
			}
		}
		#endregion

		#region PropertyList build on settingList 

		protected List<IObjectPropertyVm> _propertyList;

		public virtual List<IObjectPropertyVm> PropertyList
		{
			get
			{
				if (_propertyList == null||_propertyList.Count==0)
				{
					lock (_lock)
					{
						if (_propertyList == null||_propertyList.Count==0)
						{
							_propertyList = new List<IObjectPropertyVm>();

							foreach (var setting in SettingList)
							{
								var pr = new ObjectPropertyVm(Object, ref _addPropertyObject, setting, TypeId);
								if (setting.ControlType == Control.DoubleTextField)
								{
									pr.PairRroperty= new ObjectPropertyVm(Object, ref _addPropertyObject, (EditSettingItem)setting.PairSetting, TypeId);
								}
								_propertyList.Add(pr);
							}
						}
					}
				}
				return _propertyList;
			}
		}

		#endregion

		#region Constructor

		/// <summary>
		/// 
		/// </summary>
		/// <param name="idsStr">Ids string, if empty mean create new</param>
		/// <param name="uow"></param>
		///  <param name="typeId">used for custom field</param>
		public EditModelBase(int? id, int? typeId)
		{
			Id = id;
			_uow = new UnitOfWork();
			_typeId = typeId;
		}

		#endregion

		#region Metods

		public string GetJson()
		{
			dynamic expOb = new ExpandoObject();
			expOb.Item = GetDTOs();
			expOb.Errors = LastErrors;
			var json = JsonConvert.SerializeObject(expOb);
			return json;
		}

		/// <summary>
		/// получение json свойств объекта
		/// </summary>
		/// <returns></returns>
		protected virtual IDictionary<string, object> GetDTOs()
		{
			LastErrors.Clear();
			var expOb = new Dictionary<string, object>();
			foreach (var property in PropertyList)
			{
				try
				{
					var obj = property.GetJsonObject();
					expOb.Add(property.Name, obj);
				}
				catch (Exception ex)
				{
					LastErrors.Add(new ObjectError(property.Name, ex.GetAllMessages()));
				}
			} // add from propertyList
			return expOb;
		}


		/// <summary>
		/// set property form json to object
		/// </summary>
		/// <param name="json"></param>
		/// <returns></returns>
		public virtual List<ObjectError> SetJson(string json)
		{
			LastErrors.Clear();
			try
			{
				var obj = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
				if (obj != null)
				{
					foreach (var key in obj.Keys)
					{
						var k=key.ToLower();

						var pr = PropertyList.FirstOrDefault(x => x.Name?.ToLower() == k);//find property 
						if (pr == null)
						{

							LastErrors.Add(new ObjectError(Object.GetType().Name, $"Not found property {key}"));
							continue;
						}
						try
						{
							dynamic odb  = obj[key];
							if(odb == null)
							{ pr.Value = null; }
							else{ 
							if (!CoreHelper.ExistsProperty(odb,"Value"))
							{
                               pr.Value = obj[key];
							} else {
								pr.Value = odb.Value;
								if (pr.PairRroperty != null && CoreHelper.ExistsProperty(odb, "PairPropertyValue"))
								{
									pr.PairRroperty.Value = odb.PairPropertyValue;
								}
							}
							}
						}
						catch (Exception ex)
						{
                          LastErrors.Add(new ObjectError("Set value to "+Object.GetType().Name+"."+key+"="+obj[key], ex.GetAllMessages()));
						}
					}
				}
			}
			catch (Exception es)
			{
				LastErrors.Add(new ObjectError(" Exception", es.Message));
			}

			return LastErrors;
		}

		public abstract List<ObjectError> Save(UserDTO user);

		public virtual List<ObjectError> Delete()
		{
			throw new NotImplementedException();
		}

		#endregion

	}


}
