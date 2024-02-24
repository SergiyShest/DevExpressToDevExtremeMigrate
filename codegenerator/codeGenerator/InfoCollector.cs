
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

        public List<Info> InfoList = new List<Info>();  
        public InfoCollector(string sourcePath = $"e:\\source\\repos\\lims_portal2\\Sasha\\Sasha.Lims.WebUI\\")
        {
            SourcePath = sourcePath;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var dirs = Utils.FindDirectoriesByTemplate(sourcePath, "*grid*.cshtml").ToList().Where(x=>x.ToLower().Contains("journal")&& !x.ToLower().Contains("obj"));

            foreach (var dir in dirs)
            {
                try
                {
                    InfoList.Add(CollectInfo(dir));
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                }
            }
        }

        public Info CollectInfo(string dir)
        {
            var files = Directory.GetFiles(dir, "*cshtml").ToList();
            var indexFileName = FindIndex(files);

            string contentIndex = File.ReadAllText(indexFileName, Encoding.GetEncoding(1251));
            var gridFileName = FindGrid(contentIndex,files); 
            string contentGrid = File.ReadAllText(gridFileName, Encoding.GetEncoding(1251));
            var info = new Info(){ 
                Path = dir 
            };

            info.MainHeader = Utils.ExtractFirstTextFromHtml(contentIndex)
                ;
            info.EntityFullName = Utils.ExtractEntity(contentIndex);

        
          
            if (string.IsNullOrEmpty(info.EntityFullName)) //try find entity in grid
            { 
                info.EntityFullName = Utils.ExtractEntity(contentGrid);
            }
             info.EntityDateField = Utils.ExtractEntityDate(SourcePath, info.EntityName);

             info.Columns = Utils.ExtractColumns(contentGrid);
             info.Fields =  Utils.ExtractFields(SourcePath, info.EntityName,info.Columns);
            return info;
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
        
        private static string FindGrid(string indexContent ,List<string> files)
        {
            var gridFileName = Utils.ExtractGridFileName(indexContent).ToLower();
            if (string.IsNullOrEmpty(gridFileName)) return null;
            var grid = files.FirstOrDefault(x => x.ToLower().EndsWith(gridFileName+".cshtml"));
           
            //if (grid == null)
            //{
            //    throw new FileNotFoundException("Grid file not found.", string.Join(", ", files));
            //}
            return grid;
        }

       
    }


}
