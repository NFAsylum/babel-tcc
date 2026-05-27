# Changelog

All notable changes to the Babel TCC extension are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.9.3] - 2026-05-27

### Added

- Per-platform self-contained packages (Windows, Linux and macOS, x64 and arm64) that bundle the
  .NET runtime — **no .NET installation required**. A universal package (requires the .NET 8
  Runtime) remains as a fallback for any platform without a dedicated build.
- UI localization in French, German, Italian and Japanese (in addition to the existing languages).
- C# property accessor keywords `get` and `set` are now translated (only in accessor context, so
  identifiers named `get`/`set` are left untouched).
- Demo GIF and per-language screenshots on the README / Marketplace page.

### Fixed

- `// tradu[lang]:` identifier annotations now work for language codes with more than two segments,
  such as `ja-jp-romaji` and `pt-br-ascii` (previously these were silently ignored).
- Syntax highlighting of translated keywords and identifiers now works for non-Latin scripts
  (Chinese, Arabic) and for words starting with an accented letter.

## [0.9.2] - 2026-05-27

### Fixed

- .NET detection on activation now checks for the .NET 8 **Runtime** instead of the
  SDK. The previous check used `dotnet --version` (which reports the SDK) and falsely
  warned that the SDK was required on runtime-only machines — the extension only needs
  the runtime.

### Changed

- Release pipeline hardened: publishing to the Marketplace and Open VSX now retries on
  transient failures and is idempotent (skips when the version is already published),
  so a transient timeout no longer blocks the release or requires a manual rerun.

### Added

- README translations in English and Spanish in the repository.

## [0.9.1] - 2026-05-26

### Fixed

- README was missing from the published Visual Studio Code Marketplace
  listing, leaving the overview page blank. The release pipeline now copies
  the README into the extension before packaging.

### Added

- Automatic publishing to the Open VSX Registry, so the extension is
  installable from VS Code Marketplace and Open VSX-based clients alike.
- Localized extension manifest (English, Brazilian Portuguese, Spanish):
  command titles and setting descriptions now follow the editor's display
  language.

## [0.9.0] - 2026-05-25

First public release on the Visual Studio Code Marketplace.

### Added

- Real-time translation of source code keywords and identifiers to a target
  natural language.
- Support for C# and Python source files.
- Support for the VisuAlg (`.alg`) and Portugol Studio (`.por`) dialects.
- Translated views: editable and read-only.
- Per-language target overrides via `babel-tcc.languageOverrides`.
- Configurable translations path (`babel-tcc.translationsPath`) with
  auto-detection of a sibling translations repository and embedded fallback.
- Commands: toggle translation, select language, open translated view
  (editable/read-only), and show original code.
