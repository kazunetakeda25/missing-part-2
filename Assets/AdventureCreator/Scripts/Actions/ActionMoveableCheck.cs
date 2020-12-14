/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"ActionMoveableCheck.cs"
 * 
 *	This Action queries if a moveable object is currently being "held" by the player.
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
	public class ActionMoveableCheck : ActionCheck
	{
		
		public DragBase dragObject;
		public int constantID = 0;
		public int parameterID = -1;


		public ActionMoveableCheck ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Moveable;
			title = "Check held by player";
			description = "Queries whether or not a Draggable of PickUp object is currently being manipulated.";
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			dragObject = AssignFile <DragBase> (parameters, parameterID, constantID, dragObject);
		}
		
		
		override public bool CheckCondition ()
		{
			if (dragObject)
			{
				return (KickStarter.playerInput.IsDragObjectHeld (dragObject));
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
				dragObject = null;
			}
			else
			{
				dragObject = (DragBase) EditorGUILayout.ObjectField ("Object to check:", dragObject, typeof (DragBase), true);

				constantID = FieldToID <DragBase> (dragObject, constantID);
				dragObject = IDToField <DragBase> (dragObject, constantID, false);
			}
		}


		override public string SetLabel ()
		{
			if (dragObject)
			{
				return (" (" + dragObject.gameObject.name + ")");
			}
			return "";
		}
		
		#endif
		
	}

}