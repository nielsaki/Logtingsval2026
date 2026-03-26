# Løgtingsval

**Download page (open this address in the browser):**  
**https://nielsaki.github.io/Logtingsval2026/**

Use the line above **exactly** — it must end with **`/Logtingsval2026/`**.  
Opening only `https://nielsaki.github.io` (with nothing after it) shows **“There isn’t a GitHub Pages site here”** and is the wrong place.

You get a **ZIP** — open it, put **`Logtingsval2026.app`** where you like, and run it. **Nothing to install** (no .NET, no Terminal, no build). The app is **self-contained**.

**Direct ZIP** (works once a release exists with this file name):  
https://github.com/nielsaki/Logtingsval2026/releases/latest/download/Logtingsval2026-til-mac-Apple-Silicon.zip

### If the page is still 404, or the ZIP link fails

Whoever owns the repository must do **both** once:

1. **Turn on GitHub Pages:** repo **Settings → Pages → Build and deployment →** source **Deploy from a branch**, branch **`main`**, folder **`/docs`**, then **Save**. Wait a minute and open **https://nielsaki.github.io/Logtingsval2026/** again.
2. **Publish a release** that includes **`Logtingsval2026-til-mac-Apple-Silicon.zip`** (e.g. create a **Release** on GitHub and attach that file from `dist/til-deling/` after running `./lav-klar-til-deling.sh` on a Mac). Until a release exists, the “latest\" download link on GitHub returns 404 — the site’s download button also needs that release.

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
