using Jellyfin.Data.Enums;
using Jellyfin.Plugin.AutoTagger.Configuration;
using MediaBrowser.Controller.Library;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.AutoTagger.Api;

[ApiController]
[Route("AutoTagger")]
[Authorize(Policy = "RequiresElevation")]
public class AutoTaggerController : ControllerBase
{
    private readonly ILibraryManager _libraryManager;
    private readonly ILogger<AutoTaggerController> _logger;

    public AutoTaggerController(
        ILibraryManager libraryManager,
        ILogger<AutoTaggerController> logger)
    {
        _libraryManager = libraryManager;
        _logger         = logger;
    }

    [HttpGet("Config")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<PluginConfiguration> GetConfig()
    {
        var config = Plugin.Instance?.Configuration ?? new PluginConfiguration();
        return Ok(config);
    }

    [HttpPost("Config")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult SaveConfig([FromBody] PluginConfiguration config)
    {
        if (Plugin.Instance is null)
            return BadRequest("Plugin instance not available.");

        Plugin.Instance.UpdateConfiguration(config);
        return NoContent();
    }

    [HttpGet("Preview")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<PreviewResult> GetPreview()
    {
        var config = Plugin.Instance?.Configuration ?? new PluginConfiguration();

        var query = new MediaBrowser.Controller.Entities.InternalItemsQuery
        {
            IsVirtualItem    = false,
            Recursive        = true,
            IncludeItemTypes = new[]
            {
                BaseItemKind.Movie,
                BaseItemKind.Series
            }
        };

        var items = _libraryManager.GetItemList(query).ToList();

        var tagCounts     = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        int itemsAffected = 0;

        foreach (var item in items)
        {
            var toAdd = new List<string>();

            var seasonalTag = TagRuleEngine.GetSeasonalTag(item.PremiereDate?.Month, config);
            if (seasonalTag is not null) toAdd.Add(seasonalTag);

            foreach (var gt in TagRuleEngine.GetGenreTags(item.Genres ?? Array.Empty<string>(), config))
                toAdd.Add(gt);

            var decadeTag = TagRuleEngine.GetDecadeTag(item.PremiereDate?.Year, config);
            if (decadeTag is not null) toAdd.Add(decadeTag);

            var ratingTag = TagRuleEngine.GetRatingTag(item.OfficialRating, config);
            if (ratingTag is not null) toAdd.Add(ratingTag);

            foreach (var lt in TagRuleEngine.GetLanguageTags(item, config))
                toAdd.Add(lt);

            var resTag = TagRuleEngine.GetResolutionTag(item, config);
            if (resTag is not null) toAdd.Add(resTag);

            if (toAdd.Count > 0)
            {
                itemsAffected++;
                foreach (var t in toAdd)
                    tagCounts[t] = tagCounts.GetValueOrDefault(t) + 1;
            }
        }

        return Ok(new PreviewResult
        {
            TotalItems    = items.Count,
            ItemsAffected = itemsAffected,
            TagBreakdown  = tagCounts
                .OrderByDescending(kv => kv.Value)
                .Select(kv => new TagCount { Tag = kv.Key, Count = kv.Value })
                .ToList()
        });
    }

    public class PreviewResult
    {
        public int TotalItems { get; set; }
        public int ItemsAffected { get; set; }
        public List<TagCount> TagBreakdown { get; set; } = new();
    }

    public class TagCount
    {
        public string Tag { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
