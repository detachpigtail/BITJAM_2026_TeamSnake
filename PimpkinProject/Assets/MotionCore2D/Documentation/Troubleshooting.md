# Troubleshooting

MotionCore validates its setup in the inspector and on Play, with clear messages. Common cases:

## Inspector / Console errors (component disables itself)

| Message | Cause | Fix |
| --- | --- | --- |
| requires a MovementProfile | No profile on MotionController2D. | Assign or create a Movement Profile (button in the inspector). |
| requires a component implementing IMotionInput | No input source. | Add InputSystemReader (button in the inspector). |
| requires a component implementing IGroundSensor | No ground sensor. | Add GroundDetector2D (button in the inspector). |
| requires a component implementing IMotionBody | No motor. | Add CharacterMotor2D. |

## Warnings

| Message | Meaning |
| --- | --- |
| overriding Rigidbody2D.gravityScale to 0 | MotionCore uses manual gravity and reset the scale for you. Expected. The inspector offers a button to set it in edit mode too. |
| profile gravity is zero or negative; the actor will not fall | Set Gravity to a positive value in the profile. |

## Behaviour problems

- Character falls through / never grounded: GroundDetector2D Ground Layers is empty or excludes
  your level. Set it to your ground layer(s). The inspector warns and offers a one-click fix.
- Character detects itself as ground: the character's layer is included in Ground Layers. Remove
  it (inspector button) or move the character to its own layer.
- No movement at all: input not assigned (set Move/Jump on InputSystemReader), or Active Input
  Handling is not set to Input System / Both.
- Floaty or instant jumps: tune Jump Height, Gravity, and Fall Gravity Multiplier together - see
  the [Tuning Guide](TuningGuide.md).
- Cannot jump at all: the actor needs a Jump Controller 2D component. Without one, jump input is
  read but nothing acts on it.
- Jump does not reach Jump Height: it should, to within a couple of millimetres. If it falls short,
  something else is writing the Rigidbody2D velocity - check for other scripts setting
  `linearVelocity`, and for a second component implementing IMotionInput.
- Releasing the jump button does not shorten the jump: Variable Jump Cut Multiplier is 1 (keep
  everything) or the actor is no longer rising by the time you release. A cut applies once per jump,
  while rising only.
- Jump fires by itself a moment after you press: something is not consuming jump input. If an
  ability suspends the pipeline (MotionCore 2D Pro), InputSystemReader forgets an unread press after
  its Jump Press Memory window; lower that value if a remembered press still feels late.
- Launches off the top of slopes / slides down: confirm the slope is within Max Slope Angle;
  steeper ground is intentionally not walkable.
- Jitter on slopes/ground: set the Rigidbody2D Interpolate to Interpolate and Collision Detection
  to Continuous (the demo prefab already does this).
- Demo objects appear pink: reassign
  `Assets/MotionCore2D/Demo/Materials/DemoSpriteMaterial.mat` to the affected SpriteRenderers, or
  run `Tools > MotionCore 2D > Build Demo Scene` to regenerate the known-good demo assets.
