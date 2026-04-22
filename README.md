# Auto Tagger

Automatically applies genre, decade, rating, language, and resolution tags to every movie and TV series in your Jellyfin library based on their metadata.

Works hand-in-hand with [Seasonal Visibility](https://github.com/jshafer813/Seasonal-Visibility) — Auto Tagger writes the tags, Seasonal Visibility uses them to show or hide content per user.

---

## Features

- Genre tags - maps Jellyfin genres to custom tags (e.g. `tag-action`, `tag-horror`)
- Decade tags - tags items by release year (e.g. `tag-80s`, `tag-90s`)
- Rating tags - tags items by official rating (e.g. `tag-rated-r`, `tag-rated-pg13`)
- Language tags - tags items by audio language (e.g. `tag-lang-english`, `tag-lang-japanese`)
- Resolution tags - tags items by video quality (e.g. `tag-hd`, `tag-fullhd`, `tag-4k`)
- Stale tag cleanup - removes old auto-applied tags before re-tagging when rules change
- Dry run preview - see exactly what would be tagged before committing
- Runs daily at 3am automatically, or manually from Scheduled Tasks

---

## Installation

1. Go to **Dashboard -> Plugins -> Repositories** and add:
```
https://raw.githubusercontent.com/wildenrou/Auto-Tagger/main/manifest.json
```

If Jellyfin still shows a cached repository result, use the 10.11-specific manifest URL instead:

```text
https://raw.githubusercontent.com/wildenrou/Auto-Tagger/main/manifest-10.11.json
```

Current cache-busted 10.11 manifest:

```text
https://raw.githubusercontent.com/wildenrou/Auto-Tagger/main/manifest-10.11-v2.json
```

Patch-specific 10.11.8 manifest:

```text
https://raw.githubusercontent.com/wildenrou/Auto-Tagger/main/manifest-10.11.8.json
```
2. Go to **Dashboard -> Plugins -> Catalog** and install **Auto Tagger**
3. Restart Jellyfin
4. Navigate to **Dashboard -> Plugins -> Auto Tagger** to configure

---

## Manual installation

For Jellyfin Docker 10.11.8, build the plugin zip and copy its contents into a plugin folder under your Jellyfin config volume:

```powershell
.\build.ps1
```

The package is written to `dist/Jellyfin.Plugin.AutoTagger.zip`. Extract `Jellyfin.Plugin.AutoTagger.dll` and `meta.json` into:

```text
/config/plugins/Auto Tagger_1.0.1.0/
```

Then restart the Jellyfin container.

---

## How It Works

Auto Tagger scans every Movie and Series in your library and applies tags based on their metadata:

- **Genre tags** - mapped via a configurable `Genre Name=tag-name` list
- **Decade tags** - based on `PremiereDate` year
- **Rating tags** - based on `OfficialRating` field (e.g. PG-13, TV-MA)
- **Language tags** - based on audio stream ISO 639 language codes
- **Resolution tags** - based on video stream height (720p, 1080p, 2160p)

Tags are plain Jellyfin tags and work anywhere tags are accepted - user policies, smart collections, filters, and Seasonal Visibility blocked tag rules.

---

## Pairing with Seasonal Visibility

1. Run Auto Tagger to stamp tags onto your library
2. Open **Seasonal Visibility** and add a rule blocking e.g. `tag-horror` outside of October
3. Horror movies disappear for non-admin users outside of October and reappear automatically

---

## Requirements

- Jellyfin **10.11.8**
- .NET **9 SDK** to build from source

---

## Building from source

```powershell
.\build.ps1
```

This creates:

- `dist/Jellyfin.Plugin.AutoTagger.zip`
- `dist/Jellyfin.Plugin.AutoTagger.zip.md5`

---

## Changelog

- **v1.0.1** - Rebuilt for Jellyfin Docker 10.11.8 and packaged as a Jellyfin plugin zip.
- **v1.0.0** - Initial release: genre, decade, rating, language and resolution tagging with dry run preview and stale tag cleanup
