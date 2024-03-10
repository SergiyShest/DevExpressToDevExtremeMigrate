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
            Assert.That(resComp.place == -1, resComp.dif);

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

        #region tests 2


        // [TestCaseSource(nameof(GetDirectories))]
        [TestCase("..\\..\\..\\..\\..\\Source\\UI\\Areas\\Common\\Views\\PatientJournal")]
        public void TestInfoCollector(string dir)
        {
            CollectDataFromDir(dir);
        }

        [TestCase("..\\..\\..\\..\\..\\Source\\UI", "vPatient", "dob")]

        public void TestEntityData(string dir, string entityName, string expected)
        {
            var dateField = Utils.ExtractEntityDate(dir, entityName);
            Assert.That(expected == dateField);
        }


        [TestCase("..\\..\\..\\..\\..\\Source\\UI")]
        public void TestInfoCollectorTestAll(string dir)
        {
            AssertCollector assertCollector = new AssertCollector();
            var infoCollector = new InfoCollector(dir);

            var data = infoCollector.InfoList.Select(info => new { name = info.Path, info.EntityName, info.MainHeader, info.CardHeader, info.CardEntityName, info.AlwaysSkip });
            String json = JsonConvert.SerializeObject(data);
            File.WriteAllText("C:\\tmp\\collector.json", json);


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
                assertCollector.Assert(!string.IsNullOrEmpty(info.EntityName), "EntityName :" + info.Path);
                assertCollector.Assert(info.Columns.Count > 0, "Columns.Count > 0 :;" + info.Path);
            }
            assertCollector.ReportFailures();
        }


        [TestCase("..\\..\\..\\..\\..\\Source\\UI\\Areas\\Common\\Views\\DoctorJournal",
                 "..\\..\\..\\..\\..\\Target\\UI\\Areas\\Common\\Controllers\\DoctorJournalController.cs")]
        [TestCase("..\\..\\..\\..\\..\\Source\\UI\\Areas\\Common\\Views\\PatientJournal",
                 "..\\..\\..\\..\\..\\Target\\UI\\Areas\\Common\\Controllers\\PatientJournalController.cs")]
        [TestCase("..\\..\\..\\..\\..\\Source\\UI\\Views\\SamplesBook",
                  "..\\..\\..\\..\\..\\Target\\UI\\Controllers\\SamplesBookController.cs")]
        public void GenerateJournalControllerTests(string dir, string expected)
        {
            deleteExcpected(expected);
            var generator = new Generator();
            var info = CollectDataFromDir(dir);
            generator.GenerateJournalController(info);
            Assert.That(File.Exists(expected));
        }

        [TestCase("..\\..\\..\\..\\..\\Source\\UI\\Areas\\Common\\Views\\DoctorJournal",
                 "..\\..\\..\\..\\..\\Target\\UI\\Areas\\Common\\Controllers\\DoctorCardController.cs")]
        
        [TestCase("..\\..\\..\\..\\..\\Source\\UI\\Areas\\Common\\Views\\PatientJournal",
                  "..\\..\\..\\..\\..\\Target\\UI\\Areas\\Common\\Controllers\\PatientCardController.cs")]
        
        [TestCase("..\\..\\..\\..\\..\\Source\\UI\\Views\\SamplesBook",
                  "..\\..\\..\\..\\..\\Target\\UI\\Controllers\\SamplesBookCardController.cs")]
        public void GenerateCardControllerTests(string dir, string expected)
        {
            deleteExcpected(expected);
            var generator = new Generator();
            var info = CollectDataFromDir(dir);
            generator.GenerateCardController(info);
            Assert.That(File.Exists(expected));
        }

        [TestCase("..\\..\\..\\..\\..\\Source\\UI\\Areas\\Common\\Views\\DoctorJournal",
                  "..\\..\\..\\..\\..\\Target\\UI\\Areas\\Common\\Views\\DoctorCard\\Index.cshtml")]

        [TestCase("..\\..\\..\\..\\..\\Source\\UI\\Areas\\Common\\Views\\PatientJournal",
                  "..\\..\\..\\..\\..\\Target\\UI\\Areas\\Common\\Views\\PatientCard\\Index.cshtml")]

        [TestCase("..\\..\\..\\..\\..\\Source\\UI\\Views\\SamplesBook",
                  "..\\..\\..\\..\\..\\Target\\UI\\Views\\SamplesBookCard\\Index.cshtml")]
        public void GenerateCardViewTests(string dir, string expected)
        {
            deleteExcpected(expected);

            dir = Path.GetFullPath(dir);
            var generator = new Generator();
            var info = CollectDataFromDir(dir);

            generator.GenerateCardView(info);
            Assert.That(File.Exists(expected));
        }



        [TestCase("..\\..\\..\\..\\..\\Source\\UI\\Areas\\Common\\Views\\DoctorJournal",
                 "..\\..\\..\\..\\..\\Target\\UI\\Areas\\Common\\Views\\DoctorJournal\\Index.cshtml")]

        [TestCase("..\\..\\..\\..\\..\\Source\\UI\\Areas\\Common\\Views\\PatientJournal",
                 "..\\..\\..\\..\\..\\Target\\UI\\Areas\\Common\\Views\\PatientJournal\\Index.cshtml")]

        [TestCase("..\\..\\..\\..\\..\\Source\\UI\\Views\\SamplesBook",
                  "..\\..\\..\\..\\..\\Target\\UI\\Views\\SamplesBook\\Index.cshtml")]
        public void GenerateJournalViewTests(string dir, string expected)
        {
            deleteExcpected(expected);
            var generator = new Generator();
            var info = CollectDataFromDir(dir);
            generator.GenerateJournalView(info);
            Assert.That(File.Exists(expected));
        }

        [TestCase("..\\..\\..\\..\\..\\Source\\UI\\Areas\\Common\\Views\\DoctorJournal")]
        [TestCase("..\\..\\..\\..\\..\\Source\\UI\\Areas\\Common\\Views\\PatientJournal")]
        [TestCase("..\\..\\..\\..\\..\\Source\\UI\\Views\\SamplesBook")]
        public void GenerateCypressJournalTests(string dir)
        {

            var generator = new Generator();
            var info = CollectDataFromDir(dir);
            //			generator.GeneratePathsForInfo(info);

            generator.GenerateJournalTests(new[] { info });

        }

        [TestCase("..\\..\\..\\..\\..\\Source\\UI\\Areas\\Common\\Views\\DoctorJournal")]
        [TestCase("..\\..\\..\\..\\..\\Source\\UI\\Areas\\Common\\Views\\PatientJournal")]
        [TestCase("..\\..\\..\\..\\..\\Source\\UI\\Views\\SamplesBook")]
        public void GenerateMenuTests(string dir)
        {
            var generator = new Generator();
            var info = CollectDataFromDir(dir);
            //			generator.GeneratePathsForInfo(info);
            generator.GenerateMenu(info);

        }



        #endregion

        #region service
        private static Info CollectDataFromDir(string dir)
        {

            var assertCollector = new AssertCollector();
            var infoCollector = new InfoCollector(Path.GetFullPath("..\\..\\..\\..\\..\\Source\\UI"));

            var info = new Info() { Path = dir }; ;
           infoCollector.CollectInfo(info);
            assertCollector.Assert(!string.IsNullOrEmpty(info.Path), "Path should not be empty or null.");
            //            assertCollector.Assert(!string.IsNullOrEmpty(info.MainHeader), "MainHeader should not be empty or null.");
            assertCollector.Assert(!string.IsNullOrEmpty(info.EntityFullName), "EntityName should not be empty or null.");
            assertCollector.Assert(info.Columns.Count > 0, "Columns should contain at least one element.");

            assertCollector.ReportFailures();
            return info;
        }

        public static IEnumerable<TestCaseData> GetDirectories()
        {
            string sourcePath = $"..\\..\\..\\..\\..\\Source\\UI\\";

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

        private static void deleteExcpected(string expected)
        {
            if (File.Exists(expected))
            { File.Delete(expected); }
        }
        #endregion
    }
}

