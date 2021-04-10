using DecompEditor.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Truncon.Collections;

namespace DecompEditor.ProjectData {
  /// <summary>
  /// This class represents a specific wild encounter occurrence.
  /// </summary>
  public class WildEncounterOccurrence : ObservableObject, IComparable {
    private int midLevel;
    private int levelRange;
    private int encounterRate;
    private string time;

    public int MidLevel { 
      get => midLevel; 
      set => Set(ref midLevel, value); 
    }
    public int LevelRange { get => levelRange; set => Set(ref levelRange, value); }
    public int EncounterRate { get => encounterRate; set => Set(ref encounterRate, value); }
    public string Time { get => time; set => Set(ref time, value); }

    public int CompareTo(object obj) {
      if (obj == null || !(obj is WildEncounterOccurrence))
        return 1;
      WildEncounterOccurrence other = obj as WildEncounterOccurrence;
      if (Time != other.Time) {
        if (Time == "MORNING")
          return -1;
        if (Time == "DAY")
          return other.Time == "NIGHT" ? -1 : 1;
        return 1;
      }
      if (EncounterRate != other.EncounterRate)
        return EncounterRate > other.EncounterRate ? -1 : 1;
      if (MidLevel != other.MidLevel)
        return MidLevel > other.MidLevel ? 1 : -1;
      if (LevelRange != other.LevelRange)
        return LevelRange > other.LevelRange ? 1 : -1;
      return 0;
    }

    public WildEncounterOccurrence clone() {
      return new WildEncounterOccurrence() {
        MidLevel = MidLevel,
        LevelRange = LevelRange,
        EncounterRate = EncounterRate,
        Time = Time
      };
    }
  }

  /// <summary>
  /// This class represents a specific wild encounter pokemon group.
  /// </summary>
  public class WildEncounterPokemon : ObservableObject, IComparable {
    private PokemonSpecies species;
    private SortedObservableCollection<WildEncounterOccurrence> occurrences;

    public WildEncounterPokemon() {
      Occurrences = new SortedObservableCollection<WildEncounterOccurrence>();
    }
    public PokemonSpecies Species { get => species; set => Set(ref species, value); }
    public SortedObservableCollection<WildEncounterOccurrence> Occurrences {
      get => occurrences;
      set => SetAndTrackItemUpdates(ref occurrences, value, this);
    }
    public WildEncounterPokemon clone() {
      return new WildEncounterPokemon() {
        Species = Species,
        Occurrences = new SortedObservableCollection<WildEncounterOccurrence>(Occurrences.Select(occur => occur.clone()))
      };
    }
    public int CompareTo(object obj) {
      if (obj == null || !(obj is WildEncounterPokemon))
        return 1;
      return Comparer<string>.Default.Compare(species.Identifier, (obj as WildEncounterPokemon).Species.Identifier);
    }

    public bool shouldSerialize() => Occurrences.Count != 0;
  }

  /// <summary>
  /// This class represents a specific wild encounter habitat group.
  /// </summary>
  public class WildEncounterHabitat : ObservableObject {
    private string type;
    private int encounterRate = 0;
    private SortedObservableCollection<WildEncounterPokemon> pokemon;

    public WildEncounterHabitat() {
      Pokemon = new SortedObservableCollection<WildEncounterPokemon>();
    }
    public string Type { get => type; set => Set(ref type, value); }
    public int EncounterRate { get => encounterRate; set => Set(ref encounterRate, value); }
    public SortedObservableCollection<WildEncounterPokemon> Pokemon {
      get => pokemon;
      set => SetAndTrackItemUpdates(ref pokemon, value, this);
    }
    public bool shouldSerialize() => Pokemon.Any(pkmn => pkmn.shouldSerialize());
  }

  /// <summary>
  /// This class represents a specific wild encounter pokemon group.
  /// </summary>
  public class WildEncounterMap : ObservableObject {
    string mapName;
    string mapGroup;
    private ObservableCollection<WildEncounterHabitat> habitats;

    public string MapName { get => mapName; set => Set(ref mapName, value); }
    public string MapGroup { get => mapGroup; set => Set(ref mapGroup, value); }
    public ObservableCollection<WildEncounterHabitat> Habitats {
      get => habitats;
      set => SetAndTrackItemUpdates(ref habitats, value, this);
    }
    public bool shouldSerialize() => Habitats.Any(hab => hab.shouldSerialize());
  }

  /// <summary>
  /// This class represents the wild encounter database.
  /// </summary>
  public class WildEncounterDatabase : DatabaseBase {
    private List<string> habitatTypes;
    private ObservableCollection<WildEncounterMap> wildEncounters;

    /// <summary>
    /// The name of the database.
    /// </summary>
    public override string Name => "Wild Encounter Database";

    /// <summary>
    /// Returns the wild encounter maps defined within the project.
    /// </summary>
    public ObservableCollection<WildEncounterMap> WildEncounters { get => wildEncounters; set => SetAndTrackItemUpdates(ref wildEncounters, value, this); }

    public WildEncounterDatabase() {
      habitatTypes = new List<string>();
      WildEncounters = new ObservableCollection<WildEncounterMap>();
    }

    /// <summary>
    /// Reset the data within this database.
    /// </summary>
    protected override void reset() {
      habitatTypes.Clear();
      WildEncounters.Clear();
    }

    /// <summary>
    /// Deserialize the wild encounter data from the project directory.
    /// </summary>
    /// <param name="deserializer"></param>
    protected override void deserialize(ProjectDeserializer deserializer) {
      Deserializer.deserialize(deserializer, this);
    }

    /// <summary>
    /// Serialize the wild encounter data to the project directory.
    /// </summary>
    /// <param name="serializer"></param>
    protected override void serialize(ProjectSerializer serializer) {
      Serializer.serialize(serializer, this);
    }

    class JSONDatabase {
      public class JSONWildEncounterOccurrence {
        public string Time { get; set; }
        public int? MidLevel { get; set; } = 0;
        public int LevelRange { get; set; }
        public int EncounterRate { get; set; }

        public JSONWildEncounterOccurrence() { }
        public JSONWildEncounterOccurrence(WildEncounterOccurrence occurrence) {
          MidLevel = occurrence.MidLevel == 0 ? new int?() : occurrence.MidLevel;
          LevelRange = occurrence.LevelRange;
          EncounterRate = occurrence.EncounterRate;
          Time = occurrence.Time;
        }
        public WildEncounterOccurrence deserialize() {
          return new WildEncounterOccurrence() {
            MidLevel = MidLevel ?? 0,
            LevelRange = LevelRange,
            EncounterRate = EncounterRate,
            Time = Time
          };
        }
      }
      public class JSONWildEncounterPokemon {
        public string Species { get; set; }
        public JSONWildEncounterOccurrence[] Occurrences { get; set; }

        public JSONWildEncounterPokemon() { }
        public JSONWildEncounterPokemon(WildEncounterPokemon pokemon) {
          Species = pokemon.Species.Identifier;
          Occurrences = pokemon.Occurrences.Select(occurrence => new JSONWildEncounterOccurrence(occurrence)).ToArray();
        }
        public WildEncounterPokemon deserialize() {
          return new WildEncounterPokemon() {
            Species = Project.Instance.Species.getFromId(Species),
            Occurrences = new SortedObservableCollection<WildEncounterOccurrence>(Occurrences.Select(occurrence => occurrence.deserialize()))
          };
        }
      }
      public class JSONWildEncounterHabitat {
        public string Type { get; set; }
        public int EncounterRate { get; set; }
        public JSONWildEncounterPokemon[] Pokemon { get; set; }

        public JSONWildEncounterHabitat() { }
        public JSONWildEncounterHabitat(WildEncounterHabitat habitat) {
          Type = habitat.Type;
          EncounterRate = habitat.EncounterRate;
          Pokemon = habitat.Pokemon.Where(pkmn => pkmn.shouldSerialize()).Select(pkmn => new JSONWildEncounterPokemon(pkmn)).ToArray();
        }
        public WildEncounterHabitat deserialize() {
          return new WildEncounterHabitat() {
            Type = Type,
            EncounterRate = EncounterRate,
            Pokemon = new SortedObservableCollection<WildEncounterPokemon>(Pokemon.Select(pkmn => pkmn.deserialize()))
          };
        }
      }
      public class JSONWildEncounterMap {
        public string MapName { get; set; }
        public JSONWildEncounterHabitat[] Habitats { get; set; }

        public JSONWildEncounterMap() { }
        public JSONWildEncounterMap(WildEncounterMap map) {
          MapName = map.MapName;
          Habitats = map.Habitats.Where(hab => hab.shouldSerialize()).Select(hab => new JSONWildEncounterHabitat(hab)).ToArray();
        }
        public void deserializeInto(WildEncounterMap map) {
          Array.Sort(Habitats, (a, b) => a.Type.CompareTo(b.Type));
          int i = 0;
          foreach (JSONWildEncounterHabitat hab in Habitats) {
            while (i < map.Habitats.Count && hab.Type != map.Habitats[i].Type)
              ++i;
            Debug.Assert(i != map.Habitats.Count, "expected map habitat entry for " + hab.Type);
            map.Habitats[i] = hab.deserialize();
          }
        }
      }

      public JSONDatabase() { }
      public JSONDatabase(WildEncounterDatabase database) {
        HabitatTypes = database.habitatTypes.ToArray();
        WildEncounters = database.wildEncounters.Where(map => map.shouldSerialize()).Select(map => new JSONWildEncounterMap(map)).ToArray();
      }

      public void deserializeInto(WildEncounterDatabase database, OrderedDictionary<string, int> mapNameToIndex) {
        Array.Sort(HabitatTypes);
        database.habitatTypes = HabitatTypes.ToList();
        foreach (WildEncounterMap map in database.WildEncounters) {
          foreach (string hab in database.habitatTypes) {
            map.Habitats.Add(new WildEncounterHabitat() {
              Type = hab
            });
          }
        }
        foreach (JSONWildEncounterMap map in WildEncounters) {
          if (mapNameToIndex.TryGetValue(map.MapName, out int index))
            map.deserializeInto(database.wildEncounters[index]);
        }
      }

      public string[] HabitatTypes { get; set; }
      public JSONWildEncounterMap[] WildEncounters { get; set; }
    }

    class Deserializer {
      public static void deserialize(ProjectDeserializer deserializer, WildEncounterDatabase database) {
        OrderedDictionary<string, int> mapNameToIndex = new OrderedDictionary<string, int>();
        deserializeMaps(deserializer, database, mapNameToIndex);

        string jsonPath = Path.Combine(deserializer.project.ProjectDir, "src", "data", "wild_encounters.json");
        JSONDatabase jsonDatabase = JsonSerializer.Deserialize<JSONDatabase>(File.ReadAllText(jsonPath));
        jsonDatabase.deserializeInto(database, mapNameToIndex);
      }
      private static void deserializeMaps(ProjectDeserializer deserializer, WildEncounterDatabase database,
                                          OrderedDictionary<string, int> mapNameToIndex) {
        string currentGroupName = "";
        deserializer.deserializeFile((stream) => {
          string line = stream.ReadLine();
          if (line.tryExtractPrefix("// Map Group ", " ", out string newGroupName)) {
            currentGroupName = newGroupName;
            return;
          }
          if (line.tryExtractPrefix("#define MAP_", " ", out string mapName)) {
            mapNameToIndex.Add(mapName, database.wildEncounters.Count);
            database.wildEncounters.Add(new WildEncounterMap() {
              MapGroup = currentGroupName,
              MapName = mapName,
              Habitats = new ObservableCollection<WildEncounterHabitat>()
            });
          }
        }, "include", "constants", "map_groups.h");
      }
    }

    class Serializer {
      public static void serialize(ProjectSerializer serializer, WildEncounterDatabase database) {
        JSONDatabase jsonDatabase = new JSONDatabase(database);
        string json = JsonSerializer.Serialize(jsonDatabase, new JsonSerializerOptions() {
          IgnoreNullValues = true,
          WriteIndented = true
        });
        File.WriteAllText(Path.Combine(serializer.project.ProjectDir, "src", "data", "wild_encounters.json"), json);
      }
    }
  }
}
