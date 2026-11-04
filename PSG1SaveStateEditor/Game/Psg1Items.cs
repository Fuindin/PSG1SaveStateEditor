using System.Text.Json;

namespace PSG1SaveStateEditor.Game;

/// <summary>Equipment category, in the order the Status screen lists it.</summary>
public enum ItemKind { Weapon, Armor, Shield, Crystal, Item }

/// <summary>
/// Item-ID → name table for Phantasy Star Generation:1, confirmed by matching each
/// character's in-RAM item bag against the in-game ITEM menu. IDs share one space:
/// shields are low (0x01–0x0F), then armor, weapons, crystals, and usable items (0x54+).
/// Extend/override names without rebuilding via <c>psg1_items.json</c> next to the exe
/// (a simple { "id": "Name" } map; ids may be decimal or 0x-hex). Unknown IDs show as "Item #0xNN".
/// </summary>
public static class Psg1Items
{
    private static readonly Dictionary<int, (string Name, ItemKind Kind)> BuiltIn = new()
    {
        // Shields
        { 0x03, ("Ceramic Shield", ItemKind.Shield) },
        { 0x07, ("Protector",      ItemKind.Shield) },
        { 0x0A, ("Plasma Barrier", ItemKind.Shield) },
        { 0x0D, ("Leather Gloves", ItemKind.Shield) },
        // Armor
        { 0x12, ("Light Suit",     ItemKind.Armor) },
        { 0x18, ("Steel Armor",    ItemKind.Armor) },
        { 0x1D, ("Silk Mantle",    ItemKind.Armor) },
        { 0x21, ("Wild Loincloth", ItemKind.Armor) },
        // Weapons
        { 0x2C, ("Ceramic Sword",  ItemKind.Weapon) },
        { 0x31, ("Battle Axe",     ItemKind.Weapon) },
        { 0x3F, ("Wooden Staff",   ItemKind.Weapon) },
        { 0x41, ("Long Mace",      ItemKind.Weapon) },
        { 0x45, ("Bronze Claw",    ItemKind.Weapon) },
        // Crystals
        { 0x4F, ("Frozhe Crystal", ItemKind.Crystal) },
        // Usable items
        { 0x54, ("Trimate",        ItemKind.Item) },
        { 0x58, ("Atlas",          ItemKind.Item) },
        { 0x59, ("Antidote",       ItemKind.Item) },
        { 0x5C, ("Escape Cloth",   ItemKind.Item) },
        { 0x5F, ("Soothing Flute", ItemKind.Item) },
        { 0x60, ("Searchlight",    ItemKind.Item) },
        { 0x61, ("TranCarpet",     ItemKind.Item) },
        { 0x6B, ("LandMaster",     ItemKind.Item) },
        { 0x6C, ("FlowMover",      ItemKind.Item) },
    };

    public static IReadOnlyDictionary<int, string> Names { get; } = LoadMerged();

    public static string Name(int id) =>
        Names.TryGetValue(id, out var n) ? n : $"Item #0x{id:X2}";

    public static bool IsKnown(int id) => Names.ContainsKey(id);

    /// <summary>
    /// Category of an item: from the confirmed table, otherwise inferred from the ID range
    /// that the confirmed items fall into.
    /// </summary>
    public static ItemKind Kind(int id)
    {
        if (BuiltIn.TryGetValue(id, out var b))
        {
            return b.Kind;
        }

        return id switch
        {
            <= 0x0F => ItemKind.Shield,
            <= 0x2A => ItemKind.Armor,
            <= 0x4D => ItemKind.Weapon,
            <= 0x53 => ItemKind.Crystal,
            _ => ItemKind.Item,
        };
    }

    private static Dictionary<int, string> LoadMerged()
    {
        var map = BuiltIn.ToDictionary(kv => kv.Key, kv => kv.Value.Name);
        foreach (var (id, name) in JsonNames.Load("psg1_items.json"))
        {
            map[id] = name;   // user file wins over built-in
        }

        return map;
    }
}

/// <summary>Spell-ID → name table (display only). Names come from <c>psg1_spells.json</c>.</summary>
public static class Psg1Spells
{
    public static IReadOnlyDictionary<int, string> Names { get; } = JsonNames.Load("psg1_spells.json");

    public static string Name(int id) =>
        Names.TryGetValue(id, out var n) ? n : $"Spell #0x{id:X2}";
}

/// <summary>Reads a { "id": "Name" } override map; keys may be decimal or 0x-prefixed hex.</summary>
internal static class JsonNames
{
    public static Dictionary<int, string> Load(string fileName)
    {
        var map = new Dictionary<int, string>();
        try
        {
            string path = Path.Combine(AppContext.BaseDirectory, fileName);
            if (!File.Exists(path))
            {
                return map;
            }

            var raw = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(path));
            if (raw is null)
            {
                return map;
            }

            foreach (var (k, v) in raw)
            {
                if (string.IsNullOrWhiteSpace(v) || k.StartsWith('_'))
                {
                    continue;
                }

                string key = k.Trim();
                int id;
                bool ok = key.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                    ? int.TryParse(key[2..], System.Globalization.NumberStyles.HexNumber, null, out id)
                    : int.TryParse(key, out id);
                if (ok)
                {
                    map[id] = v;
                }
            }
        }
        catch { /* ignore a malformed override file */ }

        return map;
    }
}
