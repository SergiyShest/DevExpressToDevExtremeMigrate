using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using ChoETL;
using Sasha.Lims.Core;
using Sasha.Lims.Core.BusinessObjects;
using Sasha.Lims.Core.DTO.Access;
using Sasha.Lims.Core.Helpers;
using Sasha.Lims.DataAccess.Entities.Constants;
using Sasha.Lims.DataAccess.Entities.Entity;

namespace GroupChange
{
	/// <summary>
	/// Свойство объекта и его значения для группового редактирования 
	/// </summary>
	public class GroupPropertyVm : ObjectPropertyVm
	{

		string _propertyName;
		protected override string PropertyName
		{
			get
			{

				if (_propertyName == null)
					if (Setting != null && Setting.FieldNumber != null)
					{
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

		public int TypeId = (int) TableType.JourLine;


		private Dictionary<string, PropertyInfo> _propertyInfos = new Dictionary<string, PropertyInfo>();

		#region Value

		public object Value
		{
			set
			{

				if (Setting.IsVirtual && Setting.FieldNumber == null)
				{
					foreach (var data in _addDataList)
					{
						switch (DataType)
						{
							case UserFieldDataType.String: { data.props = value?.ToString(); } break;
							case UserFieldDataType.List:
								{
									int? val = null;
									if (value != null) val = Convert.ToInt32(value);
									data.prop = val;
								}
								break;
							case UserFieldDataType.Date:
								{
									DateTime? val = null;
									if (value != null) val = Convert.ToDateTime(value);
									data.propd = val;
								}
								break;
							case UserFieldDataType.Number:
								{
									decimal? val = null;
									if (value != null) val = Convert.ToDecimal(value);
									data.propn = val;
								}
								break;
						}
					}
				} else
				{
					foreach (var draft in ObjectList)
					{
						var propertyInfo = this.GetPropertyInfo(draft);
						if (propertyInfo == null) return;
						var valType = value?.GetType();
						object newValue;
						if (valType == null)
						{
							newValue = null;
						} else
						{
							Type prType = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;
							if (valType != prType)
							{
								if (prType.IsEnum)
								{
									if (prType == typeof(SampleStatus))
									{
										var tmpValue = Convert.ChangeType(value, typeof(int));
										newValue = (SampleStatus) tmpValue;
									} else
									if (prType == typeof(SampleModel))
									{
										var tmpValue = Convert.ChangeType(value, typeof(int));
										newValue = (SampleModel) tmpValue;
									} else
									{
										throw new ApplicationException();
									}
								}
								else
								{
									if (prType == typeof(DateTime))
									{
										newValue = CoreHelper.ParseRequestDate(value.ToString());
									} else
									{
										try
										{
											newValue = Convert.ChangeType(value, prType);
										}
										catch (Exception ex)
										{
											if (this.AvaiableValues.Count > 0)
											{
												NamedInt newVal = this.AvaiableValues.FirstOrDefault(x=>x.Name == value.ToString());
												if (newVal != null)
												{
													newValue = newVal.Id;
												}
												else 
												{
													var avValus = string.Join(Environment.NewLine, this.AvaiableValues.Select(x => x.Name));
													throw new ApplicationException($"{Name} Dont parsed value \"{value}\" avaiable values is :{Environment.NewLine}{avValus}");

												}
											}else
											throw new ApplicationException($"{Name} Dont parsed value \"{value}\" value type is is :{prType.Name}");;
										}
									}
								}

							} else
							{
								newValue = value;
							}
						}
						try
						{
							propertyInfo.SetValue(draft, newValue);
						}
						catch (Exception ex)
						{
							throw new ApplicationException($"Error set property {propertyInfo.Name}={newValue} ", ex);
						}
						_values = null;
					}
				}
			}

		}

		private List<object> _values;
		public List<object> Values
		{
			get
			{
				if (_values == null)
					_values = new List<object>();
				if (string.IsNullOrEmpty(Name)) return new List<object>();
				{
					int i = 0;
					foreach (var object_ in ObjectList)
					{
						object value = null;
						if (Setting.IsVirtual && Setting.FieldNumber == null)
						{
							var data = _addDataList[i];
							switch (DataType)
							{
								case UserFieldDataType.String: { value = data.props; } break;
								case UserFieldDataType.List: { value = data.prop; } break;
								case UserFieldDataType.Date: { value = data.propd; } break;
								case UserFieldDataType.Number: { value = data.propn; } break;
							}
						} else
						{
							var pi = GetPropertyInfo(object_);
							if (pi != null)
							{
								value = pi.GetValue(object_);
							}
						}
						if (!_values.Contains(value))
						{ _values.Add(value); }
						i++;
					}
				}
				return _values;
			}
			set
			{
				foreach (var object_ in ObjectList)
				{
					var pi = GetPropertyInfo(object_);
					if (pi != null)
						pi.SetValue(object_, value);
				}
			}
		}
		/// <summary>
		/// Реальный тип 
		/// </summary>
		public string ValuesType
		{
			get
			{
				if (_propertyInfos.Count == 0)
				{
					switch (DataType)
					{
						case UserFieldDataType.List: return "Int32"; break;
						case UserFieldDataType.Date: return "DateTime"; break;
						case UserFieldDataType.String: return "String"; break;
						case UserFieldDataType.Number: return "Decimal"; break;
						default: return string.Empty;
					}

				}


				var valType = _propertyInfos[_propertyInfos.Keys.ToList()[0]]?.PropertyType;

				if (valType.IsNullableType())
				{
					valType = valType.GetUnderlyingType();
				}
				return valType?.Name;
			}
		}

		private PropertyInfo GetPropertyInfo(object object_)
		{
			var type = object_.GetType();
			var typeName = type.FullName;
			if (!_propertyInfos.ContainsKey(typeName))
			{
				var pi = type.GetProperty(PropertyName);
				_propertyInfos.Add(type.FullName, pi);
			}
			return _propertyInfos[typeName];
		}

		#endregion

		#region Place


		#endregion

		#region ObjectList

		private DraftList ObjectList { get; }

		List<a_userAddData> _addDataList = new List<a_userAddData>();
		#endregion



		#region Constructors

		public GroupPropertyVm(DraftList objectList, IField setting) : base(setting, (int) TableType.Sample)
		{
			ObjectList = objectList;
			if (setting.IsVirtual && setting.FieldNumber == null)//установка в качестве объекта a_userAddData
			{
				a_userAddData userAddData = null;
				foreach (var obj in ObjectList)
				{
					if (obj != null)
					{
						userAddData = DataHelper.UOW.GetRepository<a_userAddData>().FirstOrDefault(x => x.row_id == obj.Id && x.table_id == TypeId && x.setting_id == setting.Id, false);
					}
					if (userAddData == null)
					{
						userAddData = new a_userAddData()
						{
							table_id = TypeId,
							row_id = obj.Id,
							setting_id = setting.Id,
							createdDateTime = DateTime.Now,
						};
					}
					_addDataList.Add(userAddData);
				}
			}
		}

		public void Save(UserDTO user, List<int?> ids)
		{
			var notFindIdList = new List<int>();

			var rep = DataHelper.UOW.GetRepository<a_userAddData>();
			foreach (var id in ids)
			{
				var ok = false;
				foreach (var userdata in _addDataList)
					if (userdata.row_id == id)
					{
						if (userdata.id == 0) userdata.createdDateTime = DateTime.Now;
						rep.Save(userdata);
						ok = true; break;
					}
				if (!ok)
				{
					//	notFindIdList.Add((int)id);
				}


			}

			foreach (var id in notFindIdList)
			{

			}

		}

		#endregion

	}





}
