using UnityEngine;
using System.Collections;
using UnityEditor;
using AC;

[CustomEditor (typeof (AlignToCamera))]
public class AlignToCameraEditor : Editor
{
	
	private AlignToCamera _target;
	
	
	private void OnEnable ()
	{
		_target = (AlignToCamera) target;
	}
	
	
	public override void OnInspectorGUI ()
	{
		_target.cameraToAlignTo = (_Camera) EditorGUILayout.ObjectField ("Camera to align to:", _target.cameraToAlignTo, typeof (_Camera), true);

		if (_target.cameraToAlignTo)
		{
			_target.distanceToCamera = EditorGUILayout.FloatField ("Distance from camera:", _target.distanceToCamera);
			_target.lockScale = EditorGUILayout.Toggle ("Lock scale?", _target.lockScale);

			if (GUILayout.Button ("Centre to camera"))
			{
				Undo.RecordObject (_target, "Centre to camera");
				_target.CentreToCamera ();
			}
		}

		if (GUI.changed)
		{
			EditorUtility.SetDirty (_target);
		}
	}

}
