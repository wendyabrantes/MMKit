#!/usr/bin/env python3
"""
Rebuilds the kit's trimmed Chinese / Japanese / Korean fonts from Noto Sans.

Nunito has no CJK characters, so the kit falls back to Noto Sans for ja / ko / zh-Hans. The full fonts are 10-18 MB
each; this keeps only the characters the kit's translations use (Runtime/KitStrings.cs) plus printable ASCII,
in SemiBold (600, normal text) and ExtraBold (800, bold text) to match Nunito.

Run it again whenever a ja / ko / zh-Hans string in KitStrings.cs changes, or new characters show as boxes.

    python3 -m venv /tmp/fontenv && /tmp/fontenv/bin/pip install fonttools
    # download the variable fonts (OFL) from https://github.com/google/fonts/tree/main/ofl :
    #   notosansjp/NotoSansJP[wght].ttf, notosanskr/NotoSansKR[wght].ttf, notosanssc/NotoSansSC[wght].ttf
    /tmp/fontenv/bin/python subset_cjk_fonts.py <folder with those three files>
"""
import os
import re
import sys

from fontTools import subset
from fontTools.ttLib import TTFont
from fontTools.varLib import instancer

HERE = os.path.dirname(os.path.abspath(__file__))
RUNTIME = os.path.join(HERE, "..", "Runtime")
OUT = os.path.join(RUNTIME, "Resources", "MMKit", "Fonts")

FONTS = {  # kit language -> Noto family
    "ja": "NotoSansJP",
    "ko": "NotoSansKR",
    "zh-Hans": "NotoSansSC",
}
WEIGHTS = {"SemiBold": 600, "ExtraBold": 800}


def strings_by_language():
    src = open(os.path.join(RUNTIME, "KitStrings.cs"), encoding="utf-8").read()
    pair = re.compile(r'\{ "([^"]+)", "((?:[^"\\]|\\.)*)" \}')
    unescape = lambda s: re.sub(r"\\u([0-9a-fA-F]{4})", lambda m: chr(int(m.group(1), 16)), s)
    tables = {}
    for m in re.finditer(r'\["([a-zA-Z-]+)"\] = new Dictionary<string, string>\s*\{(.*?)\n            \},', src, re.S):
        tables[m.group(1)] = [unescape(v) for _, v in pair.findall(m.group(2))]
    return tables


def find_source(folder, family):
    for name in os.listdir(folder):
        if name.startswith(family) and name.endswith(".ttf"):
            return os.path.join(folder, name)
    sys.exit(f"No {family}*.ttf in {folder}")


def main():
    if len(sys.argv) != 2:
        sys.exit(__doc__)
    folder = sys.argv[1]
    tables = strings_by_language()
    ascii_chars = "".join(chr(c) for c in range(0x20, 0x7F))

    for lang, family in FONTS.items():
        if lang not in tables:
            sys.exit(f"KitStrings.cs has no '{lang}' table")
        text = "".join(tables[lang]) + ascii_chars + "… "
        source = find_source(folder, family)
        for style, weight in WEIGHTS.items():
            font = instancer.instantiateVariableFont(TTFont(source), {"wght": weight}, updateFontNames=True)
            options = subset.Options()
            options.layout_features = ["*"]
            options.name_IDs = ["*"]
            options.notdef_outline = True
            sub = subset.Subsetter(options)
            sub.populate(text=text)
            sub.subset(font)
            path = os.path.join(OUT, f"{family}-{style}.ttf")
            font.save(path)
            cmap = font.getBestCmap()
            missing = sorted({c for c in "".join(tables[lang]) if ord(c) not in cmap and c not in "  "})
            print(f"{os.path.basename(path)}: {len(cmap)} chars, {os.path.getsize(path) // 1024} KB"
                  + (f", MISSING {''.join(missing)}" if missing else ""))


if __name__ == "__main__":
    main()
