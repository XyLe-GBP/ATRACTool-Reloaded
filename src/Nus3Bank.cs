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
        public string? DataPath { get; init; }
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
        public IEnumerable<Nus3Tone> ExtractableTones => _tones.Where(t => t.StreamSize > 0);
        public IEnumerable<Nus3Tone> DecodableWaveTones => _tones.Where(t => t.Codec is Nus3SubfileCodec.Atrac3 or Nus3SubfileCodec.Atrac9 or Nus3SubfileCodec.PcmWave or Nus3SubfileCodec.Ivag);
        public int ExtractableToneCount => _tones.Count(t => t.StreamSize > 0);
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
            CreateParentDirectory(outputPath);
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

            bool resolvedAtrac3Ps3 = tone.Codec == Nus3SubfileCodec.Atrac3
                ? sampleRate switch
                {
                    44100 => false,
                    48000 => true,
                    _ => isAtrac3Ps3,
                }
                : isAtrac3Ps3;
            (int startAdjustment, int endAdjustment) = GetLoopReadAdjustments(tone.Codec, sampleRate, resolvedAtrac3Ps3);
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
            if (loopPoints.HasValue)
            {
                byte[] subfile = File.ReadAllBytes(encodedSubfilePath);
                WriteSingleToneBank(subfile, outputPath, bankName, toneName, loopPoints, flavor);
            }
            else
            {
                long length = GetExistingFileLength(encodedSubfilePath);
                ValidateSubfileSize(length);
                ValidateNub2StreamCodecFromFile(encodedSubfilePath, length, flavor);

                bankName = SanitizeAsciiToken(bankName, "bank");
                toneName = SanitizeAsciiToken(toneName, bankName);

                byte[] tone = BuildToneChunkData(toneName, checked((uint)length), flavor);
                using var prepared = new Nus3PreparedPackStream(
                    string.Empty,
                    0,
                    length,
                    data: null,
                    filePath: encodedSubfilePath,
                    deleteFileOnDispose: false);
                WriteToneBankFromPreparedStreams(outputPath, bankName, tone, [prepared], length, flavor);
            }
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

            var preparedStreams = new List<Nus3PreparedPackStream>(streams.Count);
            try
            {
                var toneEntries = new List<Nus3TonePackEntry>(streams.Count);
                long packPosition = 0;

                for (int i = 0; i < streams.Count; i++)
                {
                    Nus3PreparedPackStream prepared = PreparePackStream(streams[i], flavor);
                    string toneName = SanitizeAsciiToken(streams[i].Name, $"tone_{i:D4}");

                    long streamRelativeOffset = Align(packPosition, 0x10);
                    long streamEnd = checked(streamRelativeOffset + prepared.Length);
                    if (streamRelativeOffset > uint.MaxValue || prepared.Length > uint.MaxValue || streamEnd > uint.MaxValue)
                        throw new InvalidDataException("The NUS3BANK is too large.");

                    prepared.Name = toneName;
                    prepared.StreamRelativeOffset = streamRelativeOffset;
                    toneEntries.Add(new Nus3TonePackEntry(
                        toneName,
                        checked((uint)streamRelativeOffset),
                        checked((uint)prepared.Length)));

                    preparedStreams.Add(prepared);
                    packPosition = streamEnd;
                }

                if (packPosition > uint.MaxValue)
                    throw new InvalidDataException("The NUS3BANK is too large.");

                byte[] tone = BuildToneChunkData(toneEntries, flavor);
                WriteToneBankFromPreparedStreams(outputPath, bankName, tone, preparedStreams, packPosition, flavor);
                FormMain.DebugInfo($"[Nus3Bank] Write multi tone completed. streams={streams.Count}, output={outputPath}");
            }
            finally
            {
                foreach (Nus3PreparedPackStream prepared in preparedStreams)
                    prepared.Dispose();
            }
        }

        private static void WriteToneBank(string outputPath, string bankName, byte[] tone, byte[] pack, Nus3BankBuildFlavor flavor)
        {
            using var prepared = new Nus3PreparedPackStream(
                string.Empty,
                0,
                pack.LongLength,
                pack,
                filePath: null,
                deleteFileOnDispose: false);
            WriteToneBankFromPreparedStreams(outputPath, bankName, tone, [prepared], pack.LongLength, flavor);
        }

        private static void WriteToneBankFromPreparedStreams(
            string outputPath,
            string bankName,
            byte[] tone,
            IReadOnlyList<Nus3PreparedPackStream> packStreams,
            long packSize,
            Nus3BankBuildFlavor flavor)
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
            long currentOffset = 0x14 + tocSize;
            foreach (var chunk in preJunkChunks)
                currentOffset += 8 + chunk.Data.Length;

            int junkSize = checked((int)(Align(currentOffset + 16, 0x10) - (currentOffset + 16)));
            byte[] junk = new byte[junkSize];

            CreateParentDirectory(outputPath);
            using var fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, FileOptions.SequentialScan);
            WriteAscii(fs, "NUS3");
            WriteU32LE(fs, 0);
            WriteAscii(fs, "BANK");
            WriteAscii(fs, "TOC ");
            WriteU32LE(fs, checked((uint)tocSize));
            WriteU32LE(fs, chunkCount);

            foreach (var chunk in preJunkChunks)
            {
                WriteAscii(fs, chunk.Id);
                WriteU32LE(fs, checked((uint)chunk.Data.Length));
            }

            WriteAscii(fs, "JUNK");
            WriteU32LE(fs, checked((uint)junk.Length));
            WriteAscii(fs, "PACK");
            WriteU32LE(fs, checked((uint)packSize));

            foreach (var chunk in preJunkChunks)
            {
                WriteAscii(fs, chunk.Id);
                WriteU32LE(fs, checked((uint)chunk.Data.Length));
                fs.Write(chunk.Data, 0, chunk.Data.Length);
            }

            WriteAscii(fs, "JUNK");
            WriteU32LE(fs, checked((uint)junk.Length));
            fs.Write(junk, 0, junk.Length);

            WriteAscii(fs, "PACK");
            WriteU32LE(fs, checked((uint)packSize));
            long writtenPackBytes = 0;
            foreach (Nus3PreparedPackStream stream in packStreams)
            {
                WriteZeroPadding(fs, checked(stream.StreamRelativeOffset - writtenPackBytes));
                writtenPackBytes = stream.StreamRelativeOffset;
                stream.CopyTo(fs);
                writtenPackBytes = checked(writtenPackBytes + stream.Length);
            }

            if (writtenPackBytes != packSize)
                throw new InvalidDataException("The NUS3BANK PACK size does not match the stream data.");

            if (fs.Length > uint.MaxValue)
                throw new InvalidDataException("The NUS3BANK is too large.");

            fs.Position = 0x04;
            WriteU32LE(fs, checked((uint)(fs.Length - 8)));
            FormMain.DebugInfo($"[Nus3Bank] Bank file written. output={outputPath}, bytes={fs.Length}, flavor={flavor}");
        }

        private static Nus3PreparedPackStream PreparePackStream(Nus3BankEncodeStream stream, Nus3BankBuildFlavor flavor)
        {
            if (!string.IsNullOrWhiteSpace(stream.DataPath))
                return PreparePackStreamFromFile(stream.DataPath, stream.LoopPoints, flavor);

            if (stream.Data.Length == 0)
                throw new InvalidDataException("A NUS3BANK stream is empty.");

            byte[] subfile = stream.LoopPoints.HasValue
                ? EnsureRiffLoopChunk(stream.Data, stream.LoopPoints.Value)
                : stream.Data;

            ValidateSubfileSize(subfile.LongLength);
            ValidateNub2StreamCodec(subfile, flavor);
            return new Nus3PreparedPackStream(
                string.Empty,
                0,
                subfile.LongLength,
                subfile,
                filePath: null,
                deleteFileOnDispose: false);
        }

        private static Nus3PreparedPackStream PreparePackStreamFromFile(string path, Nus3RiffLoopPoints? loopPoints, Nus3BankBuildFlavor flavor)
        {
            long length = GetExistingFileLength(path);
            ValidateSubfileSize(length);

            if (!loopPoints.HasValue)
            {
                ValidateNub2StreamCodecFromFile(path, length, flavor);
                return new Nus3PreparedPackStream(
                    string.Empty,
                    0,
                    length,
                    data: null,
                    filePath: path,
                    deleteFileOnDispose: false);
            }

            byte[] original = File.ReadAllBytes(path);
            byte[] subfile = EnsureRiffLoopChunk(original, loopPoints.Value);
            ValidateSubfileSize(subfile.LongLength);
            ValidateNub2StreamCodec(subfile, flavor);

            string tempPath = Path.Combine(Path.GetTempPath(), "ATRACTool_Reloaded_" + Guid.NewGuid().ToString("N") + ".pack");
            File.WriteAllBytes(tempPath, subfile);
            return new Nus3PreparedPackStream(
                string.Empty,
                0,
                subfile.LongLength,
                data: null,
                filePath: tempPath,
                deleteFileOnDispose: true);
        }

        private static long GetExistingFileLength(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("The encoded subfile was not found.", path);

            long length = new FileInfo(path).Length;
            if (length == 0)
                throw new InvalidDataException("The encoded subfile is empty.");

            return length;
        }

        private static void ValidateSubfileSize(long length)
        {
            if (length <= 0)
                throw new InvalidDataException("A NUS3BANK stream is empty.");

            if (length > int.MaxValue)
                throw new InvalidDataException("The NUS3BANK stream is too large.");
        }

        private static void WriteZeroPadding(Stream stream, long count)
        {
            if (count < 0)
                throw new InvalidDataException("The NUS3BANK stream offsets are invalid.");

            Span<byte> zeroes = stackalloc byte[16];
            while (count > 0)
            {
                int write = (int)Math.Min(zeroes.Length, count);
                stream.Write(zeroes[..write]);
                count -= write;
            }
        }

        private sealed class Nus3PreparedPackStream : IDisposable
        {
            public Nus3PreparedPackStream(string name, long streamRelativeOffset, long length, byte[]? data, string? filePath, bool deleteFileOnDispose)
            {
                Name = name;
                StreamRelativeOffset = streamRelativeOffset;
                Length = length;
                Data = data;
                FilePath = filePath;
                DeleteFileOnDispose = deleteFileOnDispose;
            }

            public string Name { get; set; }
            public long StreamRelativeOffset { get; set; }
            public long Length { get; }
            private byte[]? Data { get; }
            private string? FilePath { get; }
            private bool DeleteFileOnDispose { get; }

            public void CopyTo(Stream destination)
            {
                if (Data is not null)
                {
                    if (Data.LongLength != Length)
                        throw new InvalidDataException("The NUS3BANK stream length changed before writing.");

                    destination.Write(Data, 0, Data.Length);
                    return;
                }

                if (string.IsNullOrWhiteSpace(FilePath))
                    throw new InvalidDataException("The NUS3BANK stream source is invalid.");

                using var source = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, 81920, FileOptions.SequentialScan);
                if (source.Length != Length)
                    throw new InvalidDataException("The NUS3BANK stream length changed before writing.");

                source.CopyTo(destination);
            }

            public void Dispose()
            {
                if (!DeleteFileOnDispose || string.IsNullOrWhiteSpace(FilePath))
                    return;

                try
                {
                    if (File.Exists(FilePath))
                        File.Delete(FilePath);
                }
                catch
                {
                    // Temporary pack files are best-effort cleanup.
                }
            }
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
                return TryReadRiffWaveSampleRateFromFile(path, out sampleRate);
            }
            catch
            {
                sampleRate = 0;
                return false;
            }
        }

        public static bool TryReadRiffLoopChunk(string path, out uint loopStart, out uint loopEnd)
        {
            loopStart = 0;
            loopEnd = 0;
            if (!File.Exists(path))
                return false;

            try
            {
                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, 81920, FileOptions.SequentialScan);
                if (!TryReadRiffHeader(fs, out long riffEnd))
                    return false;

                long p = 12;
                Span<byte> chunkHeader = stackalloc byte[8];
                Span<byte> smplData = stackalloc byte[52];
                while (p + 8 <= riffEnd)
                {
                    fs.Position = p;
                    if (!TryReadExactly(fs, chunkHeader))
                        return false;

                    uint payloadSize = BinaryPrimitives.ReadUInt32LittleEndian(chunkHeader.Slice(4, 4));
                    long next = p + 8 + payloadSize + (payloadSize & 1u);
                    if (next < p || next > riffEnd)
                        return false;

                    if (AsciiEquals(chunkHeader[..4], "smpl") && payloadSize >= 60)
                    {
                        if (!TryReadExactly(fs, smplData) ||
                            BinaryPrimitives.ReadUInt32LittleEndian(smplData.Slice(28, 4)) == 0)
                        {
                            return false;
                        }

                        uint rawStart = BinaryPrimitives.ReadUInt32LittleEndian(smplData.Slice(44, 4));
                        uint rawEnd = BinaryPrimitives.ReadUInt32LittleEndian(smplData.Slice(48, 4));
                        if (rawEnd <= rawStart)
                            return false;

                        loopStart = rawStart;
                        loopEnd = rawEnd;
                        return true;
                    }

                    p = next;
                }
            }
            catch
            {
                loopStart = 0;
                loopEnd = 0;
            }

            return false;
        }

        public static bool TryReadRiffFactSampleCount(string path, out uint sampleCount)
        {
            sampleCount = 0;
            if (!File.Exists(path))
                return false;

            try
            {
                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, 81920, FileOptions.SequentialScan);
                if (!TryReadRiffHeader(fs, out long riffEnd))
                    return false;

                long p = 12;
                Span<byte> chunkHeader = stackalloc byte[8];
                Span<byte> factData = stackalloc byte[4];
                while (p + 8 <= riffEnd)
                {
                    fs.Position = p;
                    if (!TryReadExactly(fs, chunkHeader))
                        return false;

                    uint payloadSize = BinaryPrimitives.ReadUInt32LittleEndian(chunkHeader.Slice(4, 4));
                    long next = p + 8 + payloadSize + (payloadSize & 1u);
                    if (next < p || next > riffEnd)
                        return false;

                    if (AsciiEquals(chunkHeader[..4], "fact") && payloadSize >= 4)
                    {
                        if (!TryReadExactly(fs, factData))
                            return false;

                        sampleCount = BinaryPrimitives.ReadUInt32LittleEndian(factData);
                        return sampleCount > 0;
                    }

                    p = next;
                }
            }
            catch
            {
                sampleCount = 0;
            }

            return false;
        }

        public static bool TryWriteRiffLoopChunk(string path, Nus3RiffLoopPoints loopPoints)
        {
            if (!File.Exists(path) || !loopPoints.TryGetRiffLoop(out uint loopStart, out uint loopEnd))
                return false;

            try
            {
                using var fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.Read, 81920, FileOptions.RandomAccess);
                if (!TryReadRiffHeader(fs, out long riffEnd))
                    return false;

                long p = 12;
                Span<byte> chunkHeader = stackalloc byte[8];
                Span<byte> loopCount = stackalloc byte[4];
                Span<byte> loopRange = stackalloc byte[8];
                while (p + 8 <= riffEnd)
                {
                    fs.Position = p;
                    if (!TryReadExactly(fs, chunkHeader))
                        return false;

                    uint payloadSize = BinaryPrimitives.ReadUInt32LittleEndian(chunkHeader.Slice(4, 4));
                    long next = p + 8 + payloadSize + (payloadSize & 1u);
                    if (next < p || next > riffEnd)
                        return false;

                    if (AsciiEquals(chunkHeader[..4], "smpl") && payloadSize >= 60)
                    {
                        BinaryPrimitives.WriteUInt32LittleEndian(loopCount, 1);
                        fs.Position = p + 8 + 28;
                        fs.Write(loopCount);

                        BinaryPrimitives.WriteUInt32LittleEndian(loopRange[..4], loopStart);
                        BinaryPrimitives.WriteUInt32LittleEndian(loopRange.Slice(4, 4), loopEnd);
                        fs.Position = p + 8 + 44;
                        fs.Write(loopRange);
                        fs.Flush();
                        return true;
                    }

                    p = next;
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        private static bool TryReadRiffWaveSampleRateFromFile(string path, out int sampleRate)
        {
            sampleRate = 0;
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, 81920, FileOptions.SequentialScan);
            if (!TryReadRiffHeader(fs, out long riffEnd))
                return false;

            Span<byte> fmtData = stackalloc byte[40];
            if (!TryReadRiffFmtChunk(fs, riffEnd, fmtData, out int fmtBytesRead) || fmtBytesRead < 8)
                return false;

            uint rate = BinaryPrimitives.ReadUInt32LittleEndian(fmtData.Slice(4, 4));
            if (rate == 0 || rate > int.MaxValue)
                return false;

            sampleRate = (int)rate;
            return true;
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
            if (offset < 0 || size < 0 || offset > data.Length || size > data.Length - offset)
                return Nus3SubfileCodec.RiffUnknown;

            return DetectWaveFormat(data.AsSpan(offset, size));
        }

        private static Nus3SubfileCodec DetectWaveFormat(ReadOnlySpan<byte> data)
        {
            int size = data.Length;
            if (size < 16)
                return Nus3SubfileCodec.RiffUnknown;

            ushort formatTag = BinaryPrimitives.ReadUInt16LittleEndian(data[..2]);
            if (formatTag == 0x0001)
                return Nus3SubfileCodec.PcmWave;

            if (formatTag == 0x0270 || formatTag == 0x0271)
                return Nus3SubfileCodec.Atrac3;

            if (formatTag == 0xFFFE && size >= 40)
            {
                ReadOnlySpan<byte> subtype = data.Slice(24, 16);
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

        private static void ValidateNub2StreamCodecFromFile(string path, long length, Nus3BankBuildFlavor flavor)
        {
            if (flavor != Nus3BankBuildFlavor.Nub2)
                return;

            if (DetectSubfileCodecFromFile(path, length) != Nus3SubfileCodec.Atrac3)
                throw new InvalidDataException("NUB2 output supports ATRAC3/ATRAC3+ RIFF streams only.");
        }

        private static Nus3SubfileCodec DetectSubfileCodecFromFile(string path, long length)
        {
            if (length < 4)
                return Nus3SubfileCodec.Unknown;

            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, 81920, FileOptions.SequentialScan);
            Span<byte> signature = stackalloc byte[4];
            if (!TryReadExactly(fs, signature))
                return Nus3SubfileCodec.Unknown;

            if (AsciiEquals(signature, "BNSF"))
                return Nus3SubfileCodec.Bnsf;

            if (AsciiEquals(signature, "IVAG"))
                return Nus3SubfileCodec.Ivag;

            if (!AsciiEquals(signature, "RIFF"))
                return Nus3SubfileCodec.Unknown;

            fs.Position = 0;
            if (!TryReadRiffHeader(fs, out long riffEnd))
                return Nus3SubfileCodec.RiffUnknown;

            Span<byte> fmtData = stackalloc byte[40];
            return TryReadRiffFmtChunk(fs, riffEnd, fmtData, out int fmtBytesRead)
                ? DetectWaveFormat(fmtData[..fmtBytesRead])
                : Nus3SubfileCodec.RiffUnknown;
        }

        private static bool TryReadRiffHeader(FileStream fs, out long riffEnd)
        {
            riffEnd = 0;
            if (fs.Length < 12)
                return false;

            Span<byte> header = stackalloc byte[12];
            fs.Position = 0;
            if (!TryReadExactly(fs, header) ||
                !AsciiEquals(header[..4], "RIFF") ||
                !AsciiEquals(header.Slice(8, 4), "WAVE"))
            {
                return false;
            }

            long declaredEnd = BinaryPrimitives.ReadUInt32LittleEndian(header.Slice(4, 4)) + 8L;
            riffEnd = declaredEnd < 12 || declaredEnd > fs.Length ? fs.Length : declaredEnd;
            return true;
        }

        private static void CreateParentDirectory(string path)
        {
            string? directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);
        }

        private static bool TryReadRiffFmtChunk(FileStream fs, long riffEnd, Span<byte> fmtData, out int fmtBytesRead)
        {
            fmtBytesRead = 0;
            long p = 12;
            Span<byte> chunkHeader = stackalloc byte[8];
            while (p + 8 <= riffEnd)
            {
                fs.Position = p;
                if (!TryReadExactly(fs, chunkHeader))
                    return false;

                uint payloadSize = BinaryPrimitives.ReadUInt32LittleEndian(chunkHeader.Slice(4, 4));
                long next = p + 8 + payloadSize + (payloadSize & 1u);
                if (next < p || next > riffEnd)
                    return false;

                if (AsciiEquals(chunkHeader[..4], "fmt "))
                {
                    fmtBytesRead = checked((int)Math.Min(payloadSize, (uint)fmtData.Length));
                    return fmtBytesRead == 0 || TryReadExactly(fs, fmtData[..fmtBytesRead]);
                }

                p = next;
            }

            return false;
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

        private static bool AsciiEquals(ReadOnlySpan<byte> data, string value)
        {
            if (data.Length != value.Length)
                return false;

            for (int i = 0; i < value.Length; i++)
            {
                if (data[i] != (byte)value[i])
                    return false;
            }

            return true;
        }

        private static bool TryReadExactly(Stream stream, Span<byte> buffer)
        {
            int offset = 0;
            while (offset < buffer.Length)
            {
                int read = stream.Read(buffer[offset..]);
                if (read == 0)
                    return false;

                offset += read;
            }

            return true;
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

        private static long Align(long value, int alignment)
        {
            long mask = alignment - 1;
            return (value + mask) & ~mask;
        }
    }
}
