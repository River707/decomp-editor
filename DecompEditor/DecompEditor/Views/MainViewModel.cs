using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System.Windows;

namespace DecompEditor.Views {
  public class MainViewModel : ViewModelBase {
    /// Commands.
    public RelayCommand OpenCommand { get; private set; }
    public RelayCommand ReloadCommand { get; private set; }
    public RelayCommand SaveCommand { get; private set; }

    public MainViewModel() {
      OpenCommand = new RelayCommand(() => open());
      ReloadCommand = new RelayCommand(() => reload());
      SaveCommand = new RelayCommand(() => save());
    }

    /// <summary>
    /// Open a new project.
    /// </summary>
    void open() {
      var folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
      if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        loadProject(folderBrowserDialog.SelectedPath);
    }

    /// <summary>
    /// Reload the current project.
    /// </summary>
    void reload() => loadProject(Project.Instance.ProjectDir);

    /// <summary>
    /// Save the current project.
    /// </summary>
    void save() => Project.Instance.save();

    /// <summary>
    /// Load the project at the given directory.
    /// </summary>
    /// <param name="projectDir"></param>
    public void loadProject(string projectDir) {
      checkDirtyAndSave();

      Project.Instance.load(projectDir);

      // If loading was successful, save the current settings.
      Properties.Settings.Default.ProjectDir = projectDir;
      Properties.Settings.Default.Save();
    }

    /// <summary>
    /// Check the current state of the editors and ask if the user
    /// wants to save.
    /// </summary>
    public void checkDirtyAndSave() {
      if (!Project.Instance.IsDirty)
        return;
      MessageBoxResult result = MessageBox.Show("Would you like to save your current changes?",
                                   "Save Project Modifications", MessageBoxButton.YesNo);
      if (result == MessageBoxResult.Yes)
        save();
    }
  }
}
