using UnityEngine;
using UnityEditor;
using System.Collections;
using AC;

[CustomEditor(typeof(DragTrack))]
public class DragTrackEditor : Editor
{

	protected void SharedGUI (bool useColliders)
	{
		DragTrack _target = (DragTrack) target;

		EditorGUILayout.BeginVertical ("Button");
		EditorGUILayout.LabelField ("End-colliders", EditorStyles.boldLabel);
		
		_target.discSize = EditorGUILayout.Slider ("Gizmo size:", _target.discSize, 0f, 2f);
		if (useColliders)
		{
			_target.colliderMaterial = (PhysicMaterial) EditorGUILayout.ObjectField ("Material:", _target.colliderMaterial, typeof (PhysicMaterial), false);
		}
		
		EditorGUILayout.EndVertical ();
		
		if (GUI.changed)
		{
			EditorUtility.SetDirty (_target);
		}
	}

}
