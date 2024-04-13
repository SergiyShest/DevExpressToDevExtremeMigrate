using CodeGenerator;

using Newtonsoft.Json;
using NLog;

using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using NUnit.Framework.Internal;
using System.Text;
using static CodeGenerator.Generator;

namespace Tests
{

	public class GeneratorCommand
	{

        /// <summary>
        /// сбор информации
        /// </summary>
        /// <param name="dir">Путь к источнику</param>
        [TestCase("..\\..\\..\\..\\..\\Source\\UI")]
        public void CollectInfo(string dir)
        {

            var infoCollector = new InfoCollector(dir);
            infoCollector.CollectInfoFromSourcePath();
            var data = infoCollector.InfoList.Select(info => new { info.Path, info.EntityFullName, info.MainHeader, info.CardHeader, info.CardEntityName, info.AlwaysSkip });
            String json = JsonConvert.SerializeObject(data);
            File.WriteAllText("C:\\tmp\\collector.json", json);
        }

        [Test]
		public void GenerateAll()
		{
			var generator = new Generator();
			generator.LoadInfo();
            generator.CreateJournalControllers = true;
            generator.CreateCrdControllers  = false;
            generator.CreateJournalViews  = true;
            generator.CreateMenu  = false;
            generator.CreateCard  = false;
            generator.CreateTests  = false;
            generator.SaveMode  = FileSaveMode.Replace;
            generator.Generate();
		}

        [TestCase("..\\..\\..\\..\\..\\Source\\UI\\Areas\\Reagents\\Views\\ReagentTypeJournal")]
     // [TestCase("..\\..\\..\\..\\..\\Source\\UI\\Areas\\Common\\Views\\DoctorJournal")]
     // [TestCase("..\\..\\..\\..\\..\\Source\\UI\\Areas\\Common\\Views\\HospitalJournal")]
		public void GeneratePart(string dir)
		{
            dir = Path.GetFullPath( dir);
            var generator = new Generator();
            var infoCollector = new  InfoCollector(dir);
            infoCollector.CollectInfoFromSourcePath();
			if (infoCollector.InfoList.Count != 1) { throw new ApplicationException($" In this test expected 1 info, bat wos {infoCollector.InfoList.Count}"); }
			var info = infoCollector.InfoList[0];	
			generator.GenerateJournalController(info);
			generator.GenerateJournalView(info);
			generator.GenerateCardView(info);
			generator.GenerateCardController(info);
			generator.GenerateCardView(info);
			generator.GenerateMenu(info);
            generator.GenerateJournalTests( info);
		}



	}
}

