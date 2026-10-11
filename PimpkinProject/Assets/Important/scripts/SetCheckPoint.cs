using UnityEngine;
using System.Net.NetworkInformation;
using AA0000;
using Unity.VisualScripting;
using UnityEngine.Events;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;

public class SetCheckPoint : MonoBehaviour
{
	public string player = "Player";
	public UnityEvent onTriggerEvent;
	public GameObject checkpoint;
	public Vector3 checkpointPosition;

	private void Start()
	{
		checkpoint = GameObject.FindWithTag("CheckPoint");
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
        print(other.gameObject.tag);
		if (other.gameObject.CompareTag(player))
		{
            checkpointPosition = checkpoint.transform.position;
            print("Checkpoint position set to: " + checkpointPosition);
		}
	}
}