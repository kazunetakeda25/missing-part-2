/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"Dialog.cs"
 * 
 *	This script handles the running of dialogue lines, speech or otherwise.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace AC
{
	
	public class Dialog : MonoBehaviour
	{
		
		public List<Speech> speechList = new List<Speech>();

		private AudioSource defaultAudioSource;


		private void Awake ()
		{
			if (KickStarter.speechManager.textScrollSpeed == 0f)
			{
				Debug.LogError ("Cannot have a Text Scroll Speed of zero - please amend your Speech Manager");
			}
			
			if (KickStarter.sceneSettings.defaultSound && KickStarter.sceneSettings.defaultSound.GetComponent <AudioSource>())
			{
				defaultAudioSource = this.GetComponent <SceneSettings>().defaultSound.GetComponent <AudioSource>();
			}
		}


		public void _Update ()
		{
			for (int i=0; i<speechList.Count; i++)
			{
				speechList[i]._Update ();
				if (!speechList[i].isAlive)
				{
					EndSpeech (i);
					return;
				}
			}
		}
		
		
		public Speech StartDialog (Char _speaker, string _text, int lineID, string _language, bool isBackground, bool noAnimation)
		{
			// Remove speaker's previous line
			for (int i=0; i<speechList.Count; i++)
			{
				if (speechList[i].GetSpeakingCharacter () == _speaker)
				{
					EndSpeech (i);
					i=0;
				}
			}
			
			Speech speech = new Speech (_speaker, _text, lineID, _language, isBackground, noAnimation);
			speechList.Add (speech);

			KickStarter.runtimeVariables.AddToSpeechLog (speech.log);
			KickStarter.playerMenus.AssignSpeechToMenu (speech);

			if (speech.hasAudio)
			{
				KickStarter.stateHandler.UpdateAllMaxVolumes ();
			}

			return speech;
		}


		private void EndSpeech (int i, bool stopCharacter = false)
		{
			Speech oldSpeech = speechList[i];
			KickStarter.playerMenus.RemoveSpeechFromMenu (oldSpeech);
			if (stopCharacter && oldSpeech.GetSpeakingCharacter ())
			{
				oldSpeech.GetSpeakingCharacter ().StopSpeaking ();
			}
			speechList.RemoveAt (i);

			if (oldSpeech.hasAudio)
			{
				KickStarter.stateHandler.UpdateAllMaxVolumes ();
			}
		}


		public AudioSource GetDefaultAudioSource ()
		{
			return defaultAudioSource;
		}


		public void PlayScrollAudio ()
		{
			if (KickStarter.speechManager.textScrollCLip)
			{
				if (defaultAudioSource)
				{
					if (!defaultAudioSource.isPlaying)
					{
						defaultAudioSource.PlayOneShot (KickStarter.speechManager.textScrollCLip);
					}
				}
				else
				{
					Debug.LogWarning ("Cannot play text scroll audio clip as no 'Default' sound prefab has been defined in the Scene Manager");
				}
			}
		}


		public Speech GetLatestSpeech ()
		{
			if (speechList.Count > 0)
			{
				return speechList [speechList.Count - 1];
			}
			return null;
		}


		public bool FoundAudio ()
		{
			foreach (Speech speech in speechList)
			{
				if (speech.hasAudio)
				{
					return true;
				}
			}
			return false;
		}
		
		
		public string GetSpeaker ()
		{
			if (speechList.Count > 0)
			{
				return GetLatestSpeech ().GetSpeaker ();
			}			
			return "";
		}


		public bool CharacterIsSpeaking (Char _char)
		{
			foreach (Speech speech in speechList)
			{
				if (speech.GetSpeakingCharacter () == _char)
				{
					return true;
				}
			}
			return false;
		}
		
		
		public AC.Char GetSpeakingCharacter ()
		{
			if (speechList.Count > 0)
			{
				return GetLatestSpeech ().GetSpeakingCharacter ();
			}
			return null;
		}


		public bool AudioIsPlaying ()
		{
			if (KickStarter.options != null && KickStarter.options.optionsData.speechVolume > 0f)
			{
				foreach (Speech speech in speechList)
				{
					if (speech.hasAudio && speech.isAlive)
					{
						return true;
					}
				}
			}
			return false;
		}

		
		public void KillDialog (bool stopCharacter, bool forceMenusOff)
		{
			for (int i=0; i<speechList.Count; i++)
			{
				EndSpeech (i, stopCharacter);
				i=0;
			}
			
			KickStarter.stateHandler.UpdateAllMaxVolumes ();
			if (forceMenusOff)
			{
				KickStarter.playerMenus.ForceOffSubtitles ();
			}
		}


		public List<LipSyncShape> GenerateLipSyncShapes (LipSyncMode _lipSyncMode, int lineNumber, string speakerName, string language, string _message)
		{
			List<LipSyncShape> lipSyncShapes = new List<LipSyncShape>();
			lipSyncShapes.Add (new LipSyncShape (0, 0f, KickStarter.speechManager.lipSyncSpeed));
			TextAsset textFile = null;

			if (_lipSyncMode == LipSyncMode.Salsa2D)
			{
				return lipSyncShapes;
			}
			
			if (lineNumber > -1 && speakerName != "" && KickStarter.speechManager.searchAudioFiles)
			{
				string filename = "Lipsync/";
				if (KickStarter.speechManager.placeAudioInSubfolders)
				{
					filename += "/" + speakerName + "/";
				}
				if (language != "" && KickStarter.speechManager.translateAudio)
				{
					// Not in original language
					filename += language + "/";
				}
				filename += speakerName + lineNumber;
				textFile = Resources.Load (filename) as TextAsset;
			}
			
			if (_lipSyncMode == LipSyncMode.ReadPamelaFile && textFile != null)
			{
				string[] pamLines = textFile.text.Split('\n');
				bool foundSpeech = false;
				float fps = 24f;
				foreach (string pamLine in pamLines)
				{
					if (!foundSpeech)
					{
						if (pamLine.Contains ("framespersecond:"))
						{
							string[] pamLineArray = pamLine.Split(':');
							float.TryParse (pamLineArray[1], out fps);
						}
						else if (pamLine.Contains ("[Speech]"))
						{
							foundSpeech = true;
						}
					}
					else if (pamLine.Contains (":"))
					{
						string[] pamLineArray = pamLine.Split(':');
						
						float timeIndex = 0f;
						float.TryParse (pamLineArray[0], out timeIndex);
						string searchText = pamLineArray[1].ToLower ().Substring (0, pamLineArray[1].Length-1);
						
						bool found = false;
						foreach (string phoneme in KickStarter.speechManager.phonemes)
						{
							string[] shapesArray = phoneme.ToLower ().Split ("/"[0]);
							if (!found)
							{
								foreach (string shape in shapesArray)
								{
									if (shape == searchText)
									{
										int frame = KickStarter.speechManager.phonemes.IndexOf (phoneme);
										lipSyncShapes.Add (new LipSyncShape (frame, timeIndex, KickStarter.speechManager.lipSyncSpeed, fps));
										found = true;
									}
								}
							}
						}
						if (!found)
						{
							lipSyncShapes.Add (new LipSyncShape (0, timeIndex, KickStarter.speechManager.lipSyncSpeed, fps));
						}
					}
				}
			}
			else if (_lipSyncMode == LipSyncMode.ReadSapiFile && textFile != null)
			{
				string[] sapiLines = textFile.text.Split('\n');
				foreach (string sapiLine in sapiLines)
				{
					if (sapiLine.StartsWith ("phn "))
					{
						string[] sapiLineArray = sapiLine.Split(' ');
						
						float timeIndex = 0f;
						float.TryParse (sapiLineArray[1], out timeIndex);
						string searchText = sapiLineArray[4].ToLower ().Substring (0, sapiLineArray[4].Length-1);
						bool found = false;
						foreach (string phoneme in KickStarter.speechManager.phonemes)
						{
							string[] shapesArray = phoneme.ToLower ().Split ("/"[0]);
							if (!found)
							{
								foreach (string shape in shapesArray)
								{
									if (shape == searchText)
									{
										int frame = KickStarter.speechManager.phonemes.IndexOf (phoneme);
										lipSyncShapes.Add (new LipSyncShape (frame, timeIndex, KickStarter.speechManager.lipSyncSpeed, 60f));
										found = true;
									}
								}
							}
						}
						if (!found)
						{
							lipSyncShapes.Add (new LipSyncShape (0, timeIndex, KickStarter.speechManager.lipSyncSpeed, 60f));
						}
					}
				}
			}
			else if (_lipSyncMode == LipSyncMode.ReadPapagayoFile && textFile != null)
			{
				string[] papagoyoLines = textFile.text.Split('\n');
				foreach (string papagoyoLine in papagoyoLines)
				{
					if (papagoyoLine != "" && !papagoyoLine.Contains ("MohoSwitch"))
					{
						string[] papagoyoLineArray = papagoyoLine.Split(' ');
						
						float timeIndex = 0f;
						float.TryParse (papagoyoLineArray[0], out timeIndex);
						string searchText = papagoyoLineArray[1].ToLower ().Substring (0, papagoyoLineArray[1].Length);
						
						bool found = false;
						if (!searchText.Contains ("rest") && !searchText.Contains ("etc"))
						{
							foreach (string phoneme in KickStarter.speechManager.phonemes)
							{
								string[] shapesArray = phoneme.ToLower ().Split ("/"[0]);
								if (!found)
								{
									foreach (string shape in shapesArray)
									{
										if (shape == searchText)
										{
											int frame = KickStarter.speechManager.phonemes.IndexOf (phoneme);
											lipSyncShapes.Add (new LipSyncShape (frame, timeIndex, KickStarter.speechManager.lipSyncSpeed, 24f));
											
											found = true;
										}
									}
								}
							}
						}
						if (!found)
						{
							lipSyncShapes.Add (new LipSyncShape (0, timeIndex, KickStarter.speechManager.lipSyncSpeed, 240f));
						}
					}
				}
			}
			else if (_lipSyncMode == LipSyncMode.FromSpeechText)
			{
				for (int i=0; i<_message.Length; i++)
				{
					int maxSearch = Mathf.Min (5, _message.Length - i);
					for (int n=maxSearch; n>0; n--)
					{
						string searchText = _message.Substring (i, n);
						searchText = searchText.ToLower ();
						
						foreach (string phoneme in KickStarter.speechManager.phonemes)
						{
							string[] shapesArray = phoneme.ToLower ().Split ("/"[0]);
							foreach (string shape in shapesArray)
							{
								if (shape == searchText)
								{
									int frame = KickStarter.speechManager.phonemes.IndexOf (phoneme);
									lipSyncShapes.Add (new LipSyncShape (frame, (float) i, KickStarter.speechManager.lipSyncSpeed));
									i += n;
									n = Mathf.Min (5, _message.Length - i);
									break;
								}
							}
						}
						
					}
					lipSyncShapes.Add (new LipSyncShape (0, (float) i, KickStarter.speechManager.lipSyncSpeed));
				}
			}
			
			if (lipSyncShapes.Count > 1)
			{
				lipSyncShapes.Sort (delegate (LipSyncShape a, LipSyncShape b) {return a.timeIndex.CompareTo (b.timeIndex);});
			}
			
			return lipSyncShapes;
		}


		private void OnDestroy ()
		{
			defaultAudioSource = null;
		}

	}
	
	
	public struct SpeechGap
	{
		
		public int characterIndex;
		public float waitTime;
		
		public SpeechGap (int _characterIndex, float _waitTime)
		{
			characterIndex = _characterIndex;
			waitTime = _waitTime;
		}
		
	}
	
	
	public struct LipSyncShape
	{
		
		public int frame;
		public float timeIndex;
		
		
		public LipSyncShape (int _frame, float _timeIndex, float speed, float fps)
		{
			// Pamela / Sapi
			frame = _frame;
			timeIndex = (_timeIndex / 15f / speed / fps) + Time.time;
		}
		
		
		public LipSyncShape (int _frame, float _timeIndex, float speed)
		{
			// Automatic
			frame = _frame;
			timeIndex = (_timeIndex / 15f / speed) + Time.time;
		}
		
	}
	
}