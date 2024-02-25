using CodeGenerator;

using Newtonsoft.Json;
using NLog;

using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using NUnit.Framework.Internal;
using System.Text;

namespace Tests
{

	public class GeneratorTest
	{
		NLog.ILogger log = LogManager.GetCurrentClassLogger();

		#region tests


		//проверка генерации колонок ( нужно доработать сортировку и прочее) 
		[TestCaseSource(nameof(GetTestColumnsData))]
		public void TestColumnDescription(string input, string expected)
		{

			var cd = new ColumnDescription(input);
			var strBuilder = new StringBuilder();
			cd.AppendTemplate(strBuilder);

			var res = strBuilder.ToString();
			var resComp = StringComparer.CompareStringsIgnoringCharacters(expected, res);
			Assert.That(resComp.place == -1,resComp.dif);

		}



		[TestCase("Id", "<kf-number text = 'Код договора'    v-model = 'Item.Id' >    </kf-number>")]
		[TestCase("Name", "<kf-input  text = 'Name'    v-model = 'Item.Name' >    </kf-input>")]
		[TestCase("DocNum", "<kf-input  text =  'Номер документа'    v-model = 'Item.DocNum' >    </kf-input>")]
		[TestCase("DocDate", "<kf-date   text = 'DocDate' v-model = 'Item.DocDate' > </kf-date>")]
		[TestCase("Summ", "<kf-number text = 'Сумма'    v-model = 'Item.Summ' >    </kf-number>")]
		[TestCase("IsBool", "<kf-check    text = 'IsBool'  v-model = 'Item.IsBool' >    </kf-check>")]
		[TestCase("CurId", "<kf-select    text = 'Валюта'  v-model = 'Item.CurId' :items='CurrList'>    </kf-select>")]

		public void TestFieldDescription(string type, string expected)
		{

			var pi = typeof(TestType).GetProperty(type);

			var cd = new FieldDescription(pi, ColumnDescr());
			var strBuilder = new StringBuilder();
			cd.AppendTemplate(strBuilder);

			var res = strBuilder.ToString();
			var resComp = StringComparer.CompareStringsIgnoringCharacters(expected, res);
			Assert.That(resComp.place == -1, resComp.dif);

			static List<ColumnDescription> ColumnDescr()
			{
				var columnDescriptions = new List<ColumnDescription>();
				var cols = GetTestColumnsData();
				foreach (TestCaseData testData in cols)
				{
					columnDescriptions.Add(new ColumnDescription(testData.Arguments[0].ToString()));
				}
				return columnDescriptions;
			}

		}
        #endregion

        #region MyRegion

        [Test]
		public void TestCodeGenerator()
		{
			var cg = new Generator();
			cg.Convert();
			//  Assert.That(File.Exists(cg.ResultFilePath));
		}


		// [TestCaseSource(nameof(GetDirectories))]
		[TestCase("e:\\source\\repos\\lims_portal2\\Sasha\\Sasha.Lims.WebUI\\Areas\\Common\\Views\\PatientJournal")]
		public void TestInfoCollector(string dir)
		{
			CollectDataFromDir(dir);
		}

		[TestCase("e:\\source\\repos\\lims_portal2\\Sasha\\Sasha.Lims.WebUI", "vPatient", "dob")]

		public void TestEntityData(string dir, string entityName, string expected)
		{
			var dateField = Utils.ExtractEntityDate(dir, entityName);
			Assert.That(expected == dateField);
		}


		 [TestCase("e:\\source\\repos\\lims_portal2\\Sasha\\Sasha.Lims.WebUI")]
		public void TestInfoCollectorTestAll(string dir)
		{  AssertCollector assertCollector = new AssertCollector();
			var infoCollector = new InfoCollector(dir);
			var data = infoCollector.InfoList.Select(info => new { name = info.Path ,info.EntityName,info.MainHeader,info.CardEntityName,info.AlwaysSkip}); ;

            var serialized= JsonConvert.SerializeObject(data);
			File.WriteAllText("C:tmp/collect.json", serialized );

			foreach (var info in infoCollector.InfoList)
			{
				if (!string.IsNullOrEmpty(info.Path) &&
				 !string.IsNullOrEmpty(info.MainHeader) &&
				 !string.IsNullOrEmpty(info.EntityFullName) &&
				 info.Columns.Count > 0)
				{
					log.Debug(info.Path);
				}
				assertCollector.Assert(!string.IsNullOrEmpty(info.MainHeader), "MainHeader :" + info.Path);
				assertCollector.Assert(!string.IsNullOrEmpty(info.EntityName), "EntityName :" +  info.Path);
				assertCollector.Assert(info.Columns.Count > 0, "Columns.Count > 0 :;" +  info.Path);
			}
			assertCollector.ReportFailures();
		}


		[TestCase("e:\\source\\repos\\lims_portal2\\Sasha\\Sasha.Lims.WebUI\\Areas\\Common\\Views\\DoctorJournal",
				 "c:\\Users\\Saergey\\source\\repos\\Core\\MVC\\Areas\\Common\\Controllers\\DoctorJournalController.cs")]
		[TestCase("e:\\source\\repos\\lims_portal2\\Sasha\\Sasha.Lims.WebUI\\Areas\\Common\\Views\\PatientJournal",
				 "c:\\Users\\Saergey\\source\\repos\\Core\\MVC\\Areas\\Common\\Controllers\\PatientJournalController.cs")]
		[TestCase("e:\\source\\repos\\lims_portal2\\Sasha\\Sasha.Lims.WebUI\\Views\\SamplesBook",
				  "c:\\Users\\Saergey\\source\\repos\\Core\\MVC\\Controllers\\SamplesBookController.cs")]
		public void GenerateJournalControllerTests(string dir, string expected)
		{

			var generator = new Generator();
			var info = CollectDataFromDir(dir);
			generator.GeneratePathsForInfo(info);
			generator.GenerateJournalController(info);
			Assert.That(File.Exists(expected));
		}

		[TestCase("e:\\source\\repos\\lims_portal2\\Sasha\\Sasha.Lims.WebUI\\Areas\\Common\\Views\\DoctorJournal",
				 "c:\\Users\\Saergey\\source\\repos\\Core\\MVC\\Areas\\Common\\Controllers\\DoctorCardController.cs")]
		[TestCase("e:\\source\\repos\\lims_portal2\\Sasha\\Sasha.Lims.WebUI\\Areas\\Common\\Views\\PatientJournal",
		        "c:\\Users\\Saergey\\source\\repos\\Core\\MVC\\Areas\\Common\\Controllers\\PatientJournalController.cs")]
		[TestCase("e:\\source\\repos\\lims_portal2\\Sasha\\Sasha.Lims.WebUI\\Views\\SamplesBook",
                  "c:\\Users\\Saergey\\source\\repos\\Core\\MVC\\Controllers\\SamplesBookCardController.cs")]
		public void GenerateCardControllerTests(string dir, string expected)
		{

			var generator = new Generator();
			var info = CollectDataFromDir(dir);
			generator.GeneratePathsForInfo(info);
			generator.GenerateCardController(info);
			Assert.That(File.Exists(expected));
		}
        [TestCase("..\\..\\..\\..\\..\\Source\\UI\\Areas\\Common\\Views\\DoctorJournal",
                 "..\\..\\..\\..\\..\\Target\\UI\\Areas\\Common\\Views\\DoctorCard\\Index.cshtml")]
        //[TestCase("e:\\source\\repos\\lims_portal2\\Sasha\\Sasha.Lims.WebUI\\Areas\\Common\\Views\\PatientJournal",
        //         "c:\\Users\\Saergey\\source\\repos\\Core\\MVC\\Areas\\Common\\Views\\Index.cshtml")]
        //[TestCase("e:\\source\\repos\\lims_portal2\\Sasha\\Sasha.Lims.WebUI\\Views\\SamplesBook",
		      //   "c:\\Users\\Saergey\\source\\repos\\Core\\MVC\\Views\\SamplesBookCard\\Index.cshtml")]
		public void GenerateCardViewTests(string dir, string expected)
		{
            expected =   Path.GetFullPath(expected);
            dir = Path.GetFullPath( dir);
            var generator = new Generator();
			var info = CollectDataFromDir(dir);
			generator.GeneratePathsForInfo(info);

            generator.GenerateMenu(info);
            generator.GenerateJournalController(info);
            generator.GenerateJournalView(info);
            generator.GenerateCardView(info);
            generator.GenerateCardController(info);
			generator.GenerateCardView(info);
			Assert.That(File.Exists(expected));
		}

		[TestCase("e:\\source\\repos\\lims_portal2\\Sasha\\Sasha.Lims.WebUI\\Areas\\Common\\Views\\DoctorJournal",
				 "c:\\Users\\Saergey\\source\\repos\\Core\\MVC\\Areas\\Common\\Views\\DoctorJournal\\Index.cshtml")]
		[TestCase("e:\\source\\repos\\lims_portal2\\Sasha\\Sasha.Lims.WebUI\\Areas\\Common\\Views\\PatientJournal",
				 "c:\\Users\\Saergey\\source\\repos\\Core\\MVC\\Areas\\Common\\Views\\PatientJournal\\Index.cshtml")]
		[TestCase("e:\\source\\repos\\lims_portal2\\Sasha\\Sasha.Lims.WebUI\\Views\\SamplesBook",
				  "c:\\Users\\Saergey\\source\\repos\\Core\\MVC\\Views\\SamplesBook\\Index.cshtml")]
		public void CenerateJournalViewTests(string dir, string expected)
		{

			var generator = new Generator();
			var info = CollectDataFromDir(dir);
			generator.GeneratePathsForInfo(info);
			generator.GenerateJournalView(info);
			Assert.That(File.Exists(expected));
		}

		[TestCase("e:\\source\\repos\\lims_portal2\\Sasha\\Sasha.Lims.WebUI\\Areas\\Common\\Views\\DoctorJournal")]
		[TestCase("e:\\source\\repos\\lims_portal2\\Sasha\\Sasha.Lims.WebUI\\Areas\\Common\\Views\\PatientJournal")]
		[TestCase("e:\\source\\repos\\lims_portal2\\Sasha\\Sasha.Lims.WebUI\\Views\\SamplesBook")]
		public void CenerateSypressJournalTests(string dir)
		{

			var generator = new Generator();
			var info = CollectDataFromDir(dir);
			generator.GeneratePathsForInfo(info);

			generator.GenerateJournalTests(new[] { info });

		}

		[TestCase("e:\\source\\repos\\lims_portal2\\Sasha\\Sasha.Lims.WebUI\\Areas\\Common\\Views\\DoctorJournal")]
		[TestCase("e:\\source\\repos\\lims_portal2\\Sasha\\Sasha.Lims.WebUI\\Areas\\Common\\Views\\PatientJournal")]
		[TestCase("e:\\source\\repos\\lims_portal2\\Sasha\\Sasha.Lims.WebUI\\Views\\SamplesBook")]
		public void CenerateMenuTests(string dir)
		{
			var generator = new Generator();
			var info = CollectDataFromDir(dir);
			generator.GeneratePathsForInfo(info);
			generator.GenerateMenu(info);

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

		public static IEnumerable<TestCaseData> GetDirectories()
		{
			string sourcePath = $"e:\\source\\repos\\lims_portal2\\Sasha\\Sasha.Lims.WebUI\\";

			var dirs = Utils.FindDirectoriesByTemplate(sourcePath, "*grid*.cshtml").Where(x => x.ToLower().Contains("journal"));
			foreach (var dir in dirs)
			{
				string name = Utils.ExtractLastTwoDirectories(dir) ?? "";
				yield return new TestCaseData(dir) { TestName = name };
			}
		}

		public static IEnumerable<TestCaseData> GetTestColumnsData()
		{
			return GetTestCasesFromFile("TestColumnData.json");
		}

		public static IEnumerable<TestCaseData> GetTestFieldData()
		{
			return GetTestCasesFromFile("TestFieldData.json");
		}

		static IEnumerable<TestCaseData> GetTestCasesFromFile(string fileName)
		{
			string jsonTestData = File.ReadAllText(fileName);
			var testDataList = JsonConvert.DeserializeObject<List<TestData>>(jsonTestData);
			foreach (var testData in testDataList)
			{
				yield return new TestCaseData(testData.Input, testData.Expected) { TestName = testData.Name };
			}
		}

		public class TestData
		{
			public string Input { get; set; }
			public string Expected { get; set; }
			public string Name { get; set; }
		}

		public class TestType
		{
			public int? Id { get; set; }

			public string? Name { get; set; }

			public DateTime? DocDate { get; set; }

			public String? DocNum { get; set; }

			public Decimal? Summ { get; set; }

			public int? CurId { get; set; }

			public bool? IsBool { get; set; }
		}


		#endregion
	}
}

