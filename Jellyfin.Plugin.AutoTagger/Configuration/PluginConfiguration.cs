using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.AutoTagger.Configuration;

public class PluginConfiguration : BasePluginConfiguration
{
    // ── General ──────────────────────────────────────────────────────────────
    public bool TagMovies { get; set; } = true;
    public bool TagSeries { get; set; } = true;
    public bool RemoveStaleAutoTags { get; set; } = true;

    // ── Seasonal ──────────────────────────────────────────────────────────────
    public bool EnableSeasonalTags { get; set; } = true;
    public string SpringTag { get; set; } = "season-spring";
    public string SummerTag { get; set; } = "season-summer";
    public string FallTag { get; set; } = "season-fall";
    public string WinterTag { get; set; } = "season-winter";
    public string SpringMonths { get; set; } = "3,4,5";
    public string SummerMonths { get; set; } = "6,7,8";
    public string FallMonths { get; set; } = "9,10,11";
    public string WinterMonths { get; set; } = "12,1,2";

    // ── Genre ─────────────────────────────────────────────────────────────────
    public bool EnableGenreTags { get; set; } = true;
    public string GenreTagMappings { get; set; } = "Action=tag-action\nAdventure=tag-adventure\nAnimation=tag-animation\nComedy=tag-comedy\nCrime=tag-crime\nDocumentary=tag-documentary\nDrama=tag-drama\nFamily=tag-family\nFantasy=tag-fantasy\nHistory=tag-history\nHorror=tag-horror\nMusic=tag-music\nMystery=tag-mystery\nRomance=tag-romance\nScience Fiction=tag-scifi\nThriller=tag-thriller\nWar=tag-war\nWestern=tag-western";

    // ── Decade ────────────────────────────────────────────────────────────────
    public bool EnableDecadeTags { get; set; } = true;
    public string DecadePreFifties { get; set; } = "tag-pre1950";
    public string Decade50s { get; set; } = "tag-50s";
    public string Decade60s { get; set; } = "tag-60s";
    public string Decade70s { get; set; } = "tag-70s";
    public string Decade80s { get; set; } = "tag-80s";
    public string Decade90s { get; set; } = "tag-90s";
    public string Decade00s { get; set; } = "tag-00s";
    public string Decade10s { get; set; } = "tag-10s";
    public string Decade20s { get; set; } = "tag-20s";

    // ── Rating ────────────────────────────────────────────────────────────────
    public bool EnableRatingTags { get; set; } = true;
    public string RatingMappings { get; set; } = "G=tag-rated-g\nPG=tag-rated-pg\nPG-13=tag-rated-pg13\nR=tag-rated-r\nNC-17=tag-rated-nc17\nTV-G=tag-rated-tvg\nTV-PG=tag-rated-tvpg\nTV-14=tag-rated-tv14\nTV-MA=tag-rated-tvma";

    // ── Language ──────────────────────────────────────────────────────────────
    public bool EnableLanguageTags { get; set; } = true;
    public string LanguageMappings { get; set; } = "eng=tag-lang-english\nspa=tag-lang-spanish\nfra=tag-lang-french\ndeu=tag-lang-german\njpn=tag-lang-japanese\nkor=tag-lang-korean\nzho=tag-lang-chinese\nita=tag-lang-italian\npor=tag-lang-portuguese\nrus=tag-lang-russian";

    // ── Resolution ────────────────────────────────────────────────────────────
    public bool EnableResolutionTags { get; set; } = true;
    public string ResolutionSdTag { get; set; } = "tag-sd";
    public string ResolutionHdTag { get; set; } = "tag-hd";
    public string ResolutionFhdTag { get; set; } = "tag-fullhd";
    public string Resolution4kTag { get; set; } = "tag-4k";
}
