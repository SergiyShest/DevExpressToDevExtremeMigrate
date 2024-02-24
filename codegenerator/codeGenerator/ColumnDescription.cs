using System.Text.RegularExpressions;
using System.Text;
using System;
using System.IO;
using System.Linq;
namespace CodeGenerator
{
	public class ColumnDescription
    {
    
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
                    _caption = FieldName;
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

        #region Width
        private string? _width;
        public string? Width
        {
            get
            {
                if (_width == null)
                {
                    _width = "";
                }
                return _width;
            }
            set
            {
                if (value == null) return;
                var match = Regex.Match(value, @"^\s*Unit\.Pixel\((\d+)\)\s*$");
                if (match.Success)
                {
                    _width = match.Groups[1].Value + "px";
                }
                else
                {
                    match = Regex.Match(value, @"^\s*(\d+)\s*$");
                    if (match.Success)
                    {
                        _width = match.Groups[1].Value + "px";
                    }
                    else
                    {
                        match = Regex.Match(value, @"^\s*Unit.Percentage\((\d+)\)\s*$");
                        if (match.Success)
                        {
                            _width = match.Groups[1].Value + "%";
                        }
                        else
                        {
                            throw new Exception(value);
                        }


                        _width = value;
                    }
                }
            }
        }
        #endregion

        #region ColumnType
        private string? _columnType;
        public string? ColumnType
        {
            get
            {
                if (_columnType == null)
                {
                    _columnType = "text";
                }
                return _columnType;
            }

            set
            {
                if (value == null) return;
                var match = Regex.Match(value, @"SpinEdit|ComboBox");
                  if (match.Success)
                {
                    _columnType = "number";
                }
                else
                {
                    match = Regex.Match(value, @"CheckBox");
                    if (match.Success)
                    {
                        _columnType = "boolean";
                    }
                    else
                    {
                        match = Regex.Match(value, @"Memo|TextBox");
                        if (match.Success)
                        {
                            _columnType = "string";
                        }
                        else
                        {

                            match = Regex.Match(value, @"DateEdit|TimeEdit");
                            if (match.Success)
                            {
                                _columnType = "date";
                            }
                            else
                            {
                                throw new Exception(value);
                            }

                        }

                    }

                }
            }
        }
        #endregion

        #region NumberType
        private string? _numberType;
        public string? NumberType
        {
            get
            {
                if (_numberType == null)
                {
                    _numberType = "";
                }
                return _numberType;
            }
            set
            {
                //throw new Exception(value);
                _numberType = value;
            }
        }
        #endregion

        #region SortOrder
        private string? _sortOrder;
        public string? SortOrder
        {
            get
            {
                if (_sortOrder == null)
                {
                    _sortOrder = "";
                }
                return _sortOrder;
            }
            set { _sortOrder = value; }
        }
        #endregion

        #region AllowSort
        private string? _allowSort;
        public string? AllowSort
        {
            get
            {
                if (_allowSort == null)
                {
                    _allowSort = "";
                }
                return _allowSort;
            }
            set { _allowSort = value; }
        }
        #endregion

        #region Visible
        private string? _visible;
        public string? Visible
        {
            get
            {
                if (_visible == null)
                {
                    _visible = "";
                }
                return _visible;
            }
            set
            {
                if (value == "DefaultBoolean.False")
                { 
                  _visible = "false";
                }else
                if (value == "DefaultBoolean.True")
                {
                    _visible = "true";
                }else
                _visible = value;
            }
        }
        #endregion


        public ColumnDescription(string content)
        {
            FieldName = AssignFieldValue(content, "FieldName");
            Caption = AssignFieldValue(content, "Caption");
            Width = AssignFieldValue(content, "Width");
            ColumnType = AssignFieldValue(content, "ColumnType");
            NumberType = AssignFieldValue(content, "NumberType");
            SortOrder = AssignFieldValue(content, "SortOrder");
            Visible = AssignFieldValue(content, "Visible");
        }

        private string? AssignFieldValue(string content, string fieldName)
        {
            string field = null;
            var match = Regex.Match(content, $@"{fieldName}\s*=\s*(.*?);", RegexOptions.Multiline);
            if (match.Success)
            {
                field = match.Groups[1].Value;
            }
            return field;
        }

        public void AppendTemplate(StringBuilder stringBuilder)
        {

            if (stringBuilder.Length > 0) { stringBuilder.AppendLine(","); }
            stringBuilder.AppendLine($"{{caption:'{Caption}',");
            stringBuilder.AppendLine($"dataField:'{FieldName}',");
            stringBuilder.AppendLine($"dataType:'{ColumnType}',");
            if (!string.IsNullOrWhiteSpace(Width))
            {
                stringBuilder.AppendLine($" width:'{Width}',");
            }
            if (!string.IsNullOrWhiteSpace(Visible))
            {
                stringBuilder.AppendLine($"visible:{Visible},");
            }

            stringBuilder.AppendLine($"}}");
        }

    }

}