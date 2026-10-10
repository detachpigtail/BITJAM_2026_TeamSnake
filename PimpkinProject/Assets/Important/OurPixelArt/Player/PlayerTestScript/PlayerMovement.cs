using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    private Vector2 movement;

    // Update is called once per frame
    void Update()
    {
        float input = Input.GetAxisRaw("Horizontal");
        movement.x = input * speed * Time.deltaTime;
        transform.Translate(movement);
    }
}
