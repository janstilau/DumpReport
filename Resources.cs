namespace DumpReport
{
    class Resources
    {
        // 这个类, 就是用来存放一些静态的资源的, 比如说, css, js, xml, help 等等
        // 将所有的资源都放在这里, 有利于管理, 也有利于修改
        static public string configFile = "DumpReportCfg.xml";

        #region css
        static public string css = @"body {
    font-family: verdana, arial, sans-serif;
    font-size: 12px;
    margin-left: 25px;
}
h1 {
    color: DarkBlue;
    font-family: verdana, arial, sans-serif;
    font-size: 20px;
    margin-left: -15px;
    margin-bottom: 0px;
}
h2 {
    color: DarkBlue;
    font-family: verdana, arial, sans-serif;
    font-size: 14px;
    margin-left: -15px;
}
button {
    padding: 2px 10px;
    font-family: verdana, arial, sans-serif;
    font-size: 12px;
    border-radius: 3px;
    border: 1px solid #7F7F7F;
}
button:hover {
    background-color: DarkGray;
    color: white;
}
button:focus {
    outline:0;
}
.toggle-button {
    padding: 0px 0px;
    margin-right: 3px;
    font-size: 10px;
    font-weight: bold;
    height: 15px;
    width: 15px;
    font-family: 'Courier New', monospace;
    text-align: center;
    vertical-align: middle;
    }
.toggle-header {
    margin-bottom: 0;
    margin-left: 0;
    margin-top: 3px;
    padding: 0;
    font-size: 12px;
    vertical-align: top;
}
.toggle-header td {
    margin-bottom: 0;
    padding: 0;
    font-family: verdana, arial, sans-serif;
    vertical-align: top;
}
.report-table {
    margin-left: 15px;
    margin-top: 0px;
    margin-bottom: 0px;
    padding: 0.5em;
}
.report-table td {
    text-align: left;
    padding: 0.3em;
    font-size: 11px;
}
.report-table th {
    font-size: 12px;
    text-align: left;
    padding: 0.3em;
    background-color: #4f81BD;
    color: white;
}
.report-table tr {
    font-size: 12px;
    height: 1em;
}
.report-table tr:nth-child(even)
{
    background-color: #eee;
}
.report-table tr:nth-child(odd)
{
    background-color:#fff;
}
.sourcecode-frame {
    font-weight: bold;
    color: black;
}
.thread-id {
    font-family: 'Consolas', 'Courier New', monospace;
    font-weight: normal;
}
";
        #endregion

        #region javascript
        static public string scripts = @"
function expand(divName, buttonName) {
    if (document.getElementById(divName) === null) return;
    document.getElementById(divName).style.display = 'block';
    document.getElementById(buttonName).firstChild.data  = '-';
}
function collapse(divName, buttonName) {
    if (document.getElementById(divName) === null) return;
    document.getElementById(divName).style.display = 'none';
    document.getElementById(buttonName).firstChild.data  = '+';
}
function toggle(divName, buttonName) {
    var div = document.getElementById(divName);
    if (div === null) return;
    if (div.style.display === 'none') {
        expand(divName, buttonName);
    }
    else {
        collapse(divName, buttonName);
    }
}
function setVisibility(show) {
    var divName = '';
    var buttonId = '';
    for (var i = 0; i < numThreads; i++)
    {
        divName = 'divThread' + i.toString();
        buttonId = 'btThread' + i.toString();
        if (show == true) {
            expand(divName, buttonId);
        }
        else {
            collapse(divName, buttonId);
        }
    }
}
";
        #endregion

        #region xml
        static public string xml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<Config>
    <Debugger exe64="""" exe32="""" timeout=""60"" />
    <Pdb folder="""" />
    <Style file="""" />
    <Report file="".\DumpReport.html"" show=""1"" />
    <Log folder="""" clean=""1""/>
    <SymbolCache folder="""" />
    <SourceCodeRoot folder="""" />
</Config >
";
        #endregion

        #region help

        static public string appHelp = @"
Creates an HTML report from a user-mode dump file using WinDBG or CDB. It shows the call stacks of all threads,
exception details (if any), the loaded modules and the environment details of the target machine.

DumpReport /DUMPFILE dump_file [/PDBFOLDER pdb_folder] [/REPORTFILE html_file] [/SHOWREPORT value] [/QUIET value]

    /DUMPFILE:   Full path of the dump file to read.
    /PDBFOLDER:  (optional) Folder containing the corresponding PDBs.
                 If not specified, PDB files are expected to be in the dump's folder.
    /REPORTFILE: (optional) Full path of the HTML report file. It can also be specified in the config file.
                 By default, a file named 'DumpReport.html' is created in the execution folder.
    /SHOWREPORT: (optional) If the value is 1, the report automatically opens in the default browser.
    /QUIET:      (optional) If 1, the console window does not show progress messages.

Example:
    DumpReport /DUMPFILE ""C:\dump\crash.dmp"" /PDBFOLDER ""C:\dump"" /SHOWREPORT 1

If the dump file is the only argument, the call can be simplified as follows:
    DumpReport ""C:\dump\crash.dmp""

In this case, it is also possible to drag and drop the dump directly onto the executable.

Any value containing spaces must be enclosed in double quotes.
Providing the PDB files is not necessary but improves the information of the call stack traces.
The location of the debuggers to use and other options must be defined in the XML
configuration file ({0}).

Run 'DumpReport /CONFIG HELP' for more information on the XML configuration file.
Run 'DumpReport /STYLE HELP' for information on customizing the report's style.";

        static public string xmlHelpIntro = @"
A file named '{0}' must exist together with the executable.
This file contains the default values of the parameters.
Some can be overriden by command line.";

        static public string xmlHelpNodes = @"
<Config>: Main node.
<Debugger>:  Supported debuggers are WinDbg.exe and CDB.exe.
    exe64:   Full path of the 64-bit version debugger.
    exe32:   Full path of the 32-bit version debugger.
    timeout: Maximum number of minutes to wait for the debugger to finish.
<Pdb>:
    folder:  Folder containing the PDB files. If not specified, PDB files are expected to be
             in the same location as the dump file.
<Style>:
    file:    Full path of a custom CSS file to use.
             Run 'DumpReport /STYLE HELP' for more information about the report's CSS style.
<Report>:
    file:    Full path of the report file to be created.
    show:    If set to 1, the report will be displayed automatically in the default browser.
<Log>:
    folder:  Folder where the debugger log files will be created.
             If not specified, log files are created in the same location as the dump file.
             The name of the log files is the name of the dump file appended with '.log'
    clean:   Indicates whether the log files should be deleted after being processed.
<SymbolCache>:
    folder:  Folder to use as symbol cache. If not specified, the debugger will use its default
             symbol cache (e.g: C:\ProgramData\dbg)
<SourceCodeRoot>:
    folder:  The report will emphasize the frames whose source file's path contains this folder.

Run 'DumpReport /CONFIG CREATE' to create a default config file.
";
        static public string cssHelp = @"
CSS styles:

body:          Default style for the HTML document.
h1:            Title header.
h2:            Section header.
button:        Default button style.
toggle-button: Style of the Expand/Collapse button (+/-)
toggle-header: Auxiliary table that contains a toggle button and a label
               that describes an area that can be expanded or collapsed.
report-table:  Style for tables showing thread call stacks, loaded modules or
               environment variables. By default, a striped style is used.
sourcecode-frame: Call stack frame associated to the source code root.
thread-id:     Style for the thread identifier and intruction pointer.

Run 'DumpReport /STYLE CREATE' to create a sample CSS file (style.css).";

        #endregion

        #region debuggerScripts

        static public string dbgScriptInit = @".logopen /u ""{LOG_FILE}""
||
.foreach (module {lm1m} ) { .if ($sicmp(""${module}"",""wow64"") == 0) { .echo WOW64 found; } }
.effmach
.logclose
";
        static public string dbgScriptMain = @".logopen /u ""{LOG_FILE}""
||
{PROGRESS_STEP}
.lines -e
.foreach (module {lm1m} ) { .if ($sicmp(""${module}"",""wow64"") == 0) { .load soswow64; .echo WOW64 found; .effmach x86;  } }
.effmach
.time
.cordll -ve -u -l
.chain
.echo > !eeversion
{PROGRESS_STEP}
!eeversion
.echo >>> TARGET INFO
!envvar COMPUTERNAME
!envvar USERNAME
.echo PROCESS_ID:
|.
.echo TARGET:
vertarget
!peb
.echo >>> MANAGED THREADS
{PROGRESS_STEP}
!Threads
.echo >>> MANAGED STACKS
.block { ~* e !clrstack }
.echo >>> EXCEPTION INFO
{PROGRESS_STEP}
.exr -1
.echo EXCEPTION CONTEXT RECORD:
.ecxr
.echo EXCEPTION CALL STACK:
~#
kv n
.echo >>> HEAP
{PROGRESS_STEP}
!heap
.echo >>> INSTRUCTION POINTERS
{PROGRESS_STEP}
.block { ~* e ? {INSTRUCT_PTR} }
.echo >>> THREAD STACKS
{PROGRESS_STEP}
~* kv n
.echo >>> LOADED MODULES
{PROGRESS_STEP}
lmov
{PROGRESS_STEP}
.echo >>> END OF LOG
.logclose
";
        static public string dbgUnhandledExceptionFilter32 = @".logopen /u ""{LOG_FILE}""
||
.block { .effmach x86 }
.lines -e
r @$t0 = 0;
.foreach(value {dd[FIRST_PARAM]}){ .if (@$t0 == 1) { .exr value }; r @$t0 = @$t0 + 1; }
.logclose
";
        static public string dbgKiUserExceptionDispatch = @".logopen /u ""{LOG_FILE}""
||
.exr [CHILD_SP] + @@c++(sizeof(ntdll!_CONTEXT)) + 0x20
.logclose
";
        // Used both with 32 and 64 bits dumps
        static public string dbgRtlDispatchException = @".logopen /u ""{LOG_FILE}""
||
.exr [THIRD_PARAM]
.logclose
";
        static public string dbgWerpReportFault64 = @".logopen /u ""{LOG_FILE}""
||
r @$t0 = 0;
.foreach(value {dq[FOURTH_PARAM]}){ .if (@$t0 == 1) { .exr value; .break; }; r @$t0 = @$t0 + 1; }
.logclose
";
        #endregion
    }
}


/*
 这些参数是 WinDbg 的命令行参数。WinDbg 是一个 Windows 调试器，用于调试用户模式应用程序和内核模式驱动程序。这些参数的作用如下：

- `-y "D:\CodeHub\DictCrash\output\Release;srv**http://msdl.microsoft.com/download/symbols"`：指定符号路径。符号路径是一个分号分隔的列表，其中每个条目都是一个符号服务器或符号目录¹。
- `-z "D:\CodeHub\DictCrash\output\Release\dmps\3_2_0__Release_1689316365306.dmp"`：指定要调试的转储文件的路径¹。
- `-c "$$><C:\Users\liugq01\AppData\Local\Temp\WinDbgScript.txt;q"`：指定在启动时运行的初始调试器命令。这个命令必须用引号括起来。多个命令可以用分号分隔¹。





这些命令看起来像是 WinDbg 的命令。WinDbg 是一个 Windows 调试器，用于调试用户模式应用程序和内核模式驱动程序。这些命令的作用如下：

- `.logopen /u ""{LOG_FILE}""`：打开一个日志文件并将其写入 Unicode 格式。如果省略此选项，则调试器将以 ASCII（ANSI）格式写入日志文件¹⁷。
- `.foreach (module {lm1m} )`：遍历所有模块。
- `.if ($sicmp(""${module}"",""wow64"") == 0)`：如果模块名称与 "wow64" 相同（不区分大小写），则执行后面的语句。
- `.echo WOW64 found`：在调试器窗口中输出 "WOW64 found"。
- `.effmach`：显示或更改调试器使用的处理器模式¹³。
- `.logclose`：关闭当前打开的日志文件¹⁸。





- `.logopen /u ""{LOG_FILE}""`：打开一个日志文件并将其写入 Unicode 格式。如果省略此选项，则调试器将以 ASCII（ANSI）格式写入日志文件。
- `{PROGRESS_STEP}`：这个命令看起来像是一个自定义的宏或脚本，它的作用取决于它的定义。
- `.lines -e`：启用源代码行号信息。这样，在调试器窗口中显示堆栈跟踪时，将显示源代码行号。
- `.foreach (module {lm1m} )`：遍历所有模块。
- `.if ($sicmp(""${module}"",""wow64"") == 0)`：如果模块名称与 "wow64" 相同（不区分大小写），则执行后面的语句。
- `.load soswow64`：加载 soswow64 扩展。soswow64 是一个调试扩展，用于调试运行在 WOW64（Windows 32-bit on Windows 64-bit）环境中的托管应用程序。
- `.echo WOW64 found`：在调试器窗口中输出 "WOW64 found"。
- `.effmach x86`：更改调试器使用的处理器模式为 x86。
- `.effmach`：显示或更改调试器使用的处理器模式。
- `.time`：显示当前时间和日期。
- `.cordll -ve -u -l`：加载并初始化 CLR 调试器扩展（例如 SOS）。这个命令会自动加载最新版本的 CLR 调试器扩展，并在必要时从符号服务器下载它。
- `.chain`：显示已加载的调试器扩展和它们的版本信息。
- `.echo > !eeversion`：在调试器窗口中输出 "> !eeversion"。
- `!eeversion`：显示 CLR 的版本信息。这个命令是 SOS 扩展的一部分。
- `.echo >>> TARGET INFO`：在调试器窗口中输出 ">>> TARGET INFO"。
- `!envvar COMPUTERNAME`：显示环境变量 COMPUTERNAME 的值。这个命令是 SOS 扩展的一部分。
- `!envvar USERNAME`：显示环境变量 USERNAME 的值。这个命令是 SOS 扩展的一部分。
- `.echo PROCESS_ID:`：在调试器窗口中输出 "PROCESS_ID:"。
- `.`：这个命令会在调试器窗口中输出当前进程的 ID。
- `.echo TARGET:`：在调试器窗口中输出 "TARGET:"。
- `vertarget`：显示有关目标计算机和操作系统的信息。
- `!peb`：显示进程环境块（PEB）的内容。这个命令是 SOS 扩展的一部分。
- `.echo >>> MANAGED THREADS`：在调试器窗口中输出 ">>> MANAGED THREADS"。
- `!Threads`：显示所有托管线程的信息。这个命令是 SOS 扩展的一部分。
- `.echo >>> MANAGED STACKS`：在调试器窗口中输出 ">>> MANAGED STACKS"。
- `.block { ~* e !clrstack }`：对所有线程执行 `!clrstack` 命令。`.block { ... }` 是一个块语句，它会执行花括号内的所有命令。`~* e ...` 是一个线程命令，它会对所有线程执行指定的命令。`!clrstack` 显示当前托管堆栈。这个命令是 SOS 扩展的一部分。
- `.echo >>> EXCEPTION INFO`：在调试器窗口中输出 ">>> EXCEPTION INFO"。
- `.exr -1`：显示最近一次异常的异常记录（EXCEPTION_RECORD）。
- `.echo EXCEPTION CONTEXT RECORD:`：在调试器窗口中输出 "EXCEPTION CONTEXT RECORD:"。
- `.ecxr`：显示异常上下文记录（CONTEXT_RECORD）。
- `.echo EXCEPTION CALL STACK:`：在调试器窗口中输出 "EXCEPTION CALL STACK:"。
- `~#`：切换到发生异常的线程。
- `kv n`：显示带有源代码行号和参数值的堆栈跟踪。`n` 选项表示不要显示源文件名称和行号。
- `.echo >>> HEAP`：在调试器窗口中输出 ">>> HEAP"。
- `!heap`：显示所有堆的信息。这个命令是 SOS 扩展的一部分。
- `.echo >>> INSTRUCTION POINTERS`：在调试器窗口中输出 ">>> INSTRUCTION POINTERS"。
- `.block { ~* e ? {INSTRUCT_PTR} }`：对所有线程执行 `? {INSTRUCT_PTR}` 命令。`.block { ... }` 是一个块语句，它会执行花括号内的所有命令。`~* e ...` 是一个线程命令，它会对所有线程执行指定的命令。`? {INSTRUCT_PTR}` 显示当前指令指针的值。
- `.echo >>> THREAD STACKS`：在调试器窗口中输出 ">>> THREAD STACKS"。
- `~* kv n`：对所有线程执行 `kv n` 命令。`~* ...` 是一个线程命令，它会对所有线程执行指定的命令。`kv n` 显示带有源代码行号和参数值的堆栈跟踪。`n` 选项表示不要显示源文件名称和行号。
- `.echo >>> LOADED MODULES`：在调试器窗口中输出 ">>> LOADED MODULES"。
- `lmov`：显示所有已加载模块的详细信息，包括版本信息、时间戳、文件大小、加载地址、符号类型和符号文件路径。
- `.echo >>> END OF LOG`：在调试器窗口中输出 ">>> END OF LOG"。
- `.logclose`：关闭当前打开的日志文件。

 */