/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"ActionInputCheck.cs"
 * 
 *	This action checks if a specific key
 *	is being pressed
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
	public class ActionInputCheck : ActionCheck
	{
		
		public string inputName;
		public int parameterID = -1;

		public InputCheckType checkType = InputCheckType.Button;

		public IntCondition axisCondition;
		public float axisValue;

		
		public ActionInputCheck ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Input;
			title = "Check";
			description = "Queries whether or not the player is carrying an item. If the player can carry multiple amounts of the item, more options will show.";
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			inputName = AssignString (parameters, parameterID, inputName);
		}


		override public float Run ()
		{
			if (!isRunning)
			{
				isRunning = true;
				return defaultPauseTime;
			}
			else
			{
				isRunning = false;
				return 0f;
			}
		}


		override public bool CheckCondition ()
		{
			if (inputName != "")
			{
				if (checkType == InputCheckType.Button && KickStarter.playerInput.InputGetButton (inputName))
				{
					return true;
				}
				else if (checkType == InputCheckType.Axis)
				{
					return CheckAxisValue (KickStarter.playerInput.InputGetAxis (inputName));
				}
			}
			return false;
		}


		private bool CheckAxisValue (float fieldValue)
		{
			if (axisCondition == IntCondition.EqualTo)
			{
				if (fieldValue == axisValue)
				{
					return true;
				}
			}
			else if (axisCondition == IntCondition.NotEqualTo)
			{
				if (fieldValue != axisValue)
				{
					return true;
				}
			}
			else if (axisCondition == IntCondition.LessThan)
			{
				if (fieldValue < axisValue)
				{
					return true;
				}
			}
			else if (axisCondition == IntCondition.MoreThan)
			{
				if (fieldValue > axisValue)
				{
					return true;
				}
			}

			return false;
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			checkType = (InputCheckType) EditorGUILayout.EnumPopup ("Check type:" , checkType);

			parameterID = Action.ChooseParameterGUI (checkType.ToString () + " name:", parameters, parameterID, ParameterType.String);
			if (parameterID < 0)
			{
				inputName = EditorGUILayout.TextField (checkType.ToString () + " name:", inputName);
			}

			if (checkType == InputCheckType.Axis)
			{
				EditorGUILayout.BeginHorizontal ();
					axisCondition = (IntCondition) EditorGUILayout.EnumPopup (axisCondition);
					axisValue = EditorGUILayout.FloatField (axisValue);
				EditorGUILayout.EndHorizontal ();
			}
		}


		public override string SetLabel ()
		{
			return (" (" + inputName + ")");
		}
		
		#endif
		
	}

}