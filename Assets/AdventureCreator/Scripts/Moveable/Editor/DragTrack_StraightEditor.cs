using UnityEngine;
using UnityEditor;
using System.Collections;
using AC;

[CustomEditor(typeof(DragTrack_Straight))]
public class DragTrack_StraightEditor : DragTrackEditor
{
	
	public override void OnInspectorGUI ()
	{
		DragTrack_Straight _target = (DragTrack_Straight) target;
		
		EditorGUILayout.BeginVertical ("Button");
		EditorGUILayout.LabelField ("Track shape:", EditorStyles.boldLabel);
		
		_target.maxDistance = EditorGUILayout.FloatField ("Length:", _target.maxDistance);
		_target.rotationType = (DragRotationType) EditorGUILayout.EnumPopup ("Rotation type:", _target.rotationType);

		if (_target.rotationType == DragRotationType.Screw)
		{
			_target.screwThread = EditorGUILayout.FloatField ("Screw thread:", _target.screwThread);
			_target.dragMustScrew = EditorGUILayout.Toggle ("Drag must rotate too?", _target.dragMustScrew);
		}

		EditorGUILayout.EndVertical ();

		SharedGUI (true);
	}
	
	
	public void OnSceneGUI ()
	{
		DragTrack_Straight _target = (DragTrack_Straight) target;
		
		Handles.color = Color.white;
		Vector3 maxPosition = _target.transform.position + (_target.transform.up * _target.maxDistance);
		maxPosition = Handles.PositionHandle (maxPosition, Quaternion.identity);
		Handles.DrawSolidDisc (maxPosition, -_target.transform.up, _target.discSize);
		_target.maxDistance = Vector3.Dot (maxPosition - _target.transform.position, _target.transform.up);
		
		Handles.color = Color.grey;
		Vector3 minPosition = _target.transform.position;
		Handles.DrawSolidDisc (minPosition, _target.transform.up, _target.discSize);
		
		Handles.color = Color.white;
		Handles.DrawLine (minPosition, maxPosition);

		if (GUI.changed)
		{
			EditorUtility.SetDirty (_target);
		}
	}
	
}
