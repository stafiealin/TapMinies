using System.IO;
using UnityEngine;

namespace TapMinies.Core
{
    public class SaveService
    {
        private const string SaveFileName = "save.json";

        private static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

        public void Save(SaveData data)
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SavePath, json);
        }

        public SaveData Load()
        {
            if (!File.Exists(SavePath)) return null;

            string json = File.ReadAllText(SavePath);
            return JsonUtility.FromJson<SaveData>(json);
        }
    }
}
