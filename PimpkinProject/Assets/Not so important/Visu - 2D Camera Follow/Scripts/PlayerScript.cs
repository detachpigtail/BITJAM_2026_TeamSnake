using UnityEditor;
using UnityEngine;
namespace Visu
{
    // ----------------------------------------------------------------------------
    // PlayerScript.cs
    // Last Update: April 20th, 2024
    // Author: Cassio Polegatto
    // ----------------------------------------------------------------------------
    // Description:
    // This is a very simple Player movement script. It is not ideal, you should make your own. 
    // Or... Let me know if you'd like a good Player movement asset :)
    // Your support is greatly appreciated! 
    // Thank you!
    // ----------------------------------------------------------------------------

    public class PlayerScript : MonoBehaviour
    {
        public float moveSpeed = 30f;
        public float jumpForce = 50f;
        public LayerMask groundLayer;
        private SpriteRenderer spriteRenderer;

        private Rigidbody2D rb;
        private bool isGrounded;
        private float groundDistance = 3f;
        private float moveInput;

        private void Start()
        {
            // Get components
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();

            // Tweaking Rigidbody2D variables (delete if needed)
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.gravityScale = 10.0f;
        }

        private void Update()
        {
            // Check if the player is grounded
            isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundDistance, groundLayer);

            // Get input from horizontal axis
            moveInput = Input.GetAxisRaw("Horizontal");

            // Check for jump input
            if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            {
                Jump();
            }

            // Flip the sprite based on the movement direction
            if (moveInput != 0)
            {
                if (moveInput > 0)
                {
                    // Move right, flip sprite to face right
                    spriteRenderer.flipX = false;
                }
                else
                {
                    // Move left, flip sprite to face left
                    spriteRenderer.flipX = true;
                }
            }

        }

        private void FixedUpdate()
        {
          
            // Move the player horizontally
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
                                
        }

        private void Jump()
        {
            // "Reset" vertical velocity
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            // Apply vertical force to jump
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        private void OnDrawGizmos()
        {   
            // Draw Ground checking line
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, Vector2.down * groundDistance);
        }
    }
}
