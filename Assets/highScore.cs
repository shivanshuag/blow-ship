using UnityEngine;
using System.Collections;

public class highScore : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.touchCount > 0 && Input.GetTouch (0).phase == TouchPhase.Began && guiTexture.HitTest (Input.GetTouch (0).position)) {
			Application.LoadLevel (2);
		}
	}
}
