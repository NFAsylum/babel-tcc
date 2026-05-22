const fs = require('fs');
const path = require('path');

const VSCODE_DIR = path.resolve(__dirname, '..', 'packages', 'ide-adapters', 'vscode');
const DEST = path.join(VSCODE_DIR, 'translations');
const TEST_DATA = path.resolve(__dirname, '..', 'packages', 'core', 'MultiLingualCode.Core.Tests', 'TestData', 'translations');

// Candidate locations for the full babel-tcc-translations repo, in priority order:
// 1. Explicit override via env var (used by release.yml in CI)
// 2. Sibling of the babel-tcc repo (typical for local development)
// 3. Inside the babel-tcc workspace at top level (used by CI when actions/checkout
//    places the translations repo alongside the source tree)
const FULL_REPO_CANDIDATES = [
    process.env.BABEL_TCC_TRANSLATIONS_PATH,
    path.resolve(__dirname, '..', '..', 'babel-tcc-translations'),
    path.resolve(__dirname, '..', 'babel-tcc-translations'),
].filter(Boolean);

const SKIP_DIRS = new Set(['.git', 'node_modules', 'scripts', '__pycache__']);

function copyDir(src, dest) {
    fs.mkdirSync(dest, { recursive: true });
    for (const entry of fs.readdirSync(src)) {
        if (SKIP_DIRS.has(entry)) {
            continue;
        }
        const srcPath = path.join(src, entry);
        const destPath = path.join(dest, entry);
        if (fs.statSync(srcPath).isDirectory()) {
            copyDir(srcPath, destPath);
        } else {
            fs.copyFileSync(srcPath, destPath);
        }
    }
}

function validate(dir) {
    const natLangs = path.join(dir, 'natural-languages');
    if (!fs.existsSync(natLangs)) {
        return false;
    }
    const langs = fs.readdirSync(natLangs);
    return langs.length > 0;
}

let src;

for (const candidate of FULL_REPO_CANDIDATES) {
    if (fs.existsSync(candidate) && validate(candidate)) {
        src = candidate;
        console.log(`Using full translations repo: ${src}`);
        break;
    }
}

if (!src) {
    if (fs.existsSync(TEST_DATA) && validate(TEST_DATA)) {
        src = TEST_DATA;
        console.log(`Full repo not found, using TestData fallback: ${src}`);
    } else {
        console.error('ERROR: No translations source found.');
        for (const candidate of FULL_REPO_CANDIDATES) {
            console.error(`  Looked for: ${candidate}`);
        }
        console.error(`  Fallback:   ${TEST_DATA}`);
        process.exit(1);
    }
}

const langCount = fs.readdirSync(path.join(src, 'natural-languages')).length;
console.log(`Copying ${langCount} language(s) to ${DEST}`);
copyDir(src, DEST);
console.log('Done.');
