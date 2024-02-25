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
		public void GenarateAll()
		{
			var cg = new Generator();
			cg.Convert();
			//  Assert.That(File.Exists(cg.ResultFilePath));
		}


        [TestCase("..\\..\\..\\..\\..\\Source\\UI\\Areas\\Common\\Views\\DoctorJournal")]

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

