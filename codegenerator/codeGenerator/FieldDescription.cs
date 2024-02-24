using System.Text.RegularExpressions;
using System.Text;
using System.Reflection;
using static System.Net.Mime.MediaTypeNames;
namespace CodeGenerator
{
	public class FieldDescription
	{
		Dictionary<string, string> possibleNames = new Dictionary<string, string> {
			{ "DocNum", "Номер документа" }
			, {"CurrId","Валюта"}
			, {"CurId","Валюта"}
			, {"Summ","Сумма"}
			, {"Sum","Сумма"}
		};

		Dictionary<string, string> possibleSelectNames = new Dictionary<string, string> {
			{ "CurrId", "CurrList" },
			{ "CurId", "CurrList" },
			{ "OstId", "OstList" }
		};

		List<ColumnDescription> _columnDescr;


		#region ItemsSource
		private string? _itemsSource;
		public string? ItemsSource
		{
			get { return _itemsSource; }
			set
			{
				_itemsSource = $":items='{value}' " ;
			}
		}
		#endregion

		#region FieldName
		private string? _fieldName;
		public string? FieldName
		{
			get { return _fieldName; }
			set
			{
				if (value != null) { value = value.Replace("\"", ""); }
				_fieldName = value;
			}
		}
		#endregion

		#region Caption
		private string? _caption;
		public string? Caption
		{
			get
			{
				if (_caption == null)
				{

					if (_columnDescr != null)
					{
						_caption = _columnDescr.FirstOrDefault(x => x.FieldName == FieldName)?.Caption;
					}
					if (_caption == null)
					{
						_caption = possibleNames.FirstOrDefault(x => x.Key == FieldName).Value;
					}
					if (_caption == null)
					{
						_caption = FieldName;
					}

				}
				return _caption;
			}
			set
			{
				if (value != null) { value = value.Replace("\"", ""); }
				_caption = value;
			}
		}
		#endregion

		#region FieldType
		Type _fieldType;
		public Type FieldType
		{
			get { return _fieldType; }
			set
			{
				if (Nullable.GetUnderlyingType(value) != null)
					_fieldType = Nullable.GetUnderlyingType(value);
				else { _fieldType = value; }
			}
		}
		#endregion

		#region ControlType
		ControlType? _controlType;
		public ControlType? ControlType
		{
			get
			{
				if (_controlType == null)
				{
					if (possibleSelectNames.TryGetValue(FieldName, out string dataSource)){
                         _controlType = CodeGenerator.ControlType.select;
						ItemsSource = dataSource;
					}
					else {
						var name = FieldType.Name;
						switch (name)
						{
							case "String":
								_controlType = CodeGenerator.ControlType.input;
								break;
							case "Int32":
							case "Decimal":
								_controlType = CodeGenerator.ControlType.number;
								break;
							case "DateTime":
								_controlType = CodeGenerator.ControlType.date;
								break;
							case "Boolean":
								_controlType = CodeGenerator.ControlType.check;
								break;
							default: throw new NotImplementedException($"type {name} not implemented");
						} 
					}
				}
				return _controlType;
			}
			set
			{
				_controlType = value;
			}
		}
		#endregion

		public FieldDescription(PropertyInfo type, List<ColumnDescription> columnDescr)
		{
			FieldType = type.PropertyType;
			_fieldName = type.Name;
			_columnDescr = columnDescr;
		}
		public FieldDescription(string type, string name, List<ColumnDescription> columnDescr=null)
		{
			FieldType = Utils.ParseType(type);
			_fieldName = name;
			_columnDescr = columnDescr;
		}

		public virtual void AppendTemplate(StringBuilder stringBuilder)
		{

            stringBuilder.AppendLine($"<kf-{ControlType} text = '{Caption}' v-model = 'Item.{FieldName}' {ItemsSource} ></kf-{ControlType}>");
			//switch (name)
			//{
			//	case "String":
			//		stringBuilder.AppendLine($"<kf-input text = '{Caption}' v-model = 'Item.{FieldName}' ></kf-input>");
			//		break;
			//	case "Int32":
			//		stringBuilder.AppendLine($"<kf-number text = '{Caption}' v-model = 'Item.{FieldName}' ></kf-number>");
			//		break;
			//	case "Decimal":
			//		stringBuilder.AppendLine($"<kf-number text = '{Caption}' v-model = 'Item.{FieldName}' ></kf-number>");
			//		break;
			//	case "DateTime":
			//		stringBuilder.AppendLine($"<kf-date text = '{Caption}' v-model = 'Item.{FieldName}' ></kf-date>");
			//		break;
			//	case "Boolean":
			//		stringBuilder.AppendLine($"<kf-check text = '{Caption}' v-model = 'Item.{FieldName}' ></kf-check>");
			//		break;
			//	default: throw new NotImplementedException($"type {name} not implemented");
			//}
		}
	}

	public enum ControlType
	{
		check,
		input,
		number,
		select,
		date
	}

}