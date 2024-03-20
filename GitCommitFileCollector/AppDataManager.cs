using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GitCommitFileCollector
{
    public class AppDataManager
    {
        private static AppDataManager? SingletonInstance { get; set; }

        public static AppDataManager Instance => SingletonInstance == null ? (SingletonInstance = new AppDataManager()) : SingletonInstance;

        private const string APP_DATA_FILE = "app.json";

        private AppData AppData { get; set; }

        private AppDataManager() 
        {
            if (!File.Exists(APP_DATA_FILE))
            {
                AppData = new AppData();
                Save();
            }

            string json = File.ReadAllText(APP_DATA_FILE);
            var t = JsonSerializer.Deserialize<AppData>(json);
            if (t == null) { throw new Exception(); }
            AppData = t;
        }


        public bool NotSelectedDirectory => AppData == null || string.IsNullOrEmpty(AppData.TargetDirectory);

        public bool DirectorySelected => Directory.Exists(AppData.TargetDirectory);

        public void SetTargetDirectory(string path)
        {
            AppData.TargetDirectory = path;
            Save();
        }

        private void Save()
        {
            string s = JsonSerializer.Serialize(AppData, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(APP_DATA_FILE, s);
        }
    }
}
