using DecompEditor.ProjectData;
using DecompEditor.Views;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DecompEditor.Utils;

namespace DecompEditor.Editors {
  /// <summary>
  /// Interaction logic for WildEncounterEditorView.xaml
  /// </summary>
  public partial class WildEncounterEditorView : UserControl {
    public WildEncounterEditorView() {
      InitializeComponent();

      CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(mapList.ItemsSource);
      PropertyGroupDescription groupDescription = new PropertyGroupDescription("MapGroup");
      view.GroupDescriptions.Add(groupDescription);
    }

    public WildEncounterEditorViewModel ViewModel => DataContext as WildEncounterEditorViewModel;

    /// <summary>
    /// A list of pokemon that are currently selected to be copied.
    /// </summary>
    private List<WildEncounterPokemon> pokemonToCopy = null;

    private void mapList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      ViewModel.CurrentMap = mapList.SelectedItem as WildEncounterMap;
      habitatTabControl.Focus();
    }

    private void addOccurrenceButton_Click(object sender, RoutedEventArgs e) {
      WildEncounterPokemon pokemon = (sender as Button).DataContext as WildEncounterPokemon;
      pokemon.Occurrences.Add(new WildEncounterOccurrence() {
        MidLevel = 0,
        LevelRange = 2,
        EncounterRate = 1,
        Time = "MORNING"
      });
    }
    private void addPokemonButton_Click(object sender, RoutedEventArgs e) {
      var pokemon = ViewModel.CurrentMap.Habitats[habitatTabControl.SelectedIndex].Pokemon;
      pokemon.Add(new WildEncounterPokemon() {
        Species = ViewModel.PokemonSpecies.First()
      });
    }

    private void PokemonList_PreviewKeyDown(object sender, KeyEventArgs e) {
      ListView pokemonList = ((DependencyObject)e.OriginalSource).FindVisualParent<ListView>();
      if (pokemonList == null)
        return;

      // Handle deleting the currently selected pokemon.
      if (e.Key == Key.Delete) {
        while (pokemonList.SelectedItems.Count > 0)
          ViewModel.CurrentHabitat.Pokemon.Remove(pokemonList.SelectedItems[0] as WildEncounterPokemon);
        return;
      }

      // Handle copy/pasting the currently selected pokemon.
      if (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)) {
        if (e.Key == Key.C) {
          if (pokemonList.SelectedItems.Count != 0)
            pokemonToCopy = pokemonList.SelectedItems.Cast<WildEncounterPokemon>().ToList();
          return;
        }
      }
    }
    private void PokemonTabControl_PreviewKeyDown(object sender, KeyEventArgs e) {
      // Handle pasting the currently selected pokemon.
      if (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)) {
        if (e.Key == Key.V) {
          if (pokemonToCopy != null) {
            foreach (var pkmn in pokemonToCopy)
              ViewModel.CurrentHabitat.Pokemon.Add(pkmn.clone());
          }
          return;
        }
      }
    }

    private void OccurrenceGrid_PreviewKeyDown(object sender, KeyEventArgs e) {
      if (e.Key == Key.D) {
        DataGrid occurList = ((DependencyObject)e.OriginalSource).FindVisualParent<DataGrid>();
        if (occurList.CurrentItem == null)
          return;
        WildEncounterPokemon pokemon = (sender as DataGrid).DataContext as WildEncounterPokemon;
        pokemon.Occurrences.Add((occurList.CurrentItem as WildEncounterOccurrence).clone());
        return;
      }
    }

    private void OccurrenceGrid_GotFocus(object sender, RoutedEventArgs e) {
      ListView pokemonList = ((DependencyObject)e.OriginalSource).FindVisualParent<ListView>();
      if (pokemonList != null)
        pokemonList.SelectedItem = null;
    }

    private void OccurrenceGrid_MoveFocusToNewColumn(object sender, int offset) {
      DataGrid occurList = ((DependencyObject)sender).FindVisualParent<DataGrid>();
      if (occurList.CurrentItem == null)
        return;
      var element = occurList.Columns[occurList.CurrentCell.Column.DisplayIndex + offset].GetCellContent(occurList.CurrentCell.Item);
      var cell = element.FindVisualParent<DataGridCell>();
      cell.Focus();
    }

    private void TimeOfDay_PreviewKeyDown(object sender, KeyEventArgs e) {
      if (e.Key == Key.Tab) {
        if (!e.KeyboardDevice.IsKeyDown(Key.LeftShift)) {
          OccurrenceGrid_MoveFocusToNewColumn(sender, 1);
          return;
        }
        return;
      }
    }

    private void EncounterRate_PreviewKeyDown(object sender, KeyEventArgs e) {
      if (e.Key == Key.Tab) {
        OccurrenceGrid_MoveFocusToNewColumn(sender, e.KeyboardDevice.IsKeyDown(Key.LeftShift) ? 0 : 1);
        return;
      }
    }

    private void MidLevel_PreviewKeyDown(object sender, KeyEventArgs e) {
      if (e.Key == Key.Tab) {
        OccurrenceGrid_MoveFocusToNewColumn(sender, e.KeyboardDevice.IsKeyDown(Key.LeftShift) ? 0 : 1);
        return;
      }
    }

    private void LevelRange_PreviewKeyDown(object sender, KeyEventArgs e) {
      if (e.Key == Key.Tab) {
        if (e.KeyboardDevice.IsKeyDown(Key.LeftShift)) {
          OccurrenceGrid_MoveFocusToNewColumn(sender, 0);
          return;
        }
        DataGrid occurList = ((DependencyObject)sender).FindVisualParent<DataGrid>();
        if (occurList.CurrentItem == null)
          return;

        int newIndex = occurList.SelectedIndex < (occurList.Items.Count - 1) ? occurList.SelectedIndex + 1 : 0;
        var element = occurList.Columns[1].GetCellContent(occurList.Items[newIndex]);
        var cell = element.FindVisualParent<DataGridCell>();
        cell.Focus();
        return;
      }
    }
  }
}
