using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using MotionCore2D.Configuration;
using MotionCore2D.Core;
using MotionCore2D.Crouch;
using MotionCore2D.Events;
using MotionCore2D.Input;
using MotionCore2D.Jump;
using MotionCore2D.Physics;
using MotionCore2D.Sensing;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MotionCore2D.Tests.PlayMode
{
    /// <summary>
    /// Integration tests for the surfaces other systems build on: driving an actor from code with
    /// <see cref="ScriptedMotionInput"/>, the landing impact speed published for feedback, and the
    /// Inspector-facing <see cref="MotionEventRelay"/>.
    /// </summary>
    public sealed class MotionIntegrationPlayModeTests
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

        // --- ScriptedMotionInput -------------------------------------------

        [UnityTest]
        public IEnumerator ScriptedInput_MovesTheActorHorizontally()
        {
            World world = BuildWorld(playerStartY: 1f);
            yield return Step(30);

            float startX = world.Body.position.x;
            world.Input.Horizontal = 1f;
            yield return Step(40);

            Assert.AreEqual(world.Profile.MaxSpeed, world.Body.linearVelocity.x, 0.5f,
                "Scripted input should reach the same top speed a player would.");
            Assert.Greater(world.Body.position.x, startX + 1f);
        }

        [UnityTest]
        public IEnumerator ScriptedInput_JumpLaunchesTheActor()
        {
            World world = BuildWorld(playerStartY: 1f);
            yield return Step(30);

            float startY = world.Body.position.y;
            world.Input.Jump();

            float peak = startY;
            for (int i = 0; i < 60; i++)
            {
                yield return new WaitForFixedUpdate();
                peak = Mathf.Max(peak, world.Body.position.y);
            }

            Assert.Greater(peak, startY + 1f, "Jump() should produce a real jump.");
        }

        [UnityTest]
        public IEnumerator ScriptedInput_ClearStopsTheActor()
        {
            World world = BuildWorld(playerStartY: 1f);
            yield return Step(30);

            world.Input.Horizontal = 1f;
            yield return Step(30);
            Assert.Greater(world.Body.linearVelocity.x, 1f, "Should be moving before the clear.");

            world.Input.Clear();
            yield return Step(40);

            Assert.AreEqual(0f, world.Body.linearVelocity.x, 0.2f, "Clearing intent should let the actor stop.");
        }

        // --- Landing impact ------------------------------------------------

        [UnityTest]
        public IEnumerator LandingImpactSpeed_ReportsTheFallSpeedAndSurvivesTheGroundClamp()
        {
            // Dropped from a height, so the actor is falling fast at touchdown. The value has to
            // survive gravity zeroing vertical velocity against the ground in the same step.
            World world = BuildWorld(playerStartY: 12f);

            for (int i = 0; i < 200 && !world.Controller.State.IsGrounded; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            Assert.IsTrue(world.Controller.State.IsGrounded, "Should have landed.");
            Assert.Greater(world.Controller.State.LandingImpactSpeed, 5f,
                "A long drop should report a substantial impact speed.");
            Assert.AreEqual(0f, world.Body.linearVelocity.y, 0.01f,
                "Vertical velocity is still clamped on landing; only the recorded impact survives.");
        }

        [UnityTest]
        public IEnumerator LandingImpactSpeed_IsLargerForALongerDrop()
        {
            World shallow = BuildWorld(playerStartY: 1.6f);
            for (int i = 0; i < 200 && !shallow.Controller.State.IsGrounded; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            float shallowImpact = shallow.Controller.State.LandingImpactSpeed;
            TearDown();

            World deep = BuildWorld(playerStartY: 12f);
            for (int i = 0; i < 200 && !deep.Controller.State.IsGrounded; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            Assert.Greater(deep.Controller.State.LandingImpactSpeed, shallowImpact + 1f,
                "Impact speed should scale with the drop, so feedback can scale with it too.");
        }

        // --- MotionEventRelay ----------------------------------------------

        [UnityTest]
        public IEnumerator EventRelay_RaisesJumpedAndLeftGroundOnAJump()
        {
            World world = BuildWorld(playerStartY: 1f, withRelay: true);
            yield return Step(30);

            var jumped = 0;
            var leftGround = 0;
            world.Relay.Jumped.AddListener(() => jumped++);
            world.Relay.LeftGround.AddListener(() => leftGround++);

            world.Input.Jump();
            yield return Step(40);

            Assert.AreEqual(1, jumped, "One jump should raise Jumped once.");
            Assert.AreEqual(1, leftGround, "Leaving the ground should raise LeftGround once.");
        }

        [UnityTest]
        public IEnumerator EventRelay_RaisesLandedWithTheImpactSpeed()
        {
            World world = BuildWorld(playerStartY: 8f, withRelay: true);

            var landings = 0;
            float reportedImpact = -1f;
            world.Relay.Landed.AddListener(speed =>
            {
                landings++;
                reportedImpact = speed;
            });

            for (int i = 0; i < 200 && landings == 0; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            Assert.AreEqual(1, landings, "Landing should raise Landed once.");
            Assert.Greater(reportedImpact, 5f, "Landed should carry the speed of the fall, not zero.");
        }

        [UnityTest]
        public IEnumerator EventRelay_RaisesCrouchStartedAndEnded()
        {
            World world = BuildWorld(playerStartY: 1f, withRelay: true, withCrouch: true);
            yield return Step(30);

            var started = 0;
            var ended = 0;
            world.Relay.CrouchStarted.AddListener(() => started++);
            world.Relay.CrouchEnded.AddListener(() => ended++);

            world.Input.CrouchHeld = true;
            yield return Step(20);
            Assert.AreEqual(1, started, "Crouching should raise CrouchStarted once.");
            Assert.AreEqual(0, ended);

            world.Input.CrouchHeld = false;
            yield return Step(20);
            Assert.AreEqual(1, ended, "Standing up should raise CrouchEnded once.");
        }

        [UnityTest]
        public IEnumerator EventRelay_StopsRaisingOnceDisabled()
        {
            World world = BuildWorld(playerStartY: 1f, withRelay: true);
            yield return Step(30);

            var jumped = 0;
            world.Relay.Jumped.AddListener(() => jumped++);
            world.Relay.enabled = false;

            world.Input.Jump();
            yield return Step(40);

            Assert.AreEqual(0, jumped, "A disabled relay should have unsubscribed.");
        }

        // --- Helpers -------------------------------------------------------

        private IEnumerator Step(int fixedSteps)
        {
            for (int i = 0; i < fixedSteps; i++)
            {
                yield return new WaitForFixedUpdate();
            }
        }

        private World BuildWorld(
            float playerStartY,
            bool withRelay = false,
            bool withCrouch = false)
        {
            var ground = new GameObject("Ground") { layer = GroundLayer };
            ground.transform.position = new Vector3(0f, -0.5f, 0f);
            ground.AddComponent<BoxCollider2D>().size = new Vector2(60f, 1f);
            _spawned.Add(ground);

            var profile = ScriptableObject.CreateInstance<MovementProfile>();
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

            var input = player.AddComponent<ScriptedMotionInput>();
            player.AddComponent<JumpController2D>();

            if (withCrouch)
            {
                var crouch = player.AddComponent<CrouchController2D>();
                SetPrivate(crouch, "_bodyCollider", collider);
                SetPrivate(crouch, "_ceilingLayers", (LayerMask)(1 << GroundLayer));
            }

            var controller = player.AddComponent<MotionController2D>();
            SetPrivate(controller, "_profile", profile);

            MotionEventRelay relay = withRelay ? player.AddComponent<MotionEventRelay>() : null;

            player.transform.position = new Vector3(0f, playerStartY, 0f);
            player.SetActive(true);
            _spawned.Add(player);

            return new World(profile, controller, body, input, relay);
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
                ScriptedMotionInput input,
                MotionEventRelay relay)
            {
                Profile = profile;
                Controller = controller;
                Body = body;
                Input = input;
                Relay = relay;
            }

            public MovementProfile Profile { get; }
            public MotionController2D Controller { get; }
            public Rigidbody2D Body { get; }
            public ScriptedMotionInput Input { get; }
            public MotionEventRelay Relay { get; }
        }
    }
}
