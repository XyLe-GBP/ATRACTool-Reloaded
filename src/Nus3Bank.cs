using System.Buffers.Binary;
using System.IO;
using System.Text;

namespace ATRACTool_Reloaded
{
    internal enum Nus3SubfileCodec
    {
        Unknown,
        RiffUnknown,
        PcmWave,
        Atrac3,
        Atrac9,
        Bnsf,
        Ivag,
    }

    internal enum Nus3BankBuildFlavor
    {
        Nus3Bank,
        Nub2,
    }

    internal readonly struct Nus3RiffLoopPoints
    {
        public Nus3RiffLoopPoints(int startSample, int endSample, int riffStartAdjustment, int riffEndAdjustment)
        {
            StartSample = startSample;
            EndSample = endSample;
            RiffStartAdjustment = riffStartAdjustment;
            RiffEndAdjustment = riffEndAdjustment;
        }

        public int StartSample { get; }
        public int EndSample { get; }
        public int RiffStartAdjustment { get; }
        public int RiffEndAdjustment { get; }

        public bool TryGetRiffLoop(out uint start, out uint end)
        {
            start = 0;
            end = 0;

            long adjustedStart = (long)StartSample + RiffStartAdjustment;
            long adjustedEnd = (long)EndSample + RiffEndAdjustment;
            if (StartSample < 0 || EndSample <= StartSample ||
                adjustedStart < 0 || adjustedEnd < adjustedStart ||
                adjustedStart > uint.MaxValue || adjustedEnd > uint.MaxValue)
            {
                return false;
            }

            start = (uint)adjustedStart;
            end = (uint)adjustedEnd;
            return true;
        }
    }

    internal sealed class Nus3Chunk
    {
        public required string Id { get; init; }
        public required int ChunkOffset { get; init; }
        public required int DataOffset { get; init; }
        public required int Size { get; init; }
    }

    internal sealed class Nus3Tone
    {
        public required int Index { get; init; }
        public required string Name { get; init; }
        public required int RecordOffset { get; init; }
        public required int RecordSize { get; init; }
        public required int StreamReferenceOffset { get; init; }
        public required int StreamRelativeOffset { get; init; }
        public required int StreamSize { get; init; }
        public required int SubfileOffset { get; init; }
        public required Nus3SubfileCodec Codec { get; init; }
    }

    internal sealed class Nus3BankEncodeStream
    {
        public string Name { get; init; } = "";
        public byte[] Data { get; init; } = [];
        public Nus3RiffLoopPoints? LoopPoints { get; init; }
    }

    internal sealed class Nus3BankFile : IDisposable
    {
        private static readonly byte[] Atrac9WaveGuid =
        [
            0xD2, 0x42, 0xE1, 0x47, 0xBA, 0x36, 0x8D, 0x4D,
            0x88, 0xFC, 0x61, 0x65, 0x4F, 0x8C, 0x83, 0x6C
        ];

        private static readonly byte[] Atrac3PlusWaveGuid =
        [
            0xBF, 0xAA, 0x23, 0xE9, 0x58, 0xCB, 0x71, 0x44,
            0xA1, 0x19, 0xFF, 0xFA, 0x01, 0xE4, 0xCE, 0x62
        ];

        private byte[]? _data;
        private readonly List<Nus3Chunk> _chunks;
        private readonly List<Nus3Tone> _tones;
        private byte[] Data => _data ?? throw new ObjectDisposedException(nameof(Nus3BankFile));

        private Nus3BankFile(string sourcePath, byte[] data, List<Nus3Chunk> chunks, List<Nus3Tone> tones, Nus3Chunk packChunk)
        {
            SourcePath = sourcePath;
            _data = data;
            _chunks = chunks;
            _tones = tones;
            PackChunk = packChunk;
        }

        public string SourcePath { get; }
        public Nus3Chunk PackChunk { get; }
        public IReadOnlyList<Nus3Chunk> Chunks => _chunks;
        public IReadOnlyList<Nus3Tone> Tones => _tones;
        public IEnumerable<Nus3Tone> AtracTones => _tones.Where(t => t.Codec is Nus3SubfileCodec.Atrac3 or Nus3SubfileCodec.Atrac9);
        public IEnumerable<Nus3Tone> DecodableWaveTones => _tones.Where(t => t.Codec is Nus3SubfileCodec.Atrac3 or Nus3SubfileCodec.Atrac9 or Nus3SubfileCodec.PcmWave or Nus3SubfileCodec.Ivag);
        public int DecodableWaveToneCount => _tones.Count(t => t.Codec is Nus3SubfileCodec.Atrac3 or Nus3SubfileCodec.Atrac9 or Nus3SubfileCodec.PcmWave or Nus3SubfileCodec.Ivag);

        public void Dispose()
        {
            _data = null;
            _chunks.Clear();
            _tones.Clear();
        }

        public static bool HasNus3BankExtension(string path)
        {
            string extension = Path.GetExtension(path);
            return string.Equals(extension, ".nus3bank", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(extension, ".nub2", StringComparison.OrdinalIgnoreCase);
        }

        public static bool HasNub2Extension(string path)
        {
            return string.Equals(Path.GetExtension(path), ".nub2", StringComparison.OrdinalIgnoreCase);
        }

        public static bool LooksLikeNus3Bank(string path)
        {
            if (!File.Exists(path)) return false;
            Span<byte> header = stackalloc byte[16];
            using var fs = File.OpenRead(path);
            if (fs.Read(header) < header.Length) return false;
            return Encoding.ASCII.GetString(header[..4]) == "NUS3"
                && Encoding.ASCII.GetString(header.Slice(8, 4)) == "BANK"
                && Encoding.ASCII.GetString(header.Slice(12, 4)) == "TOC ";
        }

        public static Nus3BankFile Load(string path)
        {
            FormMain.DebugInfo($"[Nus3Bank] Load started. path={path}");
            byte[] data = File.ReadAllBytes(path);
            if (data.Length < 0x18)
                throw new InvalidDataException("The file is too small for a NUS3BANK header.");

            if (ReadAscii(data, 0x00, 4) != "NUS3")
                throw new InvalidDataException("The file does not start with NUS3.");

            if (ReadAscii(data, 0x08, 4) != "BANK" || ReadAscii(data, 0x0C, 4) != "TOC ")
                throw new InvalidDataException("The file is not a NUS3 BANKTOC container.");

            uint sizeField = ReadU32LE(data, 0x04);
            if (sizeField != data.Length - 8)
                throw new InvalidDataException("The NUS3 size field does not match the file size.");

            uint tocSize = ReadU32LE(data, 0x10);
            uint chunkCount = ReadU32LE(data, 0x14);
            if (chunkCount == 0 || chunkCount > 256)
                throw new InvalidDataException("The NUS3 TOC chunk count is invalid.");

            int tocEntryOffset = 0x18;
            int tocEnd = checked(0x14 + (int)tocSize);
            int tocEntryEnd = checked(tocEntryOffset + (int)chunkCount * 8);
            if (tocEnd > data.Length || tocEntryEnd > tocEnd)
                throw new InvalidDataException("The NUS3 TOC is outside the file bounds.");

            var entries = new List<(string Id, int Size)>();
            for (int i = 0; i < chunkCount; i++)
            {
                int entryOffset = tocEntryOffset + i * 8;
                entries.Add((ReadAscii(data, entryOffset, 4), checked((int)ReadU32LE(data, entryOffset + 4))));
            }

            int chunkOffset = tocEnd;
            var chunks = new List<Nus3Chunk>();
            foreach (var entry in entries)
            {
                if (chunkOffset + 8 > data.Length)
                    throw new InvalidDataException("A NUS3 chunk header is outside the file bounds.");

                string bodyId = ReadAscii(data, chunkOffset, 4);
                int bodySize = checked((int)ReadU32LE(data, chunkOffset + 4));
                if (bodyId != entry.Id || bodySize != entry.Size)
                    throw new InvalidDataException("A NUS3 chunk does not match the TOC entry.");

                int dataOffset = chunkOffset + 8;
                int chunkEnd = checked(dataOffset + bodySize);
                if (chunkEnd > data.Length)
                    throw new InvalidDataException("A NUS3 chunk extends beyond the file bounds.");

                chunks.Add(new Nus3Chunk
                {
                    Id = bodyId,
                    ChunkOffset = chunkOffset,
                    DataOffset = dataOffset,
                    Size = bodySize,
                });

                chunkOffset = chunkEnd;
            }

            if (chunkOffset != data.Length)
                throw new InvalidDataException("The NUS3 chunks do not end at the file size.");

            Nus3Chunk toneChunk = chunks.FirstOrDefault(c => c.Id == "TONE")
                ?? throw new InvalidDataException("The NUS3BANK does not contain a TONE chunk.");
            Nus3Chunk packChunk = chunks.FirstOrDefault(c => c.Id == "PACK")
                ?? throw new InvalidDataException("The NUS3BANK does not contain a PACK chunk.");

            var tones = ParseTones(data, toneChunk, packChunk);
            FormMain.DebugInfo($"[Nus3Bank] Load completed. path={path}, chunks={chunks.Count}, tones={tones.Count}");
            return new Nus3BankFile(path, data, chunks, tones, packChunk);
        }

        public static Nus3SubfileCodec GetPrimaryAtracCodec(string path)
        {
            try
            {
                using var bank = Load(path);
                return bank.AtracTones.FirstOrDefault()?.Codec ?? Nus3SubfileCodec.Unknown;
            }
            catch
            {
                return Nus3SubfileCodec.Unknown;
            }
        }

        public void ExtractToneToFile(Nus3Tone tone, string outputPath)
        {
            if (!_tones.Contains(tone))
                throw new ArgumentException("The tone does not belong to this bank.", nameof(tone));

            FormMain.DebugInfo($"[Nus3Bank] Extract tone started. source={SourcePath}, tone={tone.Name}, codec={tone.Codec}, output={outputPath}");
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
            using var fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);
            fs.Write(Data, tone.SubfileOffset, tone.StreamSize);
            FormMain.DebugInfo($"[Nus3Bank] Extract tone completed. output={outputPath}, bytes={tone.StreamSize}");
        }

        public bool TryReadToneLoopPoints(Nus3Tone tone, bool isAtrac3Ps3, out int startSample, out int endSample, out int sampleRate)
        {
            startSample = 0;
            endSample = 0;
            sampleRate = 0;

            if (!_tones.Contains(tone) || tone.StreamSize <= 0)
                return false;

            byte[] data = Data;
            if (!TryFindRiffChunk(data, tone.SubfileOffset, tone.StreamSize, "smpl", out _, out int smplDataOffset, out int smplSize, out _) ||
                smplSize < 60 ||
                ReadU32LE(data, smplDataOffset + 28) == 0)
            {
                return false;
            }

            uint rawStart = ReadU32LE(data, smplDataOffset + 44);
            uint rawEnd = ReadU32LE(data, smplDataOffset + 48);
            if (rawEnd <= rawStart || rawStart > int.MaxValue || rawEnd > int.MaxValue)
                return false;

            TryReadRiffWaveSampleRate(data, tone.SubfileOffset, tone.StreamSize, out sampleRate);

            (int startAdjustment, int endAdjustment) = GetLoopReadAdjustments(tone.Codec, sampleRate, isAtrac3Ps3);
            long adjustedStart = (long)rawStart - startAdjustment;
            long adjustedEnd = (long)rawEnd - endAdjustment;

            if (adjustedStart < 0 || adjustedEnd <= adjustedStart ||
                adjustedStart > int.MaxValue || adjustedEnd > int.MaxValue)
            {
                return false;
            }

            startSample = (int)adjustedStart;
            endSample = (int)adjustedEnd;
            return true;
        }

        public string MakeToneOutputName(Nus3Tone tone, string extension)
        {
            string bankName = SanitizeFileName(Path.GetFileNameWithoutExtension(SourcePath));
            string toneName = SanitizeFileName(tone.Name);
            if (string.IsNullOrWhiteSpace(toneName))
                toneName = "tone";

            return $"{bankName}__{tone.Index:D4}_{toneName}{extension}";
        }

        public static void WriteSingleToneBank(string encodedSubfilePath, string outputPath, string bankName, string toneName, Nus3RiffLoopPoints? loopPoints = null, Nus3BankBuildFlavor flavor = Nus3BankBuildFlavor.Nus3Bank)
        {
            FormMain.DebugInfo($"[Nus3Bank] Write single tone started. input={encodedSubfilePath}, output={outputPath}, flavor={flavor}");
            byte[] subfile = File.ReadAllBytes(encodedSubfilePath);
            WriteSingleToneBank(subfile, outputPath, bankName, toneName, loopPoints, flavor);
            FormMain.DebugInfo($"[Nus3Bank] Write single tone completed. output={outputPath}");
        }

        public static void WriteSingleToneBank(byte[] encodedSubfile, string outputPath, string bankName, string toneName, Nus3RiffLoopPoints? loopPoints = null, Nus3BankBuildFlavor flavor = Nus3BankBuildFlavor.Nus3Bank)
        {
            if (encodedSubfile.Length == 0)
                throw new InvalidDataException("The encoded subfile is empty.");

            if (loopPoints.HasValue)
                encodedSubfile = EnsureRiffLoopChunk(encodedSubfile, loopPoints.Value);

            ValidateNub2StreamCodec(encodedSubfile, flavor);

            bankName = SanitizeAsciiToken(bankName, "bank");
            toneName = SanitizeAsciiToken(toneName, bankName);

            byte[] tone = BuildToneChunkData(toneName, checked((uint)encodedSubfile.Length), flavor);
            WriteToneBank(outputPath, bankName, tone, encodedSubfile, flavor);
        }

        public static void WriteMultiToneBank(IReadOnlyList<Nus3BankEncodeStream> streams, string outputPath, string bankName, Nus3BankBuildFlavor flavor = Nus3BankBuildFlavor.Nus3Bank)
        {
            if (streams.Count == 0)
                throw new InvalidDataException("The NUS3BANK stream list is empty.");

            FormMain.DebugInfo($"[Nus3Bank] Write multi tone started. streams={streams.Count}, output={outputPath}, flavor={flavor}");
            bankName = SanitizeAsciiToken(bankName, "bank");

            var toneEntries = new List<Nus3TonePackEntry>(streams.Count);
            using var packStream = new MemoryStream();

            for (int i = 0; i < streams.Count; i++)
            {
                Nus3BankEncodeStream stream = streams[i];
                if (stream.Data.Length == 0)
                    throw new InvalidDataException("A NUS3BANK stream is empty.");

                byte[] subfile = stream.LoopPoints.HasValue
                    ? EnsureRiffLoopChunk(stream.Data, stream.LoopPoints.Value)
                    : stream.Data;

                ValidateNub2StreamCodec(subfile, flavor);

                if (packStream.Position > int.MaxValue || subfile.LongLength > uint.MaxValue)
                    throw new InvalidDataException("The NUS3BANK is too large.");

                int streamRelativeOffset = Align(checked((int)packStream.Position), 0x10);
                while (packStream.Position < streamRelativeOffset)
                    packStream.WriteByte(0);

                string toneName = SanitizeAsciiToken(stream.Name, $"tone_{i:D4}");
                toneEntries.Add(new Nus3TonePackEntry(
                    toneName,
                    checked((uint)streamRelativeOffset),
                    checked((uint)subfile.Length)));

                packStream.Write(subfile, 0, subfile.Length);
            }

            if (packStream.Length > uint.MaxValue)
                throw new InvalidDataException("The NUS3BANK is too large.");

            byte[] tone = BuildToneChunkData(toneEntries, flavor);
            WriteToneBank(outputPath, bankName, tone, packStream.ToArray(), flavor);
            FormMain.DebugInfo($"[Nus3Bank] Write multi tone completed. streams={streams.Count}, output={outputPath}");
        }

        private static void WriteToneBank(string outputPath, string bankName, byte[] tone, byte[] pack, Nus3BankBuildFlavor flavor)
        {
            byte[] prop = BuildPropChunkData(flavor);
            byte[] binf = BuildBinfChunkData(bankName);
            byte[] grp = BuildEmptyTableChunkData();
            byte[] dton = BuildEmptyTableChunkData();

            var preJunkChunks = new List<(string Id, byte[] Data)>
            {
                ("PROP", prop),
                ("BINF", binf),
                ("GRP ", grp),
                ("DTON", dton),
                ("TONE", tone),
            };

            const int chunkCount = 7;
            int tocSize = 4 + chunkCount * 8;
            int currentOffset = 0x14 + tocSize;
            foreach (var chunk in preJunkChunks)
                currentOffset += 8 + chunk.Data.Length;

            int junkSize = Align(currentOffset + 16, 0x10) - (currentOffset + 16);
            byte[] junk = new byte[junkSize];

            var chunks = new List<(string Id, byte[] Data)>(preJunkChunks)
            {
                ("JUNK", junk),
                ("PACK", pack),
            };

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
            using var ms = new MemoryStream();
            WriteAscii(ms, "NUS3");
            WriteU32LE(ms, 0);
            WriteAscii(ms, "BANK");
            WriteAscii(ms, "TOC ");
            WriteU32LE(ms, checked((uint)tocSize));
            WriteU32LE(ms, chunkCount);

            foreach (var chunk in chunks)
            {
                WriteAscii(ms, chunk.Id);
                WriteU32LE(ms, checked((uint)chunk.Data.Length));
            }

            foreach (var chunk in chunks)
            {
                WriteAscii(ms, chunk.Id);
                WriteU32LE(ms, checked((uint)chunk.Data.Length));
                ms.Write(chunk.Data, 0, chunk.Data.Length);
            }

            if (ms.Length > uint.MaxValue)
                throw new InvalidDataException("The NUS3BANK is too large.");

            ms.Position = 0x04;
            WriteU32LE(ms, checked((uint)(ms.Length - 8)));
            File.WriteAllBytes(outputPath, ms.ToArray());
            FormMain.DebugInfo($"[Nus3Bank] Bank file written. output={outputPath}, bytes={ms.Length}, flavor={flavor}");
        }

        private static List<Nus3Tone> ParseTones(byte[] data, Nus3Chunk toneChunk, Nus3Chunk packChunk)
        {
            if (toneChunk.Size < 4)
                throw new InvalidDataException("The TONE chunk is too small.");

            uint count = ReadU32LE(data, toneChunk.DataOffset);
            int tableEnd = checked(toneChunk.DataOffset + 4 + (int)count * 8);
            if (count > 100000 || tableEnd > toneChunk.DataOffset + toneChunk.Size)
                throw new InvalidDataException("The TONE table is invalid.");

            var tones = new List<Nus3Tone>();
            for (int i = 0; i < count; i++)
            {
                int tableEntry = toneChunk.DataOffset + 4 + i * 8;
                int recordRel = checked((int)ReadU32LE(data, tableEntry));
                int recordSize = checked((int)ReadU32LE(data, tableEntry + 4));
                int recordOffset = checked(toneChunk.DataOffset + recordRel);
                int recordEnd = checked(recordOffset + recordSize);
                int toneEnd = toneChunk.DataOffset + toneChunk.Size;

                if (recordOffset < toneChunk.DataOffset || recordEnd > toneEnd)
                    throw new InvalidDataException("A TONE record is outside the TONE chunk.");

                if (!TryParseToneRecord(data, i, recordOffset, recordEnd, recordSize, packChunk, out Nus3Tone? tone) ||
                    tone is null)
                {
                    continue;
                }

                tones.Add(tone);
            }

            return tones;
        }

        private static bool TryParseToneRecord(
            byte[] data,
            int index,
            int recordOffset,
            int recordEnd,
            int recordSize,
            Nus3Chunk packChunk,
            out Nus3Tone? tone)
        {
            tone = null;

            // Some banks contain non-audio placeholder TONE entries, typically 12 bytes.
            if (recordSize < 16 || recordOffset + 8 > recordEnd)
                return false;

            int p = recordOffset;
            byte flags2 = data[p + 7];
            p += 8;

            if ((flags2 & 0x80) != 0)
                p += 4;

            if (p >= recordEnd)
                return false;

            int nameLen = data[p];
            if (nameLen <= 0 || p + 1 + nameLen > recordEnd)
                return false;

            string name = ReadString(data, p + 1, nameLen - 1);
            p += Align(1 + nameLen, 4);

            if (!TryReadToneStreamReference(data, p, recordOffset, recordEnd, packChunk,
                out int streamReferenceOffset, out int streamRel, out int streamSize, out int subfileOffset, out Nus3SubfileCodec codec))
            {
                return false;
            }

            tone = new Nus3Tone
            {
                Index = index,
                Name = name,
                RecordOffset = recordOffset,
                RecordSize = recordSize,
                StreamReferenceOffset = streamReferenceOffset,
                StreamRelativeOffset = streamRel,
                StreamSize = streamSize,
                SubfileOffset = subfileOffset,
                Codec = codec,
            };
            return true;
        }

        private static bool TryReadToneStreamReference(
            byte[] data,
            int preferredOffset,
            int recordOffset,
            int recordEnd,
            Nus3Chunk packChunk,
            out int streamReferenceOffset,
            out int streamRel,
            out int streamSize,
            out int subfileOffset,
            out Nus3SubfileCodec codec)
        {
            if (TryReadToneStreamReferenceAt(data, preferredOffset, recordEnd, packChunk, requireKnownSubfile: false,
                out streamReferenceOffset, out streamRel, out streamSize, out subfileOffset, out codec))
            {
                return true;
            }

            for (int p = Align(recordOffset, 4); p + 16 <= recordEnd; p += 4)
            {
                if (p == preferredOffset)
                    continue;

                if (TryReadToneStreamReferenceAt(data, p, recordEnd, packChunk, requireKnownSubfile: true,
                    out streamReferenceOffset, out streamRel, out streamSize, out subfileOffset, out codec))
                {
                    return true;
                }
            }

            for (int p = Align(recordOffset, 4); p + 16 <= recordEnd; p += 4)
            {
                if (p == preferredOffset)
                    continue;

                if (TryReadToneStreamReferenceAt(data, p, recordEnd, packChunk, requireKnownSubfile: false,
                    out streamReferenceOffset, out streamRel, out streamSize, out subfileOffset, out codec))
                {
                    return true;
                }
            }

            streamReferenceOffset = 0;
            streamRel = 0;
            streamSize = 0;
            subfileOffset = 0;
            codec = Nus3SubfileCodec.Unknown;
            return false;
        }

        private static bool TryReadToneStreamReferenceAt(
            byte[] data,
            int offset,
            int recordEnd,
            Nus3Chunk packChunk,
            bool requireKnownSubfile,
            out int streamReferenceOffset,
            out int streamRel,
            out int streamSize,
            out int subfileOffset,
            out Nus3SubfileCodec codec)
        {
            streamReferenceOffset = 0;
            streamRel = 0;
            streamSize = 0;
            subfileOffset = 0;
            codec = Nus3SubfileCodec.Unknown;

            if (offset < 0 || offset + 16 > recordEnd)
                return false;

            uint link0 = ReadU32LE(data, offset);
            uint link1 = ReadU32LE(data, offset + 4);
            if (link0 != 0 || link1 != 8)
                return false;

            uint rel = ReadU32LE(data, offset + 8);
            uint size = ReadU32LE(data, offset + 12);
            ulong streamEnd = (ulong)rel + size;
            if (size == 0 || rel > int.MaxValue || size > int.MaxValue || streamEnd > (uint)packChunk.Size)
                return false;

            int candidateOffset = checked(packChunk.DataOffset + (int)rel);
            var candidateCodec = DetectSubfileCodec(data, candidateOffset, (int)size);
            if (requireKnownSubfile && candidateCodec == Nus3SubfileCodec.Unknown)
                return false;

            streamReferenceOffset = offset;
            streamRel = (int)rel;
            streamSize = (int)size;
            subfileOffset = candidateOffset;
            codec = candidateCodec;
            return true;
        }

        private static Nus3SubfileCodec DetectSubfileCodec(byte[] data, int offset, int size)
        {
            if (size < 4 || offset < 0 || offset + size > data.Length)
                return Nus3SubfileCodec.Unknown;

            string signature = ReadAscii(data, offset, 4);
            return signature switch
            {
                "RIFF" => DetectRiffCodec(data, offset, size),
                "BNSF" => Nus3SubfileCodec.Bnsf,
                "IVAG" => Nus3SubfileCodec.Ivag,
                _ => Nus3SubfileCodec.Unknown,
            };
        }

        private static Nus3SubfileCodec DetectRiffCodec(byte[] data, int offset, int size)
        {
            if (size < 12 || ReadAscii(data, offset + 8, 4) != "WAVE")
                return Nus3SubfileCodec.RiffUnknown;

            int riffEnd = offset + size;
            int p = offset + 12;
            while (p + 8 <= riffEnd)
            {
                string id = ReadAscii(data, p, 4);
                int payloadSize = checked((int)ReadU32LE(data, p + 4));
                int payloadOffset = p + 8;
                int next = payloadOffset + payloadSize + (payloadSize & 1);
                if (payloadSize < 0 || payloadOffset + payloadSize > riffEnd || next < p)
                    break;

                if (id == "fmt ")
                    return DetectWaveFormat(data, payloadOffset, payloadSize);

                p = next;
            }

            return Nus3SubfileCodec.RiffUnknown;
        }

        public static bool TryReadRiffWaveSampleRate(string path, out int sampleRate)
        {
            sampleRate = 0;
            if (!File.Exists(path))
                return false;

            try
            {
                return TryReadRiffWaveSampleRate(File.ReadAllBytes(path), out sampleRate);
            }
            catch
            {
                sampleRate = 0;
                return false;
            }
        }

        private static bool TryReadRiffWaveSampleRate(byte[] data, out int sampleRate)
        {
            return TryReadRiffWaveSampleRate(data, 0, data.Length, out sampleRate);
        }

        private static bool TryReadRiffWaveSampleRate(byte[] data, int offset, int length, out int sampleRate)
        {
            sampleRate = 0;
            if (!TryFindRiffChunk(data, offset, length, "fmt ", out _, out int fmtDataOffset, out int fmtSize, out _) || fmtSize < 8)
                return false;

            uint rate = ReadU32LE(data, fmtDataOffset + 4);
            if (rate == 0 || rate > int.MaxValue)
                return false;

            sampleRate = (int)rate;
            return true;
        }

        private static byte[] EnsureRiffLoopChunk(byte[] encodedSubfile, Nus3RiffLoopPoints loopPoints)
        {
            if (!loopPoints.TryGetRiffLoop(out uint loopStart, out uint loopEnd) ||
                !TryReadRiffWaveSampleRate(encodedSubfile, out int sampleRate))
            {
                return encodedSubfile;
            }

            if (TryFindRiffChunk(encodedSubfile, "smpl", out _, out int smplDataOffset, out int smplSize, out _) && smplSize >= 60)
            {
                byte[] copy = encodedSubfile.ToArray();
                if (ReadU32LE(copy, smplDataOffset + 28) == 0)
                    WriteU32LE(copy, smplDataOffset + 28, 1);

                WriteU32LE(copy, smplDataOffset + 44, loopStart);
                WriteU32LE(copy, smplDataOffset + 48, loopEnd);
                return copy;
            }

            byte[] smplPayload = BuildSmplChunkData(sampleRate, loopStart, loopEnd);
            return RebuildRiffWithSmpl(encodedSubfile, smplPayload);
        }

        private static byte[] BuildSmplChunkData(int sampleRate, uint loopStart, uint loopEnd)
        {
            using var ms = new MemoryStream();
            WriteU32LE(ms, 0);
            WriteU32LE(ms, 0);
            WriteU32LE(ms, sampleRate > 0 ? checked((uint)Math.Round(1_000_000_000.0 / sampleRate)) : 0);
            WriteU32LE(ms, 60);
            WriteU32LE(ms, 0);
            WriteU32LE(ms, 0);
            WriteU32LE(ms, 0);
            WriteU32LE(ms, 1);
            WriteU32LE(ms, 0);
            WriteU32LE(ms, 0);
            WriteU32LE(ms, 0);
            WriteU32LE(ms, loopStart);
            WriteU32LE(ms, loopEnd);
            WriteU32LE(ms, 0);
            WriteU32LE(ms, 0);
            return ms.ToArray();
        }

        private static byte[] RebuildRiffWithSmpl(byte[] source, byte[] smplPayload)
        {
            if (!IsRiffWave(source, out int riffEnd))
                return source;

            using var ms = new MemoryStream(source.Length + smplPayload.Length + 8);
            WriteAscii(ms, "RIFF");
            WriteU32LE(ms, 0);
            WriteAscii(ms, "WAVE");

            bool inserted = false;
            int p = 12;
            while (p + 8 <= riffEnd)
            {
                if (!TryReadRiffChunkHeader(source, p, riffEnd, out string id, out int payloadSize, out int next))
                    break;

                if (id == "smpl")
                {
                    if (!inserted)
                    {
                        WriteRiffChunk(ms, "smpl", smplPayload);
                        inserted = true;
                    }

                    p = next;
                    continue;
                }

                if (!inserted && id == "data")
                {
                    WriteRiffChunk(ms, "smpl", smplPayload);
                    inserted = true;
                }

                ms.Write(source.AsSpan(p, next - p));
                p = next;
            }

            if (!inserted)
                WriteRiffChunk(ms, "smpl", smplPayload);

            if (riffEnd < source.Length)
                ms.Write(source.AsSpan(riffEnd));

            if (ms.Length > uint.MaxValue + 8L)
                return source;

            byte[] result = ms.ToArray();
            WriteU32LE(result, 4, checked((uint)(result.Length - 8)));
            return result;
        }

        private static (int Start, int End) GetLoopReadAdjustments(Nus3SubfileCodec codec, int sampleRate, bool isAtrac3Ps3)
        {
            return codec switch
            {
                Nus3SubfileCodec.Atrac3 => isAtrac3Ps3 ? (3271, 3270) : (2459, 2458),
                Nus3SubfileCodec.Atrac9 => sampleRate switch
                {
                    12000 => (64, 63),
                    24000 => (128, 127),
                    _ => (256, 255),
                },
                _ => (0, 0),
            };
        }

        private static bool TryFindRiffChunk(byte[] data, string chunkId, out int chunkOffset, out int dataOffset, out int payloadSize, out int nextOffset)
        {
            return TryFindRiffChunk(data, 0, data.Length, chunkId, out chunkOffset, out dataOffset, out payloadSize, out nextOffset);
        }

        private static bool TryFindRiffChunk(byte[] data, int offset, int length, string chunkId, out int chunkOffset, out int dataOffset, out int payloadSize, out int nextOffset)
        {
            chunkOffset = 0;
            dataOffset = 0;
            payloadSize = 0;
            nextOffset = 0;

            if (!IsRiffWave(data, offset, length, out int riffEnd))
                return false;

            int p = offset + 12;
            while (p + 8 <= riffEnd)
            {
                if (!TryReadRiffChunkHeader(data, p, riffEnd, out string id, out int chunkSize, out int next))
                    return false;

                if (id == chunkId)
                {
                    chunkOffset = p;
                    dataOffset = p + 8;
                    payloadSize = chunkSize;
                    nextOffset = next;
                    return true;
                }

                p = next;
            }

            return false;
        }

        private static bool TryReadRiffChunkHeader(byte[] data, int offset, int riffEnd, out string id, out int payloadSize, out int nextOffset)
        {
            id = string.Empty;
            payloadSize = 0;
            nextOffset = 0;

            if (offset + 8 > riffEnd)
                return false;

            id = ReadAscii(data, offset, 4);
            uint size = ReadU32LE(data, offset + 4);
            if (size > int.MaxValue)
                return false;

            payloadSize = (int)size;
            long next = (long)offset + 8 + payloadSize + (payloadSize & 1);
            if (next < offset || next > riffEnd)
                return false;

            nextOffset = (int)next;
            return true;
        }

        private static bool IsRiffWave(byte[] data, out int riffEnd)
        {
            return IsRiffWave(data, 0, data.Length, out riffEnd);
        }

        private static bool IsRiffWave(byte[] data, int offset, int length, out int riffEnd)
        {
            riffEnd = 0;
            if (offset < 0 || length < 12 || offset > data.Length || length > data.Length - offset ||
                ReadAscii(data, offset, 4) != "RIFF" ||
                ReadAscii(data, offset + 8, 4) != "WAVE")
                return false;

            long containerEnd = (long)offset + length;
            long declaredEnd = (long)offset + ReadU32LE(data, offset + 4) + 8;
            if (declaredEnd < offset + 12L || declaredEnd > containerEnd)
                riffEnd = checked((int)containerEnd);
            else
                riffEnd = (int)declaredEnd;

            return true;
        }

        private static void WriteRiffChunk(Stream stream, string id, byte[] payload)
        {
            WriteAscii(stream, id);
            WriteU32LE(stream, checked((uint)payload.Length));
            stream.Write(payload, 0, payload.Length);
            if ((payload.Length & 1) != 0)
                stream.WriteByte(0);
        }

        private static Nus3SubfileCodec DetectWaveFormat(byte[] data, int offset, int size)
        {
            if (size < 16)
                return Nus3SubfileCodec.RiffUnknown;

            ushort formatTag = ReadU16LE(data, offset);
            if (formatTag == 0x0001)
                return Nus3SubfileCodec.PcmWave;

            if (formatTag == 0x0270 || formatTag == 0x0271)
                return Nus3SubfileCodec.Atrac3;

            if (formatTag == 0xFFFE && size >= 40)
            {
                ReadOnlySpan<byte> subtype = new(data, offset + 24, 16);
                if (IsAtrac9WaveGuid(subtype))
                    return Nus3SubfileCodec.Atrac9;

                if (subtype.SequenceEqual(Atrac3PlusWaveGuid))
                    return Nus3SubfileCodec.Atrac3;

                if (TryReadWaveExtensibleFormatTag(subtype, out ushort subFormatTag))
                {
                    if (subFormatTag == 0x0001)
                        return Nus3SubfileCodec.PcmWave;

                    if (subFormatTag == 0x0270 || subFormatTag == 0x0271)
                        return Nus3SubfileCodec.Atrac3;
                }
            }

            return Nus3SubfileCodec.RiffUnknown;
        }

        private static void ValidateNub2StreamCodec(byte[] subfile, Nus3BankBuildFlavor flavor)
        {
            if (flavor != Nus3BankBuildFlavor.Nub2)
                return;

            if (DetectSubfileCodec(subfile, 0, subfile.Length) != Nus3SubfileCodec.Atrac3)
                throw new InvalidDataException("NUB2 output supports ATRAC3/ATRAC3+ RIFF streams only.");
        }

        private static bool IsAtrac9WaveGuid(ReadOnlySpan<byte> subtype)
        {
            if (subtype.SequenceEqual(Atrac9WaveGuid))
                return true;

            ReadOnlySpan<byte> alternate =
            [
                0x47, 0xE1, 0x42, 0xD2, 0x36, 0xBA, 0x4D, 0x8D,
                0x88, 0xFC, 0x61, 0x65, 0x4F, 0x8C, 0x83, 0x6C
            ];
            return subtype.SequenceEqual(alternate);
        }

        private static bool TryReadWaveExtensibleFormatTag(ReadOnlySpan<byte> subtype, out ushort formatTag)
        {
            formatTag = 0;
            if (subtype.Length < 16)
                return false;

            ReadOnlySpan<byte> standardTail =
            [
                0x00, 0x00, 0x00, 0x00, 0x10, 0x00, 0x80,
                0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71
            ];

            if (!subtype[2..].SequenceEqual(standardTail))
                return false;

            formatTag = BinaryPrimitives.ReadUInt16LittleEndian(subtype[..2]);
            return true;
        }

        private static byte[] BuildPropChunkData(Nus3BankBuildFlavor flavor)
        {
            if (flavor == Nus3BankBuildFlavor.Nub2)
            {
                byte[] nub2Data = new byte[0x24];
                WriteU32LE(nub2Data, 0x00, 0);
                WriteU32LE(nub2Data, 0x04, 0x71);
                WriteU16LE(nub2Data, 0x08, 0);
                WriteU16LE(nub2Data, 0x0A, 3);
                WriteSizedString(nub2Data, 0x0C, "DefaultProject", nub2Data.Length - 0x0C);
                return nub2Data;
            }

            byte[] data = new byte[0x3C];
            WriteU32LE(data, 0x00, 0);
            WriteU32LE(data, 0x04, 0xF1);
            WriteU16LE(data, 0x08, 1);
            WriteU16LE(data, 0x0A, 3);
            WriteSizedString(data, 0x0C, "DefaultProject", 0x20 - 0x0C);
            WriteU32LE(data, 0x20, 0);
            WriteSizedString(data, 0x24, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), data.Length - 0x24);
            return data;
        }

        private static byte[] BuildBinfChunkData(string bankName)
        {
            using var ms = new MemoryStream();
            WriteU32LE(ms, 0);
            WriteU32LE(ms, 3);
            WriteSizedString(ms, bankName);
            Pad4(ms);
            WriteU32LE(ms, 0);
            return ms.ToArray();
        }

        private static byte[] BuildEmptyTableChunkData()
        {
            using var ms = new MemoryStream();
            WriteU32LE(ms, 0);
            return ms.ToArray();
        }

        private readonly struct Nus3TonePackEntry
        {
            public Nus3TonePackEntry(string name, uint streamRelativeOffset, uint streamSize)
            {
                Name = name;
                StreamRelativeOffset = streamRelativeOffset;
                StreamSize = streamSize;
            }

            public string Name { get; }
            public uint StreamRelativeOffset { get; }
            public uint StreamSize { get; }
        }

        private static byte[] BuildToneChunkData(string toneName, uint streamSize, Nus3BankBuildFlavor flavor)
        {
            return BuildToneChunkData([new Nus3TonePackEntry(toneName, 0, streamSize)], flavor);
        }

        private static byte[] BuildToneChunkData(IReadOnlyList<Nus3TonePackEntry> entries, Nus3BankBuildFlavor flavor)
        {
            if (entries.Count == 0)
                throw new InvalidDataException("The TONE entry list is empty.");

            var records = new List<byte[]>(entries.Count);
            foreach (Nus3TonePackEntry entry in entries)
                records.Add(BuildToneRecordData(entry.Name, entry.StreamRelativeOffset, entry.StreamSize, flavor));

            using var ms = new MemoryStream();
            WriteU32LE(ms, checked((uint)entries.Count));

            uint recordOffset = checked(4u + (uint)entries.Count * 8u);
            foreach (byte[] record in records)
            {
                WriteU32LE(ms, recordOffset);
                WriteU32LE(ms, checked((uint)record.Length));
                recordOffset = checked(recordOffset + (uint)record.Length);
            }

            foreach (byte[] record in records)
                ms.Write(record, 0, record.Length);

            return ms.ToArray();
        }

        private static byte[] BuildToneRecordData(string toneName, uint streamRelativeOffset, uint streamSize, Nus3BankBuildFlavor flavor)
        {
            using var ms = new MemoryStream();
            WriteU32LE(ms, 0);
            WriteS16LE(ms, -1);
            ms.WriteByte(0x27);
            if (flavor == Nus3BankBuildFlavor.Nub2)
                ms.WriteByte(0x0C);
            else
            {
                ms.WriteByte(0x84);
                WriteU32LE(ms, 0);
            }

            WriteSizedString(ms, toneName);
            Pad4(ms);
            WriteU32LE(ms, 0);
            WriteU32LE(ms, 8);
            WriteU32LE(ms, streamRelativeOffset);
            WriteU32LE(ms, streamSize);
            return ms.ToArray();
        }

        private static string SanitizeFileName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var invalid = Path.GetInvalidFileNameChars();
            var chars = value.Select(c => invalid.Contains(c) ? '_' : c).ToArray();
            return new string(chars).Trim();
        }

        private static string SanitizeAsciiToken(string value, string fallback)
        {
            if (string.IsNullOrWhiteSpace(value))
                value = fallback;

            var sb = new StringBuilder(value.Length);
            foreach (char c in value)
            {
                if (c is >= 'A' and <= 'Z' || c is >= 'a' and <= 'z' || c is >= '0' and <= '9' || c is '_' or '-' or '.')
                    sb.Append(c);
                else
                    sb.Append('_');
            }

            string result = sb.ToString().Trim('_');
            return result.Length == 0 ? fallback : result;
        }

        private static string ReadAscii(byte[] data, int offset, int length)
        {
            return Encoding.ASCII.GetString(data, offset, length);
        }

        private static string ReadString(byte[] data, int offset, int length)
        {
            if (length <= 0) return string.Empty;
            int actualLength = length;
            while (actualLength > 0 && data[offset + actualLength - 1] == 0)
                actualLength--;

            return Encoding.UTF8.GetString(data, offset, actualLength);
        }

        private static ushort ReadU16LE(byte[] data, int offset)
        {
            return BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(offset, 2));
        }

        private static uint ReadU32LE(byte[] data, int offset)
        {
            return BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(offset, 4));
        }

        private static void WriteU16LE(byte[] data, int offset, ushort value)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(offset, 2), value);
        }

        private static void WriteU32LE(byte[] data, int offset, uint value)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(offset, 4), value);
        }

        private static void WriteAscii(Stream stream, string value)
        {
            string id = value.PadRight(4, ' ')[..4];
            byte[] bytes = Encoding.ASCII.GetBytes(id);
            stream.Write(bytes, 0, bytes.Length);
        }

        private static void WriteU32LE(Stream stream, int value)
        {
            WriteU32LE(stream, checked((uint)value));
        }

        private static void WriteU32LE(Stream stream, uint value)
        {
            Span<byte> buffer = stackalloc byte[4];
            BinaryPrimitives.WriteUInt32LittleEndian(buffer, value);
            stream.Write(buffer);
        }

        private static void WriteS16LE(Stream stream, short value)
        {
            Span<byte> buffer = stackalloc byte[2];
            BinaryPrimitives.WriteInt16LittleEndian(buffer, value);
            stream.Write(buffer);
        }

        private static void WriteSizedString(Stream stream, string value)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(SanitizeAsciiToken(value, "item"));
            if (bytes.Length > 254)
                bytes = bytes.Take(254).ToArray();

            stream.WriteByte((byte)(bytes.Length + 1));
            stream.Write(bytes, 0, bytes.Length);
            stream.WriteByte(0);
        }

        private static void WriteSizedString(byte[] data, int offset, string value, int capacity)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(SanitizeAsciiToken(value, "item"));
            int maxTextLength = Math.Max(0, capacity - 2);
            if (bytes.Length > maxTextLength)
                bytes = bytes.Take(maxTextLength).ToArray();

            data[offset] = (byte)(bytes.Length + 1);
            Array.Copy(bytes, 0, data, offset + 1, bytes.Length);
            data[offset + 1 + bytes.Length] = 0;
        }

        private static void Pad4(Stream stream)
        {
            while ((stream.Position & 3) != 0)
                stream.WriteByte(0);
        }

        private static int Align(int value, int alignment)
        {
            int mask = alignment - 1;
            return (value + mask) & ~mask;
        }
    }
}
