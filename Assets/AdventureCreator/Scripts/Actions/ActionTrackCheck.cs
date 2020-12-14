/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"ActionMoveableCheck.cs"
 * 
 *	This action checks the position of a Drag object
 *	along a locked track
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
	public class ActionTrackCheck : ActionCheck
	{

		public Moveable_Drag dragObject;
		public int dragConstantID = 0;
		public int dragParameterID = -1;

		public float checkPosition;
		public int checkPositionParameterID = -1;

		public float errorMargin = 0.05f;
		public IntCondition condition;

		
		public ActionTrackCheck ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Moveable;
			title = "Check track position";
			description = "Queries how far a Draggable object is along it's track.";
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			dragObject = AssignFile <Moveable_Drag> (parameters, dragParameterID, dragConstantID, dragObject);
			
			checkPosition = AssignFloat (parameters, checkPositionParameterID, checkPosition);
			checkPosition = Mathf.Max (0f, checkPosition);
			checkPosition = Mathf.Min (1f, checkPosition);
		}

			
		override public ActionEnd End (List<AC.Action> actions)
		{
			return ProcessResult (CheckCondition (), actions);
		}
		
		
		override public bool CheckCondition ()
		{
			float actualPositionAlong = dragObject.GetPositionAlong ();

			if (condition == IntCondition.EqualTo)
			{
				if (actualPositionAlong > (checkPosition - errorMargin) || actualPositionAlong < (checkPosition + errorMargin))
				{
					return true;
				}
			}
			else if (condition == IntCondition.NotEqualTo)
			{
				if (actualPositionAlong < (checkPosition - errorMargin) || actualPositionAlong > (checkPosition + errorMargin))
				{
					return true;
				}
			}
			else if (condition == IntCondition.LessThan)
			{
				if (actualPositionAlong < checkPosition)
				{
					return true;
				}
			}
			else if (condition == IntCondition.MoreThan)
			{
				if (actualPositionAlong > checkPosition)
				{
					return true;
				}
			}

			return false;
		}

		
		#if UNITY_EDITOR

		override public void ShowGUI (List<ActionParameter> parameters)
		{
			dragParameterID = Action.ChooseParameterGUI ("Drag object:", parameters, dragParameterID, ParameterType.GameObject);
			if (dragParameterID >= 0)
			{
				dragConstantID = 0;
				dragObject = null;
			}
			else
			{
				dragObject = (Moveable_Drag) EditorGUILayout.ObjectField ("Drag object:", dragObject, typeof (Moveable_Drag), true);
				
				dragConstantID = FieldToID <Moveable_Drag> (dragObject, dragConstantID);
				dragObject = IDToField <Moveable_Drag> (dragObject, dragConstantID, false);
				
				if (dragObject != null && dragObject.dragMode != DragMode.LockToTrack)
				{
					EditorGUILayout.HelpBox ("The chosen Drag object must be in 'Lock To Track' mode", MessageType.Warning);
				}
			}

			condition = (IntCondition) EditorGUILayout.EnumPopup ("Condition:", condition);

			checkPositionParameterID = Action.ChooseParameterGUI ("Position:", parameters, checkPositionParameterID, ParameterType.Float);
			if (checkPositionParameterID < 0)
			{
				checkPosition = EditorGUILayout.Slider ("Position:", checkPosition, 0f, 1f);
			}


			/*EditorGUILayout.BeginHorizontal ();
			checkPosition = EditorGUILayout.Slider (checkPosition, 0f, 1f);
			EditorGUILayout.EndHorizontal ();*/

			if (condition == IntCondition.EqualTo || condition == IntCondition.NotEqualTo)
			{
				errorMargin = EditorGUILayout.Slider ("Error margin:", errorMargin, 0f, 1f);
			}
		}


		override public string SetLabel ()
		{
			if (dragObject != null)
			{
				return (dragObject.gameObject.name + " " + condition.ToString () + " " + checkPosition);
			}
			return "";
		}

		#endif

	}

}