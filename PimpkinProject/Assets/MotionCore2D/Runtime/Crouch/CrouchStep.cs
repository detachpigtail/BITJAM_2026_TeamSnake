using System;
using MotionCore2D.Steps;
using UnityEngine;

namespace MotionCore2D.Crouch
{
    /// <summary>
    /// Pipeline step that applies crouching. It feeds the per-step situation into a
    /// <see cref="CrouchStateMachine"/>, publishes the resulting <see cref="MotionState.IsCrouching"/>
    /// flag, asks the body to match its collider to that state, and caps horizontal speed to the
    /// crouch-walk fraction while crouched.
    /// </summary>
    /// <remarks>
    /// Ordered after horizontal movement so the speed cap clamps the velocity that movement just
    /// produced. All physics (the headroom probe and the collider resize) is delegated to the
    /// supplied <see cref="ICrouchBody"/>, keeping this step deterministic given its inputs.
    /// </remarks>
    internal sealed class CrouchStep : IMotionStep
    {
        private readonly CrouchStateMachine _stateMachine = new CrouchStateMachine();
        private readonly ICrouchBody _body;

        public CrouchStep(ICrouchBody body)
        {
            _body = body ?? throw new ArgumentNullException(nameof(body));
        }

        public void Execute(MotionContext context)
        {
            var inputs = new CrouchStateMachine.Inputs(
                crouchHeld: context.Input.CrouchHeld,
                isGrounded: context.State.IsGrounded,
                blockedAbove: !_body.HasHeadroom);

            CrouchStateMachine.Result result = _stateMachine.Evaluate(inputs);

            context.State.IsCrouching = result.IsCrouching;
            _body.SetCrouched(result.IsCrouching, context.Profile.CrouchHeightFactor);

            if (!result.IsCrouching)
            {
                return;
            }

            float cap = Mathf.Max(0f, context.Profile.MaxSpeed * context.Profile.CrouchSpeedMultiplier);
            Vector2 velocity = context.State.Velocity;
            velocity.x = Mathf.Clamp(velocity.x, -cap, cap);
            context.State.Velocity = velocity;
        }
    }
}
