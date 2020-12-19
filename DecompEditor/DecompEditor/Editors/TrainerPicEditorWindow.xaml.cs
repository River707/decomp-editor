using DecompEditor.Utils;
using DecompEditor.Views;
using System.ComponentModel;
using System.Windows;

namespace DecompEditor.Editors {
  /// <summary>
  /// Interaction logic for TrainerPicEditorWindow.xaml
  /// </summary>
  public partial class TrainerPicEditorWindow : Window {
    public TrainerPicEditorViewModel ViewModel => DataContext as TrainerPicEditorViewModel;
    public TrainerPicEditorWindow(TrainerPic initialPic) {
      InitializeComponent();
      ViewModel.CurrentPic = initialPic;

      var identifierSortDesc = new SortDescription("Identifier", ListSortDirection.Ascending);
      picList.Items.SortDescriptions.Add(identifierSortDesc);
      picList.Items.IsLiveSorting = true;
    }

    private string TrainerPicDir => System.IO.Path.Combine(Project.Instance.ProjectDir, "graphics", "trainers", "front_pics");

    private void uploadNewPicButton_Click(object sender, RoutedEventArgs e) {
      if (FileUtils.uploadImage(out string newPath, TrainerPicDir)) {
        ViewModel.CurrentPic.FullPath = newPath;

        // Compute size for the uncompressed size.
        System.Drawing.Bitmap image = FileUtils.loadBitmap(newPath);
        ViewModel.CurrentPic.UncompressedSize = image.Width * image.Height;
      }
    }

    private void addPicButton_Click(object sender, RoutedEventArgs e) {
      if (!FileUtils.uploadImage(out string path, TrainerPicDir))
        return;
      string identifier = "__new_pic_" + Project.Instance.Trainers.FrontPics.Count;
      System.Drawing.Bitmap image = FileUtils.loadBitmap(path);
      var newPic = new TrainerPic() {
        Identifier = identifier,
        PalettePath = identifier,
        Path = identifier,
        FullPath = path,
        UncompressedSize = image.Width * image.Height
      };
      Project.Instance.Trainers.addFrontPic(newPic);
      ViewModel.CurrentPic = newPic;
    }
  }
}
