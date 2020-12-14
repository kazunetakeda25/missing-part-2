using UnityEngine;
using UnityEditor;
using System.Collections;
using AC;

[CustomEditor (typeof (FollowSortingMap))]

public class FollowSortingMapEditor : Editor
{
	
	public override void OnInspectorGUI ()
	{
		FollowSortingMap _target = (FollowSortingMap) target;

		_target.followSortingMap = EditorGUILayout.Toggle ("Follow Sorting map?", _target.followSortingMap);

		if (_target.followSortingMap)
		{
			_target.offsetOriginal = EditorGUILayout.Toggle ("Offset original Order?", _target.offsetOriginal);
			_target.affectChildren = EditorGUILayout.Toggle ("Also affect children?", _target.affectChildren);
			_target.livePreview = EditorGUILayout.Toggle ("Edit-mode preview?", _target.livePreview);
		}

		if (GUI.changed)
		{
			EditorUtility.SetDirty (_target);
		}
	}
}
