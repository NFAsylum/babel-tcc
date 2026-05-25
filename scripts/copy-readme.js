const fs = require('fs');
const path = require('path');

// The marketplace page for the extension is rendered from the README.md that
// sits next to package.json. We keep a single source of truth at the repo root
// and copy it into the extension folder at package time, so the GitHub README
// and the Marketplace page never drift apart.
//
// No fallback: the README is the extension's storefront. Packaging a .vsix with
// a blank Marketplace page is shipping a broken listing — fail loud instead.
const REPO_ROOT = path.resolve(__dirname, '..');
const SRC = path.join(REPO_ROOT, 'README.md');
const DEST = path.resolve(__dirname, '..', 'packages', 'ide-adapters', 'vscode', 'README.md');

if (!fs.existsSync(SRC)) {
    console.error(`ERROR: README.md not found at ${SRC}`);
    console.error('The Marketplace listing is rendered from this file; cannot package without it.');
    process.exit(1);
}

fs.copyFileSync(SRC, DEST);
console.log(`Copied README.md -> ${DEST}`);
