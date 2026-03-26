#!/usr/bin/env bash
# Bygger Logtingsval2026.app — dobbeltklik i Finder for at starte (ingen Terminal).
set -euo pipefail

ROOT="$(cd "$(dirname "$0")" && pwd)"
PROJ="$ROOT/Logtingsval2026/Logtingsval2026.csproj"
PLIST="$ROOT/packaging/macos/Info.plist"
ARCH="$(uname -m)"
case "$ARCH" in
  arm64) RID=osx-arm64 ;;
  x86_64) RID=osx-x64 ;;
  *) echo "Ukendt arkitektur: $ARCH (forventet arm64 eller x86_64)"; exit 1 ;;
esac

DIST="$ROOT/dist"
PUBLISH="$DIST/publish-$RID"
APP="$DIST/Logtingsval2026.app"
EXE_NAME="Logtingsval2026"

echo "Publicerer ($RID, self-contained) …"
dotnet publish "$PROJ" -c Release -r "$RID" --self-contained true \
  -p:PublishSingleFile=false \
  -o "$PUBLISH"

echo "Samansetur .app …"
rm -rf "$APP"
mkdir -p "$APP/Contents/MacOS"
cp -R "$PUBLISH"/* "$APP/Contents/MacOS/"
chmod +x "$APP/Contents/MacOS/$EXE_NAME"
cp "$PLIST" "$APP/Contents/Info.plist"

ICON_PNG="$ROOT/Logtingsval2026/Assets/app-logo.png"
if [[ -f "$ICON_PNG" ]] && command -v iconutil >/dev/null 2>&1; then
  mkdir -p "$APP/Contents/Resources"
  "$ROOT/packaging/macos/mk-icns.sh" "$APP/Contents/Resources/AppIcon.icns" "$ICON_PNG" || true
fi

# Fjern quarantine/metadata der kan forhindre start fra Finder efter kopiering/zip.
xattr -cr "$APP" 2>/dev/null || true

echo ""
echo "Liðugt: $APP"
echo "Opna í Finder og tvítrýst á «Logtingsval2026.app»."
echo "Um macOS steðgar (óútskrivað app): høgrimús → Opna ella í Terminal: xattr -cr \"$APP\""
