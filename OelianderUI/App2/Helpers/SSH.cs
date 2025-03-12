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
    public static bool TryConnect(this SSH ssh, int window)
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
    private static MainPage main;
    private static TerminalPage term;
    public static bool Connect(this SSH ssh, int window)
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
            switch (window)
            {
                case 0:
                    main.AddLog($"{ssh.Username}@{ssh.IP}: {ex.Message}");
                    break;
                case 1:
                    term.SessionResult($"{ssh.Username}@{ssh.IP}: {ex.Message}");
                    break;
            }
            return false;
        }
    }

    public static void SendCMD(this SSH ssh, string cmd, MainPage mainWindow)
    {
        main = mainWindow;
        try
        {
            if (TryConnect(ssh, 0))
            {
                //main.AddLog($"{ssh.Username}@{ssh.IP}: Connected Successfully");
                ssh.Shell.Write(cmd + "\n");
                ssh.Shell.Flush();
            }
        }
        catch (Exception ex) 
        { 
            Console.WriteLine($"{ssh.Username}@{ssh.IP}: {ex.Message}");
            main.AddLog($"{ssh.Username}@{ssh.IP}: {ex.Message}");
        }
    }

    public static void Receiver(this SSH ssh, int window)
    {
        while (true)
        {
            try
            {
                if (ssh.Shell != null && ssh.Shell.DataAvailable)
                {
                    var content = ssh.Shell.Read();
                    switch (window)
                    {
                        case 0:
                            main.AddLog($"{ssh.Username}@{ssh.IP}: {content}\n");
                            if (ScanHelper.carryon == 1)
                            {
                                if (content.Contains("error"))
                                    ScanHelper.carryon = 2;
                                else
                                    ScanHelper.carryon = 0;
                            }
                            break;
                        case 1:
                            term.AddResult($"{ssh.Username}@{ssh.IP}: {content}");
                            break;
                    }                    
                }
            }
            catch { }
            Thread.Sleep(200);
            
        }
    }
    #endregion MainWindow Connection Logic
    #region TerminalWindow Connection Logic
    public static void SendCMD(this SSH ssh, string cmd, TerminalPage termWindow)
    {
        term = termWindow;
        try
        {
            if (TryConnect(ssh, 1))
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
    #endregion TerminalWindow Connection Logic
}
