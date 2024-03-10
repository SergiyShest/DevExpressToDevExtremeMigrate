
using NLog;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace CodeGenerator
{
    public class InfoCollector
    {

        public string SourcePath { get; set; } = $"..\\..\\..\\..\\..\\Source\\UI";
        public string TargetPath { get; set; } = $"..\\..\\..\\..\\..\\Target\\UI";

        //Path to catalog where entity placed (можно оставить пустым, но с ним поиск нужного файла происходит много быстрее)
        public static string EntityPath { get; set; } = "\\DataAccessLayer\\Sasha.Lims.DataAccess.Entities";

        ILogger log = LogManager.GetCurrentClassLogger();

        public List<Info> InfoList { get; set; } = new List<Info>();

        public InfoCollector(string sourcePath)
        {
            SourcePath = Path.GetFullPath(sourcePath);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }


        public void CollectInfoFromSourcePath()
        {
            var dirs = Utils.FindDirectoriesByTemplate(SourcePath, "*grid*.cshtml").ToList();//.Where(x => x.ToLower().Contains("journal")

            foreach (var dir in dirs)
            {
                try
                {
                    var info = new Info() { Path = dir };
                    CollectInfo(info);
                    InfoList.Add(info);
                }
                catch (Exception ex)
                {
                    log.Error(dir + ex.Message);
                }
            }
        }

        public void CollectInfo(Info info)
        {
            var dir = info.Path;
            var files = Directory.GetFiles(dir, "*cshtml").ToList();
            var indexFileName = FindIndex(files);

            string indexFileContent = File.ReadAllText(indexFileName, Encoding.GetEncoding(1251));
            var gridFileName = FindGrid(indexFileContent, files);
            string contentGrid = File.ReadAllText(gridFileName, Encoding.GetEncoding(1251));

            if (string.IsNullOrEmpty(info.MainHeader)) info.MainHeader = Utils.ExtractFirstTextFromHtml(indexFileContent);


            if (string.IsNullOrEmpty(info.EntityFullName)) //try find entity in context
            {
                info.EntityFullName = Utils.ExtractEntity(indexFileContent);
            }
            if (string.IsNullOrEmpty(info.EntityFullName)) //try find entity in grid
            {
                info.EntityFullName = Utils.ExtractEntity(contentGrid);
            }

            GeneratePathsForInfo(info, TargetPath);
            if (string.IsNullOrEmpty(info.EntityFullName)) //try find entity in current Controller
            {
                var controllerName = Path.Combine(info.ControllerPath, info.CardControllerName + ".cs");
                if (File.Exists(controllerName))
                {
                    string contentController = File.ReadAllText(controllerName, Encoding.GetEncoding(1251));
                    info.EntityFullName = Utils.ExtractEntityFromController(contentController);
                }
            }

            info.Columns = Utils.ExtractColumns(contentGrid);
            if (!string.IsNullOrEmpty(info.EntityName))
            {
                info.EntityDateField = Utils.ExtractEntityDate(SourcePath, info.EntityName);
                info.Fields = Utils.ExtractFields(SourcePath, info.EntityName, info.Columns, EntityPath);
            }
            else
            {
                log.Info(info.Path + " not found EntityName");
            }


        }

        public static void GeneratePathsForInfo(Info info, string targetPath)
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
            info.JournalViewPath = Path.Combine(targetPath, newViewPath);

            info.JournalControllerName = dirs[dirs.Count - 1] + "Controller";
            var isInArea = false;
            if (dirs.Count > 2)
            {
                dirs.RemoveRange(dirs.Count - 2, 2);
                info.DataPath = dirs.Last() + "/" + info.Name;
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

            info.ControllerPath = Path.Combine(targetPath, newControllerPath);

        }

        private static string FindIndex(List<string> files)
        {
            var index = files.FirstOrDefault(x => x.ToLower().EndsWith("index.cshtml"));
            if (index == null)
            {
                index = files.FirstOrDefault(x => !x.ToLower().Contains("grid"));
            }
            if (index == null)
            {
                throw new FileNotFoundException("Index file not found.", string.Join(", ", files));
            }
            return index;
        }

        private static string FindGrid(string indexContent, List<string> files)
        {
            var gridFileName = Utils.ExtractGridFileName(indexContent)?.ToLower();
            if (string.IsNullOrEmpty(gridFileName)) return null;
            var grid = files.FirstOrDefault(x => x.ToLower().EndsWith(gridFileName + ".cshtml"));

            return grid;
        }

    }

}
