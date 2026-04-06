# Pulse Highway - Unity Edition

A cyberpunk rhythm game with procedurally generated music, a 24-level campaign, and stunning neon visuals. Built entirely in C# with no Editor-created prefabs -- everything is generated programmatically at runtime.

---

## Table of Contents

- [Quick Start (Unity Hub)](#quick-start-unity-hub)
- [Controls](#controls)
- [Game Overview](#game-overview)
- [Project Architecture](#project-architecture)
- [Running Tests](#running-tests)
- [Building](#building)
- [CI/CD Pipeline](#cicd-pipeline)
- [Self-Hosted Runner Setup](#self-hosted-runner-setup)
- [Troubleshooting](#troubleshooting)

---

## Quick Start (Unity Hub)

### Prerequisites

- **Unity Hub** installed ([download](https://unity.com/download))
- **Unity 2022.3 LTS** (2022.3.10f1 or newer 2022.3.x patch)

### Step-by-Step Setup

1. **Install Unity 2022.3 LTS** (if not already installed):
   - Open Unity Hub
   - Go to **Installs** tab
   - Click **Install Editor**
   - Select **Unity 2022.3.x LTS** (any patch version)
   - In the module selection, check:
     - **Windows Build Support (Mono)** -- for Windows builds
     - **Android Build Support** -- for Android builds (also installs JDK and SDK)
   - Click **Install** and wait for completion

2. **Open the project**:
   - In Unity Hub, go to the **Projects** tab
   - Click **Add** (or **Open**) button
   - Browse to the `PulseHighwayUnity/` folder and select it
   - Unity Hub will detect the project and list it
   - Click on the project to open it
   - First import takes 3-5 minutes (Unity compiles shaders and imports packages)

3. **Configure the Render Pipeline** (first time only):
   - Once the Editor opens, go to **Edit > Project Settings > Graphics**
   - If "Scriptable Render Pipeline Settings" shows `None`:
     - In the Project window, right-click **Assets** folder
     - Select **Create > Rendering > URP Asset (with Universal Renderer)**
     - This creates two files: `URP-Asset` and `URP-Asset_Renderer`
     - Drag the **URP-Asset** into the "Scriptable Render Pipeline Settings" field
   - Go to **Edit > Project Settings > Quality**
     - For each quality level (Low, Medium, High), set the **Render Pipeline Asset** to the same URP Asset
   - Select the **URP-Asset** in the Project window, and in the Inspector:
     - Enable **HDR**
     - Set **Anti Aliasing (MSAA)** to **4x**

4. **Open the scene and play**:
   - In the Project window, navigate to **Assets > Scenes**
   - Double-click **BootstrapScene**
   - Press the **Play** button (or Ctrl+P)
   - The main menu appears -- click **CAMPAIGN** to start playing

5. **Play a level**:
   - Click any unlocked level card (Level 1 is unlocked by default)
   - Wait for music generation and chart analysis (~2 seconds)
   - Play using keyboard, mouse, or touch controls

---

## Controls

| Input | Lane 1 | Lane 2 | Lane 3 | Lane 4 | Lane 5 |
|-------|--------|--------|--------|--------|--------|
| **Primary Keys** | D | F | J | K | L |
| **Alt Keys** | 1 | 2 | 3 | 4 | 5 |
| **Mouse** | Click left 1/5 | Click 2/5 | Click 3/5 | Click 4/5 | Click right 1/5 |
| **Touch** | Tap zone 1 | Tap zone 2 | Tap zone 3 | Tap zone 4 | Tap zone 5 |

### How to Play

- **Tap Notes** (spinning diamonds): Press the lane key when the diamond reaches the cyan hit line
- **Hold Notes** (diamond + laser beam): Press and hold until the beam tail passes the hit line
- **Timing Judgments**: Perfect (within 25ms), Great (50ms), Good (100ms), Miss (beyond 100ms)
- **Combo**: Builds with each successful hit, resets on a miss. Higher combo = higher score multiplier
- **Ranks**: S (95%+), A (90%+), B (80%+), C (70%+), F (<70%). Get rank C+ and meet the score threshold to unlock the next level

---

## Game Overview

### Campaign (24 Levels)

| Phase | Levels | Difficulty | BPM Range | Description |
|-------|--------|-----------|-----------|-------------|
| 1: Initiation | 1-6 | Easy | 110-125 | Learn the basics |
| 2: Acceleration | 7-12 | Medium | 128-140 | Speed ramps up |
| 3: Transcendence | 13-18 | Hard | 142-152 | Dense patterns |
| 4: Mastery | 19-24 | Expert | 154-160 | Ultimate challenge |

### Music Genres (Procedurally Generated)

- **Synthwave**: Retro-futuristic synths, sawtooth bass, arpeggiated leads
- **Drum & Bass**: Breakbeat kick patterns, Reese bass, high-energy rhythms
- **Techno**: Acid 303 bass lines, rumble kicks, off-beat hi-hats

Every level generates its own unique track using additive synthesis -- no audio files needed.

### Scoring

| Judgment | Base Points | Multiplier |
|----------|------------|------------|
| Perfect | 100 | x (1 + combo x 0.1) |
| Great | 75 | x (1 + combo x 0.1) |
| Good | 50 | x (1 + combo x 0.1) |
| Hold Tick | 20 | x (1 + combo x 0.05) |

### Visual Features

- URP post-processing: bloom, chromatic aberration, vignette, color grading, film grain
- 5000-particle animated starfield
- Dynamic purple/cyan point lights
- Per-note trail renderers
- Hit burst particles (20 per hit)
- Camera shake on hits
- Neon glow pulsing on hit line
- Retro grid floor

---

## Project Architecture

All game content is created programmatically in C# -- zero Editor-created prefabs or assets.

```
PulseHighwayUnity/
  Assets/
    Scenes/
      BootstrapScene.unity        Single entry scene
    Scripts/
      Core/                       Bootstrap, scene flow, game manager, game state
      Audio/                      Procedural synth engine, genre generators, audio analysis, chart generation
      Game/                       24 level definitions, scoring, ranks, timing windows, level manager
      Gameplay/                   Highway builder, note objects, spawner, hit effects, camera shake
      Input/                      Unified keyboard/mouse/touch input with circular buffer
      UI/                         Main menu, level select, gameplay HUD, results screen
      Visual/                     Material factory, mesh factory, post-processing, particles, glow effects
      Storage/                    JSON-based save/load to Application.persistentDataPath
      PulseHighway.asmdef         Assembly definition for production code
    Editor/
      BuildScript.cs              CLI build automation entry point
      PulseHighway.Editor.asmdef  Editor assembly definition
    Tests/
      EditMode/                   80 NUnit tests for game logic, audio, and input
      PlayMode/                   Placeholder for future integration tests
  Packages/
    manifest.json                 URP, TextMeshPro, Test Framework
  ProjectSettings/                Standard Unity project settings
```

### Key Design Decisions

- **Single bootstrap scene**: All screens (menu, level select, gameplay, results) are built/swapped as GameObjects -- no multi-scene loading
- **Procedural audio**: `AudioClip.Create` with PCM sample buffers replaces Tone.js from the web version
- **Object pooling**: Notes and particles are pre-allocated and recycled to avoid GC stutters
- **Audio-synced timing**: `AudioSource.time` is the authoritative clock for note positions

---

## Running Tests

### In the Unity Editor

1. Open the project in Unity
2. Go to **Window > General > Test Runner**
3. Select the **EditMode** tab
4. Click **Run All** to execute all 80 tests

### From the Command Line

```bash
# Using the build script (auto-detects Unity)
./scripts/unity-build.sh test

# Direct Unity CLI (Linux)
/path/to/Unity -batchmode -nographics \
  -projectPath PulseHighwayUnity \
  -runTests -testPlatform EditMode \
  -testResults test-results/results.xml \
  -logFile test-results/test.log

# Direct Unity CLI (Windows, from Git Bash)
"/c/Program Files/Unity/Hub/Editor/2022.3.10f1/Editor/Unity.exe" \
  -batchmode -nographics \
  -projectPath PulseHighwayUnity \
  -runTests -testPlatform EditMode \
  -testResults test-results/results.xml \
  -logFile test-results/test.log
```

### Test Coverage (80 tests)

| Suite | Tests | What It Covers |
|-------|-------|---------------|
| ScoreCalculatorTests | 13 | Hit scoring, hold ticks, rank thresholds |
| TimingWindowsTests | 9 | Judgment windows (Perfect/Great/Good/Miss) |
| LevelDefinitionsTests | 10 | 24-level config integrity, progression |
| RankTests | 5 | Enum ordering, comparisons, display |
| SynthVoiceTests | 21 | Oscillators, ADSR envelopes, filters, clipping |
| ChartGeneratorTests | 8 | Note generation, lane validity, sorting |
| AudioAnalyzerTests | 6 | Onset detection, frequency bands |
| InputBufferTests | 8 | Circular buffer, lane matching, capacity |

---

## Building

### From the Unity Editor (GUI)

1. Open the project in Unity
2. Go to **File > Build Settings**
3. Click **Add Open Scenes** (should add BootstrapScene)
4. Select target platform:
   - **Windows**: PC, Mac & Linux Standalone > Target Platform: Windows
   - **Android**: Switch to Android platform (installs module if needed)
5. Click **Build** and choose an output folder
6. Recommended: In **Player Settings**, set Color Space to **Linear**

### From the Command Line

```bash
# Build for Windows
./scripts/unity-build.sh build windows

# Build for Android
./scripts/unity-build.sh build android

# Build for Linux
./scripts/unity-build.sh build linux

# Test then build
./scripts/unity-build.sh both windows
```

Build outputs go to `Builds/<Platform>/`:
- Windows: `Builds/StandaloneWindows64/PulseHighway.exe`
- Android: `Builds/Android/PulseHighway.apk`
- Linux: `Builds/Linux/PulseHighway`

### Build Script Reference

```
Usage: ./scripts/unity-build.sh <mode> [target]

Modes:
  test                 Run EditMode unit tests
  build <target>       Build for a platform
  both <target>        Test then build
  help                 Show help

Targets:
  windows (default)    StandaloneWindows64
  linux                StandaloneLinux64
  android              Android APK
  ios                  iOS Xcode project
  webgl                WebGL
```

---

## CI/CD Pipeline

### Workflow: `.github/workflows/unity-ci.yml`

Runs on a **self-hosted Linux runner** with Unity installed locally.

```
Push/PR to main (PulseHighwayUnity/** changed)
         |
    [Run EditMode Tests]
         |
    (tests pass?)
       /     \
      v       v
[Build Windows]  [Build Android]
      |              |
      v              v
[Upload Artifact] [Upload Artifact]
```

**Trigger conditions**:
- Push to `main` branch (only if `PulseHighwayUnity/` files changed)
- Pull requests targeting `main` (same path filter)
- Manual dispatch via GitHub Actions UI (choose build targets)

**Artifacts produced**:
- `test-results/` -- NUnit XML results (7-day retention)
- `Build-Windows/` -- Windows executable (14-day retention)
- `Build-Android/` -- Android APK (14-day retention)

### Environment Variables

Set `UNITY_EXECUTABLE` in the workflow to your runner's Unity path:
```yaml
env:
  UNITY_EXECUTABLE: /opt/unity/Editor/Unity
```

---

## Self-Hosted Runner Setup

### 1. Install Unity on the Linux Runner

```bash
# Install Unity Hub (Debian/Ubuntu)
wget -qO- https://hub.unity3d.com/linux/keys/public | gpg --dearmor | \
  sudo tee /usr/share/keyrings/Unity_Technologies_ApS.gpg > /dev/null
echo "deb [signed-by=/usr/share/keyrings/Unity_Technologies_ApS.gpg] https://hub.unity3d.com/linux/repos/deb stable main" | \
  sudo tee /etc/apt/sources.list.d/unityhub.list
sudo apt update && sudo apt install -y unityhub

# Install Unity 2022.3 LTS with build modules
unityhub --headless install --version 2022.3.10f1
unityhub --headless install-modules --version 2022.3.10f1 \
  -m linux-il2cpp windows-mono android

# Activate a license (Personal or Pro)
unityhub --headless activate --username "you@email.com" --password "pass" --serial "XXXX-XXXX-XXXX-XXXX"
# Or for Personal license:
unity -batchmode -nographics -createManualActivationFile
# Then upload the .alf file to license.unity3d.com and download the .ulf
unity -batchmode -nographics -manualLicenseFile Unity_v2022.x.ulf
```

### 2. Verify the Installation

```bash
# Check Unity is accessible
/opt/unity/Editor/Unity -version
# Should output: 2022.3.10f1

# Or if installed via Hub (path may vary):
~/Unity/Hub/Editor/2022.3.10f1/Editor/Unity -version
```

### 3. Android SDK Setup (for Android Builds)

Unity's Android module includes a bundled SDK, but you may need to set:

```bash
export ANDROID_HOME=~/Unity/Hub/Editor/2022.3.10f1/Editor/Data/PlaybackEngines/AndroidPlayer/SDK
export ANDROID_SDK_ROOT=$ANDROID_HOME
```

### 4. Update the Workflow

Edit `.github/workflows/unity-ci.yml` and set `UNITY_EXECUTABLE` to match your install path:

```yaml
env:
  UNITY_EXECUTABLE: ~/Unity/Hub/Editor/2022.3.10f1/Editor/Unity
```

---

## Troubleshooting

### "Render Pipeline Asset is None"
Follow step 3 in [Quick Start](#quick-start-unity-hub) to create and assign a URP Asset.

### Pink/magenta materials in the scene
This means URP shaders aren't loaded. Ensure the Render Pipeline Asset is assigned in both Graphics and Quality settings.

### No sound during gameplay
Check **Edit > Project Settings > Audio** -- ensure the DSP Buffer Size is set to "Best performance" or "Default".

### Tests fail with "assembly not found"
Make sure `PulseHighway.asmdef` exists at `Assets/Scripts/PulseHighway.asmdef`. Unity needs this to resolve test assembly references.

### Unity CLI hangs on Linux
Ensure you have a valid Unity license activated. Batch mode still requires a license. Add `-logFile -` to see output in real-time.

### Build fails with "No scenes in build"
Open the project once in the Editor, then go to **File > Build Settings** and click **Add Open Scenes**. This updates `EditorBuildSettings.asset`.

### Save data location
Player progress is saved to `Application.persistentDataPath/progress.json`:
- Windows: `%USERPROFILE%/AppData/LocalLow/PulseHighway/Pulse Highway/progress.json`
- Linux: `~/.config/unity3d/PulseHighway/Pulse Highway/progress.json`
- Android: `/data/data/com.PulseHighway.Game/files/progress.json`

To reset progress, delete this file.
