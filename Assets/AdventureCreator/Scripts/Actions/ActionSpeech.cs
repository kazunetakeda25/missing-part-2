/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"ActionSpeech.cs"
 * 
 *	This action handles the displaying of messages, and talking of characters.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{
	
	[System.Serializable]
	public class ActionSpeech : Action
	{
		
		public int constantID = 0;
		public int parameterID = -1;
		
		public int messageParameterID = -1;
		
		public bool isPlayer;
		public Char speaker;
		public string messageText;
		public int lineID;
		public bool isBackground = false;
		public bool noAnimation = false;
		public AnimationClip headClip;
		public AnimationClip mouthClip;
		
		public bool play2DHeadAnim = false;
		public string headClip2D = "";
		public int headLayer;
		public bool play2DMouthAnim = false;
		public string mouthClip2D = "";
		public int mouthLayer;
		
		public float waitTimeOffset = 0f;
		private float endTime = 0f;
		private bool stopAction = false;
		
		private int splitNumber = 0;
		private bool splitDelay = false;

		private Speech speech;
		
		public static string[] stringSeparators = new string[] {"\n", "\\n"};
		
		
		public ActionSpeech ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Dialogue;
			title = "Play speech";
			description = "Makes a Character talk, or – if no Character is specified – displays a message. Subtitles only appear if they are enabled from the Options menu. A 'thinking' effect can be produced by opting to not play any animation.";
			lineID = -1;
		}
		
		
		override public void AssignValues (List<ActionParameter> parameters)
		{
			speaker = AssignFile <Char> (parameters, parameterID, constantID, speaker);
			messageText = AssignString (parameters, messageParameterID, messageText);
			
			if (isPlayer)
			{
				speaker = KickStarter.player;
			}
		}
		
		
		override public float Run ()
		{
			if (KickStarter.speechManager == null)
			{
				Debug.Log ("No Speech Manager present");
				return 0f;
			}
			
			if (KickStarter.dialog && KickStarter.stateHandler)
			{
				if (!isRunning)
				{
					stopAction = false;
					isRunning = true;
					splitDelay = false;
					splitNumber = 0;

					endTime = Time.time + StartSpeech ();

					if (isBackground)
					{
						isRunning = false;
						return 0f;
					}
					return defaultPauseTime;
				}
				else
				{
					if (stopAction || (speech != null && speech.continueFromSpeech))
					{
						speech.continueFromSpeech = false;
						isRunning = false;
						return 0;
					}
					
					if (speech == null || !speech.isAlive)
					{
						if (KickStarter.speechManager.separateLines)
						{
							if (!splitDelay)
							{
								// Begin pause if more lines are present
								splitNumber ++;
								string[] textArray = messageText.Split (stringSeparators, System.StringSplitOptions.None);
								
								if (textArray.Length > splitNumber)
								{
									// Still got more to go, so pause for a moment
									splitDelay = true;
									return KickStarter.speechManager.separateLinePause;
								}
								// else finished
							}
							else
							{
								// Show next line
								splitDelay = false;
								endTime = Time.time + StartSpeech ();
								return defaultPauseTime;
							}
						}
						
						if (waitTimeOffset <= 0f)
						{
							isRunning = false;
							return 0f;
						}
						else
						{
							stopAction = true;
							return waitTimeOffset;
						}
					}
					else
					{
						if (KickStarter.speechManager.displayForever || speech.pauseGap)
						{
							return defaultPauseTime;
						}
						
						if (KickStarter.speechManager.separateLines)
						{
							return defaultPauseTime;
						}
						
						if (!speech.HasPausing ())
						{
							// Ignore this if we're using [wait] tokens
							if (Time.time < endTime)
							{
								return defaultPauseTime;
							}
							else
							{
								isRunning = false;
								return 0f;
							}
						}
					}
				}
			}
			
			return 0f;
		}
		
		
		override public void Skip ()
		{
			KickStarter.dialog.KillDialog (true, true);

			SpeechLog log = new SpeechLog ();
			log.lineID = lineID;
			log.fullText = messageText;

			if (speaker)
			{
				log.speakerName = speaker.name;
				if (!noAnimation)
				{
					speaker.isTalking = false;
					
					if (speaker.animEngine == null)
					{
						speaker.ResetAnimationEngine ();
					}
					
					if (speaker.animEngine != null)
					{
						speaker.animEngine.ActionSpeechSkip (this);
					}
				}
			}

			KickStarter.runtimeVariables.AddToSpeechLog (log);
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			if (lineID > -1)
			{
				EditorGUILayout.LabelField ("Speech Manager ID:", lineID.ToString ());
			}
			
			isPlayer = EditorGUILayout.Toggle ("Player line?",isPlayer);
			if (isPlayer)
			{
				if (Application.isPlaying)
				{
					speaker = KickStarter.player;
				}
				else
				{
					speaker = AdvGame.GetReferences ().settingsManager.GetDefaultPlayer ();
				}
			}
			else
			{
				parameterID = Action.ChooseParameterGUI ("Speaker:", parameters, parameterID, ParameterType.GameObject);
				if (parameterID >= 0)
				{
					constantID = 0;
					speaker = null;
				}
				else
				{
					speaker = (Char) EditorGUILayout.ObjectField ("Speaker:", speaker, typeof(Char), true);
					
					constantID = FieldToID <Char> (speaker, constantID);
					speaker = IDToField <Char> (speaker, constantID, false);
				}
			}
			
			messageParameterID = Action.ChooseParameterGUI ("Line text:", parameters, messageParameterID, ParameterType.String);
			if (messageParameterID < 0)
			{
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Line text:", GUILayout.Width (65f));
				EditorStyles.textField.wordWrap = true;
				messageText = EditorGUILayout.TextArea (messageText, GUILayout.MaxWidth (400f));
				EditorGUILayout.EndHorizontal ();
			}
			
			if (speaker)
			{
				noAnimation = EditorGUILayout.Toggle ("Don't animate speaker?", noAnimation);
				
				if (!noAnimation)
				{
					if (speaker.animEngine == null)
					{
						speaker.ResetAnimationEngine ();
					}
					if (speaker.animEngine)
					{
						speaker.animEngine.ActionSpeechGUI (this);
					}
				}
			}
			else if (!isPlayer)
			{
				EditorGUILayout.HelpBox ("If no Character is set, this line will be considered to be a Narration.", MessageType.Info);
			}
			
			isBackground = EditorGUILayout.Toggle ("Play in background?", isBackground);
			if (!isBackground)
			{
				waitTimeOffset = EditorGUILayout.Slider ("Wait time offset (s):", waitTimeOffset, -1f, 4f);
			}
			
			AfterRunningOption ();
		}


		public void SetSpeaker ()
		{
			if (!isPlayer && parameterID == -1)
			{
				speaker = IDToField <Char> (speaker, constantID, false);
			}
		}
		
		
		override public string SetLabel ()
		{
			if (parameterID == -1)
			{
				if (speaker)
				{
					return " (" + speaker.gameObject.name + ")";
				}
				else if (isPlayer)
				{
					return " (Player)";
				}
				else
				{
					return " (Narrator)";
				}
			}
			return "";
		}
		
		#endif
		
		
		private float StartSpeech ()
		{
			string _text = messageText;
			string _language = "";
			int _lineID = lineID;
			
			int lanuageNumber = Options.GetLanguage ();
			if (lanuageNumber > 0)
			{
				// Not in original language, so pull translation in from Speech Manager
				_text = SpeechManager.GetTranslation (_text, lineID, lanuageNumber);
				_language = KickStarter.speechManager.languages [lanuageNumber];
			}
			
			bool isSplittingLines = false;
			bool isLastSplitLine = false;

			_text = _text.Replace ("\\n", "\n");

			if (KickStarter.speechManager.separateLines)
			{
				// Split line into an array, and pull the correct one
				string[] textArray = _text.Split (stringSeparators, System.StringSplitOptions.None);
				_text = textArray [splitNumber];

				if (textArray.Length > 1)
				{
					isSplittingLines = true;

					if (splitNumber > 0)
					{
						_lineID = -1;
					}
					if (textArray.Length > splitNumber)
					{
						isLastSplitLine = true;
					}
				}
			}
			
			if (_text != "")
			{
				speech = KickStarter.dialog.StartDialog (speaker, _text, _lineID, _language, isBackground, noAnimation);
				float displayDuration = speech.displayDuration;

				if (speaker && !noAnimation)
				{
					if (speaker.animEngine == null)
					{
						speaker.ResetAnimationEngine ();
					}
					
					if (speaker.animEngine != null)
					{
						speaker.animEngine.ActionSpeechRun (this);
					}
				}
				
				if (isLastSplitLine)
				{
					return (displayDuration + waitTimeOffset);
				}
				
				if (isSplittingLines)
				{
					return displayDuration;
				}
				
				if (!isBackground)
				{
					return (displayDuration + waitTimeOffset);
				}
			}
			
			return 0f;
		}
		
	}
	
}