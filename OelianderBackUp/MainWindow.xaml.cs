using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace Oeliander
{
    public partial class MainWindow : Window
    {
        #region locals

        public newHelper helperObject { get; set; }
        public List<string> collectedCredentials = new List<string>();
        public List<string> rosVersion = new List<string>();
        public static Dictionary<User, string> _staticList = new Dictionary<User, string>();
        public static bool SaveShodanResults = false;

        #endregion locals

        public void addLog(string text, object obj)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    logRichTextBox.AppendText(text+ Environment.NewLine);
                   // collectedCredentials.Add(text); // + Environment.NewLine);
                    //logRichTextBox.DataContext = collectedCredentials;
                  //  resList.ItemsSource = collectedCredentials;
                  //  resList.Items.Refresh();
                });
            }
            catch (Exception E)
            {
                helperObject.HandleException(E);
            }
        }
        public void addToLogFile(string text)
        {
            if (!File.Exists("Oeliander.log")) { File.Create("Oeliander.log"); }
            Thread.Sleep(1000);
            try { Dispatcher.Invoke(() => System.IO.File.AppendAllText("Oeliander.log", text + Environment.NewLine)); }
            catch (Exception ex) { logRichTextBox.AppendText(ex.Message); }
        }
        public void addTargetNum(int _num)
        {
            try { Dispatcher.Invoke(() => { targetNum.Text = Convert.ToString(_num); }); }
            catch (Exception E) { helperObject.HandleException(E); }
        }
        public static List<CollectionListing> _collectionList = new List<CollectionListing>();
        
        private static int _tabulation = 0;
        public void ScanStop()
        {
            Dispatcher.Invoke(() =>
            {
                StartButton.Content = "Start"; 
                addLog(Environment.NewLine + helperObject.GetTime() + ": Scan stopped successfully\n");
                addToLogFile("\n\n\t[*] End of Scan: " + helperObject.GetTime() + "\n\n###############################################################################\n\n");
            });
        }
        public void FillList()
        {
            Dispatcher.Invoke(() =>
            {
                userGrid.ItemsSource = null;
                userGrid.ItemsSource = _collectionList;
                userGrid.Items.Refresh();
                //collectedCredentials.Add(text + Environment.NewLine);
                // logRichTextBox.DataContext = collectedCredentials;
                // resList.ItemsSource = collectedCredentials;
                // resList.Items.Refresh();
            });
        }
        public void addCred(User _uList, string _ip, bool status = false)
        {
            try
            {
                _tabulation++;
                _collectionList.Add(new CollectionListing()
                {
                    Num = _tabulation,
                    Username = _uList.Username,
                    Password = _uList.Password,
                    IPAddress = _ip,
                    Status = status? "Authenticated" : "Unauthenticated"
                });
                Console.Write(_collectionList.ToList());
                List<CollectionListing> _finalList = _collectionList.Distinct().ToList();
                Console.Write(_finalList);
                Console.WriteLine();
            }
            catch (Exception E)
            {
                helperObject.HandleException(E);
            }
        }
        public void addCred(List<User> _uList, string _ip, string status = "Unauthenticated")
        {
            try
            {
                foreach (User _collected in _uList)
                {
                    _tabulation++;
                    _collectionList.Add(new CollectionListing()
                    {
                        Num = _tabulation,
                        Username = _collected.Username,
                        Password = _collected.Password,
                        IPAddress = _ip,
                        Status = status
                    });
                }
                Console.Write(_collectionList.ToList());
                List<CollectionListing> _finalList = _collectionList.Distinct().ToList();
                Console.Write(_finalList);
                Console.WriteLine();
                Dispatcher.Invoke(() =>
                {
                    userGrid.ItemsSource = null;
                    userGrid.ItemsSource = _collectionList;
                    userGrid.Items.Refresh();
                    //collectedCredentials.Add(text + Environment.NewLine);
                    // logRichTextBox.DataContext = collectedCredentials;
                    // resList.ItemsSource = collectedCredentials;
                    // resList.Items.Refresh();
                });
            }
            catch (Exception E)
            {
                helperObject.HandleException(E);
            }
        }
        public void addLog(string text)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    logRichTextBox.AppendText(text + Environment.NewLine);
                });
            }
            catch (Exception E)
            {
                helperObject.HandleException(E);
            }
        }
        public MainWindow()
        {
            InitializeComponent();
            Settings.LoadSettings();
            rosVersion = new List<string>()
            {
                "6.30",
                "6.31",
                "6.32"
            };
            loadOsVersions();
            helperObject = new newHelper(addLog, false, this)
            {
                debugMod = false
            };
            manualScan.IsChecked = true;
            shodanScan.IsChecked = false;
        }
        private void loadOsVersions()
        {
            try
            {
                apiKey.Text = _Settings._Key;
                routerVersion.ItemsSource = rosVersion;
                routerVersion.Items.Refresh();
            }
            catch (Exception E)
            {
                helperObject.HandleException(E);
            }
        }
        private void Window_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch
            {
            }
        }
        private static bool useShodanScan = false;
        private static string _targetingString = "";
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (StartButton.Content.ToString() == "Start")
            {
                helperObject.SaveScanTime();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                if (useShodanScan)
                    helperObject.Start(useShodanScan, routerVersion.SelectedItem.ToString());
                else
                {
                    if (_targetingString == "")
                        helperObject.Start(useShodanScan, ipList.Text);
                    else
                        helperObject.Start(useShodanScan, _targetingString);
                }
            }
            else if (StartButton.Content.ToString() == "Stop")
            {
                helperObject.Stop();
            }
        }

        private void Label_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Environment.Exit(0);
        }

        private void Label_MouseDoubleClick_1(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void logRichTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            try
            {
                logRichTextBox.ScrollToEnd();
            }
            catch (Exception E)
            {
                helperObject.HandleException(E);
            }
        }
        /*
        private static string range;
        public void ExecuteCommandLine(string fullCommandLine,
                                string workingFolder = null,
                                int waitForExitMs = 0,
                                string verb = "OPEN",
                                ProcessWindowStyle windowStyle = ProcessWindowStyle.Normal)
        {
            string executable = fullCommandLine;
            string args = null;

            if (executable.StartsWith("\""))
            {
                int at = executable.IndexOf("\" ");
                if (at > 0)
                {
                    args = executable.Substring(at + 1).Trim();
                    executable = executable.Substring(0, at);
                }
            }
            else
            {
                int at = executable.IndexOf(" ");
                if (at > 0)
                {

                    if (executable.Length > at + 1)
                        args = executable.Substring(at + 1).Trim();
                    executable = executable.Substring(0, at);
                }
            }

            var pi = new ProcessStartInfo();
            //pi.UseShellExecute = true;
            pi.Verb = verb;
            pi.WindowStyle = windowStyle;

            pi.FileName = executable;
            pi.WorkingDirectory = workingFolder;
            pi.Arguments = args;


            using (var p = Process.Start(pi))
            {
                if (waitForExitMs > 0)
                {
                    if (!p.WaitForExit(waitForExitMs))
                        throw new TimeoutException("Process failed to complete in time.");
                }
            }
        }
        */

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void shodanScan_Click(object sender, RoutedEventArgs e)
        {
            if (!useShodanScan)
                useShodanScan = true;
            Console.WriteLine("Using Shodan to Scan for Targets");
        }

        private void manualScan_Click(object sender, RoutedEventArgs e)
        {
            if (useShodanScan)
                useShodanScan = false;
            Console.WriteLine("Not using ShodanAPI");
        }

        private void selectList_Click(object sender, RoutedEventArgs e)
        {
            using (OpenFileDialog op = new OpenFileDialog())
            {
                op.RestoreDirectory = true;
                op.ShowDialog();
                _targetingString = op.FileName;
                ipList.Text = Path.GetFileName(op.FileName);
            }
        }
        private void OpenTerminalWindow(object sender, RoutedEventArgs e)
        {
            var termWindow = new TerminalWindow(_collectionList);
            termWindow.Show();
        }
        private void GetLogs(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer", "Oeliander.log");
        }
        private void CheckResults(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer", ".\\Results\\Winbox");
        }
        private void ClearLogs(object sender, RoutedEventArgs e)
        {
            File.WriteAllText("Oeliander.log", "");
        }
        private void timeOutTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _Settings._Timeout = timeOutTextBox.Text;
        }

        private void apiKey_TextChanged(object sender, TextChangedEventArgs e)
        {
            _Settings._Key = apiKey.Text;
        }

        private void logRichTextBox_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!Directory.Exists("Results\\Winbox"))
                ResultButton.IsEnabled = false;
        }

        private void manualScan_Copy_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}