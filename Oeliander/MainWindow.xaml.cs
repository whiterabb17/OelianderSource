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
        public void addTargetNum(int _num)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    targetNum.Text = Convert.ToString(_num);
                });
            }
            catch (Exception E)
            {
                helperObject.HandleException(E);
            }
        }
        public static List<CollectionListing> _collectionList = new List<CollectionListing>();
        public class CollectionListing
        {
            public int item { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string IPAddress { get; set; }
            // public string Threat { get; set; }
        }
        private static int _tabulation = 0;
        public void addCred(List<User> _uList, string _ip)
        {
            try
            {
                foreach (User _collected in _uList)
                {
                    _tabulation++;
                    _collectionList.Add(new CollectionListing()
                    {
                        item = _tabulation,
                        Username = _collected.Username,
                        Password = _collected.Password,
                        IPAddress = _ip
                    });
                }
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
            GC.Collect();
            GC.WaitForPendingFinalizers();
            if (useShodanScan)
                helperObject.Start(useShodanScan, routerVersion.SelectedItem.ToString());
            else
            {
                if (_targetingString == "")
                    helperObject.Start(useShodanScan, targetIp.Text);
                else
                    helperObject.Start(useShodanScan, _targetingString);
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
        private void Label_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Environment.Exit(0);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void shodanScan_Click(object sender, RoutedEventArgs e)
        {
            if (!useShodanScan)
                useShodanScan = true;
        }

        private void manualScan_Click(object sender, RoutedEventArgs e)
        {
            if (useShodanScan)
                useShodanScan = false;
        }

        private void selectList_Click(object sender, RoutedEventArgs e)
        {
            using (OpenFileDialog op = new OpenFileDialog())
            {
                op.RestoreDirectory = true;
                op.ShowDialog();
                _targetingString = op.FileName;
                ipList.Text = op.FileName;
            }
        }

        private void timeOutTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _Settings._Timeout = timeOutTextBox.Text;
        }

        private void apiKey_TextChanged(object sender, TextChangedEventArgs e)
        {
            _Settings._Key = apiKey.Text;
        }
    }
}