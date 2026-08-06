# Mindful Run — Unity 2D Prototype

A minimalist three-stage psychological obstacle-platformer prototype. It records gameplay behavior only; it does **not** diagnose psychology.

## Requirements and setup

- Unity **6000.3.19f1** (Unity 6 LTS line used by this project)
- Universal Render Pipeline 2D
- New Input System 1.19.0
- Reference resolution: 1920 × 1080

Open the repository folder in Unity Hub. The generated scenes and prefabs are committed and ready to run. Open `Assets/Scenes/MainMenu.unity` and press Play. If you need to regenerate the prototype assets, use **Tools → Mindful Run → Rebuild Prototype**.

## Controls

- `A` / Left Arrow — move left
- `D` / Right Arrow — move right
- `Space` — jump (release early for a shorter jump)
- `R` — restart the current stage
- `Escape` — pause / resume

## Scene order

`MainMenu` → `Stage_01` (movement) → `Stage_02` (timed spikes) → `Stage_03` (impulse test) → `PrototypeComplete`

## Architecture

- `PlayerController2D` — acceleration, air control, coyote time, input buffering, variable jump, fall tuning, and reliable ground casts.
- `StickFigureAnimator` — original geometric stick figure with procedural idle/run/jump/fall/land/death poses.
- `StageManager` — death freeze, restart, exit transition, input locking, and scene flow.
- `PlayerBehaviorTracker` / `StageBehaviorData` — serializable per-stage behavior record and JSON-shaped Console output.
- `TimedSpike`, `FallingBlock`, `Hazard`, and `ExitZone` — reusable obstacle pieces.
- `SmoothCameraFollow` — bounded, horizontal, orthographic camera follow.
- `PrototypeBuilder` — deterministic Editor generator for all scenes, visuals, and prefabs.

## Creating a stage

1. Duplicate the latest `Stage_XX` scene and rename it.
2. Set `StageManager.stageNumber` and `nextSceneName`.
3. Move the player spawn, platforms, camera bounds, and exit.
4. Add the scene to Build Settings in the intended order.

The scene generator is intended for the three supplied prototype scenes; do not run it after hand-editing those scenes unless you want them rebuilt.

## Reusable obstacles

Drag prefabs from `Assets/Prefabs/Obstacles` into a stage:

- `StaticSpike` — always lethal.
- `TimedSpike` — cycles safe → orange warning → red lethal; timing is Inspector-configurable.
- `FallingBlock` — only triggers when entered above its speed threshold, shows a warning shadow, then falls.
- `ProjectileLauncher` and `MovingPlatform` — intentionally lightweight expansion placeholders with exposed settings.
- `ExitZone` — completes the current stage.

## Behavior tracking

At stage completion, `PlayerBehaviorTracker` logs JSON to the Unity Console with stage start time, first-input delay, moving/stationary duration, left/right inputs, jumps, deaths, restarts, death-to-restart delay, and completion time. `StageBehaviorData` is serializable so future JSON persistence can be added without changing gameplay code.

## Known limitations

- Visuals and audio are deliberately prototype-level; no external copyrighted art is used.
- Behavior data is Console-only and is not persisted between play sessions.
- Placeholder launcher does not spawn projectiles; the moving-platform placeholder provides only simple ping-pong motion.
- Keyboard input is the only configured control scheme.
