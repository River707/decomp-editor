using GalaSoft.MvvmLight;
using System.Collections.Generic;
using System.Linq;

namespace DecompEditor.Views {
  public class TrainerClassEditorViewModel : ViewModelBase {
    public TrainerEditorViewModel EditorViewModel { get; set; }

    public TrainerClassEditorViewModel() => EditorViewModel = ViewModelLocator.TrainerEditor;

    public IEnumerable<Item> Pokeballs => Project.Instance.Items.Items.OrderBy(item => item.Name).Where(item => item.Pocket == "POCKET_POKE_BALLS");

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
        MoneyFactor = 5,
        IVs = 10,
        Pokeball = Project.Instance.Items.getFromId("ITEM_POKE_BALL")
      };
      Project.Instance.Trainers.addClass(newClass);
      EditorViewModel.RaisePropertyChanged("CanAddClass");
      CurrentClass = newClass;
    }
  }
}
