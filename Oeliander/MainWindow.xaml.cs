using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace Oeliander
{
    public partial class MainWindow : Window
    {
        #region locals

        public NewHelper helperObject { get; set; }
        public List<string> collectedCredentials = new List<string>();
        public List<string> rosVersion = new List<string>();
        public static Dictionary<User, string> _staticList = new Dictionary<User, string>();
        public bool SaveShodanOnly = false;

        #endregion locals

        public void AddLog(string text, object obj)
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
        public void AddToLogFile(string text)
        {
            if (!File.Exists("Oeliander.log")) { File.Create("Oeliander.log"); }
            Thread.Sleep(1000);
            try { Dispatcher.Invoke(() => System.IO.File.AppendAllText("Oeliander.log", text + Environment.NewLine)); }
            catch (Exception ex) { logRichTextBox.AppendText(ex.Message); }
        }
        public void AddTargetNum(int _num)
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
                AddLog(Environment.NewLine + helperObject.GetTime() + ": Scan stopped successfully\n");
                AddToLogFile("\n\n\t[*] End of Scan: " + helperObject.GetTime() + "\n\n###############################################################################\n\n");
            });
        }
        public void FillList()
        {
            Dispatcher.Invoke(() =>
            {
                userGrid.ItemsSource = null;
                userGrid.ItemsSource = _collectionList;
                userGrid.Items.Refresh();
            });
        }
        public void AddCred(User _uList, string _ip, bool status = false)
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
        public void AddCred(List<User> _uList, string _ip, string status = "Unauthenticated")
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
                });
            }
            catch (Exception E)
            {
                helperObject.HandleException(E);
            }
        }
        public void AddLog(string text)
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

        internal static Settings settings = new Settings();
        public MainWindow()
        {
            InitializeComponent();
            settings.LoadSettings();
            rosVersion = new List<string>()
            {
                "6.30",
                "6.31",
                "6.32"
            };
            Dispatcher.Invoke(() =>
            {
                timeOutTextBox.Text = _Settings._Timeout;
                apiKey.Text = _Settings._Key;
            });
            LoadOsVersions();
            helperObject = new NewHelper(AddLog, false, this)
            {
                debugMod = false
            };
            manualScan.IsChecked = true;
            shodanScan.IsChecked = false;
        }
        private void LoadOsVersions()
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
            routerVersion.Visibility = Visibility.Visible;
            TargetOSLabel.Visibility = Visibility.Visible;
            apiKey.IsEnabled = true;
            ipList.IsEnabled = false;
            ShodanSaveOnlyChk.IsEnabled = true;
            ShodanSaveOnlyChk.Visibility = Visibility.Visible;
        }

        private void manualScan_Click(object sender, RoutedEventArgs e)
        {
            if (useShodanScan)
                useShodanScan = false;
            routerVersion.Visibility = Visibility.Hidden;
            TargetOSLabel.Visibility = Visibility.Hidden;
            apiKey.IsEnabled = false;
            ipList.IsEnabled = true;
            ShodanSaveOnlyChk.IsEnabled = false;
            ShodanSaveOnlyChk.Visibility = Visibility.Hidden;
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
            File.WriteAllText("Oeliander.log", "Oeliander Exploit Logs\n--------------------------------\n\n");
        }
        private void timeOutTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _Settings._Timeout = timeOutTextBox.Text;
            settings.Connection_Timeout = timeOutTextBox.Text;
            settings.SaveSettings();
        }

        private void apiKey_TextChanged(object sender, TextChangedEventArgs e)
        {
            _Settings._Key = apiKey.Text;
            settings.Shodan_API_Key = apiKey.Text;
            settings.SaveSettings();
        }

        private void logRichTextBox_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!Directory.Exists("Results\\Winbox"))
                ResultButton.IsEnabled = false;
        }

        private void ShodanSaveOnlyChk_Checked(object sender, RoutedEventArgs e)
        {
            if (ShodanSaveOnlyChk.IsChecked == true)
                SaveShodanOnly = true;
            else
                SaveShodanOnly = false;
        }
    }
}