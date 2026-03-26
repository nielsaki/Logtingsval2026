# Dagfør (Logtingsval2026)

Cross‑platform (**macOS today**) Avalonia desktop app for **Faroese parliamentary election** results from [**KVF**](https://kvf.fo/lv26): parties, mandate arc, elected candidates, top scorers by personal votes, and majority coalitions.

The shipped macOS build is **Apple Silicon** (M1–M4, …), **self-contained** — recipients **do not** install the .NET runtime.

---

## One download link (for users)

After you publish a **GitHub Release** that includes the file  
`Logtingsval2026-til-mac-Apple-Silicon.zip`, the stable “latest release” URL is:

```text
https://github.com/YOUR_USER/YOUR_REPO/releases/latest/download/Logtingsval2026-til-mac-Apple-Silicon.zip
```

Replace `YOUR_USER` and `YOUR_REPO` (e.g. `nielsaki` / `Logtingsval2026`).

**How to get that file on the release**

1. Push this repo to GitHub (see below).
2. Run **Actions → “Build macOS (Apple Silicon)” → Run workflow** and download the **artifact**, *or* create a version tag (e.g. `git tag v1.0.0 && git push origin v1.0.0`) so the workflow can attach the ZIP to a **Release**.
3. Ensure the published release includes **`Logtingsval2026-til-mac-Apple-Silicon.zip`** with that exact name so the `/releases/latest/download/...` link works.

Bundled instructions for recipients are produced under **`dist/til-deling/`** when you run `./lav-klar-til-deling.sh` (see **`LAES-MIG.txt`**, **`fix-app-paa-mac.sh`**, **`HVIS-APPEN-IKKE-KAN-AABNES.txt`**).

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
