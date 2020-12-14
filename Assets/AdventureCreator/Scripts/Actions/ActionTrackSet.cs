/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"ActionMoveableTrack.cs"
 * 
 *	This action is used to automatically move
 *	a draggable object along a track, provided
 *	it's already on one.
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
	public class ActionTrackSet : Action
	{
		
		public Moveable_Drag dragObject;
		public int dragParameterID = -1;
		public int dragConstantID = 0;

		public float positionAlong;
		public int positionParameterID = -1;

		public float speed = 200f;
		public bool removePlayerControl = false;
		public bool isInstant;

		
		public ActionTrackSet ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Moveable;
			title = "Set track position";
			description = "Moves a Draggable object along it's track automatically to a specific point. The effect will be disabled once the object reaches the intended point, or the Action is run again with the speed value set as a negative number.";
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			dragObject = AssignFile <Moveable_Drag> (parameters, dragParameterID, dragConstantID, dragObject);

			positionAlong = AssignFloat (parameters, positionParameterID, positionAlong);
			positionAlong = Mathf.Max (0f, positionAlong);
			positionAlong = Mathf.Min (1f, positionAlong);
		}

		
		override public float Run ()
		{
			if (dragObject == null)
			{
				isRunning = false;
				return 0f;
			}

			if (!isRunning)
			{
				isRunning = true;

				if (isInstant)
				{
					dragObject.AutoMoveAlongTrack (positionAlong, 0f, removePlayerControl);
				}
				else
				{
					dragObject.AutoMoveAlongTrack (positionAlong, speed, removePlayerControl);
				}

				if (willWait && !isInstant && speed > 0f)
				{
					return defaultPauseTime;
				}
				isRunning = false;
				return 0f;
			}
			else
			{
				if (dragObject.IsAutoMoving ())
				{
					return defaultPauseTime;
				}
				isRunning = false;
				return 0f;
			}
		}


		override public void Skip ()
		{
			if (dragObject == null)
			{
				return;
			}
			
			dragObject.AutoMoveAlongTrack (positionAlong, 0f, removePlayerControl);
		}
		
		
		#if UNITY_EDITOR

		override public void ShowGUI (List<ActionParameter> parameters)
		{
			dragParameterID = Action.ChooseParameterGUI ("Moveable object:", parameters, dragParameterID, ParameterType.GameObject);
			if (dragParameterID >= 0)
			{
				dragConstantID = 0;
				dragObject = null;
			}
			else
			{
				dragObject = (Moveable_Drag) EditorGUILayout.ObjectField ("Moveable object:", dragObject, typeof (Moveable_Drag), true);
				
				dragConstantID = FieldToID <Moveable_Drag> (dragObject, dragConstantID);
				dragObject = IDToField <Moveable_Drag> (dragObject, dragConstantID, false);

				if (dragObject != null && dragObject.dragMode != DragMode.LockToTrack)
				{
					EditorGUILayout.HelpBox ("The chosen Drag object must be in 'Lock To Track' mode", MessageType.Warning);
				}
			}

			positionParameterID = Action.ChooseParameterGUI ("New track position:", parameters, positionParameterID, ParameterType.Float);
			if (positionParameterID < 0)
			{
				positionAlong = EditorGUILayout.Slider ("New track position:", positionAlong, 0f, 1f);
			}

			isInstant = EditorGUILayout.Toggle ("Is instant?", isInstant);
			if (!isInstant)
			{
				speed = EditorGUILayout.FloatField ("Movement speed:", speed);
				removePlayerControl = EditorGUILayout.Toggle ("Remove player control?", removePlayerControl);
				willWait = EditorGUILayout.Toggle ("Wait until finish?", willWait);
			}
			
			AfterRunningOption ();
		}
		

		public override string SetLabel ()
		{
			if (dragObject)
			{
				return (" (" + dragObject.name + " to " + positionAlong);
			}
			return "";
		}

		#endif
		
	}

}