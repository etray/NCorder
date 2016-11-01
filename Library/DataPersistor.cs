using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCorder
{
    // Saves out a set of name/value pairs.
    public class DataPersistor
    {
        private static string FileName = "ncorder.json";

        public string FullPath { get; set; }

        // Delegates to get/put data into the app.
        public Func<IDictionary<string,string>> SaveDataMethod { get; set; }
        public Action<IDictionary<string,string>> RestoreDataMethod { get; set; }

        public DataPersistor()
        {
            FullPath = Path.Combine(Path.GetTempPath(), FileName);
        }

        public void Save()
        {
            // Get data from App
            IDictionary<string,string> data = this.SaveDataMethod();
            // Write data to file
            File.WriteAllText(FullPath, JsonConvert.SerializeObject(data));
        }

        public void Restore()
        {
            if (File.Exists(FullPath))
            {
                // Get data from File
                IDictionary<string, string> data = JsonConvert.DeserializeObject<IDictionary<string, string>>(File.ReadAllText(FullPath));
                // Set data in app
                this.RestoreDataMethod(data);
            }
        }
    }
}
