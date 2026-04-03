const fs = require('fs');
const path = require('path');

const VSCODE_DIR = path.resolve(__dirname, '..', 'packages', 'ide-adapters', 'vscode');
const DEST = path.join(VSCODE_DIR, 'translations');
const FULL_REPO = path.resolve(__dirname, '..', '..', 'babel-tcc-translations');
const TEST_DATA = path.resolve(__dirname, '..', 'packages', 'core', 'MultiLingualCode.Core.Tests', 'TestData', 'translations');

function copyDir(src, dest) {
    fs.mkdirSync(dest, { recursive: true });
    for (const entry of fs.readdirSync(src)) {
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

if (fs.existsSync(FULL_REPO) && validate(FULL_REPO)) {
    src = FULL_REPO;
    console.log(`Using full translations repo: ${src}`);
} else if (fs.existsSync(TEST_DATA) && validate(TEST_DATA)) {
    src = TEST_DATA;
    console.log(`Full repo not found, using TestData fallback: ${src}`);
} else {
    console.error('ERROR: No translations source found.');
    console.error(`  Looked for: ${FULL_REPO}`);
    console.error(`  Fallback:   ${TEST_DATA}`);
    process.exit(1);
}

const langCount = fs.readdirSync(path.join(src, 'natural-languages')).length;
console.log(`Copying ${langCount} language(s) to ${DEST}`);
copyDir(src, DEST);
console.log('Done.');
