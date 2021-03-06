﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moveCameraUpDown : MonoBehaviour {
	Vector3 startPos;
	public float amount = 11f;
	public float travelTime = 1f;
	bool active;
	// Use this for initialization
	void Start () {
		startPos = GameObject.Find("MenuCamera").transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		if (!active) {return;}
		Vector3 move = new Vector3(0,(amount * Time.deltaTime)/travelTime);
		GameObject.Find("MenuCamera").transform.position += move;

		if ((GameObject.Find("MenuCamera").transform.position.y > startPos.y + amount  && amount > 0)
		||(GameObject.Find("MenuCamera").transform.position.y < startPos.y + amount  && amount <= 0)) {
			GameObject.Find("MenuCamera").transform.position = new Vector3(startPos.x, startPos.y + amount, startPos.z);
			active = false;
		}
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
	}


	void OnMouseUp()
	{
		startPos = GameObject.Find("MenuCamera").transform.position;
		active = true;
	}

}
