using DecompEditor.ProjectData;
using DecompEditor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DecompEditor {
  public class Project {
    readonly Dictionary<string, string> fileReplacements = new Dictionary<string, string>();
    readonly List<DatabaseBase> databases = new List<DatabaseBase>();
    private string projectDir;
    internal AdventureDatabase Adventures { get; } = new AdventureDatabase();
    internal BattleAIDatabase BattleAI { get; } = new BattleAIDatabase();
    internal EventObjectDatabase EventObjects { get; } = new EventObjectDatabase();
    internal ItemDatabase Items { get; } = new ItemDatabase();
    internal MoveDatabase Moves { get; } = new MoveDatabase();
    internal PokemonSpeciesDatabase Species { get; } = new PokemonSpeciesDatabase();
    internal TrainerEncounterMusicDatabase TrainerEncounterMusic { get; } = new TrainerEncounterMusicDatabase();
    internal TrainerDatabase Trainers { get; } = new TrainerDatabase();
    internal string ProjectDir { get => projectDir; private set => projectDir = FileUtils.normalizePath(value); }

    internal bool IsDirty => databases.Any(db => db.IsDirty);
    internal bool IsLoading { get; private set; } = false;

    /// <summary>
    /// The main instance of the project being transformed.
    /// </summary>
    public static Project Instance { get; private set; } = new Project();

    /// <summary>
    /// The main logger instance of the project.
    /// </summary>
    public static NLog.Logger Logger { get; private set; } = NLog.LogManager.GetCurrentClassLogger();

    /// <summary>
    /// The file containing the editor logs.
    /// </summary>
    public static string LogFileName 
    => System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DecompEditor", "decompEditor.log");

    private Project() {
      registerDatabases(Adventures, BattleAI, EventObjects, Items, Moves, Species,
                        TrainerEncounterMusic, Trainers);
    }
    void registerDatabases(params DatabaseBase[] databases) => this.databases.AddRange(databases);

    /// <summary>
    /// Event for when a project is loaded.
    /// </summary>
    public delegate void LoadEventHandler();
    public event LoadEventHandler Loaded;

    /// <summary>
    /// Load the project at the provided project directory.
    /// </summary>
    public void load(string projectDir) {
      IsLoading = true;
      ProjectDir = projectDir;
      fileReplacements.Clear();
      var deserializer = new ProjectDeserializer(this);

      Logger.Info("Loading project located at: {ProjectDir}", projectDir);

      // Load the databases.
      foreach (var db in databases) {
        try {
          db.load(deserializer);
        } catch (Exception e) {
          MessageBox.Show($"Failed to load project, see {LogFileName} for more details");
          Logger.Error(e, "Failed to load {Database} from the project directory", db.Name);

          foreach (DatabaseBase database in databases)
            database.clear();
        }
      }

      // Process any file replacements that arise during upgrades.
      processFileReplacements();

      // Signal to all of the listeners that the project is loaded.
      Loaded?.Invoke();
      IsLoading = false;
    }

    /// <summary>
    /// Save the current project.
    /// </summary>
    public void save() {
      // Process any requested file replacements.
      processFileReplacements();

      var serializer = new ProjectSerializer(this);
      foreach (DatabaseBase database in databases)
        database.save(serializer);
    }

    /// <summary>
    /// Request a string replacement within the files of the project.
    /// </summary>
    public void registerFileReplacement(string from, string to) {
      foreach (KeyValuePair<string, string> kv in fileReplacements) {
        if (kv.Value == from) {
          if (kv.Key == to) {
            fileReplacements.Remove(to);
            return;
          }

          from = kv.Key;
          break;
        }
      }
      fileReplacements[from] = to;
    }

    /// <summary>
    /// Process any requested file replacements.
    /// </summary>
    void processFileReplacements() {
      if (fileReplacements.Count != 0) {
        string identifierRegex = "(^|[^0-9_a-zA-Z]){0}([^0-9_a-zA-Z]|$)", identifierRepl = "$1{0}$2";
        FileUtils.replaceInFiles(
          ProjectDir,
          fileReplacements.Select(kv => new KeyValuePair<string, string>(string.Format(identifierRegex, kv.Key),
                                                                         string.Format(identifierRepl, kv.Value))).ToList(),
          "*.c|*.h|*.inc|*.json|*.mk|*.pory");
        fileReplacements.Clear();
      }
    }
  }
}
