using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using AC;

[CustomEditor(typeof(Conversation))]
public class ConversationEditor : Editor
{

	private int sideItem = -1;
	private Conversation _target;
	private Texture2D sideIcon;
	private Vector2 scrollPos;

	
	public void OnEnable ()
	{
		_target = (Conversation) target;
	}
	
	
	public override void OnInspectorGUI ()
	{
		if (!sideIcon)
		{
			sideIcon = (Texture2D) AssetDatabase.LoadAssetAtPath ("Assets/AdventureCreator/Graphics/Textures/inspector-use.png", typeof (Texture2D));
		}

		if (_target)
		{
			_target.Upgrade ();
		}
		else
		{
			return;
		}

		EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Conversation settings", EditorStyles.boldLabel);
			_target.interactionSource = (InteractionSource) EditorGUILayout.EnumPopup ("Interaction source:", _target.interactionSource);
			_target.autoPlay = EditorGUILayout.Toggle ("Auto-play lone option?", _target.autoPlay);
			_target.isTimed = EditorGUILayout.Toggle ("Is timed?", _target.isTimed);
			if (_target.isTimed)
			{
				_target.timer = EditorGUILayout.FloatField ("Timer length (s):", _target.timer);
			}
			if (GUILayout.Button ("Conversation Editor"))
			{
				ConversationEditorWindow window = (ConversationEditorWindow) EditorWindow.GetWindow (typeof (ConversationEditorWindow));
				window.Repaint ();
			}
		EditorGUILayout.EndVertical ();
		
		EditorGUILayout.Space ();
		CreateOptionsGUI ();
		EditorGUILayout.Space ();

		if (_target.selectedOption != null && _target.options.Contains (_target.selectedOption))
		{
			EditorGUILayout.LabelField ("Dialogue option '" + _target.selectedOption.label + "' properties", EditorStyles.boldLabel);
			EditOptionGUI (_target.selectedOption, _target.interactionSource);
		}
		
		if (GUI.changed)
		{
			EditorUtility.SetDirty (_target);
		}
	}

	
	private void CreateOptionsGUI ()
	{
		EditorGUILayout.LabelField ("Dialogue options", EditorStyles.boldLabel);

		scrollPos = EditorGUILayout.BeginScrollView (scrollPos, GUILayout.Height (Mathf.Min (_target.options.Count * 21, 130f)+5));
		foreach (ButtonDialog option in _target.options)
		{
			EditorGUILayout.BeginHorizontal ();
			
			string buttonLabel = option.ID + ": " + option.label;
			if (option.label == "")
			{
				buttonLabel += "(Untitled)";	
			}
			if (_target.isTimed && _target.options.IndexOf (option) == _target.defaultOption)
			{
				buttonLabel += " (Default)";
			}
			
			if (GUILayout.Toggle (option.isEditing, buttonLabel, "Button"))
			{
				if (_target.selectedOption != option)
				{
					DeactivateAllOptions ();
					ActivateOption (option);
				}
			}
			
			if (GUILayout.Button (sideIcon, GUILayout.Width (20f), GUILayout.Height (15f)))
			{
				SideMenu (option);
			}

			EditorGUILayout.EndHorizontal ();
		}
		EditorGUILayout.EndScrollView();
		
		if (GUILayout.Button ("Add new dialogue option"))
		{
			Undo.RecordObject (_target, "Create dialogue option");
			ButtonDialog newOption = new ButtonDialog (_target.GetIDArray ());
			_target.options.Add (newOption);
			DeactivateAllOptions ();
			ActivateOption (newOption);
		}
	}


	private void ActivateOption (ButtonDialog option)
	{
		option.isEditing = true;
		_target.selectedOption = option;
	}
	
	
	private void DeactivateAllOptions ()
	{
		foreach (ButtonDialog option in _target.options)
		{
			option.isEditing = false;
		}
		_target.selectedOption = null;
	}
	
	
	private void EditOptionGUI (ButtonDialog option, InteractionSource source)
	{
		EditorGUILayout.BeginVertical ("Button");
		
		if (option.lineID > -1)
		{
			EditorGUILayout.LabelField ("Speech Manager ID:", option.lineID.ToString ());
		}
		
		option.label = EditorGUILayout.TextField ("Label:", option.label);

		if (source == InteractionSource.AssetFile)
		{
			option.assetFile = (ActionListAsset) EditorGUILayout.ObjectField ("Interaction:", option.assetFile, typeof (ActionListAsset), false);
		}
		else if (source == InteractionSource.CustomScript)
		{
			option.customScriptObject = (GameObject) EditorGUILayout.ObjectField ("Object with script:", option.customScriptObject, typeof (GameObject), true);
			option.customScriptFunction = EditorGUILayout.TextField ("Message to send:", option.customScriptFunction);
		}
		else if (source == InteractionSource.InScene)
		{
			EditorGUILayout.BeginHorizontal ();
			option.dialogueOption = (DialogueOption) EditorGUILayout.ObjectField ("Interaction:", option.dialogueOption, typeof (DialogueOption), true);
			if (option.dialogueOption == null)
			{
				if (GUILayout.Button ("Create", GUILayout.MaxWidth (60f)))
				{
					Undo.RecordObject (_target, "Auto-create dialogue option");
					DialogueOption newDialogueOption = SceneManager.AddPrefab ("Logic", "DialogueOption", true, false, true).GetComponent <DialogueOption>();
					
					newDialogueOption.gameObject.name = AdvGame.UniqueName (_target.gameObject.name + "_Option");
					newDialogueOption.Initialise ();
					EditorUtility.SetDirty (newDialogueOption);
					option.dialogueOption = newDialogueOption;
				}
			}
			EditorGUILayout.EndHorizontal ();
		}
		
		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.LabelField ("Icon texture:", GUILayout.Width (155f));
		option.icon = (Texture2D) EditorGUILayout.ObjectField (option.icon, typeof (Texture2D), false, GUILayout.Width (70f), GUILayout.Height (30f));
		EditorGUILayout.EndHorizontal ();
		
		option.isOn = EditorGUILayout.Toggle ("Is enabled?", option.isOn);
		if (source == InteractionSource.CustomScript)
		{
			EditorGUILayout.HelpBox ("Using a custom script will cause the conversation to end when finished, unless it is re-run explicitly.", MessageType.Info);
		}
		else
		{
			option.conversationAction = (ConversationAction) EditorGUILayout.EnumPopup ("When finished:", option.conversationAction);
			if (option.conversationAction == AC.ConversationAction.RunOtherConversation)
			{
				option.newConversation = (Conversation) EditorGUILayout.ObjectField ("Conversation to run:", option.newConversation, typeof (Conversation), true);
			}
		}

		if (_target.isTimed)
		{
			if (_target.options.IndexOf (option) != _target.defaultOption)
			{
				if (GUILayout.Button ("Make default", GUILayout.MaxWidth (80)))
				{
					Undo.RecordObject (_target, "Change default conversation option");
					_target.defaultOption = _target.options.IndexOf (option);
					EditorUtility.SetDirty (_target);
				}
			}
		}
		
		EditorGUILayout.EndVertical ();
	}


	private void SideMenu (ButtonDialog option)
	{
		GenericMenu menu = new GenericMenu ();
		sideItem = _target.options.IndexOf (option);
		
		menu.AddItem (new GUIContent ("Insert after"), false, Callback, "Insert after");
		if (_target.options.Count > 0)
		{
			menu.AddItem (new GUIContent ("Delete"), false, Callback, "Delete");
		}
		if (sideItem > 0 || sideItem < _target.options.Count-1)
		{
			menu.AddSeparator ("");
		}
		if (sideItem > 0)
		{
			menu.AddItem (new GUIContent ("Move up"), false, Callback, "Move up");
		}
		if (sideItem < _target.options.Count-1)
		{
			menu.AddItem (new GUIContent ("Move down"), false, Callback, "Move down");
		}
		
		menu.ShowAsContext ();
	}
	
	
	private void Callback (object obj)
	{
		if (sideItem >= 0)
		{
			ButtonDialog tempItem = _target.options[sideItem];

			switch (obj.ToString ())
			{
			case "Insert after":
				Undo.RecordObject (_target, "Insert option");
				_target.options.Insert (sideItem+1, new ButtonDialog (_target.GetIDArray ()));
				break;
				
			case "Delete":
				Undo.RecordObject (_target, "Delete option");
				DeactivateAllOptions ();
				_target.options.RemoveAt (sideItem);
				break;
				
			case "Move up":
				Undo.RecordObject (_target, "Move option up");
				_target.options.RemoveAt (sideItem);
				_target.options.Insert (sideItem-1, tempItem);
				break;
				
			case "Move down":
				Undo.RecordObject (_target, "Move option down");
				_target.options.RemoveAt (sideItem);
				_target.options.Insert (sideItem+1, tempItem);
				break;
			}
		}

		EditorUtility.SetDirty (_target);

		sideItem = -1;
	}
	
}