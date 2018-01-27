using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turning : MonoBehaviour {

	public GameObject mainCamera;

	public GameObject leftArrow;
	public GameObject rightArrow;

	private Quaternion targetRotation;

	private string blinkingState = "none";

	// Use this for initialization
	void Start () {
		targetRotation = mainCamera.transform.rotation;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.RightArrow)) {
			//targetRotation = camera.transform.rotation * Quaternion.AngleAxis(90, Vector3.up);
			startBlinker("right");
		}
		if (Input.GetKeyDown (KeyCode.LeftArrow)) {
			//targetRotation = camera.transform.rotation * Quaternion.AngleAxis(90, Vector3.down);
			startBlinker("left");
		}
		mainCamera.transform.rotation= Quaternion.Lerp (mainCamera.transform.rotation, targetRotation , 7.0f * Time.deltaTime); 
	}

	void startBlinker(string dir) {
		// Undo blinking if the same direction.
		if (blinkingState == dir) {
			dir = "none";
		}
		// Reset blinkers.
		blinkingState = dir;
		this.StopAllCoroutines ();
		resetArrows ();

		// Start new blinker if necessary.
		if (blinkingState == "none") {
			return;
		}
		GameObject arrow;
		if (blinkingState == "left") {
			arrow = leftArrow;
		} else {
			arrow = rightArrow;
		}
		this.StartCoroutine ("Blink", arrow);
	}

	IEnumerator Blink(GameObject arrow) {
		while (true) {
			if (arrow.GetComponent<Renderer> ().material.color.Equals(Color.yellow)) {
				arrow.GetComponent<Renderer> ().material.color = Color.gray;
			} else {
				arrow.GetComponent<Renderer> ().material.color = Color.yellow;
			}
			yield return new WaitForSeconds(0.5f);
		}
	}

	void resetArrows() {
		rightArrow.GetComponent<Renderer> ().material.color = Color.gray;
		leftArrow.GetComponent<Renderer> ().material.color = Color.gray;
	}
}
