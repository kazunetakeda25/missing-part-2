using UnityEngine;
using UnityEditor;
using System.Collections;
using AC;

[CustomEditor(typeof(DragTrack_Curved))]
public class DragTrack_CurvedEditor : DragTrackEditor
{
	
	public override void OnInspectorGUI ()
	{
		DragTrack_Curved _target = (DragTrack_Curved) target;

		EditorGUILayout.BeginVertical ("Button");
		EditorGUILayout.LabelField ("Track shape:", EditorStyles.boldLabel);
		
		_target.radius = EditorGUILayout.FloatField ("Radius:", _target.radius);

		_target.doLoop = EditorGUILayout.Toggle ("Is looped?", _target.doLoop);
		if (!_target.doLoop)
		{
			_target.maxAngle = EditorGUILayout.FloatField ("Maximum angle:", _target.maxAngle);

			if (_target.maxAngle > 360f)
			{
				_target.maxAngle = 360f;
			}
		}

		EditorGUILayout.EndVertical ();
		
		SharedGUI (true);
	}
	
	
	public void OnSceneGUI ()
	{
		DragTrack_Curved _target = (DragTrack_Curved) target;

		float _angle = _target.maxAngle;
		if (_target.doLoop)
		{
			_angle = 360f;
		}

		Handles.color = Color.gray;
		Vector3 startPosition = _target.transform.position + (_target.radius * _target.transform.right);
		Handles.DrawSolidDisc (startPosition, _target.transform.up, _target.discSize);

		Transform t = _target.transform;
		Vector3 originalPosition = _target.transform.position;
		Quaternion originalRotation = _target.transform.rotation;
		t.position = startPosition;
		t.RotateAround (originalPosition, _target.transform.forward, _angle);

		Handles.color = Color.white;
		Handles.DrawSolidDisc (t.position, t.up, _target.discSize);

		_target.transform.position = originalPosition;
		_target.transform.rotation = originalRotation;

		Handles.color = Color.white;
		Handles.DrawWireArc (_target.transform.position, _target.transform.forward, _target.transform.right, _angle, _target.radius);
	}
	
}
