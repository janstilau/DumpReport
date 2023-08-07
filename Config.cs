﻿using System;
using System.Xml;
using System.IO;

namespace DumpReport
{
    /// <summary>
    /// Contains the input parameters, specified from command line and from the XML configuration file.
    /// </summary>
    /// 所有的配置, 无论是 cmd 传递的, 还是配置文件的, 最终在内存里面, 是汇集到了这里.
    class Config
    {
        public string DbgExe64 { get; set; }        // Full path of the 64-bit version debugger
        public string DbgExe32 { get; set; }        // Full path of the 32-bit version debugger
        public Int32 DbgTimeout { get; set; }       // Maximum number of minutes to wait for the debugger to finish
        // Style file, 配置文件里面的 style, 在这里发挥了作用.
        public string StyleFile { get; set; }       // Full path of a custom CSS file to use
        // 最终的报告文件, 也是在这里发挥了作用.
        public string ReportFile { get; set; }      // Full path of the report to be created
        public string JsonFile { get; set; }
        public bool ReportShow { get; set; }        // If true, the report will be displayed automatically in the default browser
        // 是否打印进度信息. 
        public bool QuietMode { get; set; }         // If true. the application will not show progress messages in the console
        public string SymbolCache { get; set; }     // Folder to use as the debugger's symbol cache
        public string DumpFile { get; set; }        // Full path of the DMP file
        public string PdbFolder { get; set; }       // Folder where the PDBs are located
        public string LogFile { get; set; }         // Full path of the debugger's output file
        public string LogFolder { get; set; }       // Folder where the debugger's output file is stored
        // 是否在执行结束之后, 清除日志文件. 
        public bool LogClean { get; set; }          // If true, log files are deleted after execution
        public string SourceCodeRoot { get; set; }  // Specifies a root folder for the source files

        // 构造函数的主要作用, 就是完成所有的成员变量的初始化, 这是正确的.
        public Config()
        {
            DbgExe64 = String.Empty; // 这个值, 从配置文件中读取
            DbgExe32 = String.Empty; // 这个值, 从配置文件中读取
            DbgTimeout = 60; // 默认的超时, 一分钟, 要将 PDB, DMP 解析完毕, 然后生成报告.
            StyleFile = String.Empty; // 最终的 html 的 css 部分, 这是一个外界修改的入口而已.
            ReportFile = "DumpReport.html"; // 最终的HTML报告文件.
            JsonFile = "DumpReport.json"; // 最终的JSON报告文件. 这是这一次的修改. 
            ReportShow = false;
            QuietMode = false;
            SymbolCache = "";
            DumpFile = String.Empty;
            PdbFolder = String.Empty;
            LogFolder = String.Empty;
            LogFile = String.Empty;
            SourceCodeRoot = String.Empty;
        }

        // If the user requested for help, displays the help in the console and returns true.
        // Otherwise returns false.
        // 当, 拥有帮助参数的时候, 直接将帮助信息进行打印, 然后整个程序直接退出. 
        // 命令行程序, 其实就是一个类似函数的东西. 他具有的是固定的流程, 这个流程, 就是根据命令行参数来分辨的. 
        // 在这里, 作者提供了完善的 help 提示系统.
        public bool CheckHelp(string[] args)
        {
            // 从这里来看, 应该是系统将第一个参数删除了, 这样应用程序获取的, 就是纯业务数据了. 如果没有参数, 或者参数是 /?, 那么就打印帮助信息.
            if (args.Length == 0 || (args.Length == 1 && args[0] == "/?"))
                return PrintAppHelp();

            // 这里的逻辑, 不应该在这里, 应该抽取出去. 
            // 对于特定的参数, 有特定的处理逻辑, 将这些逻辑集中到这里处理了.
            for (int idx = 0; idx < args.Length; idx++)
            {
                // 如果, 有着 Config 这个参数, 那么就进行特殊的关于Config文件的处理.
                if (args[idx] == "/CONFIG")
                {
                    if ((idx + 1 < args.Length) && args[idx + 1] == "HELP")
                        return PrintConfigHelp();
                    if ((idx + 1 < args.Length) && args[idx + 1] == "CREATE")
                        return CreateConfigFile();
                    throw new ArgumentException("Use /CONFIG with HELP or CREATE");
                }
                // 如果, 有着 Style 这个参数, 那么就进行特殊的关于Style文件的处理.
                if (args[idx] == "/STYLE")
                {
                    if ((idx + 1 < args.Length) && args[idx + 1] == "HELP")
                        return PrintStyleHelp();
                    if ((idx + 1 < args.Length) && args[idx + 1] == "CREATE")
                        return CreateCSS();
                    throw new ArgumentException("Use /STYLE with HELP or CREATE");
                }
            }
            return false;
        }

        // Reads the parameters from the configuration file
        // 在程序的开始, 就执行到这里, 配置文件的读取, 是真正的程序的入口文件.
        /*
         *
         <?xml version="1.0" encoding="utf-8" ?>
<Config>
    <Debugger exe64="C:\Program Files (x86)\Windows Kits\10\Debuggers\x64\cdb.exe" exe32="C:\Program Files (x86)\Windows Kits\10\Debuggers\x86\cdb.exe" timeout="60" />
    <Pdb folder="" />
    <Style file="" />
    <Report file=".\DumpReport.html" show="1" />
    <Log folder="" clean="1"/>
    <SymbolCache folder="" />
    <SourceCodeRoot folder="D:\CodeHub\DictCrash\ErrorTrigger" />
</Config >

         */
        public void ReadConfigFile(string configPath)
        {
            string value;
            // 比如要有配置文件存在.
            if (!File.Exists(configPath))
                throw new Exception("Configuration file does not exist.\nPlease run 'DumpReport /CONFIG CREATE' to create it.");
            try
            {
                using (XmlReader reader = XmlReader.Create(configPath))
                {
                    while (reader.Read())
                    {
                        if (reader.IsStartElement())
                        {
                            // 所有的配置, 都是使用属性的方式进行的配置
                            // 所有, 当读到相应的节点之后, 直接就是读取相关的属性就可以了.
                            switch (reader.Name)
                            {
                                // 这的逻辑很简单, 就是找对应的属性, 读取里面的值而已.
                                case "Debugger":
                                    value = reader["exe64"];
                                    if (value != null && value.Length > 0)
                                        DbgExe64 = value;
                                    value = reader["exe32"];
                                    if (value != null && value.Length > 0)
                                        DbgExe32 = value;
                                    value = reader["timeout"];
                                    if (value != null && value.Length > 0)
                                        DbgTimeout = Convert.ToInt32(value);
                                    break;
                                case "Pdb":
                                    value = reader["folder"];
                                    if (value != null && value.Length > 0)
                                        PdbFolder = value;
                                    break;
                                case "Style":
                                    value = reader["file"];
                                    if (value != null && value.Length > 0)
                                        StyleFile = value;
                                    break;
                                case "Report":
                                    value = reader["file"];
                                    if (value != null && value.Length > 0)
                                        ReportFile = value;
                                    value = reader["show"];
                                    if (value != null && value.Length > 0)
                                        ReportShow = (value == "1");
                                    break;
                                case "Log":
                                    value = reader["folder"];
                                    if (value != null && value.Length > 0)
                                        LogFolder = value;
                                    value = reader["clean"];
                                    if (value != null && value.Length > 0)
                                        LogClean = Convert.ToInt32(value) == 1;
                                    LogClean = false;
                                    break;
                                case "SymbolCache":
                                    value = reader["folder"];
                                    if (value != null && value.Length > 0)
                                        SymbolCache = value;
                                    break;
                                case "SourceCodeRoot":
                                    value = reader["folder"];
                                    if (value != null && value.Length > 0)
                                        SourceCodeRoot = value.ToUpper();
                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw new Exception("Configuration file contains errors.\nPlease run 'DumpReport /CONFIG HELP' for XML syntax.");
            }
        }

        // Reads the parameters from the command-line
        public void ReadCommandLine(string[] args)
        {
            if (args.Length == 1 && args[0][0] != '/')
            {
                DumpFile = args[0];
                return;
            }
            try
            {
                // 这的逻辑很简单, 就是根据参数的不同, 进行不同的处理而已.
                // 最终将Conifg里面的属性, 进行填充.
                for (int idx = 0; idx < args.Length; idx++)
                {
                    // 从这里看, C# 在函数调用的时候, 也是可以增加 param 的前缀的.
                    if (args[idx] == "/DUMPFILE") DumpFile = GetParamValue(args, ref idx);
                    else if (args[idx] == "/PDBFOLDER") PdbFolder = GetParamValue(args, ref idx);
                    else if (args[idx] == "/REPORTFILE") ReportFile = GetParamValue(args, ref idx);
                    else if (args[idx] == "/SHOWREPORT") ReportShow = (GetParamValue(args, ref idx) == "1");
                    else if (args[idx] == "/QUIET") QuietMode = (GetParamValue(args, ref idx) == "1");
                    else throw new ArgumentException("Invalid parameter " + args[idx]);
                }
                if (DumpFile.Length == 0)
                    throw new ArgumentException("/DUMPFILE parameter not found");
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException(ex.Message + "\r\nPlease type 'DumpReport' for help.");
            }
        }

        // Retrieves the value from the pair '/PARAMETER value'
        string GetParamValue(string[] args, ref int idx)
        {
            if (idx + 1 >= args.Length || args[idx + 1].Length == 0 || args[idx + 1][0] == '/')
                throw new ArgumentException("Value not found for parameter " + args[idx]);
            return args[++idx];
        }

        // 判断一些, 给出的 debug 到底是哪个程序.
        // 1. 判断文件是否存在. 2. 判断是不是 windbg 或者 cdb, 也就是需要特定的程序来进行 dmp 的解析.
        public void CheckDebugger(string dbgFullPath, string bitness)
        {
            // 如果没有设定相关的路径, 其实是不会进行校验的.
            if (dbgFullPath.Length > 0)
            {
                if (!File.Exists(dbgFullPath))
                    throw new ArgumentException(String.Format("{0} debugger not found: {1}", bitness, dbgFullPath));
                string debugger = Path.GetFileName(dbgFullPath).ToLower();
                if (debugger != "windbg.exe" && debugger != "cdb.exe")
                    throw new ArgumentException(String.Format("Wrong {0} debugger ('{1}'). Only 'WinDBG.exe' or 'CDB.exe' are supported.", bitness, debugger));
            }
        }

        // Checks that the files and folders exist and sets all paths to absolute paths.
        // 程序的进行, 需要完成特定的参数检查, 不然程序就在非法的状态下进行了, 所以, 在这里专门进行了检查. 
        // 这里面的逻辑很简单, 看下对应的参数是不是合法值, 否则就 throw, 每个属性 throw 的时候, 都进行专门的错误信息说明.
        public void CheckArguments()
        {
            // Check dump file path.
            DumpFile = Utils.GetAbsolutePath(DumpFile);
            if (Path.GetExtension(DumpFile).ToUpper() != ".DMP")
                throw new Exception("Only dump files (*.dmp) are supported.");
            if (!File.Exists(DumpFile))
                throw new ArgumentException("Dump file not found: " + DumpFile);

            // Check pdb file path.
            if (PdbFolder.Length == 0)
                PdbFolder = Path.GetDirectoryName(DumpFile);
            else
                PdbFolder = Utils.GetAbsolutePath(PdbFolder);
            if (!Directory.Exists(PdbFolder))
                throw new ArgumentException("PDB folder not found: " + PdbFolder);

            // Check debugger paths.
            DbgExe64 = Utils.GetAbsolutePath(DbgExe64);
            DbgExe32 = Utils.GetAbsolutePath(DbgExe32);
            if (DbgExe64.Length == 0 && DbgExe32.Length == 0)
                throw new ArgumentException("No debuggers specified in the configuration file.\r\nPlease type 'DumpReport /CONFIG HELP' for help.");
            if (!Environment.Is64BitOperatingSystem && DbgExe32.Length == 0)
                throw new Exception("The attribute 'exe32' must be set on 32-bit computers.");
            
            // 进行可执行程序的校验 
            CheckDebugger(DbgExe64, "64-bit");
            CheckDebugger(DbgExe32, "32-bit");

            // Check style file.
            // 这个是最终输出到 html 里面, 引入的程序
            StyleFile = Utils.GetAbsolutePath(StyleFile);
            if (StyleFile.Length > 0 && !File.Exists(StyleFile))
                throw new ArgumentException("Style file (CSS) not found: " + StyleFile);

            // Check log file.
            // 如果制定了Log所在位置, 就输出到这里. 可能应该是为了收集 log 使用的
            LogFolder = Utils.GetAbsolutePath(LogFolder);
            if (LogFolder.Length > 0)
            {
                if (!Directory.Exists(LogFolder))
                    throw new ArgumentException("Invalid log folder " + LogFolder);
                LogFile = Path.Combine(LogFolder, Path.GetFileName(DumpFile) + ".log");
            }
            else
                LogFile = DumpFile + ".log";

            // Check symbol cache.
            SymbolCache = Utils.GetAbsolutePath(SymbolCache);
            if (SymbolCache.Length > 0 && !Directory.Exists(SymbolCache))
                throw new ArgumentException("Symbol cache folder not found: " + SymbolCache);

            // Make sure the report file contains a full path.
            ReportFile = Utils.GetAbsolutePath(ReportFile);
        }

        static void PrintColor(string line, ConsoleColor color)
        {
            // 如何使用颜色进行命令行的输出, 其实是 Console 本身提供了接口.
            // 在真正的进行了输出了之后, 立马进行了重置的操作.
            Console.ForegroundColor = color;
            Console.WriteLine(line);
            Console.ResetColor();
        }

        // Prints the application usage to the console
        static bool PrintAppHelp()
        {
            // 将常量, 统一的使用 Resources 进行管理, 让代码更加的清晰.
            Console.WriteLine(string.Format(Resources.appHelp, Path.GetFileName(Program.configFile)));
            return true;
        }

        // Prints the configuration file syntax to the console
        static bool PrintConfigHelp()
        {
            Console.WriteLine(string.Format(Resources.xmlHelpIntro, Path.GetFileName(Program.configFile)));
            PrintColor("\r\nSample:\r\n", ConsoleColor.White);
            Console.WriteLine(Resources.xml);
            PrintColor("Nodes:", ConsoleColor.White);
            Console.WriteLine(Resources.xmlHelpNodes);
            return true;
        }

        // Prints the CSS file syntax to the console
        static bool PrintStyleHelp()
        {
            Console.WriteLine(Resources.cssHelp);
            return true;
        }

        // Creates an empty configuration file.
        // 作为一个优雅的程序来说, 提前将用户的需求了解, 然后写出对应的帮助函数, 这是一个非常优雅的行为.
        static bool CreateConfigFile()
        {
            string file = Program.configFile;
            if (File.Exists(file))
            {
                Console.Write("File already exists. Overwrite? [Y/N] > ");
                if (Console.ReadKey().Key != ConsoleKey.Y)
                    return true;
                Console.WriteLine();
            }

            using (StreamWriter stream = new StreamWriter(file))
                stream.WriteLine(Resources.xml);
            Console.WriteLine("Configuration file created.\nPlease edit the path to the debuggers (WinDBG.exe or CDB.exe).");
            return true;
        }

        // Creates a default CSS file (style.css).
        static bool CreateCSS()
        {
            string file = Utils.GetAbsolutePath("style.css");
            if (File.Exists(file))
            {
                Console.Write("File " + file + " already exists. Overwrite? [Y/N] > ");
                if (Console.ReadKey().Key != ConsoleKey.Y)
                    return true;
                Console.WriteLine();
            }
            using (StreamWriter stream = new StreamWriter(file))
                stream.WriteLine(Resources.css);
            Console.WriteLine("File " + file + " has been created.\nEdit the <Style> entry in the XML configuration file in order to use it.");
            return true;
        }
    }
}
