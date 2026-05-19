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

**B-06 — Pack browser & unlock system** 🔲 Next

B-05 ✅ Gallery UI complete. All scripts, prefabs, and scene wiring done via YAML.

**Wired in MainMenu.unity (via text editing — no Editor needed):**
- GalleryController: _panelPackGrid, _galleryWallView, _spotlightView ✅
- GalleryWallView: _framesContainer, _frameItemPrefab, _packTitleText, _subtitleText ✅
- BottomNavController: _tabs[4] (Button/TopBorder/Icon/Label each), _panels[4], _xpBarView ✅
- ArtworkSpotlightView: _canvasGroup (CanvasGroup !u!225), _artworkImage, _titleText, all 4 buttons ✅
  - Children built: Background scrim, ArtworkImage, TitleText, CloseButton, ReplayButton, ShareButton, WallpaperButton
- DailyCardView: _artworkPreview, _beginButton, _dotIndicators[3] ✅
  - Children built: ArtworkPreview, BeginButton (gold), Dot0/Dot1/Dot2

**Remaining manual Unity Editor steps (must do before B-06 runs cleanly):**
- Assign TMP font assets to all TextMeshProUGUI in PackCard + GalleryFrameItem prefabs
- Assign TMP font assets to tab labels in scene (currently LiberationSans fallback)
- Assign lock icon sprite to LockIcon Image in PackCard prefab (art needed)
- Wire XPBarView._fill and ._xpLabel (XPBar child GOs not yet created)
- Assign dot sprites (_dotCompleted, _dotInProgress, _dotEmpty) to DailyCardView (art needed)

B-01 ✅ | B-02 ✅ | B-03 ✅ | B-04 ✅ | B-05 ✅ | B-06 🔲

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
│   ├── MainMenu.unity          ← scene index 1
│   ├── Game.unity              ← scene index 2
│   └── Settings.unity          ← scene index 3
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

## Handoff Loop

```
Claude.ai chat      → architecture + code generation → updates Notion
      ↓  (paste "Claude Code prompt" from Notion Handoff page)
Claude Code         → file creation + Unity work → reports what it did
      ↓  (MANUAL: Jason updates Handoff Log in Notion)
Claude.ai chat      → reads log → next stage planning
```

**At the end of every Claude Code session, report:**
1. Every file created or modified with full path
2. Any compile errors and how they were resolved
3. Anything that deviated from the plan and why
4. What the next session should tackle

Jason will paste this into the Notion Handoff Log.

---

## Stage Index

| Stage | Area | Status |
|---|---|---|
| B-01 | Project setup | ✅ Done |
| B-02 | Image asset pipeline | ✅ Done |
| B-03 | Jigsaw piece system | ✅ Done |
| B-04 | Touch controls | ✅ Done |
| B-05 | Gallery UI | ✅ Done |
| B-06 | Pack browser & unlock system | 🔲 |
| B-07 | Daily Image system | 🔲 |
| B-08 | Monetisation | 🔲 |
| B-09 | Audio system | 🔲 |
| B-10 | Accessibility suite | 🔲 |
| B-11 | Analytics & event tracking | 🔲 |
| B-12 | Polish & QA | 🔲 |
| B-13 | Launch prep | 🔲 |

---

*Keep this file current. It is read at the start of every Claude Code session.*
*Last updated: 2026-05-19 — B-05 ✅ Fully complete. All YAML wiring done (BottomNav, ArtworkSpotlight, DailyCard). Game.unity B-04 holdover done ([HootyBridge], [PuzzleController], [WorkspaceController] added). B-06 next.*
