/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"ActionVisible.cs"
 * 
 *	This action controls the visibilty of a GameObject and it's children.
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
	public class ActionVisible : Action
	{

		public int parameterID = -1;
		public int constantID = 0;
		public enum VisState { Visible, Invisible };
		public GameObject obToAffect;
		public bool affectChildren;
		public VisState visState = 0;
		
		
		public ActionVisible ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Object;
			title = "Visibility";
			description = "Hides or shows a GameObject. Can optionally affect the GameObject's children.";
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			obToAffect = AssignFile (parameters, parameterID, constantID, obToAffect);
		}
		
		
		override public float Run ()
		{
			bool state = false;
			if (visState == VisState.Visible)
			{
				state = true;
			}
			
			if (obToAffect)
			{
				if (obToAffect.GetComponent <Renderer>())
				{
					obToAffect.GetComponent <Renderer>().enabled = state;
				}

				if (affectChildren)
				{
					foreach (Renderer _renderer in obToAffect.GetComponentsInChildren <Renderer>())
					{
						_renderer.enabled = state;
					}
				}
					
			}
			
			return 0f;
		}
		
		
		#if UNITY_EDITOR

		override public void ShowGUI (List<ActionParameter> parameters)
		{
			parameterID = Action.ChooseParameterGUI ("Object to affect:", parameters, parameterID, ParameterType.GameObject);
			if (parameterID >= 0)
			{
				constantID = 0;
				obToAffect = null;
			}
			else
			{
				obToAffect = (GameObject) EditorGUILayout.ObjectField ("Object to affect:", obToAffect, typeof (GameObject), true);

				constantID = FieldToID (obToAffect, constantID);
				obToAffect = IDToField (obToAffect, constantID, false);
			}

			visState = (VisState) EditorGUILayout.EnumPopup ("Visibility:", visState);
			affectChildren = EditorGUILayout.Toggle ("Affect children?", affectChildren);
			
			AfterRunningOption ();
		}
		
		
		override public string SetLabel ()
		{
			string labelAdd = "";
			
			if (obToAffect)
					labelAdd = " (" + obToAffect.name + ")";
			
			return labelAdd;
		}

		#endif

	}

}