using Jellyfin.Plugin.AutoTagger.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Entities;

namespace Jellyfin.Plugin.AutoTagger;

public static class TagRuleEngine
{
    // ── Seasonal ──────────────────────────────────────────────────────────────

    public static string? GetSeasonalTag(int? releaseMonth, PluginConfiguration config)
    {
        if (!config.EnableSeasonalTags || releaseMonth is null)
            return null;

        int m = releaseMonth.Value;

        if (ParseMonths(config.SpringMonths).Contains(m)) return config.SpringTag;
        if (ParseMonths(config.SummerMonths).Contains(m)) return config.SummerTag;
        if (ParseMonths(config.FallMonths).Contains(m))   return config.FallTag;
        if (ParseMonths(config.WinterMonths).Contains(m)) return config.WinterTag;

        return null;
    }

    // ── Genre ─────────────────────────────────────────────────────────────────

    public static IReadOnlyList<string> GetGenreTags(
        IEnumerable<string> itemGenres,
        PluginConfiguration config)
    {
        if (!config.EnableGenreTags)
            return Array.Empty<string>();

        var mapping = ParseKeyValueMappings(config.GenreTagMappings);
        var result  = new List<string>();

        foreach (var genre in itemGenres)
        {
            if (mapping.TryGetValue(genre.Trim(), out var tag))
                result.Add(tag);
        }

        return result;
    }

    // ── Decade ────────────────────────────────────────────────────────────────

    public static string? GetDecadeTag(int? releaseYear, PluginConfiguration config)
    {
        if (!config.EnableDecadeTags || releaseYear is null)
            return null;

        int y = releaseYear.Value;

        if (y < 1950)  return config.DecadePreFifties;
        if (y < 1960)  return config.Decade50s;
        if (y < 1970)  return config.Decade60s;
        if (y < 1980)  return config.Decade70s;
        if (y < 1990)  return config.Decade80s;
        if (y < 2000)  return config.Decade90s;
        if (y < 2010)  return config.Decade00s;
        if (y < 2020)  return config.Decade10s;
        return config.Decade20s;
    }

    // ── Rating ────────────────────────────────────────────────────────────────

    public static string? GetRatingTag(string? officialRating, PluginConfiguration config)
    {
        if (!config.EnableRatingTags || string.IsNullOrWhiteSpace(officialRating))
            return null;

        var mapping = ParseKeyValueMappings(config.RatingMappings);
        mapping.TryGetValue(officialRating.Trim(), out var tag);
        return tag;
    }

    // ── Language ──────────────────────────────────────────────────────────────

    public static IReadOnlyList<string> GetLanguageTags(
        BaseItem item,
        PluginConfiguration config)
    {
        if (!config.EnableLanguageTags)
            return Array.Empty<string>();

        var mapping = ParseKeyValueMappings(config.LanguageMappings);
        var result  = new List<string>();

        var mediaStreams = item.GetMediaStreams();
        if (mediaStreams is null)
            return result;

        var audioLanguages = mediaStreams
            .Where(s => s.Type == MediaStreamType.Audio && !string.IsNullOrWhiteSpace(s.Language))
            .Select(s => s.Language!.Trim().ToLowerInvariant())
            .Distinct();

        foreach (var lang in audioLanguages)
        {
            if (mapping.TryGetValue(lang, out var tag))
                result.Add(tag);
        }

        return result;
    }

    // ── Resolution ────────────────────────────────────────────────────────────

    public static string? GetResolutionTag(BaseItem item, PluginConfiguration config)
    {
        if (!config.EnableResolutionTags)
            return null;

        var mediaStreams = item.GetMediaStreams();
        if (mediaStreams is null)
            return null;

        var videoStream = mediaStreams
            .FirstOrDefault(s => s.Type == MediaStreamType.Video);

        if (videoStream?.Height is null)
            return null;

        int h = videoStream.Height.Value;

        if (h >= 2160) return config.Resolution4kTag;
        if (h >= 1080) return config.ResolutionFhdTag;
        if (h >= 720)  return config.ResolutionHdTag;
        return config.ResolutionSdTag;
    }

    // ── All known auto-tags (for stale removal) ───────────────────────────────

    public static IReadOnlySet<string> GetAllKnownTags(PluginConfiguration config)
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (config.EnableSeasonalTags)
        {
            set.Add(config.SpringTag);
            set.Add(config.SummerTag);
            set.Add(config.FallTag);
            set.Add(config.WinterTag);
        }

        if (config.EnableGenreTags)
            foreach (var tag in ParseKeyValueMappings(config.GenreTagMappings).Values)
                set.Add(tag);

        if (config.EnableDecadeTags)
        {
            set.Add(config.DecadePreFifties);
            set.Add(config.Decade50s);
            set.Add(config.Decade60s);
            set.Add(config.Decade70s);
            set.Add(config.Decade80s);
            set.Add(config.Decade90s);
            set.Add(config.Decade00s);
            set.Add(config.Decade10s);
            set.Add(config.Decade20s);
        }

        if (config.EnableRatingTags)
            foreach (var tag in ParseKeyValueMappings(config.RatingMappings).Values)
                set.Add(tag);

        if (config.EnableLanguageTags)
            foreach (var tag in ParseKeyValueMappings(config.LanguageMappings).Values)
                set.Add(tag);

        if (config.EnableResolutionTags)
        {
            set.Add(config.ResolutionSdTag);
            set.Add(config.ResolutionHdTag);
            set.Add(config.ResolutionFhdTag);
            set.Add(config.Resolution4kTag);
        }

        return set;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static HashSet<int> ParseMonths(string csv)
    {
        var result = new HashSet<int>();
        foreach (var part in csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (int.TryParse(part, out int m) && m >= 1 && m <= 12)
                result.Add(m);
        }
        return result;
    }

    public static Dictionary<string, string> ParseKeyValueMappings(string raw)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(raw))
            return dict;

        foreach (var line in raw.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith('#') || !trimmed.Contains('='))
                continue;

            var idx   = trimmed.IndexOf('=');
            var key   = trimmed[..idx].Trim();
            var value = trimmed[(idx + 1)..].Trim();

            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                dict[key] = value;
        }

        return dict;
    }
}
