using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using AC;

public class ConversationEditorWindow : EditorWindow
{
	
	private Vector2 scrollPosition = Vector2.zero;

	private Conversation lastConversation;
	private Conversation conversation;
	
	private Rect convRect = new Rect (20, 150, 150, 50);
	private Rect lastRect = new Rect (20, 150, 150, 50);
	private Rect optionRect = new Rect (20, 150, 150, 50);
	private Rect interactionRect = new Rect (20, 150, 150, 50);
	private Rect finishRect = new Rect (20, 150, 150, 50);
	private Rect newRect = new Rect (20, 150, 150, 50);


	[MenuItem ("Adventure Creator/Editors/Conversation Editor", false, 2)]
	static void Init ()
	{
		ConversationEditorWindow window = (ConversationEditorWindow) EditorWindow.GetWindow (typeof (ConversationEditorWindow));
		window.Repaint ();
		window.title = "Conversation Editor";
	}

	
	public void OnInspectorUpdate()
	{
		Repaint();
	}
	
	
	private void OnGUI ()
	{
		if (Selection.activeGameObject && Selection.activeGameObject.GetComponent <Conversation>() && (conversation == null || Selection.activeGameObject != conversation.gameObject))
		{
			if (conversation != null)
			{
				lastConversation = conversation;
			}
			conversation = Selection.activeGameObject.GetComponent<Conversation>();
		}

		if (conversation)
		{
			conversation.Upgrade ();
		}
		
		if (lastConversation != null && lastConversation == conversation)
		{
			lastConversation = null;
		}
		
		if (conversation != null)
		{
			OptionsGUI ();
		}
		else if (lastConversation != null)
		{
			conversation = lastConversation;
		}
		else
		{
			GUILayout.Label ("Please select a Conversation in your scene");
		}
		
		if (GUI.changed)
		{
			EditorUtility.SetDirty (conversation);
		}
	}
	
	
	private void OptionsGUI ()
	{
		if (conversation == null)
		{
			return;
		}

		scrollPosition = GUI.BeginScrollView (new Rect (0, 0, position.width, position.height), scrollPosition, new Rect (0, 0, 1100, 77 * (conversation.options.Count + 2)), false, false);
		BeginWindows ();
		
		convRect = new Rect (20, 100, 150, 50);
		convRect = GUI.Window(-1, convRect, NodeWindow, "Conversation");

		if (GUI.Button (convRect, ""))
		{
			Selection.activeGameObject = conversation.gameObject;
			foreach (ButtonDialog option in conversation.options)
			{
				option.isEditing = false;
			}
			conversation.selectedOption = null;
		}

		if (lastConversation != null)
		{
			lastRect = new Rect (20, 20, 150, 50);
			lastRect = GUI.Window(-2, lastRect, NodeWindow, "Previous conversation");

			if (GUI.Button (lastRect, ""))
			{
				Selection.activeGameObject = lastConversation.gameObject;
			}
		}
		
		for (int i=0; i<conversation.options.Count; i++)
		{
			optionRect = new Rect (220, 20 + (i*80), 200, 50);
			optionRect = GUI.Window(i, optionRect, NodeWindow, "Dialogue option");
			
			if (conversation.options[i].label == "")
			{
				DrawNodeCurve (convRect, optionRect, Color.red);
			}
			else
			{
				DrawNodeCurve (convRect, optionRect, Color.blue);
			}
			
			if (GUI.Button (optionRect, ""))
			{
				Selection.activeGameObject = conversation.gameObject;
				foreach (ButtonDialog option in conversation.options)
				{
					option.isEditing = false;
				}
				conversation.options[i].isEditing = true;
				conversation.selectedOption = conversation.options[i];
			}
			
			interactionRect = new Rect (440, 20 + (i*80), 200, 50);
			interactionRect = GUI.Window(i + conversation.options.Count, interactionRect, NodeWindow, "Interaction");
			
			if (conversation.options[i].dialogueOption == null)
			{
				DrawNodeCurve (optionRect, interactionRect, Color.red);
			}
			else
			{
				DrawNodeCurve (optionRect, interactionRect, Color.blue);
			}
			
			if (conversation.options[i].dialogueOption != null)
			{
				finishRect = new Rect (660, 20 + (i*80), 200, 50);
				finishRect = GUI.Window(i + (conversation.options.Count*2), finishRect, NodeWindow, "When finished");
				DrawNodeCurve (interactionRect, finishRect, Color.blue);

				if (GUI.Button (finishRect, ""))
				{
					Selection.activeGameObject = conversation.gameObject;
					foreach (ButtonDialog option in conversation.options)
					{
						option.isEditing = false;
					}
					conversation.options[i].isEditing = true;
					conversation.selectedOption = conversation.options[i];
				}
				
				if (conversation.options[i].conversationAction == AC.ConversationAction.RunOtherConversation)
				{
					newRect = new Rect (880, 20 + (i*80), 200, 50);
					newRect = GUI.Window(i + (conversation.options.Count*3), newRect, NodeWindow, "Conversation");

					if (conversation.options[i].newConversation == null)
					{
						DrawNodeCurve (finishRect, newRect, Color.red);
					}
					else
					{
						DrawNodeCurve (finishRect, newRect, Color.blue);
					}

					if (conversation.options[i].newConversation != null)
					{
						if (GUI.Button (newRect, ""))
						{
							lastConversation = conversation;
							Selection.activeGameObject = conversation.options[i].newConversation.gameObject;
						}
					}
				}
			}

		}
		
		EndWindows ();

		if (GUI.Button (new Rect (260, 10 + (conversation.options.Count*80), 120, 20), "Add new option"))
		{
			Undo.RecordObject (conversation, "Create dialogue option");
			ButtonDialog newOption = new ButtonDialog (conversation.GetIDArray ());
			conversation.options.Add (newOption);

			Selection.activeGameObject = conversation.gameObject;
			foreach (ButtonDialog option in conversation.options)
			{
				option.isEditing = false;
			}
			newOption.isEditing = true;
			conversation.selectedOption = newOption;
		}

		GUI.EndScrollView ();
	}
	
	
	private void NodeWindow (int ID)
	{
		if (ID == -2)
		{
			GUILayout.Label (lastConversation.gameObject.name);
		}
		else if (ID == -1)
		{
			GUILayout.Label (conversation.gameObject.name);
		}
		else if (ID < conversation.options.Count)
		{
			GUILayout.BeginHorizontal ();
			conversation.options[ID].label = GUILayout.TextField (conversation.options[ID].label);
			if (GUILayout.Button ("-", GUILayout.Width (20f)))
			{
				Undo.RecordObject (this, "Delete dialogue option");
				
				foreach (ButtonDialog option in conversation.options)
				{
					option.isEditing = false;
				}
				conversation.selectedOption = null;
				conversation.options.RemoveAt (ID);
			}
			GUILayout.EndHorizontal ();
		}
		else if (ID < conversation.options.Count * 2)
		{
			int i = ID - conversation.options.Count;
			if (conversation.interactionSource == InteractionSource.AssetFile)
			{
				conversation.options[i].assetFile = (ActionListAsset) EditorGUILayout.ObjectField (conversation.options[i].assetFile, typeof (ActionListAsset), false);
			}
			else if (conversation.interactionSource == InteractionSource.CustomScript)
			{
				GUILayout.Label ("(Set in Inspector)");
			}
			else if (conversation.interactionSource == InteractionSource.InScene)
			{
				if (conversation.options[i].dialogueOption != null)
				{
					GUILayout.BeginHorizontal ();

					string label = conversation.options[i].dialogueOption.gameObject.name;
					if (label.Length > 22)
					{
						label = label.Substring (0,22);
					}

					if (GUILayout.Button (label))
					{
						if (conversation.interactionSource == InteractionSource.InScene)
						{
							Selection.activeGameObject = conversation.options[i].dialogueOption.gameObject;
						}
					}

					if (GUILayout.Button ("", ActionListEditorWindow.nodeSkin.customStyles[13]))
					{
						if (conversation.interactionSource == InteractionSource.InScene)
						{
							ActionListEditorWindow.Init (conversation.options[i].dialogueOption);
						}
						else
						{
							ActionListEditorWindow.Init (conversation.options[i].dialogueOption.assetFile);
						}
					}
					GUILayout.EndHorizontal ();
				}
				else
				{
					GUILayout.BeginHorizontal ();
					GUILayout.Label ("(Not set)");
					if (GUILayout.Button ("Create"))
					{
						Undo.RecordObject (conversation, "Auto-create dialogue option");
						DialogueOption newDialogueOption = SceneManager.AddPrefab ("Logic", "DialogueOption", true, false, true).GetComponent <DialogueOption>();
						
						newDialogueOption.gameObject.name = AdvGame.UniqueName (conversation.gameObject.name + "_Option");
						newDialogueOption.Initialise ();
						EditorUtility.SetDirty (newDialogueOption);
						conversation.options[i].dialogueOption = newDialogueOption;
					}
					GUILayout.EndHorizontal ();
				}
			}
		}
		else if (ID < conversation.options.Count * 3)
		{
			int i = ID - (conversation.options.Count*2);
			conversation.options[i].conversationAction = (ConversationAction) EditorGUILayout.EnumPopup (conversation.options[i].conversationAction);
		}
		else
		{
			int i = ID - (conversation.options.Count*3);
			if (conversation.options[i].newConversation != null)
			{
				conversation.options[i].newConversation = (Conversation) EditorGUILayout.ObjectField (conversation.options[i].newConversation, typeof (Conversation), true);
			}
			else
			{
				GUILayout.BeginHorizontal ();
				conversation.options[i].newConversation = (Conversation) EditorGUILayout.ObjectField (conversation.options[i].newConversation, typeof (Conversation), true);

				if (GUILayout.Button ("Create"))
				{
					Undo.RecordObject (conversation, "Auto-create conversation");
					Conversation newConversation = SceneManager.AddPrefab ("Logic", "Conversation", true, false, true).GetComponent <Conversation>();
					conversation.options[i].newConversation = newConversation;
				}
				GUILayout.EndHorizontal ();
			}
		}
	}
	
	
	private void DrawNodeCurve (Rect start, Rect end, Color color)
	{
		Vector3 startPos = new Vector3(start.x + start.width, start.y + start.height / 2, 0);
		Vector3 endPos = new Vector3(end.x, end.y + end.height / 2, 0);
		Vector3 startTan = startPos + Vector3.right * 20;
		Vector3 endTan = endPos + Vector3.left * 20;
		
		Handles.DrawBezier(startPos, endPos, startTan, endTan, color, null, 3);
	}
	
}