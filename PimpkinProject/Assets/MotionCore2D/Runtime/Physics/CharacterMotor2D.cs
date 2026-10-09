using UnityEngine;

namespace MotionCore2D.Physics
{
    /// <summary>
    /// The single approved gateway to an actor's <see cref="Rigidbody2D"/>. It adapts the body to
    /// <see cref="IMotionBody"/> so the rest of the system reads and writes velocity through an
    /// abstraction and never touches the <see cref="Rigidbody2D"/> directly.
    /// </summary>
    /// <remarks>
    /// MotionCore uses a single gravity model: manual gravity driven from the movement profile.
    /// To enforce it, this motor sets the body's <c>gravityScale</c> to zero and freezes Z
    /// rotation on awake, warning once if a non-zero scale was configured. The component holds no
    /// movement logic and makes no movement decisions.
    /// </remarks>
    [AddComponentMenu("MotionCore 2D/Physics/Character Motor 2D")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class CharacterMotor2D : MonoBehaviour, IMotionBody
    {
        private Rigidbody2D _rigidbody;

        private Rigidbody2D Body =>
            _rigidbody != null ? _rigidbody : (_rigidbody = GetComponent<Rigidbody2D>());

        /// <inheritdoc />
        public Vector2 Velocity
        {
            get => Body.linearVelocity;
            set => Body.linearVelocity = value;
        }

        /// <inheritdoc />
        public Vector2 Position => Body.position;

        private void Awake()
        {
            if (Body.gravityScale != 0f)
            {
                Debug.LogWarning(
                    $"{nameof(CharacterMotor2D)} on '{name}' uses manual gravity; overriding Rigidbody2D.gravityScale to 0.",
                    this);
                Body.gravityScale = 0f;
            }

            Body.freezeRotation = true;
        }
    }
}
