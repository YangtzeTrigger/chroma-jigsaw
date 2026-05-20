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

**B-08 — Monetisation** ✅ Code complete. Needs Panel_ZenPass scene hierarchy + Editor wiring.

B-08 scripts done:
- `IAPManager` — Unity IAP 5.3.0 wrapper; lifetime + monthly + pack purchase flow; OnPackPurchased event; RestorePurchases; GetLocalizedPrice (Core/)
- `ZenPassService` — IsZenPass bool (lifetime OR subscriber); GrantLifetime/GrantSubscriber/RevokeSubscriber (Core/)
- `SubscriptionService` — monthly sub state; POST-LAUNCH loyalty stub (Core/)
- `ZenPassView` — purchase screen; lifetime + monthly CTAs; loading overlay; restore (UI/)
- `SaveManager` — added zenPassLifetime + zenPassSubscriber (permanent, additive)
- `AdsManager` — ZenPassService.IsZenPass guard on all three ad methods
- `PackUnlockService` — real IAPManager calls; ZenPass→Owned in Evaluate; OnPackPurchased subscription; SetRegistry() helper
- `AppBootstrap` — init order: ZenPass → Subscription → IAP → Daily → LoadScene
- `ArtworkSpotlightView` — WallpaperManager API wired (Android); share text includes artwork title
- `ChromaJigsaw.Core.asmdef` — added Unity.Purchasing reference
- `ChromaJigsaw.Ads.asmdef` — added ChromaJigsaw.Core reference

**Remaining manual Unity Editor steps:**

B-07 (blocking — must do before B-07/B-08 run):
- Create DailyManifest SO: Assets → Create → Chroma Jigsaw → Daily Manifest; add 3 DailyEntry entries
- Wire AppBootstrap._dailyManifest slot in _Bootstrap.unity
- Wire GalleryController._registry, ._cardPrefab, ._gridContainer (B-06 holdover)
- Attach PackBrowserController to Panel_Puzzles; wire slots (B-06 holdover)
- Assign dot sprites to DailyCardView + DailyMasterpieceView (art needed)
- Assign TMP fonts to PackCard + GalleryFrameItem prefabs

B-08 (ZenPassView needs scene hierarchy — ask Claude.ai for spec):
- Build Panel_ZenPass hierarchy in MainMenu.unity (same YAML pattern as Panel_DailyMasterpiece)
- Wire ZenPassView Inspector slots: _canvasGroup, _lifetimePriceText, _monthlyPriceText, _lifetimeButton, _monthlyButton, _dismissButton, _restoreButton, _loadingOverlay
- Add IAPManager + ZenPassService + SubscriptionService GOs to _Bootstrap.unity

B-01 ✅ | B-02 ✅ | B-03 ✅ | B-04 ✅ | B-05 ✅ | B-06 ✅ | B-07 🔲 | B-08 ✅ (code)

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
| B-06 | Pack browser & unlock system | 🔲 |
| B-07 | Daily Image system | 🔲 (code ✅, needs Editor wiring) |
| B-08 | Monetisation | ✅ Code done — Panel_ZenPass hierarchy + Editor wiring pending |
| B-09 | Audio system | 🔲 |
| B-10 | Accessibility suite | 🔲 |
| B-11 | Analytics & event tracking | 🔲 |
| B-12 | Polish & QA | 🔲 |
| B-13 | Launch prep | 🔲 |

---

*Keep this file current. It is read at the start of every Claude Code session.*
*Last updated: 2026-05-20 — B-08 monetisation code complete. Commit ce8a6cc. IAPManager, ZenPassService, SubscriptionService, ZenPassView created. AdsManager, PackUnlockService, AppBootstrap, ArtworkSpotlightView updated. SaveData: zenPassLifetime + zenPassSubscriber added. Panel_ZenPass hierarchy not yet built — Claude.ai to spec before next session. B-07 Editor steps still outstanding.*
