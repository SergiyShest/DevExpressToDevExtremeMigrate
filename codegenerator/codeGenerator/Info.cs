using System.Text;

namespace CodeGenerator
{
	public class Info
	{
		public string Name { get; internal set; }

		public string Area { get; internal set; }

		public string Path { get; set; }

		public string EntityName
		{
			get
			{
				var parts = EntityFullName.Split('.');
				if (parts.Length > 0)
					return parts[parts.Length - 1];
				return EntityFullName;
			}

		}

		public string EntityPath
		{
			get
			{
				var parts = EntityFullName.Split('.').ToList();
				parts.Remove(EntityName);
				return string.Join(".", parts);
			}

		}

		public string EntityFullName { get; set; }

		public string MainHeader { get; set; }

		public string DateHeader { get; set; } = "Дата";

		public List<ColumnDescription> Columns { get; set; }

		public string ColumnsStr
		{
			get
			{
				var columns = new StringBuilder();
				foreach (var column in Columns)
				{
					column.AppendTemplate(columns);
				}
				return columns.ToString();
			}
		}

		public List<FieldDescription> Fields { get; set; }

		public string FieldsStr
		{
			get
			{
				var fields = new StringBuilder();
				foreach (var field in Fields)
				{
					field.AppendTemplate(fields);
				}
				return fields.ToString();
			}
		}

		public string ControllerPath { get; set; }

		public string JournalControllerName { get; set; }

		public string CardControllerName
		{
			get
			{
				if (JournalControllerName.Contains("Journal"))
				{
					return JournalControllerName.Replace("Journal", "Card");
				}
				return JournalControllerName.Replace("Controller", "CardController");;
			}
		}

		public string JournalViewPath { get; set; }

		public string CardViewPath
		{
			get
			{
				if (JournalViewPath.Contains("Journal"))
				{
					return JournalViewPath.Replace("Journal", "Card");
				}
				return  JournalViewPath+"Card";
			}
		}

		public string DataPath { get; set; }

		public string HtmlRequestPath { get { return string.IsNullOrEmpty(this.Area) ? Name : this.Area + "/" + Name; } }

		public string NameSpace { get; set; }

		public string? EntityDateField { get; internal set; }
	}


}
//public string ControllerPath { get; set; }

//public string ViewPath { get; set; }