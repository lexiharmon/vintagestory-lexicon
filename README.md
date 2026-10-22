<p align="center"><img src="assets/logo.png" alt="Lexicon logo" width="160"></p>

# Lexicon

A [Vintage Story](https://www.vintagestory.at/) client-side mod that improves the Survival Handbook search bar.

Vanilla search shows every page whose title or description contains *any* typed word as a substring. Lexicon shows exact matches instead:

- Words must match as **complete words**, not case-sensitive ("axe" finds "Iron axe" but no longer "Iron pickaxe").
- The **last word** you type may be a word prefix, so "iron sa" finds "Iron saw" while you are still typing.
- Complete-word hits are listed before prefix hits. Result ordering otherwise matches vanilla.
- Only titles are searched by default. The Command Handbook is untouched.

## Config

Created on first launch as `ModConfig/lexicon.json`:

| Setting | Default | Meaning |
|---|---|---|
| `Enabled` | `true` | Set to `false` to get vanilla search back. |
| `Mode` | `Words` | `Words` (all words, any order), `Phrase` (words together, in order), `Title` (title must equal the query). |
| `PrefixLastWord` | `true` | Let the last typed word match the start of a word. Ignored in `Title` mode. |
| `SearchDescriptions` | `false` | Also search page descriptions (still whole-word). |

## Install

Run `Lexicon-Setup.exe` (Windows), or copy `Lexicon_<version>.zip` into `%APPDATA%\VintagestoryData\Mods`. The installer can also update or uninstall, and supports `--silent`, `--uninstall` and `--mods-dir "path"`.

## Build

Requires the .NET 10 SDK and a Vintage Story 1.22 install (set the `VINTAGE_STORY` environment variable if it is not in `%APPDATA%\Vintagestory`).

```
dotnet build Lexicon/Lexicon.csproj
dotnet publish LexiconInstaller/LexiconInstaller.csproj -c Release -o LexiconInstaller/publish
```

The first command produces `Lexicon/bin/Lexicon_<version>.zip`; the installer embeds that zip, so build the mod first.

## How it works

`FilterItemsPatch` is a Harmony prefix on the Survival Handbook's `GuiDialogHandbook.FilterItems`. It reproduces the vanilla method with a different match test (`ExactMatcher`), so a game update that changes `FilterItems` may need the patch updated.
