using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Text.Json;

namespace DumpReport
{
    public class JsonOutput
    {
        private string _outputFilePath;
        private readonly Dictionary<string, object> _keyValuePairs;

        public JsonOutput()
        {
            _keyValuePairs = new Dictionary<string, object>();
        }

        public void Open(string outputFilePath)
        {
            string jsonPath = Utils.GetAbsolutePath(outputFilePath);
            _outputFilePath = jsonPath;
        }

        public void AddKeyValuePair(string key, object value)
        {
            _keyValuePairs[key] = value;
        }

        public void ClearKeyValuePairs()
        {
            _keyValuePairs.Clear();
        }

        public void WriteToFile()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            var json = JsonSerializer.Serialize(_keyValuePairs, options);
            File.WriteAllText(_outputFilePath, json);
        }

        internal static List<object> GetAllLoadedModules(List<ModuleInfo> modules)
        {
            var result = new List<object>();
            foreach (ModuleInfo module in modules)
            {
                var value = new Dictionary<string, object>();
                value["Start Address"] = module.startAddr.TrimStart('0');
                value["End Address"] = module.endAddr.TrimStart('0');
                value["Module Name"] = module.moduleName;
                value["Timestamp"] = module.timestamp;
                value["UTC Time"] = Utils.GetUtcTimeFromTimestamp(module.timestamp);
                value["Path"] = module.imagePath;
                value["File Version"] = module.fileVersion;
                value["Product Version"] = module.productVersion;
                value["Description"] = module.fileDescription;
                value["PDB status"] = module.pdbStatus;
                value["PDB Path"] = module.pdbPath;
                result.Add(value);
            }
            return result;
        }

        internal static List<object> GetThreadInfo(ThreadInfo threadInfo)
        {
            List<FrameInfo> stack = threadInfo.stack;
            var result = new List<object>();
            int counter = 0;
            foreach (FrameInfo frame in stack)
            {
                var frameDict = GetStackFrameInfo(frame, counter);
                result.Add(frameDict);
                counter += 1;
            }
            return result;
        }

        static Dictionary<string, object> GetStackFrameInfo(FrameInfo frameInfo, int idx)
        {
            var result = new Dictionary<string, object>();
            result["module"] = frameInfo.module;
            result["function"] = Utils.EscapeSpecialChars(frameInfo.function);
            result["file"] = frameInfo.file;
            result["line"] = frameInfo.line;
            return result;
        }
    }

}
