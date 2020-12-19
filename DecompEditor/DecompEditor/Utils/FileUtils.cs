using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace DecompEditor.Utils {
  internal static class FileUtils {
    public static IEnumerable<string> getFiles(string SourceFolder, string Filter, SearchOption searchOption) => Filter.Split('|').SelectMany(filter => Directory.EnumerateFiles(SourceFolder, filter, searchOption));
    public static void replaceInFiles(string sourceFolder, IList<KeyValuePair<string, string>> toReplace,
                                      string fileFilter) {
      if (toReplace.Count != 0)
        replaceInFiles(getFiles(sourceFolder, fileFilter, SearchOption.AllDirectories), toReplace);
    }
    public static void replaceInFiles(string file, IList<KeyValuePair<string, string>> toReplace) => replaceInFiles(Enumerable.Repeat(file, 1), toReplace);
    public static void replaceInFiles(IEnumerable<string> files,
                                      IList<KeyValuePair<string, string>> toReplace) {
      Regex[] fromRegexs = toReplace.Select(kv => new Regex(kv.Key, RegexOptions.Compiled)).ToArray();
      Parallel.ForEach(files, file => {
        string[] lines = File.ReadAllLines(file);
        bool replacedALine = false;
        for (int lineIt = 0; lineIt < lines.Length; ++lineIt) {
          for (int replIt = 0; replIt < fromRegexs.Length; ++replIt) {
            string oldLine = lines[lineIt];
            string newLine = fromRegexs[replIt].Replace(oldLine, toReplace[replIt].Value);
            if (!ReferenceEquals(newLine, oldLine)) {
              lines[lineIt] = newLine;
              replacedALine = true;
            }
          }
        }
        if (replacedALine)
          File.WriteAllLines(file, lines);
      });
    }
    public static string normalizePath(string path) {
      if (path == string.Empty)
        return path;
      return Path.GetFullPath(new Uri(path).LocalPath)
                 .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                 .ToUpperInvariant();
    }

    public static BitmapImage loadBitmapImage(string path) {
      var bitmap = new BitmapImage();
      bitmap.BeginInit();
      bitmap.UriSource = new Uri(path);
      bitmap.CacheOption = BitmapCacheOption.OnLoad;
      bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
      bitmap.EndInit();
      return bitmap;
    }
    public static Bitmap loadBitmap(string path) {
      Bitmap img;
      using (var bmpTemp = new Bitmap(path)) {
        img = new Bitmap(bmpTemp);
      }
      return img;
    }
    public static int getImageWidth(string path) {
      // TODO: We really shouldn't have to load the image here.
      return loadBitmapImage(path).PixelWidth;
    }
    public static bool uploadImage(out string path, string initialDir) {
      var openFileDialog = new OpenFileDialog {
        InitialDirectory = initialDir,
        Filter = "Sprite Image (*.png)|*.png",
        RestoreDirectory = true
      };
      bool result = openFileDialog.ShowDialog() == true;
      path = result ? openFileDialog.FileName : string.Empty;
      return result;
    }

    public static string readResource(string name) {
      Assembly assembly = Assembly.GetExecutingAssembly();
      string resourcePath = assembly.GetManifestResourceNames()
            .Single(str => str.EndsWith(name));

      using (Stream stream = assembly.GetManifestResourceStream(resourcePath)) {
        using (StreamReader reader = new StreamReader(stream)) {
          return reader.ReadToEnd();
        }
      }
    }
  }
}
