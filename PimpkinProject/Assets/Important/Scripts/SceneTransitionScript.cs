using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class SceneTransitionScript : MonoBehaviour
{
    public string player = "Player";
	public UnityEvent onTriggerEvent;
	public GameObject door;

    [System.Obsolete]
    private void Start()
	{
		door = GameObject.FindWithTag("door");
	}

    [System.Obsolete]
    private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.gameObject.CompareTag(player))
		{
			if (SceneManager.GetActiveScene().name == "FactoryScene01")
			{
				SceneManager.LoadScene("FactoryScene02");
			}
				
			if (SceneManager.GetActiveScene().name == "FactoryScene02")
			{
				SceneManager.LoadScene("FactoryScene03");
			}
				
		}
	}
}
