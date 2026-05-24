# CLAUDE.md — Chroma Jigsaw

> This file is read automatically by Claude Code at the start of every session.
> It is the persistent context layer for this project. Keep it current.

---

## Project Identity

| Field | Value |
|---|---|
| **Game** | Chroma Jigsaw |
| **Parent project** | Chroma-Logic Gallery |
| **Track** | B — Standalone jigsaw game (built first) |
| **Engine** | Unity 6 (6000.0.x LTS) |
| **Platform** | Android primary |
| **Local root** | `C:\Users\jason\projects\ChromaLogic\Jigsaw` |
| **Notion hub** | https://www.notion.so/363cac35b9b681dcbeb1cb47194204fa |
| **B-01 Handoff** | https://www.notion.so/363cac35b9b68137aab7eb24c7197291 |

---

## Current Stage

**B-12d — Core Game Loop** ✅ Code + wiring complete. Puzzle rendering fixed. Pending: device QA + AudioMixer Editor action.

B-11 ✅ | B-12a ✅ (48e1f91) | B-12b ✅ (a88fe45) | B-12c ✅ (a0f77a5) | B-12d ✅ (3792c07)

### B-12d all fixes (across sessions):
- **HootyBridge**: `_puzzlePrefab` + `_puzzleParent` wired via YAML — commit a3f7828
- **Game.unity**: PuzzleCanvas GO added (Screen Space Overlay) — commit a3f7828
- **Game.unity**: FocusHUD_Canvas wired to PuzzleController._hudGroups — commit cf685f6
- **Gallery.unity**: "BEGIN RITUAL" → "BEGIN" — commit 341e68d
- **DailyMasterpieceView.HandleBegin()**: implemented — commit 341e68d
- **Pack_ALP_000.asset**: Alpine Stillness → Free — commit 341e68d
- **GraphicsSettings.asset**: Hootybird shaders added to AlwaysIncludedShaders — commit b147eaa
- **Puzzle.cs**: `material.SetTexture("_MainTex", PuzzleTexture)` — fixes white-square rendering in Unity 6 ([PerRendererData] + custom material doesn't propagate via RawImage.texture) — commit 3792c07
- **Game.unity**: removed AudioListener from Main Camera (was causing per-frame spam) — commit 3792c07

### Working puzzle entry paths:
1. **Packs tab → Whispering Pines (or Alpine Stillness) → PuzzleSelectView → BEGIN** → Game scene, cat01_2k test image
2. **Sanctuary tab → DailyCardView BEGIN → DailyMasterpieceView BEGIN** → Game scene, solid-colour fallback image

**AudioMixer exposed parameters:** ✅ Fixed via YAML (commit 47c7122). Parameters were exposed but left with Unity's default placeholder names — renamed to `MasterVol`, `SFXVol`, `MusicVol` directly in `MainMixer.mixer`.

**Remaining manual Unity Editor steps (still outstanding):**

B-07 (cosmetic — not blocking):
- EBGaramond SemiBold SDF: create via Font Asset Creator (Window → TextMeshPro → Font Asset Creator; source: Assets/Art/Fonts/EBGaramond-SemiBold.ttf); then re-assign TitleText in prefabs

B-09 (audio — blocked on art):
- Add MusicContextController GO to Gallery.unity and Game.unity; call SetContext() from scene controllers
- Assign Suno .mp3 clips to AudioManager Inspector slots (after Jason generates them)

B-10 (accessibility):
- Refine Panel_ZenPass button positions in Unity Editor (currently approximate absolute offsets)
- Wire OnGrandInterfaceChanged + OnArtworkBrightnessChanged in scene controllers (Grand Interface + Artwork Brightness are no-ops until subscribers exist)

**Resolved in commit 18b1e7c (Editor session 2026-05-21):** ✅ AccessibilityService GO in _Bootstrap ✅ IAPManager / ZenPassService / SubscriptionService GOs in _Bootstrap ✅ AppBootstrap._dailyManifest wired ✅ AudioManager.mixer wired

⚠️ Burst AOT: Edit → Project Settings → Player → Other Settings → Burst AOT Settings → **uncheck Enable Burst AOT Compilation** (Burst 1.8.29 crash bug)

B-01 ✅ | B-02 ✅ | B-03 ✅ | B-04 ✅ | B-05 ✅ | B-06 ✅ | B-07 🔲 (Editor) | B-08 ✅ | B-09 ✅ | B-10 ✅ | B-11 ✅ | B-12d ✅ (code+wiring) | B-12 🔲 (full QA pending device test)

---

## Unity Project Settings (Android)

```
Minimum API Level : 22 (Android 5.1)
Target API Level  : 34 (Android 14)
Scripting Backend : IL2CPP
Target Arch       : ARM64 only
.NET Standard     : 2.1
Pipeline          : URP (Universal Render Pipeline)
```

### Script Execution Order (load-bearing — do not change without reason)

```
AppBootstrap     -100
SaveManager       -90
AudioManager      -80
AnalyticsManager  -70
AdsManager        -60
(everything else)   0
```

AudioManager reads volumes from SaveManager on `Awake()`. Order is a hard dependency.

---

## Folder Structure

All paths relative to `Assets/`:

```
Assets/
├── _Bootstrap/
├── Scenes/
│   ├── _Bootstrap.unity        ← scene index 0
│   ├── Gallery.unity           ← scene index 1 (formerly MainMenu.unity)
│   ├── Game.unity              ← scene index 2
│   ├── RewardViewer.unity      ← scene index 3
│   └── Settings.unity          ← scene index 4
├── Scripts/
│   ├── Core/                   ← SaveManager.cs, AppBootstrap.cs, HootyBridge.cs
│   ├── UI/
│   ├── Data/                   ← AnalyticsManager.cs
│   ├── Audio/                  ← AudioManager.cs
│   └── Ads/                    ← AdsManager.cs
├── Art/
│   ├── UI/
│   ├── Backgrounds/
│   ├── Fonts/                  ← EB Garamond + Montserrat .otf files
│   └── Icons/
├── Audio/
│   ├── SFX/
│   └── Music/
├── Prefabs/
│   ├── Core/
│   └── UI/
├── Settings/                   ← URP pipeline assets
├── Resources/                  ← keep lean, runtime-loaded config only
└── ThirdParty/
    └── Hootybird/              ← third-party asset, do not modify directly
```

### Assembly Definitions

One `.asmdef` per `Scripts/` subfolder:

| File | Name |
|---|---|
| `Scripts/Core/ChromaJigsaw.Core.asmdef` | `ChromaJigsaw.Core` |
| `Scripts/UI/ChromaJigsaw.UI.asmdef` | `ChromaJigsaw.UI` |
| `Scripts/Data/ChromaJigsaw.Data.asmdef` | `ChromaJigsaw.Data` |
| `Scripts/Audio/ChromaJigsaw.Audio.asmdef` | `ChromaJigsaw.Audio` |
| `Scripts/Ads/ChromaJigsaw.Ads.asmdef` | `ChromaJigsaw.Ads` |

---

## Core Singletons

All singletons follow this pattern — do not deviate:

```csharp
private static T _instance;
public static T Instance
{
    get
    {
        if (_instance == null) _instance = FindAnyObjectByType<T>();
        if (_instance == null) { var go = new GameObject("[T]"); _instance = go.AddComponent<T>(); }
        return _instance;
    }
}

private void Awake()
{
    if (_instance != null && _instance != this) { Destroy(gameObject); return; }
    _instance = this;
    DontDestroyOnLoad(gameObject);
    // init...
}
```

### SaveManager (`ChromaJigsaw.Core`)
- Serialises `SaveData` to JSON at `Application.persistentDataPath/chroma_save.json`
- Atomic write: `.tmp` → rotate `.bak` → rename to primary
- `JsonUtility` — missing fields default silently (backward-compatible)
- **Never remove fields from `SaveData`** — only add. Mark deprecated fields `[Obsolete]`

### AudioManager (`ChromaJigsaw.Audio`)
- SFX pool: 8 `AudioSource` components (configurable)
- Music cross-fade uses `Time.unscaledDeltaTime` — works when paused. Do not change to `Time.deltaTime`
- Volume stored as linear 0–1 in `SaveData`; converted to dB for the `AudioMixer`
- Mixer param names: `MasterVol`, `SFXVol`, `MusicVol` (must match Exposed Parameters in the AudioMixer asset)

### AnalyticsManager (`ChromaJigsaw.Data`)
- Firebase stub. Offline queue holds up to 200 events before SDK is live
- To activate Firebase: uncomment all blocks marked `// FIREBASE_READY`
- Event names: always `snake_case`

### AdsManager (`ChromaJigsaw.Ads`)
- AdMob stub. Interstitial cooldown: 60 seconds
- To activate AdMob: uncomment blocks marked `// ADMOB`

### AppBootstrap (`ChromaJigsaw.Core`)
- Lives in `_Bootstrap` scene only
- Initialises all singletons in dependency order, then loads `MainMenu`

---

## Coding Conventions

- **Namespaces**: always use full namespace — `ChromaJigsaw.Core`, `ChromaJigsaw.Audio`, etc.
- **No static helper classes** with side effects — use singleton instances
- **No `FindObjectOfType` in gameplay code** — cache in `Awake()` or use singleton accessors
- **No direct Hootybird API calls** outside `HootyBridge.cs` — ever
- **Analytics event names**: `snake_case` strings, e.g. `puzzle_started`, `hint_used`
- **SaveData**: additive changes only. Never rename or remove a field
- **No `Debug.Log` in production paths** — wrap in `#if UNITY_EDITOR` or use a log level guard
- **No hardcoded strings** for scene names — use constants in a `SceneNames.cs` static class
- **No timers on any game mode** — this is a no-pressure game by design

---

## Hootybird Integration Rules

- Import to `Assets/ThirdParty/Hootybird/`
- Never call Hootybird APIs from game logic — all calls go through `HootyBridge.cs`
- If Hootybird has an `AudioManager`, `GameManager`, or `SaveManager`, rename theirs before import
- `HootyBridge.cs` lives in `Assets/Scripts/Core/`

```csharp
// Pattern — all Hootybird calls look like this in game code:
HootyBridge.Instance.LoadPuzzle(imageTexture, pieceCount);
HootyBridge.Instance.OnPieceSnapped += HandleSnap;

// Never like this:
HootyBird.JigsawManager.Instance.LoadPuzzle(...); // ❌
```

---

## Track Merge Rules (Track C will merge B + A)

These rules exist so the merge is clean. Do not break them:

1. Folder structure matches Track A exactly: `/Scripts/Core`, `/UI`, `/Data`, `/Audio`, `/Ads`
2. `SaveManager` interface is identical across tracks
3. `AudioManager` interface is identical across tracks
4. Accessibility preferences stored at `PlayerData.accessibilityPrefs`
5. Analytics event names are `snake_case` across all tracks
6. Pack bundle JSON schema: `{ packId, packName, imageIds[], isOwned, completionFlags[] }`
7. No red UI states anywhere in any track
8. No timers anywhere in any track

---

## Game Design Constants

```
Piece counts    : 12 / 24 / 48 / 96
XP per puzzle   : 12pc=10, 24pc=25, 48pc=60, 96pc=150
Snap radius     : generous (12/24), tighter (48), precise (96)
Workspace bg    : dark felt texture
Rotation        : long-press + drag, snaps 90°
Image delivery  : 1080p in-game, 4K for Souvenir download
Pack size       : 12 images per pack
```

---

## Handoff Loop — Dual-Note Protocol

Each stage Notion page has two permanent channels. Read them before starting work.

### 📨 Claude.ai → Claude Code
Architectural brief: scope, design decisions (final), constraints, files to read.
Written by Claude.ai. **Read this first. It defines what to build and why.**

### 📤 Claude Code → Claude.ai
Build report: what was built, commit hashes, deviations from spec, hard blockers,
open questions that need a design decision before the next stage can start.
**Append a timestamped entry at the end of every Claude Code session.**

```
Claude.ai        → writes 📨 brief to Notion stage page
                        ↓
Claude Code      → reads 📨 brief → builds → writes 📤 report to same page
                        ↓
Claude.ai        → reads 📤 report → writes next 📨 brief
```

**Jason's role:** Open Unity Editor, provide art, approve direction.
Not a relay — both Claude instances read Notion directly.

### At the end of every Claude Code session (unprompted):
1. Append a `📤 Claude Code → Claude.ai` entry to the current stage Notion page
2. Commit and push all changes
3. Update CLAUDE.md Current Stage section to match reality
4. Update memory/project_stage_progress.md

**Entry must include:** files changed + commits, deviations from spec, hard blockers
(things that will crash or break without Jason's Editor work), open design questions
Claude.ai needs to answer before the next stage brief is written.

---

## Stage Index

| Stage | Area | Status |
|---|---|---|
| B-01 | Project setup | ✅ Done |
| B-02 | Image asset pipeline | ✅ Done |
| B-03 | Jigsaw piece system | ✅ Done |
| B-04 | Touch controls | ✅ Done |
| B-05 | Gallery UI | ✅ Done |
| B-06 | Pack browser & unlock system | ✅ Done |
| B-07 | Daily Image system | 🔲 Code ✅ — Editor wiring pending |
| B-08 | Monetisation | ✅ Done |
| B-09 | Audio system | ✅ Code done — clip slots + scene wiring pending |
| B-10 | Accessibility suite | ✅ Done (code + scene wiring complete) |
| B-11 | Analytics & event tracking | ✅ Done |
| B-12 | Polish & QA | 🔲 (B-12a–d code+wiring done — device QA pending) |
| B-13 | Launch prep | 🔲 |

---

*Keep this file current. It is read at the start of every Claude Code session.*
*Last updated: 2026-05-24 — B-12d all console errors resolved. Puzzle rendering fixed (Puzzle.cs SetTexture, commit 3792c07). AudioListener spam fixed (Game.unity, commit 3792c07). AudioMixer param names fixed (MainMixer.mixer, commit 47c7122). Pending: device QA only.*
