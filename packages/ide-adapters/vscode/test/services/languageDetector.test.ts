import { describe, it, expect, beforeEach } from 'vitest';
import { LanguageDetector } from '../../src/services/languageDetector';

describe('LanguageDetector', () => {
  let detector: LanguageDetector;

  beforeEach(() => {
    detector = new LanguageDetector();
  });

  describe('detectLanguage', () => {
    it('should return CSharp for .cs files', () => {
      expect(detector.detectLanguage('Program.cs')).toBe('CSharp');
    });

    it('should return CSharp for uppercase .CS extension', () => {
      expect(detector.detectLanguage('Program.CS')).toBe('CSharp');
    });

    it('should return CSharp for mixed case .Cs extension', () => {
      expect(detector.detectLanguage('file.Cs')).toBe('CSharp');
    });

    it('should return undefined for unsupported extension', () => {
      expect(detector.detectLanguage('script.py')).toBeUndefined();
    });

    it('should return undefined for file without extension', () => {
      expect(detector.detectLanguage('Makefile')).toBeUndefined();
    });

    it('should work with nested paths', () => {
      expect(detector.detectLanguage('/src/models/User.cs')).toBe('CSharp');
    });
  });

  describe('isSupported', () => {
    it('should return true for .cs files', () => {
      expect(detector.isSupported('file.cs')).toBe(true);
    });

    it('should return false for .txt files', () => {
      expect(detector.isSupported('file.txt')).toBe(false);
    });

    it('should return false for files without extension', () => {
      expect(detector.isSupported('README')).toBe(false);
    });
  });

  describe('getFileExtension', () => {
    it('should return lowercase extension with dot', () => {
      expect(detector.getFileExtension('file.cs')).toBe('.cs');
    });

    it('should lowercase mixed case extension', () => {
      expect(detector.getFileExtension('file.CS')).toBe('.cs');
    });

    it('should return empty string for file without extension', () => {
      expect(detector.getFileExtension('Makefile')).toBe('');
    });
  });

  describe('getSupportedExtensions', () => {
    it('should return all registered extensions', () => {
      const extensions = detector.getSupportedExtensions();
      expect(extensions).toContain('.cs');
      expect(extensions.length).toBeGreaterThanOrEqual(1);
    });
  });
});
