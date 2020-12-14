using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(ConversationDaikon))]
public class ConversationDaikonEditor : Editor
{
	private static GUIContent
		deleteContent = new GUIContent("-", "Delete this option");
	
	private static GUILayoutOption
		buttonWidth = GUILayout.MaxWidth(20f);
	
	private ConversationDaikon _target;
	
	
	public void OnEnable()
	{
		_target = (ConversationDaikon) target;
		if (_target.position.Count == 0) {
			_target.position.Clear();
			foreach(AC.ButtonDialog opt in _target.options)
			{
				_target.position.Add(new Vector3());
				
			}
		}
		if (_target.height.Count == 0) {
			_target.height.Clear();
			foreach(AC.ButtonDialog opt in _target.options)
			{
				_target.height.Add(new float());
			}
		}
	}
	
	
	public override void OnInspectorGUI()
	{
		
		_target.isTimed = EditorGUILayout.Toggle ("Is timed?", _target.isTimed);
		
		if (_target.isTimed)
		{
			_target.timer = EditorGUILayout.FloatField ("Timer length (s):", _target.timer);
		}

		_target.quizType = (Quiz)EditorGUILayout.EnumPopup ("quizType: ", _target.quizType);
		_target.pos = EditorGUILayout.Vector2Field ("position", _target.pos);
		_target.hei = EditorGUILayout.FloatField("Width", _target.hei);
		int count = 0;
		foreach (AC.ButtonDialog option in _target.options)
		{
			EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.BeginHorizontal ();
			option.label = EditorGUILayout.TextField ("Label", option.label);
			if (GUILayout.Button (deleteContent, EditorStyles.miniButtonRight, buttonWidth))
			{
				Undo.RegisterSceneUndo ("Delete dialogue option");
				_target.options.Remove (option);
				_target.position.Remove(_target.position[count]);
				_target.height.Remove(_target.height[count]);
				break;
			}
			//float asdf = 0;
			//asdf = EditorGUILayout.FloatField("adsf", adsf);

			EditorGUILayout.EndHorizontal ();

			//_target.position[count] = EditorGUILayout.Vector3Field("position", _target.position[count]);
			//_target.height[count] = EditorGUILayout.FloatField("Width", _target.height[count]);
			EditorGUILayout.BeginHorizontal ();
			
			option.dialogueOption = (AC.DialogueOption) EditorGUILayout.ObjectField ("Interaction:", option.dialogueOption, typeof (AC.DialogueOption), true);
			
			if (option.dialogueOption == null)
			{
				if (GUILayout.Button ("Auto-create", GUILayout.MaxWidth (90f)))
				{
					Undo.RegisterSceneUndo ("Auto-create dialogue option");
					AC.DialogueOption newDialogueOption = AC.SceneManager.AddPrefab ("Logic", "DialogueOption", true, false, true).GetComponent <AC.DialogueOption>();
					
					newDialogueOption.gameObject.name = AC.AdvGame.UniqueName (_target.gameObject.name + "_Option");
					option.dialogueOption = newDialogueOption;
				}
			}
			
			EditorGUILayout.EndHorizontal ();
			
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Is enabled?", GUILayout.MaxWidth (90));
			option.isOn = EditorGUILayout.Toggle (option.isOn, buttonWidth);
			if (_target.isTimed)
			{
				if (_target.defaultOption == option)
				{
					EditorGUILayout.LabelField ("Default");
				}
				else
				{
					if (GUILayout.Button ("Make default", GUILayout.MaxWidth (80)))
					{
						Undo.RegisterUndo (_target, "Change default conversation option");
						_target.defaultOption = option;
					}
				}
			}
			EditorGUILayout.EndHorizontal ();
			
			EditorGUILayout.EndVertical ();
			EditorGUILayout.Space ();
				count++;
		}
		
		if (GUILayout.Button ("Add new dialogue option"))
		{
			Undo.RegisterSceneUndo ("Create dialogue option");
			_target.options.Add (new AC.ButtonDialog(null));
			_target.position.Add(new Vector3());
			_target.height.Add(new float());
		}
		
		if (GUI.changed)
		{
			EditorUtility.SetDirty (_target);
		}
		
	}
	
}