using MotionCore2D.Steps;
using UnityEngine;

namespace MotionCore2D.Crouch
{
    /// <summary>
    /// Adds crouching to an actor: while the crouch control is held on the ground the collider is
    /// lowered (feet planted) and horizontal speed is capped to the profile's crouch fraction. The
    /// actor cannot stand back up while a low ceiling is overhead. It is a separate, optional
    /// component — an actor without it simply cannot crouch.
    /// </summary>
    /// <remarks>
    /// Like <see cref="Jump.JumpController2D"/>, this component owns the feature but does not drive
    /// the simulation itself: it exposes a <see cref="Step"/> that <see cref="Core.MotionController2D"/>
    /// inserts into its pipeline, so crouching participates in the single, ordered velocity update.
    /// The decision lives in a pure <see cref="CrouchStateMachine"/>; this component supplies the two
    /// physics operations that decision needs — the headroom probe and the collider resize — through
    /// <see cref="ICrouchBody"/>. All speed and height tuning comes from the movement profile.
    /// </remarks>
    [AddComponentMenu("MotionCore 2D/Crouch/Crouch Controller 2D")]
    [DisallowMultipleComponent]
    public sealed class CrouchController2D : MonoBehaviour, ICrouchBody
    {
        [SerializeField, Tooltip("The collider lowered while crouching. Auto-detected from this GameObject (then its children) when left empty. BoxCollider2D and CapsuleCollider2D are resized; other types only update the crouch state.")]
        private Collider2D _bodyCollider;

        [SerializeField, Tooltip("Physics layers whose overhead geometry blocks standing back up. Must exclude the actor's own collider.")]
        private LayerMask _ceilingLayers = 0;

        [SerializeField, Min(0f), Tooltip("Small inset above the crouched collider from which the headroom check begins, avoiding a start already touching the actor.")]
        private float _headroomSkin = 0.02f;

        private CrouchStep _step;
        private bool _initialized;
        private bool _isCrouched;
        private float _standingWorldHeight;

        // Cached standing geometry, kept so the collider can be restored exactly.
        private Vector2 _boxStandingSize;
        private Vector2 _boxStandingOffset;
        private Vector2 _capsuleStandingSize;
        private Vector2 _capsuleStandingOffset;
        private BoxCollider2D _box;
        private CapsuleCollider2D _capsule;

        /// <summary>
        /// The crouch pipeline step contributed by this component. Consumed by
        /// <see cref="Core.MotionController2D"/> during pipeline assembly.
        /// </summary>
        public IMotionStep Step => _step != null ? _step : (_step = new CrouchStep(this));

        /// <summary>Whether the collider is currently lowered to its crouched size.</summary>
        public bool IsCrouched => _isCrouched;

        private void Awake()
        {
            EnsureInitialized();
        }

        private void EnsureInitialized()
        {
            if (_initialized)
            {
                return;
            }

            if (_bodyCollider == null)
            {
                _bodyCollider = GetComponent<Collider2D>();
                if (_bodyCollider == null)
                {
                    _bodyCollider = GetComponentInChildren<Collider2D>();
                }
            }

            if (_bodyCollider == null)
            {
                Debug.LogWarning(
                    $"{nameof(CrouchController2D)} on '{name}' found no Collider2D to resize; crouch state will still be reported but the collider will not change.",
                    this);
                _initialized = true;
                return;
            }

            _box = _bodyCollider as BoxCollider2D;
            _capsule = _bodyCollider as CapsuleCollider2D;

            if (_box != null)
            {
                _boxStandingSize = _box.size;
                _boxStandingOffset = _box.offset;
            }
            else if (_capsule != null)
            {
                _capsuleStandingSize = _capsule.size;
                _capsuleStandingOffset = _capsule.offset;
            }
            else
            {
                Debug.LogWarning(
                    $"{nameof(CrouchController2D)} on '{name}' uses a {_bodyCollider.GetType().Name}; only BoxCollider2D and CapsuleCollider2D are resized. Crouch state and the speed cap still apply.",
                    this);
            }

            _standingWorldHeight = _bodyCollider.bounds.size.y;
            _initialized = true;
        }

        /// <inheritdoc />
        bool ICrouchBody.HasHeadroom
        {
            get
            {
                EnsureInitialized();

                if (!_isCrouched || _bodyCollider == null || _ceilingLayers == 0)
                {
                    return true;
                }

                Bounds bounds = _bodyCollider.bounds;
                float gap = (bounds.min.y + _standingWorldHeight) - bounds.max.y;
                if (gap <= _headroomSkin)
                {
                    return true;
                }

                Vector2 origin = new Vector2(bounds.center.x, bounds.max.y + _headroomSkin);
                Vector2 probeSize = new Vector2(Mathf.Max(0f, bounds.size.x - _headroomSkin), _headroomSkin);
                float distance = gap - _headroomSkin;

                RaycastHit2D hit = Physics2D.BoxCast(origin, probeSize, 0f, Vector2.up, distance, _ceilingLayers);
                return hit.collider == null;
            }
        }

        /// <inheritdoc />
        void ICrouchBody.SetCrouched(bool crouched, float heightFactor)
        {
            EnsureInitialized();

            if (crouched == _isCrouched)
            {
                return;
            }

            _isCrouched = crouched;
            float factor = Mathf.Clamp(heightFactor, 0.1f, 1f);

            if (_box != null)
            {
                ApplyBox(crouched, factor);
            }
            else if (_capsule != null)
            {
                ApplyCapsule(crouched, factor);
            }
        }

        private void ApplyBox(bool crouched, float factor)
        {
            if (!crouched)
            {
                _box.size = _boxStandingSize;
                _box.offset = _boxStandingOffset;
                return;
            }

            float feetLocalY = _boxStandingOffset.y - (_boxStandingSize.y * 0.5f);
            float crouchedHeight = _boxStandingSize.y * factor;
            _box.size = new Vector2(_boxStandingSize.x, crouchedHeight);
            _box.offset = new Vector2(_boxStandingOffset.x, feetLocalY + (crouchedHeight * 0.5f));
        }

        private void ApplyCapsule(bool crouched, float factor)
        {
            if (!crouched)
            {
                _capsule.size = _capsuleStandingSize;
                _capsule.offset = _capsuleStandingOffset;
                return;
            }

            float feetLocalY = _capsuleStandingOffset.y - (_capsuleStandingSize.y * 0.5f);
            float crouchedHeight = _capsuleStandingSize.y * factor;
            _capsule.size = new Vector2(_capsuleStandingSize.x, crouchedHeight);
            _capsule.offset = new Vector2(_capsuleStandingOffset.x, feetLocalY + (crouchedHeight * 0.5f));
        }

        private void OnValidate()
        {
            _headroomSkin = Mathf.Max(0f, _headroomSkin);
        }
    }
}
