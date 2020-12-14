using UnityEngine;
using UnityEditor;
using System.Collections;
using AC;

[CustomEditor (typeof (RememberCollider), true)]
public class RememberColliderEditor : ConstantIDEditor
{
	
	public override void OnInspectorGUI()
	{
		RememberCollider _target = (RememberCollider) target;

		EditorGUILayout.BeginVertical ("Button");
		EditorGUILayout.LabelField ("Hotspot", EditorStyles.boldLabel);
		_target.startState = (AC_OnOff) EditorGUILayout.EnumPopup ("Collider state on start:", _target.startState);
		EditorGUILayout.EndVertical ();

		if (_target.GetComponent <Collider>() == null)
		{
			EditorGUILayout.HelpBox ("This script expects a Collider component!", MessageType.Warning);
		}
		
		SharedGUI ();
	}
	
}
