using DecompEditor.Utils;
using DecompEditor.Views;
using System.ComponentModel;
using System.Windows;

namespace DecompEditor.Editors {
  /// <summary>
  /// Interaction logic for OverworldPicEditorWindow.xaml
  /// </summary>
  public partial class OverworldPicEditorWindow : Window {
    public OverworldPicEditorViewModel ViewModel => DataContext as OverworldPicEditorViewModel;
    public OverworldPicEditorWindow(EventObjectPic initialPic) {
      InitializeComponent();
      ViewModel.CurrentPic = initialPic;

      var identifierSortDesc = new SortDescription("Identifier", ListSortDirection.Ascending);
      spritePicList.Items.SortDescriptions.Add(identifierSortDesc);
      spritePicList.Items.IsLiveSorting = true;
    }

    private string EventPicDir => System.IO.Path.Combine(Project.Instance.ProjectDir, "graphics", "object_events", "pics");

    private void uploadNewPicButton_Click(object sender, RoutedEventArgs e) {
      if (FileUtils.uploadImage(out string newPath, EventPicDir))
        ViewModel.CurrentPic.FullPath = newPath;
    }

    private void addPicButton_Click(object sender, RoutedEventArgs e) {
      if (!FileUtils.uploadImage(out string path, EventPicDir))
        return;
      ObservableCollection<EventObjectPic> pics = Project.Instance.EventObjects.Pics;
      string identifier = "__new_pic_" + pics.Count;
      var newPic = new EventObjectPic() {
        Identifier = identifier,
        Path = identifier,
        FullPath = path,
      };
      pics.Add(newPic);
      ViewModel.CurrentPic = newPic;
    }
  }
}
