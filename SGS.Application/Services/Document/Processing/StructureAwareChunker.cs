using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

using SGS.Application.Models.DocumentProcessing;

namespace SGS.Application.Services.Document.Processing;

public sealed record ChunkingOptions
{
    // These are estimated tokens, not tokens counted by a particular LLM tokenizer.
    public int MaxEstimatedTokens { get; init; } = 512;
    public int OverlapEstimatedTokens { get; init; } = 64;
    public bool SplitAtSectionBoundaries { get; init; } = true;

    public void Validate()
    {
        if (MaxEstimatedTokens < 8) throw new ArgumentOutOfRangeException(nameof(MaxEstimatedTokens));
        if (OverlapEstimatedTokens < 0 || OverlapEstimatedTokens >= MaxEstimatedTokens)
            throw new ArgumentOutOfRangeException(nameof(OverlapEstimatedTokens));
    }
}

public interface ITextTokenEstimator
{
    int Estimate(string text);
}

public sealed class Utf8TokenEstimator : ITextTokenEstimator
{
    // Cheap cross-language approximation for experimenting. It is NOT a model
    // tokenizer; a production LLM budget must be checked with its real tokenizer.
    public int Estimate(string text) =>
        string.IsNullOrEmpty(text) ? 0 : Math.Max(1, (Encoding.UTF8.GetByteCount(text) + 2) / 3);
}

public interface IDocumentChunker
{
    IReadOnlyList<DocumentChunk> Chunk(ExtractedDocument document,
        ChunkingOptions? options = null);
}

public sealed class StructureAwareChunker(ITextTokenEstimator? estimator = null) : IDocumentChunker
{
    private static readonly Regex WordRegex = new(@"\S+", RegexOptions.Compiled);
    private readonly ITextTokenEstimator _estimator = estimator ?? new Utf8TokenEstimator();

    private sealed record Word(DocumentBlock Block, int Start, int End, int Weight);

    public IReadOnlyList<DocumentChunk> Chunk(ExtractedDocument document,
        ChunkingOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(document);
        System.Text.Json.JsonSerializer.Serialize(document);
        options ??= new ChunkingOptions();
        options.Validate();
        var words = Tokenize(document.Blocks, options.MaxEstimatedTokens);
        var chunks = new List<DocumentChunk>();
        var sourceMap = document.Sources.ToDictionary(s => s.SourceId);
        int index = 0;
        while (index < words.Count)
        {
            var start = index;
            var section = words[index].Block.SectionPath;
            int budget = 0;
            while (index < words.Count)
            {
                var next = words[index];
                if (index > start && options.SplitAtSectionBoundaries &&
                    !string.Equals(section, next.Block.SectionPath, StringComparison.Ordinal))
                    break;
                if (budget + next.Weight > options.MaxEstimatedTokens) break;
                budget += next.Weight;
                index++;
            }
            if (index == start)
                throw new InvalidOperationException("A word exceeds the estimated chunk budget.");

            var slices = new List<ChunkSourceSpan>();
            var pieces = new List<string>();
            for (int j = start; j < index;)
            {
                var current = words[j];
                int finish = current.End;
                int k = j + 1;
                while (k < index && words[k].Block.BlockId == current.Block.BlockId)
                {
                    finish = words[k].End;
                    k++;
                }
                string piece = current.Block.Text[current.Start..finish];
                pieces.Add(piece);
                slices.Add(new ChunkSourceSpan(current.Block.BlockId, current.Block.SourceId,
                    sourceMap[current.Block.SourceId].PageNumber,
                    current.Block.StartOffset + current.Start, finish - current.Start));
                j = k;
            }
            string text = string.Join("\n", pieces);
            string identity = $"{document.DocumentId}|{chunks.Count}|" +
                string.Join(";", slices.Select(s => $"{s.SourceId}:{s.StartOffset}:{s.Length}"));
            string chunkId = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(identity)))
                .ToLowerInvariant()[..16];
            chunks.Add(new DocumentChunk($"c-{chunkId}", document.DocumentId,
                text, section, budget, slices));

            if (index == words.Count) break;
            int nextStart = index;
            int overlap = 0;
            while (nextStart > start + 1 && options.OverlapEstimatedTokens > 0)
            {
                var prev = words[nextStart - 1];
                bool sameSection = !options.SplitAtSectionBoundaries ||
                    string.Equals(prev.Block.SectionPath, words[index].Block.SectionPath,
                        StringComparison.Ordinal);
                if (!sameSection || overlap + prev.Weight > options.OverlapEstimatedTokens) break;
                overlap += prev.Weight;
                nextStart--;
            }
            index = nextStart;
        }
        
        System.Text.Json.JsonSerializer.Serialize(chunks);
        return chunks;
    }

    private List<Word> Tokenize(IReadOnlyList<DocumentBlock> blocks, int limit)
    {
        var words = new List<Word>();
        foreach (var block in blocks)
        {
            foreach (Match match in WordRegex.Matches(block.Text))
            {
                string token = match.Value;
                int weight = Math.Max(1, _estimator.Estimate(token) + 1); // separators
                if (weight <= limit)
                {
                    words.Add(new Word(block, match.Index, match.Index + match.Length, weight));
                    continue;
                }
                // Oversized run with no spaces: split without breaking surrogate pairs.
                int index = match.Index;
                int end = match.Index + match.Length;
                while (index < end)
                {
                    int next = index;
                    while (next < end)
                    {
                        int increment = char.IsHighSurrogate(block.Text[next]) &&
                            next + 1 < end && char.IsLowSurrogate(block.Text[next + 1]) ? 2 : 1;
                        if (_estimator.Estimate(block.Text[index..(next + increment)]) + 1 > limit)
                            break;
                        next += increment;
                    }
                    if (next == index)
                        throw new InvalidOperationException("The token estimator cannot fit one character.");
                    words.Add(new Word(block, index, next,
                        _estimator.Estimate(block.Text[index..next]) + 1));
                    index = next;
                }
            }
        }
        return words;
    }
}
