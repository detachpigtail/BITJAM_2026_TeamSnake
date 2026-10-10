using UnityEngine;
using System.Threading;

public class HydraulicPressScript : MonoBehaviour
{
    public Vector3 startPos;
    public Vector3 targetPos;

    public float speed = 2f;

    public int damage;
    private bool movingUp = true;

    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {  
        Vector3 des = movingUp ? targetPos : startPos;
        transform.position = Vector3.MoveTowards(transform.position, des, speed * Time.deltaTime);
        if (Vector3.Distance(transform.position, des) < 0.1f)
        {
            movingUp = !movingUp;
        }
    }
}
//penis