using DecompEditor.ParserUtils;
using DecompEditor.Utils;
using GalaSoft.MvvmLight;
using System.Collections.Generic;
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
    public IEnumerable<CDefine> EncounterMusic => Project.Instance.TrainerEncounterMusic.EncounterMusic;

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
    public IEnumerable<CDefine> TrainerAIScripts => Project.Instance.BattleAI.AIScripts;

    /// <summary>
    /// The set of trainer classes within the project.
    /// </summary>
    public IEnumerable<TrainerClass> TrainerClasses => Project.Instance.Trainers.Classes;

    /// <summary>
    /// The set of trainer encounter music within the project.
    /// </summary>
    public IEnumerable<TrainerPic> TrainerPics => Project.Instance.Trainers.FrontPics;

    /// <summary>
    /// The currently selected trainer.
    /// </summary>
    Trainer currentTrainer;
    public Trainer CurrentTrainer {
      get => currentTrainer;
      set {
        Set(ref currentTrainer, value);
        if (value != null)
          CurrentPokemon = value.Party.Pokemon[0];
        RaisePropertyChanged("TrainerIsSelected");
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
  }
}
