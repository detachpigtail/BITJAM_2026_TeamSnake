using MotionCore2D.Jump;
using NUnit.Framework;
using UnityEngine;

namespace MotionCore2D.Tests.EditMode
{
    /// <summary>Unit tests for the jump behaviours in <see cref="JumpStateMachine"/>.</summary>
    public sealed class JumpStateMachineTests
    {
        private const float Dt = 0.02f;
        private const float Gravity = 30f;
        private const float Window = 0.1f;
        private const float Height = 2f;
        private const float Cut = 0.5f;

        private static JumpStateMachine.Inputs Frame(
            bool grounded,
            bool pressed,
            bool held,
            float velocityY,
            float coyoteTime = Window,
            float bufferTime = Window)
        {
            return new JumpStateMachine.Inputs(
                Dt, grounded, pressed, held, velocityY, Gravity, coyoteTime, bufferTime, Height, Cut);
        }

        // --- Launch --------------------------------------------------------

        [Test]
        public void GroundedPress_Launches()
        {
            var machine = new JumpStateMachine();

            JumpStateMachine.Result result = machine.Evaluate(Frame(true, true, true, 0f));

            Assert.IsTrue(result.ChangesVerticalVelocity);
            Assert.IsTrue(result.Launched);
            Assert.Greater(result.VerticalVelocity, 0f);
        }

        [Test]
        public void LaunchVelocity_CompensatesForOneHalfIntegrationStep()
        {
            var machine = new JumpStateMachine();

            JumpStateMachine.Result result = machine.Evaluate(Frame(true, true, true, 0f));

            // sqrt(2 g h) is the continuous answer; the caller integrates with semi-implicit Euler,
            // which would overshoot by half a step of gravity if launched at exactly that speed.
            float expected = Mathf.Sqrt(2f * Gravity * Height) - (Gravity * Dt * 0.5f);
            Assert.AreEqual(expected, result.VerticalVelocity, 0.0001f);
        }

        [Test]
        public void IntegratedTrajectory_ReachesConfiguredJumpHeight()
        {
            // The contract the inspector advertises: a full jump peaks at Jump Height. Reproduce the
            // controller's step order (gravity, then jump) and its semi-implicit integration, and
            // confirm the apex lands on the configured height rather than near it.
            var machine = new JumpStateMachine();
            float velocityY = 0f;
            float y = 0f;
            float peak = 0f;

            for (int step = 0; step < 200; step++)
            {
                bool grounded = step == 0;
                if (!grounded)
                {
                    velocityY -= Gravity * Dt; // GravityStep, which runs before the jump step
                }

                JumpStateMachine.Result result = machine.Evaluate(
                    Frame(grounded, pressed: step == 0, held: true, velocityY: velocityY));
                if (result.ChangesVerticalVelocity)
                {
                    velocityY = result.VerticalVelocity;
                }

                y += velocityY * Dt;
                peak = Mathf.Max(peak, y);
            }

            Assert.AreEqual(Height, peak, 0.01f, "A full jump should peak at the configured Jump Height.");
        }

        // --- Assist windows ------------------------------------------------

        [Test]
        public void CoyoteTime_AllowsJumpShortlyAfterLeavingGround()
        {
            var machine = new JumpStateMachine();
            machine.Evaluate(Frame(true, false, false, 0f)); // grounded: arm coyote

            JumpStateMachine.Result result = machine.Evaluate(Frame(false, true, true, 0f)); // airborne within window

            Assert.IsTrue(result.ChangesVerticalVelocity);
        }

        [Test]
        public void CoyoteTime_ExpiresAfterWindow()
        {
            var machine = new JumpStateMachine();
            machine.Evaluate(Frame(true, false, false, 0f)); // arm coyote (0.1s)
            for (int i = 0; i < 10; i++)
            {
                machine.Evaluate(Frame(false, false, false, 0f)); // 0.2s airborne, no press
            }

            JumpStateMachine.Result result = machine.Evaluate(Frame(false, true, true, 0f));

            Assert.IsFalse(result.ChangesVerticalVelocity);
        }

        [Test]
        public void JumpBuffer_FiresOnLandingWhenPressedJustBefore()
        {
            var machine = new JumpStateMachine();
            machine.Evaluate(Frame(false, true, true, -3f)); // press while still airborne

            JumpStateMachine.Result result = machine.Evaluate(Frame(true, false, true, -3f)); // land within buffer

            Assert.IsTrue(result.ChangesVerticalVelocity);
            Assert.Greater(result.VerticalVelocity, 0f);
        }

        [Test]
        public void JumpBuffer_ExpiresAfterWindow()
        {
            var machine = new JumpStateMachine();
            machine.Evaluate(Frame(false, true, true, -3f)); // press while airborne
            for (int i = 0; i < 10; i++)
            {
                machine.Evaluate(Frame(false, false, true, -3f)); // 0.2s still airborne
            }

            JumpStateMachine.Result result = machine.Evaluate(Frame(true, false, true, -3f));

            Assert.IsFalse(result.ChangesVerticalVelocity, "A long-expired press should not fire on landing.");
        }

        [Test]
        public void ZeroBufferWindow_StillJumpsOnAPressThisStep()
        {
            // A zero buffer is a legitimate tuning choice: no remembered presses, nothing more. It
            // must not make the actor unable to jump at all.
            var machine = new JumpStateMachine();

            JumpStateMachine.Result result = machine.Evaluate(Frame(true, true, true, 0f, bufferTime: 0f));

            Assert.IsTrue(result.ChangesVerticalVelocity, "A press on the ground should jump even with no buffer window.");
        }

        [Test]
        public void ZeroCoyoteWindow_StillJumpsWhileGrounded()
        {
            // Likewise a zero coyote window only removes the post-ledge grace period.
            var machine = new JumpStateMachine();

            JumpStateMachine.Result result = machine.Evaluate(Frame(true, true, true, 0f, coyoteTime: 0f));

            Assert.IsTrue(result.ChangesVerticalVelocity, "A press on the ground should jump even with no coyote window.");
        }

        [Test]
        public void ZeroCoyoteWindow_DoesNotJumpOnceAirborne()
        {
            var machine = new JumpStateMachine();
            machine.Evaluate(Frame(true, false, false, 0f, coyoteTime: 0f));

            JumpStateMachine.Result result = machine.Evaluate(Frame(false, true, true, -1f, coyoteTime: 0f));

            Assert.IsFalse(result.ChangesVerticalVelocity);
        }

        // --- Variable height -----------------------------------------------

        [Test]
        public void VariableHeight_CutsVelocityOnEarlyRelease()
        {
            var machine = new JumpStateMachine();
            JumpStateMachine.Result jump = machine.Evaluate(Frame(true, true, true, 0f));

            JumpStateMachine.Result cut = machine.Evaluate(Frame(false, false, false, jump.VerticalVelocity));

            Assert.IsTrue(cut.ChangesVerticalVelocity);
            Assert.IsFalse(cut.Launched, "Cutting a jump short is not a new launch.");
            Assert.AreEqual(jump.VerticalVelocity * Cut, cut.VerticalVelocity, 0.001f);
        }

        [Test]
        public void VariableHeight_NoCutWhileHeld()
        {
            var machine = new JumpStateMachine();
            JumpStateMachine.Result jump = machine.Evaluate(Frame(true, true, true, 0f));

            JumpStateMachine.Result held = machine.Evaluate(Frame(false, false, true, jump.VerticalVelocity));

            Assert.IsFalse(held.ChangesVerticalVelocity);
        }

        [Test]
        public void VariableHeight_CutsOnlyOncePerJump()
        {
            var machine = new JumpStateMachine();
            JumpStateMachine.Result jump = machine.Evaluate(Frame(true, true, true, 0f));
            JumpStateMachine.Result cut = machine.Evaluate(Frame(false, false, false, jump.VerticalVelocity));

            JumpStateMachine.Result again = machine.Evaluate(Frame(false, false, false, cut.VerticalVelocity));

            Assert.IsFalse(again.ChangesVerticalVelocity, "The cut should not keep re-applying while rising.");
        }

        // --- Ground suppression after a launch -----------------------------

        [Test]
        public void LaunchFrame_GroundStillReportedDoesNotAllowASecondJump()
        {
            // The ground probe still finds the surface for a step or two after a launch, because the
            // body has not integrated the new velocity yet. Trusting it would re-arm coyote time and
            // hand out a free mid-air jump on the very next press.
            var machine = new JumpStateMachine();
            JumpStateMachine.Result launch = machine.Evaluate(Frame(true, true, true, 0f));
            Assert.IsTrue(launch.Launched);

            JumpStateMachine.Result second = machine.Evaluate(Frame(true, true, true, launch.VerticalVelocity));

            Assert.IsFalse(second.ChangesVerticalVelocity, "A press while still rising should not jump again.");
        }

        [Test]
        public void LaunchFrame_GroundStillReportedKeepsVariableHeightWorking()
        {
            var machine = new JumpStateMachine();
            JumpStateMachine.Result launch = machine.Evaluate(Frame(true, true, true, 0f));

            // Ground is still reported while rising; the release must still cut the jump short.
            JumpStateMachine.Result cut = machine.Evaluate(Frame(true, false, false, launch.VerticalVelocity));

            Assert.IsTrue(cut.ChangesVerticalVelocity, "An early release should cut the jump even while ground is still reported.");
            Assert.AreEqual(launch.VerticalVelocity * Cut, cut.VerticalVelocity, 0.001f);
        }

        [Test]
        public void GroundSuppression_EndsAsSoonAsTheActorStopsRising()
        {
            var machine = new JumpStateMachine();
            machine.Evaluate(Frame(true, true, true, 0f)); // launch

            // Something stopped the ascent immediately (a low ceiling). The actor is genuinely on the
            // ground again, so the next press must jump.
            machine.Evaluate(Frame(true, false, true, 0f));
            JumpStateMachine.Result result = machine.Evaluate(Frame(true, true, true, 0f));

            Assert.IsTrue(result.ChangesVerticalVelocity);
        }

        [Test]
        public void GroundSuppression_IsCappedSoASustainedRiseCannotLockOutJumping()
        {
            var machine = new JumpStateMachine();
            machine.Evaluate(Frame(true, true, true, 0f)); // launch

            // A lift keeps the actor rising while it stands on a moving surface. Ground readings must
            // start counting again rather than being ignored for as long as the rise lasts.
            for (int i = 0; i < 20; i++) // 0.4s, past the suppression cap
            {
                machine.Evaluate(Frame(true, false, true, 5f));
            }

            JumpStateMachine.Result result = machine.Evaluate(Frame(true, true, true, 5f));

            Assert.IsTrue(result.ChangesVerticalVelocity);
        }

        // --- State and reset -----------------------------------------------

        [Test]
        public void IsRising_TracksTheAscentAndClearsAtTheApex()
        {
            var machine = new JumpStateMachine();
            Assert.IsFalse(machine.IsRising);

            JumpStateMachine.Result launch = machine.Evaluate(Frame(true, true, true, 0f));
            Assert.IsTrue(machine.IsRising);

            machine.Evaluate(Frame(false, false, true, launch.VerticalVelocity));
            Assert.IsTrue(machine.IsRising, "Still rising mid-ascent.");

            machine.Evaluate(Frame(false, false, true, 0f)); // apex
            Assert.IsFalse(machine.IsRising);
        }

        [Test]
        public void AdvanceAssistTimers_ExpiresCoyoteCreditWhileTheMachineIsSkipped()
        {
            // The controller skips the jump step while an ability owns the body. The player still
            // sees that time pass, so a coyote window must not be banked across it.
            var machine = new JumpStateMachine();
            machine.Evaluate(Frame(true, false, false, 0f)); // arm coyote

            machine.AdvanceAssistTimers(1f);

            JumpStateMachine.Result result = machine.Evaluate(Frame(false, true, true, -2f));

            Assert.IsFalse(result.ChangesVerticalVelocity);
        }

        [Test]
        public void AdvanceAssistTimers_ExpiresABufferedPress()
        {
            var machine = new JumpStateMachine();
            machine.Evaluate(Frame(false, true, true, -3f)); // buffer a press

            machine.AdvanceAssistTimers(1f);

            JumpStateMachine.Result result = machine.Evaluate(Frame(true, false, true, -3f));

            Assert.IsFalse(result.ChangesVerticalVelocity);
        }

        [Test]
        public void AdvanceAssistTimers_KeepsAnAscentSoVariableHeightSurvivesABriefSuspension()
        {
            var machine = new JumpStateMachine();
            JumpStateMachine.Result launch = machine.Evaluate(Frame(true, true, true, 0f));

            machine.AdvanceAssistTimers(Dt); // one skipped step mid-ascent

            JumpStateMachine.Result cut = machine.Evaluate(Frame(false, false, false, launch.VerticalVelocity));

            Assert.IsTrue(cut.ChangesVerticalVelocity, "An interrupted jump should still honour an early release.");
        }

        [Test]
        public void Reset_DiscardsBufferedPressesAndTheAscent()
        {
            var machine = new JumpStateMachine();
            machine.Evaluate(Frame(true, true, true, 0f)); // launch, arming an ascent
            machine.Evaluate(Frame(false, true, true, 5f)); // buffer a press mid-air

            machine.Reset();

            // Nothing is remembered: airborne with no coyote credit means no jump.
            JumpStateMachine.Result result = machine.Evaluate(Frame(false, false, true, 5f));

            Assert.IsFalse(result.ChangesVerticalVelocity);
            Assert.IsFalse(machine.IsRising);
        }
    }
}
