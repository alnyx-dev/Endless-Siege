<h1 align="center">
  <br>
  Endless Siege
  <br>
</h1>

<h4 align="center">Top-down survival — fight the endless horde, beat your record.</h4>

<p align="center">
  <img src="https://img.shields.io/badge/Unity-6%2B-000?logo=unity&logoColor=white" alt="Unity">
  <img src="https://img.shields.io/badge/C%23-10-blue?logo=csharp&logoColor=white" alt="C#">
  <img src="https://img.shields.io/badge/License-MIT-green" alt="License">
  <img src="https://img.shields.io/badge/Platform-Windows%20%7C%20Android-lightgrey" alt="Platform">
</p>

<p align="center">
  Enemies spawn endlessly from all sides. Your weapon auto-targets — just move, aim, and survive.
</p>

---

## Overview

| | |
|---|---|
| **Genre** | Survival / Arena |
| **Camera** | Top-down with smooth follow |
| **Input** | Mobile joystick (extensible to keyboard/gamepad via `IMovementInput`) |
| **Objective** | Survive as long as possible. Best time saved automatically. |

---

## Controls

| Action | Input |
|--------|-------|
| Move | Joystick |
| Attack | Automatic — fires when muzzle faces nearest enemy |

---

## How It Works

### Game Flow

```
┌──────────┐       ┌──────────┐       ┌──────────┐
│ MenuScene │──────▶│GameScene │──────▶│ GameOver │
└──────────┘ Start └────┬─────┘ Death └────┬─────┘
                        │                   │
                        └───────────────────┘
                              Menu
```

<details>
<summary><strong>Gameplay State Machine</strong></summary>

<br>

- **`MenuState`** — attached to the menu scene, calls `SceneManager.LoadScene("GameScene")` on button press
- **`GameState`** — lives in the game scene, increments `ElapsedTime` every frame, listens to `PlayerHealth.OnDeath`
- **On death**: saves best time → `PlayerPrefs`, freezes `Time.timeScale = 0`, fires `OnDeath` event
- **`RequestMenu()`** — resets time scale, loads `MenuScene`

</details>

---

### Player

<details open>
<summary><strong>Components</strong></summary>

<br>

| File | What it does |
|------|-------------|
| `PlayerMovement` | Rigidbody movement on XZ plane. Auto-rotates to face the nearest enemy. Drives Animator with smoothed `MoveX`/`MoveZ`. |
| `PlayerWeapon` | Auto-targeting gun. Uses `Physics.OverlapSphereNonAlloc` to find enemies within range. Fires when the muzzle angle is within tolerance. Pulls bullets from `ObjectPool<Bullet>`. |
| `PlayerHealth` | Implements `IDamageable`. 100 HP default. Fires `OnDeath` once when health hits 0. |
| `Bullet` | Homing projectile — tracks target position each frame. On hit: deals damage, plays SFX, returns to pool. |
| `WeaponConfig` | ScriptableObject — `damage`, `fireRate`, `range`, `bulletSpeed`. |

</details>

---

### Enemies

<details open>
<summary><strong>Components</strong></summary>

<br>

| File | What it does |
|------|-------------|
| `Enemy` | Implements `IDamageable` + `IPoolable`. Moves toward player, deals melee damage. Despawns if too far away. |
| `EnemySpawner` | Keeps `minEnemiesOnField` alive. Spawns off-screen in a ring around the player. Uses `ObjectPool<Enemy>`. |
| `EnemyConfig` | ScriptableObject — `maxHealth`, `moveSpeed`, `damage`, `attackRange`, `attackInterval`, `despawnDistance`. |

</details>

> **Adding new enemy types:** right-click in the Project window → `Create > Game > Enemy Config`, then drag it into the spawner's `enemyConfigs` array.

---

### World Generation

`WorldGenerator` scatters trees, rocks, and bushes within a square area while keeping a clear radius around the player. Seeded `System.Random` ensures reproducible layouts.

> Use the editor menu **Tools → Create World Generator** to auto-assign prefabs from the `Low_Poly_Nature_Pack_Lite` folder.

---

### Object Pooling

Both bullets and enemies are managed by a generic `ObjectPool<T>` — stack-based, pre-allocated, with `OnSpawn()` / `OnDespawn()` lifecycle hooks. Zero `Instantiate` / `Destroy` calls during gameplay.

---

## Project Structure

```
Assets/_EndlessSienge/Scripts/
│
├── Core/
│   ├── IDamageable.cs          ← universal damage contract
│   ├── IPoolable.cs            ← pool lifecycle hooks
│   ├── IMovementInput.cs       ← input abstraction
│   ├── ObjectPool.cs           ← generic object pool
│   └── CameraFollow.cs         ← smoothed camera follow
│
├── Gameplay/
│   ├── MenuState.cs            ← scene loader
│   └── GameState.cs            ← timer + death + best time
│
├── Player/
│   ├── PlayerMovement.cs       ← movement + auto-rotate
│   ├── PlayerWeapon.cs         ← auto-targeting gun
│   ├── PlayerHealth.cs         ← HP + death event
│   ├── Bullet.cs               ← homing projectile
│   └── WeaponConfig.cs         ← ScriptableObject
│
├── Enemies/
│   ├── Enemy.cs                ← poolable enemy entity
│   ├── EnemySpawner.cs         ← continuous off-screen spawning
│   └── EnemyConfig.cs          ← ScriptableObject
│
├── UI/
│   ├── SurvivalUI.cs           ← timer + game over panel
│   ├── HealthBar.cs            ← reusable fill HP bar
│   ├── BestTimeText.cs         ← record display
│   └── JoystickPackAdapter.cs  ← wraps Joystick Pack
│
├── Utils/
│   ├── SfxPlayer.cs            ← one-shot SFX helper
│   └── MusicPlayer.cs          ← looped BGM
│
└── Editor/
    └── WorldGeneratorMenu.cs   ← editor menu tool
```

---

## Interfaces at a Glance

```
┌─────────────────────────────────────────────────┐
│  IDamageable                                    │
│  ├── PlayerHealth                               │
│  └── Enemy                                      │
│  → Same HealthBar works on both                 │
├─────────────────────────────────────────────────┤
│  IMovementInput                                 │
│  └── JoystickPackAdapter                        │
│  → Swap to keyboard/gamepad by implementing one │
├─────────────────────────────────────────────────┤
│  IPoolable                                      │
│  ├── Bullet                                     │
│  └── Enemy                                      │
│  → Enables ObjectPool<T> recycling              │
└─────────────────────────────────────────────────┘
```

---

## Getting Started

1. Clone the repo and open in **Unity 6+**
2. Open `MenuScene` or `GameScene`
3. In `GameScene`, ensure a `WorldGenerator` object exists
   - If not: **Tools → Create World Generator**
4. Create enemy configs: **Assets → Create → Game → Enemy Config**
5. Assign configs to the `EnemySpawner` component
6. Build & run on Android or test in the Editor

---

## Third-Party Assets

| Asset | Purpose |
|-------|---------|
| [Joystick Pack](https://assetstore.unity.com/packages/tools/integration/joystick-pack-113938) | Mobile joystick input |
| [Bloodlines UI](https://assetstore.unity.com/packages/tools/gui/bloodlines-ui-264777) | UI framework |
| [Low Poly Nature Pack Lite](https://assetstore.unity.com/packages/3d/environments/low-poly-nature-pack-lite-163060) | Trees, rocks, bushes |
| [Character Pack Lowpoly](https://assetstore.unity.com/packages/3d/characters/character-pack-lowpoly-free-8701) | Player model |

---

<p align="center">
  <sub>Made with Unity</sub>
</p>
