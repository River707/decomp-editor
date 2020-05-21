﻿using GalaSoft.MvvmLight;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DecompEditor.Views {
  public class TrainerEditorViewModel : ViewModelBase {
    public TrainerEditorViewModel() {
      Project.Instance.Loaded += () => {
        CurrentPokemon = null;
        CurrentTrainer = null;
        RaisePropertyChanged(string.Empty);
      };
    }

    /// <summary>
    /// The set of trainer encounter music within the project.
    /// </summary>
    public IEnumerable<string> EncounterMusic => Project.Instance.TrainerEncounterMusic.EncounterMusic
      .Select(x => x);

    /// <summary>
    /// The set of trainer encounter music within the project.
    /// </summary>
    public IEnumerable<Item> Items => Project.Instance.Items.Items.OrderBy(item => item.Name);

    /// <summary>
    /// The set of pokemon moves within the project.
    /// </summary>
    public IEnumerable<Move> Moves => Project.Instance.Moves.Moves.OrderBy(move => move.Name);

    /// <summary>
    /// The set of pokemon species within the project.
    /// </summary>
    public IEnumerable<PokemonSpecies> PokemonSpecies => Project.Instance.Species.Species;

    /// <summary>
    /// The current set of trainers within the project.
    /// </summary>
    public ObservableCollection<Trainer> Trainers => Project.Instance.Trainers.Trainers;

    /// <summary>
    /// The set of AI scripts that can be attached to a trainer.
    /// </summary>
    public IEnumerable<string> TrainerAIScripts => Project.Instance.BattleAI.AIScripts;

    /// <summary>
    /// The set of trainer classes within the project.
    /// </summary>
    public IEnumerable<TrainerClass> TrainerClasses => Project.Instance.TrainerClasses.Classes;

    /// <summary>
    /// The set of trainer encounter music within the project.
    /// </summary>
    public IEnumerable<TrainerPic> TrainerPics => Project.Instance.TrainerPics.FrontPics;

    /// <summary>
    /// The currently selected trainer.
    /// </summary>
    Trainer currentTrainer;
    public Trainer CurrentTrainer {
      get => currentTrainer;
      set {
        Set(ref currentTrainer, value);
        RaisePropertyChanged("TrainerIsSelected");
        RaisePropertyChanged("CanAddPokemon");
      }
    }
    public bool TrainerIsSelected => currentTrainer != null;

    /// <summary>
    /// The currently selected pokemon in the party.
    /// </summary>
    Pokemon currentPokemon;
    public Pokemon CurrentPokemon {
      get => currentPokemon;
      set {
        Set(ref currentPokemon, value);
        RaisePropertyChanged("PokemonIsSelected");
      }
    }
    public bool PokemonIsSelected => currentPokemon != null;
    public bool CanAddPokemon => CurrentTrainer != null ? CurrentTrainer.Party.Pokemon.Count != 6 : false;
  }
}
