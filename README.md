# Løgtingsval

Mac app (Apple Silicon) for **Løgtingsval** results from [KVF](https://kvf.fo/lv26). Unofficial, not a KVF product.

## Download

**One page, one button:** https://nielsaki.github.io/Logtingsval2026/

You get **one ZIP** — **only the app** inside (`Logtingsval2026.app`). It is a large download because the .NET runtime is bundled; there is **no source code** and **no extra project files** in that ZIP (same file you’d get from `dist/til-deling/` or your Desktop after `./lav-klar-til-deling.sh`).

**Note:** The button uses the copy you **upload to GitHub Releases** (step 3). GitHub stores that file; visitors never clone the repo.

**First time macOS blocks the app:** Right‑click the app → **Open** → **Open**.

---

## You: set this up once (3 steps)

1. **Pages:** Repo **Settings → Pages →** source **Deploy from a branch**, branch **`main`**, folder **`/docs`**, Save.  
2. **Build the ZIP on your Mac** (needs [.NET 10 SDK](https://dotnet.microsoft.com/download)):
   ```bash
   chmod +x lav-klar-til-deling.sh packaging/macos/mk-icns.sh && ./lav-klar-til-deling.sh
   ```
   ZIP path: **`dist/til-deling/Logtingsval2026-til-mac-Apple-Silicon.zip`** (or copy from Desktop if you built there).
3. **Releases → Create a new release:** choose a tag (e.g. `v1.0.0`), attach that ZIP **with this exact filename**, publish.

After that, the download page button works for everyone.

Optional automation: add **`packaging/ci/release-macos.yml`** on GitHub as **`.github/workflows/release-macos.yml`** (create the file in the browser).
