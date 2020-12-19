using DecompEditor.ParserUtils;
using System.Collections.Generic;
using Truncon.Collections;

namespace DecompEditor {
  public class BattleAIDatabase : DatabaseBase {
    /// <summary>
    /// The AI scripts present within the project.
    /// </summary>
    OrderedDictionary<string, CDefine> aiScripts = new OrderedDictionary<string, CDefine>();

    /// <summary>
    /// The name of the database.
    /// </summary>
    public override string Name => "Battle AI Database";

    /// <summary>
    /// Returns the define of a battle script given the ID of the script.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public CDefine getFromId(string name) => aiScripts[name];

    /// <summary>
    /// Returns all of the available AI scripts.
    /// </summary>
    public IEnumerable<CDefine> AIScripts => aiScripts.Values;

    /// <summary>
    /// Resets the data within this database.
    /// </summary>
    protected override void reset() => aiScripts.Clear();

    /// <summary>
    /// Deserialize the battle AI scripts from the project directory.
    /// </summary>
    /// <param name="serializer"></param>
    protected override void deserialize(ProjectDeserializer serializer) => aiScripts = serializer.parseDefineNames("AI_SCRIPT_", "include", "constants", "battle_ai.h");
  }
}
