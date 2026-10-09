using System;
using MotionCore2D.Configuration;
using MotionCore2D.Input;
using MotionCore2D.Sensing;

namespace MotionCore2D
{
    /// <summary>
    /// Immutable bundle of everything a motion step needs for a single simulation step: the
    /// read-only tuning data, the sampled input, the sampled ground information, the elapsed
    /// time, and the shared mutable <see cref="MotionState"/>.
    /// </summary>
    /// <remarks>
    /// The context is a lightweight value type so that building one per step incurs no heap
    /// allocation. It carries references rather than copies; the contained <see cref="State"/>
    /// is intentionally mutable so a pipeline of steps can evolve it in place.
    /// </remarks>
    public readonly struct MotionContext
    {
        /// <summary>
        /// Read-only tuning data for the actor. Steps must never mutate the profile.
        /// </summary>
        public MovementProfile Profile { get; }

        /// <summary>
        /// The input intent sampled for this step.
        /// </summary>
        public IMotionInput Input { get; }

        /// <summary>
        /// The ground information sampled for this step.
        /// </summary>
        public GroundInfo Ground { get; }

        /// <summary>
        /// The simulation time step, in seconds, that this update represents.
        /// </summary>
        public float DeltaTime { get; }

        /// <summary>
        /// The shared, mutable runtime state for the actor. Steps read from and write to it.
        /// </summary>
        public MotionState State { get; }

        /// <summary>
        /// Initialises a new <see cref="MotionContext"/>.
        /// </summary>
        /// <param name="profile">Read-only tuning data. Must not be <c>null</c>.</param>
        /// <param name="input">The sampled input intent. Must not be <c>null</c>.</param>
        /// <param name="ground">The sampled ground information.</param>
        /// <param name="deltaTime">The simulation time step, in seconds.</param>
        /// <param name="state">The shared mutable runtime state. Must not be <c>null</c>.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="profile"/>, <paramref name="input"/>, or
        /// <paramref name="state"/> is <c>null</c>.
        /// </exception>
        public MotionContext(
            MovementProfile profile,
            IMotionInput input,
            GroundInfo ground,
            float deltaTime,
            MotionState state)
        {
            Profile = profile ?? throw new ArgumentNullException(nameof(profile));
            Input = input ?? throw new ArgumentNullException(nameof(input));
            State = state ?? throw new ArgumentNullException(nameof(state));
            Ground = ground;
            DeltaTime = deltaTime;
        }
    }
}
