const fs = require('fs');
const path = require('path');

// Copies the repository's examples/ into the extension folder, so they ship inside the .vsix.
// `vsce` only packages what is inside the extension folder, and examples/ lives at the repository
// root: without this copy, "Open Example" would offer files that are not in the installed package.
//
// Like copy-translations.js, a missing source fails loudly instead of packaging an extension whose
// examples command breaks only after it is published.

const REPO_ROOT = path.resolve(__dirname, '..');
const SOURCE = path.join(REPO_ROOT, 'examples');
const DEST = path.join(REPO_ROOT, 'packages', 'ide-adapters', 'vscode', 'examples');

// Build and cache folders that can exist inside examples/ after running them locally.
const SKIP_DIRS = new Set(['.multilingual', 'bin', 'obj', '__pycache__', 'node_modules']);

function copyDir(src, dest) {
  fs.mkdirSync(dest, { recursive: true });
  let copied = 0;
  for (const entry of fs.readdirSync(src)) {
    if (SKIP_DIRS.has(entry)) {
      continue;
    }
    const srcPath = path.join(src, entry);
    const destPath = path.join(dest, entry);
    if (fs.statSync(srcPath).isDirectory()) {
      copied += copyDir(srcPath, destPath);
    } else {
      fs.copyFileSync(srcPath, destPath);
      copied += 1;
    }
  }
  return copied;
}

if (!fs.existsSync(SOURCE)) {
  console.error(`ERROR: examples folder not found: ${SOURCE}`);
  console.error('The examples are required to package the extension ("Open Example" uses them).');
  process.exit(1);
}

// Start from an empty destination, so an example removed from the repository is not shipped from a
// previous run.
fs.rmSync(DEST, { recursive: true, force: true });
const fileCount = copyDir(SOURCE, DEST);

if (fileCount === 0) {
  console.error(`ERROR: no example files found in ${SOURCE}`);
  process.exit(1);
}

console.log(`Copied ${fileCount} example file(s) to ${DEST}`);
