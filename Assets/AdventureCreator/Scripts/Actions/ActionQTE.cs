/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"ActionQTE.cs"
 * 
 *	This action checks if a specific key
 *	is being pressed within a set time limit
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
	public class ActionQTE : ActionCheck
	{

		public enum QTEType { SingleKeypress, HoldKey, ButtonMash };
		public QTEType qteType = QTEType.SingleKeypress;

		public int menuNameParameterID = -1;
		public string menuName;
		public bool animateUI = false;
		public bool wrongKeyFails = false;

		public int inputNameParameterID = -1;
		public string inputName;
		
		public int durationParameterID = -1;
		public float duration;
		
		public float holdDuration;
		public float cooldownTime;
		public int targetPresses;
		public bool doCooldown;

		
		public ActionQTE ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Input;
			title = "QTE";
			description = "Initiates a Quick Time Event for a set duration. The QTE type can either be a single key- press, holding a button down, or button-mashing. The Input button must be defined in Unity's Input Manager.";
		}
		
		
		override public void AssignValues (List<ActionParameter> parameters)
		{
			menuName = AssignString (parameters, menuNameParameterID, menuName);
			inputName = AssignString (parameters, inputNameParameterID, inputName);
			duration = AssignFloat (parameters, durationParameterID, duration);
		}
		
		
		override public float Run ()
		{
			if (duration <= 0f || inputName == "")
			{
				isRunning = false;
				return 0f;
			}

			if (!isRunning)
			{
				isRunning = true;

				Animator animator = null;
				if (menuName != "")
				{
					AC.Menu menu = PlayerMenus.GetMenuWithName (menuName);
					menu.TurnOn (true);
					if (menu.canvas != null && menu.canvas.GetComponent <Animator>())
					{
						animator = menu.canvas.GetComponent <Animator>();
					}
				}

				if (qteType == QTEType.SingleKeypress)
				{
					KickStarter.playerQTE.StartQTE (inputName, duration, animator, wrongKeyFails);
				}
				else if (qteType == QTEType.HoldKey)
				{
					KickStarter.playerQTE.StartQTE (inputName, duration, holdDuration, animator, wrongKeyFails);
				}
				else if (qteType == QTEType.ButtonMash)
				{
					KickStarter.playerQTE.StartQTE (inputName, duration, targetPresses, doCooldown, cooldownTime, animator, wrongKeyFails);
				}

				return defaultPauseTime;
			}
			else
			{
				if (KickStarter.playerQTE.GetState () == QTEState.None)
				{
					return defaultPauseTime;
				}

				if (menuName != "")
				{
					PlayerMenus.GetMenuWithName (menuName).TurnOff (true);
				}

				isRunning = false;
				return 0f;
			}
		}


		override public void Skip ()
		{
			KickStarter.playerQTE.SkipQTE ();
			if (menuName != "")
			{
				PlayerMenus.GetMenuWithName (menuName).TurnOff (true);
			}
		}
		

		override public bool CheckCondition ()
		{
			if (KickStarter.playerQTE.GetState () == QTEState.Win)
			{
				return true;
			}
			return false;
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			qteType = (QTEType) EditorGUILayout.EnumPopup ("QTE type:" , qteType);
			
			inputNameParameterID = Action.ChooseParameterGUI ("Button name:", parameters, inputNameParameterID, ParameterType.String);
			if (inputNameParameterID < 0)
			{
				inputName = EditorGUILayout.TextField ("Button name:", inputName);
			}
			wrongKeyFails = EditorGUILayout.Toggle ("Wrong key fails?", wrongKeyFails);
			
			durationParameterID = Action.ChooseParameterGUI ("Duration (s):", parameters, durationParameterID, ParameterType.Float);
			if (durationParameterID < 0)
			{
				duration = EditorGUILayout.Slider ("Duration (s):", duration, 0f, 10f);
			}
			
			if (qteType == QTEType.ButtonMash)
			{
				targetPresses = EditorGUILayout.IntField ("Target # of presses:", targetPresses);
				doCooldown = EditorGUILayout.Toggle ("Cooldown effect?", doCooldown);
				if (doCooldown)
				{
					cooldownTime = EditorGUILayout.Slider ("Cooldown time (s):", cooldownTime, 0f, duration);
				}
			}
			else if (qteType == QTEType.HoldKey)
			{
				holdDuration = EditorGUILayout.Slider ("Required duration (s):", holdDuration, 0f, duration);
			}

			menuNameParameterID = Action.ChooseParameterGUI ("Menu to display (optional):", parameters, menuNameParameterID, ParameterType.String);
			if (menuNameParameterID < 0)
			{
				menuName = EditorGUILayout.TextField ("Menu to display (optional):", menuName);
			}

			animateUI = EditorGUILayout.Toggle ("Animate UI?", animateUI);

			if (animateUI)
			{
				if (qteType == QTEType.SingleKeypress)
				{
					EditorGUILayout.HelpBox ("The Menu's Canvas must have an Animator with 2 States: Win, Lose.", MessageType.Info);
				}
				else if (qteType == QTEType.ButtonMash)
				{
					EditorGUILayout.HelpBox ("The Menu's Canvas must have an Animator with 3 States: Hit, Win, Lose.", MessageType.Info);
				}
				else if (qteType == QTEType.HoldKey)
				{
					EditorGUILayout.HelpBox ("The Menu's Canvas must have an Animator with 2 States: Win, Lose, and 1 Trigger: Held.", MessageType.Info);
				}
			}
		}
		
		
		public override string SetLabel ()
		{
			return (" (" + qteType.ToString () + " - " + inputName + ")");
		}
		
		#endif
		
	}
	
}
