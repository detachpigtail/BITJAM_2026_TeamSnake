# Quick Start

## Run the demo (about 1 minute)

1. In Unity, set Active Input Handling to "Input System Package" or "Both"
   (Project Settings > Player). Restart the editor if Unity asks you to.
2. Open `Assets/MotionCore2D/Demo/Scenes/PlatformerDemo.unity`.
3. Press Play.
4. Controls:
   - Move: WASD, arrow keys, or a gamepad left stick
   - Jump: Space, or the gamepad south button (A / Cross). Hold for a higher jump.
   - Crouch: Left Ctrl or C, or the gamepad east button (B / Circle). Hold it down.
5. Try each feature: run up the slope, jump the gap to the ledge, and crouch to get through the
   low tunnel over the upper platform. While you are under the tunnel, releasing crouch does
   nothing until you are clear of it.
6. Select the Player in the Hierarchy to see the ground probe and velocity gizmos in the
   Scene view.

The demo uses only assets inside `Assets/MotionCore2D`, including its own input actions at
`Demo/Input/MotionCore2D_Demo.inputactions`. There is no external setup.

## Regenerate the demo (optional)

If the demo assets are ever removed, choose the menu
Tools > MotionCore 2D > Build Demo Scene to rebuild the profile, Player prefab, and scene.

## Build your own character

When you are ready to add MotionCore to your own object, follow the
[Setup Guide](SetupGuide.md), then adjust feel with the [Tuning Guide](TuningGuide.md).
