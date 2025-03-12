using System.Windows;
using System.Windows.Controls;

using OelianderUI.Contracts.Views;

using MahApps.Metro.Controls;

namespace OelianderUI.Views;

public partial class ShellDialogWindow : MetroWindow, IShellDialogWindow
{
    public string DialogHeadingString { get; set; }
    public string DialogTextString { get; set; }
    private readonly string ExclamationGlyph = "\uE783";
    private readonly string WarningGlyph = "\uEB56";

    public ShellDialogWindow(string heading, string text, bool warning)
    {
        InitializeComponent();
        DataContext = this;
        DialogHeadingString = heading;
        DialogTextString = text;
        DialogIcon.Glyph = warning ? WarningGlyph : ExclamationGlyph;
    }

    public Frame GetDialogFrame()
        => dialogFrame;

    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }
}
