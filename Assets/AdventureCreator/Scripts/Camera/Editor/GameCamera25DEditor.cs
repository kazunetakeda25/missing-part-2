using UnityEngine;
using UnityEditor;
using System.Collections;
using AC;

[CustomEditor (typeof (GameCamera25D))]

public class GameCamera25DEditor : Editor
{

	public override void OnInspectorGUI ()
	{
		GameCamera25D _target = (GameCamera25D) target;
		
		EditorGUILayout.BeginVertical ("Button");
		EditorGUILayout.LabelField ("Background image", EditorStyles.boldLabel);
	
		EditorGUILayout.BeginHorizontal ();
		_target.backgroundImage = (BackgroundImage) EditorGUILayout.ObjectField ("Prefab:", _target.backgroundImage, typeof (BackgroundImage), true);
		
		if (_target.backgroundImage)
		{
			if (GUILayout.Button ("Set as active", GUILayout.MaxWidth (90f)))
			{
				Undo.RecordObject (_target, "Set active background");
				
				_target.SetActiveBackground ();
				_target.SnapCameraInEditor ();
			}
		}
		else
		{
			if (GUILayout.Button ("Create", GUILayout.MaxWidth (90f)))
			{
				Undo.RecordObject (_target, "Create Background Image");
				BackgroundImage newBackgroundImage = SceneManager.AddPrefab ("SetGeometry", "BackgroundImage", true, false, true).GetComponent <BackgroundImage>();
				
				string cameraName = _target.gameObject.name;

				newBackgroundImage.gameObject.name = AdvGame.UniqueName (cameraName + ": Background");
				_target.backgroundImage = newBackgroundImage;
			}
		}
		
		EditorGUILayout.EndHorizontal ();
		EditorGUILayout.EndVertical ();

		if (_target.isActiveEditor)
		{
			_target.UpdateCameraSnap ();
		}
		
		if (GUI.changed)
		{
			EditorUtility.SetDirty (_target);
		}
	}
	
}
