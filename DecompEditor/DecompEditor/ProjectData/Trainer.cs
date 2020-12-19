using DecompEditor.ParserUtils;
using DecompEditor.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Media;

namespace DecompEditor {
  /// <summary>
  /// A specific pokemon from within a trainer's party.
  /// </summary>
  public class Pokemon : ObservableObject {
    private int iv = 0;
    private int level = 5;
    private PokemonSpecies species;
    private Item heldItem;
    private ObservableCollection<Move> moves;

    public int Iv { get => iv; set => Set(ref iv, value); }
    public int Level {
      get => level;
      set => Set(ref level, value);
    }
    public PokemonSpecies Species { get => species; set => Set(ref species, value); }
    public Item HeldItem { get => heldItem; set => Set(ref heldItem, value); }
    public ObservableCollection<Move> Moves {
      get => moves;
      set => SetAndTrack(ref moves, value);
    }

    public static Pokemon createDefault() {
      return new Pokemon {
        HeldItem = Project.Instance.Items.getFromId("NONE"),
        Moves = new ObservableCollection<Move>(Enumerable.Repeat(Project.Instance.Moves.getFromId("MOVE_NONE"), 4)),
        Species = Project.Instance.Species.getFromId("SPECIES_NONE")
      };
    }
  }
  /// <summary>
  /// The part of a trainer defined within the project.
  /// </summary>
  public class TrainerParty : ObservableObject {
    private ObservableCollection<Pokemon> pokemon;

    /// <summary>
    /// The pokemon within the party.
    /// </summary>
    public ObservableCollection<Pokemon> Pokemon {
      get => pokemon;
      set => SetAndTrackItemUpdates(ref pokemon, value, this);
    }

    public TrainerParty() => Pokemon = new ObservableCollection<Pokemon>();
  }
  /// <summary>
  /// This class represents a specific trainer within the project.
  /// </summary>
  public class Trainer : ObservableObject {
    private string identifier;
    private TrainerClass @class;
    private CDefine encounterMusic;
    private TrainerPic pic;
    private string name;
    private ObservableCollection<Item> items;
    private bool doubleBattle;
    private bool isMale = true;
    private ObservableCollection<CDefine> aiFlags;
    private TrainerParty party;

    /// <summary>
    /// The C identifier of the trainer.
    /// </summary>
    public string Identifier {
      get => identifier;
      set {
        var oldIdentifier = identifier;
        if (!Set(ref identifier, value) || oldIdentifier == null)
          return;
        Project.Instance.registerFileReplacement("TRAINER_" + oldIdentifier,
                                                 "TRAINER_" + identifier);
      }
    }
    /// <summary>
    /// The class of the trainer.
    /// </summary>
    public TrainerClass Class { get => @class; set => Set(ref @class, value); }
    /// <summary>
    /// The encounter music of the trainer battle.
    /// </summary>
    public CDefine EncounterMusic { get => encounterMusic; set => Set(ref encounterMusic, value); }
    /// <summary>
    /// The front pic of the trainer.
    /// </summary>
    public TrainerPic Pic { get => pic; set => Set(ref pic, value); }
    /// <summary>
    /// The name of the trainer.
    /// </summary>
    public string Name { get => name; set => Set(ref name, value); }
    /// <summary>
    /// The items held by the trainer.
    /// </summary>
    public ObservableCollection<Item> Items {
      get => items;
      set => SetAndTrack(ref items, value);
    }
    /// <summary>
    /// Whether this trainer is a double battle or not.
    /// </summary>
    public bool DoubleBattle { get => doubleBattle; set => Set(ref doubleBattle, value); }
    /// <summary>
    /// Whether this trainer is male or not.
    /// </summary>
    public bool IsMale { get => isMale; set => Set(ref isMale, value); }
    /// <summary>
    /// Whether this trainer female or not.
    /// </summary>
    public bool IsFemale { get => !IsMale; set => IsMale = !value; }
    /// <summary>
    /// The AI flags used by this trainer during battle.
    /// </summary>
    public ObservableCollection<CDefine> AIFlags {
      get => aiFlags;
      set => SetAndTrack(ref aiFlags, value);
    }
    /// <summary>
    /// The party of Pokemon used by this trainer.
    /// </summary>
    public TrainerParty Party { get => party; set => SetAndTrack(ref party, value); }

    public Trainer() {
      AIFlags = new ObservableCollection<CDefine>();
      Items = new ObservableCollection<Item>();
    }
  }
  /// <summary>
  /// This class represents a specific trainer class.
  /// </summary>
  public class TrainerClass : ObservableObject {
    private string identifier;
    private string name;
    private int moneyFactor = 5;

    /// <summary>
    /// The C identifier of the class.
    /// </summary>
    public string Identifier {
      get => identifier;
      set {
        if (identifier != null) {
          Project.Instance.registerFileReplacement("TRAINER_CLASS_" + identifier,
                                                   "TRAINER_CLASS_" + value);
        }
        Set(ref identifier, value);
      }
    }
    /// <summary>
    /// The name of the trainer class.
    /// </summary>
    public string Name { get => name; set => Set(ref name, value); }
    /// <summary>
    /// The money factor of the trainer class.
    /// </summary>
    public int MoneyFactor { get => moneyFactor; set => Set(ref moneyFactor, value); }
  }
  /// <summary>
  /// This class represents a specific trainer front pic.
  /// </summary>
  public class TrainerPic : ObservableObject {
    private string fullPath;
    private string path;
    private string palettePath;
    private string identifier;
    private int coordSize = 8;
    private int coordYOffset = 1;
    private int uncompressedSize;

    /// <summary>
    /// The full file path of the picture.
    /// </summary>
    public string FullPath {
      get => fullPath;
      set => Set(ref fullPath, FileUtils.normalizePath(value));
    }
    /// <summary>
    /// The project relative path of the picture.
    /// </summary>
    public string Path { get => path; set => Set(ref path, value); }
    /// <summary>
    /// The path of the palette used for the picture.
    /// </summary>
    public string PalettePath { get => palettePath; set => Set(ref palettePath, value); }
    /// <summary>
    /// The C identifier of the picture.
    /// </summary>
    public string Identifier {
      get => identifier;
      set {
        if (identifier != null) {
          string curCppVar = identifier.fromSnakeToPascal();
          string newCppVar = value.fromSnakeToPascal();
          Project.Instance.registerFileReplacement("TRAINER_PIC_" + identifier,
                                                   "TRAINER_PIC_" + value);
          Project.Instance.registerFileReplacement("gTrainerPalette_" + curCppVar,
                                                   "gTrainerPalette_" + newCppVar);
        }
        Set(ref identifier, value);
      }
    }
    /// <summary>
    /// The coordinate size of the picture.
    /// </summary>
    public int CoordSize { get => coordSize; set => Set(ref coordSize, value); }
    /// <summary>
    /// The Y-coordinate offset of the picture.
    /// </summary>
    public int CoordYOffset { get => coordYOffset; set => Set(ref coordYOffset, value); }
    /// <summary>
    /// The uncompressed size of the picture.
    /// </summary>
    public int UncompressedSize { get => uncompressedSize; set => Set(ref uncompressedSize, value); }
  }
  class TrainerDatabase : DatabaseBase {
    private ObservableCollection<TrainerClass> classes;
    private ObservableCollection<TrainerPic> frontPics;
    private ObservableCollection<Trainer> trainers;

    /// <summary>
    /// The name of this database.
    /// </summary>
    public override string Name => "Trainer Database";

    // TODO: These could be configurable.
    public int MaxClassCount => 255;
    public int MaxClassNameLen => 13;

    /// <summary>
    /// Returns the trainer classes defined within the project.
    /// </summary>
    public ObservableCollection<TrainerClass> Classes {
      get => classes;
      private set => SetAndTrackItemUpdates(ref classes, value, this);
    }

    /// <summary>
    /// Returns all of the front trainer pics within the project.
    /// </summary>
    public ObservableCollection<TrainerPic> FrontPics {
      get => frontPics;
      set => SetAndTrackItemUpdates(ref frontPics, value, this);
    }

    /// <summary>
    /// Returns the trainers defined within the project.
    /// </summary>
    public ObservableCollection<Trainer> Trainers {
      get => trainers;
      private set => SetAndTrackItemUpdates(ref trainers, value, this);
    }

    public TrainerDatabase() {
      Classes = new ObservableCollection<TrainerClass>();
      FrontPics = new ObservableCollection<TrainerPic>();
      Trainers = new ObservableCollection<Trainer>();
    }

    /// <summary>
    /// Reset the data within this database.
    /// </summary>
    protected override void reset() {
      Classes.Clear();
      FrontPics.Clear();
      Trainers.Clear();
    }

    /// <summary>
    /// Add a trainer class to the database.
    /// </summary>
    /// <param name="newClass"></param>
    public void addClass(TrainerClass newClass) => Classes.Add(newClass);

    /// <summary>
    /// Add a new trainer front pic to the project.
    /// </summary>
    /// <param name="newPic"></param>
    public void addFrontPic(TrainerPic newPic) => FrontPics.Add(newPic);

    /// <summary>
    /// Deserialize the trainers within the project directory.
    /// </summary>
    /// <param name="deserializer"></param>
    protected override void deserialize(ProjectDeserializer deserializer)
      => Deserializer.deserialize(deserializer, this);

    /// <summary>
    /// Serialize the trainer data to the project directory.
    /// </summary>
    /// <param name="serializer"></param>
    protected override void serialize(ProjectSerializer serializer)
      => Serializer.serialize(serializer, this);

    /// <summary>
    /// Returns true if the project needs to upgrade its trainer format.
    /// </summary>
    /// <returns></returns>
    public override bool needsUpgrade() {
      // If the json file exists, this project has the new format.
      return !File.Exists(Path.Combine(Project.Instance.ProjectDir, "src", "data", "trainers.json"));
    }

    /// <summary>
    /// Upgrades the event object format of the project.
    /// </summary>
    /// <param name="deserializer"></param>
    /// <param name="serializer"></param>
    protected override void upgrade(ProjectDeserializer deserializer, ProjectSerializer serializer) {
      // Load and convert the trainer storage format.
      var converter = new ProjectData.OldFormat.Trainers.Converter();
      converter.convert(deserializer, this);

      // Serialize the new format.
      serialize(serializer);
    }

    public class JSONDatabase {
      public JSONDatabase() { }
      public JSONDatabase(TrainerDatabase database) {
        Classes = database.Classes.ToArray();
        FrontPics = database.FrontPics.ToArray();
        Trainers = database.Trainers.Select(trainer => new JSONTrainer(trainer)).ToArray();
      }

      public void deserializeInto(TrainerDatabase database) {
        Dictionary<string, TrainerClass> idToClass = new Dictionary<string, TrainerClass>();
        foreach (var @class in Classes) {
          database.Classes.Add(@class);
          idToClass.Add(@class.Identifier, @class);
        }
        Dictionary<string, TrainerPic> idToPic = new Dictionary<string, TrainerPic>();
        foreach (var pic in FrontPics) {
          database.FrontPics.Add(pic);
          idToPic.Add(pic.Identifier, pic);
        }
        foreach (var trainer in Trainers)
          database.Trainers.Add(trainer.deserialize(idToClass, idToPic));
      }

      public class JSONPokemon {
        public JSONPokemon() { }
        public JSONPokemon(Pokemon pokemon, bool partyHasItems, bool partyHasMoves) {
          Iv = pokemon.Iv;
          Level = pokemon.Level;
          Species = pokemon.Species.Identifier;
          HeldItem = partyHasItems ? "ITEM_" + pokemon.HeldItem.Identifier : null;
          Moves = partyHasMoves ? pokemon.Moves.Select(move => move.Identifier).ToArray() : null;
        }
        public Pokemon deserialize() {
          return new Pokemon() {
            Iv = Iv,
            Level = Level,
            Species = Project.Instance.Species.getFromId(Species),
            HeldItem = Project.Instance.Items.getFromId(HeldItem),
            Moves = new ObservableCollection<Move>(Moves.Select(move => Project.Instance.Moves.getFromId(move)))
          };
        }

        public int Iv { get; set; } = 0;
        public int Level { get; set; } = 5;
        public string Species { get; set; } = "SPECIES_NONE";
        public string HeldItem { get; set; } = "NONE";
        public string[] Moves { get; set; } = Enumerable.Repeat("MOVE_NONE", 4).ToArray();
      }
      public class JSONTrainerParty {
        public bool? HasItems { get; set; }
        public bool? HasMoves { get; set; }
        public JSONPokemon[] Pokemon { get; set; }
      }
      public class JSONTrainer {
        public JSONTrainer() { }
        public JSONTrainer(Trainer trainer) {
          Identifier = trainer.Identifier;
          Class = trainer.Class.Identifier;
          EncounterMusic = trainer.EncounterMusic.Identifier;
          Pic = trainer.Pic.Identifier;
          Name = trainer.Name;
          Items = trainer.Items.Any(item => item.Identifier != "NONE") ? trainer.Items.Select(item => "ITEM_" + item.Identifier).ToArray() : null;
          DoubleBattle = trainer.DoubleBattle ? new bool?(true) : null;
          IsMale = trainer.IsMale ? new bool?(true) : null;
          AIFlags = trainer.AIFlags.OrderBy(flag => flag.Order).Select(flag => flag.Identifier).ToArray();

          bool partyHasItems = trainer.Party.Pokemon.Any(pokemon => pokemon.HeldItem.Identifier != "NONE");
          bool partyHasMoves = trainer.Party.Pokemon.Any(pokemon => pokemon.Moves.Any(move => move.Identifier != "MOVE_NONE"));
          Party = new JSONTrainerParty() {
            HasItems = partyHasItems ? new bool?(true) : null,
            HasMoves = partyHasMoves ? new bool?(true) : null,
            Pokemon = trainer.Party.Pokemon.Select(pokemon => new JSONPokemon(pokemon, partyHasItems, partyHasMoves)).ToArray()
          };
        }
        public Trainer deserialize(Dictionary<string, TrainerClass> idToClass,
                                   Dictionary<string, TrainerPic> idToPic) {
          return new Trainer() {
            Identifier = Identifier,
            Class = idToClass[Class],
            EncounterMusic = Project.Instance.TrainerEncounterMusic.getFromId("TRAINER_ENCOUNTER_MUSIC_" + EncounterMusic),
            Pic = idToPic[Pic],
            Name = Name,
            Items = new ObservableCollection<Item>(Items.Select(item => Project.Instance.Items.getFromId(item))),
            DoubleBattle = (bool)DoubleBattle,
            IsMale = (bool)IsMale,
            AIFlags = new ObservableCollection<CDefine>(AIFlags.Select(flag => Project.Instance.BattleAI.getFromId("AI_SCRIPT_" + flag))),
            Party = new TrainerParty() {
              Pokemon = new ObservableCollection<Pokemon>(Party.Pokemon.Select(pokemon => pokemon.deserialize()))
            }
          };
        }

        public string Identifier { get; set; }
        public string Class { get; set; }
        public string EncounterMusic { get; set; }
        public string Pic { get; set; }
        public string Name { get; set; }
        public string[] Items { get; set; } = Enumerable.Repeat("NONE", 4).ToArray();
        public bool? DoubleBattle { get; set; } = false;
        public bool? IsMale { get; set; } = false;
        public string[] AIFlags { get; set; }
        public JSONTrainerParty Party { get; set; }
      }

      public TrainerClass[] Classes { get; set; }
      public TrainerPic[] FrontPics { get; set; }
      public JSONTrainer[] Trainers { get; set; }
    }

    class Deserializer {
      public static void deserialize(ProjectDeserializer deserializer, TrainerDatabase database) {
        string jsonPath = Path.Combine(deserializer.project.ProjectDir, "src", "data", "trainers.json");
        JSONDatabase jsonDatabase = JsonSerializer.Deserialize<JSONDatabase>(File.ReadAllText(jsonPath));
        jsonDatabase.deserializeInto(database);
      }
    }

    class Serializer {
      public static void serialize(ProjectSerializer serializer, TrainerDatabase database) {
        JSONDatabase jsonDatabase = new JSONDatabase(database);
        string json = JsonSerializer.Serialize(jsonDatabase, new JsonSerializerOptions() {
          IgnoreNullValues = true,
          WriteIndented = true
        });
        File.WriteAllText(Path.Combine(serializer.project.ProjectDir, "src", "data", "trainers.json"), json);

        // Update the location of the picture image and palettes if necessary.
        updatePicLocations(database, serializer.project.ProjectDir);
      }

      static void updatePicLocations(TrainerDatabase database, string projectDir) {
        // Check to see if any of the pics changed location.
        foreach (TrainerPic pic in database.FrontPics) {
          string fullPrettyPath = Path.Combine(projectDir, "graphics/trainers/front_pics", pic.Path + ".png");
          string normalizedPath = FileUtils.normalizePath(fullPrettyPath);
          if (pic.FullPath == normalizedPath)
            continue;
          File.Copy(pic.FullPath, fullPrettyPath, true);
          pic.FullPath = normalizedPath;

          // Generate a new palette file.
          IList<Color> palette = FileUtils.loadBitmapImage(normalizedPath).Palette.Colors;
          string normalizePalPath = Path.Combine(projectDir, "graphics/trainers/palettes", pic.PalettePath + ".pal");
          using (var palWriter = new StreamWriter(normalizePalPath, false)) {
            palWriter.WriteLine("JASC-PAL");
            palWriter.WriteLine("0100");
            palWriter.WriteLine(palette.Count);
            foreach (Color color in palette)
              palWriter.WriteLine("{0} {1} {2}", color.R, color.G, color.B);
          }

          // Delete any existing bpp/pal files to force a rebuild.
          File.Delete(Path.ChangeExtension(normalizedPath, ".4bpp"));
          File.Delete(Path.ChangeExtension(normalizedPath, ".4bpp.lz"));
          File.Delete(Path.ChangeExtension(normalizePalPath, ".gbapal"));
          File.Delete(Path.ChangeExtension(normalizePalPath, ".gbapal.lz"));
        }
      }
    }
  }
}
