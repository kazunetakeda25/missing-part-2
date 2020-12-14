using UnityEngine;
using UnityEditor;
using System.Collections;
using AC;

[CustomEditor (typeof (RememberNPC), true)]
public class RememberNPCEditor : ConstantIDEditor
{
	
	public override void OnInspectorGUI()
	{
		RememberNPC _target = (RememberNPC) target;
		
		EditorGUILayout.BeginVertical ("Button");
		EditorGUILayout.LabelField ("NPC", EditorStyles.boldLabel);
		_target.startState = (AC_OnOff) EditorGUILayout.EnumPopup ("Hotspot state on start:", _target.startState);
		EditorGUILayout.EndVertical ();

		if (_target.GetComponent <NPC>() == null)
		{
			EditorGUILayout.HelpBox ("This script expects an NPC component!", MessageType.Warning);
		}


		SharedGUI ();
	}
	
}
