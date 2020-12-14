using UnityEngine;
using UnityEditor;
using System.Collections;
using AC;

[CustomEditor(typeof(MainCamera))]

public class MainCameraEditor : Editor
{
	
	public override void OnInspectorGUI()
	{
		MainCamera _target = (MainCamera) target;

		EditorGUILayout.BeginVertical ("Button");
		_target.fadeTexture = (Texture2D) EditorGUILayout.ObjectField ("Fade texture:", _target.fadeTexture, typeof (Texture2D), false);
		_target.lookAtTransform = (Transform) EditorGUILayout.ObjectField ("LookAt child:", _target.lookAtTransform, typeof (Transform), true);
		EditorGUILayout.EndVertical ();

		if (Application.isPlaying)
		{
			EditorGUILayout.BeginVertical ("Button");
			if (_target.attachedCamera)
			{
				_target.attachedCamera = (_Camera) EditorGUILayout.ObjectField ("Attached camera:", _target.attachedCamera, typeof (_Camera), true);
			}
			else
			{
				EditorGUILayout.LabelField ("Attached camera: None");
			}
			EditorGUILayout.EndVertical ();
		}

		if (GUI.changed)
		{
			EditorUtility.SetDirty (_target);
		}
	}

}
