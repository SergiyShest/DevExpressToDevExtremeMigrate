

using System.Text;
using System.Text.Json.Serialization;

namespace CodeGenerator
{
    public class Info
    {

        public bool? AlwaysSkip { get; set; } = false;

        public string Name { get; internal set; }

        public string Area { get; internal set; }

        public string Path { get; set; }

        string _entityName;

        public string EntityName
        {
            get
            {
                if (_entityName == null)
                {
                    if (string.IsNullOrWhiteSpace(EntityFullName)) return null;
                    var parts = EntityFullName?.Split('.');
                    if (parts.Length > 0)
                        _entityName = parts[parts.Length - 1];
                    else _entityName = EntityFullName;
                }
                return _entityName;
            }
            set { _entityName = value; }

        }

        string _cardEntityName;
        public string CardEntityName
        {
            get
            {
                if (_cardEntityName == null)
                {
                    return EntityName;
                }
                return _cardEntityName;
            }
            set { _cardEntityName = value; }

        }


        public string EntityPath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(EntityFullName)) return null;
                var parts = EntityFullName.Split('.').ToList();
                parts.Remove(EntityName);
                return string.Join(".", parts);
            }

        }

        public string EntityFullName { get; set; }

        public string MainHeader { get; set; }

        public string CardHeader { get; set; }

        #region IsGuide при генерации формы журнала будет использован шаблон без дополнительных кнопок для редактирования.
        bool? _isGuide;
        public bool? IsGuide
        {
            get
            {
                if (_isGuide == null)
                {
                    if (MainHeader != null && MainHeader.ToLower().Contains("справочник")) _isGuide = true;

                }
                return _isGuide;
            }
            set
            {
                _isGuide = value;
            }
        }
        #endregion

        public string DateHeader { get; set; } = "Дата";

        public List<ColumnDescription> Columns { get; internal set; } = new List<ColumnDescription>();

        [JsonIgnore]
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
       
        /// <summary>
        /// список полей в  форме редактирования
        /// </summary>
        public List<FieldDescription> Fields { get; internal set; }

        public string FieldsStr
        {
            get
            {
                var fields = new StringBuilder();
                if (Fields != null)
                    foreach (var field in Fields)
                    {
                        field.AppendTemplate(fields);
                    }
                return fields.ToString();
            }
        }

        public string ControllerPath { get; set; }

        public string? JournalControllerName { get; set; }

        public string CardControllerName
        {
            get
            {
                if (JournalControllerName == null) return "ErrorController";
                if (JournalControllerName.Contains("Journal"))
                {
                    return JournalControllerName.Replace("Journal", "Card");
                }
                return JournalControllerName.Replace("Controller", "CardController"); ;
            }
        }

        public string JournalViewPath { get; set; }

        public string CardViewPath
        {
            get
            {
                if (JournalViewPath == null) return "ErrorCard";
                if (JournalViewPath.Contains("Journal"))
                {
                    return JournalViewPath.Replace("Journal", "Card");
                }
                return JournalViewPath + "Card";
            }
        }
        /// <summary>
        /// часть ссылки указывающей из формы журнала на форму карточки
        /// </summary>
        public string DataPath { get; set; }

        public string HtmlRequestPath { get { return string.IsNullOrEmpty(this.Area) ? Name : this.Area + "/" + Name; } }

        public string NameSpace { get; set; }

        public string? EntityDateField { get; internal set; }

        public override string ToString()
        {
            return MainHeader;
        }
    }


}
