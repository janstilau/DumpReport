﻿using System;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace DumpReport
{
    class Program
    {
        const int EXIT_SUCCESS = 0;
        const int EXIT_FAILURE = 1;

        static public string configFile = null;
        static public string appDirectory = null; // 使用系统的API, 将当前进程的路径获取到了.
        static public bool is32bitDump = false; // True if the dump corresponds to a 32-bit process
        static int exitCode = EXIT_SUCCESS;

        static Config config = new Config(); // Stores the paramaters of the application
        static Report report = new Report(config); // Outputs extracted data to an HTML file
        static LogManager logManager = new LogManager(config, report); // Parses the debugger's output log

        static int Main(string[] args)
        {
            try
            {
                WriteTitle();

                appDirectory = new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).DirectoryName;
                configFile = Utils.GetAbsolutePath(Resources.configFile);

                // If the user just requests help, show help and quit.
                if (config.CheckHelp(args) == true)
                    return EXIT_SUCCESS;

                // Read parameters from config file and command line
                config.ReadConfigFile(configFile);
                config.ReadCommandLine(args);

                // Create the report file
                report.Open(config.ReportFile);

                // Basic check of the input parameters
                config.CheckArguments();

                WriteDebuggerVersion();

                WriteConsole("Processing dump " + config.DumpFile);
                WriteConsole("Checking dump bitness...", true);
                // Find out dump bitness.
                // 从执行结果来看, 这个 log 文件, 是和当前的 dmp 文件是一级的. .dmp.log 如此命名的. 
                LaunchDebugger(Resources.dbgScriptInit, config.LogFile);
                CheckDumpBitness();
                // Execute main debugger script
                WriteConsole("Creating log...", true);
                LaunchDebugger(Resources.dbgScriptMain, config.LogFile, !config.QuietMode);

                // Process debugger's output
                WriteConsole("Reading log...", true);

                // 从现在开始, 就是在处理 log 文件了.
                logManager.ReadLog();
                logManager.ParseLog();
                logManager.CombineParserInfo();

                // If the dump reveals an exception but the details are missing, try to find them
                if (logManager.GetExceptionInfo() && logManager.NeedMoreExceptionInfo())
                {
                    FindExceptionRecord(); // Execute a new script in order to retrieve the exception record
                    logManager.GetExceptionInfo(); // Check again with the new script output
                }

                // Write the extracted information to the report file
                logManager.WriteReport();
            }
            catch (Exception ex)
            {
                // 获取到 ex 之后, 就是显示 ex 的 message 字段里面的内容
                ShowError(ex.Message);
            }

            if (report.IsOpen())
            {
                report.Close();
                if (config.ReportShow && File.Exists(config.ReportFile))
                    LaunchBrowser(config.ReportFile);
            }
            WriteConsole("Finished.");
            return exitCode; // Return success (0) or failure (1)
        }

        // Selects the most appropiate debugger to use depending on the current OS and the dump bitness
        public static string GetDebuggerPath()
        {
            if (!Environment.Is64BitOperatingSystem)
                return config.DbgExe32;

            if (is32bitDump)
            {
                if (config.DbgExe32.Length > 0)
                    return config.DbgExe32;
                return config.DbgExe64;
            }
            else
            {
                if (config.DbgExe64.Length > 0)
                    return config.DbgExe64;
                return config.DbgExe32;
            }
        }

        static async Task<bool> LaunchDebuggerAsync(Process process)
        {
            // 在这里, 超时时间发挥了作用.
            return await Task.Run(() =>
            {
                return process.WaitForExit(config.DbgTimeout * 60 * 1000); // Convert minutes to milliseconds
            });
        }

        static string PreprocessScript(string script, string outFile, LogProgress progress)
        {
            // Insert the output path in the script
            script = script.Replace("{LOG_FILE}", outFile);
            // Set the proper intruction pointer register
            script = script.Replace("{INSTRUCT_PTR}", is32bitDump ? "@eip" : "@rip");
            // Adapt or remove the progress information
            if (progress != null)
                script = progress.PrepareScript(script, outFile, is32bitDump);
            else
                script = LogProgress.RemoveProgressMark(script);
            return script;
        }

        // Launches the debugger, which automatically executes a script and stores the output into a file
        public static void LaunchDebugger(string injectedScript, string outFile, bool showProgress = false)
        {
            LogProgress progress = showProgress ? new LogProgress() : null;

            // Set the path of the temporary script file
            string scriptFile = Path.Combine(Path.GetTempPath(), "WinDbgScript.txt");

            // Replace special marks in the original script
            injectedScript = PreprocessScript(injectedScript, outFile, progress);

            // Remove old files
            File.Delete(scriptFile);
            File.Delete(outFile);

            // Create the script file
            // 将要运行的命令, 写入了 scriptFile 文件里面. 这样, 下面的命令是从文件中读取执行的
            using (StreamWriter stream = new StreamWriter(scriptFile))
                stream.WriteLine(injectedScript);

            // Start the debugger
            string arguments = string.Format(@"-y ""{0};srv*{1}*http://msdl.microsoft.com/download/symbols"" -z ""{2}"" -c ""$$><{3};q""",
                config.PdbFolder, config.SymbolCache, config.DumpFile, scriptFile);
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = GetDebuggerPath(),
                Arguments = arguments,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden // WinDBG will only hide the main window
            };

            // 启动了一个新的进程, 并且等待这个进程结束.
            Process process = new Process();
            process.StartInfo = psi;
            if (!process.Start())
                throw new Exception("The debugger could not be launched.");
            Task<bool> task = LaunchDebuggerAsync(process);
            while (!task.IsCompleted)
            {
                task.Wait(500);
                if (showProgress)
                    progress.ShowLogProgress();
            }
            bool exited = task.Result;

            File.Delete(scriptFile);

            if (!exited)
            {
                process.Kill();
                throw new Exception(String.Format("Execution has been cancelled after {0} minutes.", config.DbgTimeout));
            }
            if (process.ExitCode != 0)
                throw new Exception("The debugger did not finish properly.");

            if (showProgress)
                progress.DeleteProgressFile();

            // Check that the output log has been generated
            // 使用了调试工具, 完成了日志文件的输出, 然后读取解析这个日志文件, 来进行真正的分析.
            if (!File.Exists(outFile))
                throw new Exception("The debugger did not generate any output.");
        }

        // Opens an html file with the default browser
        public static void LaunchBrowser(string htmlFile)
        {
            Process.Start(htmlFile);
        }

        // Determines whether the dump corresponds to a 32 or 64-bit process, by reading the
        // output of a script previously executed by the debugger
        public static void CheckDumpBitness()
        {
            bool x86Found = false;
            bool wow64Found = false;
            // Read the output file generated by the debugger
            using (StreamReader file = new StreamReader(config.LogFile, Encoding.Unicode))
            {
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    if (line.Contains("WOW64 found"))
                        wow64Found = true;
                    else if (line.Contains("Effective machine") && line.Contains("x86"))
                        x86Found = true;
                }
                is32bitDump = (wow64Found || x86Found);
            }

            // 这里有什么问题吗, 可执行程序和 Debugger 的位数不相同.
            if (is32bitDump && GetDebuggerPath() == config.DbgExe64)
                logManager.notes.Add("32-bit dump processed with a 64-bit debugger.");
            else if (!is32bitDump && GetDebuggerPath() == config.DbgExe32)
                logManager.notes.Add("64-bit dump processed with a 32-bit debugger.");
            if (wow64Found)
                logManager.notes.Add("64-bit dumps of 32-bit processes may show inaccurate or incomplete call stack traces.");
        }

        // Tries to obtain the proper exception record by using auxiliary debugger scripts.
        public static void FindExceptionRecord()
        {
            string exrLogFile = config.LogFile;
            exrLogFile = Path.ChangeExtension(exrLogFile, ".exr.log"); // Store the output of the exception record script in a separate file
            File.Delete(exrLogFile); // Delete previous logs

            WriteConsole("Getting exception record...", true);

            string script = logManager.GetExceptionRecordScript();
            if (script != null)
                LaunchDebugger(script, exrLogFile);

            if (File.Exists(exrLogFile))
            {
                logManager.ParseExceptionRecord(exrLogFile);
                if (config.LogClean)
                    File.Delete(exrLogFile);
            }
        }

        public static void ShowError(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n" + msg);
            Console.ResetColor();
            report.WriteError(msg);
            exitCode = EXIT_FAILURE;
        }

        // 在程序的开始, 打印了一下层序的名称和版本号.
        public static void WriteTitle()
        {
            if (config.QuietMode) return;
            // 系统提供了一些公用的方法, 用来获取当前正在执行的程序的信息, 包括这个可执行文件的路径, 版本号等等
            Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            Version version = assembly.GetName().Version;
            Console.ForegroundColor = ConsoleColor.White;
            // C# 里面的格式化输出, 是使用了这种方式. {0} {1} {2} 这种方式.
            Console.WriteLine(String.Format("{0} {1}.{2}", Assembly.GetCallingAssembly().GetName().Name,
                version.Major, version.Minor));
            Console.ResetColor();
        }

        public static void WriteDebuggerVersion()
        {
            if (config.QuietMode) return;
            string debuggerPath = GetDebuggerPath();
            var versionInfo = FileVersionInfo.GetVersionInfo(debuggerPath);
            string version = versionInfo.FileVersion;
            Console.WriteLine("Using {0} version {1}", Path.GetFileName(debuggerPath).ToLower(), version);
        }

        public static void WriteConsole(string msg, bool sameLine = false)
        {
            if (config.QuietMode) return;
            if (sameLine)
            {
                string blank = String.Empty;
                blank = blank.PadLeft(Console.WindowWidth - 1, ' ');
                Console.Write("\r" + blank); // Clean the line before writing
                Console.Write("\r" + msg);
            }
            else
                Console.WriteLine(msg);
        }
    }
}
