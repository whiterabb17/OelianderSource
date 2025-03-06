using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using OelianderUI.Helpers;

namespace OelianderUI.Views;

public partial class TerminalPage : Page, INotifyPropertyChanged
{
    #region locals

    public ScanHelper helperObject
    {
        get; set;
    }
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
                //logRichTextBox.AppendText(text + Environment.NewLine);
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
            //Dispatcher.Invoke(() => { logRichTextBox.AppendText(text + Environment.NewLine); });
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
            //Dispatcher.Invoke(() => { connectionString.Text = ""; connectionString.Text = text; });
        }
        catch (Exception E)
        {
            helperObject.HandleException(E);
        }
    }

    private void FillConnectionList(List<CollectionListing> _collectionList)
    {
        Dispatcher.Invoke(() =>
        {
            //userGrid.ItemsSource = null;
            //userGrid.ItemsSource = _collectionList;
            //userGrid.Items.Refresh();
        });
    }
    public TerminalPage()
    {
        InitializeComponent();
        DataContext = this;
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
}
