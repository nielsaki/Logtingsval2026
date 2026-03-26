# Logtingsval2026

Cross‑platform (**macOS today**) Avalonia desktop app for **Faroese parliamentary election** (***Løgtingsval***) results from [**KVF**](https://kvf.fo/lv26): parties, mandate arc, elected candidates, top scorers by personal votes, and majority coalitions.

In the Faroese UI, **Dagfør** means **update** and is used only for refreshing data (manual **Dagfør** button and **Dagfør automatically every 5 seconds**). The app and window name is **Løgtingsval**.

The shipped macOS build is **Apple Silicon** (M1–M4, …), **self-contained** — recipients **do not** install the .NET runtime.

**End users should not clone this repository.** They only need the ZIP from a release or the small download page below.

---

## Download page (one button for everyone)

The folder **`docs/`** is a minimal static site: one page with a **Download** button that points at the latest release asset **`Logtingsval2026-til-mac-Apple-Silicon.zip`** (via the GitHub API). Visitors never see the rest of the repo unless they follow “source” links.

**Turn it on (once per repository)**

1. On GitHub: **Settings → Pages**.
2. **Build and deployment**: source **Deploy from a branch**, branch **`main`**, folder **`/docs`**, Save.
3. After a minute, the site is at **`https://YOUR_USER.github.io/YOUR_REPO/`** (replace with your GitHub username and repository name).

**Custom domain:** In **`docs/index.html`**, set `REPO_OVERRIDE = 'YOUR_USER/YOUR_REPO'` so the button still finds releases.

---

## Direct download link (optional)

After a **GitHub Release** includes **`Logtingsval2026-til-mac-Apple-Silicon.zip`**, this URL always serves that file from the latest release:

```text
https://github.com/YOUR_USER/YOUR_REPO/releases/latest/download/Logtingsval2026-til-mac-Apple-Silicon.zip
```

**How to publish that ZIP on the release**

1. Push this repo to GitHub (see below).
2. Run **Actions → “Build macOS (Apple Silicon)” → Run workflow** and download the **artifact**, *or* push a version tag (e.g. `git tag v1.0.0 && git push origin v1.0.0`) so the workflow can attach the ZIP to a **Release**.
3. Ensure the release includes **`Logtingsval2026-til-mac-Apple-Silicon.zip`** with that exact name.

Bundled instructions for recipients are produced under **`dist/til-deling/`** when you run `./lav-klar-til-deling.sh` locally (see **`LAES-MIG.txt`**, **`fix-app-paa-mac.sh`**, **`HVIS-APPEN-IKKE-KAN-AABNES.txt`**). That folder is gitignored — only the **release ZIP** and the **GitHub Pages** download page are meant for sharing.

---

## Build and package (on your Mac)

Requirements: [.NET SDK](https://dotnet.microsoft.com/download) matching the project (e.g. **.NET 10**).

**Full zip for sharing (recommended)**

```bash
chmod +x lav-klar-til-deling.sh packaging/macos/mk-icns.sh
./lav-klar-til-deling.sh
```

Output: **`dist/til-deling/`** (ZIP + helper files).

**`.app` bundle only**

```bash
chmod +x build-macos-app.sh
./build-macos-app.sh
# → dist/Logtingsval2026.app
```

---

## Git and GitHub (this folder only)

If `git status` lists your **entire home directory**, `git init` was run in the wrong place. Initialise the repo **only** inside this project:

```bash
cd ~/Desktop/logtingsval2026
git init
git add .
git commit -m "Initial commit"
```

On GitHub, create a **new empty** repository (no README), then:

```bash
git remote add origin https://github.com/YOUR_USER/YOUR_REPO.git
git branch -M main
git push -u origin main
```

If `origin` already exists:

```bash
git remote set-url origin https://github.com/YOUR_USER/YOUR_REPO.git
git push -u origin main
```

**Pushing workflow files** (`.github/workflows/…`) with HTTPS requires a Personal Access Token that includes the **`workflow`** scope (classic token) or equivalent permissions (fine‑grained token). Without it, GitHub rejects pushes that add or update workflow files.

---

## GitHub Actions

Workflow: **`.github/workflows/release-macos.yml`** (“Build macOS (Apple Silicon)”)

- **Manual run:** *Actions* → select the workflow → *Run workflow* → download the **Artifacts** ZIP.
- **Tag push** (`v*`): builds and can create a **Release** with the distributable ZIP (see workflow).

Runners use **macOS** (`macos-latest`), suitable for **`osx-arm64`** publish.

---

## macOS “can’t open the app” (Gatekeeper)

See **`packaging/macos/HVIS-APPEN-IKKE-KAN-AABNES.txt`** and **`fix-app-paa-mac.sh`**. Users often need **Right‑click → Open** once, and/or `xattr -cr` on the `.app`.

---

## Data / disclaimer

Election numbers come from KVF (`valurslit/lv2026`). This repository is an **unofficial** viewer for local / family use; it is **not** an official KVF product.
