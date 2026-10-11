using UnityEngine;
using System.Net.NetworkInformation;
using AA0000;
using Unity.VisualScripting;
using UnityEngine.Events;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;


public class DeathScript : MonoBehaviour
{
	public string player = "Player";
	public UnityEvent onTriggerEvent;
	public Collider2D deathCollider;
	public ScriptableObject SetCheckPoint;

    private void Start()
	{
	}

    private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.gameObject.CompareTag(player))
		{
			other.gameObject.transform.position = FindAnyObjectByType<SetCheckPoint>().checkpointPosition;
			other.gameObject.GetComponent<AudioSource>().Play();
		}
	}
}