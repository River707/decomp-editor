using DecompEditor.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Truncon.Collections;

namespace DecompEditor {
  /// <summary>
  /// This class represents a specific game segment.
  /// </summary>
  public class DifficultyGameSegment : ObservableObject {
    private string completionFlag;
    private int minimumLevel;
    private int maximumLevel;

    public string CompletionFlag { get => completionFlag; set => Set(ref completionFlag, value); }
    public int MinimumLevel { get => minimumLevel; set => Set(ref minimumLevel, value); }
    public int MaximumLevel { get => maximumLevel; set => Set(ref maximumLevel, value); }

    /// <summary>
    /// Returns true if this segment is the unknown segment.
    /// </summary>
    public bool IsUnknown => this == UnknownSegment;
    public bool IsKnown => !IsUnknown;
    /// <summary>
    /// Returns the index of this segment, or null if this is the unknown segment.
    /// </summary>
    public int? Index => IsUnknown ? new int?() : Project.Instance.Difficulty.GameSegments.IndexOf(this);

    /// <summary>
    /// A singleton representing an unknown game segment, i.e. it could be any segment in the game.
    /// </summary>
    static public DifficultyGameSegment UnknownSegment = new DifficultyGameSegment();

    public override string ToString() {
      if (IsUnknown)
        return "???";
      return "[" + MinimumLevel + ", " + MaximumLevel + "]";
    }

    /// <summary>
    /// Different subsections of a game segment.
    /// </summary>
    public enum Section { 
      Early,
      Middle,
      Late
    }

    public void generateDataForTrainer(Trainer trainer, Section section) {
      // We don't generate data for unknown segments.
      if (IsUnknown)
        return;

      // Compute the level range for the current section.
      int fullLevelRange = maximumLevel - minimumLevel;
      int sectionLevelRange = fullLevelRange / 3 + ((fullLevelRange % 3) != 0 ? 1 : 0);
      int sectionStart = minimumLevel + (int)section * sectionLevelRange;
      int sectionEnd = sectionStart + sectionLevelRange;

      Random random = new Random();
      foreach (Pokemon pkmn in trainer.Party.Pokemon)
        pkmn.Level = random.Next(sectionStart, sectionEnd);
    }
  }

  /// <summary>
  /// This class represents the difficulty database.
  /// </summary>
  public class DifficultyDatabase : DatabaseBase {
    private ObservableCollection<DifficultyGameSegment> gameSegments;

    /// <summary>
    /// The name of the database.
    /// </summary>
    public override string Name => "Difficulty Database";

    /// <summary>
    /// Returns the game segments defined within the project.
    /// </summary>
    public ObservableCollection<DifficultyGameSegment> GameSegments { get => gameSegments; set => SetAndTrackItemUpdates(ref gameSegments, value, this); }

    public DifficultyDatabase() {
      GameSegments = new ObservableCollection<DifficultyGameSegment>();
    }

    /// <summary>
    /// Reset the data within this database.
    /// </summary>
    protected override void reset() {
      GameSegments.Clear();
    }

    /// <summary>
    /// Deserialize the difficulty data from the project directory.
    /// </summary>
    /// <param name="deserializer"></param>
    protected override void deserialize(ProjectDeserializer deserializer) {
      Deserializer.deserialize(deserializer, this);
    }

    /// <summary>
    /// Serialize the difficulty data to the project directory.
    /// </summary>
    /// <param name="serializer"></param>
    protected override void serialize(ProjectSerializer serializer) {
      Serializer.serialize(serializer, this);
    }

    class JSONDatabase {
      public JSONDatabase() { }
      public JSONDatabase(DifficultyDatabase database) {
        GameSegments = database.GameSegments.ToArray();
      }

      public void deserializeInto(DifficultyDatabase database) {
        database.GameSegments = new ObservableCollection<DifficultyGameSegment>(GameSegments);
      }

      public DifficultyGameSegment[] GameSegments { get; set; }
    }

    class Deserializer {
      public static void deserialize(ProjectDeserializer deserializer, DifficultyDatabase database) {
        string jsonPath = Path.Combine(deserializer.project.ProjectDir, "src", "data", "difficulty.json");
        JSONDatabase jsonDatabase = JsonSerializer.Deserialize<JSONDatabase>(File.ReadAllText(jsonPath));
        jsonDatabase.deserializeInto(database);
      }
    }

    class Serializer {
      public static void serialize(ProjectSerializer serializer, DifficultyDatabase database) {
        JSONDatabase jsonDatabase = new JSONDatabase(database);
        string json = JsonSerializer.Serialize(jsonDatabase, new JsonSerializerOptions() {
          IgnoreNullValues = true,
          WriteIndented = true
        });
        File.WriteAllText(Path.Combine(serializer.project.ProjectDir, "src", "data", "difficulty.json"), json);
      }
    }
  }
}
