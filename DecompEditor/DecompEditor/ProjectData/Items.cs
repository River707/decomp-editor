using DecompEditor.ParserUtils;
using DecompEditor.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Truncon.Collections;

namespace DecompEditor {
  /// <summary>
  /// A specific item within the project.
  /// </summary>
  public class Item : ObservableObject {
    private string name;
    private string identifier;
    private int price;
    private object holdEffect;
    private object holdEffectParam;
    private string description;
    private int importance;
    private int unk19;
    private string pocket;
    private string type;
    private string fieldUseFunction;
    private int battleUsage;
    private string battleUseFunction;
    private object secondaryId;
    private string pic;

    /// <summary>
    /// The name of the item.
    /// </summary>
    public string Name { get => name; set => Set(ref name, value); }
    /// <summary>
    /// The C identifier of the item.
    /// </summary>
    public string Identifier { get => identifier; set => Set(ref identifier, value); }
    /// <summary>
    /// The price of the item.
    /// </summary>
    public int Price { get => price; set => Set(ref price, value); }
    /// <summary>
    /// The hold effect of the item.
    /// </summary>
    public object HoldEffect { get => holdEffect; set => Set(ref holdEffect, value); }
    /// <summary>
    /// The hold effect parameter of the item.
    /// </summary>
    public object HoldEffectParam { get => holdEffectParam; set => Set(ref holdEffectParam, value); }
    /// <summary>
    /// The description of the item.
    /// </summary>
    public string Description { get => description; set => Set(ref description, value); }
    /// <summary>
    /// The importance of the item.
    /// </summary>
    public int Importance { get => importance; set => Set(ref importance, value); }
    /// <summary>
    /// The unk19 of the item.
    /// </summary>
    public int Unk19 { get => unk19; set => Set(ref unk19, value); }
    /// <summary>
    /// The pocket of the item.
    /// </summary>
    public string Pocket { get => pocket; set => Set(ref pocket, value); }
    /// <summary>
    /// The type of the item.
    /// </summary>
    public string Type { get => type; set => Set(ref type, value); }
    /// <summary>
    /// The field use function of the item.
    /// </summary>
    public string FieldUseFunction { get => fieldUseFunction; set => Set(ref fieldUseFunction, value); }
    /// <summary>
    /// The battle usage of the item.
    /// </summary>
    public int BattleUsage { get => battleUsage; set => Set(ref battleUsage, value); }
    /// <summary>
    /// The battle usage function of the item.
    /// </summary>
    public string BattleUseFunction { get => battleUseFunction; set => Set(ref battleUseFunction, value); }
    /// <summary>
    /// The secondary id of the item.
    /// </summary>
    public object SecondaryId { get => secondaryId; set => Set(ref secondaryId, value); }
    /// <summary>
    /// The pic path of the item.
    /// </summary>
    public string Pic { get => pic; set => Set(ref pic, value); }
  }

  public class ItemDatabase : DatabaseBase {
    readonly OrderedDictionary<string, Item> nameToItem = new OrderedDictionary<string, Item>();

    /// <summary>
    /// The name of this database.
    /// </summary>
    public override string Name => "Item Database";

    /// <summary>
    /// Returns the items defined within the project.
    /// </summary>
    public IEnumerable<Item> Items => nameToItem.Values;

    /// <summary>
    /// Returns the item correpsonding to the provided id.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Item getFromId(string id) => nameToItem[id.Replace("ITEM_", "")];

    /// <summary>
    /// Reset the data held by this database.
    /// </summary>
    protected override void reset() => nameToItem.Clear();

    /// <summary>
    /// Deserialize the items from the project directory.
    /// </summary>
    /// <param name="deserializer"></param>
    protected override void deserialize(ProjectDeserializer deserializer)
      => Deserializer.deserialize(deserializer, this);


    public class JSONDatabase {
      public JSONDatabase() { }
      public JSONDatabase(ItemDatabase database) {
        Items = database.Items.Select(item => new JSONItem(item)).ToArray();
      }

      public void deserializeInto(ItemDatabase database) {
        foreach (var item in Items)
          database.nameToItem.Add(item.Identifier, item.deserialize());
      }

      public class JSONItem {
        public JSONItem() { }
        public JSONItem(Item item) {
          Name = item.Name;
          Identifier = item.Identifier;
          Price = item.Price;
          HoldEffect = item.HoldEffect;
          HoldEffectParam = item.HoldEffectParam;
          Description = item.Description;
          Importance = item.Importance;
          Unk19 = item.Unk19;
          Pocket = item.Pocket;
          Type = item.Type;
          FieldUseFunction = item.FieldUseFunction;
          BattleUsage = item.BattleUsage;
          BattleUseFunction = item.BattleUseFunction;
          SecondaryId = item.SecondaryId;
          Pic = item.Pic;
        }
        public Item deserialize() {
          return new Item() {
            Name = Name,
            Identifier = Identifier,
            Price = Price,
            HoldEffect = HoldEffect,
            HoldEffectParam = HoldEffectParam,
            Description = Description,
            Importance = Importance,
            Unk19 = Unk19,
            Pocket = Pocket,
            Type = Type,
            FieldUseFunction = FieldUseFunction,
            BattleUsage = BattleUsage,
            BattleUseFunction = BattleUseFunction,
            SecondaryId = SecondaryId,
            Pic = Pic
          };
        }

        public string Name { get; set; }
        public string Identifier { get; set; }
        public int Price { get; set; }
        public object HoldEffect { get; set; } = "";
        public object HoldEffectParam { get; set; } = null;
        public string Description { get; set; }
        public int Importance { get; set; } = 0;
        public int Unk19 { get; set; } = 0;
        public string Pocket { get; set; }
        public string Type { get; set; } = "";
        public string FieldUseFunction { get; set; } = null;
        public int BattleUsage { get; set; } = 0;
        public string BattleUseFunction { get; set; } = "";
        public object SecondaryId { get; set; } = null;
        public string Pic { get; set; }
      }

      public JSONItem[] Items { get; set; }
    }

    class Deserializer {
      public static void deserialize(ProjectDeserializer deserializer, ItemDatabase database) {
        string jsonPath = Path.Combine(deserializer.project.ProjectDir, "src", "data", "items.json");
        JSONDatabase jsonDatabase = JsonSerializer.Deserialize<JSONDatabase>(File.ReadAllText(jsonPath));
        jsonDatabase.deserializeInto(database);
      }
    }

    class Serializer {
      public static void serialize(ProjectSerializer serializer, ItemDatabase database) {
        JSONDatabase jsonDatabase = new JSONDatabase(database);
        string json = JsonSerializer.Serialize(jsonDatabase, new JsonSerializerOptions() {
          IgnoreNullValues = true,
          WriteIndented = true
        });
        File.WriteAllText(Path.Combine(serializer.project.ProjectDir, "src", "data", "items.json"), json);
      }
    }
  }
}
