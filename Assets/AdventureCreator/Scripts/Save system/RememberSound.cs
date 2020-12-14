/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"RememberSound.cs"
 * 
 *	This script is attached to Sound objects in the scene
 *	we wish to save.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	[RequireComponent (typeof (AudioSource))]
	[RequireComponent (typeof (Sound))]
	public class RememberSound : Remember
	{
		
		public override string SaveData ()
		{
			Sound sound = GetComponent <Sound>();
			AudioSource audioSource = GetComponent <AudioSource>();

			SoundData soundData = new SoundData();
			soundData.objectID = constantID;
			if (sound.IsFadingOut ())
			{
				soundData.isPlaying = false;
			}
			else
			{
				soundData.isPlaying = sound.IsPlaying ();
			}
			soundData.isLooping = audioSource.loop;
			soundData.samplePoint = audioSource.timeSamples;
			soundData.relativeVolume = sound.relativeVolume;

			if (audioSource.clip != null)
			{
				soundData.clipID = AssetLoader.GetAssetInstanceID (audioSource.clip);
			}
			
			return Serializer.SaveScriptData <SoundData> (soundData);
		}
		
		
		public override void LoadData (string stringData, bool restoringSaveFile)
		{
			SoundData data = Serializer.LoadScriptData <SoundData> (stringData);
			if (data == null) return;
			
			Sound sound = GetComponent <Sound>();
			AudioSource audioSource = GetComponent <AudioSource>();

			sound.relativeVolume = data.relativeVolume;
			if (!restoringSaveFile && sound.surviveSceneChange)
			{
				return;
			}

			if (data.isPlaying)
			{
				audioSource.clip = AssetLoader.RetrieveAsset (audioSource.clip, data.clipID);
				sound.PlayAtPoint (data.isLooping, data.samplePoint);
			}
			else
			{
				sound.Stop ();
			}
		}
		
	}
	
	
	[System.Serializable]
	public class SoundData : RememberData
	{
		public bool isPlaying;
		public bool isLooping;
		public int samplePoint;
		public string clipID;
		public float relativeVolume;

		public SoundData () { }
	}
	
}