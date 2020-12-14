using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof (FirstPersonCamera))]

public class FirstPersonCameraEditor : Editor
{
	
	private static GUILayoutOption
		labelWidth = GUILayout.MaxWidth (60),
		intWidth = GUILayout.MaxWidth (130);
	
	
	public override void OnInspectorGUI ()
	{
		FirstPersonCamera _target = (FirstPersonCamera) target;
		
		EditorGUILayout.BeginVertical ("Button");
			_target.headBob = EditorGUILayout.BeginToggleGroup ("Bob head when moving?", _target.headBob);
				EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("Speed:", labelWidth);
					_target.bobbingSpeed = EditorGUILayout.FloatField (_target.bobbingSpeed, intWidth);
					EditorGUILayout.LabelField ("Amount:", labelWidth);
					_target.bobbingAmount = EditorGUILayout.FloatField (_target.bobbingAmount, intWidth);
				EditorGUILayout.EndHorizontal ();
			EditorGUILayout.EndToggleGroup ();
		EditorGUILayout.EndVertical ();
		
		EditorGUILayout.BeginVertical ("Button");
			_target.allowMouseWheelZooming = EditorGUILayout.BeginToggleGroup ("Allow mouse-wheel zooming?", _target.allowMouseWheelZooming);
				EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("Min FOV:", labelWidth);
					_target.minimumZoom = EditorGUILayout.FloatField (_target.minimumZoom, intWidth);
					EditorGUILayout.LabelField ("Max FOV:", labelWidth);
					_target.maximumZoom = EditorGUILayout.FloatField (_target.maximumZoom, intWidth);
				EditorGUILayout.EndHorizontal ();
			EditorGUILayout.EndToggleGroup ();
		EditorGUILayout.EndVertical ();
		
		EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Constrain Y-rotation (degrees)");
			EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Min:", labelWidth);
				_target.minY = EditorGUILayout.FloatField (_target.minY, intWidth);
				EditorGUILayout.LabelField ("Max:", labelWidth);
				_target.maxY = EditorGUILayout.FloatField (_target.maxY, intWidth);
			EditorGUILayout.EndHorizontal ();
		EditorGUILayout.EndVertical ();
		
		EditorGUILayout.BeginVertical ("Button");
			_target.sensitivity = EditorGUILayout.Vector2Field ("Freelook sensitivity:", _target.sensitivity);
		EditorGUILayout.EndVertical ();

		if (GUI.changed)
		{
			EditorUtility.SetDirty (_target);
		}
	}
	
}
