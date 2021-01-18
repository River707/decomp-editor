using DecompEditor.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace DecompEditor.ProjectData {
  public class AdventureTaskCondition : ObservableObject {
    private string devComment;

    /// <summary>
    /// A dev comment describing the nature of this condition.
    /// </summary>
    public string DevComment { get => devComment; set => Set(ref devComment, value); }
  }
  public class AdventureTaskFlagCondition : AdventureTaskCondition {
    private string identifier;
    private bool value;

    /// <summary>
    /// The identifier of the condition.
    /// </summary>
    public string Identifier { get => identifier; set => Set(ref identifier, value); }
    /// <summary>
    /// The expected value of the condition.
    /// </summary>
    public bool Value {
      get => value;
      set => Set(ref this.value, value);
    }

    /// <summary>
    /// Create a default condition.
    /// </summary>
    /// <returns></returns>
    static public AdventureTaskFlagCondition createDefault() {
      return new AdventureTaskFlagCondition() {
        Identifier = "FLAG_TEMP_1",
        Value = true
      };
    }
  }
  public class AdventureTaskVariableCondition : AdventureTaskCondition {
    private string identifier;
    private int value;

    /// <summary>
    /// The identifier of the condition.
    /// </summary>
    public string Identifier { get => identifier; set => Set(ref identifier, value); }
    /// <summary>
    /// The expected value of the condition.
    /// </summary>
    public int Value { get => value; set => Set(ref this.value, value); }

    /// <summary>
    /// Create a default condition.
    /// </summary>
    /// <returns></returns>
    static public AdventureTaskVariableCondition createDefault() {
      return new AdventureTaskVariableCondition() {
        Identifier = "VAR_TEMP_0",
        Value = 0
      };
    }
  }
  public class AdventureTaskTaskCondition : AdventureTaskCondition {
    AdventureTask task;
    ValueStates value;

    /// <summary>
    /// The possible value states of an Adventure Task.
    /// </summary>
    public enum ValueStates {
      Unlocked,
      InProgress,
      Completed,
      Failure
    }

    /// <summary>
    /// The identifier of the condition.
    /// </summary>
    public AdventureTask Task { get => task; set => Set(ref task, value); }

    /// <summary>
    /// The expected value of the condition.
    /// </summary>
    public ValueStates Value { get => value; set => Set(ref this.value, value); }

    /// <summary>
    /// Create a default condition.
    /// </summary>
    /// <returns></returns>
    static public AdventureTaskTaskCondition createDefault(AdventureTask task) {
      return new AdventureTaskTaskCondition() {
        Task = task,
        Value = ValueStates.Completed
      };
    }
  }
  public class AdventureTask : ObservableObject {
    string title;
    string identifier;
    string inProgressDescription;
    string failureDescription;
    string completedDescription;
    string pointOfContact;
    string pointOfContactLocalIdentifier;
    string pointOfContactGraphicsIdentifier;
    string location;
    ObservableCollection<AdventureTaskCondition> preConditions;
    ObservableCollection<AdventureTaskCondition> failureConditions;

    /// <summary>
    /// The parent adventure of this task.
    /// </summary>
    public Adventure Parent { get; set; }

    /// <summary>
    /// The title of the task.
    /// </summary>
    public string Title { get => title; set => Set(ref title, value); }

    /// <summary>
    /// The identifier of the task.
    /// </summary>
    public string Identifier { 
      get => identifier;
      set {
        var oldIdentifier = identifier;
        if (!Set(ref identifier, value) || oldIdentifier == null || Parent == null)
          return;
        Project.Instance.registerFileReplacement("ADVENTURE_" + Parent.Identifier + "_TASK_" + oldIdentifier,
                                                 "ADVENTURE_" + FullIdentifier);
      }
    }

    /// <summary>
    /// The full identifier of the task.
    /// </summary>
    public string FullIdentifier => Parent.Identifier + "_TASK_" + Identifier;
    
    /// <summary>
    /// The description of the task when it is in progress.
    /// </summary>
    public string InProgressDescription { get => inProgressDescription; set => Set(ref inProgressDescription, value); }

    /// <summary>
    /// The description of the task when it has failed.
    /// </summary>
    public string FailureDescription { get => failureDescription; set => Set(ref failureDescription, value); }

    /// <summary>
    /// The description of the task when it has been completed.
    /// </summary>
    public string CompletedDescription { get => completedDescription; set => Set(ref completedDescription, value); }

    /// <summary>
    /// The name of the point of contact for this task, or empty if there is none.
    /// </summary>
    public string PointOfContact { get => pointOfContact; set => Set(ref pointOfContact, value); }

    /// <summary>
    /// The local identifier of the point of contact for this task, or empty if there is none.
    /// </summary>
    public string PointOfContactLocalIdentifier {
      get => pointOfContactLocalIdentifier;
      set => Set(ref pointOfContactLocalIdentifier, value);
    }

    /// <summary>
    /// The graphics identifier of the point of contact for this task, or empty if there is none.
    /// </summary>
    public string PointOfContactGraphicsIdentifier {
      get => pointOfContactGraphicsIdentifier;
      set => Set(ref pointOfContactGraphicsIdentifier, value);
    }

    /// <summary>
    /// The map location of this task.
    /// </summary>
    public string Location { get => location; set => Set(ref location, value); }

    /// <summary>
    /// The pre-conditions of this task.
    /// </summary>
    public ObservableCollection<AdventureTaskCondition> PreConditions {
      get => preConditions;
      set => SetAndTrackItemUpdates(ref preConditions, value, this);
    }

    /// <summary>
    /// The failure conditions of this task.
    /// </summary>
    public ObservableCollection<AdventureTaskCondition> FailureConditions {
      get => failureConditions;
      set => SetAndTrackItemUpdates(ref failureConditions, value, this);
    }

    public override string ToString() => Identifier + " : " + Title;
  }
  public class Adventure : ObservableObject {
    string title;
    string devTitle;
    string identifier;
    ObservableCollection<AdventureTask> tasks;

    /// <summary>
    /// The title of the adventure.
    /// </summary>
    public string Title { get => title; set => Set(ref title, value); }

    /// <summary>
    /// A dev title of the adventure.
    /// </summary>
    public string DevTitle { get => devTitle; set => Set(ref devTitle, value); }

    /// <summary>
    /// The identifier of the adventure.
    /// </summary>
    public string Identifier {
      get => identifier;
      set {
        var oldIdentifier = identifier;
        if (!Set(ref identifier, value) || oldIdentifier == null)
          return;
        Project.Instance.registerFileReplacement("ADVENTURE_" + oldIdentifier, "ADVENTURE_" + identifier);
        foreach (var task in Tasks)
          Project.Instance.registerFileReplacement("ADVENTURE_" + oldIdentifier + "_TASK_" + task.Identifier,
                                                   "ADVENTURE_" + identifier + "_TASK_" + task.Identifier);
      }
    }

    /// <summary>
    /// The tasks of the adventure.
    /// </summary>
    public ObservableCollection<AdventureTask> Tasks {
      get => tasks;
      set {
        SetAndTrackItemUpdates(ref tasks, value, this);
        foreach (var task in tasks)
          task.Parent = this;
      }
    }

    /// <summary>
    /// Return the task with the corresponding identifier.
    /// </summary>
    /// <param name="identifier"></param>
    /// <returns></returns>
    public AdventureTask getTask(string identifier) {
      return tasks.First(task => task.Identifier == identifier);
    }

    public void AddDefaultTask() {
      tasks.Add(new AdventureTask() {
        Parent = this,
        Title = "Task Num " + tasks.Count,
        Identifier = "NUM_" + tasks.Count,
        InProgressDescription = "",
        FailureDescription = "",
        CompletedDescription = "",
        PointOfContact = "",
        PointOfContactLocalIdentifier = "",
        PointOfContactGraphicsIdentifier = "",
        Location = "ROUTE1",
        PreConditions = new ObservableCollection<AdventureTaskCondition>(),
        FailureConditions = new ObservableCollection<AdventureTaskCondition>()
      });
    }
  }
  public class AdventureDatabase : DatabaseBase {
    private ObservableCollection<Adventure> adventures;

    /// <summary>
    /// The name of the database.
    /// </summary>
    public override string Name => "Adventure Database";

    /// <summary>
    /// Returns the adventures defined within the project.
    /// </summary>
    public ObservableCollection<Adventure> Adventures {
      get => adventures;
      private set => SetAndTrackItemUpdates(ref adventures, value, this);
    }

    /// <summary>
    /// Add a new default adventure.
    /// </summary>
    /// <returns></returns>
    public Adventure addNewAdventure() {
      var adventure = new Adventure() {
        Title = "Adventure " + Adventures.Count,
        DevTitle = "Adventure " + Adventures.Count,
        Identifier = "ADVENTURE_" + Adventures.Count,
        Tasks = new ObservableCollection<AdventureTask>()
      };
      adventure.AddDefaultTask();
      Adventures.Add(adventure);
      return adventure;
    }

    /// <summary>
    /// Return the adventure corresponding to the given identifier.
    /// </summary>
    /// <param name="identifier"></param>
    /// <returns></returns>
    public Adventure getAdventureFromId(string identifier) {
      return Adventures.First(adventure => adventure.Identifier == identifier);
    }
    /// <summary>
    /// Return the adventure task corresponding to the given identifier.
    /// </summary>
    /// <param name="identifier"></param>
    /// <returns></returns>
    public AdventureTask getTaskFromFullId(string identifier) {
      string[] adventureAndTask = identifier.Split("_TASK_");
      return getAdventureFromId(adventureAndTask[0]).getTask(adventureAndTask[1]);
    }

    public AdventureDatabase() {
      Adventures = new ObservableCollection<Adventure>();
    }

    /// <summary>
    /// Resets the data within this database.
    /// </summary>
    protected override void reset() => Adventures.Clear();

    /// <summary>
    /// Deserialize the adventures from the project directory.
    /// </summary>
    /// <param name="serializer"></param>
    protected override void deserialize(ProjectDeserializer serializer) {
      Deserializer.deserialize(serializer, this);
    }

    /// <summary>
    /// Serialize the adventures to the project directory.
    /// </summary>
    /// <param name="serializer"></param>
    protected override void serialize(ProjectSerializer serializer) {
      Serializer.serialize(serializer, this);
    }

    class JSONDatabase {
      public class JSONAdventureTaskCondition {
        public string Identifier { get; set; }
        public string Value { get; set; }
        public string DevComment { get; set; } = "";
        public string Type { get; set; }

        public JSONAdventureTaskCondition() { }
        public JSONAdventureTaskCondition(AdventureTaskCondition condition) {
          DevComment = condition.DevComment;

          if (condition is AdventureTaskFlagCondition flagCondition) {
            Identifier = flagCondition.Identifier;
            Value = flagCondition.Value ? "1" : "0";
            Type = "Flag";
            return;
          }
          if (condition is AdventureTaskVariableCondition varCondition) {
            Identifier = varCondition.Identifier;
            Value = varCondition.Value.ToString();
            Type = "Var";
            return;
          }
          if (condition is AdventureTaskTaskCondition taskCondition) {
            Identifier = taskCondition.Task.FullIdentifier;
            Value = taskCondition.Value.ToString();
            Type = "Task";
            return;
          }
          throw new Exception("unknown adventure task condition type");
        }
        public AdventureTaskCondition deserialize(Dictionary<AdventureTaskTaskCondition, string> taskConditionToId) {
          if (Type == "Flag") {
            return new AdventureTaskFlagCondition() {
              Identifier = Identifier,
              Value = Value == "1" ? true : false,
              DevComment = DevComment
            };
          }
          if (Type == "Var") {
            return new AdventureTaskVariableCondition() {
              Identifier = Identifier,
              Value = int.Parse(Value),
              DevComment = DevComment
            };
          }
          var condition = new AdventureTaskTaskCondition() {
            Value = Enum.Parse<AdventureTaskTaskCondition.ValueStates>(Value),
            DevComment = DevComment,
          };
          taskConditionToId.Add(condition, Identifier);
          return condition;
        }
      }
      public class JSONAdventureTask {
        public string Title { get; set; }
        public string Identifier { get; set; }
        public string InProgressDescription { get; set; }
        public string FailureDescription { get; set; } = "";
        public string CompletedDescription { get; set; }
        public string PointOfContact { get; set; } = "";
        public string PointOfContactLocalIdentifier { get; set; } = "";
        public string PointOfContactGraphicsIdentifier { get; set; } = "";
        public string Location { get; set; }
        public JSONAdventureTaskCondition[] PreConditions { get; set; } = new JSONAdventureTaskCondition[0];
        public JSONAdventureTaskCondition[] FailureConditions { get; set; } = new JSONAdventureTaskCondition[0];

        public JSONAdventureTask() { }
        public JSONAdventureTask(AdventureTask task) {
          Title = task.Title;
          Identifier = task.Identifier;
          InProgressDescription = task.InProgressDescription;
          FailureDescription = task.FailureDescription;
          CompletedDescription = task.CompletedDescription;
          if (task.PointOfContact.Length == 0) {
            PointOfContact = PointOfContactGraphicsIdentifier = PointOfContactLocalIdentifier = null;
          } else {
            PointOfContact = task.PointOfContact;
            PointOfContactLocalIdentifier = task.PointOfContactLocalIdentifier;
            PointOfContactGraphicsIdentifier = task.PointOfContactGraphicsIdentifier;
          }
          Location = task.Location;
          PreConditions = task.PreConditions.Count == 0
            ? null
            : task.PreConditions.Select(condition => new JSONAdventureTaskCondition(condition)).ToArray();
          FailureConditions = task.FailureConditions.Count == 0
            ? null
            : task.FailureConditions.Select(condition => new JSONAdventureTaskCondition(condition)).ToArray();
        }
        public AdventureTask deserialize(Dictionary<AdventureTaskTaskCondition, string> taskConditionToId) {
          return new AdventureTask() {
            Title = Title,
            Identifier = Identifier,
            InProgressDescription = InProgressDescription,
            FailureDescription = FailureDescription,
            CompletedDescription = CompletedDescription,
            PointOfContact = PointOfContact,
            PointOfContactLocalIdentifier = PointOfContactLocalIdentifier,
            PointOfContactGraphicsIdentifier = PointOfContactGraphicsIdentifier,
            Location = Location,
            PreConditions = new ObservableCollection<AdventureTaskCondition>(PreConditions.Select(condition => condition.deserialize(taskConditionToId))),
            FailureConditions = new ObservableCollection<AdventureTaskCondition>(FailureConditions.Select(condition => condition.deserialize(taskConditionToId)))
          };
        }
      }
      public class JSONAdventure {
        public JSONAdventure() { }
        public JSONAdventure(Adventure adventure) {
          Title = adventure.Title;
          DevTitle = adventure.DevTitle;
          Identifier = adventure.Identifier;
          Tasks = adventure.Tasks.Select(task => new JSONAdventureTask(task)).ToArray();
        }
        public Adventure deserialize(Dictionary<AdventureTaskTaskCondition, string> taskConditionToId) {
          return new Adventure() {
            Title = Title,
            DevTitle = DevTitle,
            Identifier = Identifier,
            Tasks = new ObservableCollection<AdventureTask>(Tasks.Select(task => task.deserialize(taskConditionToId)))
          };
        }

        public string Title { get; set; }
        public string DevTitle { get; set; } = "";
        public string Identifier { get; set; }
        public JSONAdventureTask[] Tasks { get; set; }
      }

      public JSONDatabase() { }
      public JSONDatabase(AdventureDatabase database) {
        Adventures = database.Adventures.Select(adventure => new JSONAdventure(adventure)).ToArray();
      }

      public void deserializeInto(AdventureDatabase database) {
        Dictionary<AdventureTaskTaskCondition, string> taskConditionToId = new Dictionary<AdventureTaskTaskCondition, string>();
        database.Adventures = new ObservableCollection<Adventure>(Adventures.Select(adventure => adventure.deserialize(taskConditionToId)));

        // Now that all of the adventures have been deserialized, resolve the Task conditions.
        if (taskConditionToId.Count != 0) {
          Dictionary<string, AdventureTask> idToTask = new Dictionary<string, AdventureTask>();
          foreach (var adventure in database.Adventures)
            foreach (var task in adventure.Tasks)
              idToTask.Add(task.FullIdentifier, task);
          foreach (var adventure in database.Adventures) {
            foreach (var task in adventure.Tasks) {
              foreach (var condition in task.PreConditions) {
                if (condition is AdventureTaskTaskCondition taskCondition)
                  taskCondition.Task = idToTask[taskConditionToId[taskCondition]];
              }
              foreach (var condition in task.FailureConditions) {
                if (condition is AdventureTaskTaskCondition taskCondition)
                  taskCondition.Task = idToTask[taskConditionToId[taskCondition]];
              }
            }
          }
        }
      }

      public JSONAdventure[] Adventures { get; set; }
    }

    class Deserializer {
      public static void deserialize(ProjectDeserializer deserializer, AdventureDatabase database) {
        string jsonPath = Path.Combine(deserializer.project.ProjectDir, "src", "data", "adventures.json");
        JSONDatabase jsonDatabase = JsonSerializer.Deserialize<JSONDatabase>(File.ReadAllText(jsonPath));
        jsonDatabase.deserializeInto(database);
      }
    }

    class Serializer {
      public static void serialize(ProjectSerializer serializer, AdventureDatabase database) {
        JSONDatabase jsonDatabase = new JSONDatabase(database);
        string json = JsonSerializer.Serialize(jsonDatabase, new JsonSerializerOptions() {
          IgnoreNullValues = true,
          WriteIndented = true
        });
        File.WriteAllText(Path.Combine(serializer.project.ProjectDir, "src", "data", "adventures.json"), json);
      }
    }
  }
}
