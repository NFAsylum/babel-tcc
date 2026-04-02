#!/usr/bin/env python3
"""Validates translation JSON files for consistency.

Checks that:
- Every keywords-base.json is valid JSON with a 'keywords' dict
- Every translation file has a 'translations' dict
- Every keyword ID in the base has a corresponding translation
- No orphan IDs exist in translation files
"""

import json
import os
import sys

TRANSLATIONS_DIRS = [
    os.path.join("packages", "core", "MultiLingualCode.Core.Tests", "TestData", "translations"),
]

# Also check the sibling translations repo if it exists
SIBLING_REPO = os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))),
                            "..", "babel-tcc-translations")


def find_translations_roots():
    roots = []
    for d in TRANSLATIONS_DIRS:
        if os.path.isdir(d):
            roots.append(d)
    if os.path.isdir(SIBLING_REPO):
        roots.append(SIBLING_REPO)
    return roots


def validate_root(root):
    errors = []
    prog_dir = os.path.join(root, "programming-languages")
    nat_dir = os.path.join(root, "natural-languages")

    if not os.path.isdir(prog_dir):
        errors.append(f"Missing programming-languages directory in {root}")
        return errors

    for lang in os.listdir(prog_dir):
        base_path = os.path.join(prog_dir, lang, "keywords-base.json")
        if not os.path.isfile(base_path):
            continue

        try:
            with open(base_path, "r", encoding="utf-8") as f:
                base = json.load(f)
        except json.JSONDecodeError as e:
            errors.append(f"Invalid JSON in {base_path}: {e}")
            continue

        if "keywords" not in base:
            errors.append(f"Missing 'keywords' key in {base_path}")
            continue

        keyword_ids = set(str(v) for v in base["keywords"].values())

        if not os.path.isdir(nat_dir):
            continue

        for nat_lang in os.listdir(nat_dir):
            trans_path = os.path.join(nat_dir, nat_lang, f"{lang}.json")
            if not os.path.isfile(trans_path):
                continue

            try:
                with open(trans_path, "r", encoding="utf-8") as f:
                    trans = json.load(f)
            except json.JSONDecodeError as e:
                errors.append(f"Invalid JSON in {trans_path}: {e}")
                continue

            if "translations" not in trans:
                errors.append(f"Missing 'translations' key in {trans_path}")
                continue

            trans_ids = set(trans["translations"].keys())
            missing = keyword_ids - trans_ids
            orphan = trans_ids - keyword_ids

            # Missing translations are warnings (not errors) because translations
            # may be work-in-progress. Orphan IDs are errors because they indicate
            # stale entries that reference non-existent keywords.
            if missing:
                print(
                    f"  WARNING: {trans_path}: missing translations for IDs: {sorted(missing)}"
                )
            if orphan:
                errors.append(
                    f"{trans_path}: orphan translation IDs (not in base): {sorted(orphan)}"
                )

    return errors


def main():
    roots = find_translations_roots()
    if not roots:
        print("WARNING: No translation directories found. Skipping validation.")
        return 0

    all_errors = []
    for root in roots:
        print(f"Validating translations in: {root}")
        errors = validate_root(root)
        all_errors.extend(errors)

    if all_errors:
        print(f"\n{len(all_errors)} validation error(s) found:")
        for err in all_errors:
            print(f"  ERROR: {err}")
        return 1

    print("All translation files are valid.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
