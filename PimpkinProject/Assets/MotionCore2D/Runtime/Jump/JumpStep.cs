using System;
using MotionCore2D.Steps;
using UnityEngine;

namespace MotionCore2D.Jump
{
    /// <summary>
    /// Pipeline step that applies jumping. It feeds the per-step situation into a
    /// <see cref="JumpStateMachine"/> and, when the machine decides a jump begins or should be cut,
    /// writes the resulting vertical velocity into the shared state. Gravity used for the launch
    /// calculation comes from the movement profile, matching the manual gravity model.
    /// </summary>
    /// <remarks>
    /// Ordered after gravity so that a launch is the last word on vertical velocity for the step.
    /// Were gravity to run afterwards it would shave a step of acceleration off every launch before
    /// the body ever integrated it, and the actor would fall short of the configured Jump Height.
    /// </remarks>
    internal sealed class JumpStep : IMotionStep
    {
        private readonly JumpStateMachine _stateMachine = new JumpStateMachine();
        private readonly Action _launched;

        /// <summary>Initialises a new <see cref="JumpStep"/>.</summary>
        /// <param name="launched">Raised once each time a jump begins. Optional.</param>
        public JumpStep(Action launched = null)
        {
            _launched = launched;
        }

        /// <summary>Whether a jump is currently carrying the actor upward.</summary>
        public bool IsRising => _stateMachine.IsRising;

        /// <summary>Clears jump timers and any in-progress ascent.</summary>
        public void Reset() => _stateMachine.Reset();

        /// <summary>Expires the assist windows for a step this step did not run.</summary>
        /// <param name="deltaTime">Elapsed time to charge against the windows, in seconds.</param>
        public void AdvanceAssistTimers(float deltaTime) => _stateMachine.AdvanceAssistTimers(deltaTime);

        public void Execute(MotionContext context)
        {
            var profile = context.Profile;

            var inputs = new JumpStateMachine.Inputs(
                deltaTime: context.DeltaTime,
                isGrounded: context.State.IsGrounded,
                jumpPressed: context.Input.ConsumeJumpPressed(),
                jumpHeld: context.Input.JumpHeld,
                velocityY: context.State.Velocity.y,
                gravity: profile.Gravity,
                coyoteTime: profile.CoyoteTime,
                jumpBufferTime: profile.JumpBufferTime,
                jumpHeight: profile.JumpHeight,
                cutMultiplier: profile.VariableJumpCutMultiplier);

            JumpStateMachine.Result result = _stateMachine.Evaluate(inputs);
            if (result.ChangesVerticalVelocity)
            {
                Vector2 velocity = context.State.Velocity;
                velocity.y = result.VerticalVelocity;
                context.State.Velocity = velocity;
            }

            context.State.IsJumping = _stateMachine.IsRising;

            if (result.Launched)
            {
                _launched?.Invoke();
            }
        }
    }
}
