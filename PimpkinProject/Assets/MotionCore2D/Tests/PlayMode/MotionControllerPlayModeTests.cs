using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using MotionCore2D.Configuration;
using MotionCore2D.Core;
using MotionCore2D.Crouch;
using MotionCore2D.Jump;
using MotionCore2D.Physics;
using MotionCore2D.Sensing;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MotionCore2D.Tests.PlayMode
{
    /// <summary>
    /// Integration tests exercising the full <see cref="MotionController2D"/> pipeline against real
    /// 2D physics: movement, gravity, jump height and variable height, coyote time, jump buffering,
    /// crouching under a ceiling, ground detection, and slope handling.
    /// </summary>
    public sealed class MotionControllerPlayModeTests
    {
        private const int GroundLayer = 0;
        private const int PlayerLayer = 6;

        private readonly List<Object> _spawned = new List<Object>();

        private float _originalMaximumDeltaTime;

        [OneTimeSetUp]
        public void PinTimestep()
        {
            // These tests advance the simulation with WaitForFixedUpdate, which resumes once per
            // frame, not once per fixed step. On a loaded machine a single frame can cover several
            // fixed steps, so a sampling loop silently skips over the moment it is trying to
            // measure - an apex, or a landing between two samples. Capping the frame delta at one
            // fixed step makes the two line up, so the tests measure the same thing on any machine.
            _originalMaximumDeltaTime = Time.maximumDeltaTime;
            Time.maximumDeltaTime = Time.fixedDeltaTime;
        }

        [OneTimeTearDown]
        public void RestoreTimestep()
        {
            Time.maximumDeltaTime = _originalMaximumDeltaTime;
        }


        [TearDown]
        public void TearDown()
        {
            foreach (Object spawned in _spawned)
            {
                if (spawned != null)
                {
                    Object.DestroyImmediate(spawned);
                }
            }

            _spawned.Clear();
        }

        // --- Tests ---------------------------------------------------------

        [UnityTest]
        public IEnumerator GroundDetection_ReportsGroundedThenAirborneWhenLifted()
        {
            World world = BuildWorld(playerStartY: 1f);
            yield return Step(40);

            Assert.IsTrue(world.Controller.State.IsGrounded, "Should be grounded after settling.");

            world.Body.position = new Vector2(0f, 6f);
            yield return Step(10);

            Assert.IsFalse(world.Controller.State.IsGrounded, "Should be airborne after being lifted.");
        }

        [UnityTest]
        public IEnumerator Gravity_AcceleratesDownwardAndClampsToMaxFallSpeed()
        {
            World world = BuildWorld(playerStartY: 30f);
            yield return Step(40);

            float maxFall = world.Profile.MaxFallSpeed;
            Assert.Less(world.Body.linearVelocity.y, 0f, "Should be falling.");
            Assert.GreaterOrEqual(world.Body.linearVelocity.y, -maxFall - 0.5f, "Should not exceed max fall speed.");
            Assert.AreEqual(-maxFall, world.Body.linearVelocity.y, 1f, "Should clamp to max fall speed.");
        }

        [UnityTest]
        public IEnumerator HorizontalMovement_ReachesMaxSpeedAndMovesRight()
        {
            World world = BuildWorld(playerStartY: 1f);
            yield return Step(30);

            float startX = world.Body.position.x;
            world.Input.Horizontal = 1f;
            yield return Step(40);

            Assert.AreEqual(world.Profile.MaxSpeed, world.Body.linearVelocity.x, 0.5f, "Should reach max speed.");
            Assert.Greater(world.Body.position.x, startX + 1f, "Should have moved right.");
        }

        [UnityTest]
        public IEnumerator Jump_LaunchesUpwardAndGainsHeight()
        {
            World world = BuildWorld(playerStartY: 1f);
            yield return Step(30);

            float startY = world.Body.position.y;
            world.Input.JumpHeld = true;
            world.Input.QueueJump();

            float peak = startY;
            float maxUpVelocity = 0f;
            for (int i = 0; i < 60; i++)
            {
                yield return new WaitForFixedUpdate();
                peak = Mathf.Max(peak, world.Body.position.y);
                maxUpVelocity = Mathf.Max(maxUpVelocity, world.Body.linearVelocity.y);
            }

            // The jump's direct, deterministic effect is an upward launch velocity. Integrated peak
            // height over a fixed-frame window is sensitive to batch-mode physics timing, so assert
            // primarily on the launch velocity and require a clear height gain as well.
            Assert.Greater(maxUpVelocity, 2f, "Jump should produce a clear upward launch velocity.");
            Assert.Greater(peak, startY + 0.4f, "Jump should gain height above the start.");
        }

        [UnityTest]
        public IEnumerator Jump_PeaksNearTheConfiguredJumpHeight()
        {
            // Jump Height is a documented contract, not a loose hint. The pure state machine pins the
            // exact arithmetic; this checks the assembled controller honours it against real physics,
            // with enough slack for collision resolution settling the actor before the jump.
            World world = BuildWorld(playerStartY: 1f);
            yield return Step(40);

            float startY = world.Body.position.y;
            world.Input.JumpHeld = true;
            world.Input.QueueJump();

            float peak = startY;
            for (int i = 0; i < 90; i++)
            {
                yield return new WaitForFixedUpdate();
                peak = Mathf.Max(peak, world.Body.position.y);
            }

            float gained = peak - startY;
            Assert.AreEqual(world.Profile.JumpHeight, gained, 0.25f,
                $"A full jump should peak near Jump Height ({world.Profile.JumpHeight}), but gained {gained}.");
        }

        [UnityTest]
        public IEnumerator Jump_SinglePressDoesNotProduceASecondLaunchWhileRising()
        {
            // The ground probe still finds the floor for a step or two after a launch. A press held
            // across that window must not be spent again in mid-air.
            World world = BuildWorld(playerStartY: 1f);
            yield return Step(30);

            var launches = 0;
            world.Jump.Jumped += () => launches++;

            world.Input.JumpHeld = true;
            for (int i = 0; i < 10; i++)
            {
                world.Input.QueueJump(); // the player mashes jump through the launch
                yield return new WaitForFixedUpdate();
            }

            Assert.AreEqual(1, launches, "Only the first press should launch; the rest happen in mid-air.");
        }

        [UnityTest]
        public IEnumerator Jump_ReportsIsJumpingWhileRisingAndClearsOnLanding()
        {
            World world = BuildWorld(playerStartY: 1f);
            yield return Step(30);

            Assert.IsFalse(world.Controller.State.IsJumping, "Not jumping while standing still.");

            world.Input.JumpHeld = true;
            world.Input.QueueJump();

            bool reportedJumping = false;
            for (int i = 0; i < 5 && !reportedJumping; i++)
            {
                yield return new WaitForFixedUpdate();
                reportedJumping = world.Controller.State.IsJumping;
            }

            Assert.IsTrue(reportedJumping, "Should report a jump in progress after launching.");

            yield return Step(120); // rise, fall, land
            Assert.IsTrue(world.Controller.State.IsGrounded, "Should be back on the ground.");
            Assert.IsFalse(world.Controller.State.IsJumping, "The jump should be over once the actor lands.");
        }

        [UnityTest]
        public IEnumerator Jump_FiresTheJumpedEventOncePerJump()
        {
            World world = BuildWorld(playerStartY: 1f);
            yield return Step(30);

            var launches = 0;
            world.Jump.Jumped += () => launches++;

            world.Input.JumpHeld = true;
            world.Input.QueueJump();
            yield return Step(120); // full jump and landing
            Assert.AreEqual(1, launches, "One press, one Jumped event.");

            world.Input.QueueJump();
            yield return Step(60);
            Assert.AreEqual(2, launches, "A second press after landing should raise the event again.");
        }

        [UnityTest]
        public IEnumerator VariableJumpHeight_EarlyReleaseIsLowerThanFullHold()
        {
            float fullPeak = 0f;
            float tapPeak = 0f;

            World full = BuildWorld(playerStartY: 1f);
            yield return Step(30);
            float fullStart = full.Body.position.y;
            full.Input.JumpHeld = true;
            full.Input.QueueJump();
            for (int i = 0; i < 60; i++)
            {
                yield return new WaitForFixedUpdate();
                fullPeak = Mathf.Max(fullPeak, full.Body.position.y - fullStart);
            }

            TearDown();

            World tap = BuildWorld(playerStartY: 1f);
            yield return Step(30);
            float tapStart = tap.Body.position.y;
            tap.Input.JumpHeld = false; // released immediately
            tap.Input.QueueJump();
            for (int i = 0; i < 60; i++)
            {
                yield return new WaitForFixedUpdate();
                tapPeak = Mathf.Max(tapPeak, tap.Body.position.y - tapStart);
            }

            Assert.Greater(fullPeak, 0f);
            Assert.Greater(tapPeak, 0f);
            Assert.Less(tapPeak, fullPeak, "A tap should not jump as high as a full hold.");
        }

        [UnityTest]
        public IEnumerator CoyoteTime_AllowsJumpShortlyAfterLeavingGround()
        {
            World world = BuildWorld(playerStartY: 1f);
            yield return Step(30); // grounded: coyote armed

            world.Body.position = new Vector2(0f, 4f); // airborne, but within coyote window
            world.Input.JumpHeld = true;
            world.Input.QueueJump();

            bool launched = false;
            for (int i = 0; i < 6; i++)
            {
                yield return new WaitForFixedUpdate();
                if (world.Body.linearVelocity.y > 0f)
                {
                    launched = true;
                    break;
                }
            }

            Assert.IsTrue(launched, "Coyote-time jump should launch the actor upward.");
        }

        [UnityTest]
        public IEnumerator JumpBuffer_FiresOnLandingWhenPressedBeforeContact()
        {
            World world = BuildWorld(playerStartY: 0.6f); // just above ground, airborne
            world.Input.JumpHeld = true;
            world.Input.QueueJump(); // pressed before landing

            bool launchedAfterContact = false;
            for (int i = 0; i < 40; i++)
            {
                yield return new WaitForFixedUpdate();
                if (world.Body.linearVelocity.y > 0.5f)
                {
                    launchedAfterContact = true;
                    break;
                }
            }

            Assert.IsTrue(launchedAfterContact, "Buffered jump should fire on landing.");
        }

        [UnityTest]
        public IEnumerator Crouch_LowersTheColliderAndCapsSpeed()
        {
            World world = BuildWorld(playerStartY: 1f, withCrouch: true);
            yield return Step(30);

            float standingHeight = world.Collider.bounds.size.y;
            world.Input.Horizontal = 1f;
            world.Input.CrouchHeld = true;
            yield return Step(40);

            Assert.IsTrue(world.Controller.State.IsCrouching, "Holding crouch on the ground should crouch.");
            Assert.Less(world.Collider.bounds.size.y, standingHeight - 0.1f, "The collider should be shorter while crouched.");

            float cap = world.Profile.MaxSpeed * world.Profile.CrouchSpeedMultiplier;
            Assert.LessOrEqual(Mathf.Abs(world.Body.linearVelocity.x), cap + 0.1f, "Crouch-walking should respect the speed cap.");

            world.Input.CrouchHeld = false;
            yield return Step(20);

            Assert.IsFalse(world.Controller.State.IsCrouching, "Releasing crouch with headroom should stand up.");
            Assert.AreEqual(standingHeight, world.Collider.bounds.size.y, 0.01f, "The collider should return to its standing height.");
        }

        [UnityTest]
        public IEnumerator Crouch_CannotStandUpUnderALowCeiling()
        {
            // The headroom probe geometry is the part most easily got wrong, so exercise it against
            // real colliders rather than trusting the pure state machine alone.
            World world = BuildWorld(playerStartY: 1f, withCrouch: true);
            yield return Step(30);

            float standingHeight = world.Collider.bounds.size.y;
            float crouchedHeight = standingHeight * world.Profile.CrouchHeightFactor;

            world.Input.CrouchHeld = true;
            yield return Step(20);
            Assert.IsTrue(world.Controller.State.IsCrouching, "Should be crouched before the ceiling arrives.");

            // A ceiling with room for the crouched collider but not the standing one.
            var ceiling = new GameObject("Ceiling") { layer = GroundLayer };
            ceiling.transform.position = new Vector2(
                world.Body.position.x,
                world.Collider.bounds.min.y + crouchedHeight + 0.15f + 0.5f);
            ceiling.AddComponent<BoxCollider2D>().size = new Vector2(6f, 1f);
            _spawned.Add(ceiling);
            yield return Step(5);

            world.Input.CrouchHeld = false;
            yield return Step(30);

            Assert.IsTrue(world.Controller.State.IsCrouching, "Should stay crouched while the ceiling blocks standing.");

            Object.DestroyImmediate(ceiling);
            yield return Step(20);

            Assert.IsFalse(world.Controller.State.IsCrouching, "Should stand up once the ceiling is gone.");
            // Assert the restored height, not an absolute world position: the body settles into
            // Unity's contact offset over the test, which moves where the collider sits without
            // saying anything about whether the resize was undone correctly.
            Assert.AreEqual(standingHeight, world.Collider.bounds.size.y, 0.01f, "Standing should restore the standing height.");
        }

        [UnityTest]
        public IEnumerator Slope_ActorFollowsSurfaceAndStaysGrounded()
        {
            World world = BuildWorld(playerStartY: 2f, slopeAngle: 20f);
            yield return Step(40); // settle on slope

            float startY = world.Body.position.y;
            world.Input.Horizontal = 1f;
            yield return Step(40);

            Assert.IsTrue(world.Controller.State.IsGrounded, "Should stay grounded while walking the slope.");
            Assert.Greater(Mathf.Abs(world.Body.position.y - startY), 0.2f, "Height should track the slope as the actor moves.");
            // Walking up a 20-degree slope at max speed legitimately yields vy ~= speed*sin(20) ~= 2.7.
            // The actor staying grounded (asserted above) is the real "did not launch" check; this is
            // a sanity ceiling that still catches an actual launch spike off the surface.
            Assert.Less(world.Body.linearVelocity.y, 4f, "Should follow the slope, not launch off it.");
        }

        // --- Helpers -------------------------------------------------------

        private IEnumerator Step(int fixedSteps)
        {
            for (int i = 0; i < fixedSteps; i++)
            {
                yield return new WaitForFixedUpdate();
            }
        }

        private World BuildWorld(float playerStartY, float slopeAngle = 0f, bool withCrouch = false)
        {
            var ground = new GameObject("Ground") { layer = GroundLayer };
            ground.transform.SetPositionAndRotation(
                new Vector3(0f, -0.5f, 0f), Quaternion.Euler(0f, 0f, slopeAngle));
            ground.AddComponent<BoxCollider2D>().size = new Vector2(60f, 1f);
            _spawned.Add(ground);

            var profile = ScriptableObject.CreateInstance<MovementProfile>();
            SetPrivate(profile, "_coyoteTime", 0.15f);
            SetPrivate(profile, "_jumpBufferTime", 0.2f);
            profile.Validate();
            _spawned.Add(profile);

            var player = new GameObject("Player") { layer = PlayerLayer };
            player.SetActive(false);

            var body = player.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = 0f;
            body.freezeRotation = true;
            var collider = player.AddComponent<CapsuleCollider2D>();
            collider.size = new Vector2(0.7f, 1f);

            player.AddComponent<CharacterMotor2D>();

            var detector = player.AddComponent<GroundDetector2D>();
            SetPrivate(detector, "_groundLayers", (LayerMask)(1 << GroundLayer));

            var input = player.AddComponent<TestMotionInput>();
            var jump = player.AddComponent<JumpController2D>();

            if (withCrouch)
            {
                var crouch = player.AddComponent<CrouchController2D>();
                SetPrivate(crouch, "_bodyCollider", collider);
                SetPrivate(crouch, "_ceilingLayers", (LayerMask)(1 << GroundLayer));
            }

            var controller = player.AddComponent<MotionController2D>();
            SetPrivate(controller, "_profile", profile);

            player.transform.position = new Vector3(0f, playerStartY, 0f);
            player.SetActive(true);
            _spawned.Add(player);

            return new World(profile, controller, body, input, jump, collider);
        }

        private static void SetPrivate(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Field '{fieldName}' not found on {target.GetType().Name}.");
            field.SetValue(target, value);
        }

        private readonly struct World
        {
            public World(
                MovementProfile profile,
                MotionController2D controller,
                Rigidbody2D body,
                TestMotionInput input,
                JumpController2D jump,
                Collider2D collider)
            {
                Profile = profile;
                Controller = controller;
                Body = body;
                Input = input;
                Jump = jump;
                Collider = collider;
            }

            public MovementProfile Profile { get; }
            public MotionController2D Controller { get; }
            public Rigidbody2D Body { get; }
            public TestMotionInput Input { get; }
            public JumpController2D Jump { get; }
            public Collider2D Collider { get; }
        }
    }
}
