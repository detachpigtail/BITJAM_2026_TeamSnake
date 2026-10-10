# Setup Guide

How to add MotionCore 2D Basic to your own character. The custom inspectors flag any missing
piece with a one-click fix, so you can also just add MotionController2D and follow the prompts.

## 1. Create a movement profile

Assets > Create > MotionCore 2D > Movement Profile. This asset holds all tuning (see the
[Tuning Guide](TuningGuide.md)). One profile can be shared by many actors.

## 2. Build the character GameObject

Add these components (required components are added automatically):

| Component | Role |
| --- | --- |
| Rigidbody2D | The physics body. Body Type = Dynamic. CharacterMotor2D sets gravityScale to 0 at runtime. |
| Collider2D (Capsule recommended) | The character's shape. |
| CharacterMotor2D | The only component that touches the Rigidbody2D. |
| GroundDetector2D | Box-cast ground sensing. |
| InputSystemReader | Reads the Input System. |
| MotionController2D | The orchestrator. Assign the Movement Profile here. |
| JumpController2D (optional) | Adds jumping. Omit it for a non-jumping actor. |
| CrouchController2D (optional) | Adds crouching. Omit it for an actor that cannot crouch. |
| MotionDebugView (optional) | Scene-view gizmos. |

Each optional feature contributes a step to the controller's pipeline, which runs in a fixed order:
horizontal movement, crouch, gravity, jump, slope projection. MotionController2D is the only
component that writes to the Rigidbody2D, so there is exactly one velocity update per fixed step.

## 3. Configure ground detection

On GroundDetector2D:

- Ground Layers: the layer(s) your level geometry uses. Must not include the character's own
  layer, or the probe detects itself. The inspector warns you if it does.
- Origin Offset: place the probe at the character's feet.
- Probe Size / Distance: a box slightly narrower than the collider, reaching a little below the
  feet.

## 4. Configure input

InputSystemReader needs two actions assigned, and takes a third if the actor crouches:

- Move: a Value action of control type Vector2 (its X axis is used).
- Jump: a Button action.
- Crouch (optional): a Button action, held to crouch. Leave it unset for an actor that cannot crouch.

The package includes a ready-made input asset at
`Assets/MotionCore2D/Demo/Input/MotionCore2D_Demo.inputactions` with Move and Jump already set up.
You can assign actions from it, or use your own input asset. The referenced actions are enabled
automatically while the component is enabled.

## 5. Press Play

The character rests on the ground, moves horizontally, and (with JumpController2D) jumps. If
anything is misconfigured, the inspector and Console explain the problem and the fix - see
[Troubleshooting](Troubleshooting.md).

## 6. React to the character (optional)

For audio, particles, or animation, read state and events rather than inspecting the Rigidbody2D:

```csharp
var controller = GetComponent<MotionController2D>();
var jump = GetComponent<JumpController2D>();

controller.Landed += PlayLandingDust;
controller.LeftGround += StopFootsteps;
jump.Jumped += PlayJumpSound;

void Update()
{
    MotionState state = controller.State;
    _animator.SetBool("Grounded", state.IsGrounded);
    _animator.SetBool("Crouching", state.IsCrouching);
    _animator.SetBool("Jumping", state.IsJumping);
    _animator.SetFloat("Speed", Mathf.Abs(state.Velocity.x));
}
```

`MotionState.IsJumping` is true only while a jump is carrying the actor upward, so it distinguishes
a jump from any other upward motion. After teleporting or respawning an actor, call
`JumpController2D.ResetJumpState()` so a press made before the move cannot fire after it.
