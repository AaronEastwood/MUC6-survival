using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour 
{
	
	public static MusicManager instance = null; 	//Declared a public static variable of type SoundManager (this script) and arbitrarily named it "instance". This allows other scripts to call functions from this one (the SoundManager).
	public GameObject prefab = null;				//Declared a public variable of type GameObject (arbitrarily named it "prefab"). This will be assigned with our SoundManager prefab via the inspector.
	public GameObject musicGenerator = null;		//Declared a public variable of type GameObject that will the instance of "prefab" we will create in the Play() function (arbitrarily named it "soundGenerator").
	public AudioSource source = null;				//Declared a public variable of type AudioSource that will reference the audio source attached to "soundGenerator" (arbitrarily named it "clip").
	public AudioClip[] music = null;
	public AudioClip hit = null;
	
	//Removed "Start" and "Update" because they are not needed
	
	//Step One: Created the Singleton...
	//The purpose of the singleton is to make sure that only one instance of the sound manger exists at a time.
	
	void Awake ()
	{
		if (instance == null)						//This checks to see if there is already an instance of SoundManger
			instance = this;						//If there is not an instance, then this assigns the "instance" variable to this script
		else if (instance != this)					//If there is an instance of SoundManager that is not this one then...
			Destroy (musicGenerator);				//...kill it with fire!!!
		DontDestroyOnLoad (musicGenerator);			//Instructs Unity not to destroy the game object when a scene is loaded.
	}
	
	//Step Two: Created a function to play the sound effect...
	
	public void PlayMusic()				//Created a public function that will play the sound. Set it up to be passed an AudioClip as an argument (arbitrarily named the variable "soundEffect").
	{
		musicGenerator = Instantiate (prefab);					//Assigned the "soundGenerator" variable to an instance of "prefab" using Unity's "Instantiate()" function.
		source = musicGenerator.GetComponent<AudioSource> (); 	//Assigned the "source" variable with the audio source component from the instantiated object (NOT the original prefab) using GetComponent. 
		source.clip = music[Random.Range(0, music.Length)]; 								//Set the active clip of the audio source to "soundEffect"
		source.Play (); 										//Instructed the audio source to play
		
		//Step Three: Destroyed the prefab...
		
		//Destroy (musicGenerator, soundEffect.length); 			//Called Unity's "Destroy()" function. Passed the instance of prefab and the length of the audio clip (using "soundEffect.length") as arguments.
	}

	public void PlayHit()
	{
		musicGenerator = Instantiate (prefab);					//Assigned the "soundGenerator" variable to an instance of "prefab" using Unity's "Instantiate()" function.
		source = musicGenerator.GetComponent<AudioSource> (); 	//Assigned the "source" variable with the audio source component from the instantiated object (NOT the original prefab) using GetComponent. 
		source.clip = hit;								//Set the active clip of the audio source to "soundEffect"
		source.loop = false;
		source.Play (); 										//Instructed the audio source to play
		
		//Step Three: Destroyed the prefab...
		
		Destroy (musicGenerator, hit.length); 			//Called Unity's "Destroy()" function. Passed the instance of prefab and the length of the audio clip (using "soundEffect.length") as arguments.
	}

	public void StopMusic()
	{
		Destroy (musicGenerator);
	}
}