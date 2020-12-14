using UnityEngine;
using System.Collections;
using UnityEditor;
using AC;

[CustomEditor (typeof (Parallax2D))]
public class Parallax2DEditor : Editor
{

	private Parallax2D _target;


	private void OnEnable ()
	{
		_target = (Parallax2D) target;
	}


	public override void OnInspectorGUI ()
	{
		EditorGUILayout.BeginVertical ("Button");
			_target.depth = EditorGUILayout.FloatField ("Depth:", _target.depth);
		EditorGUILayout.EndVertical ();

		EditorGUILayout.BeginVertical ("Button");
			_target.xScroll = EditorGUILayout.BeginToggleGroup ("Scroll in X direction?", _target.xScroll);
			_target.xOffset = EditorGUILayout.FloatField ("Offset:", _target.xOffset);
			EditorGUILayout.EndToggleGroup ();
		EditorGUILayout.EndVertical ();

		EditorGUILayout.BeginVertical ("Button");
			_target.yScroll = EditorGUILayout.BeginToggleGroup ("Scroll in Y direction?", _target.yScroll);
			_target.yOffset = EditorGUILayout.FloatField ("Offset:", _target.yOffset);
			EditorGUILayout.EndToggleGroup ();
		EditorGUILayout.EndVertical ();

		if (GUI.changed)
		{
			EditorUtility.SetDirty (_target);
		}
	}
}
