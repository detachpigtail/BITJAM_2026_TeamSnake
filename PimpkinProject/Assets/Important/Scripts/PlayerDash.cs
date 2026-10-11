using MotionCore2D.Configuration;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PlayerDash : MonoBehaviour
{
    public MovementProfile movementProfile;
    public Rigidbody2D rigidbody2d;

    public bool canDash = true;
    public bool isDashing = false;
    [SerializeField] private float dashingPower_x = 0;
    [SerializeField] private float dashingPower_y = 0;
    [SerializeField] private float dashingTime = 0.2f;
    [SerializeField] private float dashingCooldown = 1f;
    [SerializeField] private TrailRenderer tr;


    // Update is called once per frame
    void Update()
    {
        //Checks witch way the player dashes
        if(Input.GetKey(KeyCode.D))
        
        {
            dashingPower_x = 18;
        } else if (Input.GetKey(KeyCode.A))
        {
            dashingPower_x = -18;
        } else
        {
            dashingPower_x = 0;
        }

        if (Input.GetKey(KeyCode.W))
        {
            dashingPower_y = 18;
        } else if (Input.GetKey(KeyCode.S))
        {
            dashingPower_y = -18;
        } else
        {
            dashingPower_y = 0;
        }

        //Prevents the dash from gaining too much speed when dashing diagonally
        if (Input.GetKey(KeyCode.D) && Input.GetKey(KeyCode.W))
        {
            dashingPower_x = 10;
            dashingPower_y = 10;
        }
        if (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.W))
        {
            dashingPower_x = -10;
            dashingPower_y = 10;
        }
        if (Input.GetKey(KeyCode.D) && Input.GetKey(KeyCode.S))
        {
            dashingPower_x = 12;
            dashingPower_y = -12;
        }
        if (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.S))
        {
            dashingPower_x = -12;
            dashingPower_y = -12;
        }

        //Calls for the dash to happen
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        { 
            StartCoroutine(Dash());
        }
    }

    //The actual dash
    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        movementProfile._gravity = 0;
        rigidbody2d.linearVelocity = new Vector2(transform.localScale.x * dashingPower_x, dashingPower_y);
        tr.emitting = true;
        yield return new WaitForSeconds(dashingTime);
        tr.emitting = false;
        isDashing = false;
        movementProfile._gravity = 40f;
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }

}
