using UnityEngine;
using UnityEditor;
using System.Collections;
using AC;

[CustomEditor (typeof (RememberMoveable), true)]
public class RememberMoveableEditor : ConstantIDEditor
{
	
	public override void OnInspectorGUI ()
	{
		RememberMoveable _target = (RememberMoveable) target;
		
		EditorGUILayout.BeginVertical ("Button");
		EditorGUILayout.LabelField ("Moveable", EditorStyles.boldLabel);
		_target.startState = (AC_OnOff) EditorGUILayout.EnumPopup ("Moveable state on start:", _target.startState);
		EditorGUILayout.EndVertical ();
		
		if (_target.GetComponent <Moveable>() == null)
		{
			EditorGUILayout.HelpBox ("This script expects a Moveable component!", MessageType.Warning);
		}
		
		SharedGUI ();
	}
	
}
