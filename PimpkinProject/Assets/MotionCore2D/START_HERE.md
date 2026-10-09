# MotionCore 2D Basic - Start Here

## First Run

1. Set Active Input Handling to "Input System Package" or "Both"
   (Project Settings > Player). Restart the editor if Unity asks you to.
2. Open `Assets/MotionCore2D/Demo/Scenes/PlatformerDemo.unity`.
3. Press Play.

## Demo Controls

| Action | Keyboard | Gamepad |
| --- | --- | --- |
| Move | A / D or Arrow Keys | Left stick |
| Jump | Space | South button |
| Higher jump | Hold Space | Hold South button |
| Short hop | Tap and release Space | Tap South button |
| Crouch | Hold Left Ctrl or C | Hold East button |

Walk right to try each feature in turn: run up the slope, jump the gap to the ledge, and crouch to
get through the low tunnel. While you are under the tunnel, releasing crouch does nothing until you
are clear of it.

Select the Player in the Hierarchy to watch the ground probe, velocity, and ground normal draw live
in the Scene view.

## Use In Your Scene

1. Create tuning data: `Assets > Create > MotionCore 2D > Movement Profile`.
2. Add `MotionController2D` to your character and assign the profile. The inspector lists anything
   still missing, each with a one-click fix.
3. Set `GroundDetector2D`'s Ground Layers to your level geometry, and keep the character on its own
   layer.
4. Assign the Move and Jump actions on `InputSystemReader`, or use `ScriptedMotionInput` for an
   actor driven from code.
5. Add `JumpController2D` and `CrouchController2D` for the features you want. An actor without them
   simply cannot jump or crouch.

Full walkthrough: `Documentation/SetupGuide.md`.

## Required Setup

- Unity 6000.2 or newer.
- Input System package (`com.unity.inputsystem`) - the only dependency.
- Active Input Handling set to Input System Package or Both, unless you drive actors exclusively
  through `ScriptedMotionInput`.

## Tuning The Feel

Every gameplay value lives on the MovementProfile asset; nothing is hardcoded. The profile
inspector shows the resulting jump launch velocity and time to apex as you edit.
See `Documentation/TuningGuide.md`.

## Documentation

- `README.md` - package overview and feature list.
- `Documentation/QuickStart.md` - shortest path to a running demo.
- `Documentation/SetupGuide.md` - adding MotionCore to your own character.
- `Documentation/TuningGuide.md` - what every profile value does.
- `Documentation/Examples.md` - the scripts in `Examples/` and what each shows.
- `Documentation/Troubleshooting.md` - common setup and behaviour problems.
- `Documentation/FAQ.md` - scope, compatibility, and extension questions.

## Support

Use the support contact listed on the Asset Store publisher page. Include your Unity version,
MotionCore version, console errors, and a short description of your character setup.
