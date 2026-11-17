import * as vscode from 'vscode';

/**
 * Matches a URI path that starts with a Windows drive letter, as VS Code writes it: a leading
 * slash, the letter, and the colon (`/c:/dir/file.cs`).
 */
const WINDOWS_DRIVE_PATH_PATTERN: RegExp = /^\/[a-zA-Z]:/;

/** Separator between the scheme and the path in the key built by `buildUriKey`. */
const URI_KEY_SEPARATOR: string = '::';

/**
 * Normalizes a URI path so two spellings of the same file compare equal.
 *
 * Only the Windows drive letter is lowered. Windows does not guarantee its case and VS Code
 * propagates whatever it receives, so the same file reaches the extension as `/C:/dir/file.cs` in
 * one place and `/c:/dir/file.cs` in another, within the same session. The rest of the path is left
 * untouched on purpose: on Linux paths are genuinely case sensitive, and lowering them whole would
 * make two distinct files compare equal. A path without a drive letter is returned unchanged, so
 * this is a no-op outside Windows.
 * @param uriPath - The path component of a URI, as given by `vscode.Uri.path`.
 * @returns The same path with the drive letter in lower case, when there is one.
 */
export function normalizeUriPath(uriPath: string): string {
  if (!WINDOWS_DRIVE_PATH_PATTERN.test(uriPath)) {
    return uriPath;
  }
  return `/${uriPath.charAt(1).toLowerCase()}${uriPath.substring(2)}`;
}

/**
 * Decides whether two URI paths point to the same file, ignoring drive letter case.
 * @param firstPath - The first path component to compare.
 * @param secondPath - The second path component to compare.
 * @returns True when both paths address the same file.
 */
export function isSameUriPath(firstPath: string, secondPath: string): boolean {
  return normalizeUriPath(firstPath) === normalizeUriPath(secondPath);
}

/**
 * Decides whether two URIs address the same document: same scheme and same file. Replaces comparing
 * `uri.toString()`, which is drive letter sensitive.
 * @param firstUri - The first URI to compare.
 * @param secondUri - The second URI to compare.
 * @returns True when both URIs address the same document.
 */
export function isSameUri(firstUri: vscode.Uri, secondUri: vscode.Uri): boolean {
  return firstUri.scheme === secondUri.scheme && isSameUriPath(firstUri.path, secondUri.path);
}

/**
 * Builds a stable map key for a URI, so a value stored under one spelling of the path is found
 * under any other spelling of the same file.
 * @param uri - The URI to build a key for.
 * @returns A key combining the scheme and the normalized path.
 */
export function buildUriKey(uri: vscode.Uri): string {
  return `${uri.scheme}${URI_KEY_SEPARATOR}${normalizeUriPath(uri.path)}`;
}
