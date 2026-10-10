# Changelog

All notable changes to MotionCore 2D Basic are documented here. This project follows Semantic
Versioning.

## [Unreleased]

### Added
- Crouching (optional CrouchController2D): lowers the collider with the feet planted, caps
  horizontal speed to a profile crouch fraction, and prevents standing up under a low ceiling via
  an overhead headroom probe. Decision logic lives in a pure, unit-tested CrouchStateMachine; the
  pipeline runs Horizontal -> Crouch -> Gravity -> Jump -> SlopeProjection.
- IMotionInput.CrouchHeld intent, read by InputSystemReader from an optional crouch action.
- MotionState.IsCrouching flag for animation and gameplay to observe.
- MovementProfile crouch tuning: Crouch Speed Multiplier and Crouch Height Factor.
- IMotionStepProvider: a generic extension point letting components (in this or another assembly)
  contribute pipeline steps. Provider steps run after the built-in pipeline, so add-ons such as Pro
  combat can adjust the otherwise-final velocity without the controller knowing about them.
- JumpController2D.Jumped event, raised on the step a jump launches, for jump audio, particles, and
  squash-and-stretch without polling velocity.
- JumpController2D.IsRising and MotionState.IsJumping, distinguishing a rising jump from any other
  upward motion (which a velocity check alone cannot do).
- JumpController2D.ResetJumpState() and JumpStateMachine.Reset(), to drop coyote credit, a buffered
  press, and an in-progress ascent after teleporting or respawning an actor.
- MotionController2D.Profile is now settable. Assigning a profile revalidates the setup, so an actor
  spawned without one starts moving as soon as a valid profile arrives instead of staying inert.
- MovementProfile inspector shows Time To Apex alongside the launch velocity.
- InputSystemReader "Jump Press Memory": how long an unconsumed jump press stays remembered.
- CrouchController2D custom inspector, validating the collider to resize and the ceiling layers
  (empty, or including the actor's own layer) with one-click fixes.
- The demo now covers crouching: a Crouch action (Left Ctrl / C / gamepad east) in the demo input
  asset, a CrouchController2D on the Player prefab, and a low tunnel in the scene that can only be
  crossed while crouched. Build Demo Scene generates all of it.
- Play-mode integration tests for the jump height contract, the single-press-one-jump rule, the
  Jumped event, IsJumping reporting, and crouching under a ceiling. The play-mode suites also cap the frame
  delta at one fixed step, so a sampling loop measures the same thing on a loaded machine as on an
  idle one.
- ScriptedMotionInput: a supported IMotionInput driven from code rather than a device, for enemies
  and NPCs, cutscenes, on-screen touch controls, replays, networked actors, and tests. Unlike
  InputSystemReader it has no Input System or Active Input Handling requirement, so an actor can be
  driven in any project configuration.
- MotionEventRelay: exposes Landed, LeftGround, Jumped, CrouchStarted, and CrouchEnded as
  UnityEvents, so jump and landing audio, particles, and camera shake can be wired in the Inspector
  without writing code.
- MotionState.LandingImpactSpeed: the downward speed at the moment of landing, captured before
  gravity clamps against the ground. Landing feedback can scale with the drop instead of treating
  every landing alike.
- Examples folder with three documented scripts: MotionAnimatorExample (drive an Animator from
  published state), PatrolInputExample (an AI patroller reusing the full movement system through
  ScriptedMotionInput), and RespawnExample (what a correct teleport has to clear). They build as
  their own assembly so they can be deleted wholesale, and so they add no dependency to the package:
  the Animator example compiles itself out where Unity's Animation module is absent, leaving the
  Input System as the package's only dependency.
- START_HERE.md, Documentation/FAQ.md, and Documentation/Examples.md; README gained "Extending it",
  "What this is not", and Support sections.
- Custom inspectors now offer buttons that open the package's own guides. They read the shipped
  Markdown from disk, so they work offline.
- MotionController2D's inspector warns when an actor has more than one IMotionInput provider, which
  previously made the effective input depend on component order.

### Changed
- Pipeline order is now Horizontal -> Crouch -> Gravity -> Jump -> SlopeProjection; gravity moved
  ahead of the jump step so a launch is the last word on vertical velocity for the step. Custom
  IMotionStep implementations still run after all of these, unchanged.
- JumpStateMachine.Result gained a Launched flag and the clearer Launch / Cut factory methods.
  SetVerticalVelocity still works as an obsolete alias for Launch and will be removed in 2.0.

### Fixed
- Jumps fell about 5% short of the configured Jump Height. Gravity was applied after the launch, so
  every jump lost a step of acceleration before the body had integrated it at all. Gravity now runs
  before the jump step and the launch velocity compensates for the half step that fixed-step
  integration adds, bringing the apex within about two millimetres of Jump Height.
- Setting Jump Buffer Time to 0 disabled jumping entirely, and so did setting Coyote Time to 0.
  Both windows are now treated as assists that widen an opportunity rather than as preconditions for
  one, so a grounded press always jumps and either window can be zeroed for a precision feel.
- A single press could fire a second jump in mid-air. For a step or two after a launch the ground
  probe still finds the surface the actor just left, which re-armed coyote time. Ground is now
  ignored while a launch is still carrying the actor upward.
- Variable jump height silently stopped working for the same reason: ground still being reported
  ended the ascent, so releasing the button no longer cut the jump short. Most visible on low Jump
  Height values, where the actor does not clear the ground probe in one step.
- Coyote time and a buffered press were banked while the pipeline was suspended (by
  SuspendPipeline, or a Pro traversal ability), granting a free jump out of a ledge left seconds
  earlier. The assist windows now expire across a suspension, while an ascent already under way is
  preserved so variable jump height survives a brief interruption.
- An unconsumed jump press was remembered indefinitely and fired a jump whenever something started
  reading input again. InputSystemReader now forgets a press after Jump Press Memory (0.2s by
  default) and drops any pending press when disabled.
- MotionState.Velocity went stale while the pipeline was suspended, so consumers reading it for
  animation saw the velocity from before control was handed over. The shared state is now kept an
  honest read-only view every step, whether or not the pipeline runs.
- Deceleration only applied when input was released, not when it was reversed, despite documenting
  both. Turning around charged the acceleration rate, so a deliberately heavy actor still turned on
  a dime.
- GroundDetector2D could report a zero-length surface normal when its cast began already overlapping
  a collider. The normal is now normalised, with world up substituted for a degenerate reading.

## [1.0.0]

Initial release.

### Added
- MotionController2D orchestrator running an ordered step pipeline
  (Horizontal -> Jump -> Gravity -> SlopeProjection) and writing the body once per fixed step.
- Horizontal movement with acceleration, deceleration, and air control.
- Manual gravity model (gravityScale = 0 plus profile gravity) with fall multiplier and maximum
  fall speed.
- Jumping with variable jump height, coyote time, and jump buffering (optional JumpController2D).
- GroundDetector2D box-cast sensing: grounded state, surface normal, slope angle, and distance.
- Walkable-slope handling via velocity projection along the surface tangent.
- CharacterMotor2D as the single Rigidbody2D gateway.
- InputSystemReader input abstraction; movement logic depends only on IMotionInput.
- MovementProfile ScriptableObject holding all tuning, with validation.
- Custom inspectors with setup validation and one-click fixes.
- Debug visualization (MotionDebugView, gizmos).
- Self-contained demo: package-owned input actions, Player prefab, and platformer scene with
  ground, a slope, and a gap, plus a menu command to regenerate them.
- Edit-mode unit tests (jump logic, profile validation, context) and play-mode integration tests.
