using GalaSoft.MvvmLight;
using System.Linq;

namespace DecompEditor.Views {
  public class TrainerClassEditorViewModel : ViewModelBase {
    public TrainerEditorViewModel EditorViewModel { get; set; }

    public TrainerClassEditorViewModel() => EditorViewModel = ViewModelLocator.TrainerEditor;

    TrainerClass currentClass;
    public TrainerClass CurrentClass {
      get => currentClass;
      set {
        Set(ref currentClass, value);
        RaisePropertyChanged("ClassIsSelected");
      }
    }
    public bool ClassIsSelected => currentClass != null;

    public bool CanAddClass => EditorViewModel.TrainerClasses.Count() != Project.Instance.Trainers.MaxClassCount;

    internal void addClass() {
      var newClass = new TrainerClass() {
        Identifier = "CLASS_ID_" + EditorViewModel.TrainerClasses.Count(),
        Name = "Class Name",
        MoneyFactor = 5
      };
      Project.Instance.Trainers.addClass(newClass);
      EditorViewModel.RaisePropertyChanged("CanAddClass");
      CurrentClass = newClass;
    }
  }
}
