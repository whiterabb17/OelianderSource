using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Oeliander
{
    public partial class TerminalWindow : Window
    {
        #region locals

        public NewHelper helperObject { get; set; }
        public List<string> collectedCredentials = new List<string>();
        public List<string> rosVersion = new List<string>();
        public static Dictionary<User, string> _staticList = new Dictionary<User, string>();

        #endregion locals

        public void AddResult(string text, object obj)
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
        
        public static List<CollectionListing> _collectionList = new List<CollectionListing>();

        public void AddResult(string text)
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
        public void SessionResult(string text)
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
                Dispatcher.Invoke(() => { logRichTextBox.AppendText($"[!] {helperObject.GetTime()}: Connection to {ip} failed" + Environment.NewLine); });
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
                    ssh.SendCMD(commandText.Text.Trim().Replace("> ", ""), this);
                    Dispatcher.Invoke(() =>
                    {
                        commandText.Text = "> ";
                    });
                }
            }
        }

        private void userGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            CollectionListing selectedItem = (CollectionListing)userGrid.CurrentItem;
            if (selectedItem != null)
            {
                Dispatcher.Invoke(() => { connectionString.Text = $"{selectedItem.Username}@{selectedItem.IPAddress}:{selectedItem.Password}"; });
            }
        }
    }
}