using Newtonsoft.Json;
using System.IO;
using TShockAPI;

namespace WSRC_Plugin
{
    public class Config
    {
        public static string ConfigPath = Path.Combine(TShock.SavePath, "", "WSRC_config.json");
        public int ListenPort = 7778;
        public int MaxConnections = 5;
        public int MessageBufferLength = 20;
        public string Username = "server";
        public string Password = "";

        public void Save()
        {
            using (StreamWriter sw = new StreamWriter(File.Open(ConfigPath, FileMode.Create)))
            {
                sw.Write(JsonConvert.SerializeObject(this, Formatting.Indented));
            }
        }

        public static Config Load()
        {
            using (StreamReader sr = new StreamReader(File.Open(ConfigPath, FileMode.Open)))
            {
                return JsonConvert.DeserializeObject<Config>(sr.ReadToEnd());
            }
        }
    }
}
