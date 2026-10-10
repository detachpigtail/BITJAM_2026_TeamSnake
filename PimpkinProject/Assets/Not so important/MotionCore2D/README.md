# MotionCore 2D Basic

Open `Assets/MotionCore2D/Demo/Scenes/PlatformerDemo.unity` and press Play.

That is the fastest way to see everything working. Move with WASD or arrow keys (or a
gamepad stick), and jump with Space (or the gamepad south button). Hold jump for a higher jump.
Hold Left Ctrl or C (or the gamepad east button) to crouch, which you need in order to get through
the low tunnel on the upper platform.

MotionCore 2D Basic is a focused 2D platformer character controller for Unity 6, built on the
Input System.

## Features

- Horizontal movement with acceleration and deceleration
- Air control
- Manual gravity (gravityScale = 0, gravity driven from the movement profile)
- Fall multiplier and maximum fall speed
- Jump with variable jump height, reaching the configured Jump Height exactly
- Coyote time
- Jump buffering
- Crouching with a lowered collider and a ceiling check
- Box-cast ground detection (normal, angle, distance)
- Walkable-slope handling
- Input from the Input System, or from your own code via `ScriptedMotionInput` (AI, cutscenes,
  touch controls, tests)
- Events and published state for animation and audio, including landing impact speed
- Inspector-wireable UnityEvents via `MotionEventRelay`, so feedback needs no code
- Debug visualization (scene gizmos)
- A ready-to-run demo scene and Player prefab
- Example scripts for animation, AI patrol movement, and respawning

## Requirements

- Unity 6000.2 or newer
- Input System package (`com.unity.inputsystem`) - the only dependency

Set Active Input Handling to "Input System Package" or "Both" in
Project Settings > Player.

## Install

Import the `MotionCore2D` folder into your project's `Assets`. Everything the package needs,
including the demo input actions, lives under `Assets/MotionCore2D`.

## Extending it

Nothing needs to be modified inside the package:

- Add a mechanic by implementing `IMotionStepProvider`; your steps join the ordered velocity update.
- Add an input source by implementing `IMotionInput` (four members).
- React to motion through `MotionController2D.State`, its `Landed` / `LeftGround` events, and
  `JumpController2D.Jumped`.

## Documentation

- [Start Here](START_HERE.md)
- [Quick Start](Documentation/QuickStart.md)
- [Setup Guide](Documentation/SetupGuide.md)
- [Tuning Guide](Documentation/TuningGuide.md)
- [Examples](Documentation/Examples.md)
- [Troubleshooting](Documentation/Troubleshooting.md), [FAQ](Documentation/FAQ.md)
- [Changelog](CHANGELOG.md)
- [License](LICENSE.md) and [Third-Party Notices](Third-Party-Notices.md)

## What this is not

MotionCore 2D Basic is the core movement layer, intentionally scoped. It does not include double
jump, dash, wall jump, ledge grab, moving or one-way platforms, ladders, combat, enemies, or mobile
touch controls. The traversal and combat mechanics are in MotionCore 2D Pro, which composes on top of
Basic rather than replacing it, so a Basic character keeps working when you add Pro.

## Support

Use the support contact listed on the Asset Store publisher page. Include your Unity version,
MotionCore version, console errors, and a short description of your character setup.


