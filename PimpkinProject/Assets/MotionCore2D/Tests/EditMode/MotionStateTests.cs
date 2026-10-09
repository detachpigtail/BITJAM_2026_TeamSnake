using NUnit.Framework;
using UnityEngine;

namespace MotionCore2D.Tests.EditMode
{
    /// <summary>Unit tests for <see cref="MotionState"/>.</summary>
    public sealed class MotionStateTests
    {
        [Test]
        public void NewState_StartsAtRestAndAirborne()
        {
            var state = new MotionState();

            Assert.AreEqual(Vector2.zero, state.Velocity);
            Assert.IsFalse(state.IsGrounded);
            Assert.IsFalse(state.WasGrounded);
            Assert.IsFalse(state.IsCrouching);
            Assert.IsFalse(state.IsJumping);
        }

        [Test]
        public void Velocity_IsReadWrite()
        {
            var state = new MotionState { Velocity = new Vector2(3f, -2f) };

            Assert.AreEqual(new Vector2(3f, -2f), state.Velocity);
        }
    }
}
