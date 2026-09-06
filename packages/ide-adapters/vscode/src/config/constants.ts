/** Extension configuration section prefix used in VS Code settings. */
export const CONFIG_SECTION = 'babel-tcc';

/** Configuration keys for VS Code settings (babel-tcc.*). */
export const CONFIG_KEYS = {
  ENABLED: `${CONFIG_SECTION}.enabled`,
  LANGUAGE: `${CONFIG_SECTION}.language`,
  TRANSLATIONS_PATH: `${CONFIG_SECTION}.translationsPath`,
  READONLY: `${CONFIG_SECTION}.readonly`,
} as const;

/**
 * Context keys published to VS Code so menus and keybindings can declare visibility with `when`
 * clauses. Names are camelCase on purpose: a hyphen inside a context key is parsed as an operator
 * in `when` expressions, so `babel-tcc.*` would not work here.
 */
export const CONTEXT_KEYS = {
  ENABLED: 'babelTcc.enabled',
  SUPPORTED_FILE: 'babelTcc.supportedFile',
  TRANSLATED_VIEW: 'babelTcc.translatedView',
  READONLY_VIEW: 'babelTcc.readonlyView',
} as const;

/** Command identifiers registered by the extension. */
export const COMMANDS = {
  TOGGLE: `${CONFIG_SECTION}.toggle`,
  SELECT_LANGUAGE: `${CONFIG_SECTION}.selectLanguage`,
  OPEN_TRANSLATED_EDITABLE: `${CONFIG_SECTION}.openTranslatedEditable`,
  OPEN_TRANSLATED_READONLY: `${CONFIG_SECTION}.openTranslatedReadonly`,
  SHOW_ORIGINAL: `${CONFIG_SECTION}.showOriginal`,
} as const;

/** Sibling repository name for auto-detection of translations. */
export const TRANSLATIONS_REPO_NAME = 'babel-tcc-translations';

/** Translation directory structure paths. */
export const TRANSLATION_PATHS = {
  NATURAL_LANGUAGES: 'natural-languages',
  PROGRAMMING_LANGUAGES: 'programming-languages',
  KEYWORDS_BASE: 'keywords-base.json',
} as const;
