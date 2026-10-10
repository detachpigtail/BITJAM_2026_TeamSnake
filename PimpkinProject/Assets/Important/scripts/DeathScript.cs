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
	public GameObject death;
	public Collider2D deathCollider;

    [System.Obsolete]
    private void Start()
	{
		death = GameObject.FindWithTag("Death");
		deathCollider = death.GetComponent<Collider2D>();
	}

    [System.Obsolete]
    private void OnTriggerEnter2D(Collider2D deathCollider)
	{
		if (deathCollider.gameObject.CompareTag(player))
		{
			deathCollider.gameObject.transform.position = FindObjectOfType<SetCheckPoint>().checkpointPosition;
		}
	}
}

