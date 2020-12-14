/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"ActionParent.cs"
 * 
 *	This action is used to set and clear the parent of GameObjects.
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
	public class ActionParent : Action
	{

		public int parentTransformID = 0;
		public int parentTransformParameterID = -1;
		public int obToAffectID = 0;
		public int obToAffectParameterID = -1;

		public enum ParentAction { SetParent, ClearParent };
		public ParentAction parentAction;

		public Transform parentTransform;
		
		public GameObject obToAffect;
		public bool isPlayer;
		
		public bool setPosition;
		public Vector3 newPosition;
		
		public bool setRotation;
		public Vector3 newRotation;
		

		public ActionParent ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Object;
			title = "Set parent";
			description = "Parent one GameObject to another. Can also set the child's local position and rotation.";
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			parentTransform = AssignFile (parameters, parentTransformParameterID, parentTransformID, parentTransform);
			obToAffect = AssignFile (parameters, obToAffectParameterID, obToAffectID, obToAffect);

			if (isPlayer && KickStarter.player)
			{
				obToAffect = KickStarter.player.gameObject;
			}
		}
		
		
		override public float Run ()
		{
			if (parentAction == ParentAction.SetParent && parentTransform)
			{
				obToAffect.transform.parent = parentTransform;
				
				if (setPosition)
				{
					obToAffect.transform.localPosition = newPosition;
				}
				
				if (setRotation)
				{
					obToAffect.transform.localRotation = Quaternion.LookRotation (newRotation);
				}
			}

			else if (parentAction == ParentAction.ClearParent)
			{
				obToAffect.transform.parent = null;
			}
			
			return 0f;
		}
		
		
		#if UNITY_EDITOR

		override public void ShowGUI (List<ActionParameter> parameters)
		{
			isPlayer = EditorGUILayout.Toggle ("Is Player?", isPlayer);
			if (!isPlayer)
			{
				obToAffectParameterID = Action.ChooseParameterGUI ("Object to affect:", parameters, obToAffectParameterID, ParameterType.GameObject);
				if (obToAffectParameterID >= 0)
				{
					obToAffectID = 0;
					obToAffect = null;
				}
				else
				{
					obToAffect = (GameObject) EditorGUILayout.ObjectField ("Object to affect:", obToAffect, typeof(GameObject), true);
					
					obToAffectID = FieldToID (obToAffect, obToAffectID);
					obToAffect = IDToField (obToAffect, obToAffectID, false);
				}
			}

			parentAction = (ParentAction) EditorGUILayout.EnumPopup ("Method:", parentAction);
			if (parentAction == ParentAction.SetParent)
			{
				parentTransformParameterID = Action.ChooseParameterGUI ("Parent to:", parameters, parentTransformParameterID, ParameterType.GameObject);
				if (parentTransformParameterID >= 0)
				{
					parentTransformID = 0;
					parentTransform = null;
				}
				else
				{
					parentTransform = (Transform) EditorGUILayout.ObjectField ("Parent to:", parentTransform, typeof(Transform), true);
					
					parentTransformID = FieldToID (parentTransform, parentTransformID);
					parentTransform = IDToField (parentTransform, parentTransformID, false);
				}
			
				setPosition = EditorGUILayout.Toggle ("Set local position?", setPosition);
				if (setPosition)
				{
					newPosition = EditorGUILayout.Vector3Field ("Position vector:", newPosition);
				}
				
				setRotation = EditorGUILayout.Toggle ("Set local rotation?", setRotation);
				if (setRotation)
				{
					newRotation = EditorGUILayout.Vector3Field ("Rotation vector:", newRotation);
				}
			}
			
			AfterRunningOption ();
		}
		
		
		override public string SetLabel ()
		{
			string labelAdd = "";
			
			if (obToAffect)
			{
				labelAdd = " (" + obToAffect.name + ")";
			}
			
			return labelAdd;
		}

		#endif

	}

}