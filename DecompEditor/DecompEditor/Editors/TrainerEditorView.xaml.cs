using DecompEditor.Utils;
using DecompEditor.Views;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DecompEditor.Editors {
  /// <summary>
  /// Interaction logic for TrainerEditorView.xaml
  /// </summary>
  public partial class TrainerEditorView : UserControl {
    public TrainerEditorView() {
      InitializeComponent();

      var identifierSortDesc = new SortDescription("Identifier", ListSortDirection.Ascending);
      aiScripts.Items.SortDescriptions.Add(identifierSortDesc);
      classList.Items.SortDescriptions.Add(identifierSortDesc);
      classList.Items.IsLiveSorting = true;
      musicList.Items.SortDescriptions.Add(identifierSortDesc);
      picList.Items.SortDescriptions.Add(identifierSortDesc);
      picList.Items.IsLiveSorting = true;
      trainerList.Items.SortDescriptions.Add(identifierSortDesc);
      trainerList.Items.IsLiveSorting = true;
    }

    public TrainerEditorViewModel ViewModel => DataContext as TrainerEditorViewModel;

    private void trainerList_SelectionChanged(object sender, SelectionChangedEventArgs evt) => ViewModel.CurrentTrainer = trainerList.SelectedItem as Trainer;

    private void partyMenu_SelectionChanged(object sender, SelectionChangedEventArgs e) => ViewModel.CurrentPokemon = partyMenu.SelectedItem as Pokemon;

    private void trainerItem_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      if (ViewModel.CurrentTrainer == null)
        return;

      var itemCombo = sender as ComboBox;
      ListBoxItem parentControl = itemCombo.FindVisualParent<ListBoxItem>();
      ListBox parentList = parentControl.FindVisualParent<ListBox>();
      int itemIndex = parentList.ItemContainerGenerator.IndexFromContainer(parentControl);
      ViewModel.CurrentTrainer.Items[itemIndex] = itemCombo.SelectedItem as Item;
    }
    private void partyMove_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      if (ViewModel.CurrentPokemon == null)
        return;

      var moveCombo = sender as ComboBox;
      ListBoxItem parentControl = moveCombo.FindVisualParent<ListBoxItem>();
      ListBox parentList = parentControl.FindVisualParent<ListBox>();
      int moveIndex = parentList.ItemContainerGenerator.IndexFromContainer(parentControl);
      ViewModel.CurrentPokemon.Moves[moveIndex] = moveCombo.SelectedItem as Move;
    }
    private void editClassButton_Click(object sender, RoutedEventArgs e) {
      var window = new TrainerClassEditorWindow(classList.SelectedItem as TrainerClass) {
        Owner = Application.Current.MainWindow,
        WindowStartupLocation = WindowStartupLocation.CenterOwner
      };
      window.ShowDialog();
    }

    private Point? partyMenuDragstartPoint;
    private void partyMenu_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) => partyMenuDragstartPoint = e.GetPosition(null);
    private void partyMenu_PreviewMouseMove(object sender, MouseEventArgs e) {
      // Check that a drag is happening from the party menu.
      if (partyMenuDragstartPoint == null || sender != partyMenu)
        return;

      // Get the current mouse position
      Point mousePos = e.GetPosition(null);
      Vector diff = partyMenuDragstartPoint.Value - mousePos;
      // test for the minimum displacement to begin the drag
      if (!(e.LeftButton == MouseButtonState.Pressed &&
          (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
          Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))) {
        return;
      }

      // Get the pokemon from the row being dragged.
      DataGridRow dataGridRow = ((DependencyObject)e.OriginalSource).FindVisualParent<DataGridRow>();
      if (dataGridRow == null)
        return;
      int rowIndex = dataGridRow.GetIndex();

      // Initialize the drag & drop operation.
      var dataObj = new DataObject("origIndex", rowIndex);
      dataObj.SetData("DragSource", sender);
      DragDrop.DoDragDrop(partyMenu, dataObj, DragDropEffects.Copy);
      partyMenuDragstartPoint = null;
    }

    private void partyMenu_PreviewMouseUp(object sender, MouseButtonEventArgs e) => partyMenuDragstartPoint = null;

    private void partyMenu_Drop(object sender, DragEventArgs e) {
      var dg = sender as DataGrid;
      if (dg == null)
        return;
      DataGridRow newGridRow = ((DependencyObject)e.OriginalSource).FindVisualParent<DataGridRow>();
      if (newGridRow == null)
        return;
      int newRowIndex = newGridRow.GetIndex();

      var dgSrc = e.Data.GetData("DragSource") as DataGrid;
      object origIndexObj = e.Data.GetData("origIndex");
      if (dgSrc == null || origIndexObj == null || (int)origIndexObj == newRowIndex)
        return;
      ViewModel.CurrentTrainer.Party.Pokemon.Move((int)origIndexObj, newGridRow.GetIndex());
    }

    private void partyMenu_PreviewDragOver(object sender, DragEventArgs e) {
      // Don't allow drag if the trainer only has one pokemon.
      if (ViewModel.CurrentTrainer.Party.Pokemon.Count == 1)
        e.Effects = DragDropEffects.None;
    }

    private void partyMenu_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
      DataGridRow clickedRow = ((DependencyObject)e.OriginalSource).FindVisualParent<DataGridRow>();
      if (clickedRow == null || !clickedRow.IsNewItem)
        return;
      ViewModel.CurrentTrainer.Party.Pokemon.Add(Pokemon.createDefault());
      ViewModel.RaisePropertyChanged("CanAddPokemon");
    }

    private void partyMenu_PreviewKeyDown(object sender, KeyEventArgs e) {
      DataGridRow clickedRow = ((DependencyObject)e.OriginalSource).FindVisualParent<DataGridRow>();
      if (e.Key != Key.Delete || clickedRow == null || clickedRow.IsNewItem)
        return;
      ObservableCollection<Pokemon> pokemon = ViewModel.CurrentTrainer.Party.Pokemon;
      if (pokemon.Count == 1)
        return;
      int removeIndex = clickedRow.GetIndex();
      pokemon.RemoveAt(removeIndex);
      ViewModel.RaisePropertyChanged("CanAddPokemon");
      partyMenu.SelectedIndex = Math.Max(0, removeIndex - 1);
    }

    private void editPicButton_Click(object sender, RoutedEventArgs e) {
      var window = new TrainerPicEditorWindow(picList.SelectedItem as TrainerPic) {
        Owner = Application.Current.MainWindow,
        WindowStartupLocation = WindowStartupLocation.CenterOwner
      };
      window.ShowDialog();
    }
  }
}
