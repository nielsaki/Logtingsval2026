#!/usr/bin/env bash
# Kør på modtagerens Mac (Terminal). Giver appen kør-rettighed og fjerne quarantine.
# Brúk: ./fix-app-paa-mac.sh  [sti/til/Logtingsval2026.app]
set -euo pipefail

APP="${1:-}"
if [[ -z "$APP" ]]; then
  echo "Brúk: $0 /sti/til/Logtingsval2026.app"
  echo "Ella: $0   (so skal tú standa í tí mapuni har .app liggur, og skriva:"
  echo "       $0 \"\$PWD/Logtingsval2026.app\")"
  exit 1
fi

if [[ ! -d "$APP" ]]; then
  echo "Finnur ikki: $APP"
  exit 1
fi

EXE="$APP/Contents/MacOS/Logtingsval2026"
if [[ ! -e "$EXE" ]]; then
  echo "Ógilda .app: finnur ikki $EXE (veit tú mappan er ov?)"
  exit 1
fi

echo "Stiller: xattr -cr …"
xattr -cr "$APP" 2>/dev/null || true
echo "Stiller: chmod +x …"
chmod +x "$EXE"
echo ""
echo "Liðugt. Rætt at opna: høgrimús á «$(basename "$APP")» → Opna → Opna."
