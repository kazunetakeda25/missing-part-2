/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"Dialog.cs"
 * 
 *	This class processes any dialogue line.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{

	public class Speech
	{

		public SpeechLog log;

		public string displayText;
		public bool isBackground;
		public float displayDuration;
		public bool isAlive;

		private int gapIndex = -1;
		private int continueIndex = -1;
		private List<SpeechGap> speechGaps = new List<SpeechGap>();
		private float endTime;
		private float continueTime;

		private AC.Char speaker;
		private bool isSkippable;
		public bool pauseGap;
		public bool hasAudio;

		private float scrollAmount = 0f;
		private float pauseEndTime = 0f;
		public bool continueFromSpeech = false;


		public void EndPause ()
		{
			pauseGap = false;
			gapIndex ++;
			scrollAmount = 0f;
		}

		
		public Speech (Char _speaker, string _message, int lineID, string _language, bool _isBackground, bool _noAnimation)
		{
			log.Clear ();
			isBackground = _isBackground;
			
			if (_speaker)
			{
				speaker = _speaker;
				speaker.isTalking = !_noAnimation;
				log.speakerName = _speaker.name;
				
				if (_speaker.GetComponent <Player>())
				{
					if (KickStarter.settingsManager.playerSwitching == PlayerSwitching.Allow || !KickStarter.speechManager.usePlayerRealName)
					{
						log.speakerName = "Player";
					}
				}
				
				if (_speaker.GetComponent <Hotspot>())
				{
					if (_speaker.GetComponent <Hotspot>().hotspotName != "")
					{
						log.speakerName = _speaker.GetComponent <Hotspot>().hotspotName;
					}
				}
				
				if (_speaker.portraitIcon != null)
				{
					_speaker.portraitIcon.Reset ();
				}
				
				if (!_noAnimation)
				{
					if (KickStarter.speechManager.lipSyncMode == LipSyncMode.Off)
					{
						speaker.isLipSyncing = false;
					}
					else if (KickStarter.speechManager.lipSyncMode == LipSyncMode.Salsa2D || KickStarter.speechManager.lipSyncMode == LipSyncMode.FromSpeechText || KickStarter.speechManager.lipSyncMode == LipSyncMode.ReadPamelaFile || KickStarter.speechManager.lipSyncMode == LipSyncMode.ReadSapiFile || KickStarter.speechManager.lipSyncMode == LipSyncMode.ReadPapagayoFile)
					{
						speaker.StartLipSync (KickStarter.dialog.GenerateLipSyncShapes (KickStarter.speechManager.lipSyncMode, lineID, log.speakerName, _language, _message));
					}
				}
			}
			else
			{
				if (speaker)
				{
					speaker.isTalking = false;
				}
				speaker = null;			
				log.speakerName = "Narrator";
			}
			
			_message = AdvGame.ConvertTokens (_message);

			if (lineID > -1)
			{
				log.lineID = lineID;
			}

			// Play sound and time displayDuration to it
			if (lineID > -1 && log.speakerName != "" && KickStarter.speechManager.searchAudioFiles)
			{
				string fullFilename = "Speech/";
				string filename = KickStarter.speechManager.GetLineFilename (lineID);
				if (_language != "" && KickStarter.speechManager.translateAudio)
				{
					// Not in original language
					fullFilename += _language + "/";
				}
				if (KickStarter.speechManager.placeAudioInSubfolders)
				{
					fullFilename += filename + "/";
				}
				fullFilename += filename + lineID;
				
				AudioClip clipObj = Resources.Load (fullFilename) as AudioClip;
				if (clipObj)
				{
					AudioSource audioSource = null;

					if (_speaker != null)
					{
						if (!_noAnimation)
						{
							if (KickStarter.speechManager.lipSyncMode == LipSyncMode.FaceFX)
							{
								FaceFXIntegration.Play (speaker, log.speakerName + lineID, clipObj);
							}
						}
						
						if (_speaker.GetComponent <AudioSource>())
						{
							_speaker.GetComponent <AudioSource>().volume = KickStarter.options.optionsData.speechVolume;
							audioSource = _speaker.GetComponent <AudioSource>();
						}
						else
						{
							Debug.LogWarning (_speaker.name + " has no audio source component!");
						}
					}
					else if (KickStarter.player && KickStarter.player.GetComponent <AudioSource>())
					{
						KickStarter.player.GetComponent <AudioSource>().volume = KickStarter.options.optionsData.speechVolume;
						audioSource = KickStarter.player.GetComponent <AudioSource>();
					}
					else
					{
						audioSource = KickStarter.dialog.GetDefaultAudioSource ();
					}
					
					if (audioSource != null)
					{
						audioSource.clip = clipObj;
						audioSource.loop = false;
						audioSource.Play();
						hasAudio = true;
					}
					
					displayDuration = clipObj.length;
				}
				else
				{
					displayDuration = KickStarter.speechManager.screenTimeFactor * (float) _message.Length;
					if (displayDuration < 0.5f)
					{
						displayDuration = 0.5f;
					}
					
					Debug.Log ("Cannot find audio file: " + fullFilename);
				}
			}
			else
			{
				displayDuration = KickStarter.speechManager.screenTimeFactor * (float) _message.Length;
				if (displayDuration < 0.5f)
				{
					displayDuration = 0.5f;
				}
			}
			
			_message = DetermineGaps (_message);
			
			if (speechGaps.Count > 0)
			{
				gapIndex = 0;
				foreach (SpeechGap gap in speechGaps)
				{
					displayDuration += (float) gap.waitTime;
				}
			}
			else
			{
				gapIndex = -1;
			}
			
			log.fullText = _message;
			
			if (!KickStarter.speechManager.scrollSubtitles)
			{
				if (continueIndex > 0)
				{
					continueTime = Time.time + (continueIndex / KickStarter.speechManager.textScrollSpeed);
				}
				
				if (speechGaps.Count > 0)
				{
					displayText = log.fullText.Substring (0, speechGaps[0].characterIndex);
				}
				else
				{
					displayText = log.fullText;
				}
			}
			else
			{
				displayText = "";
			}
			
			isAlive = true;
			isSkippable = true;
			pauseGap = false;
			endTime = Time.time + displayDuration;
		}
		

		public void _Update ()
		{
			if (KickStarter.stateHandler.gameState != GameState.Paused)
			{
				UpdateInput ();
			}

			if (pauseGap)
			{
				if (pauseEndTime > 0f && Time.time > pauseEndTime)
				{
					EndPause ();
				}
				else
				{
					return;
				}
			}

			if (KickStarter.speechManager.scrollSubtitles)
			{
				if (scrollAmount < 1f)
				{
					if (!pauseGap)
					{
						scrollAmount += KickStarter.speechManager.textScrollSpeed / 100f / log.fullText.Length;
						if (scrollAmount > 1f)
						{
							scrollAmount = 1f;
						}
						
						int currentCharIndex = (int) (scrollAmount * log.fullText.Length);
						
						if (gapIndex > 0)
						{
							currentCharIndex += speechGaps[gapIndex-1].characterIndex;
							if (currentCharIndex > log.fullText.Length)
							{
								currentCharIndex = log.fullText.Length;
							}
						}
						
						string newText = log.fullText.Substring (0, currentCharIndex);
						
						if (displayText != newText && !hasAudio)
						{
							KickStarter.dialog.PlayScrollAudio ();
						}
						
						displayText = newText;
						if (gapIndex >= 0 && speechGaps.Count > gapIndex)
						{
							if (currentCharIndex == speechGaps [gapIndex].characterIndex)
							{
								float waitTime = speechGaps [gapIndex].waitTime;
								pauseGap = true;
								if (waitTime >= 0f)
								{
									pauseEndTime = Time.time + waitTime;
								}
								else
								{
									pauseEndTime = 0f;
								}
								return;
							}
						}

						if (continueIndex >= 0)
						{
							if (currentCharIndex >= continueIndex)
							{
								continueIndex = -1;
								continueFromSpeech = true;
							}
						}
					}
					return;
				}
				displayText = log.fullText;
			}
			else
			{
				if (gapIndex >= 0 && speechGaps.Count >= gapIndex)
				{
					if (gapIndex == speechGaps.Count)
					{
						displayText = log.fullText;
						foreach (SpeechGap gap in speechGaps)
						{
							endTime -= gap.waitTime;
						}
					}
					else
					{
						float waitTime = (float) speechGaps[gapIndex].waitTime;
						displayText = log.fullText.Substring (0, speechGaps[gapIndex].characterIndex);
						
						if (waitTime >= 0)
						{
							endTime = Time.time + waitTime;
						}
						else
						{
							pauseGap = true;
						}
					}
				}
				else
				{
					displayText = log.fullText;
				}
				
				if (continueIndex >= 0)
				{
					if (continueTime < Time.time)
					{
						continueFromSpeech = true;
					}
				}
			}

			if (Time.time > endTime)
			{
				if ((KickStarter.speechManager.displayForever && isBackground)
				 || !KickStarter.speechManager.displayForever)
				{
					EndMessage ();
				}
			}
		}


		private void EndMessage (bool forceOff = false)
		{
			isSkippable = false;
			
			if (speaker)
			{
				speaker.StopSpeaking ();
			}
			
			if (!forceOff && gapIndex >= 0 && gapIndex < speechGaps.Count)
			{
				gapIndex ++;
			}
			else
			{
				isAlive = false;
				KickStarter.stateHandler.UpdateAllMaxVolumes ();
			}
		}


		private void UpdateInput ()
		{
			if (isSkippable)
			{
				if (pauseGap && !IsBackgroundSpeech ())
				{
					if ((KickStarter.playerInput.mouseState == MouseState.SingleClick || KickStarter.playerInput.mouseState == MouseState.RightClick))
					{
						if (speechGaps[gapIndex].waitTime < 0f)
						{
							KickStarter.playerInput.mouseState = MouseState.Normal;
							EndPause ();
						}
						else if (KickStarter.speechManager.allowSpeechSkipping)
						{
							KickStarter.playerInput.mouseState = MouseState.Normal;
							EndPause ();
						}
					}
				}
				
				else if (KickStarter.speechManager.displayForever)
				{
					if ((KickStarter.playerInput.mouseState == MouseState.SingleClick || KickStarter.playerInput.mouseState == MouseState.RightClick))
					{
						KickStarter.playerInput.mouseState = MouseState.Normal;
						
						if (KickStarter.stateHandler.gameState == GameState.Cutscene)
						{
							if (KickStarter.speechManager.endScrollBeforeSkip && KickStarter.speechManager.scrollSubtitles && displayText != log.fullText)
							{
								// Stop scrolling
								scrollAmount = 1f;
								displayText = log.fullText;
							}
							else
							{
								// Stop message
								EndMessage ();
							}
						}
					}
				}
				
				else if ((KickStarter.playerInput.mouseState == MouseState.SingleClick || KickStarter.playerInput.mouseState == MouseState.RightClick) && KickStarter.speechManager && KickStarter.speechManager.allowSpeechSkipping &&
				         (!IsBackgroundSpeech () || KickStarter.speechManager.allowGameplaySpeechSkipping))
				{
					KickStarter.playerInput.mouseState = MouseState.Normal;
					
					if (KickStarter.stateHandler.gameState == GameState.Cutscene || (KickStarter.speechManager.allowGameplaySpeechSkipping && KickStarter.stateHandler.gameState == GameState.Normal))
					{
						if (KickStarter.speechManager.endScrollBeforeSkip && KickStarter.speechManager.scrollSubtitles && displayText != log.fullText)
						{
							// Stop scrolling
							if (speechGaps.Count > 0 && speechGaps.Count > gapIndex)
							{
								while (gapIndex < speechGaps.Count && speechGaps[gapIndex].waitTime >= 0)
								{
									// Find next wait
									gapIndex ++;
								}
								
								if (gapIndex == speechGaps.Count)
								{
									scrollAmount = 1f;
									displayText = log.fullText;
								}
								else
								{
									pauseGap = true;
									displayText = log.fullText.Substring (0, speechGaps[gapIndex].characterIndex);
								}
							}
							else
							{
								scrollAmount = 1f;
								displayText = log.fullText;
							}
						}
						else
						{
							EndMessage (true);
						}
					}
				}
			}
		}


		private string DetermineGaps (string _text)
		{
			speechGaps.Clear ();
			continueIndex = -1;
			
			if (_text != null)
			{
				if (_text.Contains ("[wait"))
				{
					while (_text.Contains ("[wait"))
					{
						int startIndex = _text.IndexOf ("[wait");

						if (_text.Substring (startIndex).StartsWith ("[wait]"))
						{
							// Indefinite wait
							speechGaps.Add (new SpeechGap (startIndex, -1));
							_text = _text.Substring (0, startIndex) + _text.Substring (startIndex + 6);
						}
						else
						{
							// Timed wait
							int endIndex = _text.IndexOf ("]", startIndex);
							string waitTimeText = _text.Substring (startIndex + 6, endIndex - startIndex - 6);
							speechGaps.Add (new SpeechGap (startIndex, FloatParse (waitTimeText)));
							_text = _text.Substring (0, startIndex) + _text.Substring (endIndex + 1); 
						}
					}
				}
				
				if (_text.Contains ("[continue]"))
				{
					continueIndex = _text.IndexOf ("[continue]");
					_text = _text.Replace ("[continue]", "");
				}
			}

			// Sort speechGaps
			if (speechGaps.Count > 1)
			{
				speechGaps.Sort (delegate (SpeechGap a, SpeechGap b) {return a.characterIndex.CompareTo (b.characterIndex);});
			}
			
			return _text;
		}


		private float FloatParse (string text)
		{
			float _value = 0f;
			if (text != "")
			{
				float.TryParse (text, out _value);
			}
			return _value;
		}


		private bool IsBackgroundSpeech ()
		{
			if (KickStarter.stateHandler.gameState == GameState.Normal)
			{
				return true;
			}
			return false;
		}


		public string GetSpeaker ()
		{
			if (speaker)
			{
				if (speaker.speechLabel != "")
				{
					return speaker.speechLabel;
				}
				return speaker.name;
			}
			
			return "";
		}


		public Color GetColour ()
		{
			if (speaker)
			{
				return speaker.speechColor;
			}
			return Color.white;
		}
		
		
		public AC.Char GetSpeakingCharacter ()
		{
			return speaker;
		}


		public bool HasPausing ()
		{
			if (speechGaps.Count > 0)
			{
				return true;
			}
			return false;
		}


		public Sprite GetPortraitSprite ()
		{
			if (speaker && speaker.portraitIcon.texture)
			{
				if (IsAnimating ())
				{
					if (speaker.isLipSyncing)
					{
						return speaker.portraitIcon.GetAnimatedSprite (speaker.GetLipSyncFrame ());
					}
					else
					{
						return speaker.portraitIcon.GetAnimatedSprite (true);
					}
				}
				else
				{
					return speaker.portraitIcon.GetSprite ();
				}
			}
			return null;
		}
		
		
		public Texture2D GetPortrait ()
		{
			if (speaker && speaker.portraitIcon.texture)
			{
				return speaker.portraitIcon.texture;
			}
			return null;
		}
		
		
		public bool IsAnimating ()
		{
			if (speaker && speaker.portraitIcon.isAnimated)
			{
				return true;
			}
			return false;
		}


		public Rect GetAnimatedRect ()
		{
			if (speaker != null && speaker.portraitIcon != null)
			{
				if (speaker.isLipSyncing)
				{
					return speaker.portraitIcon.GetAnimatedRect (speaker.GetLipSyncFrame ());
				}
				else if (speaker.isTalking)
				{
					return speaker.portraitIcon.GetAnimatedRect ();
				}
				else
				{
					return speaker.portraitIcon.GetAnimatedRect (0);
				}
			}
			return new Rect (0,0,0,0);
		}

	}


	public struct SpeechLog
	{

		public string fullText;
		public string speakerName;
		public int lineID;

		public void Clear ()
		{
			fullText = "";
			speakerName = "";
			lineID = -1;
		}

	}

}