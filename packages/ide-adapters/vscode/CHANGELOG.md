# Changelog

All notable changes to the Babel TCC extension are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.9.1] - 2026-05-26

### Fixed

- README was missing from the published Visual Studio Code Marketplace
  listing, leaving the overview page blank. The release pipeline now copies
  the README into the extension before packaging.

### Added

- Automatic publishing to the Open VSX Registry, so the extension is
  installable from VS Code Marketplace and Open VSX-based clients alike.

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
