# Dagfør (Logtingsval2026)

Avalonia-app til **løgtingsval**-úrslit frá [**KVF**](https://kvf.fo/lv26): flokkar, bogi, vald persónar, topskorarar, meirilutasamslög.  
**Kun macOS við Apple Silicon** (M1–M4 …); appin er *self-contained* — brúkarin skal **ikki** seta .NET upp.

---

## Eitt niðurlagsleinkja (til luttakarar)

Tá tú brúkar **GitHub Releases**, er breiða leinkjan til **seinastu** útgávu (skift `DITBRUGERNAVN` og `DITREPO`):

```text
https://github.com/DITBRUGERNAVN/DITREPO/releases/latest/download/Logtingsval2026-til-mac-Apple-Silicon.zip
```

**Tí skalt tú gera:**
1. Skuffa kelduna á GitHub (sjá niðanfyri).
2. **Actions → “Build macOS (Apple Silicon)” → Run workflow** (ella skuffa git *tag*, t.d. `v1.0.0`).
3. Á **Releases**: set inn eina frámerking við ZIP-fílinum `Logtingsval2026-til-mac-Apple-Silicon.zip` (GitHub Actions kann gera tað sjálv tá tú skuffar tag – sjá workflow).
4. Kopiera URL’in í `releases/latest/download/...` og geva honum til familju/felag.

Við **LAES-MIG.txt** og `fix-app-paa-mac.sh` í `dist/til-deling/` eftir `./lav-klar-til-deling.sh`.

---

## Byggja og pakka (á tínum Mac)

Krav: [.NET SDK](https://dotnet.microsoft.com/download) (mit verður brúkt .NET 10).

```bash
chmod +x lav-klar-til-deling.sh packaging/macos/mk-icns.sh
./lav-klar-til-deling.sh
```

Útkoman liggur í **`dist/til-deling/`** (ZIP + vegleiðing).

Bert `.app` (utan heila “til-deling”-mappu):

```bash
chmod +x build-macos-app.sh
./build-macos-app.sh
# → dist/Logtingsval2026.app
```

---

## Git + GitHub (keldikot við *bert* hendan mappuna)

Um `git status` vísir heila tín **heimamappu**, er `git init` rakst í ringum stað. Ger so:

```bash
cd ~/Desktop/logtingsval2026
git init
git add .
git commit -m "Initial commit: Dagfør"
```

Á **github.com**: new repository **utan** README · kopiera URL.

```bash
git remote add origin https://github.com/DITBRUGERNAVN/DITREPO.git
git branch -M main
git push -u origin main
```

---

## Automatiskur build (GitHub Actions)

Workflow **`Build macOS (Apple Silicon)`** (`.github/workflows/release-macos.yml`):

- **Manuelt:** *Actions* → vel workflow → *Run workflow* → niðurheinta **Artifacts**.
- **Við tag:** skuffa `v1.0.0` (ella `v*`); workflow byggir og kann seta ZIP á Release (við `contents: write` og `softprops/action-gh-release`).

---

## Trupulleikar við at opna appina (Gatekeeper)

Skrivað í **`packaging/macos/HVIS-APPEN-IKKE-KAN-AABNES.txt`** og **`fix-app-paa-mac.sh`**.

---

## Loyvir / kelda

Úrslit frá KVF (`valurslit/lv2026`).  
Hendan keldan er fyri lokal útvals og familju; ikki KVF sín ræði ting.
