#!/usr/bin/env bash
# Koyr hetta á TÍNI Mac (M1/M2/M3/M4 = arm64). Stovnar .app + zip + vegleiðing
# til at senda til onnur Mac við Apple Silicon — tær klør ikki at seta .NET upp.
set -euo pipefail

ROOT="$(cd "$(dirname "$0")" && pwd)"
PROJ="$ROOT/Logtingsval2026/Logtingsval2026.csproj"
PLIST="$ROOT/packaging/macos/Info.plist"
EXE_NAME="Logtingsval2026"

ARCH="$(uname -m)"
case "$ARCH" in
  arm64) RID_MAC=osx-arm64 ;;
  x86_64) RID_MAC=osx-x64 ;;
  *) echo "Ókent: $ARCH (búða við arm64 ella x86_64)"; exit 1 ;;
esac

DIST="$ROOT/dist"
DEL="$DIST/til-deling"
PUBLISH_MAC="$DIST/publish-$RID_MAC"
APP="$DIST/${EXE_NAME}.app"

echo "== 1/2 Byggir Mac-app ($RID_MAC) =="
dotnet publish "$PROJ" -c Release -r "$RID_MAC" --self-contained true \
  -p:PublishSingleFile=false -o "$PUBLISH_MAC"

echo "== Saml .app =="
rm -rf "$APP"
mkdir -p "$APP/Contents/MacOS"
cp -R "$PUBLISH_MAC"/* "$APP/Contents/MacOS/"
chmod +x "$APP/Contents/MacOS/$EXE_NAME"
cp "$PLIST" "$APP/Contents/Info.plist"
ICON_PNG="$ROOT/Logtingsval2026/Assets/app-logo.png"
if [[ -f "$ICON_PNG" ]] && command -v iconutil >/dev/null 2>&1; then
  mkdir -p "$APP/Contents/Resources"
  "$ROOT/packaging/macos/mk-icns.sh" "$APP/Contents/Resources/AppIcon.icns" "$ICON_PNG" || true
fi
xattr -cr "$APP" 2>/dev/null || true

echo "== 2/2 Zip + vegleiðing (send hetta) =="
rm -rf "$DEL"
mkdir -p "$DEL"

ZIP_MAC="$DEL/${EXE_NAME}-til-mac-Apple-Silicon.zip"
ditto -c -k --keepParent "$APP" "$ZIP_MAC"

cat > "$DEL/LAES-MIG.txt" << 'EOF'
SEND KUN TIL MAC MED APPLE SILICON (M1, M2, M3, M4 …)
Appen indeholder alt — der skal ikke installeres .NET.

1) Gem zip-filen og dobbeltklik den (eller Mail/iCloud udpakker automatisk).

2) Læg mappen "Logtingsval2026.app" på Skrivebord eller i Programmer.

3) Første gang du åbner den:
   Åbn Terminal (Søg efter "Terminal") og kør (ret stien hvis appen ligger et andet sted):

   xattr -cr ~/Desktop/Logtingsval2026.app
   chmod +x ~/Desktop/Logtingsval2026.app/Contents/MacOS/Logtingsval2026

   (Træk appen ind i Terminal efter kommandoen, hvis du ikke vil skrive stien.)

4) Højreklik på Logtingsval2026.app → Åbn → Åbn.
   Brug ikke almindelig dobbeltklik første gang.

5) VIRKER DET IKKE? Læs filen HVIS-APPEN-IKKE-KAN-AABNES.txt
   eller kør:  ./fix-app-paa-mac.sh  og træk appen ind efter kommandoen.

VIGTIGT: Send som .zip (ikke .tar) — ellers kan macOS ødelægge appen.
EOF

cp "$ROOT/packaging/macos/HVIS-APPEN-IKKE-KAN-AABNES.txt" "$DEL/"
cp "$ROOT/fix-app-paa-mac.sh" "$DEL/"
chmod +x "$DEL/fix-app-paa-mac.sh"

echo ""
echo "Liðugt. Send innihaldið í:"
echo "  $DEL"
echo ""
echo "  • $ZIP_MAC  (hetta skal hon fáa)"
echo "  • LAES-MIG.txt + fix-app-paa-mac.sh + HVIS-APPEN-IKKE-KAN-AABNES.txt"
echo ""
ls -lh "$DEL"
