using DecompEditor.ParserUtils;
using DecompEditor.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Truncon.Collections;

namespace DecompEditor.ProjectData.OldFormat {
  class Trainers {
    public class Converter {
      class Deserializer {
        readonly OrderedDictionary<string, TrainerPic> idToPic = new OrderedDictionary<string, TrainerPic>();
        readonly Dictionary<string, TrainerClass> idToTrainerClass = new Dictionary<string, TrainerClass>();

        class TrainerPartyDeserializer : ArrayDeserializer {
          class PokemonDeserializer : StructDeserializer<Pokemon> {
            public PokemonDeserializer(Action<Pokemon> handler) : base(handler) {
              addInteger("iv", (iv) => current.Iv = iv);
              addInteger("lvl", (lvl) => current.Level = lvl);
              addEnum("species", (species) => current.Species = Project.Instance.Species.getFromId(species));
              addEnum("heldItem", (heldItem) => current.HeldItem = Project.Instance.Items.getFromId(heldItem));
              addEnumList("moves", (moves) => current.Moves = new ObservableCollection<Move>(moves.Select(move => Project.Instance.Moves.getFromId(move))));
            }
          }

          public TrainerPartyDeserializer(Dictionary<string, TrainerParty> cppToParty) {
            TrainerParty current = null;
            var pkmDeserializer = new PokemonDeserializer((pkm) => {
              if (pkm.HeldItem == null)
                pkm.HeldItem = Project.Instance.Items.getFromId("NONE");
              if (pkm.Moves == null)
                pkm.Moves = new ObservableCollection<Move>(Enumerable.Repeat(Project.Instance.Moves.getFromId("MOVE_NONE"), 4));
              current.Pokemon.Add(pkm);
            });
            initialize(pkmDeserializer, "sParty_", (name) => {
              current = new TrainerParty();
              cppToParty.Add(name, current);
            });
          }
        }
        class TrainerDeserializer : StructDeserializer<Trainer> {
          public TrainerDeserializer(Dictionary<string, TrainerClass> idToTrainerClass, OrderedDictionary<string, TrainerPic> idToPic,
                                     Dictionary<string, TrainerParty> cppToParty, Action<string, Trainer> handler) : base(handler) {
            addEnum("trainerClass", (tclass) => current.Class = idToTrainerClass[tclass.Remove(0, "TRAINER_CLASS_".Length)]);
            addEnumMask("encounterMusic_gender", (flags) => {
              current.IsMale = flags.Length == 1;
              current.EncounterMusic = Project.Instance.TrainerEncounterMusic.getFromId(flags[^1]);
            });
            addEnum("trainerPic", (pic) => current.Pic = idToPic[pic.Remove(0, "TRAINER_PIC_".Length).Replace("RS_", "RUBY_SAPPHIRE_")]);
            addString("trainerName", (name) => current.Name = name);
            addEnumList("items", (items) => {
              if (items.Length == 0)
                current.Items = new ObservableCollection<Item>(Enumerable.Repeat(Project.Instance.Items.getFromId("NONE"), 4));
              else
                current.Items = new ObservableCollection<Item>(items.Select(item => Project.Instance.Items.getFromId(item)));
            });
            addEnum("doubleBattle", (doubleBattle) => current.DoubleBattle = doubleBattle[0] != 'F');
            addEnumMask("aiFlags", (flags) => {
              if (flags[0][0] != '0') {
                foreach (string flag in flags)
                  current.AIFlags.Add(Project.Instance.BattleAI.getFromId(flag));
              }
            });
            addEnum("party", (partyStruct) => {
              int partyNameIndex = partyStruct.LastIndexOf(' ') + 1;
              string partyName = partyStruct.Substring(partyNameIndex, partyStruct.Length - partyNameIndex - 1);
              if (cppToParty.TryGetValue(partyName, out TrainerParty party))
                current.Party = party;
            });
          }
        }
        public void deserialize(ProjectDeserializer deserializer, TrainerDatabase database) {
          var cppToParty = new Dictionary<string, TrainerParty>();
          loadClassNames(deserializer, database);
          loadMoneyFactors(deserializer);
          loadPicAndPalettePath(deserializer, database);
          loadPicTables(deserializer, database);
          loadTrainerParties(deserializer, cppToParty);
          loadTrainers(deserializer, database, cppToParty);
        }
        void loadTrainerParties(ProjectDeserializer deserializer, Dictionary<string, TrainerParty> cppToParty) {
          var partyDeserializer = new TrainerPartyDeserializer(cppToParty);
          deserializer.deserializeFile(partyDeserializer, "src", "data", "trainer_parties.h");
        }
        void loadTrainers(ProjectDeserializer deserializer, TrainerDatabase database,
                          Dictionary<string, TrainerParty> cppToParty) {
          var trainerDeserializer = new TrainerDeserializer(idToTrainerClass, idToPic, cppToParty, (name, trainer) => {
            trainer.Identifier = name.Remove(0, "TRAINER_".Length);
            if (trainer.Identifier != "NONE")
              database.Trainers.Add(trainer);
          });
          var trainerListDeserialzier = new ArrayDeserializer(trainerDeserializer, "gTrainers");
          deserializer.deserializeFile(trainerListDeserialzier, "src", "data", "trainers.h");
        }
        void loadClassNames(ProjectDeserializer deserializer, TrainerDatabase database) {
          deserializer.deserializeFile((reader) => {
            if (!StructBodyDeserializer.Element.tryDeserializeBracketString(reader.ReadLine(), out string classEnum, out string className))
              return;
            TrainerClass trainerClass = new TrainerClass() {
              Identifier = classEnum.Remove(0, "TRAINER_CLASS_".Length),
              Name = className
            };
            database.addClass(trainerClass);
            idToTrainerClass.Add(trainerClass.Identifier, trainerClass);
          }, "src", "data", "text", "trainer_class_names.h");
        }
        void loadMoneyFactors(ProjectDeserializer deserializer) {
          var moneyFactorDeserializer = new InlineStructDeserializer((elements) => {
            if (elements[0] == "0xFF")
              return;
            string id = elements[0].Remove(0, "TRAINER_CLASS_".Length);
            if (idToTrainerClass.TryGetValue(id, out TrainerClass tClass))
              tClass.MoneyFactor = int.Parse(elements[1]);
          });
          var arrayDeserializer = new ArrayDeserializer(moneyFactorDeserializer, "gTrainerMoneyTable");
          deserializer.deserializeFile(arrayDeserializer, "src", "battle_main.c");
        }
        void loadPicAndPalettePath(ProjectDeserializer deserializer, TrainerDatabase database) {
          var fileDeserializer = new FileDeserializer();
          fileDeserializer.add(new IncBinDeserializer("gTrainerFrontPic_", "u32", (cppVar, fileName) => {
            cppVar = cppVar.fromPascalToSnake();
            if (!idToPic.TryGetValue(cppVar, out TrainerPic trainerPic)) {
              database.addFrontPic(trainerPic = new TrainerPic { Identifier = cppVar });
              idToPic.Add(cppVar, trainerPic);
            }
            trainerPic.Path = fileName.Remove(0, "graphics/trainers/front_pics/".Length);
            trainerPic.FullPath = Path.Combine(deserializer.project.ProjectDir, fileName + ".png");
          }));
          fileDeserializer.add(new IncBinDeserializer("gTrainerPalette_", "u32", (cppVar, fileName) => {
            cppVar = cppVar.fromPascalToSnake();
            if (!idToPic.TryGetValue(cppVar, out TrainerPic trainerPic)) {
              database.addFrontPic(trainerPic = new TrainerPic { Identifier = cppVar });
              idToPic.Add(cppVar, trainerPic);
            }
          }));
          deserializer.deserializeFile(fileDeserializer, "src", "data", "graphics", "trainers.h");
        }
        void loadPicTables(ProjectDeserializer deserializer, TrainerDatabase database) {
          deserializer.deserializeFile((stream) => {
            string line = stream.ReadLine();
            if (line.tryExtractPrefix("    TRAINER_SPRITE(", ",", out string classEnum)) {
              if (!idToPic.TryGetValue(classEnum, out TrainerPic existingPic))
                return;
              int sizeIndex = line.LastIndexOf(' ') + 1;
              existingPic.UncompressedSize = Convert.ToInt32(line.Substring(sizeIndex, line.Length - sizeIndex - 2), 16);
              return;
            }
            if (!line.tryExtractPrefix("    [TRAINER_PIC_", "]", out classEnum) ||
                !idToPic.TryGetValue(classEnum, out TrainerPic trainerPic)) {
              return;
            }

            int structStartIndex = line.LastIndexOf('{') + ".size = ".Length + 1;
            int structSplitIndex = line.IndexOf(',', structStartIndex);
            trainerPic.CoordSize = int.Parse(line.Substring(structStartIndex, structSplitIndex - structStartIndex));
            structSplitIndex += ", .y_offset = ".Length;
            trainerPic.CoordYOffset = int.Parse(line.Substring(structSplitIndex, line.Length - structSplitIndex - 2));
          }, "src", "data", "trainer_graphics", "front_pic_tables.h");
        }
      }

      public void convert(ProjectDeserializer deserializer, TrainerDatabase database) {
        // Load in the legacy format.
        Deserializer trainerDeserializer = new Deserializer();
        trainerDeserializer.deserialize(deserializer, database);

        // Populate the project with the new format.
        populateWithNewFormat();
      }

      /// <summary>
      /// Overwrite the existing game mechanisms and use the new format for trainers instead.
      /// </summary>
      void populateWithNewFormat() {
        string projectDir = Project.Instance.ProjectDir;

        // Remove direct use of Pic and Class IDs.
        string trainersConstantsPath = Path.Combine(projectDir, "include", "constants", "trainers.h");
        List<string> curLines = File.ReadAllLines(trainersConstantsPath).ToList();
        curLines.RemoveAll(line => line.StartsWith("#define TRAINER_CLASS_") || line.StartsWith("#define TRAINER_PIC_"));
        int injaIndex = curLines.FindLastIndex(line => line.StartsWith("#include ")) + 1;

        // Generate the file for constants/trainers.h
        using (StreamWriter stream = new StreamWriter(trainersConstantsPath + ".json.txt")) {
          stream.NewLine = "\n";
          stream.WriteLine("{{ doNotModifyHeader }}");
          foreach (string str in curLines.Take(injaIndex))
            stream.WriteLine(str);
          stream.Write(FileUtils.readResource("trainer_constants.json.txt"));
          foreach (string str in curLines.Skip(injaIndex))
            stream.WriteLine(str);
        }

        // Generate the file for trainer_class_names.h
        string trainerClassNamesPath = Path.Combine(projectDir, "src", "data", "text", "trainer_class_names.h");
        using (StreamWriter stream = new StreamWriter(trainerClassNamesPath + ".json.txt")) {
          stream.NewLine = "\n";
          stream.Write(FileUtils.readResource("trainer_class_names.json.txt"));
        }

        // Remove the direct use of Trainer Money Table.
        string battleMainPath = Path.Combine(projectDir, "src", "battle_main.c");
        curLines = File.ReadAllLines(battleMainPath).ToList();
        int moneyTableIndex = curLines.FindIndex(line => line.EndsWith("gTrainerMoneyTable[] ="));
        int moneyTableEndIndex = curLines.FindIndex(moneyTableIndex, line => line.StartsWith("};"));
        curLines.RemoveRange(moneyTableIndex + 1, moneyTableEndIndex - moneyTableIndex);
        curLines[moneyTableIndex] = "#include \"data/trainer_class_money_table.h\"";
        File.WriteAllLines(battleMainPath, curLines);

        // Generate the file for trainer_class_money_table.h
        using (StreamWriter stream = new StreamWriter(Path.Combine(projectDir, "src", "data", "trainer_class_money_table.h.json.txt"))) {
          stream.NewLine = "\n";
          stream.Write(FileUtils.readResource("trainer_class_money_table.json.txt"));
        }

        // Generate the file for graphics/trainers.h
        string trainerGraphicsPath = Path.Combine(projectDir, "src", "data", "graphics", "trainers.h");
        curLines = File.ReadAllLines(trainerGraphicsPath).ToList();
        using (StreamWriter stream = new StreamWriter(trainerGraphicsPath + ".json.txt")) {
          stream.NewLine = "\n";
          stream.Write(FileUtils.readResource("trainer_graphics.json.txt"));

          // Copy all of the current lines not related to front pics.
          // TODO: This wouldn't be necessary if we also handled back sprites.
          foreach (string line in curLines) {
            if (line.Length != 0 && !line.StartsWith("const u32 gTrainerFrontPic_") &&
                !line.StartsWith("const u32 gTrainerPalette_")) {
              stream.WriteLine(line);
            }
          }
        }

        // Remove the direct use of graphic externs.
        string graphicsPath = Path.Combine(projectDir, "include", "graphics.h");
        curLines = File.ReadAllLines(graphicsPath).ToList();
        curLines.RemoveAll(line => line.StartsWith("extern const u32 gTrainerFrontPic_") ||
                                   line.StartsWith("extern const u32 gTrainerPalette_"));
        curLines.Insert(3, "#include \"trainer_graphics.h\"");
        File.WriteAllLines(graphicsPath, curLines);

        // Generate the file for trainer_graphics.h
        using (StreamWriter stream = new StreamWriter(Path.Combine(projectDir, "include", "trainer_graphics.h.json.txt"))) {
          stream.NewLine = "\n";
          stream.Write(FileUtils.readResource("trainer_graphics_pointers.json.txt"));
        }

        // Generate the file for front_pic_tables.h
        using (StreamWriter stream = new StreamWriter(Path.Combine(projectDir, "src", "data", "trainer_graphics", "front_pic_tables.h.json.txt"))) {
          stream.NewLine = "\n";
          stream.Write(FileUtils.readResource("trainer_front_pic_tables.json.txt"));
        }

        // Generate the file for front_pic_anims.h
        using (StreamWriter stream = new StreamWriter(Path.Combine(projectDir, "src", "data", "trainer_graphics", "front_pic_anims.h.json.txt"))) {
          stream.NewLine = "\n";
          stream.Write(FileUtils.readResource("trainer_front_pic_anims.json.txt"));
        }

        // Generate the file for trainers.h
        using (StreamWriter stream = new StreamWriter(Path.Combine(projectDir, "src", "data", "trainers.h.json.txt"))) {
          stream.NewLine = "\n";
          stream.Write(FileUtils.readResource("trainers.json.txt"));
        }

        // Generate the file for front_pic_anims.h
        using (StreamWriter stream = new StreamWriter(Path.Combine(projectDir, "src", "data", "trainer_parties.h.json.txt"))) {
          stream.NewLine = "\n";
          stream.Write(FileUtils.readResource("trainer_parties.json.txt"));
        }

        // Update the json_data_rules makefile.
        string jsonDataRulesPath = Path.Combine(projectDir, "json_data_rules.mk");
        using (StreamWriter stream = new StreamWriter(jsonDataRulesPath, /*append=*/true)) {
          stream.NewLine = "\n";
          stream.Write(FileUtils.readResource("trainer_json_data_rules.txt"));
        }

        // If the user hasn't removed the original trainer pic IDs, a few will need fixups.
        Project.Instance.registerFileReplacement("TRAINER_PIC_RS_BRENDAN", "TRAINER_PIC_RUBY_SAPPHIRE_BRENDAN");
        Project.Instance.registerFileReplacement("TRAINER_PIC_RS_MAY", "TRAINER_PIC_RUBY_SAPPHIRE_MAY");
      }
    }
  }
}
