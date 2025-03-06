using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

using OelianderUI.Contracts.Views;
using OelianderUI.Core.Contracts.Services;
using OelianderUI.Core.Models;
using OelianderUI.Helpers;
using Microsoft.Win32;
using Windows.System;

namespace OelianderUI.Views;

public partial class MainPage : Page, INotifyPropertyChanged, INavigationAware
{
    private readonly IScanResultService _scanResults;
    internal static bool ShodanScan = false;
    internal static string _targetingString = "";
    public ObservableCollection<ScanResult> Source { get; } = new ObservableCollection<ScanResult>();
    #region locals

    public ScanHelper helperObject
    {
        get; set;
    }
    public List<string> collectedCredentials = new List<string>();
    public List<string> rosVersion = new List<string>();
    public static Dictionary<Helpers.User, string> _staticList = new Dictionary<Helpers.User, string>();
    public bool SaveShodanOnly = false; 
    private static int _tabulation = 0; 
    internal static Settings settings = new Settings();
    public static List<CollectionListing> _collectionList = new List<CollectionListing>();

    #endregion locals

    public void AddLog(string text)
    {
        try
        {
            Dispatcher.Invoke(() =>
            {
                LogBox.AppendText(text + Environment.NewLine);
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
        catch (Exception ex) { LogBox.AppendText(ex.Message); }
    }
    public void AddLog(string text, object obj)
    {
        try
        {
            Dispatcher.Invoke(() =>
            {
                LogBox.AppendText(text + Environment.NewLine);
            });
        }
        catch (Exception E)
        {
            helperObject.HandleException(E);
        }
    }
    public MainPage(IScanResultService scanResultService)
    {
        _scanResults = scanResultService;
        InitializeComponent();
        DataContext = this;
        rosVersion = new List<string>()
        {
            "6.30",
            "6.31",
            "6.32"
        };
        LoadOsVersions();
    }
    public void ScanStop()
    {
        Dispatcher.Invoke(() =>
        {
            StartScanButton.Content = "Start";
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
    public void AddCred(Helpers.User _uList, string _ip, bool status = false)
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
                Status = status ? "Authenticated" : "Unauthenticated"
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
    public void AddCred(List<Helpers.User> _uList, string _ip, string status = "Unauthenticated")
    {
        try
        {
            foreach (Helpers.User _collected in _uList)
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
    private void LoadOsVersions()
    {
        try
        {
            routerVersion.ItemsSource = rosVersion;
            routerVersion.Items.Refresh();
        }
        catch (Exception E)
        {
            helperObject.HandleException(E);
        }
    }

    public async void OnNavigatedTo(object parameter)
    {
        Source.Clear();

        // Replace this with your actual data
        var data = await _scanResults.GetGridDataAsync();

        foreach (var item in data)
        {
            Source.Add(item);
        }
    }

    public void OnNavigatedFrom()
    {
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void Set<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
    {
        if (Equals(storage, value))
        {
            return;
        }

        storage = value;
        OnPropertyChanged(propertyName);
    }

    private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private void ManualScanButton_Checked(object sender, System.Windows.RoutedEventArgs e)
    {
        if (ShodanScan == true)
        {
            ShodanScan = false;
            routerVersion.Visibility = System.Windows.Visibility.Hidden;
            ROSTextBlock.Visibility = System.Windows.Visibility.Hidden;
            TargetTextBox.IsEnabled = true;
        }
        //else
        //{
        //    ShodanScan = true;
        //    routerVersion.Visibility = System.Windows.Visibility.Visible;
        //    ROSTextBlock.Visibility = System.Windows.Visibility.Visible;
        //    TargetTextBox.IsEnabled = false;
        //}
    }

    private void ShodanScanButton_Checked(object sender, System.Windows.RoutedEventArgs e)
    {
        if (ShodanScan == false)
        {
            ShodanScan = true;
            routerVersion.Visibility = System.Windows.Visibility.Visible;
            ROSTextBlock.Visibility = System.Windows.Visibility.Visible;
            TargetTextBox.IsEnabled = false;
        }
        //else
        //{
        //    ShodanScan = false;
        //    routerVersion.Visibility = System.Windows.Visibility.Hidden;
        //    ROSTextBlock.Visibility = System.Windows.Visibility.Hidden;
        //    TargetTextBox.IsEnabled = true;
        //}
    }

    private void fileSelect_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        var op = new OpenFileDialog();
        op.RestoreDirectory = true;
        op.ShowDialog();
        TargetTextBox.Text = op.FileName;
        _targetingString = Path.GetFileName(op.FileName);
    }

    private void StartScan(object sender, System.Windows.RoutedEventArgs e)
    {
        if (StartScanButton.Content.ToString() == "Start")
        {
            helperObject.SaveScanTime();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            if (ShodanScan)
                helperObject.Start(ShodanScan, routerVersion.SelectedItem.ToString());
            else
            {
                if (_targetingString == "")
                    helperObject.Start(ShodanScan, TargetTextBox.Text);
                else
                    helperObject.Start(ShodanScan, _targetingString);
            }
        }
        else if (StartScanButton.Content.ToString() == "Stop")
        {
            helperObject.Stop();
        }

    }
}
