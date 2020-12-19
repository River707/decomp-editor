using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;

namespace DecompEditor.Views {
  public class ViewModelLocator {
    public ViewModelLocator() {
      ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

      SimpleIoc.Default.Register<MainViewModel>();
      SimpleIoc.Default.Register<AdventureEditorViewModel>();
      SimpleIoc.Default.Register<OverworldEditorViewModel>();
      SimpleIoc.Default.Register<OverworldPicEditorViewModel>();
      SimpleIoc.Default.Register<TrainerClassEditorViewModel>();
      SimpleIoc.Default.Register<TrainerEditorViewModel>();
      SimpleIoc.Default.Register<TrainerPicEditorViewModel>();
    }

    public static MainViewModel Main => SimpleIoc.Default.GetInstance<MainViewModel>();
    public static AdventureEditorViewModel AdventureEditor => SimpleIoc.Default.GetInstance<AdventureEditorViewModel>();
    public static OverworldEditorViewModel OverworldEditor => SimpleIoc.Default.GetInstance<OverworldEditorViewModel>();
    public static OverworldPicEditorViewModel OverworldPicEditor => SimpleIoc.Default.GetInstance<OverworldPicEditorViewModel>();
    public static TrainerClassEditorViewModel TrainerClassEditor => SimpleIoc.Default.GetInstance<TrainerClassEditorViewModel>();
    public static TrainerEditorViewModel TrainerEditor => SimpleIoc.Default.GetInstance<TrainerEditorViewModel>();
    public static TrainerPicEditorViewModel TrainerPicEditor => SimpleIoc.Default.GetInstance<TrainerPicEditorViewModel>();
  }
}
