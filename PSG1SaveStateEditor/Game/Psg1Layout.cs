using System.Text.Json;

namespace PSG1SaveStateEditor.Game;

/// <summary>One editable numeric field within a character's record (all u32 LE in PSG1).</summary>
public sealed class FieldDef
{
    public string Key { get; set; } = "";
    /// <summary>Short in-game label (e.g. "ATT.").</summary>
    public string Label { get; set; } = "";
    /// <summary>Plain-English name shown beside the label.</summary>
    public string Description { get; set; } = "";
    /// <summary>Optional muted note shown after the editor.</summary>
    public string? Hint { get; set; }
    /// <summary>Byte offset relative to the character record base.</summary>
    public int Offset { get; set; } = -1;
    public long Min { get; set; }
    public long Max { get; set; } = 9999;
}

/// <summary>A roster slot: canonical name + role (display only).</summary>
public sealed class MemberDef
{
    public int Slot { get; set; }
    public string Name { get; set; } = "";
    public string? Role { get; set; }
}

/// <summary>
/// Memory map for Phantasy Star Generation:1 (SLPM-623.62) party data, verified against a
/// real PCSX2 save (eeMemory.bin). Four records of 0x84 bytes in roster order
/// (Alisa, Tyrone, Myau, Lutz). Each record ends with that character's own item bag.
/// JSON-overridable via psg1_layout.json next to the executable.
/// </summary>
public sealed class Psg1Layout
{
    /// <summary>Absolute offset of character[0] (Alisa) in eeMemory.bin.</summary>
    public int CharArrayBase { get; set; } = 0x191130;
    public int CharStride { get; set; } = 0x84;
    public int CharCount { get; set; } = 4;

    /// <summary>Relative offset of the u32 character id (0..3) — used as a sanity check.</summary>
    public int CharIdOffset { get; set; } = 0x04;

    /// <summary>
    /// Absolute offset of the party meseta (u32), in the party header just before the
    /// character array. Verified against the MAIN MENU "MONEY" readout (56302).
    /// </summary>
    public int MesetaOffset { get; set; } = 0x1910B0;
    public long MesetaMax { get; set; } = 999_999;

    /// <summary>Relative offset of the learned-spell list (one byte per spell id, 0-terminated).</summary>
    public int SpellOffset { get; set; } = 0x44;
    public int SpellCapacity { get; set; } = 32;

    /// <summary>
    /// Relative offset of the u32 equipped bitmask: bit i set = item slot i is equipped.
    /// </summary>
    public int EquipMaskOffset { get; set; } = 0x64;

    /// <summary>Relative offset of the item bag: u16 item ids, 0-terminated.</summary>
    public int ItemOffset { get; set; } = 0x68;

    /// <summary>
    /// Number of u16 item slots physically available in the record (0x68..0x83 = 14).
    /// </summary>
    public int ItemSlotsInRecord { get; set; } = 14;

    /// <summary>
    /// Maximum items the editor will let a character carry. 10 is the largest bag observed
    /// in a real save, so it is known-safe; raise it (up to ItemSlotsInRecord) in
    /// psg1_layout.json if the game allows more.
    /// </summary>
    public int ItemCapacity { get; set; } = 10;

    public List<FieldDef> Fields { get; set; } = new();
    public List<MemberDef> Members { get; set; } = new();

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
    };

    private const string EquipNote = "base value — the Status screen adds equipment";

    public static Psg1Layout Default() => new()
    {
        Fields =
        {
            new() { Key = "level",   Label = "LV",       Description = "Level",           Offset = 0x30, Min = 1, Max = 99 },
            new() { Key = "exp",     Label = "EXP",      Description = "Experience",      Offset = 0x34, Min = 0, Max = 9_999_999 },
            new() { Key = "next",    Label = "NEXT",     Description = "Next level at",   Offset = 0x38, Min = 0, Max = 9_999_999 },
            new() { Key = "hpCur",   Label = "HP",       Description = "Current HP",      Offset = 0x08, Min = 0, Max = 9999 },
            new() { Key = "hpMax",   Label = "HP MAX",   Description = "Max HP",          Offset = 0x0C, Min = 1, Max = 9999 },
            new() { Key = "mpCur",   Label = "MP",       Description = "Current MP",      Offset = 0x10, Min = 0, Max = 9999 },
            new() { Key = "mpMax",   Label = "MP MAX",   Description = "Max MP",          Offset = 0x14, Min = 0, Max = 9999 },
            new() { Key = "attack",  Label = "ATT.",     Description = "Attack",          Offset = 0x18, Min = 0, Max = 999, Hint = EquipNote },
            new() { Key = "defense", Label = "DEF.",     Description = "Defense",         Offset = 0x1C, Min = 0, Max = 999, Hint = EquipNote },
            new() { Key = "mpow",    Label = "M. POW.",  Description = "Magic Power",     Offset = 0x20, Min = 0, Max = 999 },
            new() { Key = "mdef",    Label = "M. DEF.",  Description = "Magic Defense",   Offset = 0x24, Min = 0, Max = 999 },
            new() { Key = "hit",     Label = "HIT",      Description = "Hit",             Offset = 0x28, Min = 0, Max = 999 },
            new() { Key = "dex",     Label = "DEXT.",    Description = "Dexterity",       Offset = 0x2C, Min = 0, Max = 999, Hint = EquipNote },
        },
        // Roster order in RAM (matches the u32 id at +0x04).
        Members =
        {
            new() { Slot = 0, Name = "Alisa",  Role = "Heroine of Palma" },
            new() { Slot = 1, Name = "Tyrone", Role = "Warrior" },
            new() { Slot = 2, Name = "Myau",   Role = "Musk Cat" },
            new() { Slot = 3, Name = "Lutz",   Role = "Esper" },
        }
    };

    public static Psg1Layout Load()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "psg1_layout.json");
        if (File.Exists(path))
        {
            try
            {
                var loaded = JsonSerializer.Deserialize<Psg1Layout>(File.ReadAllText(path), JsonOpts);
                if (loaded is { Fields.Count: > 0, Members.Count: > 0 })
                {
                    loaded.ItemCapacity = Math.Clamp(loaded.ItemCapacity, 1, loaded.ItemSlotsInRecord);
                    return loaded;
                }
            }
            catch { /* fall back to baked-in default */ }
        }

        return Default();
    }
}
