using MotionCore2D.Core;
using MotionCore2D.Input;
using MotionCore2D.Jump;
using MotionCore2D.Physics;
using UnityEngine;
using UnityEngine.Events;

namespace MotionCore2D.Examples
{
    /// <summary>
    /// Example respawner that teleports the actor back to a checkpoint, showing the small amount of
    /// tidying a correct teleport needs: stop the body, and clear the movement state that was true
    /// where the actor used to be.
    /// </summary>
    /// <remarks>
    /// Moving the transform alone leaves an actor that arrives at the checkpoint still falling at
    /// terminal velocity, and possibly still holding a jump press made a moment before it died — so
    /// it jumps on arrival. Clearing velocity and calling
    /// <see cref="JumpController2D.ResetJumpState"/> is the whole fix; this example is mostly
    /// wiring so it can be driven from a death trigger or a UnityEvent.
    /// </remarks>
    [AddComponentMenu("MotionCore 2D/Examples/Respawn Example")]
    public sealed class RespawnExample : MonoBehaviour
    {
        [SerializeField, Tooltip("Where the actor reappears. Uses its starting position when empty.")]
        private Transform _checkpoint;

        [SerializeField, Tooltip("Respawn automatically when the actor falls below this world height. Set well under your level.")]
        private float _killHeight = -20f;

        [SerializeField, Tooltip("Whether the kill height is checked every frame. Turn it off to respawn only from Respawn() or the event.")]
        private bool _useKillHeight = true;

        [SerializeField, Tooltip("Raised after the actor has been moved and its motion state cleared. Wire it to VFX, SFX, or a camera snap.")]
        private UnityEvent _respawned = new UnityEvent();

        private IMotionBody _body;
        private JumpController2D _jump;
        private ScriptedMotionInput _scriptedInput;
        private Vector2 _startPosition;

        /// <summary>Raised after a respawn completes.</summary>
        public UnityEvent Respawned => _respawned;

        private void Awake()
        {
            _body = GetComponent<IMotionBody>();
            _jump = GetComponent<JumpController2D>();
            _scriptedInput = GetComponent<ScriptedMotionInput>();
            _startPosition = transform.position;

            if (GetComponent<MotionController2D>() == null)
            {
                Debug.LogWarning(
                    $"{nameof(RespawnExample)} on '{name}' expects a {nameof(MotionController2D)} on the same object.",
                    this);
            }
        }

        private void Update()
        {
            if (_useKillHeight && transform.position.y < _killHeight)
            {
                Respawn();
            }
        }

        /// <summary>
        /// Moves the actor to its checkpoint and clears the motion state carried over from where it
        /// was. Safe to call from a UnityEvent, a death trigger, or a debug key.
        /// </summary>
        public void Respawn()
        {
            Vector2 destination = _checkpoint != null ? (Vector2)_checkpoint.position : _startPosition;
            transform.position = destination;

            // Arrive at rest. Without this the actor keeps the fall speed it built up on the way
            // down and slams into the checkpoint floor.
            if (_body != null)
            {
                _body.Velocity = Vector2.zero;
            }

            // Drop coyote credit, any buffered press, and a jump that was in progress, so nothing
            // the player did before dying takes effect after respawning.
            if (_jump != null)
            {
                _jump.ResetJumpState();
            }

            // A scripted actor also forgets its intent; a player's reader re-reads the live device
            // state on its own, so there is nothing to clear there.
            if (_scriptedInput != null)
            {
                _scriptedInput.Clear();
            }

            _respawned.Invoke();
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.8f);
            float x = transform.position.x;
            Gizmos.DrawLine(new Vector3(x - 5f, _killHeight, 0f), new Vector3(x + 5f, _killHeight, 0f));

            if (_checkpoint != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(_checkpoint.position, 0.3f);
            }
        }
#endif
    }
}
