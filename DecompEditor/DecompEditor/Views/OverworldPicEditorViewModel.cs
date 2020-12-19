using DecompEditor.Utils;
using GalaSoft.MvvmLight;

namespace DecompEditor.Views {
  public class OverworldPicEditorViewModel : ViewModelBase {
    EventObjectPic currentPic;

    public EventObjectPic CurrentPic {
      get => currentPic;
      set {
        Set(ref currentPic, value);
        RaisePropertyChanged("PicIsSelected");
      }
    }
    public bool PicIsSelected => currentPic != null;

    /// <summary>
    /// The set of sprite pics within the project.
    /// </summary>
    public ObservableCollection<EventObjectPic> SpritePics => Project.Instance.EventObjects.Pics;
  }
}
