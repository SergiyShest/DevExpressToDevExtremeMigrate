using Newtonsoft.Json.Linq;
using NUnit.Framework.Internal;
using System.Diagnostics;
using System.Text;

namespace Tests2e2 { 
    [TestFixture]
	public class CypressTests
	{

		Process mvcProcess;

		[OneTimeSetUp]
		public void Setup()
		{
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

			string command = "dotnet exec ui.dll --urls=https://localhost:7210";
			string workingDirectory =  Path.GetFullPath( @"..\..\..\..\UI\bin\Release\net6.0\publish\");

			mvcProcess = TestsUtils.RunProcess(command, workingDirectory);
			Thread.Sleep(1000);
		}
		
		[OneTimeTearDown]
		public void TearDown()
		{
			if (mvcProcess != null && !mvcProcess.HasExited)
			{
				mvcProcess.Kill(true);
				mvcProcess.WaitForExit(); // Ждем, чтобы убедиться, что процесс завершен
				mvcProcess.Dispose();
			}
		}

		[TestCaseSource(nameof(GetTests))]
		public void ViewTests(string command )
		{
			string workingDirectory = Path.GetFullPath(@"..\..\..\..\Tests2e2\js_tests");
			var process = TestsUtils.RunProcess("npm run "+command, workingDirectory);
			process.WaitForExit();
			int exitCode = process.ExitCode;

			// Проверяем exit code
			if (exitCode != 0)
			{
				Assert.Fail($"Тесты завершились с ошибкой. Exit Code: {exitCode}");
			}
			else
			{
				Console.WriteLine("Тесты успешно выполнены.");
			}
        } 

        public static IEnumerable<TestCaseData> GetTests()
        {
            string jsonConfigPath = Path.GetFullPath(@"..\..\..\..\Tests2e2\js_tests\package.json");
            string jsonConfig = File.ReadAllText(jsonConfigPath);
            JObject packageJson = JObject.Parse(jsonConfig);
            var scripts = packageJson["scripts"];

            if (scripts != null)
            {
                foreach (JProperty script in scripts.Children<JProperty>())
                {
					if (!script.Name.Contains("run:e2e.")) continue;
					yield return new TestCaseData(script.Name.ToString()) { TestName = script.Name};
                }
            }
            else
            {
                Console.WriteLine("Scripts not found in the JSON.");
            }
           
        }

    }
}
