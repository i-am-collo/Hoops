# 🏀 HOOPS

A 2D arcade basketball game for Android, built with Unity. Swipe to shoot, chain baskets into combos, buy yourself extra seconds, and survive hoops that get faster every level.



---

## Table of Contents

1. [Overview](#overview)
2. [Core Gameplay Loop](#core-gameplay-loop)
3. [Tech Stack](#tech-stack)
4. [Getting Started](#getting-started)
5. [Project Structure](#project-structure)
6. [Scenes](#scenes)
7. [Gameplay Systems](#gameplay-systems)
8. [Balancing Reference](#balancing-reference)
9. [Script Responsibilities](#script-responsibilities)
10. [Prefab Setup](#prefab-setup)
11. [UI Specification](#ui-specification)
12. [Audio](#audio)
13. [Particles & Visual Effects](#particles--visual-effects)
14. [Coding Standards](#coding-standards)
15. [Development Roadmap](#development-roadmap)
16. [Android Build](#android-build)
17. [Release Checklist](#release-checklist)
18. [Future Expansion](#future-expansion)
19. [Contributing](#contributing)
20. [License](#license)

---

## Overview

| Property | Value |
|---|---|
| **Game name** | HOOPS |
| **Genre** | 2D arcade basketball |
| **Platform** | Android |
| **Engine** | Unity 2022 LTS (Universal Render Pipeline) |
| **Language** | C# |
| **Orientation** | Portrait |
| **Reference resolution** | 1080 × 1920 |
| **Target frame rate** | 60 FPS |

---

## Core Gameplay Loop

1. The player drags/swipes the basketball.
2. The swipe vector determines shot **angle** and **power**.
3. The ball follows a realistic 2D arc under gravity.
4. Scoring a basket increases the score.
5. Consecutive baskets build a **combo**.
6. Every **3 consecutive baskets** adds **+3 seconds** to the timer.
7. Every **5 points** raises the **level**.
8. Hoop speed increases with each level.
9. The game ends when the timer reaches zero.

---

## Tech Stack

| Area | Technology |
|---|---|
| Engine | Unity 2022 LTS |
| Rendering | Universal Render Pipeline (URP) |
| Input | Unity Input System |
| UI text | TextMeshPro |
| Camera | Cinemachine |
| 2D tooling | 2D Sprite, 2D Animation |
| Telemetry | Unity Analytics **or** Firebase SDK |
| Scripting backend (Android) | IL2CPP |

---

## Getting Started

### Prerequisites

- Unity Hub with **Unity 2022 LTS** installed
- Android Build Support module (including **Android SDK & NDK Tools** and **OpenJDK**)
- An Android device (API 26+) with USB debugging enabled, or an emulator
- Git

### Setup

```bash
# 1. Clone the repository
git clone <your-repository-url> hoops
cd hoops

# 2. Open the project
#    Unity Hub → Add → select the "hoops" folder → open with Unity 2022 LTS
```

### Required packages

Install via **Window → Package Manager**:

- Universal RP
- Input System
- TextMeshPro
- Cinemachine
- 2D Sprite
- 2D Animation
- Unity Analytics / Firebase SDK (choose one)

### Physics settings

Configure under **Edit → Project Settings → Physics 2D**:

| Setting | Value |
|---|---|
| Gravity Y | `-9.8` |
| Default collision detection (on rigidbodies) | Continuous |
| Default interpolation (on rigidbodies) | Enabled |

### Run the game

1. Open `Assets/Scenes/MainMenu.unity`.
2. Press **Play** in the editor (use the Device Simulator at 1080 × 1920 for portrait).
3. For device testing: **File → Build Settings → Android → Build And Run**.

---

## Project Structure

```
Assets/
├── Art/
│   ├── Backgrounds/
│   ├── UI/
│   ├── Ball/
│   ├── Hoop/
│   └── Effects/
├── Audio/
│   ├── Music/
│   └── SFX/
├── Materials/
├── Prefabs/
│   ├── Ball.prefab
│   ├── Hoop.prefab
│   ├── HUD.prefab
│   └── ParticleEffects.prefab
├── Scenes/
│   ├── MainMenu.unity
│   ├── Gameplay.unity
│   ├── GameOver.unity
│   └── Settings.unity
├── Scripts/
│   ├── Core/
│   ├── Gameplay/
│   ├── UI/
│   ├── Audio/
│   └── Managers/
└── ScriptableObjects/
    └── LevelConfigs/
```

---

## Scenes

### Main Menu
Logo · Play · Instructions · Settings · Exit · animated hoop background.

### Gameplay
Main Camera · Canvas · GameManager · ScoreManager · AudioManager · Ball Spawn Point · Basketball · Hoop · Court Background · HUD · Particle Systems.

### Game Over
Final score · highest combo · level reached · time played · **Play Again** · **Main Menu**.

### Settings
Audio volume and game preferences.

---

## Gameplay Systems

### Swipe shooting
- A touch must begin **on the basketball**.
- While dragging, a line visualizes the predicted trajectory.
- **Swipe length** → launch force. **Swipe direction** → launch angle.
- Launch force is **clamped** to a maximum.

### Ball physics
- `Rigidbody2D` with gravity enabled.
- Rotation applied during flight.
- Realistic bounce on rim collisions.

### Hoop movement
- **Level 1:** stationary.
- **Level 2+:** horizontal oscillation, with speed increasing every level.

### Scoring and combo
- Basket = **+1** score.
- Consecutive baskets increase the combo; a **miss resets it**.
- Every 3 consecutive baskets: **+3 seconds**, a combo overlay, and a confetti effect.

### Level progression
- Every **5 points** triggers a level-up.
- Hoop speed increases and the background atmosphere changes (day → dusk → night).

---

## Balancing Reference

All values should live in `ScriptableObjects/LevelConfigs/` so they can be tuned without code changes.

| Variable | Value |
|---|---|
| Starting timer | 60 s |
| Maximum timer | 90 s |
| Combo requirement | 3 consecutive baskets |
| Combo reward | +3 s |
| Level threshold | every 5 points |
| Ball respawn delay | 0.4 s |
| Target resolution | 1080 × 1920 |
| Target frame rate | 60 FPS |

### Recommended initial hoop speeds

| Level | Speed |
|---|---|
| 1 | 0 |
| 2 | 2 |
| 3 | 3 |
| 4 | 4.5 |
| 5 | 6 |

---

## Script Responsibilities

| Script | Responsibility |
|---|---|
| `GameManager.cs` | Game state machine, starting/ending sessions, pausing during overlays, restart flow |
| `BallController.cs` | Swipe input, launch force, respawn logic, ball reset |
| `HoopController.cs` | Horizontal movement, speed scaling, rim collision handling |
| `ScoreManager.cs` | Score tracking, combo logic, timer countdown, level progression |
| `UIManager.cs` | HUD updates, overlay animations, menu transitions |
| `AudioManager.cs` | Music playback, SFX playback, volume settings |
| `LevelConfig.cs` | ScriptableObject holding hoop speed, level thresholds, background assets, difficulty values |

---

## Prefab Setup

### Basketball (`Ball.prefab`)

**Components:** `SpriteRenderer`, `Rigidbody2D`, `CircleCollider2D`, `TrailRenderer` (optional), `BallController`

**Rigidbody2D**

| Setting | Value |
|---|---|
| Mass | 1 |
| Gravity Scale | 1 |
| Collision Detection | Continuous |
| Interpolate | Interpolate |

**Tag:** `PlayerBall`

### Hoop (`Hoop.prefab`)

**Components:**
- `SpriteRenderer`
- `BoxCollider2D` (backboard)
- `CircleCollider2D` ×2 (left rim, right rim)
- Trigger collider (score detection)
- `HoopController`

**Tags:** `Hoop`, `ScoreZone`

---

## UI Specification

| Region | Contents |
|---|---|
| **Top HUD** | Score, timer, level, combo |
| **Center** | Moving hoop and gameplay area |
| **Bottom** | Ball spawn location and swipe interaction area |

**Behaviors**
- Timer turns **red** below 10 seconds.
- Combo flash animation.
- Level-up overlay.
- Game-over panel.

---

## Audio

**Music:** Day Loop · Dusk Loop · Night Loop

**Sound effects:** Ball Swish · Rim Bounce · Combo Fanfare · Level-Up Jingle · Countdown Beeps · Button Click · Game Over Sting

**Rules**
- Apply random pitch variation to swish sounds.
- Crossfade smoothly between level music themes.

---

## Particles & Visual Effects

| Effect | Trigger |
|---|---|
| Confetti explosion | Combo reward |
| Star burst | Successful shot |
| Hoop shake | Rim collision |
| Screen shake | Rim collision / big events |
| Ball trail | Ball in flight |
| Net sway | Ball passes through hoop |
| Successful-shot flash | Basket scored |
| Combo pulse | Combo increment |

---

## Coding Standards

- Use **singleton managers** where appropriate.
- Use **ScriptableObjects** for all balancing data.
- Use **events/delegates** for UI updates.
- Keep **gameplay logic separate from UI logic**.

---

## Development Roadmap

| Phase | Scope |
|---|---|
| **1 – Foundation** | Project structure, gameplay scene, ball and hoop prefabs, swipe shooting |
| **2 – Game rules** | Scoring, combo mechanic, timer, level progression |
| **3 – Presentation** | HUD, overlays, audio system, particle effects |
| **4 – Release prep** | Optimization, analytics, settings system, Android build configuration |

---

## Android Build

| Setting | Value |
|---|---|
| Platform | Android |
| Minimum SDK | API 26 |
| Target SDK | API 34 |
| Scripting backend | IL2CPP |
| Architectures | ARM64, ARMv7 |

**Optimization**
- Sprite atlasing enabled
- Texture compression enabled
- VSync disabled on mobile
- Target 60 FPS (`Application.targetFrameRate = 60`)

> **Note:** Google Play raises its required target API level periodically. Check the current requirement before publishing and update Target SDK if needed.

---

## Release Checklist

**Core**
- [ ] Swipe shooting works
- [ ] Ball physics stable
- [ ] Hoop movement stable
- [ ] Basket detection accurate
- [ ] Combo system functional
- [ ] Timer system functional
- [ ] Level system functional

**UI**
- [ ] HUD updates correctly
- [ ] Overlays animate correctly
- [ ] Game-over screen works

**Audio**
- [ ] Music loops correctly
- [ ] SFX trigger correctly

**Optimization**
- [ ] Stable 60 FPS
- [ ] Android build successful
- [ ] No physics tunneling
- [ ] Memory usage acceptable

---

## Future Expansion

- Leaderboards
- Ball skins
- Court themes
- Daily challenges
- Achievement system
- Multiplayer score comparison
- Ads integration
- Reward system

---

## Contributing

1. Fork the repository and create a feature branch: `git checkout -b feature/your-feature`.
2. Follow the [coding standards](#coding-standards).
3. Keep balancing values in ScriptableObjects, not hard-coded.
4. Test on a physical Android device before opening a pull request.
5. Submit a pull request describing what changed and why.

---

## License

License to be defined by the project owner. Add a `LICENSE` file to the repository root and update this section.
