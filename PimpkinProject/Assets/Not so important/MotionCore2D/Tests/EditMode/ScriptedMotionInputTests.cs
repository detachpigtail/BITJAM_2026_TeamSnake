using MotionCore2D.Input;
using NUnit.Framework;
using UnityEngine;

namespace MotionCore2D.Tests.EditMode
{
    /// <summary>Unit tests for <see cref="ScriptedMotionInput"/>, the code-driven input source.</summary>
    public sealed class ScriptedMotionInputTests
    {
        private GameObject _owner;
        private ScriptedMotionInput _input;

        [SetUp]
        public void SetUp()
        {
            _owner = new GameObject("ScriptedInputActor");
            _input = _owner.AddComponent<ScriptedMotionInput>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_owner != null)
            {
                Object.DestroyImmediate(_owner);
            }
        }

        [Test]
        public void NewInput_ReportsNoIntent()
        {
            Assert.AreEqual(0f, _input.Horizontal);
            Assert.IsFalse(_input.JumpHeld);
            Assert.IsFalse(_input.CrouchHeld);
            Assert.IsFalse(_input.ConsumeJumpPressed());
        }

        [Test]
        public void Horizontal_IsClampedToTheValidRange()
        {
            _input.Horizontal = 5f;
            Assert.AreEqual(1f, _input.Horizontal, "Above range should clamp to full right.");

            _input.Horizontal = -5f;
            Assert.AreEqual(-1f, _input.Horizontal, "Below range should clamp to full left.");

            _input.Horizontal = 0.35f;
            Assert.AreEqual(0.35f, _input.Horizontal, 0.0001f, "In-range values should pass through.");
        }

        [Test]
        public void PressJump_IsEdgeTriggeredAndConsumedOnce()
        {
            _input.PressJump();

            Assert.IsTrue(_input.ConsumeJumpPressed(), "The queued press should be reported once.");
            Assert.IsFalse(_input.ConsumeJumpPressed(), "A consumed press should not be reported again.");
        }

        [Test]
        public void PressJump_QueuedTwiceStillYieldsOneJump()
        {
            // Two calls before the simulation reads it is one press, not a stored-up second jump.
            _input.PressJump();
            _input.PressJump();

            Assert.IsTrue(_input.ConsumeJumpPressed());
            Assert.IsFalse(_input.ConsumeJumpPressed());
        }

        [Test]
        public void Jump_QueuesAPressAndHoldsTheControl()
        {
            _input.Jump();

            Assert.IsTrue(_input.JumpHeld, "A full jump should hold the control.");
            Assert.IsTrue(_input.ConsumeJumpPressed());
        }

        [Test]
        public void Jump_NotHeldRequestsAShortHop()
        {
            _input.Jump(held: false);

            Assert.IsFalse(_input.JumpHeld, "A short hop should not hold the control.");
            Assert.IsTrue(_input.ConsumeJumpPressed());
        }

        [Test]
        public void Clear_DiscardsEveryKindOfIntent()
        {
            _input.Horizontal = 1f;
            _input.CrouchHeld = true;
            _input.Jump();

            _input.Clear();

            Assert.AreEqual(0f, _input.Horizontal);
            Assert.IsFalse(_input.JumpHeld);
            Assert.IsFalse(_input.CrouchHeld);
            Assert.IsFalse(_input.ConsumeJumpPressed(), "A queued press should not survive Clear.");
        }

        [Test]
        public void SatisfiesTheMotionInputContract()
        {
            // The whole point of the component is that movement logic cannot tell it apart from a
            // device-backed reader.
            Assert.IsInstanceOf<IMotionInput>(_input);
        }
    }
}
