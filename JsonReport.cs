using System.Collections.Generic;
using System.IO;
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

        internal static Dictionary<string, object> GetThreadInfo(ThreadInfo threadInfo)
        {
            var result = new Dictionary<string, object>();
            List<FrameInfo> stack = threadInfo.stack;
            var stacks = new List<object>();
            int counter = 0;
            foreach (FrameInfo frame in stack)
            {
                var frameDict = GetStackFrameInfo(frame, counter);
                stacks.Add(frameDict);
                counter += 1;
            }
            return result;
        }

        static Dictionary<string, object> GetStackFrameInfo(FrameInfo frameInfo, int idx)
        {
            var result = new Dictionary<string, object>();
            result["idx"] = idx.ToString();
            result["module"] = frameInfo.module;
            result["function"] = Utils.EscapeSpecialChars(frameInfo.function);
            result["file"] = frameInfo.file;
            result["line"] = frameInfo.line;
            return result;
        }
    }

}
