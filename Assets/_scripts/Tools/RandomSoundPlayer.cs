using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class RandomSoundPlayer : MonoBehaviour 
{
	public AudioClip[] clips;
	public float minDelayBetweenClips;
	public float maxDelayBetweenClips;

	private float timer;

	private void Update()
	{
		if(timer >= 0)
			UpdateTimer();
		else
			PlayNextSound();
	}

	private void Start() 
	{ 
		DetermineNextInterval(); 
	}

	private void DetermineNextInterval() 
	{ 
		timer = Random.Range(minDelayBetweenClips, maxDelayBetweenClips); 
	}

	private void UpdateTimer()
	{
		timer -= Time.deltaTime;
	}

	private void PlayNextSound()
	{
		if(this.GetComponent<AudioSource>().isPlaying)
			this.GetComponent<AudioSource>().Stop();

		this.GetComponent<AudioSource>().clip = clips[Random.Range(0, clips.Length)];
		this.GetComponent<AudioSource>().Play();

		DetermineNextInterval();
	}
}
