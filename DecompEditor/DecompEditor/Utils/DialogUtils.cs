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
    /// Try to find the visual parent of the specified type.
    internal static T FindVisualChild<T>(this DependencyObject parentElement) where T : DependencyObject {
      int count = VisualTreeHelper.GetChildrenCount(parentElement);
      for (int i = 0; i < count; i++) {
        DependencyObject child = VisualTreeHelper.GetChild(parentElement, i);
        IInputElement inputElement = child as IInputElement;
        if (null != inputElement && inputElement is T childAsT)
          return childAsT;
        child = FindVisualChild<T>(child);
        if (child != null)
          return child as T;
      }
      return null;
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
