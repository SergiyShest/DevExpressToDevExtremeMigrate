
using NLog;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace CodeGenerator
{
   public class InfoCollector
    {



        public string SourcePath { get; set; }

        NLog.ILogger log = LogManager.GetCurrentClassLogger();
        Generator _generator;
        public List<Info> InfoList { get; set; } = new List<Info>();

        public InfoCollector(string sourcePath, Generator generator)
        {
            _generator = generator;
            SourcePath = Path.GetFullPath(sourcePath);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var dirs = Utils.FindDirectoriesByTemplate(SourcePath, "*grid*.cshtml").ToList();//.Where(x => x.ToLower().Contains("journal")

            foreach (var dir in dirs)
            {
                try
                {
                        InfoList.Add(CollectInfo(dir));
                }
                catch (Exception ex)
                {
                    log.Error(dir + ex.Message);
                }
            }
        }

        public Info CollectInfo(string dir)
        {
            var files = Directory.GetFiles(dir, "*cshtml").ToList();
            var indexFileName = FindIndex(files);

            string currentView = File.ReadAllText(indexFileName, Encoding.GetEncoding(1251));
            var gridFileName = FindGrid(currentView, files);
            string contentGrid = File.ReadAllText(gridFileName, Encoding.GetEncoding(1251));
            var info = new Info()
            {
                Path = dir
            };

            info.MainHeader = Utils.ExtractFirstTextFromHtml(currentView);

            info.EntityFullName = Utils.GetEntity(dir);
            if (string.IsNullOrEmpty(info.EntityFullName)) //try find entity in context
            {
                info.EntityFullName = Utils.ExtractEntity(currentView);
            }
            if (string.IsNullOrEmpty(info.EntityFullName)) //try find entity in grid
            {
                info.EntityFullName = Utils.ExtractEntity(contentGrid);
            }
            if (string.IsNullOrEmpty(info.EntityFullName)) //try find entity in current Controller
            {
                _generator.GeneratePathsForInfo(info);
                var controllerName = Path.Combine(info.ControllerPath, info.CardControllerName + ".cs");
                if (File.Exists(controllerName))
                {
                    string contentCurentController = File.ReadAllText(controllerName, Encoding.GetEncoding(1251));
                    info.EntityFullName = Utils.ExtractEntityFromController(contentCurentController);
                }
            }

            info.Columns = Utils.ExtractColumns(contentGrid);
            if (!string.IsNullOrEmpty(info.EntityName))
            {

                info.EntityDateField = Utils.ExtractEntityDate(SourcePath, info.EntityName);
                info.Fields = Utils.ExtractFields(SourcePath, info.EntityName, info.Columns);
            }
            else
            {
                log.Info(info.Path + " not found EntityName");
            }

            return info;
        }


        public static void Collect(Info info, string sourcePath)
        {
            var files = Directory.GetFiles(info.Path, "*cshtml").ToList();
            var indexFileName = FindIndex(files);

            string currentView = File.ReadAllText(indexFileName, Encoding.GetEncoding(1251));
            var gridFileName = FindGrid(currentView, files);
            string contentGrid = File.ReadAllText(gridFileName, Encoding.GetEncoding(1251));

            info.Columns = Utils.ExtractColumns(contentGrid);
            if (!string.IsNullOrEmpty(info.EntityName))
            {
                info.EntityDateField = Utils.ExtractEntityDate(sourcePath, info.EntityName);
                var entName = info.CardEntityName ?? info.EntityName;
                info.Fields = Utils.ExtractFields(sourcePath, entName, info.Columns);
            }
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
