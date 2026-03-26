#!/usr/bin/env bash
# Ger AppIcon.icns úr PNG (krývur macOS sips + iconutil).
set -euo pipefail
[[ $# -eq 2 ]] || { echo "Brúkara: $0 út.icns inn.png"; exit 1; }
OUT_ICNS="$1"
SRC_PNG="$2"
[[ -f "$SRC_PNG" ]] || { echo "Finnur ikki: $SRC_PNG"; exit 1; }

TMP=$(mktemp -d)
trap 'rm -rf "$TMP"' EXIT
S="$TMP/AppIcon.iconset"
mkdir "$S"

sips -z 16 16 "$SRC_PNG" --out "$S/icon_16x16.png" >/dev/null
sips -z 32 32 "$SRC_PNG" --out "$S/icon_16x16@2x.png" >/dev/null
sips -z 32 32 "$SRC_PNG" --out "$S/icon_32x32.png" >/dev/null
sips -z 64 64 "$SRC_PNG" --out "$S/icon_32x32@2x.png" >/dev/null
sips -z 128 128 "$SRC_PNG" --out "$S/icon_128x128.png" >/dev/null
sips -z 256 256 "$SRC_PNG" --out "$S/icon_128x128@2x.png" >/dev/null
sips -z 256 256 "$SRC_PNG" --out "$S/icon_256x256.png" >/dev/null
sips -z 512 512 "$SRC_PNG" --out "$S/icon_256x256@2x.png" >/dev/null
sips -z 512 512 "$SRC_PNG" --out "$S/icon_512x512.png" >/dev/null
sips -z 1024 1024 "$SRC_PNG" --out "$S/icon_512x512@2x.png" >/dev/null

iconutil --convert icns --output "$OUT_ICNS" "$S"
