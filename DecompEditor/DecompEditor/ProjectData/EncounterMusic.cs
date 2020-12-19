using DecompEditor.ParserUtils;
using System.Collections.Generic;
using Truncon.Collections;

namespace DecompEditor {
  class TrainerEncounterMusicDatabase : DatabaseBase {
    /// <summary>
    /// The parsed set of encounter music types.
    /// </summary>
    OrderedDictionary<string, CDefine> encounterMusic = new OrderedDictionary<string, CDefine>();

    /// <summary>
    /// The name of the database.
    /// </summary>
    public override string Name => "Encounter Music Database";

    /// <summary>
    /// Returns all of the encounter music types within the project.
    /// </summary>
    public IEnumerable<CDefine> EncounterMusic => encounterMusic.Values;

    /// <summary>
    /// Get an encounter music definition from its ID.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public CDefine getFromId(string id) => encounterMusic[id];

    /// <summary>
    /// Reset the data within this database.
    /// </summary>
    protected override void reset() => encounterMusic.Clear();

    /// <summary>
    /// Deserialize the encounter music from within the project directory.
    /// </summary>
    /// <param name="serializer"></param>
    protected override void deserialize(ProjectDeserializer serializer) {
      encounterMusic = serializer.parseDefineNames(
        "TRAINER_ENCOUNTER_MUSIC_", "include", "constants", "trainers.h");
    }
  }
}
