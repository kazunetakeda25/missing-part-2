using UnityEngine;
using System.Collections;
using UnityEditor;
using AC;


[CustomEditor (typeof (MenuLink))]
public class MenuLinkEditor : Editor
{

	private MenuLink _target;
	
	
	private void OnEnable ()
	{
		_target = (MenuLink) target;
	}

	public override void OnInspectorGUI ()
	{
		if (_target == null)
		{
			return;
		}

		if (Application.isPlaying)
		{
			EditorGUILayout.LabelField ("Menu name:", _target.menuName);
			EditorGUILayout.LabelField ("Element name:",_target.elementName);
			EditorGUILayout.LabelField ("Slot number:", _target.slot.ToString ());
			EditorGUILayout.LabelField ("Is visible?", _target.IsVisible ().ToString ());

			if (GUILayout.Button ("Interact"))
			{
				_target.Interact ();
			}
		}
		else
		{
			_target.menuName = EditorGUILayout.TextField ("Menu name:", _target.menuName);
			_target.elementName = EditorGUILayout.TextField ("Element name:", _target.elementName);
			_target.slot = EditorGUILayout.IntField ("Slot number (optional):", _target.slot);
		}

		_target.setTextLabels = EditorGUILayout.Toggle ("Set guiText / TextMesh labels?", _target.setTextLabels);

		if (GUI.changed)
		{
			EditorUtility.SetDirty (_target);
		}
	}

}
