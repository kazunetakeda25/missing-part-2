using UnityEngine;
using UnityEditor;
using System.Collections;
using AC;

[CustomEditor(typeof(DragBase))]
public class DragBaseEditor : Editor
{

	protected CursorManager cursorManager;


	protected void GetReferences ()
	{
		if (AdvGame.GetReferences ().cursorManager)
		{
			cursorManager = AdvGame.GetReferences ().cursorManager;
		}
	}


	protected void SharedGUI (DragBase _target, bool isOnHinge)
	{
		EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Collision settings:", EditorStyles.boldLabel);
			_target.ignorePlayerCollider = EditorGUILayout.ToggleLeft ("Ignore Player's collider?", _target.ignorePlayerCollider);
			_target.ignoreMoveableRigidbodies = EditorGUILayout.ToggleLeft ("Ignore Moveable Rigidbodies?", _target.ignoreMoveableRigidbodies);
			_target.childrenShareLayer = EditorGUILayout.ToggleLeft ("Place children on same layer?", _target.childrenShareLayer);
		EditorGUILayout.EndVertical ();

		EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Icon settings:", EditorStyles.boldLabel);
			_target.showIcon = EditorGUILayout.Toggle ("Icon at contact point?", _target.showIcon);
			if (_target.showIcon)
			{
				if (cursorManager && cursorManager.cursorIcons.Count > 0)
				{
					int cursorInt = cursorManager.GetIntFromID (_target.iconID);
					cursorInt = EditorGUILayout.Popup ("Cursor icon:", cursorInt, cursorManager.GetLabelsArray (cursorInt));
					_target.iconID = cursorManager.cursorIcons [cursorInt].id;
				}
				else
				{
					_target.iconID = -1;
				}
			}		
		EditorGUILayout.EndVertical ();

		EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Sound settings:", EditorStyles.boldLabel);
			_target.moveSoundClip = (AudioClip) EditorGUILayout.ObjectField ("Move sound:", _target.moveSoundClip, typeof (AudioClip), false);
			_target.slideSoundThreshold = EditorGUILayout.FloatField ("Min. move speed:", _target.slideSoundThreshold);
			_target.slidePitchFactor = EditorGUILayout.FloatField ("Pitch factor:", _target.slidePitchFactor);
			_target.collideSoundClip = (AudioClip) EditorGUILayout.ObjectField ("Collide sound:", _target.collideSoundClip, typeof (AudioClip), false);
			if (isOnHinge)
			{
				_target.onlyPlayLowerCollisionSound = EditorGUILayout.Toggle ("Only on lower boundary?", _target.onlyPlayLowerCollisionSound);
			}
		EditorGUILayout.EndVertical ();
	}

}
