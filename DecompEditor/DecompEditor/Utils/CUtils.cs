using DecompEditor.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace DecompEditor.ParserUtils {
  public class CDefine {
    /// <summary>
    /// The raw C identifier of the define.
    /// </summary>
    public string Identifier { get; set; }
    /// <summary>
    /// A name suitable for use as a C variable name.
    /// </summary>
    public string VariableName => Identifier.fromSnakeToPascal();
    public int Order { get; set; }

    public override string ToString() => Identifier.fromSnakeToPascalSentence();

    public static bool operator <(CDefine a, CDefine b) => a.Order < b.Order;
    public static bool operator >(CDefine a, CDefine b) => a.Order > b.Order;
  }

  public abstract class DeserializerBase {
    public abstract bool tryDeserialize(string currentLine, StreamReader reader);
  }
  public abstract class StructBodyDeserializer : DeserializerBase {
    public enum NameKind {
      EnumBracket,
      StructElement,
    }
    public enum ValueKind {
      EnumList, // Identifier (, Identifer)*
      EnumMask, // Identifier (| Identifer)*
      Enum,     // Identifier
      Integer,  // 0-9+
      String,   // ""
    }

    public delegate void ElementValueHandler(object valueObj);
    public delegate void EnumListHandler(string[] values);
    public delegate void EnumMaskHandler(string[] values);
    public delegate void EnumHandler(string value);
    public delegate void IntegerHandler(int value);
    public delegate void StringHandler(string value);

    public class Element {
      public static bool tryDeserializeBracketString(string line, out string name, out string value) {
        value = string.Empty;
        if (!tryDeserializeName(ref line, NameKind.EnumBracket, out name))
          return false;
        string result = string.Empty;
        deserializeValue(line, ValueKind.String, (valueObj) => result = valueObj as string);
        value = result;
        return true;
      }

      public static bool tryDeserializeName(ref string line, NameKind nameKind, out string name) {
        name = string.Empty;
        switch (nameKind) {
          case NameKind.EnumBracket: {
            if (!line.StartsWith("    ["))
              return false;
            name = line.Substring(5, line.IndexOf(']') - 5);
            break;
          }
          case NameKind.StructElement: {
            line = line.TrimStart();
            if (!line.StartsWith("."))
              return false;
            name = line.Substring(1, line.IndexOf(' ') - 1);
            break;
          }
        }
        line = line.Substring(line.IndexOf('=', name.Length + 2) + 2);
        if (line[^1] == ',')
          line = line.Remove(line.Length - 1);
        return true;
      }

      public static void deserializeValue(string line, ValueKind valueKind, ElementValueHandler handler) {
        switch (valueKind) {
          case ValueKind.EnumList: {
            if (line[0] == '{') {
              line = line.Remove(0, 1);
              line = line.Remove(line.Length - 1);
            }

            handler(line.Split(", ", StringSplitOptions.RemoveEmptyEntries));
            break;
          }
          case ValueKind.EnumMask: {
            handler(line.Split(" | "));
            break;
          }
          case ValueKind.Enum: {
            handler(line);
            break;
          }
          case ValueKind.Integer: {
            handler(int.Parse(line));
            break;
          }
          case ValueKind.String: {
            int valueIndexStart = line.IndexOf('\"') + 1;
            int valueIndexEnd = line.LastIndexOf('\"');
            handler(line.Substring(valueIndexStart, valueIndexEnd - valueIndexStart));
            break;
          }
        }
      }
    }

    class Value {
      public ValueKind kind;
      public ElementValueHandler deserializeHandler;
    };

    readonly List<Value> values = new List<Value>();
    readonly Dictionary<string, int> nameToElementIndex = new Dictionary<string, int>();

    protected void deserializeBody(string line, StreamReader stream) {
      while (Element.tryDeserializeName(ref line, NameKind.StructElement, out string name)) {
        if (nameToElementIndex.TryGetValue(name, out int elementIndex)) {
          Value element = values[elementIndex];
          if (element != null)
            Element.deserializeValue(line, element.kind, element.deserializeHandler);
        }
        line = stream.ReadLine().Trim();
      }
    }
    protected void deserializeInlineBody(string line, StreamReader stream) {
      line = line.Substring(0, line.LastIndexOf('}') - 0);
      string[] elements = line.Split(',', StringSplitOptions.RemoveEmptyEntries);
      if (elements.Length != values.Count)
        throw new Exception("Invalid inline struct element count");
      for (int i = 0; i != elements.Length; ++i) {
        if (values[i] != null)
          Element.deserializeValue(line, values[i].kind, values[i].deserializeHandler);
      }
    }

    public void addIgnored(string name) {
      nameToElementIndex.Add(name, values.Count);
      values.Add(null);
    }
    public void addEnumList(string name, EnumListHandler handler) => addValue(name, ValueKind.EnumList, (obj) => handler(obj as string[]));
    public void addEnumMask(string name, EnumMaskHandler handler) => addValue(name, ValueKind.EnumMask, (obj) => handler(obj as string[]));
    public void addEnum(string name, EnumHandler handler) => addValue(name, ValueKind.Enum, (obj) => handler(obj as string));
    public void addInteger(string name, IntegerHandler handler) => addValue(name, ValueKind.Integer, (obj) => handler((int)obj));
    public void addString(string name, StringHandler handler) => addValue(name, ValueKind.String, (obj) => handler(obj as string));
    public void addValue(string name, ValueKind kind, ElementValueHandler handler) {
      nameToElementIndex.Add(name, values.Count);
      values.Add(new Value() {
        kind = kind,
        deserializeHandler = handler
      });
    }
  }
  public class StructDeserializer<T> : StructBodyDeserializer where T : new() {
    /// <summary>
    /// The current item being deserialized.
    /// </summary>
    public T current;

    /// <summary>
    /// The handler invoked when a new item is deserialized.
    /// </summary>
    readonly Action<string, T> handler;

    protected StructDeserializer(Action<string, T> handler) => this.handler = handler;
    protected StructDeserializer(Action<T> handler) => this.handler = (_, item) => handler(item);

    public override bool tryDeserialize(string currentLine, StreamReader reader) {
      if (currentLine.tryExtractPrefix("[", "]", out string newItemName))
        currentLine = reader.ReadLine().Trim();
      else if (!currentLine.StartsWith("{"))
        return false;

      // Deserialize the body.
      current = new T();
      if (currentLine.EndsWith(';')) {
        deserializeInlineBody(currentLine, reader);
      } else {
        currentLine = reader.ReadLine().Trim();
        deserializeBody(currentLine, reader);
      }

      handler(newItemName, current);
      return true;
    }
  }
  public class CustomDeserializer : DeserializerBase {
    readonly Func<string, StreamReader, bool> tryDeserializeFunc;

    public CustomDeserializer(Func<string, StreamReader, bool> tryDeserializeFunc)
      => this.tryDeserializeFunc = tryDeserializeFunc;

    public override bool tryDeserialize(string currentLine, StreamReader reader)
      => tryDeserializeFunc(currentLine, reader);
  }
  public class InlineStructDeserializer : CustomDeserializer {
    public InlineStructDeserializer(Action<string[]> handler)
      : base((currentLine, stream) => {
        if (!currentLine.StartsWith('{'))
          return false;
        currentLine = currentLine.Substring(1, currentLine.LastIndexOf('}') - 1);
        handler(currentLine.Split(',').Select(val => val.Trim()).ToArray());
        return true;
      }) { }
  }

  public class ArrayDeserializer : DeserializerBase {
    bool deserialized = false;
    DeserializerBase elementDeserializer;
    protected Action<string> handler;
    Regex nameRegex;

    bool IsSingleInstance => handler == null;

    public ArrayDeserializer() { }
    public ArrayDeserializer(DeserializerBase elementDeserializer,
                             string namePrefix, Action<string> handler)
      => initialize(elementDeserializer, namePrefix, handler);
    public ArrayDeserializer(DeserializerBase elementDeserializer, string namePrefix)
      => initialize(elementDeserializer, namePrefix);
    protected void initialize(DeserializerBase elementDeserializer,
                              string namePrefix, Action<string> handler) {
      this.elementDeserializer = elementDeserializer;
      this.handler = handler;
      nameRegex = new Regex("(" + namePrefix + @"[a-zA-Z0-9_]+)\[\] =( {)?$", RegexOptions.Compiled);
    }
    protected void initialize(DeserializerBase elementDeserializer, string namePrefix) {
      this.elementDeserializer = elementDeserializer;
      nameRegex = new Regex(namePrefix + @"\[\] =( {)?$", RegexOptions.Compiled);
    }

    public override bool tryDeserialize(string currentLine, StreamReader reader) {
      if (deserialized)
        return false;
      Match match = nameRegex.Match(currentLine);
      if (!match.Success)
        return false;
      handler?.Invoke(match.Groups[1].Value);

      // Skip {
      if (!currentLine.EndsWith("{"))
        reader.ReadLine();
      do {
        currentLine = reader.ReadLine().Trim();
        if (currentLine.StartsWith("}"))
          break;
        elementDeserializer.tryDeserialize(currentLine, reader);
      } while (true);
      deserialized = IsSingleInstance;
      return true;
    }
  }

  public class IncBinDeserializer : DeserializerBase {
    /// <summary>
    /// The handler invoked when an item is deserialized.
    /// </summary>
    readonly Action<string, string> handler;

    /// <summary>
    /// The prefix of the INCBIN line being parsed.
    /// </summary>
    readonly string linePrefix;

    public IncBinDeserializer(string varPrefix, string size, Action<string, string> handler) {
      this.handler = handler;
      linePrefix = string.Format("const {0} {1}", size, varPrefix);
    }

    public override bool tryDeserialize(string currentLine, StreamReader stream) {
      if (!currentLine.tryExtractPrefix(linePrefix, "[", out string varName))
        return false;
      int skipLen = currentLine.IndexOf('\"') + 1;
      if (skipLen == 0)
        return false;
      string fileName = currentLine[skipLen..currentLine.IndexOf('.', skipLen)];
      handler(varName, fileName);
      return true;
    }
  }
  public class FileDeserializer {
    readonly List<DeserializerBase> deserializers = new List<DeserializerBase>();

    public void add(DeserializerBase deserializer) => deserializers.Add(deserializer);
    public void add(Func<string, StreamReader, bool> tryDeserializeFunc) => add(new CustomDeserializer(tryDeserializeFunc));

    public void deserialize(StreamReader reader) {
      while (!reader.EndOfStream) {
        string currentLine = reader.ReadLine().Trim();
        if (currentLine.Length == 0)
          continue;

        foreach (DeserializerBase deserializer in deserializers) {
          if (deserializer.tryDeserialize(currentLine, reader))
            break;
        }
      }
    }
  }
}
