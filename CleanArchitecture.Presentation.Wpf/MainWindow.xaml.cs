using CleanArchitecture.Presentation.Wpf.Views;
using System.Windows;

namespace CleanArchitecture.Presentation.Wpf;

/// <summary>
/// Shell window: keeps window-level concerns (menus, navigation, theme), and
/// hosts feature views (here: TodosView). No domain/persistence logic here.
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow() => InitializeComponent();
}
