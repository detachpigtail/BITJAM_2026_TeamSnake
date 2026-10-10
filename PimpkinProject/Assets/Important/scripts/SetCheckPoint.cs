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
	public Collider2D checkpointCollider;
	public Vector3 checkpointPosition;

	private void Start()
	{
		checkpoint = GameObject.FindWithTag("CheckPoint");
		checkpointCollider = checkpoint.GetComponent<Collider2D>();
	}

	private void OnTriggerEnter2D(Collider2D checkpointCollider)
	{
        print(checkpointCollider.gameObject.tag);
		if (checkpointCollider.gameObject.CompareTag(player))
		{
            checkpointPosition = checkpoint.transform.position;
		}
	}
}