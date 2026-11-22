import { describe, it, expect, beforeEach, vi } from 'vitest';
import * as fs from 'fs';
import * as path from 'path';
import { commands } from '../__mocks__/vscode';
import {
  OPEN_WALKTHROUGH_BUILTIN,
  buildWalkthroughReference,
  openWalkthrough,
} from '../../src/commands/openWalkthrough';
import { WALKTHROUGH_ID } from '../../src/config/constants';

const EXTENSION_ROOT: string = path.join(__dirname, '../..');

describe('openWalkthrough', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should reference the walkthrough as <extension id>#<walkthrough id>', () => {
    expect(buildWalkthroughReference('Publisher.extension')).toBe(`Publisher.extension#${WALKTHROUGH_ID}`);
  });

  it('should open the walkthrough of the running extension, in the current editor group', async () => {
    await openWalkthrough({ extension: { id: 'NFAsylum.multilingual-code' } } as any);

    expect(commands.executeCommand).toHaveBeenCalledWith(
      OPEN_WALKTHROUGH_BUILTIN,
      `NFAsylum.multilingual-code#${WALKTHROUGH_ID}`,
      false
    );
  });

  // The reference only works if the extension id the command builds at runtime is the one VS Code
  // derives from package.json: `publisher.name`.
  it('should match the id VS Code gives the extension', () => {
    const manifest: { publisher: string; name: string } = JSON.parse(
      fs.readFileSync(path.join(EXTENSION_ROOT, 'package.json'), 'utf-8')
    );
    expect(buildWalkthroughReference(`${manifest.publisher}.${manifest.name}`)).toBe(
      `NFAsylum.multilingual-code#${WALKTHROUGH_ID}`
    );
  });
});
