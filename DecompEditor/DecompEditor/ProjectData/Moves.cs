using DecompEditor.ParserUtils;
using System.Collections.Generic;
using Truncon.Collections;

namespace DecompEditor {
  /// <summary>
  /// A specific Pokemon move defined within the project.
  /// </summary>
  public class Move {
    /// <summary>
    /// The C identifier of the move.
    /// </summary>
    public string Identifier { get; set; }
    /// <summary>
    /// The name of the move.
    /// </summary>
    public string Name { get; set; }
  }
  public class MoveDatabase : DatabaseBase {
    readonly OrderedDictionary<string, Move> idToMove = new OrderedDictionary<string, Move>();

    /// <summary>
    /// The name of this database.
    /// </summary>
    public override string Name => "Move Database";

    /// <summary>
    /// Returns the moves defined within the project.
    /// </summary>
    public IEnumerable<Move> Moves => idToMove.Values;
    /// <summary>
    /// Returns a move given its corresponding ID.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Move getFromId(string id) => idToMove[id];

    /// <summary>
    /// Reset the data held by this database.
    /// </summary>
    protected override void reset() => idToMove.Clear();

    protected override void deserialize(ProjectDeserializer deserializer) {
      deserializer.deserializeFile((reader) => {
        if (!StructBodyDeserializer.Element.tryDeserializeBracketString(reader.ReadLine(), out string moveEnum, out string moveName))
          return;
        idToMove.Add(moveEnum, new Move() {
          Identifier = moveEnum,
          Name = moveName
        });
      }, "src", "data", "text", "move_names.h");
    }
  }
}
