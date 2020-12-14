/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"Sound.cs"
 * 
 *	This script allows for easy playback of audio sources from within the ActionList system.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	[RequireComponent (typeof (AudioSource))]
	public class Sound : MonoBehaviour
	{

		public SoundType soundType;
		public bool playWhilePaused = false;
		public float relativeVolume = 1f;
		public bool surviveSceneChange = false;

		private float maxVolume = 1f;
		private float smoothVolume = 1f;
		private float smoothUpdateSpeed = 20f;
		private float fadeStartTime;
		private float fadeEndTime;
		private FadeType fadeType;
		private bool isFading = false;

		private Options options;
		private AudioSource audioSource;
		private bool isPlaying = false;
		
		
		private void Awake ()
		{
			if (surviveSceneChange)
			{
				DontDestroyOnLoad (this);
			}
			
			if (GetComponent <AudioSource>())
			{
				audioSource = GetComponent <AudioSource>();

				if (audioSource.playOnAwake)
				{
					isPlaying = true;
					audioSource.playOnAwake = false;
				}
			}

			audioSource.ignoreListenerPause = playWhilePaused;
			AdvGame.AssignMixerGroup (audioSource, soundType);
		}


		private void OnLevelWasLoaded ()
		{
			// Search for duplicates carried over from scene change
			if (GetComponent <ConstantID>())
			{
				int ownID = GetComponent <ConstantID>().constantID;
				Sound[] sounds = FindObjectsOfType (typeof (Sound)) as Sound[];
				foreach (Sound sound in sounds)
				{
					if (sound != this && sound.GetComponent <ConstantID>() && sound.GetComponent <ConstantID>().constantID == ownID)
					{
						DestroyImmediate (sound.gameObject);
						return;
					}
				}
			}
		}

		
		public void AfterLoading ()
		{
			if (audioSource == null && GetComponent <AudioSource>())
			{
				audioSource = GetComponent <AudioSource>();
			}

			if (audioSource)
			{
				audioSource.ignoreListenerPause = playWhilePaused;
				
				if (audioSource.playOnAwake && audioSource.clip)
				{
					FadeIn (0.5f, audioSource.loop);
				}
				else
				{
					SetMaxVolume ();
				}
			}
			else
			{
				Debug.LogWarning ("Sound object " + this.name + " has no AudioSource component.");
			}
		}
		

		public void _Update ()
		{
			if (isFading && audioSource.isPlaying)
			{
				smoothVolume = maxVolume;
				float progress = (Time.time - fadeStartTime) / (fadeEndTime - fadeStartTime);
				
				if (fadeType == FadeType.fadeIn)
				{
					if (progress > 1f)
					{
						audioSource.volume = smoothVolume;
						isFading = false;
					}
					else
					{
						audioSource.volume = progress * smoothVolume;
					}
				}
				else if (fadeType == FadeType.fadeOut)
				{
					if (progress > 1f)
					{
						audioSource.volume = 0f;
						Stop ();
					}
					else
					{
						audioSource.volume = (1 - progress) * smoothVolume;
					}
				}
				SetSmoothVolume ();
			}
			else
			{
				SetSmoothVolume ();
				if (audioSource)
				{
					audioSource.volume = smoothVolume;
				}
			}
		}


		private void SetSmoothVolume ()
		{
			if (smoothVolume != maxVolume)
			{
				if (smoothUpdateSpeed > 0)
				{
					smoothVolume = Mathf.Lerp (smoothVolume, maxVolume, Time.deltaTime * smoothUpdateSpeed);
				}
				else
				{
					smoothVolume = maxVolume;
				}
			}
		}
		
		
		public void Interact ()
		{
			isFading = false;
			SetMaxVolume ();
			Play (audioSource.loop);
		}
		
		
		public void FadeIn (float fadeTime, bool loop)
		{
			if (audioSource.clip == null)
			{
				return;
			}

			audioSource.loop = loop;
			
			fadeStartTime = Time.time;
			fadeEndTime = Time.time + fadeTime;
			fadeType = FadeType.fadeIn;
			
			SetMaxVolume ();
			isFading = true;
			isPlaying = true;
			audioSource.volume = 0f;
			audioSource.timeSamples = 0;
			audioSource.Play ();
		}
		
		
		public void FadeOut (float fadeTime)
		{
			if (isPlaying)
			{
				fadeStartTime = Time.time;
				fadeEndTime = Time.time + fadeTime;
				fadeType = FadeType.fadeOut;
				
				SetMaxVolume ();
				isFading = true;
			}
		}


		public bool IsFadingOut ()
		{
			if (isFading && fadeType == FadeType.fadeOut)
			{
				return true;
			}
			return false;
		}


		public void Play ()
		{
			if (audioSource == null)
			{
				return;
			}
			isFading = false;
			isPlaying = true;
			SetMaxVolume ();
			audioSource.Play ();
		}
		
		
		public void Play (bool loop)
		{
			audioSource.loop = loop;
			audioSource.timeSamples = 0;
			Play ();
		}


		public void Play (AudioClip clip, bool loop)
		{
			audioSource.clip = clip;
			audioSource.loop = loop;
			audioSource.timeSamples = 0;
			Play ();
		}


		public void PlayAtPoint (bool loop, int samplePoint)
		{
			audioSource.loop = loop;
			audioSource.timeSamples = samplePoint;
			Play ();
		}
		
		
		public void SetMaxVolume ()
		{
			maxVolume = relativeVolume;
			
			if (KickStarter.options && KickStarter.options.optionsData != null && soundType != SoundType.Other)
			{
				if (soundType == SoundType.Music)
				{
					maxVolume *= KickStarter.options.optionsData.musicVolume;
				}
				else if (soundType == SoundType.SFX)
				{
					maxVolume *= KickStarter.options.optionsData.sfxVolume;
				}
			}

			if (KickStarter.dialog.AudioIsPlaying ())
			{
				if (soundType == SoundType.SFX)
				{
					maxVolume *= 1f - KickStarter.speechManager.sfxDucking;
				}
				else if (soundType == SoundType.Music)
				{
					maxVolume *= 1f - KickStarter.speechManager.musicDucking;
				}
			}

			if (isPlaying && playWhilePaused && KickStarter.stateHandler && KickStarter.stateHandler.gameState == GameState.Paused)
			{
				smoothVolume = maxVolume;
			}
		}


		public void Stop ()
		{
			isFading = false;
			isPlaying = false;
			audioSource.Stop ();
		}


		public bool IsFading ()
		{
			return isFading;
		}


		public bool IsPlaying ()
		{
			return isPlaying;
		}


		public bool IsPlaying (AudioClip clip)
		{
			if (audioSource != null && clip != null && audioSource.clip != null && audioSource.clip == clip && isPlaying)
			{
				return true;
			}
			return false;
		}
		
			
		public bool canDestroy
		{
			get
			{
				if (surviveSceneChange && !isPlaying)
				{
					return true;
				}
				return false;
			}
		}


		public void EndOldMusic (Sound newSound)
		{
			if (soundType == SoundType.Music && isPlaying && this != newSound)
			{
				if (!isFading || fadeType == FadeType.fadeIn)
				{
					FadeOut (0.1f);
				}
			}
		}


		private void TurnOn ()
		{
			audioSource.timeSamples = 0;
			Play ();
		}


		private void TurnOff ()
		{
			FadeOut (0.2f);
		}


		private void Kill ()
		{
			Stop ();
		}

	}
	
}
