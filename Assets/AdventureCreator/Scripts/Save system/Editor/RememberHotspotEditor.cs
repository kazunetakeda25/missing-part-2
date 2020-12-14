using UnityEngine;
using UnityEditor;
using System.Collections;
using AC;

[CustomEditor (typeof (RememberHotspot), true)]
public class RememberHotspotEditor : ConstantIDEditor
{
	
	public override void OnInspectorGUI()
	{
		RememberHotspot _target = (RememberHotspot) target;

		EditorGUILayout.BeginVertical ("Button");
		EditorGUILayout.LabelField ("Hotspot", EditorStyles.boldLabel);
		_target.startState = (AC_OnOff) EditorGUILayout.EnumPopup ("Hotspot state on start:", _target.startState);
		EditorGUILayout.EndVertical ();

		if (_target.GetComponent <Hotspot>() == null)
		{
			EditorGUILayout.HelpBox ("This script expects a Hotspot component!", MessageType.Warning);
		}

		SharedGUI ();
	}

}
