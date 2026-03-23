using Jellyfin.Data.Enums;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.AutoTagger.ScheduledTasks;

public class AutoTagTask : IScheduledTask
{
    private readonly ILibraryManager _libraryManager;
    private readonly ILogger<AutoTagTask> _logger;

    public string Name        => "Run Auto Tagger";
    public string Key         => "AutoTaggerRun";
    public string Description => "Applies seasonal, genre, decade, rating, language and resolution tags to all movies and series.";
    public string Category    => "Auto Tagger";

    public AutoTagTask(ILibraryManager libraryManager, ILogger<AutoTagTask> logger)
    {
        _libraryManager = libraryManager;
        _logger         = logger;
    }

    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
    {
        return new[]
        {
            new TaskTriggerInfo
            {
                Type           = MediaBrowser.Model.Tasks.TaskTriggerInfoType.DailyTrigger,
                TimeOfDayTicks = TimeSpan.FromHours(3).Ticks
            }
        };
    }

    public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
    {
        var config = Plugin.Instance?.Configuration;
        if (config is null)
        {
            _logger.LogWarning("AutoTagger: plugin configuration not available, skipping run.");
            return;
        }

        var query = new InternalItemsQuery
        {
            IsVirtualItem    = false,
            Recursive        = true,
            IncludeItemTypes = new[]
            {
                BaseItemKind.Movie,
                BaseItemKind.Series
            }
        };

        var allItems = _libraryManager.GetItemList(query)
            .Where(i =>
                (config.TagMovies && i is Movie) ||
                (config.TagSeries && i is Series))
            .ToList();

        if (allItems.Count == 0)
        {
            _logger.LogInformation("AutoTagger: no items to process.");
            progress.Report(100);
            return;
        }

        var knownTags = TagRuleEngine.GetAllKnownTags(config);

        int processed = 0;
        foreach (var item in allItems)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await ProcessItemAsync(item, config, knownTags).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AutoTagger: error processing item '{Name}'", item.Name);
            }

            processed++;
            progress.Report((double)processed / allItems.Count * 100);
        }

        _logger.LogInformation("AutoTagger: finished — processed {Count} item(s).", processed);
    }

    private async Task ProcessItemAsync(
        BaseItem item,
        Configuration.PluginConfiguration config,
        IReadOnlySet<string> knownTags)
    {
        var existingTags = item.Tags?.ToList() ?? new List<string>();
        var updatedTags  = existingTags.ToList();

        // 1. Remove stale auto-tags
        if (config.RemoveStaleAutoTags)
            updatedTags.RemoveAll(t => knownTags.Contains(t));

        // 2. Compute new tags
        var toAdd = new List<string>();

        // Seasonal
        var seasonalTag = TagRuleEngine.GetSeasonalTag(item.PremiereDate?.Month, config);
        if (seasonalTag is not null) toAdd.Add(seasonalTag);

        // Genre
        foreach (var gt in TagRuleEngine.GetGenreTags(item.Genres ?? Array.Empty<string>(), config))
            toAdd.Add(gt);

        // Decade
        var decadeTag = TagRuleEngine.GetDecadeTag(item.PremiereDate?.Year, config);
        if (decadeTag is not null) toAdd.Add(decadeTag);

        // Rating
        var ratingTag = TagRuleEngine.GetRatingTag(item.OfficialRating, config);
        if (ratingTag is not null) toAdd.Add(ratingTag);

        // Language
        foreach (var lt in TagRuleEngine.GetLanguageTags(item, config))
            toAdd.Add(lt);

        // Resolution
        var resTag = TagRuleEngine.GetResolutionTag(item, config);
        if (resTag is not null) toAdd.Add(resTag);

        // 3. Merge — avoid duplicates
        foreach (var tag in toAdd)
        {
            if (!updatedTags.Contains(tag, StringComparer.OrdinalIgnoreCase))
                updatedTags.Add(tag);
        }

        // 4. Save only if something changed
        var changed = !updatedTags.OrderBy(t => t)
                                   .SequenceEqual(existingTags.OrderBy(t => t),
                                                  StringComparer.OrdinalIgnoreCase);
        if (!changed)
            return;

        item.Tags = updatedTags.ToArray();

        await _libraryManager.UpdateItemAsync(
            item,
            item.GetParent(),
            ItemUpdateType.MetadataEdit,
            CancellationToken.None).ConfigureAwait(false);

        _logger.LogInformation(
            "AutoTagger: updated '{Name}' — added [{Added}]",
            item.Name,
            string.Join(", ", toAdd));
    }
}
