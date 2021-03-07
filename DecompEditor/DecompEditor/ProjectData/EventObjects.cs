using DecompEditor.ParserUtils;
using DecompEditor.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using Truncon.Collections;
using DecompEditor.ProjectData;
using System.Drawing;
using System.Windows.Controls.Primitives;
using System.Text.Json;

namespace DecompEditor {
  /// <summary>
  /// This class represents a specific event object within the project.
  /// </summary>
  public class EventObject : ObservableObject {
    private string identifier;
    private int width;
    private int height;
    private string shadowSize;
    private bool inanimate;
    private string tracks;
    private EventObjectAnimTable animations;
    private string affineAnimations;
    private ObservableCollection<Frame> frames;

    public EventObject() => Frames = new ObservableCollection<Frame>();

    /// <summary>
    /// This class represents a single frame of an object event pic table.
    /// </summary>
    public class Frame : ObservableObject {
      public Frame() { }
      public Frame(Frame other) {
        Pic = other.Pic;
        Index = other.Index;
      }
      private EventObjectPic pic;
      private int index = 0;

      public EventObjectPic Pic { get => pic; set => SetAndTrack(ref pic, value); }
      public int Index { get => index; set => Set(ref index, value); }
    }

    /// <summary>
    /// The C identifier of the event object.
    /// </summary>
    public string Identifier {
      get => identifier;
      set {
        if (identifier != null) {
          Project.Instance.registerFileReplacement(
            "OBJ_EVENT_GFX_" + identifier.fromPascalToSnake(), 
            "OBJ_EVENT_GFX_" + value.fromPascalToSnake());
        }

        Set(ref identifier, value);
      }
    }
    /// <summary>
    /// The pixel width of the object.
    /// </summary>
    public int Width { get => width; set => Set(ref width, value); }
    /// <summary>
    /// The pixel height of the object.
    /// </summary>
    public int Height { get => height; set => Set(ref height, value); }
    /// <summary>
    /// The shadow size of the object.
    /// </summary>
    public string ShadowSize { get => shadowSize; set => Set(ref shadowSize, value); }
    /// <summary>
    /// Whether the object is animate or not.
    /// </summary>
    public bool Inanimate { get => inanimate; set => Set(ref inanimate, value); }
    /// <summary>
    /// The type of tracks left by the object.
    /// </summary>
    public string Tracks { get => tracks; set => Set(ref tracks, value); }
    /// <summary>
    /// The animation table used by the object.
    /// </summary>
    public EventObjectAnimTable Animations { get => animations; set => SetAndTrack(ref animations, value); }
    /// <summary>
    /// The affine animation table used by the object.
    /// </summary>
    public string AffineAnimations { get => affineAnimations; set => Set(ref affineAnimations, value); }
    /// <summary>
    /// The animation frames used by the object.
    /// </summary>
    public ObservableCollection<Frame> Frames {
      get => frames; set => SetAndTrackItemUpdates(ref frames, value, this);
    }
  }
  /// <summary>
  /// This class represents an animation table within the project.
  /// </summary>
  public class EventObjectAnimTable {
    /// <summary>
    /// The C identifier of the animation table.
    /// </summary>
    public string Identifier { get; set; }
    /// <summary>
    /// A pretty name to use for the animation table.
    /// </summary>
    public string PrettyName { get; set; }
  }
  /// <summary>
  /// This class represents a specific event object picture.
  /// </summary>
  public class EventObjectPic : ObservableObject {
    private string identifier;
    private string path;
    private string fullPath;

    /// <summary>
    /// The C identifier of the picture.
    /// </summary>
    public string Identifier { get => identifier; set => Set(ref identifier, value); }
    /// <summary>
    /// The relative path of the picture within the project.
    /// </summary>
    public string Path {
      get => path;
      set {
        if (path != null)
          Project.Instance.registerFileReplacement(path + ".4bpp", value + ".4bpp");
        Set(ref path, value);
      }
    }
    /// <summary>
    /// The full file path of the picture.
    /// </summary>
    public string FullPath {
      get => fullPath;
      set => Set(ref fullPath, FileUtils.normalizePath(value));
    }
  }
  public class EventObjectDatabase : DatabaseBase {
    ObservableCollection<EventObjectPic> pics;
    readonly List<EventObjectAnimTable> animTables = new List<EventObjectAnimTable>();
    private ObservableCollection<EventObject> objects;

    /// <summary>
    /// The name of this database.
    /// </summary>
    public override string Name => "Event Object Database";

    /// <summary>
    /// Returns the event objects defined within the project.
    /// </summary>
    public ObservableCollection<EventObject> Objects { get => objects; set => SetAndTrackItemUpdates(ref objects, value, this); }

    /// <summary>
    /// Returns the animation tables defined within the project.
    /// </summary>
    public IEnumerable<EventObjectAnimTable> AnimTables => animTables;

    /// <summary>
    /// Returns the event object pictures defined within the project.
    /// </summary>
    public ObservableCollection<EventObjectPic> Pics { get => pics; set => SetAndTrackItemUpdates(ref pics, value, this); }

    /// <summary>
    /// Returns the shadow sizes defined within the project.
    /// </summary>
    public List<string> ShadowSizes { get; set; } = new List<string>();

    /// <summary>
    /// Returns the track types defined within the project.
    /// </summary>
    public List<string> TrackTypes { get; set; } = new List<string>();

    public EventObjectDatabase() {
      Objects = new ObservableCollection<EventObject>();
      Pics = new ObservableCollection<EventObjectPic>();
    }

    /// <summary>
    /// Reset the data within this database.
    /// </summary>
    protected override void reset() {
      animTables.Clear();
      Objects.Clear();
      Pics.Clear();
      ShadowSizes.Clear();
      TrackTypes.Clear();
    }

    /// <summary>
    /// Deserialize the event objects from the project directory.
    /// </summary>
    /// <param name="deserializer"></param>
    protected override void deserialize(ProjectDeserializer deserializer) {
      // Load the tracks and shadow sizes.
      Dictionary<string, EventObjectAnimTable> varToAnimTable = new Dictionary<string, EventObjectAnimTable>();
      Deserializer.loadAnimTables(deserializer, animTables, varToAnimTable);
      Deserializer.loadTracksAndShadowSizes(deserializer, ShadowSizes, TrackTypes);

      // Deserialize the object events.
      Deserializer.loadEventObjects(this, deserializer, varToAnimTable);
    }

    /// <summary>
    /// Serialize the event object data to the project directory.
    /// </summary>
    /// <param name="serializer"></param>
    protected override void serialize(ProjectSerializer serializer) {
      Serializer.serialize(serializer, this);
    }

    /// <summary>
    /// A simple JSON compatible representation for serializing event object data.
    /// </summary>
    public class JSONDatabase {
      public JSONDatabase() { }
      public JSONDatabase(EventObjectDatabase database) {
        EventObjects = database.Objects.Select(evtObject => new JSONEventObject(evtObject)).ToArray();
        Pics = database.Pics.Select(pic => new JSONEventObjectPic() {
          Identifier = pic.Identifier,
          Path = pic.Path
        }).ToArray();
      }

      public void deserializeInto(EventObjectDatabase database, Dictionary<string, EventObjectAnimTable> varToAnimTable) {
        Dictionary<string, EventObjectPic> varToPic = new Dictionary<string, EventObjectPic>();
        foreach (var pic in Pics) {
          database.Pics.Add(new EventObjectPic() {
            Identifier = pic.Identifier,
            Path = pic.Path,
            FullPath = Path.Combine(Project.Instance.ProjectDir, "graphics", "object_events", "pics", pic.Path + ".png")
          });
          varToPic.Add(pic.Identifier, database.Pics[^1]);
        }
        foreach (var obj in EventObjects)
          database.Objects.Add(obj.deserialize(varToAnimTable, varToPic));
      }

      public class JSONEventObjectPic {
        public JSONEventObjectPic() { }
        public string Identifier { get; set; }
        public string Path { get; set; }
      }
      public class JSONEventObject {
        public class JSONFrame {
          public JSONFrame() { }
          public JSONFrame(EventObject.Frame frame) {
            Identifier = frame.Pic.Identifier;
            StartIndex = frame.Index == 0 ? null : new int?(frame.Index);
            Count = null;
          }

          public string Identifier { get; set; }
          public int? StartIndex { get; set; } = 0;
          public int? Count { get; set; } = 1;
        }

        public JSONEventObject() { }
        public JSONEventObject(EventObject obj) {
          Identifier = obj.Identifier;
          Width = obj.Width;
          Height = obj.Height;
          ShadowSize = getOrNullClass(ShadowSize, obj.ShadowSize);
          Inanimate = getOrNullValue((bool)Inanimate, obj.Inanimate);
          Tracks = getOrNullClass(Tracks, obj.Tracks);
          Animations = obj.Animations.Identifier;
          AffineAnimations = getOrNullClass(AffineAnimations, obj.AffineAnimations);

          for (int i = 0; i != obj.Frames.Count; ++i) {
            if (i != 0) {
              if (obj.Frames[i - 1].Pic == obj.Frames[i].Pic &&
                  obj.Frames[i - 1].Index == obj.Frames[i].Index - 1) {
                if (Frames[^1].Count == null)
                  Frames[^1].Count = 2;
                else
                  ++Frames[^1].Count;
                continue;
              }
            }
            Frames.Add(new JSONFrame(obj.Frames[i]));
          }
        }

        public EventObject deserialize(Dictionary<string, EventObjectAnimTable> varToAnimTable,
                                       Dictionary<string, EventObjectPic> varToPic) {
          EventObject obj = new EventObject() {
            Identifier = Identifier,
            Width = Width,
            Height = Height,
            ShadowSize = ShadowSize,
            Inanimate = (bool)Inanimate,
            Tracks = Tracks,
            Animations = varToAnimTable[Animations],
            AffineAnimations = AffineAnimations
          };

          // Add the animation frames to the object.
          foreach (var frame in Frames) {
            for (int i = 0, e = (int)frame.Count; i != e; ++i) {
              obj.Frames.Add(new EventObject.Frame() {
                Pic = varToPic[frame.Identifier],
                Index = (int)frame.StartIndex + i
              });
            }
          }

          return obj;
        }

        /// <summary>
        /// If the given values are equal this method returns null, otherwise returns the new value.
        /// </summary>
        Nullable<T> getOrNullValue<T>(T value, T newValue) where T : struct {
          return EqualityComparer<T>.Default.Equals(value, newValue) ? null : new T?(newValue);
        }
        T getOrNullClass<T>(T value, T newValue) where T : class => EqualityComparer<T>.Default.Equals(value, newValue) ? null : newValue;

        public string Identifier { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string ShadowSize { get; set; } = "M";
        public bool? Inanimate { get; set; } = false;
        public string Tracks { get; set; } = "FOOT";
        public string Animations { get; set; }
        public string AffineAnimations { get; set; } = "gDummySpriteAffineAnimTable";
        public List<JSONFrame> Frames { get; set; } = new List<JSONFrame>();
      }
      public JSONEventObject[] EventObjects { get; set; }
      public JSONEventObjectPic[] Pics { get; set; }
    }

    static class Deserializer {
      public static void loadEventObjects(EventObjectDatabase database, ProjectDeserializer deserializer,
                                          Dictionary<string, EventObjectAnimTable> varToAnimTable) {
        string jsonPath = Path.Combine(deserializer.project.ProjectDir, "src", "data", "object_events", "event_objects.json");
        JSONDatabase jsonDatabase = JsonSerializer.Deserialize<JSONDatabase>(File.ReadAllText(jsonPath));
        jsonDatabase.deserializeInto(database, varToAnimTable);
      }

      public static void loadAnimTables(ProjectDeserializer deserializer, List<EventObjectAnimTable> animTables,
                                        Dictionary<string, EventObjectAnimTable> varToAnimTable) {
        deserializer.deserializeFile((reader) => {
          if (!reader.ReadLine().tryExtractPrefix("const union AnimCmd *const gObjectEventImageAnimTable_", "[", out string name))
            return;
          var animTable = new EventObjectAnimTable() {
            Identifier = "gObjectEventImageAnimTable_" + name,
            PrettyName = name.fromPascalToSentence()
          };
          varToAnimTable.Add(animTable.Identifier, animTable);
          animTables.Add(animTable);
        }, "src", "data", "object_events", "object_event_anims.h");
      }
      public static void loadTracksAndShadowSizes(ProjectDeserializer deserializer, List<string> shadowSizes,
                                           List<string> trackTypes) {
        deserializer.deserializeFile((stream) => {
          string line = stream.ReadLine();
          if (line.tryExtractPrefix("#define SHADOW_SIZE_", " ", out string enumName))
            shadowSizes.Add(enumName);
          else if (line.tryExtractPrefix("#define TRACKS_", " ", out enumName))
            trackTypes.Add(enumName);
        }, "include", "constants", "event_objects.h");
      }
    }

    static class Serializer {
      public static void serialize(ProjectSerializer serializer, EventObjectDatabase database) {
        updateSpriteSheetMakeRules(serializer, database);
        moveGraphicsPics(serializer, database);

        // Generate and save a JSON database.
        JSONDatabase jsonDatabase = new JSONDatabase(database);
        string json = JsonSerializer.Serialize(jsonDatabase, new JsonSerializerOptions() {
          IgnoreNullValues = true,
          WriteIndented = true
        });
        File.WriteAllText(Path.Combine(serializer.project.ProjectDir, "src", "data", "object_events", "event_objects.json"), json);
      }

      static void updateSpriteSheetMakeRules(ProjectSerializer serializer, EventObjectDatabase database) {
        var checkedFiles = new HashSet<EventObjectPic>();
        var checkedFilePaths = new HashSet<string>();
        var needSprites = new Dictionary<string, Tuple<int, int>>();
        foreach (EventObject obj in database.Objects) {
          int objWidth = obj.Width, objHeight = obj.Height;
          foreach (EventObject.Frame frame in obj.Frames) {
            if (!checkedFiles.Add(frame.Pic))
              continue;
            checkedFilePaths.Add(frame.Pic.Path);

            BitmapImage file = FileUtils.loadBitmapImage(frame.Pic.FullPath);
            if (file.PixelWidth != objWidth || file.PixelHeight != objHeight)
              needSprites.Add(frame.Pic.Path, new Tuple<int, int>(objWidth / 8, objHeight / 8));
          }
        }
        string[] curLines = File.ReadAllLines(Path.Combine(serializer.project.ProjectDir, "spritesheet_rules.mk"));
        bool updatedExistingSheet = false;
        for (int i = 0, e = curLines.Length; i != e; ++i) {
          if (!curLines[i].tryExtractPrefix("$(OBJEVENTGFXDIR)/", ".", out string linePath) ||
              !checkedFilePaths.Contains(linePath))
            continue;

          // If the sprite doesn't need a sprite sheet, remove it.
          if (!needSprites.TryGetValue(linePath, out Tuple<int, int> widthHeight)) {
            // TODO: We could also just remove the lines completely, i.e. null them out.
            curLines[i] = curLines[++i] = string.Empty;
            updatedExistingSheet = true;
            continue;
          }
          needSprites.Remove(linePath);

          string widthHeightLine = curLines[++i];
          int widthIndex = widthHeightLine.IndexOf("-mwidth") + "-mwidth ".Length;
          int widthIndexEnd = widthHeightLine.IndexOf(' ', widthIndex);

          int heightIndex = widthIndex + " -mheight ".Length;
          bool changed = widthHeight.Item1 != int.Parse(widthHeightLine.Substring(widthIndex, widthIndexEnd - widthIndex)) ||
                         widthHeight.Item2 != int.Parse(widthHeightLine.Substring(heightIndex));
          if (changed) {
            curLines[i] = string.Format("\t$(GFX) $< $@ -mwidth {0} -mheight {1}", widthHeight.Item1, widthHeight.Item2);
            updatedExistingSheet = true;
          }
        }

        if (updatedExistingSheet)
          File.WriteAllLines(Path.Combine(serializer.project.ProjectDir, "spritesheet_rules.mk"), curLines);
        if (needSprites.Count != 0) {
          var writer = new StreamWriter(Path.Combine(serializer.project.ProjectDir, "spritesheet_rules.mk"), true);
          foreach (KeyValuePair<string, Tuple<int, int>> pathAndDims in needSprites) {
            writer.WriteLine();
            writer.WriteLine("$(OBJEVENTGFXDIR)/" + pathAndDims.Key + ".4bpp: %.4bpp: %.png");
            writer.WriteLine(string.Format("\t$(GFX) $< $@ -mwidth {0} -mheight {1}", pathAndDims.Value.Item1, pathAndDims.Value.Item2));
          }
          writer.Close();
        }
      }
      static void moveGraphicsPics(ProjectSerializer serializer, EventObjectDatabase database) {
        // Check to see if any of the pics changed location.
        foreach (EventObjectPic pic in database.pics) {
          string fullPrettyPath = Path.Combine(serializer.project.ProjectDir, "graphics/object_events/pics/", pic.Path + ".png");
          string normalizedPath = FileUtils.normalizePath(fullPrettyPath);
          if (pic.FullPath == normalizedPath)
            continue;
          File.Copy(pic.FullPath, fullPrettyPath, true);
          pic.FullPath = normalizedPath;

          // Delete any existing bpp/pal files to force a rebuild.
          File.Delete(Path.ChangeExtension(normalizedPath, ".4bpp"));
          File.Delete(Path.ChangeExtension(normalizedPath, ".gbapal"));
        }
      }
    }
  }
}
