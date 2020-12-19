using DecompEditor.ProjectData;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DecompEditor.Views {
  public class AdventureEditorViewModel : ViewModelBase {
    public AdventureEditorViewModel() {
      Project.Instance.Loaded += () => {
        CurrentAdventure = null;
        CurrentTask = null;
        RaisePropertyChanged(string.Empty);
      };
    }

    /// <summary>
    /// The set of adventures within the project.
    /// </summary>
    public IEnumerable<Adventure> Adventures => Project.Instance.Adventures.Adventures;

    /// <summary>
    /// The set of all tasks within the project.
    /// </summary>
    public IEnumerable<AdventureTask> Tasks => Adventures.SelectMany(adventure => adventure.Tasks);


    /// <summary>
    /// The set of possible values for a task condition.
    /// </summary>
    public IEnumerable<AdventureTaskTaskCondition.ValueStates> TaskConditionValues => Enum.GetValues(typeof(AdventureTaskTaskCondition.ValueStates)).Cast<AdventureTaskTaskCondition.ValueStates>();

    /// <summary>
    /// The currently selected adventure.
    /// </summary>
    Adventure currentAdventure;
    public Adventure CurrentAdventure {
      get => currentAdventure;
      set {
        Set(ref currentAdventure, value);
        RaisePropertyChanged("AdventureIsSelected");
        RaisePropertyChanged("TaskIsSelected");
      }
    }
    public bool AdventureIsSelected => currentAdventure != null;

    /// <summary>
    /// The currently selected task in the adventure.
    /// </summary>
    AdventureTask currentTask;
    public AdventureTask CurrentTask {
      get => currentTask;
      set {
        Set(ref currentTask, value);
        RaisePropertyChanged("TaskIsSelected");
      }
    }
    public bool TaskIsSelected => currentTask != null;
  }
}
