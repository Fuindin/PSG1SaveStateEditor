using System.Buffers.Binary;

namespace PSG1SaveStateEditor.Game;

/// <summary>One slot of a character's item bag.</summary>
public readonly record struct BagItem(int Index, int Id, bool Equipped)
{
    public string Name => Psg1Items.Name(Id);
    public ItemKind Kind => Psg1Items.Kind(Id);
}

/// <summary>A live view over one character record in eeMemory.bin.</summary>
public sealed class Psg1Character
{
    private readonly byte[] _ee;
    private readonly Psg1Layout _layout;
    private readonly MemberDef _member;

    public Psg1Character(byte[] ee, Psg1Layout layout, MemberDef member)
    {
        _ee = ee;
        _layout = layout;
        _member = member;
    }

    public int Slot => _member.Slot;
    public string Name => _member.Name;
    public string Role => string.IsNullOrEmpty(_member.Role) ? "—" : _member.Role!;

    private int RecordBase => _layout.CharArrayBase + _member.Slot * _layout.CharStride;

    private bool InRange(int at, int size) => at >= 0 && at + size <= _ee.Length;

    // ---- Stats -----------------------------------------------------------

    public bool TryGet(string key, out long value)
    {
        value = 0;
        var f = _layout.Fields.FirstOrDefault(x => x.Key == key);
        if (f is null)
        {
            return false;
        }

        int at = RecordBase + f.Offset;
        if (!InRange(at, 4))
        {
            return false;
        }

        value = BinaryPrimitives.ReadUInt32LittleEndian(_ee.AsSpan(at));
        return true;
    }

    public long Get(string key) => TryGet(key, out long v) ? v : 0;

    public void Set(string key, long value)
    {
        var f = _layout.Fields.FirstOrDefault(x => x.Key == key);
        if (f is null)
        {
            return;
        }

        value = Math.Clamp(value, f.Min, f.Max);
        int at = RecordBase + f.Offset;
        if (InRange(at, 4))
        {
            BinaryPrimitives.WriteUInt32LittleEndian(_ee.AsSpan(at), (uint)value);
        }
    }

    // ---- Spells (read-only) ----------------------------------------------

    public IReadOnlyList<int> Spells()
    {
        var list = new List<int>();
        int at = RecordBase + _layout.SpellOffset;
        for (int i = 0; i < _layout.SpellCapacity && InRange(at + i, 1); i++)
        {
            byte id = _ee[at + i];
            if (id == 0)
            {
                break;
            }

            list.Add(id);
        }

        return list;
    }

    // ---- Item bag ----------------------------------------------------------

    private uint EquipMask
    {
        get
        {
            int at = RecordBase + _layout.EquipMaskOffset;
            return InRange(at, 4) ? BinaryPrimitives.ReadUInt32LittleEndian(_ee.AsSpan(at)) : 0;
        }
        set
        {
            int at = RecordBase + _layout.EquipMaskOffset;
            if (InRange(at, 4))
            {
                BinaryPrimitives.WriteUInt32LittleEndian(_ee.AsSpan(at), value);
            }
        }
    }

    /// <summary>This character's bag, in storage order (equipped items are flagged).</summary>
    public IReadOnlyList<BagItem> Items()
    {
        var list = new List<BagItem>();
        uint mask = EquipMask;
        int at = RecordBase + _layout.ItemOffset;
        for (int i = 0; i < _layout.ItemSlotsInRecord && InRange(at + i * 2, 2); i++)
        {
            int id = BinaryPrimitives.ReadUInt16LittleEndian(_ee.AsSpan(at + i * 2));
            if (id == 0)
            {
                break;
            }

            list.Add(new BagItem(i, id, (mask & (1u << i)) != 0));
        }

        return list;
    }

    public IEnumerable<BagItem> Equipped() => Items().Where(i => i.Equipped);

    public int ItemCapacity => _layout.ItemCapacity;

    public bool CanAdd => Items().Count < _layout.ItemCapacity;

    private void WriteId(int index, int id)
    {
        int at = RecordBase + _layout.ItemOffset + index * 2;
        if (index >= 0 && index < _layout.ItemSlotsInRecord && InRange(at, 2))
        {
            BinaryPrimitives.WriteUInt16LittleEndian(_ee.AsSpan(at), (ushort)id);
        }
    }

    /// <summary>Appends an (unequipped) item. Returns false if the bag is full.</summary>
    public bool AddItem(int id)
    {
        if (id is <= 0 or > 0xFFFF)
        {
            return false;
        }

        int count = Items().Count;
        if (count >= _layout.ItemCapacity)
        {
            return false;
        }

        WriteId(count, id);
        if (count + 1 < _layout.ItemSlotsInRecord)
        {
            WriteId(count + 1, 0);   // keep the list 0-terminated
        }

        return true;
    }

    /// <summary>Swaps an unequipped item for another id. Equipped items are left alone.</summary>
    public bool ReplaceItem(int index, int id)
    {
        var items = Items();
        if (index < 0 || index >= items.Count || items[index].Equipped || id is <= 0 or > 0xFFFF)
        {
            return false;
        }

        WriteId(index, id);
        return true;
    }

    /// <summary>
    /// Removes an unequipped item, shifting later items down and shifting the equipped
    /// bitmask to match so the right pieces stay equipped.
    /// </summary>
    public bool RemoveItem(int index)
    {
        var items = Items();
        if (index < 0 || index >= items.Count || items[index].Equipped)
        {
            return false;
        }

        for (int i = index; i < items.Count - 1; i++)
        {
            WriteId(i, items[i + 1].Id);
        }

        WriteId(items.Count - 1, 0);

        uint mask = EquipMask;
        uint low = mask & ((1u << index) - 1);
        uint high = (mask >> (index + 1)) << index;
        EquipMask = low | high;
        return true;
    }
}
