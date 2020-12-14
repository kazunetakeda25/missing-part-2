using UnityEngine;
using UnityEditor;
using System.Collections;
using AC;

[CustomEditor(typeof(ArrowPrompt))]
public class ArrowPromptEditor : Editor
{
	
	public override void OnInspectorGUI()
	{
		ArrowPrompt _target = (ArrowPrompt) target;
		
		EditorGUILayout.BeginVertical ("Button");
			GUILayout.Label ("Settings", EditorStyles.boldLabel);
			_target.arrowPromptType = (ArrowPromptType) EditorGUILayout.EnumPopup ("Input type:", _target.arrowPromptType);
			_target.disableHotspots = EditorGUILayout.ToggleLeft ("Disable Hotspots when active?", _target.disableHotspots);
		EditorGUILayout.EndVertical ();
		EditorGUILayout.Space ();

		EditorGUILayout.BeginVertical ("Button");
			GUILayout.Label ("Up arrow", EditorStyles.boldLabel);
			ArrowGUI (_target.upArrow, "Up");
		EditorGUILayout.EndVertical ();
		EditorGUILayout.Space ();
		
		EditorGUILayout.BeginVertical ("Button");
			GUILayout.Label ("Left arrow", EditorStyles.boldLabel);
			ArrowGUI (_target.leftArrow, "Left");
		EditorGUILayout.EndVertical ();
		EditorGUILayout.Space ();

		EditorGUILayout.BeginVertical ("Button");
			GUILayout.Label ("Right arrow", EditorStyles.boldLabel);
			ArrowGUI (_target.rightArrow, "Right");
		EditorGUILayout.EndVertical ();
		EditorGUILayout.Space ();

		EditorGUILayout.BeginVertical ("Button");
			GUILayout.Label ("Down arrow", EditorStyles.boldLabel);
			ArrowGUI (_target.downArrow, "Down");
		EditorGUILayout.EndVertical ();

		if (GUI.changed)
		{
			EditorUtility.SetDirty (_target);
		}
	}
	
	
	private void ArrowGUI (Arrow arrow, string label)
	{
		if (arrow != null)
		{
			ArrowPrompt _target = (ArrowPrompt) target;

			arrow.isPresent = EditorGUILayout.Toggle ("Provide?", arrow.isPresent);
		
			if (arrow.isPresent)
			{
				arrow.texture = (Texture2D) EditorGUILayout.ObjectField ("Icon texture:", arrow.texture, typeof (Texture2D), true);

				EditorGUILayout.BeginHorizontal ();
				arrow.linkedCutscene = (Cutscene) EditorGUILayout.ObjectField ("Linked Cutscene:", arrow.linkedCutscene, typeof (Cutscene), true);
				if (arrow.linkedCutscene == null)
				{
					if (GUILayout.Button ("Create", GUILayout.MaxWidth (60f)))
					{
						Undo.RecordObject (_target, "Auto-create Cutscene");
						Cutscene newCutscene = SceneManager.AddPrefab ("Logic", "Cutscene", true, false, true).GetComponent <Cutscene>();
						
						newCutscene.gameObject.name = AdvGame.UniqueName (_target.gameObject.name + ": " + label);
						arrow.linkedCutscene = newCutscene;
					}
				}
				EditorGUILayout.EndHorizontal ();
			}
		}	
	}

}
