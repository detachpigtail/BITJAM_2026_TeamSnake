# FAQ

## Scope

**What is deliberately not included?**
Double jump, dash, wall jump, ledge grab, moving and one-way platforms, ladders, swimming, combat,
enemies, and mobile touch controls. Basic is the core movement layer: run, jump, gravity, ground and
slope handling, and crouch. The traversal and combat mechanics live in MotionCore 2D Pro, which
composes on top of Basic rather than replacing it.

**Can I add my own mechanic without modifying the package?**
Yes, and that is the intended route. Implement `IMotionStepProvider` on a component on the actor and
return one or more `IMotionStep`s. They run after the built-in pipeline, so they see the
otherwise-final velocity for the step and can adjust it. The controller needs no knowledge of them,
so your code survives package updates.

**Is it 2D only?**
Yes. It drives a `Rigidbody2D` through `CharacterMotor2D` and uses 2D physics queries throughout.

## Setup and compatibility

**Do I have to use the Input System?**
For player-controlled actors, yes - `InputSystemReader` is built on it. For everything else, use
`ScriptedMotionInput`, which has no package requirement at all and works in any project
configuration: enemies, cutscenes, on-screen touch buttons, replays, networked remote actors, and
tests.

**Can I use my own input system?**
Implement `IMotionInput` on a component. It is four members: `Horizontal`, `JumpHeld`, `CrouchHeld`,
and `ConsumeJumpPressed()`. Nothing in the movement logic depends on where input comes from. Keep
one input provider enabled per actor; the controller's inspector warns if it finds more.

**Why is my Rigidbody2D gravity scale being reset to 0?**
MotionCore uses a single, explicit gravity model driven from the profile, so vertical motion has one
owner. `CharacterMotor2D` forces `gravityScale` to 0 on Awake and warns once if it was not already.
Tune `Gravity`, `Fall Gravity Multiplier`, and `Max Fall Speed` on the profile instead.

**Does it work with a Kinematic Rigidbody2D?**
Use Dynamic. The system writes `linearVelocity` and relies on the 2D solver for collision response
and depenetration, which Dynamic provides.

**Does it work in builds, or only in the editor?**
In builds. The only editor-only code is gizmo drawing and the custom inspectors, which are compiled
out of player builds.

## Behaviour

**Does the actor really jump exactly as high as Jump Height?**
Yes, to within about two millimetres at typical settings. The launch velocity is derived from Jump
Height and Gravity and corrected for the fixed timestep, and there is a test asserting the
integrated apex against the configured value.

**Can I set Coyote Time or Jump Buffer Time to 0?**
Yes. Both are assists that widen an opportunity the player already had, never preconditions for one.
Zeroing either sharpens the feel; a grounded press still jumps.

**How do I stop the actor jumping right after a respawn?**
Call `JumpController2D.ResetJumpState()` after moving it. That drops coyote credit, any buffered
press, and a jump in progress. `Examples/RespawnExample.cs` shows the whole pattern, including
zeroing velocity.

**How do I play a sound or effect on jump and landing?**
Either add `MotionEventRelay` and wire the UnityEvents in the Inspector with no code, or subscribe to
`JumpController2D.Jumped`, `MotionController2D.Landed`, and `MotionController2D.LeftGround` from
script. `MotionState.LandingImpactSpeed` gives the fall speed at touchdown, so feedback can scale
with the drop.

**How do I drive an Animator?**
Read `MotionController2D.State`: it publishes `IsGrounded`, `IsJumping`, `IsCrouching`, and
`Velocity` once per fixed step, so animation never has to re-derive movement state or inspect the
Rigidbody2D. `Examples/MotionAnimatorExample.cs` is a working starting point.

**Why does the actor slide down steep slopes instead of climbing them?**
Ground steeper than the profile's `Max Slope Angle` is not treated as walkable, by design.
`GroundDetector2D` reports the raw angle; the walkability judgement is a movement policy.

**Can several actors share one MovementProfile?**
Yes. A profile is read-only configuration; all mutable per-actor state lives in `MotionState`.
Editing a shared profile retunes every actor referencing it, which is usually what you want.

## Upgrading

**Will a package update overwrite my tuning?**
Your MovementProfile assets and scene setup are yours and are not touched by an update. Only the
files under `Assets/MotionCore2D` are replaced. Keep custom code in your own folders and hook in
through `IMotionStepProvider` or `IMotionInput` rather than editing package files.

**Is it safe to install Pro later?**
Yes. Pro adds components alongside Basic and never modifies it, so an existing Basic character keeps
working and gains Pro mechanics as you add them. See Pro's `Documentation/UpgradeFromBasic.md`.
