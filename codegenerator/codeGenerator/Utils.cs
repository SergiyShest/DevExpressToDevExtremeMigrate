using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;

namespace CodeGenerator
{
    public static  class Utils
    {

       public static string[] FindDirectoriesByTemplate(string rootDirectory,string template)
        {
            try
            {
                // Рекурсивно ищем все файлы с шаблоном 
                var matchingFiles = Directory.GetFiles(rootDirectory,template, SearchOption.AllDirectories);

                // Извлекаем директории из найденных файлов
                var matchingDirectories = matchingFiles.Select(Path.GetDirectoryName).Distinct().ToArray();

                return matchingDirectories;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при поиске директорий: {ex.Message}");
                return new string[0];
            }
        }

        /// <summary>
        /// используется для генерации имени
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
       public static string ExtractLastTwoDirectories(string fullPath)
        {
            try
            {
                // Получаем информацию о директории
                DirectoryInfo directoryInfo = new DirectoryInfo(fullPath);
                List<string> dirs = new List<string>();

                do
                {
                    
                    if ((directoryInfo.Name != "Views") &&  (directoryInfo.Name == "Areas") )
                    {dirs.Insert(0, directoryInfo.Name); }
                    

                   directoryInfo = directoryInfo.Parent;
                } while (directoryInfo.Name.ToLower().EndsWith("ui"));

                return Path.Combine(dirs.ToArray());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при извлечении директорий: {ex.Message}");
                return $"Ошибка при извлечении директорий: {ex.Message}";
            }
        }


        static Dictionary<string,string> specialСases = new Dictionary<string,string>();

        internal static string GetEntity(string dir)
        {
            var name = new DirectoryInfo(dir).Name;
            var key = specialСases.Keys.FirstOrDefault(x => x == name);
            if (key != null)
            {
                return specialСases[key];
            }
            return null;
        }

        public static string ExtractFirstTextFromHtml(string content,string tag="h1")
        {

            string startTag = "<"+tag+">";
            string endTag = "</"+tag+">";
            int startIndex = content.IndexOf(startTag,StringComparison.InvariantCultureIgnoreCase) + startTag.Length;
            int endIndex = content.IndexOf(endTag, startIndex,StringComparison.InvariantCultureIgnoreCase);
            if (startIndex != -1 && endIndex != -1)
            {
                string extractedText = content.Substring(startIndex, endIndex - startIndex);
                return extractedText;
            }
            else
            {
                return null;
            }
        }

        internal static string ExtractEntity(string content)
        {
            string searchPattern = @"@model\s+\w+<([\w\.]+)>";
            return ExtractValue(content, searchPattern);
        }

        internal static string ExtractEntityFromController(string content)
        {
            string searchPattern = @"@model\s+GenericCardController<([\w\.]+)>";
            return ExtractValue(content, searchPattern);
        }

        public static string? ExtractEntityDate(string dir,string entityName)
        {
            var directory = new DirectoryInfo(dir);
            var path= directory.Parent.FullName;
			var matchingFiles = Directory.GetFiles(path, entityName+".cs", SearchOption.AllDirectories); 
            if(matchingFiles.Length > 0)
            {
                foreach (var matchingFile in matchingFiles) 
                {
                    var content = File.ReadAllText(matchingFile);
                    var matches= Regex.Matches(content, "DateTime[\\s>]*(\\w+)\\s*\\{\\s*get;\\s*set;\\s*\\}");
                    var dateFields = new List<string>();
                    foreach(Match m in matches) {
                        dateFields.Add(m.Groups[1].Value);
                    }
                    if (dateFields.Count > 0)
                    {
                      return dateFields[0];//to do: get clever
                    }
                }
            }
            return null;
        }
        
        internal static string ExtractGridFileName(string content)
        {
            string searchPattern = @"@Html.Partial\(""(\w*grid\w*)"", Model\)";
            string res= ExtractValue(content, searchPattern);
            if(res == null) {
                 searchPattern = @"@Html.Action\(""(\w*grid\w*)""";
             res= ExtractValue(content, searchPattern); 
            }
            return res;
        }
        
        internal static string ExtractValue(string content, string searchPattern)
        {
            Regex regex = new Regex(searchPattern, RegexOptions.Multiline|RegexOptions.IgnoreCase);
            var match = regex.Match(content);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            return null;
        }

       internal   static List<ColumnDescription> ExtractColumns(string contentGrid)
        {
            var columnDescriptions = new List<ColumnDescription>();
            Regex regex = new Regex(@"\w+\.Columns\.Add\(\w+\s*=>\s*{([^\}]+)}\);", RegexOptions.Multiline);
            var cxx = regex.Matches(contentGrid);
            foreach (Match m in cxx)
            {
                columnDescriptions.Add(new ColumnDescription(m.Groups[1].Value));
            }
            return columnDescriptions;
        }

       internal static string ChangeTagContentById(string htmlContent, string tagId, string newContent,string tagName= "div")
        {
            // Используем регулярное выражение для поиска тега по id
            string pattern = $@"(<{tagName}[^>]*id=""{tagId}""[^>]*>)([^@]+?)(</{tagName}>)";
            Regex regex = new Regex(pattern, RegexOptions.Multiline|RegexOptions.IgnoreCase);

            // Заменяем содержимое тега
            string modifiedHtml = regex.Replace(htmlContent, match =>
            {
                return $"{match.Groups[1].Value}{Environment.NewLine}{newContent}{Environment.NewLine}{match.Groups[3].Value}";
            });

            return modifiedHtml;
        }

        internal static string GetTagContentById(string htmlContent, string tagId, string tagName = "div")
        {
            // Используем регулярное выражение для поиска тега по id
            string pattern = $@"<{tagName}[^>]*id=""{tagId}""[^>]*>([^@]+?)</{tagName}>";
            Regex regex = new Regex(pattern, RegexOptions.Multiline|RegexOptions.CultureInvariant);
            var match= regex.Match(htmlContent);
            if(match.Success)
            return match.Groups[1].Value;

            return null;
        }

		public static Type ParseType(string typeDescription)
		{
			switch (typeDescription.ToLower())
			{
				case "datetime":
					return typeof(DateTime);
				case "string":
					return typeof(string);
				case "int":
					return typeof(int);
				case "decimal":
					return typeof(decimal);
				// Добавьте другие кейсы при необходимости
				default:
					throw new ArgumentException($"Неизвестное описание типа: {typeDescription}");
			}
		}

		internal static List<FieldDescription> ExtractFields(string dir, string entityName, List < ColumnDescription > columnDescr,string entityPath)
		{
            var res =  new List<FieldDescription>();
			var directory = new DirectoryInfo(dir);

            while (directory.FullName.ToUpper().Contains("\\UI"))
            {
                directory = directory.Parent;
            }

            var matchingFiles = Directory.GetFiles(directory.FullName+ entityPath, entityName + ".cs", SearchOption.AllDirectories);
			if (matchingFiles.Length > 0)
			{
				foreach (var matchingFile in matchingFiles)
				{
					var content = File.ReadAllText(matchingFile);
					var matches = Regex.Matches(content, "public\\s+(\\w+)[\\s>]*(\\w+)\\s*\\{\\s*get;\\s*set;\\s*\\}");
					foreach (Match m in matches)
					{
						res.Add(new FieldDescription(m.Groups[1].Value,m.Groups[2].Value,columnDescr));
					}
				}
                return res;
			} 
            return res;
		}


	}
}