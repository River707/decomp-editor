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
    private void partyMenu_PreviewMouseMove(object sender, MouseEventArgs e) {
      if (!(e.Source is TabItem tabItem))
        return;
      if (Mouse.PrimaryDevice.LeftButton == MouseButtonState.Pressed)
        DragDrop.DoDragDrop(tabItem, tabItem, DragDropEffects.All);
    }

    private void partyMenu_Drop(object sender, DragEventArgs e) {
      if (!(e.Source is TabItem tabItemTarget))
        return;
      if (!(e.Data.GetData(typeof(TabItem)) is TabItem tabItemSource))
        return;
      if (tabItemTarget.Equals(tabItemSource))
        return;

      ObservableCollection<Pokemon> pokemon = ViewModel.CurrentTrainer.Party.Pokemon;
      int sourceIndex = pokemon.IndexOf(tabItemSource.Content as Pokemon);
      int targetIndex = pokemon.IndexOf(tabItemTarget.Content as Pokemon);
      ViewModel.CurrentTrainer.Party.Pokemon.Move(sourceIndex, targetIndex);
      ViewModel.CurrentPokemon = pokemon[targetIndex];
    }

    private void partyMenu_PreviewKeyDown(object sender, KeyEventArgs e) {
      ObservableCollection<Pokemon> pokemon = ViewModel.CurrentTrainer.Party.Pokemon;
      if (e.Key == Key.Delete) {
        if (pokemon.Count == 1)
          return;

        int removeIndex = pokemon.IndexOf(ViewModel.CurrentPokemon);
        pokemon.RemoveAt(removeIndex);
        ViewModel.CurrentPokemon = pokemon[Math.Max(0, removeIndex - 1)];
      }
      if (e.Key == Key.A && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))) {
        if (pokemon.Count == 6)
          return;

        int insertIndex = pokemon.IndexOf(ViewModel.CurrentPokemon) + 1;
        pokemon.Insert(insertIndex, Pokemon.createDefault());
        ViewModel.CurrentPokemon = pokemon[insertIndex];
        return;
      }
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
