using UnityEngine;
using UnityEditor;
using System.Collections;
using AC;

[CustomEditor (typeof (RememberVisibility), true)]
public class RememberVisibilityEditor : ConstantIDEditor
{
	
	public override void OnInspectorGUI()
	{
		RememberVisibility _target = (RememberVisibility) target;

		EditorGUILayout.BeginVertical ("Button");
		EditorGUILayout.LabelField ("Visibility", EditorStyles.boldLabel);
		_target.startState = (AC_OnOff) EditorGUILayout.EnumPopup ("Visibility on start:", _target.startState);
		_target.affectChildren = EditorGUILayout.Toggle ("Affect children?", _target.affectChildren);
		EditorGUILayout.EndVertical ();

		SharedGUI ();
	}
	
}
