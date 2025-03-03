using Renci.SshNet;
using System;
using System.Threading;

namespace Oeliander
{
    public class SSH
    {
        public bool Enabled { get; set; }
        public SshClient Client { get; set; }
        public ShellStream Shell { get; set; }
        public Thread Thread { get; set; }
        public string IP { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public SSH(string ip, string username, string password)
        {
            IP = ip;
            Username = username;
            Password = password;
        }
    }

    public static class ServerExtensions
    {
        #region MainWindow Connection Logic
        public static bool TryConnect(this SSH ssh)
        {
            try
            {
                if (ssh.Client == null || ssh.Shell == null || !ssh.Client.IsConnected)
                    return ssh.Connect();
                else
                    return false;
            }
            catch { return false; }
        }
        private static MainWindow main;
        public static bool Connect(this SSH ssh)
        {
            try
            {
                ssh.Client = new SshClient(ssh.IP, 22, ssh.Username, ssh.Password);
                ssh.Client.ConnectionInfo.Timeout = TimeSpan.FromMilliseconds(Convert.ToInt32(_Settings.settings.Connection_Timeout));
                ssh.Client.Connect();
                ssh.Shell = ssh.Client.CreateShellStream("vt-100", 80, 60, 800, 600, 65536);

                Thread thread = new Thread(() => ssh.Receiver());
                thread.Start();
                return ssh.Client.IsConnected;
            }
            catch (Exception ex) 
            {
                main.addLog($"{ssh.Username}@{ssh.IP}: {ex.Message}");
                Console.WriteLine($"{ssh.Username}@{ssh.IP}: {ex.Message}"); return false; 
            }
        }

        public static void SendCMD(this SSH ssh, string cmd, MainWindow mainWindow)
        {
            main = mainWindow;
            try
            {
                if (TryConnect(ssh))
                {
                    main.addLog($"{ssh.Username}@{ssh.IP}: Connected Successfully");
                    ssh.Shell.Write(cmd + "\n");
                    ssh.Shell.Flush();
                }
            }
            catch (Exception ex) { Console.WriteLine($"{ssh.Username}@{ssh.IP}: {ex.Message}"); }
        }

        public static void Receiver(this SSH ssh)
        {
            while (true)
            {
                try
                {
                    if (ssh.Shell != null && ssh.Shell.DataAvailable)
                    {
                        string content = ssh.Shell.Read();
                        Console.WriteLine($"{ssh.Username}@{ssh.IP}:", content);
                        main.addLog($"{ssh.Username}@{ssh.IP}: {content}\n");
                    }
                }
                catch { }
                Thread.Sleep(200);
            }
        }
        #endregion MainWindow Connection Logic
        #region TerminalWindow Connection Logic
        public static bool TryConnect(this SSH ssh, TerminalWindow window)
        {
            try
            {
                if (ssh.Client == null || ssh.Shell == null || !ssh.Client.IsConnected)
                    return ssh.Connect(window);
                else
                    return false;
            }
            catch { return false; }
        }
        public static bool Connect(this SSH ssh, TerminalWindow window)
        {
            try
            {
                ssh.Client = new SshClient(ssh.IP, 22, ssh.Username, ssh.Password);
                ssh.Client.ConnectionInfo.Timeout = TimeSpan.FromMilliseconds(Convert.ToInt32(_Settings.settings.Connection_Timeout));
                ssh.Client.Connect();
                ssh.Shell = ssh.Client.CreateShellStream("vt-100", 80, 60, 800, 600, 65536);

                Thread thread = new Thread(() => ssh.Receiver(window));
                thread.Start();
                return ssh.Client.IsConnected;
            }
            catch (Exception ex)
            {
                window.sessionResult($"{ssh.Username}@{ssh.IP}: {ex.Message}");
                Console.WriteLine($"{ssh.Username}@{ssh.IP}: {ex.Message}"); return false;
            }
        }
        public static void SendCMD(this SSH ssh, string cmd, TerminalWindow termWindow)
        {
            TerminalWindow term = termWindow;
            try
            {
                if (TryConnect(ssh))
                {
                    ssh.Shell.Write(cmd + "\n");
                    ssh.Shell.Flush();
                }
                else 
                {
                    term.sessionResult($"{ssh.Username}@{ssh.IP}: Disconnected Unexpectadly");
                }
            }
            catch (Exception ex) 
            { 
                term.sessionResult($"{ssh.Username}@{ssh.IP}: {ex.Message}");
                Console.WriteLine($"{ssh.Username}@{ssh.IP}: {ex.Message}"); 
            }
        }

        public static void Receiver(this SSH ssh, TerminalWindow window)
        {
            while (true)
            {
                try
                {
                    if (ssh.Shell != null && ssh.Shell.DataAvailable)
                    {
                        string content = ssh.Shell.Read();
                        Console.WriteLine($"{ssh.Username}@{ssh.IP}:", content);
                        window.addResult($"{ssh.Username}@{ssh.IP}> {content}");
                    }
                }
                catch { }
                Thread.Sleep(200);
            }
        }
        #endregion TerminalWindow Connection Logic
    }
}