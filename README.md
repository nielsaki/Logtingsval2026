# Løgtingsval

Unofficial Mac / Windows desktop app for **Løgtingsval** results from [KVF](https://kvf.fo/lv26). Not a KVF product.

You need the [.NET 10 SDK](https://dotnet.microsoft.com/download) installed.

---

### Step 1 — Download

Download this repository as a ZIP from GitHub and unzip it, **or** clone it with Git. Open the folder so it contains `Logtingsval2026.sln` and the `Logtingsval2026` project folder.

---

### Step 2 — Run in the terminal

**Mac (Terminal):** go to the project folder, then:

```bash
chmod +x lav-klar-til-deling.sh packaging/macos/mk-icns.sh
./lav-klar-til-deling.sh
```

**Windows (PowerShell or Command Prompt):** go to the project folder, then:

```powershell
dotnet publish Logtingsval2026\Logtingsval2026.csproj -c Release -r win-x64 --self-contained true -o dist\publish-win
```

---

### Step 3 — App on the desktop

**Mac:** you get `Logtingsval2026.app` inside the `dist` folder. Drag it to your Desktop (or Applications). First time macOS may block it — right‑click the app → **Open** → **Open**.

**Windows:** copy the whole folder `dist\publish-win` to your Desktop (all files must stay together). Double‑click `Logtingsval2026.exe` inside that folder.
