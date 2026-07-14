/** Extension configuration section prefix used in VS Code settings. */
export const CONFIG_SECTION = 'babel-tcc';

/** Configuration keys for VS Code settings (babel-tcc.*). */
export const CONFIG_KEYS = {
  ENABLED: `${CONFIG_SECTION}.enabled`,
  LANGUAGE: `${CONFIG_SECTION}.language`,
  TRANSLATIONS_PATH: `${CONFIG_SECTION}.translationsPath`,
  READONLY: `${CONFIG_SECTION}.readonly`,
  // Identifier translation (Serviço 1): hosted backend vs. local llama-server.
  LLM_MODE: `${CONFIG_SECTION}.llm.mode`,
  SERVICES_URL: `${CONFIG_SECTION}.services.url`,
  SERVICES_API_KEY: `${CONFIG_SECTION}.services.apiKey`,
  LOCAL_LLM_URL: `${CONFIG_SECTION}.local-llm.url`,
} as const;

/** Command identifiers registered by the extension. */
export const COMMANDS = {
  TOGGLE: `${CONFIG_SECTION}.toggle`,
  SELECT_LANGUAGE: `${CONFIG_SECTION}.selectLanguage`,
  OPEN_TRANSLATED_EDITABLE: `${CONFIG_SECTION}.openTranslatedEditable`,
  OPEN_TRANSLATED_READONLY: `${CONFIG_SECTION}.openTranslatedReadonly`,
  SHOW_ORIGINAL: `${CONFIG_SECTION}.showOriginal`,
  SUGGEST_TRANSLATION: `${CONFIG_SECTION}.suggestTranslation`,
} as const;

/** SecretStorage key under which the hosted-backend API key is stored securely. */
export const SECRET_API_KEY = `${CONFIG_SECTION}.services.apiKey`;

/** Marker comment written when a user accepts an identifier translation suggestion. */
export const TRANSLATION_MARKER_PREFIX = '// tradu';

/** Sibling repository name for auto-detection of translations. */
export const TRANSLATIONS_REPO_NAME = 'babel-tcc-translations';

/** Translation directory structure paths. */
export const TRANSLATION_PATHS = {
  NATURAL_LANGUAGES: 'natural-languages',
  PROGRAMMING_LANGUAGES: 'programming-languages',
  KEYWORDS_BASE: 'keywords-base.json',
} as const;
