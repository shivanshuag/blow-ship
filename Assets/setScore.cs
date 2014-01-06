using UnityEngine;
using System.Collections;

public class setScore : MonoBehaviour {


	// Use this for initialization
	void Start () {
				if (!PlayerPrefs.HasKey ("highScore")) {
						PlayerPrefs.SetFloat ("highScore", 0.0f);
						PlayerPrefs.Save ();
				}
		}
	
	// Update is called once per frame
	void Update () {
		float score = PlayerPrefs.GetFloat ("highScore");
		int min = Mathf.FloorToInt((score / 60f));
		int sec = Mathf.FloorToInt((score % 60f));
		int fraction = Mathf.FloorToInt(((score * 10) % 10));
		this.guiText.text = "High Score  "+min+":"+sec+":"+fraction;
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			HandleBackbutton();
		}
	}

	void HandleBackbutton(){
				Application.LoadLevel (0);
		}

}
