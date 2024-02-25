using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using ChoETL;
using Sasha.Lims.Core;
using Sasha.Lims.Core.Helpers;
using Sasha.Lims.DataAccess.Entities.Constants;
using Sasha.Lims.DataAccess.Entities.Entity;
using System.Linq;
using System.Globalization;
using System.Collections;
using Sasha.Lims.Core.BusinessObjects;
using Sasha.Lims.Core.DTO.Access;
using Sasha.Lims.DataAccess.Repositories.Abstract;

namespace GroupChange
{
	/// <summary>
	/// Обертка над свойством объекта и его значением и представлением в интерфейсе
	/// </summary>
	public class ObjectPropertyVm : IObjectPropertyVm
	{

		public IField Setting { get; }

		private PropertyInfo _propertyInfo;
		int? _typeId;

		#region Name

		public string Name {
			get {
				if (string.IsNullOrEmpty(Setting.Name))
				{
                   return Setting.Text;
				}
				return Setting.Name;
			}
		}

		string _propertyName;
		protected virtual string PropertyName
		{
			get
			{
				var settig = Setting as FieldDescription;
				if (settig != null) return settig.PropertyName;
				if (_propertyName == null)
					if (Setting != null && Setting.FieldNumber != null)
					{//todo:переделать все имена одинаково
						switch ((UserFieldDataType) Setting.DataType)
						{
							case UserFieldDataType.List: _propertyName = "Prop" + Setting.FieldNumber; break;
							case UserFieldDataType.String: _propertyName = "PropS" + Setting.FieldNumber; break;
							case UserFieldDataType.Date: _propertyName = "PropD" + Setting.FieldNumber; break;
							case UserFieldDataType.Number: _propertyName = "PropN" + Setting.FieldNumber; break;
							default: _propertyName = "Prop" + Setting.FieldNumber; break;
						}
					} else
					{
						_propertyName = Name;
					}
				return _propertyName;
			}
		}

		public string ListName { get { return Setting.ListName; } }

		#endregion

		public string GroupName { get { return Setting.GroupName; } }

		#region Text
		string _caption;
		public string Caption
		{
			get
			{
				if (string.IsNullOrEmpty(_caption))
				{
					if (string.IsNullOrEmpty(Setting.Text))
					{
						_caption = Setting.Name;
					}
						else
					{
						_caption = Setting.Text;
					}
				}
				return _caption;
			}
			set
			{
				_caption = value;
			}
		}
		#endregion

		#region Value

		public virtual object Value
		{
			get
			{
				object obValue = null;
				if (obValue == null && Setting.HasValue)
				{
					if (!PropertyInfo.CanRead)
					{
						throw new InvalidOperationException($"object {Object.GetType().Name} has property {Name} bat is not readable.");
					}

					obValue = PropertyInfo.GetValue(Object);
				}
				return obValue;
			}

			set
			{
				var val = value;
				if (val != null)
				{
					Type prType = Nullable.GetUnderlyingType(PropertyInfo.PropertyType) ?? PropertyInfo.PropertyType;
					if (prType == typeof(decimal))
					{
						val = decimal.Parse(val.ToString().Replace(",", "."), NumberStyles.Number, CultureInfo.InvariantCulture);
					}
					val = Convert.ChangeType(val, prType);
				}
				if (!PropertyInfo.CanWrite)
				{
					throw new InvalidOperationException($"object {Object.GetType().Name} has property {Name} bat is not writable.");
				}

				PropertyInfo.SetValue(Object, val);
			}

		}


		/// <summary>
		/// 
		/// </summary>
		public string ValuesType
		{
			get
			{
				var valType = PropertyInfo?.PropertyType;
				if (valType != null)
					if (valType.IsNullableType())
					{
						valType = valType.GetUnderlyingType();
					}
				return valType?.Name;
			}
		}


		private PropertyInfo GetPropertyInfo()
		{
			{
				if (_propertyInfo == null)
				{
					if (Object == null) return null;
					_propertyInfo = Object.GetType().GetProperty(PropertyName);
					if (_propertyInfo == null)
					{
						_propertyInfo = Object.GetType().GetProperty(PropertyName.ToLower());
					}
					if (_propertyInfo == null)
					{

					}
				}
				return _propertyInfo;
			}
		}

		public virtual object GetJsonObject()
		{
			return GetJsonObject(Value, ValuesType);
		}

		public virtual object GetJsonObject(object value, string valuesType)
		{
			IList avaiableValues = null;
			if (ControlType == Control.Select || ControlType == Control.ComboBox)
			{
				avaiableValues = AvaiableValues;
				if (DataType == UserFieldDataType.String && AvaiableValues != null)
				{
					avaiableValues = AvaiableValues.Where(x => x.Name != null).Select(x => x.Name).ToList();
				}
			}

			return new
			{
				Value = value,
				Name = Name,
				ValuesType = valuesType,
				Column = Column,
				Row = Row,
				Text = Caption,
				GroupName = GroupName,
				Rules = Rules,
				ControlType = ControlType?.ToString(),
				List = ListName,
				Readonly = Readonly,
				DataType = DataType?.ToString(),
				AvaiableValues = avaiableValues,
				Tab = Setting.Tab,
				DataSource = Setting.DataSource,
				PairProperty = PairRroperty?.GetJsonObject()

			};
		}

		private PropertyInfo PropertyInfo
		{
			get
			{
				if (Object == null) return null;
				if (GetPropertyInfo() == null)
				{
					throw new InvalidOperationException($"object {Object.GetType().Name} has not property {Name}");
				}
				return GetPropertyInfo();
			}
		}
		#endregion

		#region AvaiableValues

		List<NamedInt> _avaiableValues;
		public List<NamedInt> AvaiableValues
		{
			get
			{
				if (_avaiableValues == null || _avaiableValues.Count == 0)
				{
					if (DataType == UserFieldDataType.List)
					{
						var usd = DataHelper.Current.GetUserFieldDescription((int) _typeId, Name);
						if (usd != null)
							_avaiableValues = usd.AvaiableValues;
						else { _avaiableValues = new List<NamedInt>(); }
					}
				}
				return _avaiableValues;
			}
			private set
			{
				_avaiableValues = value;
			}
		}

		#endregion

		/// <summary>
		/// Column where property need be placed
		/// </summary>
		public int? Column { get { return Setting.Column; } }
		/// <summary>
		/// Row where property need be placed
		/// </summary>
		public int? Row { get { return Setting.Order; } }

		public Control? ControlType { get { return Setting.ControlType; } }

		public bool? Readonly { get { return Setting.Readonly; } }

		#region TargetTypes

		public string TargetTypes { get { return Setting.TargetTypes; } }


		#endregion

		public UserFieldDataType? DataType { get { return Setting.DataType; } }

		#region Object

		 a_userAddData _userAddData { get; set; }


		private object Object { get; }
		#endregion
		public List<string> Rules { get { return Setting.Rules; } }

		public IObjectPropertyVm PairRroperty { get; set; }


		#region Constructors
		//static Regex regName = new Regex(@"^Prop(\d+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		public ObjectPropertyVm(IField setting, int? typeId)
		{
			Setting = setting;
			_typeId = typeId;
			AvaiableValues = setting.AvaiableValues;



		}
		public ObjectPropertyVm(object obj, IField setting, int? typeId) : this(setting, typeId)
		{
			Object = obj;
		}


		public ObjectPropertyVm(object obj, ref a_userData dobObj, IField setting, int? typeId) : this(setting, typeId)
		{
			Object = obj;
			if (GetPropertyInfo() == null)//Нужное свойство возможно находится в дополнительном объекте
			{
				IEntity entity = obj as IEntity;
				if (setting.FieldNumber == null)//установка в качестве объекта a_userAddData
				{
					if (entity != null)
					{
						_userAddData = DataHelper.UOW.GetRepository<a_userAddData>().FirstOrDefault(x => x.row_id == entity.id && x.table_id == (int) typeId && x.setting_id == setting.Id, false);
					}
					if (_userAddData == null)
					{
						_userAddData = new a_userAddData()
						{
							table_id = (int) typeId,
							setting_id = setting.Id,
							createdDateTime = DateTime.Now,
						};
					}

					Object = _userAddData;
				} else
				{//установка в качестве объекта a_userData

					if (dobObj == null)
					{
						if (entity != null)
							dobObj = DataHelper.UOW.GetRepository<a_userData>().GetAll().FirstOrDefault(x => x.row_id == entity.id && x.table_id == (int) typeId);
						if (dobObj == null)
						{
							dobObj = new a_userData() { row_id = entity?.id, table_id = (int) typeId };
						}
					}
					Object = dobObj;
				}
			}
		}

		#endregion



		public virtual void Save(UserDTO user, int mainId)
		{

			if (_userAddData != null)
			{
				var rep = DataHelper.UOW.GetRepository<a_userAddData>();
				_userAddData.aspNetUser_Id = user.Id;
				_userAddData.row_id = mainId;

				if (_userAddData.id == 0) _userAddData.createdDateTime = DateTime.Now;

				rep.Save(_userAddData);
			}
		}
	}

	public interface IObjectPropertyVm
	{
		IField Setting { get; }
		string Name { get; }
		object Value { get; set; }
		//string ValuesType { get; }
		//int Column { get; }
		//int? Row { get; }
		//string Text { get; }
		//List<string> Rules { get; }
		//List<NamedInt> AvaiableValues { get; }
		IObjectPropertyVm PairRroperty { get; }
		object GetJsonObject();
		void Save(UserDTO user, int mainId);
	}
}
