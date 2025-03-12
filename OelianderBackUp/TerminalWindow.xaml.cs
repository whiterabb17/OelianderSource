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
using static System.Net.Mime.MediaTypeNames;

namespace Oeliander
{
    public partial class TerminalWindow : Window
    {
        #region locals

        public newHelper helperObject { get; set; }
        public List<string> collectedCredentials = new List<string>();
        public List<string> rosVersion = new List<string>();
        public static Dictionary<User, string> _staticList = new Dictionary<User, string>();

        #endregion locals

        public void addResult(string text, object obj)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    logRichTextBox.AppendText(text+ Environment.NewLine);
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
        
        public static List<CollectionListing> _collectionList = new List<CollectionListing>();

        public void addResult(string text)
        {
            try
            {
                Dispatcher.Invoke(() => { logRichTextBox.AppendText(text + Environment.NewLine); });
            }
            catch (Exception E)
            {
                helperObject.HandleException(E);
            }
        }
        public void sessionResult(string text)
        {
            try
            {
                Dispatcher.Invoke(() => { connectionString.Text = ""; connectionString.Text = text; });
            }
            catch (Exception E)
            {
                helperObject.HandleException(E);
            }
        }

        public TerminalWindow(List<CollectionListing> connectionList)
        {
            InitializeComponent();
            FillConnectionList(connectionList);
        }
        private void FillConnectionList(List<CollectionListing> _collectionList)
        {
            Dispatcher.Invoke(() =>
            {
                userGrid.ItemsSource = null;
                userGrid.ItemsSource = _collectionList;
                userGrid.Items.Refresh();
            });
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
        private static SSH ssh;
        public void StartSSHConnection(string ip, User user)
        {
            ssh = new SSH(ip, user.Username, user.Password);
            if (ssh.TryConnect(this))
            {
                ssh.SendCMD("whoami", this);
            }
            else
            {
                Dispatcher.Invoke(() => { logRichTextBox.AppendText($"Connection to {ip} failed" + Environment.NewLine); });
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string args = connectionString.Text.Trim();
            User User = new User(args.Split('@')[0], args.Split(':')[1]);
            var a = args.Split('@')[1];
            var ip = a.Split(':')[0];
            StartSSHConnection(ip, User);
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

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void commandText_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (ssh.TryConnect())
                {
                    ssh.SendCMD(commandText.Text.Trim(), this);
                    Dispatcher.Invoke(() =>
                    {
                        commandText.Text = "";
                    });
                }
            }
        }
    }
}