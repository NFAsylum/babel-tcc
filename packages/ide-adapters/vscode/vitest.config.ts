import { defineConfig } from 'vitest/config';
import * as path from 'path';

export default defineConfig({
  test: {
    include: ['test/**/*.test.ts'],
    coverage: {
      provider: 'v8',
      reporter: ['text', 'lcov', 'json-summary'],
      reportsDirectory: 'coverage',
      include: ['src/**/*.ts'],
      exclude: ['src/**/*.d.ts'],
    },
    alias: {
      vscode: path.resolve(__dirname, 'test/__mocks__/vscode.ts'),
    },
  },
});
