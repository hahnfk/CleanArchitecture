namespace CleanArchitecture.Presentation.Wpf.Converters;

using CleanArchitecture.Presentation.Wpf.Models;
using System.Globalization;
using System.Windows.Data;

/// <summary>
/// Builds a RenameParam from ListViewItem.Id and TitleEditor.Text
/// </summary>
public sealed class RenameParamConverter : IMultiValueConverter
{
    public object Convert(object [] values, Type targetType, object parameter, CultureInfo culture)
    {
        var todoId = values [0]?.ToString() ?? "";
        var newTitle = values [1]?.ToString() ?? "";
        return new RenameParam(todoId, newTitle);
    }

    public object [] ConvertBack(object value, Type [] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
