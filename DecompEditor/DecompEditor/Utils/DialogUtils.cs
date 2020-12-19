using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace DecompEditor.Utils {
  public static class DialogUtils {
    /// Try to find the visual parent of the specified type.
    internal static T FindVisualParent<T>(this DependencyObject childElement) where T : DependencyObject {
      DependencyObject parent = VisualTreeHelper.GetParent(childElement);
      if (parent == null)
        return null;
      else if (parent is T parentAsT)
        return parentAsT;
      return FindVisualParent<T>(parent);
    }
  }
  public class ImageConverter : IValueConverter {
    public object Convert(object value, Type targetType,
        object parameter, System.Globalization.CultureInfo culture) {
      if (value == null || string.IsNullOrEmpty(value.ToString()))
        return null;
      return FileUtils.loadBitmapImage((string)value);
    }

    public object ConvertBack(object value, Type targetType,
        object parameter, System.Globalization.CultureInfo culture) => throw new NotImplementedException("Un-expected two way conversion.");
  }
}
