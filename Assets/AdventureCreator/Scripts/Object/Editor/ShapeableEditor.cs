using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using AC;

[CustomEditor(typeof(Shapeable))]

public class ShapeableEditor : Editor
{
	
	private ShapeGroup selectedGroup;
	private ShapeKey selectedKey;
	
	
	public override void OnInspectorGUI()
	{
		Shapeable _target = (Shapeable) target;
		
		_target.shapeGroups = AllGroupsGUI (_target.shapeGroups);
		
		if (selectedGroup != null)
		{
			selectedGroup = GroupGUI (selectedGroup);
		}
		
		if (GUI.changed)
		{
			EditorUtility.SetDirty (_target);
		}
	}
	
	
	private ShapeGroup GroupGUI (ShapeGroup shapeGroup)
	{
		EditorGUILayout.Space ();
		
		EditorGUILayout.BeginVertical ("Button");
		EditorGUILayout.LabelField ("Shape group " + shapeGroup.label, EditorStyles.boldLabel);
		
		shapeGroup.label = EditorGUILayout.TextField ("Group label:", shapeGroup.label);
		
		shapeGroup.shapeKeys = AllKeysGUI (shapeGroup.shapeKeys);
		
		if (selectedKey != null && shapeGroup.shapeKeys.Contains (selectedKey))
		{
			selectedKey = KeyGUI (selectedKey);
		}
		
		EditorGUILayout.EndVertical ();
		
		return shapeGroup;
	}
	
	
	private ShapeKey KeyGUI (ShapeKey shapeKey)
	{
		EditorGUILayout.LabelField ("Shape key " + shapeKey.label, EditorStyles.boldLabel);
		
		shapeKey.label = EditorGUILayout.TextField ("Key label:", shapeKey.label);
		shapeKey.index = EditorGUILayout.IntField ("BlendShape index:", shapeKey.index);
		return shapeKey;
	}
	
	
	private List<ShapeGroup> AllGroupsGUI (List<ShapeGroup> shapeGroups)
	{
		EditorGUILayout.LabelField ("Shape groups", EditorStyles.boldLabel);
		
		foreach (ShapeGroup shapeGroup in shapeGroups)
		{
			EditorGUILayout.BeginHorizontal ();
			
			string buttonLabel = shapeGroup.ID + ": ";
			if (shapeGroup.label == "")
			{
				buttonLabel += "(Untitled)";	
			}
			else
			{
				buttonLabel += shapeGroup.label;
			}
			
			bool buttonOn = false;
			if (selectedGroup == shapeGroup)
			{
				buttonOn = true;
			}
			
			if (GUILayout.Toggle (buttonOn, buttonLabel, "Button"))
			{
				if (selectedGroup != shapeGroup)
				{
					selectedGroup = shapeGroup;
					selectedKey = null;
				}
			}
			
			if (GUILayout.Button ("-", GUILayout.Width (20f), GUILayout.Height (15f)))
			{
				shapeGroups.Remove (shapeGroup);
				selectedGroup = null;
				selectedKey = null;
				break;
			}
			
			EditorGUILayout.EndHorizontal ();
		}
		
		if (GUILayout.Button ("Create new shape group"))
		{
			ShapeGroup newShapeGroup = new ShapeGroup (GetIDArray (shapeGroups));
			shapeGroups.Add (newShapeGroup);
			selectedGroup = newShapeGroup;
			selectedKey = null;
		}
		
		return shapeGroups;
	}
	
	
	private List<ShapeKey> AllKeysGUI (List<ShapeKey> shapeKeys)
	{
		EditorGUILayout.LabelField ("Shape keys", EditorStyles.boldLabel);
		
		foreach (ShapeKey shapeKey in shapeKeys)
		{
			EditorGUILayout.BeginHorizontal ();
			
			string buttonLabel = shapeKey.ID + ": ";
			if (shapeKey.label == "")
			{
				buttonLabel += "(Untitled)";	
			}
			else
			{
				buttonLabel += shapeKey.label;
			}
			
			bool buttonOn = false;
			if (selectedKey == shapeKey)
			{
				buttonOn = true;
			}
			
			if (GUILayout.Toggle (buttonOn, buttonLabel, "Button"))
			{
				if (selectedKey != shapeKey)
				{
					selectedKey = shapeKey;
				}
			}
			
			if (GUILayout.Button ("-", GUILayout.Width (20f), GUILayout.Height (15f)))
			{
				shapeKeys.Remove (shapeKey);
				selectedKey = null;
				break;
			}
			
			EditorGUILayout.EndHorizontal ();
		}
		
		if (GUILayout.Button ("Create new shape key"))
		{
			ShapeKey newShapeKey = new ShapeKey (GetIDArray (shapeKeys));
			shapeKeys.Add (newShapeKey);
			selectedKey = newShapeKey;
		}
		
		return shapeKeys;
	}
	
	
	private int[] GetIDArray (List<ShapeKey> shapeKeys)
	{
		List<int> idArray = new List<int>();
		foreach (ShapeKey shapeKey in shapeKeys)
		{
			idArray.Add (shapeKey.ID);
		}
		idArray.Sort ();
		return idArray.ToArray ();
	}
	
	
	private int[] GetIDArray (List<ShapeGroup> shapeGroups)
	{
		List<int> idArray = new List<int>();
		foreach (ShapeGroup shapeGroup in shapeGroups)
		{
			idArray.Add (shapeGroup.ID);
		}
		idArray.Sort ();
		return idArray.ToArray ();
	}
	
}
