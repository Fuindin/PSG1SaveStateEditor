using System.Buffers.Binary;
using System.IO.Hashing;
using System.Text;
using ZstdSharp;

namespace PSG1SaveStateEditor.SaveFormat;

/// <summary>
/// Reader/writer for PCSX2 v2 save states (".p2s").
/// The file is a ZIP container whose memory regions are compressed with
/// Zstandard (ZIP compression method 93). Standard zip libraries cannot read
/// these, so we parse the container ourselves and use ZstdSharp for payloads.
///
/// Untouched entries are repacked byte-for-byte (their original compressed
/// stream is reused), so only the regions we actually modify are recompressed.
/// </summary>
public sealed class P2SArchive
{
    private const uint LocalSig   = 0x04034b50;
    private const uint CentralSig = 0x02014b50;
    private const uint EocdSig    = 0x06054b50;
    public  const ushort MethodStore = 0;
    public  const ushort MethodZstd  = 93;

    public sealed class Entry
    {
        public required string Name;
        public ushort VersionMadeBy;
        public ushort VersionNeeded;
        public ushort Flags;
        public ushort Method;
        public ushort ModTime;
        public ushort ModDate;
        public uint   Crc32;
        public uint   CompressedSize;
        public uint   UncompressedSize;
        public ushort InternalAttrs;
        public uint   ExternalAttrs;

        /// <summary>Original compressed bytes copied verbatim from the source file.</summary>
        public required byte[] RawCompressed;

        /// <summary>Replacement uncompressed bytes (set when an entry is edited).</summary>
        public byte[]? NewData;

        public bool Dirty => NewData is not null;
    }

    public List<Entry> Entries { get; } = new();

    public Entry this[string name] =>
        Entries.FirstOrDefault(e => e.Name == name)
        ?? throw new KeyNotFoundException($"Entry '{name}' not found in save state.");

    public bool Has(string name) => Entries.Any(e => e.Name == name);

    /// <summary>Returns the decompressed bytes for an entry (edited bytes if present).</summary>
    public byte[] GetData(string name)
    {
        var e = this[name];
        if (e.NewData is not null)
        {
            return e.NewData;
        }

        return e.Method switch
        {
            MethodStore => e.RawCompressed,
            // Use the zstd frame's own declared content size; PCSX2's zip header
            // size field is not always consistent with the frame (observed on the
            // "Internal Structures" entry), so we must not cap on it.
            MethodZstd  => new Decompressor().Unwrap(e.RawCompressed).ToArray(),
            _ => throw new NotSupportedException($"Unsupported compression method {e.Method} for '{name}'.")
        };
    }

    /// <summary>Stages replacement bytes for an entry; written on Save().</summary>
    public void SetData(string name, byte[] data) => this[name].NewData = data;

    public static P2SArchive Load(string path) => Load(File.ReadAllBytes(path));

    public static P2SArchive Load(byte[] data)
    {
        var archive = new P2SArchive();

        // Locate the End Of Central Directory record by scanning backwards.
        int eocd = -1;
        for (int i = data.Length - 22; i >= 0; i--)
        {
            if (BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(i)) == EocdSig) 
            { 
                eocd = i; break; 
            }
        }

        if (eocd < 0)
        {
            throw new InvalidDataException("Not a valid zip/.p2s file (no EOCD record).");
        }

        ushort entryCount = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(eocd + 10));
        uint   cdOffset   = BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(eocd + 16));

        int p = (int)cdOffset;
        for (int n = 0; n < entryCount; n++)
        {
            if (BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(p)) != CentralSig)
            {
                throw new InvalidDataException($"Corrupt central directory at offset 0x{p:X}.");
            }

            ushort versionMadeBy = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(p + 4));
            ushort versionNeeded = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(p + 6));
            ushort flags         = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(p + 8));
            ushort method        = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(p + 10));
            ushort modTime       = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(p + 12));
            ushort modDate       = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(p + 14));
            uint   crc           = BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(p + 16));
            uint   compSize      = BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(p + 20));
            uint   uncompSize    = BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(p + 24));
            ushort nameLen       = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(p + 28));
            ushort extraLen      = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(p + 30));
            ushort commentLen    = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(p + 32));
            ushort internalAttrs = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(p + 36));
            uint   externalAttrs = BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(p + 38));
            uint   localOff      = BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(p + 42));
            string name          = Encoding.UTF8.GetString(data, p + 46, nameLen);
            p += 46 + nameLen + extraLen + commentLen;

            // Resolve the actual payload start via the local header (its name/extra
            // lengths can differ from the central record).
            int lp = (int)localOff;
            if (BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(lp)) != LocalSig)
            {
                throw new InvalidDataException($"Corrupt local header for '{name}'.");
            }

            ushort lNameLen  = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(lp + 26));
            ushort lExtraLen = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(lp + 28));
            int dataStart = lp + 30 + lNameLen + lExtraLen;

            archive.Entries.Add(new Entry
            {
                Name = name,
                VersionMadeBy = versionMadeBy,
                VersionNeeded = versionNeeded,
                Flags = flags,
                Method = method,
                ModTime = modTime,
                ModDate = modDate,
                Crc32 = crc,
                CompressedSize = compSize,
                UncompressedSize = uncompSize,
                InternalAttrs = internalAttrs,
                ExternalAttrs = externalAttrs,
                RawCompressed = data.AsSpan(dataStart, (int)compSize).ToArray(),
            });
        }

        return archive;
    }

    public void Save(string path)
    {
        using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
        Save(fs);
    }

    public void Save(Stream output)
    {
        // Precompute the payload for every entry (recompress only edited ones).
        var payloads = new byte[Entries.Count][];
        for (int i = 0; i < Entries.Count; i++)
        {
            var e = Entries[i];
            if (e.Dirty)
            {
                byte[] raw = e.NewData!;
                e.Crc32 = Crc32.HashToUInt32(raw);
                e.UncompressedSize = (uint)raw.Length;
                byte[] payload = e.Method switch
                {
                    MethodStore => raw,
                    MethodZstd  => new Compressor(CompressionLevel).Wrap(raw).ToArray(),
                    _ => throw new NotSupportedException($"Cannot write compression method {e.Method}.")
                };
                e.CompressedSize = (uint)payload.Length;
                payloads[i] = payload;
            }
            else
            {
                payloads[i] = e.RawCompressed;
            }
        }

        using var w = new BinaryWriter(output, Encoding.UTF8, leaveOpen: true);
        var localOffsets = new uint[Entries.Count];

        // Local headers + payloads.
        for (int i = 0; i < Entries.Count; i++)
        {
            var e = Entries[i];
            localOffsets[i] = (uint)output.Position;
            byte[] nameBytes = Encoding.UTF8.GetBytes(e.Name);

            w.Write(LocalSig);
            w.Write(e.VersionNeeded);
            w.Write(e.Flags);
            w.Write(e.Method);
            w.Write(e.ModTime);
            w.Write(e.ModDate);
            w.Write(e.Crc32);
            w.Write(e.CompressedSize);
            w.Write(e.UncompressedSize);
            w.Write((ushort)nameBytes.Length);
            w.Write((ushort)0);             // extra length
            w.Write(nameBytes);
            w.Write(payloads[i]);
        }

        // Central directory.
        uint cdStart = (uint)output.Position;
        for (int i = 0; i < Entries.Count; i++)
        {
            var e = Entries[i];
            byte[] nameBytes = Encoding.UTF8.GetBytes(e.Name);

            w.Write(CentralSig);
            w.Write(e.VersionMadeBy);
            w.Write(e.VersionNeeded);
            w.Write(e.Flags);
            w.Write(e.Method);
            w.Write(e.ModTime);
            w.Write(e.ModDate);
            w.Write(e.Crc32);
            w.Write(e.CompressedSize);
            w.Write(e.UncompressedSize);
            w.Write((ushort)nameBytes.Length);
            w.Write((ushort)0);             // extra length
            w.Write((ushort)0);             // comment length
            w.Write((ushort)0);             // disk number start
            w.Write(e.InternalAttrs);
            w.Write(e.ExternalAttrs);
            w.Write(localOffsets[i]);
            w.Write(nameBytes);
        }
        uint cdSize = (uint)output.Position - cdStart;

        // End of central directory.
        w.Write(EocdSig);
        w.Write((ushort)0);                 // this disk
        w.Write((ushort)0);                 // disk with CD start
        w.Write((ushort)Entries.Count);     // entries on this disk
        w.Write((ushort)Entries.Count);     // total entries
        w.Write(cdSize);
        w.Write(cdStart);
        w.Write((ushort)0);                 // comment length
        w.Flush();
    }

    /// <summary>Zstd level used when recompressing edited regions. PCSX2 reads any valid frame.</summary>
    public int CompressionLevel { get; set; } = 7;
}
