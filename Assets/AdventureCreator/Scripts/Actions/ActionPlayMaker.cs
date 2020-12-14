/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"ActionPlayMaker.cs"
 * 
 *	This action interacts with the popular
 *	PlayMaker FSM-manager.
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
	public class ActionPlayMaker : Action
	{

		public bool isPlayer;

		public int constantID = 0;
		public int parameterID = -1;
		public GameObject linkedObject;

		public string fsmName;
		public string eventName;


		public ActionPlayMaker ()
		{
			this.isDisplayed = true;
			category = ActionCategory.ThirdParty;
			title = "PlayMaker";
			description = "Calls a specified Event within a PlayMaker FSM. Note that PlayMaker is a separate Unity Asset, and the 'PlayMakerIsPresent' preprocessor must be defined for this to work.";
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			if (isPlayer)
			{
				linkedObject = GameObject.FindWithTag (Tags.player);
			}
			else
			{
				linkedObject = AssignFile (parameters, parameterID, constantID, linkedObject);
			}
		}


		override public float Run ()
		{
			if (linkedObject != null && eventName != "")
			{
				if (fsmName != "")
				{
					PlayMakerIntegration.CallEvent (linkedObject, eventName, fsmName);
				}
				else
				{
					PlayMakerIntegration.CallEvent (linkedObject, eventName);
				}
			}

			return 0f;
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			if (PlayMakerIntegration.IsDefinePresent ())
			{
				isPlayer = EditorGUILayout.Toggle ("Use Player's FSM?", isPlayer);
				if (!isPlayer)
				{
					parameterID = Action.ChooseParameterGUI ("PlayMaker FSM:", parameters, parameterID, ParameterType.GameObject);
					if (parameterID >= 0)
					{
						constantID = 0;
						linkedObject = null;
					}
					else
					{
						linkedObject = (GameObject) EditorGUILayout.ObjectField ("PlayMaker FSM:", linkedObject, typeof (GameObject), true);
						
						constantID = FieldToID (linkedObject, constantID);
						linkedObject = IDToField (linkedObject, constantID, false);
					}
				}

				fsmName = EditorGUILayout.TextField ("FSM to call (optional):", fsmName);
				eventName = EditorGUILayout.TextField ("Event to call:", eventName);
			}
			else
			{
				EditorGUILayout.HelpBox ("The 'PlayMakerIsPresent' Scripting Define Symbol must be listed in the\nPlayer Settings. Please set it from Edit -> Project Settings -> Player", MessageType.Warning);
			}

			AfterRunningOption ();
		}
		
		
		public override string SetLabel ()
		{
			string labelAdd = "";
			return labelAdd;
		}
		
		#endif
	}

}