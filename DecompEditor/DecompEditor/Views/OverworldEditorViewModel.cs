using GalaSoft.MvvmLight;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DecompEditor.Views {
  public class OverworldEditorViewModel : ViewModelBase {
    public OverworldEditorViewModel() {
      Project.Instance.Loaded += () => {
        CurrentObject = null;
        RaisePropertyChanged(string.Empty);
      };
    }

    /// <summary>
    /// The set of animation tables within the project.
    /// </summary>
    public IEnumerable<EventObjectAnimTable> AnimTables => Project.Instance.EventObjects.AnimTables
      .Select(x => x);

    /// <summary>
    /// The set of event objects within the project.
    /// </summary>
    public ObservableCollection<EventObject> Objects => Project.Instance.EventObjects.Objects;

    /// <summary>
    /// The set of shadow sizes within the project.
    /// </summary>
    public IEnumerable<string> ShadowSizes => Project.Instance.EventObjects.ShadowSizes;

    /// <summary>
    /// The set of sprite pics within the project.
    /// </summary>
    public ObservableCollection<EventObjectPic> SpritePics => Project.Instance.EventObjects.Pics;

    /// <summary>
    /// The set of shadow sizes within the project.
    /// </summary>
    public IEnumerable<string> TrackTypes => Project.Instance.EventObjects.TrackTypes;

    /// <summary>
    /// The current index within the animation table.
    /// </summary>
    public int AnimTableIndex {
      get => animTableIndex;
      set {
        Set(ref animTableIndex, value);
        RaisePropertyChanged("CurrentFrame");
      }
    }
    /// <summary>
    /// The currently selected frame within the event object.
    /// </summary>
    public EventObject.Frame CurrentFrame {
      get {
        if (CurrentObject == null)
          return null;
        ObservableCollection<EventObject.Frame> frames = CurrentObject.Frames;
        return AnimTableIndex >= frames.Count ? null : frames[AnimTableIndex];
      }
    }

    /// <summary>
    /// The currently selected event object.
    /// </summary>
    EventObject currentObject;
    private int animTableIndex = 0;

    public EventObject CurrentObject {
      get => currentObject;
      set {
        Set(ref currentObject, value);
        RaisePropertyChanged("ObjectIsSelected");
        AnimTableIndex = 0;
      }
    }
    public bool ObjectIsSelected => currentObject != null;
  }
}
