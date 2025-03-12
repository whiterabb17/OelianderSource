using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OelianderUI.Views;
using Renci.SshNet;

namespace OelianderUI.Helpers;

public class SSH
{
    public bool Enabled
    {
        get; set;
    }
    public SshClient Client
    {
        get; set;
    }
    public ShellStream Shell
    {
        get; set;
    }
    public Thread Thread
    {
        get; set;
    }
    public string IP
    {
        get; set;
    }
    public string Username
    {
        get; set;
    }
    public string Password
    {
        get; set;
    }

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
    private static MainPage main;
    public static bool Connect(this SSH ssh)
    {
        try
        {
            ssh.Client = new SshClient(ssh.IP, 22, ssh.Username, ssh.Password);
            ssh.Client.ConnectionInfo.Timeout = TimeSpan.FromMilliseconds(Convert.ToInt32("3000"));
            ssh.Client.Connect();
            ssh.Shell = ssh.Client.CreateShellStream("vt-100", 80, 60, 800, 600, 65536);

            Thread thread = new Thread(() => ssh.Receiver());
            thread.Start();
            return ssh.Client.IsConnected;
        }
        catch (Exception ex)
        {
            main.AddLog($"{ssh.Username}@{ssh.IP}: {ex.Message}");
            Console.WriteLine($"{ssh.Username}@{ssh.IP}: {ex.Message}"); return false;
        }
    }

    public static void SendCMD(this SSH ssh, string cmd, MainPage mainWindow)
    {
        main = mainWindow;
        try
        {
            if (TryConnect(ssh))
            {
                main.AddLog($"{ssh.Username}@{ssh.IP}: Connected Successfully");
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
                    main.AddLog($"{ssh.Username}@{ssh.IP}: {content}\n");
                }
            }
            catch { }
            Thread.Sleep(200);
        }
    }
    #endregion MainWindow Connection Logic
    #region TerminalWindow Connection Logic
    public static bool TryConnect(this SSH ssh, TerminalPage window)
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
    public static bool Connect(this SSH ssh, TerminalPage window)
    {
        try
        {
            ssh.Client = new SshClient(ssh.IP, 22, ssh.Username, ssh.Password);
            ssh.Client.ConnectionInfo.Timeout = TimeSpan.FromMilliseconds(Convert.ToInt32("3000"));
            ssh.Client.Connect();
            ssh.Shell = ssh.Client.CreateShellStream("vt-100", 80, 60, 800, 600, 65536);

            Thread thread = new Thread(() => ssh.Receiver(window));
            thread.Start();
            return ssh.Client.IsConnected;
        }
        catch (Exception ex)
        {
            window.SessionResult($"{ssh.Username}@{ssh.IP}: {ex.Message}");
            Console.WriteLine($"{ssh.Username}@{ssh.IP}: {ex.Message}"); return false;
        }
    }
    public static void SendCMD(this SSH ssh, string cmd, TerminalPage termWindow)
    {
        TerminalPage term = termWindow;
        try
        {
            if (TryConnect(ssh))
            {
                ssh.Shell.Write(cmd + "\n");
                ssh.Shell.Flush();
            }
            else
            {
                term.SessionResult($"{ssh.Username}@{ssh.IP}: Disconnected Unexpectadly");
            }
        }
        catch (Exception ex)
        {
            term.SessionResult($"{ssh.Username}@{ssh.IP}: {ex.Message}");
            Console.WriteLine($"{ssh.Username}@{ssh.IP}: {ex.Message}");
        }
    }

    public static void Receiver(this SSH ssh, TerminalPage window)
    {
        while (true)
        {
            try
            {
                if (ssh.Shell != null && ssh.Shell.DataAvailable)
                {
                    string content = ssh.Shell.Read();
                    Console.WriteLine($"{ssh.Username}@{ssh.IP}:", content);
                    window.AddResult($"{ssh.Username}@{ssh.IP}> {content}");
                }
            }
            catch { }
            Thread.Sleep(200);
        }
    }
    #endregion TerminalWindow Connection Logic
}
