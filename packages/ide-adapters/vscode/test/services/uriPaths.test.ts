import { describe, it, expect } from 'vitest';
import { Uri } from '../__mocks__/vscode';
import { normalizeUriPath, isSameUriPath, isSameUri, buildUriKey } from '../../src/services/uriPaths';

describe('uriPaths', () => {
  describe('normalizeUriPath', () => {
    it('should lower an upper case drive letter', () => {
      expect(normalizeUriPath('/C:/Users/dev/Program.cs')).toBe('/c:/Users/dev/Program.cs');
    });

    it('should leave an already lower case drive letter untouched', () => {
      expect(normalizeUriPath('/c:/Users/dev/Program.cs')).toBe('/c:/Users/dev/Program.cs');
    });

    it('should normalize any drive letter, not only C', () => {
      expect(normalizeUriPath('/D:/projects/app.cs')).toBe('/d:/projects/app.cs');
    });

    it('should preserve the case of the rest of the path', () => {
      expect(normalizeUriPath('/C:/Users/Klebe/MyFolder/Program.CS')).toBe(
        '/c:/Users/Klebe/MyFolder/Program.CS'
      );
    });

    it('should leave a path without a drive letter untouched', () => {
      expect(normalizeUriPath('/home/Dev/Program.cs')).toBe('/home/Dev/Program.cs');
    });

    it('should not treat a multi letter segment as a drive letter', () => {
      expect(normalizeUriPath('/AB:/thing')).toBe('/AB:/thing');
    });

    it('should return an empty path unchanged', () => {
      expect(normalizeUriPath('')).toBe('');
    });
  });

  describe('isSameUriPath', () => {
    it('should match two spellings of the same Windows path', () => {
      expect(isSameUriPath('/C:/Users/dev/Program.cs', '/c:/Users/dev/Program.cs')).toBe(true);
    });

    it('should not match two different files', () => {
      expect(isSameUriPath('/c:/Users/dev/Program.cs', '/c:/Users/dev/Other.cs')).toBe(false);
    });

    it('should stay case sensitive outside the drive letter', () => {
      // On Linux these are genuinely different files, so they must not compare equal.
      expect(isSameUriPath('/home/User/program.cs', '/home/user/program.cs')).toBe(false);
    });
  });

  describe('isSameUri', () => {
    it('should match the same document spelled with a different drive case', () => {
      const upperCaseDrive: Uri = Uri.parse('babel-tcc-translated:/C:/Users/dev/Program.cs');
      const lowerCaseDrive: Uri = Uri.parse('babel-tcc-translated:/c:/Users/dev/Program.cs');
      expect(isSameUri(upperCaseDrive, lowerCaseDrive)).toBe(true);
    });

    it('should not match the same path under different schemes', () => {
      const translatedUri: Uri = Uri.parse('babel-tcc-translated:/c:/Users/dev/Program.cs');
      const originalUri: Uri = Uri.parse('file:/c:/Users/dev/Program.cs');
      expect(isSameUri(translatedUri, originalUri)).toBe(false);
    });
  });

  describe('buildUriKey', () => {
    it('should build the same key for both drive spellings', () => {
      const upperCaseDrive: Uri = Uri.parse('babel-tcc-translated:/C:/Users/dev/Program.cs');
      const lowerCaseDrive: Uri = Uri.parse('babel-tcc-translated:/c:/Users/dev/Program.cs');
      expect(buildUriKey(upperCaseDrive)).toBe(buildUriKey(lowerCaseDrive));
    });

    it('should build different keys for different schemes', () => {
      const translatedUri: Uri = Uri.parse('babel-tcc-translated:/c:/Users/dev/Program.cs');
      const readonlyUri: Uri = Uri.parse('babel-tcc-readonly:/c:/Users/dev/Program.cs');
      expect(buildUriKey(translatedUri)).not.toBe(buildUriKey(readonlyUri));
    });
  });
});
