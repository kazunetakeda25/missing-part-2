/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"ActionPause.cs"
 * 
 *	This action pauses the game by a given amount.
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
	public class ActionPause : Action
	{

		public int parameterID = -1;
		public float timeToPause;

		
		public ActionPause ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Engine;
			title = "Pause game";
			description = "Waits a set time before continuing.";
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			timeToPause = AssignFloat (parameters, parameterID, timeToPause);
			timeToPause = Mathf.Max (0f, timeToPause);
		}

		
		override public float Run ()
		{
			if (!isRunning)
			{
				isRunning = true;
				return timeToPause;
			}
			else
			{
				isRunning = false;
				return 0f;
			}
		}


		#if UNITY_EDITOR

		override public void ShowGUI (List<ActionParameter> parameters)
		{
			parameterID = Action.ChooseParameterGUI ("Wait time (s):", parameters, parameterID, ParameterType.Float);
			if (parameterID < 0)
			{
				timeToPause = EditorGUILayout.Slider ("Wait time (s):", timeToPause, 0, 10f);
			}
			AfterRunningOption ();
		}
		

		public override string SetLabel ()
		{
			string labelAdd = " (" + timeToPause + "s)";
			return labelAdd;
		}

		#endif
		
	}

}