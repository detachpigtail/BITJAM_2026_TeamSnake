using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class SceneTransitionScript : MonoBehaviour
{
    public string player = "Player";
	public UnityEvent onTriggerEvent;
	public GameObject door;
	public Collider2D doorCollider;

    [System.Obsolete]
    private void Start()
	{
		door = GameObject.FindWithTag("door");
		doorCollider = door.GetComponent<Collider2D>();
	}

    [System.Obsolete]
    private void OnTriggerEnter2D(Collider2D doorCollider)
	{
		if (doorCollider.gameObject.CompareTag(player))
		{
			SceneManager.LoadScene("FactoryScene01");
		}
	}
}
