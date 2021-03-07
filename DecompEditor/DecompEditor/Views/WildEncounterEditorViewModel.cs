using DecompEditor.ProjectData;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace DecompEditor.Views {
  public class WildEncounterEditorViewModel : ViewModelBase {
    public WildEncounterEditorViewModel() {
      Project.Instance.Loaded += () => {
        CurrentMap = null;
        RaisePropertyChanged(string.Empty);
      };
    }

    /// <summary>
    /// The set of encounter maps within the project.
    /// </summary>
    public IEnumerable<WildEncounterMap> WildEncounters => Project.Instance.WildEncounters.WildEncounters;

    /// <summary>
    /// The set of pokemon species within the project.
    /// </summary>
    public IEnumerable<PokemonSpecies> PokemonSpecies => Project.Instance.Species.Species.Skip(1);

    /// <summary>
    /// The times of day within the project.
    /// </summary>
    public IEnumerable<string> TimesOfDay => new string[] { "MORNING", "DAY", "NIGHT" };

    public int MorningEncounterRate =>
      Enumerable.Sum(CurrentHabitat.Pokemon.SelectMany(
        pokemon => pokemon.Occurrences.Where(occur => occur.Time == "MORNING")
        .Select(occur => occur.EncounterRate)));

    public int DayEncounterRate =>
      Enumerable.Sum(CurrentHabitat.Pokemon.SelectMany(
        pokemon => pokemon.Occurrences.Where(occur => occur.Time == "DAY")
        .Select(occur => occur.EncounterRate)));
    public int NightEncounterRate =>
      Enumerable.Sum(CurrentHabitat.Pokemon.SelectMany(
        pokemon => pokemon.Occurrences.Where(occur => occur.Time == "NIGHT")
        .Select(occur => occur.EncounterRate)));

    /// <summary>
    /// The currently selected map encounter.
    /// </summary>
    WildEncounterMap currentMap;
    public WildEncounterMap CurrentMap {
      get => currentMap;
      set {
        WildEncounterMap oldMap = currentMap;
        if (oldMap != null)
          oldMap.PropertyChanged -= CurrentMap_PropertyChanged;

        Set(ref currentMap, value);
        RaisePropertyChanged("MapIsSelected");
        if (currentMap == null) {
          CurrentHabitat = null;
          return;
        }
        currentMap.PropertyChanged += CurrentMap_PropertyChanged;

        // Update the current habitat.
        if (CurrentHabitat != null)
          CurrentHabitat = CurrentMap.Habitats[oldMap.Habitats.IndexOf(currentHabitat)];
        else
          CurrentHabitat = CurrentMap.Habitats.First(hab => hab.Type == "Land");
      }
    }
    public bool MapIsSelected => currentMap != null;

    private void CurrentMap_PropertyChanged(object sender, PropertyChangedEventArgs e) {
      RaisePropertyChanged("MorningEncounterRate");
      RaisePropertyChanged("DayEncounterRate");
      RaisePropertyChanged("NightEncounterRate");
    }

    /// <summary>
    /// The currently selected habitat.
    /// </summary>
    WildEncounterHabitat currentHabitat;
    public WildEncounterHabitat CurrentHabitat {
      get => currentHabitat;
      set {
        if (currentMap != null && value == null)
          return;

        Set(ref currentHabitat, value);
        RaisePropertyChanged("MorningEncounterRate");
        RaisePropertyChanged("DayEncounterRate");
        RaisePropertyChanged("NightEncounterRate");
      }
    }
  }
}
