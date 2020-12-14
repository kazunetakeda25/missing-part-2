/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"ActionObjectCheck.cs"
 * 
 *	This action checks if an object is
 *	in the scene.
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
	public class ActionObjectCheck : ActionCheck
	{


		public GameObject gameObject;
		public int parameterID = -1;
		public int constantID = 0; 


		public ActionObjectCheck ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Object;
			title = "Check presence";
			description = "Use to determine if a particular GameObject or prefab is present in the current scene.";
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			gameObject = AssignFile (parameters, parameterID, constantID, gameObject);
		}
		
		
		override public bool CheckCondition ()
		{
			if (gameObject != null && gameObject.activeInHierarchy)
			{
				return true;
			}
			return false;
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			parameterID = Action.ChooseParameterGUI ("Object to check:", parameters, parameterID, ParameterType.GameObject);
			if (parameterID >= 0)
			{
				constantID = 0;
				gameObject = null;
			}
			else
			{
				gameObject = (GameObject) EditorGUILayout.ObjectField ("Object to check:", gameObject, typeof (GameObject), true);
				
				constantID = FieldToID (gameObject, constantID);
				gameObject = IDToField (gameObject, constantID, false);
			}
		}
		
		#endif
		
	}

}