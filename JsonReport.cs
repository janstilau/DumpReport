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
    }

}
