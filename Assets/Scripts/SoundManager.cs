using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour 
{
	
	public static SoundManager instance = null; 				//Declared a public static variable of type SoundManager (this script) and arbitrarily named it "instance". This allows other scripts to call functions from this one (the SoundManager).
	public GameObject prefab = null;							//Declared a public variable of type GameObject (arbitrarily named it "prefab"). This will be assigned with our SoundManager prefab via the inspector.
	public GameObject soundGenerator = null;					//Declared a public variable of type GameObject that will the instance of "prefab" we will create in the Play() function (arbitrarily named it "soundGenerator").
	public AudioSource source = null;							//Declared a public variable of type AudioSource that will reference the audio source attached to "soundGenerator" (arbitrarily named it "source").
	
																//Removed "Start" and "Update" because they are not needed
	
																//Step One: Created the Singleton...
																//The purpose of the singleton is to make sure that only one instance of the sound manger exists at a time.
	
	void Awake ()
	{
		if (instance == null)									//This checks to see if there is already an instance of SoundManger
			instance = this;									//If there is not an instance, then this assigns the "instance" variable to this script
		/*else if (instance != this)								//If there is an instance of SoundManager that is not this one then...
			Destroy (gameObject);								//...kill it with fire!!!
		DontDestroyOnLoad (gameObject);*/							//Instructs Unity not to destroy the game object when a scene is loaded.
	}
	
																//Step Two: Created a function to play the sound effect...
	public void PlaySound (AudioClip soundEffect)				//Created a public function that will play the sound. Set it up to be passed an AudioClip as an argument (arbitrarily named the variable "soundEffect").
	{
		soundGenerator = Instantiate (prefab);					//Assigned the "soundGenerator" variable to an instance of "prefab" using Unity's "Instantiate()" function.
		soundGenerator.tag = "SoundEffect";
		source = soundGenerator.GetComponent<AudioSource> (); 	//Assigned the "source" variable with the audio source component from the instantiated object (NOT the original prefab) using GetComponent. 
		source.clip = soundEffect; 								//Set the active clip of the audio source to "soundEffect"
		source.Play (); 										//Instructed the audio source to play

																//Step Three: Destroyed the prefab...
		Destroy (soundGenerator, soundEffect.length); 			//Called Unity's "Destroy()" function. Passed the instance of prefab and the length of the audio clip (using "soundEffect.length") as arguments.
	}

	public void PlayAmbiance (AudioClip ambiance)			//Created a public function that will play the sound. Set it up to be passed an AudioClip as an argument (arbitrarily named the variable "soundEffect").
	{
		soundGenerator = Instantiate (prefab);					//Assigned the "soundGenerator" variable to an instance of "prefab" using Unity's "Instantiate()" function.
		soundGenerator.tag = "Ambiance";
		source = soundGenerator.GetComponent<AudioSource> (); 	//Assigned the "source" variable with the audio source component from the instantiated object (NOT the original prefab) using GetComponent. 
		source.clip = ambiance; 								//Set the active clip of the audio source to "soundEffect"
		source.loop = true;										//Allows me to speficy whether or not I want the clip to loop. Added this so I could make the ambiance loop.
		source.Play ();											//Instructed the audio source to play

		if (source.loop == false)
			Destroy (soundGenerator, ambiance.length); 			//Called Unity's "Destroy()" function. Passed the instance of prefab and the length of the audio clip (using "soundEffect.length") as arguments.
	}

	public void PlayMusic (AudioClip music)
	{
		soundGenerator = Instantiate (prefab);					//Assigned the "soundGenerator" variable to an instance of "prefab" using Unity's "Instantiate()" function.
		soundGenerator.tag = "Music";
		source = soundGenerator.GetComponent<AudioSource> (); 	//Assigned the "source" variable with the audio source component from the instantiated object (NOT the original prefab) using GetComponent. 
		source.clip = music; 									//Set the active clip of the audio source to "soundEffect"
		source.loop = true;										//Allows me to speficy whether or not I want the clip to loop. Added this so I could make the ambiance loop.
		source.Play ();

		if (source.loop == false)
			Destroy (soundGenerator, music.length);
	}

	public void Stop (string audioItem)
	{
		if (audioItem == "sfx") 
		{
			GameObject[] existingSfx = GameObject.FindGameObjectsWithTag ("SoundEffect");
			foreach (GameObject sfxObject in existingSfx) 
			{
				source = sfxObject.GetComponent<AudioSource> ();
				source.Stop ();
			}
		}
		if (audioItem == "ambiance") 
		{
			GameObject[] existingAmbiance = GameObject.FindGameObjectsWithTag ("Ambiance");
			foreach (GameObject ambianceObject in existingAmbiance) 
			{
				source = ambianceObject.GetComponent<AudioSource> ();
				source.Stop ();
			}
		}
		if (audioItem == "music") 
		{
			GameObject[] existingMusic = GameObject.FindGameObjectsWithTag ("Music");
			foreach (GameObject musicObject in existingMusic) 
			{
				source = musicObject.GetComponent<AudioSource> ();
				source.Stop ();
			}
		}
	}
}