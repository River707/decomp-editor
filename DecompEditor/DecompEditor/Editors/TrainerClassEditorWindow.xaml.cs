using DecompEditor.Views;
using System.ComponentModel;
using System.Windows;

namespace DecompEditor.Editors {
  /// <summary>
  /// Interaction logic for TrainerClassEditorWindow.xaml
  /// </summary>
  public partial class TrainerClassEditorWindow : Window {
    public TrainerClassEditorViewModel ViewModel => DataContext as TrainerClassEditorViewModel;

    public TrainerClassEditorWindow(TrainerClass initialClass) {
      InitializeComponent();
      ViewModel.CurrentClass = initialClass;
      className.MaxLength = Project.Instance.Trainers.MaxClassNameLen;

      classList.Items.SortDescriptions.Add(new SortDescription("Identifier", ListSortDirection.Ascending));
      classList.Items.IsLiveSorting = true;
    }

    private void addClassButton_Click(object sender, RoutedEventArgs e) => ViewModel.addClass();
  }
}
