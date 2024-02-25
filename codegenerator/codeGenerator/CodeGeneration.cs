using System.IO;
using System.Net;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Reflection.Emit;
using NLog;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace CodeGenerator
{
    public class Generator
    {
        ILogger log = LogManager.GetCurrentClassLogger();

        public string TestTarget { get; set; } = "..\\..\\..\\..\\..\\Target\\Tests2e2\\test_e2e";

        public string TargetMenu { get; set; } = $"..\\..\\..\\..\\..\\Target\\UI\\Views\\Shared\\_Layout.cshtml";

        public string SourcePath { get; set; } = $"..\\..\\..\\..\\..\\Source\\UI";

        public string TargetPath { get; set; } = $"..\\..\\..\\..\\..\\Target\\UI";

        public bool CreateControllers { get; set; } = false;

        public bool CreateCardControllers { get; set; } = false;
        public bool CreateViews { get; set; } = true;

        public bool CreateCardViews { get; set; } = true;

        public bool CreateTests { get; set; } = false;

        public bool CreateMenu { get; set; } = false;

        public void Convert()
        {
            var infoCollector = new InfoCollector(SourcePath);
            foreach (var info in infoCollector.InfoList)
            {
                if (info.AlwaysSkip == true) continue;
                try
                {
                    GeneratePathsForInfo(info);
                    if (CreateControllers) GenerateJournalController(info);
                    if (CreateViews) GenerateJournalView(info);
                    if (CreateCardControllers) GenerateCardController(info);
                    if (CreateCardViews) GenerateCardView(info);
                    if (CreateMenu) GenerateMenu(info);
                    log.Info("Succsess " + info.Name);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(info.Path + " " + ex.ToString());
                    log.Error(ex, info.Path);
                }
            }
            if (CreateTests) GenerateJournalTests(infoCollector.InfoList);

        }

        public void GeneratePathsForInfo(Info info)
        {

            var di = new DirectoryInfo(info.Path);
            List<string> dirs = new List<string>();
            while (!di.Name.ToLower().EndsWith("ui"))
            {
                dirs.Insert(0, di.Name);
                di = di.Parent;
            }
            info.Name = dirs[dirs.Count - 1];
            var newViewPath = Path.Combine(dirs.ToArray());
            info.JournalViewPath = Path.GetFullPath(Path.Combine(TargetPath, newViewPath));

            info.JournalControllerName = dirs[dirs.Count - 1] + "Controller";
            var isInArea = false;
            if (dirs.Count > 2)
            {
                dirs.RemoveRange(dirs.Count - 2, 2);
                info.DataPath = dirs.Last() + "/" + info.Name.Replace("Journal", "");
                isInArea = true;
            }
            else
            {
                dirs.RemoveAt(0);
                info.DataPath = dirs.Last();
            }


            string newControllerPath;
            if (isInArea)
            {
                info.Area = dirs.Last();
                dirs.Add("Controllers");
                newControllerPath = Path.Combine(dirs.ToArray());
                info.NameSpace = "UI." + string.Join(".", dirs.ToArray());
            }
            else
            {
                newControllerPath = "Controllers";
                info.NameSpace = "UI.Controllers";
            }

            info.ControllerPath = Path.GetFullPath(Path.Combine(TargetPath, newControllerPath));

        }

        public void GenerateJournalController(Info info)
        {
            var controllerContent = File.ReadAllText("templates/targetJournalController.Template", Encoding.GetEncoding(1251));
            if (info.EntityFullName != null)
            {
                controllerContent = controllerContent.Replace("using Entity;", "using Entity;\r\nusing " + info.EntityPath + ";");
                controllerContent = controllerContent.Replace("vAnswer2", info.EntityName);
            }
            controllerContent = controllerContent.Replace("DkJournalController", info.JournalControllerName);

            controllerContent = controllerContent.Replace("x.Cdate", "x." + info.EntityDateField);

            var area = string.Empty;
            if (info.Area != null) { area = "[Area(\"" + info.Area + "\")]"; }
            controllerContent = controllerContent.Replace("[Area(\"\")]", area);

            controllerContent = controllerContent.Replace("namespace tmp", "namespace " + info.NameSpace);

            var controllerName = Path.Combine(info.ControllerPath, info.JournalControllerName + ".cs");
            SaveFile(controllerName, controllerContent);

        }

        public void GenerateCardController(Info info)
        {
            var controllerContent = File.ReadAllText("templates/targetCardController.Template", Encoding.GetEncoding(1251));
            if (info.EntityFullName != null)
            {
                controllerContent = controllerContent.Replace("using Entity;", "using Entity;\r\nusing " + info.EntityPath + ";");
                controllerContent = controllerContent.Replace("vAnswer2", info.EntityName);
            }
            controllerContent = controllerContent.Replace("DkCardController", info.CardControllerName);

            var area = string.Empty;
            if (info.Area != null) { area = "[Area(\"" + info.Area + "\")]"; }
            controllerContent = controllerContent.Replace("[Area(\"\")]", area);
            controllerContent = controllerContent.Replace("namespace tmp", "namespace " + info.NameSpace);

            var controllerName = Path.Combine(info.ControllerPath, info.CardControllerName + ".cs");
            SaveFile(controllerName, controllerContent);
        }
        public void GenerateCardView(Info info)
        {
            var viewContent = File.ReadAllText("templates/targetCard.Template", Encoding.GetEncoding(1251));
            viewContent = viewContent.Replace("/Contract/Dk", "/" + info.DataPath);
            viewContent = viewContent.Replace("$items$", "/" + info.FieldsStr);

            var viewFullName = Path.Combine(info.CardViewPath, "Index.cshtml");
            SaveFile(viewFullName, viewContent);
        }
        public void GenerateJournalView(Info info)
        {
            var viewContent = File.ReadAllText("templates/targetJournal.Template", Encoding.GetEncoding(1251));
            viewContent = viewContent.Replace("$MainHeader$", info.MainHeader);
            viewContent = viewContent.Replace("$columns$", info.ColumnsStr);
            viewContent = viewContent.Replace("$DateHeader$", info.DateHeader);
            viewContent = viewContent.Replace("/Contract/Dk", "/" + info.DataPath);

            var viewFullName = Path.Combine(info.JournalViewPath, "Index.cshtml");
            SaveFile(viewFullName, viewContent);

        }

        public void GenerateJournalTests(IEnumerable<Info> infoList)
        {
            var testContent = File.ReadAllText("templates/SypressTestJournal.Template", Encoding.GetEncoding(1251));
            var jsonConfigPath = Path.Combine(TestTarget, "package.json");
            string jsonConfig = File.ReadAllText(jsonConfigPath);
            JObject packageJson = JObject.Parse(jsonConfig);

            foreach (var info in infoList)
            {
                testContent = testContent.Replace("/Common/DoctorJournal", "/" + info.HtmlRequestPath);
                testContent = testContent.Replace("$MainHeader$", info.MainHeader);
                var testName = info.Name + "_SypressTests.js";
                var viewFullName = Path.Combine(TestTarget + "\\tests\\e2e\\specs\\" + testName);
                SaveFile(viewFullName, testContent, true, Encoding.UTF8);
                packageJson["scripts"][$"run:e2e.{info.Name}"] = $"cypress run --spec 'tests/e2e/specs/{testName}'";

            }
            string updatedJson = JsonConvert.SerializeObject(packageJson, Formatting.Indented);
            File.WriteAllText(jsonConfigPath, updatedJson);


        }

        public void GenerateMenu(Info info)
        {
            string navMenuId = $"{info.Area?.ToLower()}-dropdown-menu";
            string navItemId = $"{info.Name.ToLower()}-dropdown-item";
            string newItem = $"<a id=\"{info.Name.ToLower()}-dropdown-item\"   class=\"dropdown-item\" asp-area=\"{info.Area}\" asp-controller=\"{info.Name}\" asp-action=\"Index\">{info.MainHeader ?? info.Name}</a>";
            var content = File.ReadAllText(TargetMenu, Encoding.GetEncoding(1251));
            var menuItemsTags = Utils.GetTagContentById(content, navMenuId);
            if (menuItemsTags == null)
            {
                throw new Exception($"сформируй в файле teg id={navMenuId}");
            }
            var tagItem = Utils.GetTagContentById(menuItemsTags, navItemId);//ищу существующий пункт меню
            if (tagItem == null)
            {
                menuItemsTags = menuItemsTags + Environment.NewLine + newItem;//добавляю если нет 
            }
            else
            {
                menuItemsTags = Utils.ChangeTagContentById(menuItemsTags, navItemId, newItem);//заменяю если есть
            }
            content = Utils.ChangeTagContentById(content, navMenuId, menuItemsTags);
            SaveFile(TargetMenu, content);
        }

        private void SaveFile(string filePath, string content, bool replace = true, Encoding? encoding = null)
        {
            if (encoding == null) encoding = Encoding.GetEncoding(1251);

            string directoryPath = Path.GetDirectoryName(filePath);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
            string fileExtension = Path.GetExtension(filePath);
            if (replace)
            {
                if (File.Exists(filePath)) File.Delete(filePath);
            }
            else
            {
                int i = 0;
                while (File.Exists(filePath))
                {
                    i++;
                    // Формируем новое имя файла с суффиксом числа
                    string newFileName = $"{fileNameWithoutExtension}_{i}{fileExtension}";
                    filePath = Path.Combine(directoryPath, newFileName);
                }
            }
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            File.WriteAllText(filePath, content, encoding);
        }

    }
}
