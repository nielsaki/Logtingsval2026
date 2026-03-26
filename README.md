# Løgtingsval

**[Download for Mac (Apple Silicon)](https://nielsaki.github.io/Logtingsval2026/)**

You get a **ZIP** — open it, put **`Logtingsval2026.app`** where you like, and run it. **Nothing to install** (no .NET, no Terminal, no build). The app is **self-contained**.

**Direct link** (same file as the button above):  
https://github.com/nielsaki/Logtingsval2026/releases/latest/download/Logtingsval2026-til-mac-Apple-Silicon.zip

### First time on macOS

If the Mac says the app can’t be checked for malware: **Right‑click the app → Open → Open** once. If it still fails, the ZIP from the release includes a short help file (`LAES-MIG.txt`) with steps — read that on the Mac, not the GitHub project page.

### In the app

Faroese election results from [**KVF**](https://kvf.fo/lv26): parties, mandate arc, candidates, coalitions. **Dagfør** means **update** (refresh data). The window title is **Løgtingsval**.

---

Unofficial viewer for personal use; **not** an official KVF product. Data: KVF `valurslit/lv2026`.

---

<details>
<summary><strong>Maintainers only</strong> — repository, Pages, releases, build</summary>

This section is for people updating the app or the website. **End users do not need any of this.**

The GitHub repository holds **source code**. Sharing the app publicly is done with the **download page** (`docs/`, GitHub Pages) and **Releases** (ZIP asset `Logtingsval2026-til-mac-Apple-Silicon.zip`).

**Pages (once):** Repository **Settings → Pages →** branch **`main`**, folder **`/docs`**. Site: `https://nielsaki.github.io/Logtingsval2026/`  
Custom domain: in `docs/index.html`, set `REPO_OVERRIDE = 'nielsaki/Logtingsval2026'`.

**Attach the ZIP to a release:** Push a tag `v*` so Actions can run, or run the workflow manually and upload the artifact — see `.github/workflows/release-macos.yml` (pushing this file needs a HTTPS token with **`workflow`** scope).

**Build locally (maintainers):** [.NET SDK](https://dotnet.microsoft.com/download) (e.g. **.NET 10**). Then:

```bash
chmod +x lav-klar-til-deling.sh packaging/macos/mk-icns.sh
./lav-klar-til-deling.sh
```

Output: `dist/til-deling/` (ZIP + helper texts for recipients). Or `.app` only: `./build-macos-app.sh` → `dist/Logtingsval2026.app`.

**Git:** Initialise git only inside this project folder, not your home directory. Pushing workflow YAML may require PAT **`workflow`** scope.

**Gatekeeper / “can’t open” (reference):** `packaging/macos/HVIS-APPEN-IKKE-KAN-AABNES.txt`, `fix-app-paa-mac.sh`.

</details>
