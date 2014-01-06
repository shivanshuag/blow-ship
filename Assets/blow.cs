using UnityEngine;

using System.Collections;

using System.Collections.Generic;



public class blow : MonoBehaviour {
	

	private const int FREQUENCY = 48000;    // Wavelength, I think.
	
	private const int SAMPLECOUNT = 1024;   // Sample Count.
	
	private const float REFVALUE = 0.1f;    // RMS value for 0 dB.
	
	private const float THRESHOLD = 0.02f;  // Minimum amplitude to extract pitch (recieve anything)
	
	private const float ALPHA = 0.05f;      // The alpha for the low pass filter (I don't really understand this).

	public float baseSpeed = -0.01f;

	private float speed = -0.01f;

	public GameObject boat;

	private float timer = 0.0f; 
	//public GameObject resultDisplay;   // GUIText for displaying results
	
	//public GameObject blowDisplay;     // GUIText for displaying blow or not blow.

	public GameObject textDisplay;

	public int recordedLength = 50;    // How many previous frames of sound are analyzed.
	
	public int requiedBlowTime = 4;    // How long a blow must last to be classified as a blow (and not a sigh for instance).
	
	public int clamp = 160;            // Used to clamp dB (I don't really understand this either).
	
	
	
	private float rmsValue;            // Volume in RMS
	
	private float dbValue;             // Volume in DB
	
	private float pitchValue;          // Pitch - Hz (is this frequency?)
	
	private int blowingTime;           // How long each blow has lasted
	
	
	
	private float lowPassResults;      // Low Pass Filter result
	
	private float peakPowerForChannel; //
	
	
	
	private float[] samples;           // Samples
	
	private float[] spectrum;          // Spectrum
	
	private List<float> dbValues;      // Used to average recent volume.
	
	private List<float> pitchValues;   // Used to average recent pitch.
	
	int min,sec, fraction;
	float timecount;
	int flagSet = 0, flagBlow = 0;
	TextMesh t;
	public void Start () {
		//speed = baseSpeed;
		t = (TextMesh)textDisplay.GetComponent(typeof(TextMesh));
		samples = new float[SAMPLECOUNT];
		
		spectrum = new float[SAMPLECOUNT];
		
		dbValues = new List<float>();
		
		pitchValues = new List<float>();
		
		if (!PlayerPrefs.HasKey ("highScore")) {
						PlayerPrefs.SetFloat ("highScore", 0.0f);
						PlayerPrefs.Save ();
				}

		
		StartMicListener();
		
	}
	
	
	
	public void Update () {
		//Debug.Log (timer);
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			HandleBackbutton();
		}

		timer += Time.deltaTime;
		timecount = timer - 2.3f;
		if (timer > 1 && flagSet == 0) {
			t.text = "Set";
			flagSet = 1;
		}
		if (timer > 2 && flagBlow == 0) {
						t.text = "Blow!";
						flagBlow = 1;
				}
		if (timer > 2.3f) {
			min = Mathf.FloorToInt((timecount / 60f));
			sec = Mathf.FloorToInt((timecount % 60f));
			fraction = Mathf.FloorToInt(((timecount * 10) % 10));
			t.text = string.Format("{0}:{1}:{2}", min, sec, fraction);
		}
		//Debug.Log (speed);
		// If the audio has stopped playing, this will restart the mic play the clip.
		if(flagBlow == 1)
			boat.transform.Translate ( speed, 0, 0);
		
		if (!audio.isPlaying) {
			
			StartMicListener();
			
		}
		

		
		// Gets volume and pitch values
		
		AnalyzeSound();
		
		
		
		// Runs a series of algorithms to decide whether a blow is occuring.
		
		DeriveBlow();
		
		
		if(boat.transform.position.x < -330){
			if(PlayerPrefs.GetFloat("highScore") > timer || PlayerPrefs.GetFloat("highScore") == 0.0f){
				PlayerPrefs.SetFloat("highScore", timer);
				PlayerPrefs.Save ();
			}
			Application.LoadLevel("main-screen");
		}
		// Update the meter display.
		
/*		if (resultDisplay){
			
			resultDisplay.guiText.text = "RMS: " + rmsValue.ToString("F2") + " (" + dbValue.ToString("F1") + " dB)\n" + "Pitch: " + pitchValue.ToString("F0") + " Hz";
			
		}
*/		
	}
	
	
	
	/// Starts the Mic, and plays the audio back in (near) real-time.
	
	private void StartMicListener() {
		
		audio.clip = Microphone.Start("Built-in Microphone", true, 60, FREQUENCY);
		
		// HACK - Forces the function to wait until the microphone has started, before moving onto the play function.
		
		while (!(Microphone.GetPosition("Built-in Microphone") > 0)) {
			
		} audio.Play();
		
	}
	
	
	
	/// Credits to aldonaletto for the function, http://goo.gl/VGwKt
	
	/// Analyzes the sound, to get volume and pitch values.
	
	private void AnalyzeSound() {
		
		
		
		// Get all of our samples from the mic.
		
		audio.GetOutputData(samples, 0);
		
		
		
		// Sums squared samples
		
		float sum = 0;
		
		for (int i = 0; i < SAMPLECOUNT; i++){
			
			sum += Mathf.Pow(samples[i], 2);
			
		}
		
		
		
		// RMS is the square root of the average value of the samples.
		
		rmsValue = Mathf.Sqrt(sum / SAMPLECOUNT);
		
		dbValue = 20 * Mathf.Log10(rmsValue / REFVALUE);
		
		
		
		// Clamp it to {clamp} min
		
		if (dbValue < -clamp) {
			
			dbValue = -clamp;
			
		}
		
		
		
		// Gets the sound spectrum.
/*		
		audio.GetSpectrumData(spectrum, 0, FFTWindow.BlackmanHarris);
		
		float maxV = 0;
		
		int maxN = 0;
		
		
		
		// Find the highest sample.
		
		for (int i = 0; i < SAMPLECOUNT; i++){
			
			if (spectrum[i] > maxV && spectrum[i] > THRESHOLD){
				
				maxV = spectrum[i];
				
				maxN = i; // maxN is the index of max
				
			}
			
		}
		
		
		
		// Pass the index to a float variable
		
		float freqN = maxN;
		
		
		
		// Interpolate index using neighbours
		
		if (maxN > 0 && maxN < SAMPLECOUNT - 1) {
			
			float dL = spectrum[maxN-1] / spectrum[maxN];
			
			float dR = spectrum[maxN+1] / spectrum[maxN];
			
			freqN += 0.5f * (dR * dR - dL * dL);
			
		}
		
		
		
		// Convert index to frequency
		
		pitchValue = freqN * 24000 / SAMPLECOUNT;
*/		
	}
	
	
	
	private void DeriveBlow() {
		
		
		
		UpdateRecords(dbValue, dbValues);
		
//		UpdateRecords(pitchValue, pitchValues);
		
		
		
		// Find the average pitch in our records (used to decipher against whistles, clicks, etc).
/*		
		float sumPitch = 0;
		
		foreach (float num in pitchValues) {
			
			sumPitch += num;
			
		}
		
		sumPitch /= pitchValues.Count;
		
*/
		
		// Run our low pass filter.
		
		lowPassResults = LowPassFilter(dbValue);
		
		
		
		// Decides whether this instance of the result could be a blow or not.
		
		if (lowPassResults > -10 ) {
			
			blowingTime += 1;
			
		} else {
			
			blowingTime = 0;
			
		}
		
		
		
		// Once enough successful blows have occured over the previous frames (requiredBlowTime), the blow is triggered.
		
		// This example says "blowing", or "not blowing", and also blows up a sphere.
		
		if (blowingTime > requiedBlowTime) {
			
			//blowDisplay.guiText.text = "Blowing";
			
			//GameObject.FindGameObjectWithTag("Meter").transform.localScale *= 1.012f;
			if (speed > -0.3f)
				speed = speed - 0.04f;

			//Debug.Log("Blowing");
		} else {

			if(speed < -0.02f)
				speed = speed + 0.015f;
			speed = -0.01f;
			//blowDisplay.guiText.text = "Not blowing";
			
			//GameObject.FindGameObjectWithTag("Meter").transform.localScale *= 0.999f;

			//Debug.Log ("Not Blowing");
		}
		
	}
	
	
	
	// Updates a record, by removing the oldest entry and adding the newest value (val).
	
	private void UpdateRecords(float val, List<float> record) {
		
		if (record.Count > recordedLength) {
			
			record.RemoveAt(0);
			
		}
		
		record.Add(val);
		
	}
	
	
	
	/// Gives a result (I don't really understand this yet) based on the peak volume of the record
	
	/// and the previous low pass results.
	
	private float LowPassFilter(float peakVolume) {
		
		return ALPHA * peakVolume + (1.0f - ALPHA) * lowPassResults;
		
	}

	void HandleBackbutton(){
		Application.LoadLevel (0);
	}

	
}
