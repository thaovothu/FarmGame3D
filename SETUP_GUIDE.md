# 🚀 Setup Guide

## Prerequisites

### Required Software
- **Unity Hub** 2.0 or later
- **Unity Editor** 2021.3 LTS or later
- **Visual Studio** 2019/2022 or **VS Code** with C# extension
- **.NET Standard 2.1** (included with Unity)

### Recommended
- **Git** for version control
- **Newtonsoft.Json** (will be auto-imported by Unity)

---

## Quick Start (5 minutes)

### 1. Clone/Download Project
```bash
git clone https://github.com/thaovothu/FarmGame3D.git

```

### 2. Open in Unity
1. Open **Unity Hub**
2. Click **"Add"** button
3. Navigate to project folder
4. Select folder (Unity will detect it)
5. Click **"Open"**

Unity will import all assets (may take 1-2 minutes on first open)

### 3. Run the Game
1. In Unity Editor, open scene: `Assets/Scenes/Menu.unity`
2. Press **Play** button (▶️) or press `Ctrl+P` (Windows) / `Cmd+P` (Mac)
3. Game will start with Console UI interface

---

## Project Configuration

### Config File Location
The main configuration file is at:
```
FarmGame/Config/game_config.csv
```

You can edit this file with any text editor to change game balance.

### Save File Location
Game saves are stored at:
- **Windows**: `C:\Users\[Username]\AppData\Roaming\FarmGame\savegame.json`
- **Mac**: `~/Library/Application Support/FarmGame/savegame.json`
- **Linux**: `~/.config/unity3d/FarmGame/savegame.json`

---

## Development Setup

### IDE Setup

#### Visual Studio
1. Open `FarmGame.sln` in Visual Studio
2. VS should automatically detect Unity project
3. Set up Unity debugger (optional):
   - Tools → Options → Cross Platform → Unity
   - Enable "Attach Unity" option

#### VS Code
1. Install **C# extension** (Microsoft)
2. Install **Unity Code Snippets** extension
3. Open folder in VS Code
4. Press `Ctrl+Shift+P` → "OmniSharp: Select Project" → Choose Assembly-CSharp.csproj

### Setting Up Newtonsoft.Json

If Newtonsoft.Json is missing:

1. Open Unity Package Manager: **Window → Package Manager**
2. Click **+** → "Add package from git URL"
3. Enter: `com.unity.nuget.newtonsoft-json`
4. Click **Add**

Alternative:
```json
// Add to Packages/manifest.json
{
  "dependencies": {
    "com.unity.nuget.newtonsoft-json": "3.0.2"
  }
}
```

---

## Running Tests

### Unity Test Runner

1. Open Test Runner: **Window → General → Test Runner**
2. Select **EditMode** tab (tests run without Play mode)
3. Click **"Run All"**
4. View results in same window

---

## Building the Game

### Build for Windows
1. **File → Build Settings**
2. Select **PC, Mac & Linux Standalone**
3. Target Platform: **Windows**
4. Click **Build**
5. Choose output folder
6. Executable will be created

### Build for Mac
1. **File → Build Settings**
2. Select **PC, Mac & Linux Standalone**
3. Target Platform: **macOS**
4. Click **Build**

### Build for WebGL
1. **File → Build Settings**
2. Select **WebGL**
3. Click **"Switch Platform"** (if not already selected)
4. Click **Build**
5. Upload to server or use Unity Play

---

## Troubleshooting

### Issue: Config file not found

**Error:** `Failed to load configuration: FileNotFoundException`

**Solution:**
1. Check that `Config/game_config.csv` exists
2. Verify file is not empty
3. Ensure file is in correct location relative to project root

**Workaround:**
- Game will create default config if file is missing
- Or copy config file to Unity's StreamingAssets folder

---

## Next Steps

### For Players
- Read [README.md](README.md) for gameplay instructions
- Check [game_config.csv](Config/game_config.csv) to understand mechanics

### For Developers
- Read [ARCHITECTURE.md](ARCHITECTURE.md) to understand design
- Explore domain entities in `Assets/Scripts/Domain/`
- Run and study unit tests

### For Game Designers
- Edit `Config/game_config.csv` to balance game
- No coding required!
- Test changes in Play mode

---

## Additional Resources

### Unity Learning
- [Unity Manual](https://docs.unity3d.com/Manual/index.html)
- [Unity Scripting Reference](https://docs.unity3d.com/ScriptReference/)

### C# Resources
- [Microsoft C# Documentation](https://docs.microsoft.com/en-us/dotnet/csharp/)
- [Clean Code by Robert C. Martin](https://www.amazon.com/Clean-Code-Handbook-Software-Craftsmanship/dp/0132350882)

### Testing
- [NUnit Documentation](https://docs.nunit.org/)
- [Unity Test Framework](https://docs.unity3d.com/Packages/com.unity.test-framework@latest)

---

## Support

### Getting Help
- Check documentation files (README, ARCHITECTURE)
- Review code comments
- Run tests to understand behavior
- Check Unity Console for errors

### Reporting Issues
When reporting issues, include:
1. Unity version
2. Operating system
3. Error messages (full stack trace)
4. Steps to reproduce
5. Expected vs actual behavior

---

## License & Contact

This project is for evaluation purposes for Wolffun Game Developer Test.

For questions about the architecture or implementation, please refer to the code comments and documentation files.

Happy coding! 🎮
