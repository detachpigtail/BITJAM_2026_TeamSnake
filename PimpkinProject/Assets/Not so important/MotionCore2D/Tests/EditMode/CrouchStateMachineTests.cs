using MotionCore2D.Crouch;
using NUnit.Framework;

namespace MotionCore2D.Tests.EditMode
{
    /// <summary>Unit tests for the crouch decision logic in <see cref="CrouchStateMachine"/>.</summary>
    public sealed class CrouchStateMachineTests
    {
        private static CrouchStateMachine.Inputs Frame(bool crouchHeld, bool grounded, bool blockedAbove)
        {
            return new CrouchStateMachine.Inputs(crouchHeld, grounded, blockedAbove);
        }

        [Test]
        public void HoldingCrouchWhileGrounded_Crouches()
        {
            var machine = new CrouchStateMachine();

            CrouchStateMachine.Result result = machine.Evaluate(Frame(crouchHeld: true, grounded: true, blockedAbove: false));

            Assert.IsTrue(result.IsCrouching);
        }

        [Test]
        public void HoldingCrouchWhileAirborne_DoesNotCrouch()
        {
            var machine = new CrouchStateMachine();

            CrouchStateMachine.Result result = machine.Evaluate(Frame(crouchHeld: true, grounded: false, blockedAbove: false));

            Assert.IsFalse(result.IsCrouching);
        }

        [Test]
        public void ReleasingCrouchWithHeadroom_StandsUp()
        {
            var machine = new CrouchStateMachine();
            machine.Evaluate(Frame(crouchHeld: true, grounded: true, blockedAbove: false));

            CrouchStateMachine.Result result = machine.Evaluate(Frame(crouchHeld: false, grounded: true, blockedAbove: false));

            Assert.IsFalse(result.IsCrouching);
        }

        [Test]
        public void ReleasingCrouchUnderCeiling_StaysCrouched()
        {
            var machine = new CrouchStateMachine();
            machine.Evaluate(Frame(crouchHeld: true, grounded: true, blockedAbove: false));

            CrouchStateMachine.Result result = machine.Evaluate(Frame(crouchHeld: false, grounded: true, blockedAbove: true));

            Assert.IsTrue(result.IsCrouching);
        }

        [Test]
        public void OnceCeilingClears_StandsUp()
        {
            var machine = new CrouchStateMachine();
            machine.Evaluate(Frame(crouchHeld: true, grounded: true, blockedAbove: false));
            machine.Evaluate(Frame(crouchHeld: false, grounded: true, blockedAbove: true)); // blocked, stays crouched

            CrouchStateMachine.Result result = machine.Evaluate(Frame(crouchHeld: false, grounded: true, blockedAbove: false));

            Assert.IsFalse(result.IsCrouching);
        }

        [Test]
        public void HoldingCrouchUnderCeiling_RemainsCrouchedRegardlessOfRelease()
        {
            var machine = new CrouchStateMachine();

            // Holding crouch keeps crouching even while blocked above.
            CrouchStateMachine.Result held = machine.Evaluate(Frame(crouchHeld: true, grounded: true, blockedAbove: true));
            Assert.IsTrue(held.IsCrouching);

            // Releasing while still blocked keeps the actor crouched.
            CrouchStateMachine.Result released = machine.Evaluate(Frame(crouchHeld: false, grounded: true, blockedAbove: true));
            Assert.IsTrue(released.IsCrouching);
        }

        [Test]
        public void Reset_ReturnsToStanding()
        {
            var machine = new CrouchStateMachine();
            machine.Evaluate(Frame(crouchHeld: true, grounded: true, blockedAbove: false));

            machine.Reset();

            Assert.IsFalse(machine.IsCrouching);
        }
    }
}
