/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"Options.cs"
 * 
 *	This script provides a runtime instance of OptionsData,
 *	and has functions for saving and loading this data
 *	into the PlayerPrefs
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	public class Options : MonoBehaviour
	{
		
		public OptionsData optionsData;
		public static int languageNumber = 0;

		private string ppKey = "Options";

		
		private void Awake ()
		{
			if (KickStarter.settingsManager)
			{
				optionsData = new OptionsData (KickStarter.settingsManager.defaultLanguage, KickStarter.settingsManager.defaultShowSubtitles, KickStarter.settingsManager.defaultSfxVolume, KickStarter.settingsManager.defaultMusicVolume, KickStarter.settingsManager.defaultSpeechVolume);
			}
			else
			{
				optionsData = new OptionsData ();
			}
			LoadPrefs ();

			if (optionsData.language == 0 && KickStarter.speechManager && KickStarter.speechManager.ignoreOriginalText && KickStarter.speechManager.languages.Count > 1)
			{
				// Ignore original language
				optionsData.language = 1;
				SavePrefs ();
			}

			Options.languageNumber = optionsData.language;
			OnLevelWasLoaded ();
		}
		
		
		public void SavePrefs ()
		{
			// Linked Variables
			RuntimeVariables.DownloadAll ();
			optionsData.linkedVariables = SaveSystem.CreateVariablesData (KickStarter.runtimeVariables.globalVars, true, VariableLocation.Global);
			
			string optionsBinary = Serializer.SerializeObjectBinary (optionsData);
			PlayerPrefs.SetString (ppKey, optionsBinary);
			
			if (Application.isPlaying)
			{
				CustomSaveOptionsHook ();
			}

			Debug.Log ("PlayerPrefs saved.");
		}
		
		
		private void LoadPrefs ()
		{
			if (Application.isPlaying)
			{
				CustomLoadOptionsHook ();
			}

			if (PlayerPrefs.HasKey (ppKey))
			{
				string optionsBinary = PlayerPrefs.GetString (ppKey);
				optionsData = Serializer.DeserializeObjectBinary <OptionsData> (optionsBinary);
				Options.languageNumber = optionsData.language;

				Debug.Log ("PlayerPrefs loaded.");
			}
		}
		
		
		private void OnLevelWasLoaded ()
		{
			#if UNITY_5
			if (KickStarter.settingsManager.volumeControl == VolumeControl.AudioSources)
			{
				SetVolume (SoundType.Music);
				SetVolume (SoundType.SFX);
			}
			else
			{
				AdvGame.SetMixerVolume (KickStarter.settingsManager.musicMixerGroup, KickStarter.settingsManager.musicAttentuationParameter, optionsData.musicVolume);
				AdvGame.SetMixerVolume (KickStarter.settingsManager.sfxMixerGroup, KickStarter.settingsManager.sfxAttentuationParameter, optionsData.sfxVolume);
				AdvGame.SetMixerVolume (KickStarter.settingsManager.speechMixerGroup, KickStarter.settingsManager.speechAttentuationParameter, optionsData.speechVolume);
			}
			#else
			SetVolume (SoundType.Music);
			SetVolume (SoundType.SFX);
			#endif
		}


		public void SetVolume (SoundType _soundType)
		{
			Sound[] soundObs = FindObjectsOfType (typeof (Sound)) as Sound[];
			foreach (Sound soundOb in soundObs)
			{
				if (soundOb.soundType == _soundType)
				{
					soundOb.AfterLoading ();
				}
			}
		}


		public static void SetLanguage (int i)
		{
			if (KickStarter.options && KickStarter.options.optionsData != null)
			{
				KickStarter.options.optionsData.language = i;
				KickStarter.options.SavePrefs ();
				Options.languageNumber = i;
			}
			else
			{
				Debug.LogWarning ("Could not find Options data!");
			}
		}


		public static string GetLanguageName ()
		{
			return KickStarter.speechManager.languages [GetLanguage ()];
		}
		
		
		public static int GetLanguage ()
		{
			return languageNumber;
		}


		private void CustomSaveOptionsHook ()
		{
			ISaveOptions[] saveOptionsHooks = GetSaveOptionsHooks (GetComponents (typeof (ISaveOptions)));
			if (saveOptionsHooks != null && saveOptionsHooks.Length > 0)
			{
				foreach (ISaveOptions saveOptionsHook in saveOptionsHooks)
				{
					saveOptionsHook.PreSaveOptions ();
				}
			}
		}
		
		
		private void CustomLoadOptionsHook ()
		{
			ISaveOptions[] saveOptionsHooks = GetSaveOptionsHooks (GetComponents (typeof (ISaveOptions)));
			if (saveOptionsHooks != null && saveOptionsHooks.Length > 0)
			{
				foreach (ISaveOptions saveOptionsHook in saveOptionsHooks)
				{
					saveOptionsHook.PostLoadOptions ();
				}
			}
		}
		
		
		private ISaveOptions[] GetSaveOptionsHooks (IList list)
		{
			ISaveOptions[] ret = new ISaveOptions[list.Count];
			list.CopyTo (ret, 0);
			return ret;
		}
		
	}

}