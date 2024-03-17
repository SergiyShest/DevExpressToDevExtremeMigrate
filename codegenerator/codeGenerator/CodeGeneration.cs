using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using NLog;
using System.Diagnostics;
using System.Text;

namespace CodeGenerator
{
    public class Generator
    {
        #region Properties
        ILogger log = LogManager.GetCurrentClassLogger();
        #region Pathces

        public string TestTarget { get; set; } = "..\\..\\..\\..\\..\\Target\\Tests2e2\\js_tests";

        public string TargetMenu { get; set; } = $"..\\..\\..\\..\\..\\Target\\UI\\Views\\Shared\\_Layout.cshtml";

        public string SourcePath { get; set; } = $"..\\..\\..\\..\\..\\Source\\UI";

        public string TargetPath { get; set; } = $"..\\..\\..\\..\\..\\Target\\UI";

        #endregion

        #region Switches
        public bool CreateJournalControllers { get; set; } = false;

        public bool CreateCrdControllers { get; set; } = false;

        public bool CreateJournalViews { get; set; } = true;

        public bool CreateMenu { get; set; } = false;

        public bool CreateCard { get; set; } = false;

        public bool CreateTests { get; set; } = false;

        public FileSaveMode SaveMode { get; set; } = FileSaveMode.Replace;
        #endregion
        public List<Info> InfoList { get; set; }

        public enum FileSaveMode
        {
            Replace,
            SaveWithNewName,
            SkipExisting
        }

        #endregion

        public Generator()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public void LoadInfo(string entityName = null, string fileName = "templates/collector.json")
        {
            if (File.Exists(fileName))
            {
                var infoCollector = new InfoCollector(SourcePath);
                var fileContent = File.ReadAllText(fileName);
                InfoList = JsonConvert.DeserializeObject<List<Info>>(fileContent);
                foreach (var info in InfoList)
                {
                    if (entityName == null || info.EntityName == entityName)
                        infoCollector.CollectInfo(info);
                }
            }
        }

        public void Generate()
        {
            if (InfoList == null)
            {
                throw new Exception("Нужно собрать информацию. Выполните сначала функцию LoadInfo");
            }

            foreach (var info in InfoList)
            {
                if (info.AlwaysSkip == true) continue;
                try
                {
                    InfoCollector.GeneratePathsForInfo(info, TargetPath);
                    if (CreateJournalControllers) GenerateJournalController(info);
                    if (CreateJournalViews) GenerateJournalView(info);
                    if (CreateCrdControllers) GenerateCardController(info);
                    if (CreateCard) GenerateCardView(info);
                    if (CreateMenu) GenerateMenu(info);
                    if (CreateTests) GenerateJournalTests(info);
                    log.Info("Success " + info.Name);

                }

                catch (Exception ex)
                {
                    Debug.WriteLine(info.Path + " " + ex.ToString());
                    log.Error(ex, info.Path);
                }
            }

        }

        public void GenerateJournalController(Info info)
        {
            string controllerContent;
            if (info.EntityDateField == null)
                controllerContent = File.ReadAllText("templates/targetJournalControllerWitoutFliter.Template", Encoding.GetEncoding(1251));
            else
            {
                controllerContent = File.ReadAllText("templates/targetJournalController.Template", Encoding.GetEncoding(1251));
            }

            if (info.EntityFullName != null)
            {
                controllerContent = controllerContent.Replace("using Entity;", "using Entity;\r\nusing " + info.EntityPath + ";");
                controllerContent = controllerContent.Replace("$entity$", info.EntityName);
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
                controllerContent = controllerContent.Replace("using MVC.Controllers;", "using MVC.Controllers;\r\nusing " + info.EntityPath + ";");
            }
            controllerContent = controllerContent.Replace("$entity$", info.EntityName);
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
            var dataPath = info.JournalControllerPath.Replace(info.JournalControllerName.Replace("Controller", ""), info.CardControllerName.Replace("Controller", ""));
            viewContent = viewContent.Replace("$cardControllerPath$", dataPath);
            viewContent = viewContent.Replace("$items$", info.FieldsStr);

            var viewFullName = Path.Combine(info.CardViewPath, "Index.cshtml");
            SaveFile(viewFullName, viewContent);
        }

        public void GenerateJournalView(Info info)
        {
            string viewContent;
            if (info.IsGuide == true)
            {
                viewContent = File.ReadAllText("templates/targetJournalGuid.Template", Encoding.GetEncoding(1251));
            }
            else
            {
                viewContent = File.ReadAllText("templates/targetJournal.Template", Encoding.GetEncoding(1251));
            }

            viewContent = viewContent.Replace("$MainHeader$", info.MainHeader ?? info.EntityName);
            viewContent = viewContent.Replace("$columns$", info.ColumnsStr);
            viewContent = viewContent.Replace("$DateHeader$", info.DateHeader);
            var cardPath = info.JournalControllerPath.Replace("Journal", "");
            viewContent = viewContent.Replace("$cardControllerPath$", "/" + cardPath + "Card");

            viewContent = viewContent.Replace("$HeaderReport$", "Отчет " + info.CardHeader ?? info.EntityName);

            viewContent = viewContent.Replace("$Header$", info.CardHeader ?? info.EntityName);

            viewContent = viewContent.Replace("$storageKey$", info.Path.Replace($"\\", ""));

            viewContent = viewContent.Replace("$journalControllerPath$", "/" + info.JournalControllerPath);

            var viewFullName = Path.Combine(info.JournalViewPath, "Index.cshtml");
            SaveFile(viewFullName, viewContent);

        }

        public void GenerateJournalTests(Info info)
        {   //сохранение js теста
            var testContent = File.ReadAllText("templates/CypressTestJournal.Template", Encoding.GetEncoding(1251));
            testContent = testContent.Replace("$HtmlRequestPath$", info.HtmlRequestPath);
            testContent = testContent.Replace("$MainHeader$", info.MainHeader);
            var testName = info.Name + ".tests.cy.js";
            var viewFullName = Path.Combine(TestTarget + "\\cypress\\e2e\\" + testName);
            SaveFile(viewFullName, testContent, true, Encoding.UTF8);
            
            //внесение изменений в package.json
            var jsonConfigPath = Path.Combine(TestTarget, "package.json");
            string jsonConfig = File.ReadAllText(jsonConfigPath);
            JObject packageJson = JObject.Parse(jsonConfig);
            packageJson["scripts"][$"run:e2e.{info.Name}"] = $"cypress run --spec \"cypress/e2e/{testName}\"";

            string updatedJson = JsonConvert.SerializeObject(packageJson, Formatting.Indented);
            SaveFile(jsonConfigPath, updatedJson, true);


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
                throw new Exception($"Create teg id={navMenuId}");
            }
            var tagItem = Utils.GetTagContentById(menuItemsTags, navItemId, "a");//ищу существующий пункт меню
            if (tagItem == null)
            {
                menuItemsTags = menuItemsTags + Environment.NewLine + newItem;//добавляю если нет 
            }
            else
            {
                menuItemsTags = Utils.ChangeTagContentById(menuItemsTags, navItemId, newItem, "a");//заменяю если есть
            }
            content = Utils.ChangeTagContentById(content, navMenuId, menuItemsTags);
            SaveFile(TargetMenu, content);
        }

        private void SaveFile(string filePath, string content, bool? replace = null, Encoding? encoding = null)
        {
            if (encoding == null) encoding = Encoding.GetEncoding(1251);

            string directoryPath = Path.GetDirectoryName(filePath);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
            string fileExtension = Path.GetExtension(filePath);


            if (replace == true)
            {
                if (File.Exists(filePath)) File.Delete(filePath);
            }
            else
            {
                switch (SaveMode)
                {
                    case FileSaveMode.Replace: if (File.Exists(filePath)) File.Delete(filePath); break;
                    case FileSaveMode.SaveWithNewName:
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

                        break;
                    case FileSaveMode.SkipExisting: break;//если файл существует то он будет перезаписан

                }
            }

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, content, encoding);
            }
        }

        public void SaveInfoToFile(string path)
        {
            var list = InfoList.Select(x => new { x.MainHeader, x.Name, x.Path, x.EntityName });
            StringBuilder sb = new StringBuilder();
            foreach (var item in list)
            {
                sb.AppendLine($"{item.MainHeader};{item.Name};{item.Path};{item.EntityName}");
            }
            File.WriteAllText(path, sb.ToString(), Encoding.GetEncoding(1251));

        }


    }



}
