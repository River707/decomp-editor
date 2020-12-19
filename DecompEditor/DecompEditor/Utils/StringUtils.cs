using NaturalSort.Extension;
using System;
using System.Globalization;
using System.Linq;

namespace DecompEditor.Utils {
  static class StringUtils {
    /// Convert the given string to a pascal sentence from snake case.
    public static string fromSnakeToPascalSentence(this string str) {
      TextInfo info = CultureInfo.CurrentCulture.TextInfo;
      string result = info.ToTitleCase(str.ToLower().Replace("_", " "));
      return result.Replace("Cooltrainer", "Cool Trainer");
    }
    public static string fromSnakeToPascal(this string str) => str.fromSnakeToPascalSentence().fromSentenceToPascal();

    /// Try to extract a substring from the given string if the string starts
    /// with the provided filter.
    public static bool tryExtractPrefix(this string str, string filter, string delimiter, out string result) {
      result = string.Empty;
      if (!str.StartsWith(filter))
        return false;
      int endIndex = str.IndexOf(delimiter, filter.Length + 1);
      if (endIndex == -1)
        return false;
      result = str[filter.Length..endIndex];
      return true;
    }

    public static string fromPascalToSentence(this string str) => string.Concat(str.Select((x, i) => i > 0 && char.IsUpper(x) ? " " + x.ToString() : x.ToString()));
    public static string fromPascalToSnake(this string str) {
      string result = string.Concat(str.Select((x, i) => i > 0 && (char.IsUpper(x) || (!char.IsLower(x) && char.IsLetter(str[i - 1]))) ? "_" + x.ToString() : x.ToString())).ToUpper();
      return result.Replace("COOL_TRAINER", "COOLTRAINER");
    }
    public static string fromSentenceToPascal(this string str) => str.Replace(" ", string.Empty);

    public static int CompareToNatural(this string lhs, string rhs) => StringComparer.CurrentCulture.WithNaturalSort().Compare(lhs, rhs);
  }
}
