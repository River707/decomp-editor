using DecompEditor.ParserUtils;
using DecompEditor.Utils;
using System;
using System.IO;
using System.Linq;
using Truncon.Collections;

namespace DecompEditor {
  public class ProjectDeserializer {
    public Project project { get; private set; }

    public ProjectDeserializer(Project project) => this.project = project;

    /// <summary>
    /// Open a file for read at the given path.
    /// </summary>
    StreamReader openFile(params string[] paths) {
      string filePath = Path.Combine(paths.Prepend(project.ProjectDir).ToArray());
      Project.Logger.Info("Deserializing data within {File}", FileUtils.normalizePath(filePath));
      return File.OpenText(filePath);
    }
    public void deserializeFile(FileDeserializer deserializer, params string[] paths) {
      StreamReader stream = openFile(paths);
      deserializer.deserialize(stream);
      stream.Close();
    }
    public void deserializeFile(DeserializerBase deserializer, params string[] paths) {
      var fileDeserializer = new FileDeserializer();
      fileDeserializer.add(deserializer);
      deserializeFile(fileDeserializer, paths);
    }
    public void deserializeFile(Func<StreamReader, bool> deserializer, params string[] paths) {
      StreamReader stream = openFile(paths);
      while (!stream.EndOfStream) {
        if (deserializer(stream))
          break;
      }
      stream.Close();
    }
    public void deserializeFile(Action<StreamReader> deserializer, params string[] paths) {
      deserializeFile((reader) => {
        deserializer(reader);
        return false;
      }, paths);
    }

    /// <summary>
    /// Parse a set of C-Define names that begin with the given prefix. Returns
    /// a list of parsed C defines.
    /// </summary>
    public OrderedDictionary<string, CDefine> parseDefineNames(string prefix, params string[] paths) {
      var defines = new OrderedDictionary<string, CDefine>();
      StreamReader reader = openFile(paths);
      while (!reader.EndOfStream) {
        if (reader.ReadLine().tryExtractPrefix("#define " + prefix, " ", out string defineName)) {
          defines.Add(prefix + defineName, new CDefine() {
            Identifier = defineName,
            Order = defines.Count
          });
        }
      }
      reader.Close();
      return defines;
    }
  }
  public class ProjectSerializer {
    public Project project { get; private set; }

    public ProjectSerializer(Project project) => this.project = project;

    public StreamWriter openFile(params string[] paths) => new StreamWriter(Path.Combine(paths.Prepend(project.ProjectDir).ToArray()));
    public void serializeFile(Action<StreamWriter> serializeFn, params string[] paths) {
      StreamWriter stream = openFile(paths);
      serializeFn(stream);
      stream.Close();
    }
    public void serializePartialFile(Func<string, bool> sectionCheck, Action<StreamWriter> action, params string[] paths)
      => serializePartialFile(sectionCheck, (str) => !sectionCheck(str), action, paths);
    public void serializePartialFile(Func<string, bool>[] sectionChecks, Action<StreamWriter>[] actions, params string[] paths)
      => serializePartialFile(sectionChecks, sectionChecks.Select(sectionCheck => new Func<string, bool>((str) => !sectionCheck(str))).ToArray(),
                              actions, paths);
    public void serializePartialFile(Func<string, bool> sectionBeginCheck, Func<string, bool> sectionEndCheck,
                                     Action<StreamWriter> action, params string[] paths)
      => serializePartialFile(new[] { sectionBeginCheck }, new[] { sectionEndCheck }, new[] { action }, paths);
    public void serializePartialFile(Func<string, bool>[] sectionBeginChecks, Func<string, bool>[] sectionEndChecks, Action<StreamWriter>[] actions, params string[] paths) {
      string[] curLines = File.ReadAllLines(Path.Combine(paths.Prepend(project.ProjectDir).ToArray()));
      StreamWriter stream = openFile(paths);
      stream.NewLine = "\n";

      // Copy the existing lines as is.
      int curLine = 0;
      for (int i = 0, e = sectionBeginChecks.Length; i != e; ++i) {
        while (!sectionBeginChecks[i](curLines[curLine]))
          stream.WriteLine(curLines[curLine++]);
        while (curLine != curLines.Length && !sectionEndChecks[i](curLines[curLine++]))
          continue;
        actions[i](stream);
      }

      while (curLine != curLines.Length)
        stream.WriteLine(curLines[curLine++]);
      stream.Close();
    }
  }
}
