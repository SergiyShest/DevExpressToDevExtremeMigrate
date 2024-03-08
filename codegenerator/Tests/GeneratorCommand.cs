using CodeGenerator;

using Newtonsoft.Json;
using NLog;

using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using NUnit.Framework.Internal;
using System.Text;

namespace Tests
{

	public class GeneratorCommand
	{
		NLog.ILogger log = LogManager.GetCurrentClassLogger();

		#region tests

        [TestCase("..\\..\\..\\..\\..\\Source\\UI")]
        public void CollectInfo(string dir)
        {

            var infoCollector = new InfoCollector(dir, new Generator());

            var data = infoCollector.InfoList.Select(info => new { info.Path, info.EntityName, info.MainHeader, info.CardHeader, info.CardEntityName, info.AlwaysSkip });
            String json = JsonConvert.SerializeObject(data);
            File.WriteAllText("C:\\tmp\\collector.json", json);

        }

        [Test]
		public void GenerateAll()
		{
			var cg = new Generator();
			cg.Convert();
		}


        [TestCase("..\\..\\..\\..\\..\\Source\\UI\\Areas\\Common\\Views\\DoctorJournal")]
        [TestCase("..\\..\\..\\..\\..\\Source\\UI\\Areas\\Common\\Views\\HospitalJournal")]
		public void GeneratePart(string dir)
		{
            dir = Path.GetFullPath( dir);
            var generator = new Generator();
			var info = CollectDataFromDir(dir);
			generator.GeneratePathsForInfo(info);
			generator.GenerateJournalController(info);
			generator.GenerateJournalView(info);
			generator.GenerateCardView(info);
			generator.GenerateCardController(info);
			generator.GenerateCardView(info);
			generator.GenerateMenu(info);

           generator.GenerateJournalTests(new List<Info> { info });
		}


        #endregion

        #region service
        private static Info CollectDataFromDir(string dir)
		{

			var assertCollector = new AssertCollector();

			var infoCollector = new InfoCollector("not exists dir",new Generator());

			infoCollector.SourcePath = Path.GetFullPath( "..\\..\\..\\..\\..\\Source\\UI");
			var info = infoCollector.CollectInfo(dir);
			assertCollector.Assert(!string.IsNullOrEmpty(info.Path), "Path should not be empty or null.");
			assertCollector.Assert(!string.IsNullOrEmpty(info.EntityFullName), "EntityName should not be empty or null.");
			assertCollector.Assert(info.Columns.Count > 0, "Columns should contain at least one element.");

			assertCollector.ReportFailures();
			return info;
		}

		#endregion
	}
}

