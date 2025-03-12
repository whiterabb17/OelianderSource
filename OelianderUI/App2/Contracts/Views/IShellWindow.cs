using System.Windows.Controls;

namespace OelianderUI.Contracts.Views;

public interface IShellWindow
{
    Frame GetNavigationFrame();

    void ShowWindow();

    void CloseWindow();
}
