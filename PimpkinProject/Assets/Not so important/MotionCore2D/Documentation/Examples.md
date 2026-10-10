# Examples

The `Examples` folder holds small, self-contained scripts that show how to connect MotionCore to the
rest of a game. They are deliberately thin glue, not gameplay systems: read one, then copy the parts
you need into your own code rather than building on top of it.

None of them are required for MotionCore to work, and none of them change how the actor moves.

They live in their own assembly (`MotionCore2D.Examples`), so they can be deleted wholesale without
touching the package, and so they add no dependencies to it. `MotionAnimatorExample` is the one piece
that needs Unity's Animation module for the `Animator` type; it compiles itself out when that module
is not installed, rather than making MotionCore depend on it.

## MotionAnimatorExample

Drives an `Animator` and a `SpriteRenderer`'s facing from the actor's published state.

The point it demonstrates: **animation never needs to inspect the Rigidbody2D or re-derive movement
state.** Everything it reads is already resolved on `MotionState` once per fixed step, which keeps
the visuals in step with the simulation.

| Reads | Sets |
| --- | --- |
| `State.Velocity.x` | `Speed` float, sprite `flipX` |
| `State.Velocity.y` | `VerticalSpeed` float |
| `State.IsGrounded` | `Grounded` bool |
| `State.IsJumping` | `Jumping` bool |
| `State.IsCrouching` | `Crouching` bool |
| `JumpController2D.Jumped` | `Jump` trigger |
| `MotionController2D.Landed` | `Land` trigger |

Requires Unity's Animation module (`com.unity.modules.animation`), which is present in the default
project template. Without it this one example is compiled out and the rest of the package is
unaffected.

Setup:

1. Add it to an actor that already has `MotionController2D`.
2. Assign the Animator, or leave it empty to auto-detect on this object or a child.
3. Rename the parameter fields to match your Animator controller, or clear a field to skip it.

## PatrolInputExample

Walks an actor back and forth between two points by writing to a `ScriptedMotionInput`, turning
around early when there is no ground ahead so it does not walk off a ledge.

The point it demonstrates: **a non-player actor reuses the entire movement system** - acceleration,
air control, gravity, slopes, jumping - with no AI-specific support in the controller. It only sets
the same intent a player's controls would produce, which is why an enemy moves with the same feel as
the player.

Setup:

1. Build an actor as usual, but add `ScriptedMotionInput` instead of `InputSystemReader`.
2. Add `PatrolInputExample` and set Patrol Radius and Walk Speed Fraction.
3. Set Ground Layers to match the actor's `GroundDetector2D` so the ledge check agrees with it.

Select the actor to see the patrol span and the look-ahead probe drawn in the Scene view.

## RespawnExample

Teleports the actor to a checkpoint, or back to its start position, when it falls below a kill
height.

The point it demonstrates: **the small amount of tidying a correct teleport needs.** Moving the
transform alone leaves an actor that arrives still falling at terminal velocity, and possibly still
holding a jump press made just before it died - so it jumps on arrival. Zeroing velocity and calling
`JumpController2D.ResetJumpState()` is the whole fix.

Setup:

1. Add it to the actor.
2. Assign a Checkpoint transform, or leave it empty to respawn at the starting position.
3. Set Kill Height below your level, or turn off Use Kill Height and call `Respawn()` yourself from a
   death trigger or UnityEvent.
4. Wire the `Respawned` event to VFX, SFX, or a camera snap.

## Wiring feedback without code

For jump and landing audio, particles, or camera shake, you do not need an example script at all.
Add `MotionEventRelay` to the actor and wire its UnityEvents in the Inspector:

| Event | Raised when | Payload |
| --- | --- | --- |
| `Landed` | The actor touches down | Downward speed at impact |
| `LeftGround` | The actor becomes airborne | - |
| `Jumped` | A jump launches | - |
| `CrouchStarted` | The actor starts crouching | - |
| `CrouchEnded` | The actor stands back up | - |

The `Landed` payload lets feedback scale with the drop, so a short step down reads differently from a
long fall.
