using UnityEngine;
using System.Collections;

public class TextMessenger : MonoBehaviour 
{
	private const string TEXT_MESSAGE = "See anything? Any clues about the figurine? Speed it up!";
	private const float TEXT_MESSAGE_DURATION = 4.0f;

	public float timeBetweenText;
	public ACCommunicator acc;

	private float timer;

	private void OnEnable()
	{
		ResetTimer();
	}

	private void ResetTimer()
	{
		timer = timeBetweenText;
	}

	private void Update()
	{
		timer -= Time.deltaTime;

		if(timer <= 0) {
			acc.ShowTextMessage(TEXT_MESSAGE, TEXT_MESSAGE_DURATION);
			ResetTimer();
		}
	}

}
