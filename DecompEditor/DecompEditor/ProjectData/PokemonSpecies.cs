using DecompEditor.ParserUtils;
using System.Collections.Generic;
using System.IO;
using Truncon.Collections;

namespace DecompEditor {
  /// <summary>
  /// A specific Pokemon species.
  /// </summary>
  public class PokemonSpecies {
    /// <summary>
    /// The C identifier of the species.
    /// </summary>
    public string Identifier { get; set; }
    /// <summary>
    /// The name of the species.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The full file path of the front picture.
    /// </summary>
    public string FullFrontPicPath {
      get {
        string path = Path.Combine(Project.Instance.ProjectDir, "graphics", "pokemon", Name, "front.png");
        if (!File.Exists(path))
          path = Path.Combine(Project.Instance.ProjectDir, "graphics/pokemon/question_mark/circled/front.png");
        return path;
      }
    }
  }
  public class PokemonSpeciesDatabase : DatabaseBase {
    readonly OrderedDictionary<string, PokemonSpecies> enumToSpecies = new OrderedDictionary<string, PokemonSpecies>();

    /// <summary>
    /// The name of the database.
    /// </summary>
    public override string Name => "Pokemon Species Database";

    /// <summary>
    /// Returns the species corresponding to the given ID.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public PokemonSpecies getFromId(string id) => enumToSpecies[id];

    /// <summary>
    /// Return the list of species defined within the project.
    /// </summary>
    public IEnumerable<PokemonSpecies> Species => enumToSpecies.Values;

    /// <summary>
    /// Reset the data within this database.
    /// </summary>
    protected override void reset() => enumToSpecies.Clear();

    /// <summary>
    /// Deserialize the species data from the project directory.
    /// </summary>
    /// <param name="deserializer"></param>
    protected override void deserialize(ProjectDeserializer deserializer) {
      deserializer.deserializeFile((stream) => {
        if (StructBodyDeserializer.Element.tryDeserializeBracketString(stream.ReadLine(), out string speciesEnum, out string speciesName)) {
          enumToSpecies.Add(speciesEnum, new PokemonSpecies() {
            Identifier = speciesEnum,
            Name = speciesName
          });
        }
      }, "src", "data", "text", "species_names.h");
    }
  }
}
