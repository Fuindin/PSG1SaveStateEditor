using System.Buffers.Binary;
using PSG1SaveStateEditor.SaveFormat;

namespace PSG1SaveStateEditor.Game;

/// <summary>
/// High-level model: opens a .p2s, exposes meseta and the 4 character records for
/// editing, and repacks to a new file. Reads/writes eeMemory.bin in place.
/// </summary>
public sealed class Psg1Save
{
    private readonly P2SArchive _archive;
    private readonly byte[] _ee;
    private readonly Psg1Layout _layout;

    public string SourcePath { get; }
    public string? DetectedSerial { get; }
    public IReadOnlyList<Psg1Character> Characters { get; }

    private Psg1Save(string path, P2SArchive archive, byte[] ee, Psg1Layout layout)
    {
        SourcePath = path;
        _archive = archive;
        _ee = ee;
        _layout = layout;
        DetectedSerial = FindSerial(ee);
        Characters = layout.Members.Select(m => new Psg1Character(ee, layout, m)).ToList();
    }

    public static Psg1Save Open(string path, Psg1Layout layout)
    {
        var archive = P2SArchive.Load(path);
        if (!archive.Has("eeMemory.bin"))
        {
            throw new InvalidDataException("This .p2s has no eeMemory.bin region.");
        }

        return new Psg1Save(path, archive, archive.GetData("eeMemory.bin"), layout);
    }

    /// <summary>
    /// True if the character array looks sane: every record carries its own roster id
    /// and Alisa's level / max HP are in range.
    /// </summary>
    public bool StructureFound
    {
        get
        {
            int b = _layout.CharArrayBase;
            for (int i = 0; i < _layout.Members.Count; i++)
            {
                int at = b + _layout.Members[i].Slot * _layout.CharStride + _layout.CharIdOffset;
                if (at < 0 || at + 4 > _ee.Length
                    || BinaryPrimitives.ReadUInt32LittleEndian(_ee.AsSpan(at)) != (uint)_layout.Members[i].Slot)
                {
                    return false;
                }
            }

            var alisa = Characters.FirstOrDefault();
            if (alisa is null)
            {
                return false;
            }

            long level = alisa.Get("level"), maxHp = alisa.Get("hpMax");
            return level is >= 1 and <= 99 && maxHp is >= 1 and <= 9999;
        }
    }

    public long Meseta
    {
        get
        {
            int at = _layout.MesetaOffset;
            return at >= 0 && at + 4 <= _ee.Length
                ? BinaryPrimitives.ReadUInt32LittleEndian(_ee.AsSpan(at))
                : 0;
        }
        set
        {
            long v = Math.Clamp(value, 0, _layout.MesetaMax);
            int at = _layout.MesetaOffset;
            if (at >= 0 && at + 4 <= _ee.Length)
            {
                BinaryPrimitives.WriteUInt32LittleEndian(_ee.AsSpan(at), (uint)v);
            }
        }
    }

    public void SaveAs(string destPath)
    {
        _archive.SetData("eeMemory.bin", _ee);
        _archive.Save(destPath);
    }

    /// <summary>Best-effort PS2 serial detection (SLxS_nnn.nn) for the status bar.</summary>
    private static string? FindSerial(byte[] ee)
    {
        for (int i = 0; i < ee.Length - 11; i++)
        {
            if (ee[i] != (byte)'S' || ee[i + 1] != (byte)'L')
            {
                continue;
            }

            char c2 = (char)ee[i + 2], c3 = (char)ee[i + 3];
            if (c2 is 'U' or 'E' or 'P' or 'A' && c3 is 'S' or 'L' or 'M' or 'B'
                && ee[i + 4] == (byte)'_' && char.IsDigit((char)ee[i + 5]))
            {
                return System.Text.Encoding.ASCII.GetString(ee, i, 11);
            }
        }

        return null;
    }
}
