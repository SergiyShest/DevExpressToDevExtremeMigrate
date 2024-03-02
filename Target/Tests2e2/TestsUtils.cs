using NLog;
using System.Diagnostics;
using System.Text;

namespace Tests2e2
{
	public class TestsUtils
	{
       static NLog.ILogger  log = LogManager.GetCurrentClassLogger();
		public static Process RunProcess(string command, string workingDirectory)
		{
			
			Encoding encoding = Encoding.UTF8;
			Process process = new Process();
			{
				ProcessStartInfo startInfo = new ProcessStartInfo
				{
					FileName = "cmd.exe",
					Arguments = $"/c {command}",
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					UseShellExecute = false,
					CreateNoWindow = false,
					WorkingDirectory = workingDirectory,
					StandardOutputEncoding = encoding,
					StandardErrorEncoding = encoding
				};

				Console.OutputEncoding = encoding;

				process.OutputDataReceived += (sender, e) => log.Info(e.Data);
				process.ErrorDataReceived += (sender, e) => log.Error(e.Data);

				process.StartInfo = startInfo;
				process.Start();

				process.BeginOutputReadLine();
				process.BeginErrorReadLine();
				return process;

			}
		}
	}
}
