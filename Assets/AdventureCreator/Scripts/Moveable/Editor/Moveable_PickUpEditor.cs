using UnityEngine;
using UnityEditor;
using System.Collections;
using AC;

[CustomEditor(typeof(Moveable_PickUp))]
public class Moveable_PickUpEditor : DragBaseEditor
{

	public override void OnInspectorGUI ()
	{
		Moveable_PickUp _target = (Moveable_PickUp) target;
		GetReferences ();

		EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Movment settings:", EditorStyles.boldLabel);
			_target.maxSpeed = EditorGUILayout.FloatField ("Max speed:", _target.maxSpeed);
			_target.invertInput = EditorGUILayout.Toggle ("Invert input?", _target.invertInput);
			_target.breakForce = EditorGUILayout.FloatField ("Break force:", _target.breakForce);
		EditorGUILayout.EndVertical ();

		EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Rotation settings:", EditorStyles.boldLabel);
			_target.allowRotation = EditorGUILayout.Toggle ("Allow rotation?", _target.allowRotation);
			if (_target.allowRotation)
			{
				_target.rotationFactor = EditorGUILayout.FloatField ("Rotation factor:", _target.rotationFactor);
			}
		EditorGUILayout.EndVertical ();

		EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Zoom settings:", EditorStyles.boldLabel);
			_target.allowZooming = EditorGUILayout.Toggle ("Allow zooming?", _target.allowZooming);
			if (_target.allowZooming)
			{
				_target.zoomSpeed = EditorGUILayout.FloatField ("Zoom speed:", _target.zoomSpeed);
				_target.minZoom = EditorGUILayout.FloatField ("Closest distance:", _target.minZoom);
				_target.maxZoom = EditorGUILayout.FloatField ("Farthest distance:", _target.maxZoom);
			}
		EditorGUILayout.EndVertical ();

		EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Throw settings:", EditorStyles.boldLabel);
			_target.allowThrow = EditorGUILayout.Toggle ("Allow throwing?", _target.allowThrow);
			if (_target.allowThrow)
			{
				_target.throwForce = EditorGUILayout.FloatField ("Force scale:", _target.throwForce);
				_target.chargeTime = EditorGUILayout.FloatField ("Charge time:", _target.chargeTime);
				_target.pullbackDistance = EditorGUILayout.FloatField ("Pull-back distance:", _target.pullbackDistance);
			}		
		EditorGUILayout.EndVertical ();

		SharedGUI (_target, false);
	
		if (GUI.changed)
		{
			EditorUtility.SetDirty (_target);
		}
	}
}
