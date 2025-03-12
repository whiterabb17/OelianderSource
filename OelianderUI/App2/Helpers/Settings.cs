using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OelianderUI.Helpers
{
    internal class _Settings
    {
        internal static Settings settings = new Settings();
        internal static string _Pattern { get; set; } = settings.Shodan_Pattern;
        internal static string _Key { get; set; } = settings.Shodan_API_Key;
        internal static string _Timeout { get; set; } = settings.Connection_Timeout;
    }
    public class Settings
    {
        public string Shodan_Pattern { get; set; } = @"\x92\x02index\x00\x00\x00\x00\x00\x00\x01";
        public string Shodan_API_Key { get; set; } = ""; // = ""
        public string Connection_Timeout { get; set; } = "3000";
        public void SaveSettings()
        {
            try
            {
                File.Delete("settings.json");
                File.WriteAllText("settings.json", JsonConvert.SerializeObject(_Settings.settings));
                Console.WriteLine("Settings Saved");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public static void SaveConfig() => File.WriteAllText("settings.json", JsonConvert.SerializeObject(_Settings.settings));
        public void LoadSettings()
        {
            try
            {
                if (!File.Exists("settings.json"))
                {
                    _Settings._Pattern = _Settings.settings.Shodan_Pattern;
                    _Settings._Timeout = _Settings.settings.Connection_Timeout;
                    _Settings._Key = _Settings.settings.Shodan_API_Key;
                    SaveConfig();
                }
                else
                {
                    var _settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("settings.json"));
                    _Settings._Pattern = _settings.Shodan_Pattern;
                    _Settings._Timeout = _settings.Connection_Timeout;
                    _Settings._Key = _settings.Shodan_API_Key;
                }
                //_Settings.settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("settings.json"));
            }
            catch { }
        }

        public static PropertyInfo[] GetSettingVariables()
        {
            return typeof(Settings).GetProperties();
        }
    }

}
