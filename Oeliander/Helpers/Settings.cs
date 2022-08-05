using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Oeliander
{
    internal class _Settings
    {
        internal static Settings settings = new Settings();
        internal static string _Pattern = settings.Shodan_Pattern;
        internal static string _Key = settings.Shodan_API_Key;
        internal static string _Timeout = settings.Connection_Timeout;
    }
    public class Settings
    {
        public string Shodan_Pattern { get; set; } = @"\x92\x02index\x00\x00\x00\x00\x00\x00\x01";
        public string Shodan_API_Key { get; set; } = "DY4e5sKxXqzQALjmIvom5t7YCnz19lnC"; // = "0nac3uXRcY0UgCPw0QyRpUJyZoaJsqPi"
        public string Connection_Timeout { get; set; } = "3000";

        public void SaveSettings() => File.WriteAllText("settings.json", JsonConvert.SerializeObject(_Settings.settings));
        public static void SaveConfig() => File.WriteAllText("settings.json", JsonConvert.SerializeObject(_Settings.settings));
        public static void LoadSettings()
        {
            try
            {
                if (!File.Exists("settings.json"))
                    SaveConfig();
                else
                    _Settings.settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("settings.json"));
            }
            catch { }
        }

        public static PropertyInfo[] GetSettingVariables()
        {
            return typeof(Settings).GetProperties();
        }
    }
}