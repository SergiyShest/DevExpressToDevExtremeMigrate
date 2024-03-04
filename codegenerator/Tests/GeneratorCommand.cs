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



        [Test]
		public void GenerateAll()
		{
			var cg = new Generator();
			cg.Convert();
			//  Assert.That(File.Exists(cg.ResultFilePath));
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
			var infoCollector = new InfoCollector("not exists dir");

			infoCollector.SourcePath = Path.GetFullPath( "..\\..\\..\\..\\..\\Source\\UI");
			var info = infoCollector.CollectInfo(dir);
			assertCollector.Assert(!string.IsNullOrEmpty(info.Path), "Path should not be empty or null.");
			//            assertCollector.Assert(!string.IsNullOrEmpty(info.MainHeader), "MainHeader should not be empty or null.");
			assertCollector.Assert(!string.IsNullOrEmpty(info.EntityFullName), "EntityName should not be empty or null.");
			assertCollector.Assert(info.Columns.Count > 0, "Columns should contain at least one element.");

			assertCollector.ReportFailures();
			return info;
		}

		#endregion
	}
}

