using System.Windows;
using System.Windows.Controls;

namespace CleanArchitecture.Presentation.Wpf.Views;
public partial class TodosView : UserControl
{
    public TodosView() => InitializeComponent();
    private void TextBox_Loaded(object sender, RoutedEventArgs e)
    {
        var tb = sender as TextBox;
        if (tb == null) return;
        if (tb.Visibility == Visibility.Visible)
        {
            tb.Focus();
            tb.SelectAll();
        }
    }
}

