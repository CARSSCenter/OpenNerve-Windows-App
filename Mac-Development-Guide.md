# Mac Development Guide — ONrecorder (OpenNerve Windows App)

Build, run, and test ONrecorder from an Apple Silicon Mac (M1, M2, M3, M4).

> Verified on: MacBook (M3), macOS ___, date ___

---

## What you'll end up with

- A Windows 11 virtual machine (VM) running on your Mac.
- ONrecorder running live inside that VM, from Visual Studio and as an `.exe`.
- After that: .NET tools on your Mac for quick tests of code that has no UI.

**Why the VM comes first:** ONrecorder is a Windows-only app (`net8.0-windows`, x86). Its window can't open on macOS. So the VM is the first thing that can block you. Get it working before anything else.

## The pipeline

1️⃣ 🍺 INSTALL — Get the Mac apps you need for the VM.

⬇️

2️⃣ 🪟 CREATE — Make the Windows 11 ARM VM.

⬇️

3️⃣ 🛠️ INSTALL — Add Visual Studio and Git inside Windows.

⬇️

4️⃣ 📥 CLONE — Download the code inside Windows.

⬇️

5️⃣ 🚀 RUN — Start ONrecorder live. *Milestone: it's live!*

⬇️

6️⃣ 💻 INSTALL — Add .NET tools on your Mac.

⬇️

7️⃣ 🧪 TEST — Unit-test non-UI code on your Mac.

⬇️

8️⃣ 🔍 CHECK — (Optional) Compile the whole app on your Mac.

---

## If a step fails

1. **Stop.** Don't change anything yet.
2. **Copy** the exact error text, or take a screenshot.
3. **Share** it (in your issue or PR) before trying a fix.

---

## Part 1 — Get it live in the VM

### Step 1: Install the Mac apps for the VM

Open **Terminal** and paste:

```bash
# Homebrew (skip if `brew -v` already works)
/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"

# UTM: free VM app for Apple Silicon
brew install --cask utm

# CrystalFetch: downloads the Windows 11 ARM installer from Microsoft
brew install --cask crystalfetch
```

✅ **Passed if:** UTM and CrystalFetch both open from your Applications folder.

### Step 2: Create the Windows 11 ARM VM

1. Open **CrystalFetch**. Download **Windows 11, ARM64**.
2. Open **UTM**. Click **Create → Virtualize → Windows**.
3. Select the file CrystalFetch downloaded.
4. Give the VM at least **8 GB RAM**, **4 CPU cores**, and **80 GB disk**.
5. Start the VM and finish Windows setup.
6. When asked for a product key, choose **"I don't have a product key."** That's fine for development.
7. Inside Windows, install the **UTM guest tools** when prompted. They add clipboard sharing and window resizing.

✅ **Passed if:** Windows reaches the desktop, and the screen resizes when you resize the UTM window.

### Step 3: Install Visual Studio and Git in Windows

Inside Windows, open **PowerShell** and paste:

```powershell
winget install Microsoft.VisualStudio.2022.Community --override "--add Microsoft.VisualStudio.Workload.ManagedDesktop --includeRecommended --passive"
winget install Git.Git
```

This can take a while. Let it finish.

✅ **Passed if:** Visual Studio 2022 opens, and `git --version` works in a **new** PowerShell window.

### Step 4: Clone the code inside Windows

Clone **inside Windows**, not into a shared folder from your Mac. UTM shared folders show up as a network drive. That makes builds slow and can confuse Visual Studio.

```powershell
mkdir C:\src; cd C:\src
git clone https://github.com/<your-username>/OpenNerve-Windows-App.git
```

Replace `<your-username>` with your GitHub username (use your fork).

To move changes between your Mac and the VM, use `git push` and `git pull` through your fork.

✅ **Passed if:** the file `C:\src\OpenNerve-Windows-App\ONrecorder.sln` exists.

### Step 5: Run ONrecorder live 🚀

1. Open `ONrecorder.sln` in Visual Studio.
2. Set the configuration to **Debug** and the platform to **x86**.
3. Wait for NuGet to restore packages.
4. Press **F5**.

✅ **Passed if:** the ONrecorder window opens.

Now try the standalone `.exe`:

1. Open File Explorer.
2. Go to `bin\x86\Debug\net8.0-windows10.0.26100.0\win-x86\` inside the repo. (The exact folder may differ slightly.)
3. Double-click the `.exe`.

✅ **Passed if:** the `.exe` opens outside Visual Studio.

🎉 **It's live.** Everything below is optional speed-ups on your Mac.

---

## Part 2 — Mac tools (only after the VM is live)

### Step 6: Install .NET and VS Code on your Mac

Back on your Mac, open **Terminal** and paste:

```bash
# .NET 8 SDK, using Microsoft's official script
curl -sSL https://dot.net/v1/dotnet-install.sh | bash -s -- --channel 8.0
echo 'export DOTNET_ROOT="$HOME/.dotnet"; export PATH="$HOME/.dotnet:$PATH"' >> ~/.zshrc
source ~/.zshrc

# VS Code and the C# Dev Kit
brew install --cask visual-studio-code
code --install-extension ms-dotnettools.csdevkit
```

Why the script and not `brew install dotnet-sdk`? Homebrew installs the newest .NET. Test projects that target .NET 8 need the 8.0 runtime.

✅ **Passed if:** `dotnet --list-sdks` shows a line starting with `8.0`.

### Step 7: Unit-test non-UI code on your Mac

You can test code that doesn't touch the UI right on your Mac. Examples: parsers, validators, and `TraceListener` classes.

1. Put the logic in its own `.cs` file.
2. Make a test project from the repo root:

```bash
dotnet new xunit -n ONrecorder.Tests -f net8.0
cd ONrecorder.Tests
```

3. In `ONrecorder.Tests.csproj`, link the one file you want to test. (Don't reference `ONrecorder.csproj`. It's Windows-only.)

```xml
<ItemGroup>
  <Compile Include="../YourClass.cs" Link="YourClass.cs" />
</ItemGroup>
```

4. Run the tests:

```bash
dotnet test
```

✅ **Passed if:** `dotnet test` says all tests passed.

> Keep this test project out of `ONrecorder.sln` until maintainers agree on where tests should live.

### Step 8 (optional): Compile the whole app on your Mac

This checks for compile errors without opening the VM. It does **not** run the app.

```bash
dotnet build ONrecorder.csproj -p:EnableWindowsTargeting=true -p:Platform=x86
```

✅ **Passed if:** the output ends with `Build succeeded`.

If it fails on the `Microsoft.WindowsAppSDK` package, skip this step. The VM covers it.

---

## Gotchas

- **Debug messages only exist in Debug builds.** `Debug.WriteLine` calls are removed from Release builds. If you're testing anything that uses debug messages, build in **Debug**, including the `.exe`.
- **Run the `.exe` from its build folder.** The app finds files (like `secrets\`) by going four folders up from the `.exe`. A copied or moved `.exe` will look in the wrong place. See `Developer-Guide.md` §1.
- **No Bluetooth in the VM.** The VM can't use your Mac's Bluetooth, so connecting to an IPG over BLE won't work there. UI work and anything that doesn't need a live device is fine.
- **USB to the board is untested.** UTM can pass a USB device into the VM (the **USB** button in UTM's toolbar). That might let the USB connection in `USB_COMMUNICATION.md` work. Nobody has confirmed it yet.
- **Admin key.** Admin-only features need `secrets\admin_priv_d_hex.txt`. See `Developer-Guide.md` §1. You don't need it for regular UI work.
- **Intel Mac?** Use a regular x64 Windows 11 installer in UTM instead of ARM64. Everything else is the same.
