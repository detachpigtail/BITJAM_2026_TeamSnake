# Tuning Guide

Every value lives on the MovementProfile asset. There are no hardcoded gameplay values, so all
feel comes from here. Values are clamped to safe ranges automatically. The profile inspector also
shows the derived jump launch velocity and time to apex.

Units: speeds are world units per second; accelerations are world units per second, per second.

## Horizontal

| Field | Meaning | Increasing it... |
| --- | --- | --- |
| Max Speed | Top horizontal speed. | runs faster. |
| Acceleration | Rate of speeding up toward Max Speed. | snappier starts. |
| Deceleration | Rate of slowing when input is released **or reversed**. | quicker stops and sharper turns. |
| Air Control Factor | Fraction of accel/decel applied while airborne (0 to 1). | more mid-air steering. |

## Jump

| Field | Meaning |
| --- | --- |
| Jump Height | Apex height of a full jump, in world units. This is a measured guarantee, not an approximation: the launch velocity is derived from Jump Height and Gravity and corrected for the fixed timestep, so a full jump peaks within a couple of millimetres of the value you type. |
| Variable Jump Cut Multiplier | Fraction of upward velocity kept when the button is released early (0 to 1). Lower = shorter taps. |

Jumping needs a Jump Controller 2D component on the actor. Without one the actor simply cannot
jump, and these values do nothing.

## Crouch

Requires a Crouch Controller 2D component on the actor.

| Field | Meaning |
| --- | --- |
| Crouch Speed Multiplier | Top horizontal speed while crouched, as a fraction of Max Speed (0 = cannot move while crouched, 1 = no slowdown). |
| Crouch Height Factor | Crouched collider height as a fraction of standing height. The feet stay planted; the top is lowered. The actor cannot stand up again while a ceiling is in the way. |

## Gravity

| Field | Meaning |
| --- | --- |
| Gravity | Downward acceleration while rising. Must be positive. |
| Fall Gravity Multiplier | Extra gravity while descending (1 or more). Higher = snappier, less floaty falls. |
| Max Fall Speed | Terminal downward speed. |

## Assist

| Field | Meaning |
| --- | --- |
| Coyote Time | Grace period after leaving ground during which a jump still counts (seconds). |
| Jump Buffer Time | Window before landing during which a jump press is remembered and fires on contact (seconds). |

Both are assists that widen an opportunity the player already had, so **either can safely be set to
0** for a precision-platformer feel. Zeroing Coyote Time only removes the post-ledge grace period;
zeroing Jump Buffer Time only stops presses being remembered. A grounded press still jumps in both
cases.

## Slope

| Field | Meaning |
| --- | --- |
| Max Slope Angle | Steepest slope (degrees) treated as walkable. On walkable slopes the actor follows the surface and neither slides nor launches. Steeper ground is not climbed. |

## A good starting point

The defaults are tuned for a responsive platformer: Max Speed 8, Acceleration and Deceleration 60,
Jump Height 3.5, Gravity 40, Fall Multiplier 1.6, Coyote 0.1s, Buffer 0.12s. Tune Jump Height and
Gravity together: raising gravity makes a given height feel snappier but needs a higher launch.

Because Jump Height is honoured exactly, Gravity is the dial for *feel* and Jump Height the dial for
*reach*. Raise both together to keep the same jump arc while making it faster, and check the Time To
Apex readout in the profile inspector to see the result.
