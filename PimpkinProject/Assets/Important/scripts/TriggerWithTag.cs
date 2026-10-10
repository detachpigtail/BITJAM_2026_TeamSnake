using System.Net.NetworkInformation;
using AA0000;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class TriggerWithTag : MonoBehaviour
{
	public string tagToCheck = "Player";
	public UnityEvent onTriggerEvent;
	public GameObject gate;

	private void Start()
	{
		gate = GameObject.FindWithTag("Gate");

	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag(tagToCheck))
		{
			//onTriggerEvent.Invoke();
			gate.transform.position = new Vector3(0, 3.5f, 0);
		}
	}
}
