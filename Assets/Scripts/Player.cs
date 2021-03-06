﻿using UnityEngine;
using UnityEngine.Audio;								//Added this to enable access to AudioMixerGroup and AudioSnapshot data types
using System.Collections;
using UnityEngine.UI;									//Allows us to use UI.

namespace Completed
{
	//Player inherits from MovingObject, our base class for objects that can move, Enemy also inherits from this.
	public class Player : MovingObject
	{
		public float restartLevelDelay = 1f;			//Delay time in seconds to restart level.
		public int pointsPerFood = 10;					//Number of points to add to player food points when picking up a food object.
		public int pointsPerSoda = 20;					//Number of points to add to player food points when picking up a soda object.
		public int wallDamage = 1;						//How much damage a player does to a wall when chopping it.
		public Text foodText;							//UI Text to display current player food total.

		public AudioClip[] footsteps = null;			//Footstep sound effect
		public AudioClip[] eat = null;					//Eat sound effect
		public AudioClip[] drink = null;				//Drink sound effect
		public AudioClip[] playerHit = null;			//Sound that accompanies the level start screen
		public AudioClip[] playerChop = null;			//Player chopping bush sound effect	

		public AudioMixerGroup footstepsMix = null;		//Audio mixer for footsteps
		public AudioMixerGroup chopMix = null;			//Audio mixer for chopping sounds
		public AudioMixerGroup eatMix = null;			//Audio mixer for eating sounds
		public AudioMixerGroup vocalMix = null;			//Audio mixer for vocal sounds

		public AudioMixerSnapshot musNormal = null;		//Snapshot of music faders when the player's state is nornmal
		public AudioMixerSnapshot ambNormal = null; 	//Snapshot of ambiance faders when the player's state is nornmal
		public AudioMixerSnapshot musLowHealth = null;	//Snapshot of music faders when the player's food points are low
		public AudioMixerSnapshot ambLowHealth = null;	//Snapshot of ambiance faders when the player's food points are low
		public AudioMixerSnapshot musOff = null;		//Snapshot of the music faders after End of Day and Game Over
		public AudioMixerSnapshot ambOff = null;		//Snapshot of the ambiance faders after End of Day and Game Over
		
		private Animator animator;						//Used to store a reference to the Player's animator component.
		private int food;								//Used to store player food points total during level.
		//private Vector2 touchOrigin = -Vector2.one;	//Used to store location of screen touch origin for mobile controls.
		
		
		//Start overrides the Start function of MovingObject
		protected override void Start ()
		{
			//Get a component reference to the Player's animator component
			animator = GetComponent<Animator>();
			
			//Get the current food point total stored in GameManager.instance between levels.
			food = GameManager.instance.playerFoodPoints;
			
			//Set the foodText to reflect the current player food total.
			foodText.text = "Food: " + food;
			
			//Call the Start function of the MovingObject base class.
			base.Start ();
		}
		
		
		//This function is called when the behaviour becomes disabled or inactive.
		private void OnDisable ()
		{
			//When Player object is disabled, store the current local food total in the GameManager so it can be re-loaded in next level.
			GameManager.instance.playerFoodPoints = food;
		}
		
		
		private void Update ()
		{
			//If it's not the player's turn, exit the function.
			if(!GameManager.instance.playersTurn) return;
			
			int horizontal = 0;  	//Used to store the horizontal move direction.
			int vertical = 0;		//Used to store the vertical move direction.
			
			//Check if we are running either in the Unity editor or in a standalone build.
			#if UNITY_STANDALONE || UNITY_WEBPLAYER
			
			//Get input from the input manager, round it to an integer and store in horizontal to set x axis move direction
			horizontal = (int) (Input.GetAxisRaw ("Horizontal"));
			
			//Get input from the input manager, round it to an integer and store in vertical to set y axis move direction
			vertical = (int) (Input.GetAxisRaw ("Vertical"));
			
			//Check if moving horizontally, if so set vertical to zero.
			if(horizontal != 0)
			{
				vertical = 0;
			}
			//Check if we are running on iOS, Android, Windows Phone 8 or Unity iPhone
			#elif UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
			
			//Check if Input has registered more than zero touches
			if (Input.touchCount > 0)
			{
				//Store the first touch detected.
				Touch myTouch = Input.touches[0];
				
				//Check if the phase of that touch equals Began
				if (myTouch.phase == TouchPhase.Began)
				{
					//If so, set touchOrigin to the position of that touch
					touchOrigin = myTouch.position;
				}
				
				//If the touch phase is not Began, and instead is equal to Ended and the x of touchOrigin is greater or equal to zero:
				else if (myTouch.phase == TouchPhase.Ended && touchOrigin.x >= 0)
				{
					//Set touchEnd to equal the position of this touch
					Vector2 touchEnd = myTouch.position;
					
					//Calculate the difference between the beginning and end of the touch on the x axis.
					float x = touchEnd.x - touchOrigin.x;
					
					//Calculate the difference between the beginning and end of the touch on the y axis.
					float y = touchEnd.y - touchOrigin.y;
					
					//Set touchOrigin.x to -1 so that our else if statement will evaluate false and not repeat immediately.
					touchOrigin.x = -1;
					
					//Check if the difference along the x axis is greater than the difference along the y axis.
					if (Mathf.Abs(x) > Mathf.Abs(y))
						//If x is greater than zero, set horizontal to 1, otherwise set it to -1
						horizontal = x > 0 ? 1 : -1;
					else
						//If y is greater than zero, set horizontal to 1, otherwise set it to -1
						vertical = y > 0 ? 1 : -1;
				}
			}
			
			#endif //End of mobile platform dependendent compilation section started above with #elif
			//Check if we have a non-zero value for horizontal or vertical
			if(horizontal != 0 || vertical != 0)
			{
				//Call AttemptMove passing in the generic parameter Wall, since that is what Player may interact with if they encounter one (by attacking it)
				//Pass in horizontal and vertical as parameters to specify the direction to move Player in.
				AttemptMove<Wall> (horizontal, vertical);
			}
		}
		
		//AttemptMove overrides the AttemptMove function in the base class MovingObject
		//AttemptMove takes a generic parameter T which for Player will be of the type Wall, it also takes integers for x and y direction to move in.
		protected override void AttemptMove <T> (int xDir, int yDir)
		{
			//Every time player moves, subtract from food points total.
			food--;
			
			//Update food text display to reflect current score.
			foodText.text = "Food: " + food;
			
			//Call the AttemptMove method of the base class, passing in the component T (in this case Wall) and x and y direction to move.
			base.AttemptMove <T> (xDir, yDir);
			
			//Hit allows us to reference the result of the Linecast done in Move.
			RaycastHit2D hit;
			
			//If Move returns true, meaning Player was able to move into an empty space.
			if (Move (xDir, yDir, out hit)) 
			{
				//Play sound -- MUST have ".instance" to specify the particular object: "[Script].[variableNameFromScript].[FunctionName]"
				//Arguments passed are: the sound effect name (array), a random array element number, the pitch (float), and the mixer group
				SoundManager.instance.PlaySound (footsteps[Random.Range(0, footsteps.Length)], Random.Range (0.95f,1.05f), footstepsMix);
			}
			
			//Since the player has moved and lost food points, check if the game has ended.
			CheckIfGameOver ();
			
			//Set the playersTurn boolean of GameManager to false now that players turn is over.
			GameManager.instance.playersTurn = false;
		}
		
		
		//OnCantMove overrides the abstract function OnCantMove in MovingObject.
		//It takes a generic parameter T which in the case of Player is a Wall which the player can attack and destroy.
		protected override void OnCantMove <T> (T component)
		{
			//Set hitWall to equal the component passed in as a parameter.
			Wall hitWall = component as Wall;
			
			//Call the DamageWall function of the Wall we are hitting.
			hitWall.DamageWall (wallDamage);
			
			//Set the attack trigger of the player's animation controller in order to play the player's attack animation.
			animator.SetTrigger ("playerChop");

			//Play sound -- MUST have ".instance" to specify the particular object: "[Script].[variableNameFromScript].[FunctionName]"
			//Arguments passed are: the sound effect name (array), a random array element number, the pitch (float), and the mixer group
			SoundManager.instance.PlaySound (playerChop[Random.Range(0, playerChop.Length)], Random.Range (0.95f,1.05f), chopMix);
		}
		
		
		//OnTriggerEnter2D is sent when another object enters a trigger collider attached to this object (2D physics only).
		private void OnTriggerEnter2D (Collider2D other)
		{
			//Check if the tag of the trigger collided with is Exit.
			if(other.tag == "Exit")
			{
				//Fades out the music and mabiance
				SoundManager.instance.ToSnapshot (musOff, 2.0f);
				SoundManager.instance.ToSnapshot (ambOff, 2.0f);

				//Invoke the Restart function to start the next level with a delay of restartLevelDelay (default 1 second).
				Invoke ("Restart", restartLevelDelay);
				
				//Disable the player object since level is over.
				enabled = false;
			}
			
			//Check if the tag of the trigger collided with is Food.
			else if(other.tag == "Food")
			{
				//Add pointsPerFood to the players current food total.
				food += pointsPerFood;
				
				//Update foodText to represent current total and notify player that they gained points
				foodText.text = "+" + pointsPerFood + " Food: " + food;

				if (food <= 50) //checks the number of food points remaining
				{
					//Transitions to the low health snapshots when food poins are below 50
					SoundManager.instance.ToSnapshot (musLowHealth, 3.0f);
					SoundManager.instance.ToSnapshot (ambLowHealth, 0.3f);
				} 
				else 
				{
					//Transitions to the normal snapshots when food points are 50 or above
					SoundManager.instance.ToSnapshot (musNormal, 1.0f);
					SoundManager.instance.ToSnapshot (ambNormal, 1.0f);
				}

				//Disable the food object the player collided with.
				other.gameObject.SetActive (false);

				//Play sound -- MUST have ".instance" to specify the particular object: "[Script].[variableNameFromScript].[FunctionName]"
				//Arguments passed are: the sound effect name (array), a random array element number, the pitch (float), and the mixer group
				SoundManager.instance.PlaySound (eat[Random.Range (0, eat.Length)], Random.Range (0.95f, 1.05f), eatMix);
			}
			
			//Check if the tag of the trigger collided with is Soda.
			else if(other.tag == "Soda")
			{
				//Add pointsPerSoda to players food points total
				food += pointsPerSoda;
				
				//Update foodText to represent current total and notify player that they gained points
				foodText.text = "+" + pointsPerSoda + " Food: " + food;

				if (food <= 50) //checks the number of food points remaining
				{
					//Transitions to the low health snapshots when food poins are below 50
					SoundManager.instance.ToSnapshot (musLowHealth, 3.0f);
					SoundManager.instance.ToSnapshot (ambLowHealth, 0.3f);
				} 
				else 
				{
					//Transitions to the normal snapshots when food points are 50 or above
					SoundManager.instance.ToSnapshot (musNormal, 1.0f);
					SoundManager.instance.ToSnapshot (ambNormal, 1.0f);
				}
				
				//Disable the soda object the player collided with.
				other.gameObject.SetActive (false);

				//Play sound -- MUST have ".instance" to specify the particular object: "[Script].[variableNameFromScript].[FunctionName]"
				//Arguments passed are: the sound effect name (array), a random array element number, the pitch (float), and the mixer group
				SoundManager.instance.PlaySound (drink[Random.Range (0, drink.Length)], Random.Range (0.95f, 1.05f), eatMix);
			}
		}
		
		
		//Restart reloads the scene when called.
		private void Restart ()
		{
			//Load the last scene loaded, in this case Main, the only scene in the game.
			Application.LoadLevel (Application.loadedLevel);
		}
		
		
		//LoseFood is called when an enemy attacks the player.
		//It takes a parameter loss which specifies how many points to lose.
		public void LoseFood (int loss)
		{
			//Set the trigger for the player animator to transition to the playerHit animation.
			animator.SetTrigger ("playerHit");

			//Play sound -- MUST have ".instance" to specify the particular object: "[Script].[variableNameFromScript].[FunctionName]"
			//Arguments passed are: the sound effect name (array), a random array element number, the pitch (float), and the mixer group
			SoundManager.instance.PlaySound (playerHit[Random.Range(0, playerHit.Length)], 1.0f, vocalMix);
			
			//Subtract lost food points from the players total.
			food -= loss;
			
			//Update the food display with the new total.
			foodText.text = "-"+ loss + " Food: " + food;

			//Check to see if game has ended.
			CheckIfGameOver ();
		}
		
		
		//CheckIfGameOver checks if the player is out of food points and if so, ends the game.
		private void CheckIfGameOver ()
		{
			//Check if food point total is less than or equal to zero.
			if (food <= 0) 
			{
				//Call the GameOver function of GameManager.
				GameManager.instance.GameOver ();
			}

			if (food <= 50) //checks the number of food points remaining
			{
				//Transitions to the low health snapshots when food poins are below 50
				SoundManager.instance.ToSnapshot (musLowHealth, 3.0f);
				SoundManager.instance.ToSnapshot (ambLowHealth, 0.3f);
			} 
			else 
			{
				//Transitions to the normal snapshots when food points are 50 or above
				SoundManager.instance.ToSnapshot (musNormal, 1.0f);
				SoundManager.instance.ToSnapshot (ambNormal, 1.0f);
			}
		}
	}
}

