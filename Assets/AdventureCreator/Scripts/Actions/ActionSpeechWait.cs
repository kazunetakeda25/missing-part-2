/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"ActionSpeechWait.cs"
 * 
 *	This Action waits until a particular character has stopped speaking.
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
	public class ActionSpeechWait : Action
	{

		public int constantID = 0;
		public int parameterID = -1;

		public bool isPlayer;
		public Char speaker;

		
		public ActionSpeechWait ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Dialogue;
			title = "Wait for speech";
			description = "Waits until a particular character has stopped speaking.";
		}
		
		
		override public void AssignValues (List<ActionParameter> parameters)
		{
			speaker = AssignFile <Char> (parameters, parameterID, constantID, speaker);

			if (isPlayer)
			{
				speaker = KickStarter.player;
			}
		}


		override public float Run ()
		{
			if (speaker == null)
			{
				Debug.LogWarning ("No speaker set.");
			}
			else if (!isRunning)
			{
				isRunning = true;

				if (KickStarter.dialog.CharacterIsSpeaking (speaker))
				{
					return defaultPauseTime;
				}
			}
			else
			{
				if (KickStarter.dialog.CharacterIsSpeaking (speaker))
				{
					return defaultPauseTime;
				}
				else
				{
					isRunning = false;
				}
			}
			
			return 0f;
		}


		override public void Skip ()
		{
			return;
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
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
			
			AfterRunningOption ();
		}
		
		
		public override string SetLabel ()
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
			}
			return "";
		}
		
		#endif
		
	}
	
}