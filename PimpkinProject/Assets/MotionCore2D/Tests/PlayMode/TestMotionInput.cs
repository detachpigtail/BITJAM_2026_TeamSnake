using MotionCore2D.Input;
using UnityEngine;

namespace MotionCore2D.Tests.PlayMode
{
    /// <summary>
    /// Scriptable <see cref="IMotionInput"/> used by play-mode tests to drive an actor without a
    /// real input device. Horizontal axis and jump-held are set directly; jump presses are queued
    /// and consumed edge-style.
    /// </summary>
    public sealed class TestMotionInput : MonoBehaviour, IMotionInput
    {
        private bool _jumpQueued;

        public float Horizontal { get; set; }

        public bool JumpHeld { get; set; }

        public bool CrouchHeld { get; set; }

        public void QueueJump() => _jumpQueued = true;

        public bool ConsumeJumpPressed()
        {
            bool queued = _jumpQueued;
            _jumpQueued = false;
            return queued;
        }
    }
}
