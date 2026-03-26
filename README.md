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
2. **Publish a release** that includes **`Logtingsval2026-til-mac-Apple-Silicon.zip`** (e.g. create a **Release** on GitHub and attach that file from `dist/til-deling/` after running `./lav-klar-til-deling.sh` on a Mac). Until a release exists, the “latest” download link on GitHub returns 404 — the site’s download button also needs that release.

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

### Easiest way to ship the app (no GitHub Actions required)

1. **Pages (once):** **Settings → Pages →** branch **`main`**, folder **`/docs`**. Site: `https://nielsaki.github.io/Logtingsval2026/`
2. On your Mac, build the ZIP (needs [.NET SDK](https://dotnet.microsoft.com/download), e.g. **.NET 10**):

```bash
chmod +x lav-klar-til-deling.sh packaging/macos/mk-icns.sh
./lav-klar-til-deling.sh
```

3. **GitHub → Releases → Create a new release**, tag e.g. `v1.0.0`, and **attach** `dist/til-deling/Logtingsval2026-til-mac-Apple-Silicon.zip`.

After that, the download page and the “latest” ZIP link work for everyone — **no clone, no build for them.**

Custom domain: in `docs/index.html`, keep `REPO_OVERRIDE = 'nielsaki/Logtingsval2026'`.

### Optional: GitHub Actions (build ZIP on GitHub)

The workflow YAML lives in **`packaging/ci/release-macos.yml`** (not under `.github/` in this repo), so **`git push` does not need the PAT `workflow` scope.** To enable Actions once: on GitHub **Add file → Create new file**, path **`.github/workflows/release-macos.yml`**, paste that file’s contents, commit to **`main`**. Then use **Actions → Run workflow** or push a `v*` tag.

### macOS: Git never asks for a password

An old token is stored in **Keychain**. To be prompted again after you create a new PAT:

```bash
printf "protocol=https\nhost=github.com\n\n" | git credential-osxkeychain erase
```

Next `git push` over **HTTPS** will ask for credentials (username + PAT). To use HTTPS instead of SSH:  
`git remote set-url origin https://github.com/nielsaki/Logtingsval2026.git`  
If you keep **SSH** and see `Permission denied (publickey)`, add your key to the agent (`ssh-add --apple-use-keychain ~/.ssh/id_ed25519`) or follow [GitHub SSH](https://docs.github.com/en/authentication/connecting-to-github-with-ssh).

**`.app` only:** `./build-macos-app.sh` → `dist/Logtingsval2026.app`.

**Git:** Initialise git only inside this project folder, not your home directory.

**Gatekeeper / “can’t open” (reference):** `packaging/macos/HVIS-APPEN-IKKE-KAN-AABNES.txt`, `fix-app-paa-mac.sh`.

</details>
