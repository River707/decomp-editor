using DecompEditor.Views;
using System.Windows;

namespace DecompEditor {
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window {
    public MainWindow() {
      InitializeComponent();

      // Check the settings to see if the user already opened a project directory.
      if (Properties.Settings.Default.ProjectDir != "")
        ViewModel.loadProject(Properties.Settings.Default.ProjectDir);
    }

    public MainViewModel ViewModel => DataContext as MainViewModel;

    private void window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
      ViewModel.checkDirtyAndSave();
      NLog.LogManager.Shutdown();
    }
  }
}
