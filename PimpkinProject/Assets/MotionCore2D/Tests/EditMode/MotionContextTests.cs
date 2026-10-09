using System;
using MotionCore2D.Configuration;
using MotionCore2D.Input;
using MotionCore2D.Sensing;
using NUnit.Framework;
using UnityEngine;

namespace MotionCore2D.Tests.EditMode
{
    /// <summary>
    /// Unit tests for <see cref="MotionContext"/>, also demonstrating that the input abstraction
    /// can be exercised without any Unity input device.
    /// </summary>
    public sealed class MotionContextTests
    {
        /// <summary>In-memory <see cref="IMotionInput"/> used to drive tests deterministically.</summary>
        private sealed class FakeInput : IMotionInput
        {
            private bool _pressed;

            public float Horizontal { get; set; }

            public bool JumpHeld { get; set; }

            public bool CrouchHeld { get; set; }

            public void PressJump() => _pressed = true;

            public bool ConsumeJumpPressed()
            {
                bool pressed = _pressed;
                _pressed = false;
                return pressed;
            }
        }

        [Test]
        public void Constructor_ExposesAllSuppliedReferences()
        {
            var profile = ScriptableObject.CreateInstance<MovementProfile>();
            var input = new FakeInput { Horizontal = 1f };
            var state = new MotionState();

            var context = new MotionContext(profile, input, GroundInfo.Airborne, 0.02f, state);

            Assert.AreSame(profile, context.Profile);
            Assert.AreSame(input, context.Input);
            Assert.AreSame(state, context.State);
            Assert.AreEqual(0.02f, context.DeltaTime);
            Assert.IsFalse(context.Ground.IsGrounded);

            UnityEngine.Object.DestroyImmediate(profile);
        }

        [Test]
        public void Constructor_ThrowsWhenRequiredReferenceIsNull()
        {
            var state = new MotionState();
            var input = new FakeInput();

            Assert.Throws<ArgumentNullException>(
                () => new MotionContext(null, input, GroundInfo.Airborne, 0.02f, state));
        }

        [Test]
        public void ConsumeJumpPressed_IsEdgeTriggered()
        {
            var input = new FakeInput();
            input.PressJump();

            Assert.IsTrue(input.ConsumeJumpPressed());
            Assert.IsFalse(input.ConsumeJumpPressed());
        }
    }
}
