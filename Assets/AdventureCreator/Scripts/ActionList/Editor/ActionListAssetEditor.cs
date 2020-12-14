using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using AC;

[CustomEditor(typeof(ActionListAsset))]

[System.Serializable]
public class ActionListAssetEditor : Editor
{

	private AC.Action actionToAffect = null;
	
	private ActionsManager actionsManager;


	private void OnEnable ()
	{
		if (AdvGame.GetReferences ())
		{
			if (AdvGame.GetReferences ().actionsManager)
			{
				actionsManager = AdvGame.GetReferences ().actionsManager;
				AdventureCreator.RefreshActions ();
			}
			else
			{
				Debug.LogError ("An Actions Manager is required - please use the Game Editor window to create one.");
			}
		}
		else
		{
			Debug.LogError ("A References file is required - please use the Game Editor window to create one.");
		}
	}
	

	public override void OnInspectorGUI ()
	{
		ActionListAsset _target = (ActionListAsset) target;

		EditorGUILayout.BeginVertical ("Button");
		EditorGUILayout.LabelField ("Asset properties", EditorStyles.boldLabel);
		_target.actionListType = (ActionListType) EditorGUILayout.EnumPopup ("When running:", _target.actionListType);
		if (_target.actionListType == ActionListType.PauseGameplay)
		{
			_target.isSkippable = EditorGUILayout.Toggle ("Is skippable?", _target.isSkippable);
			_target.unfreezePauseMenus = EditorGUILayout.Toggle ("Unfreeze 'pause' Menus?", _target.unfreezePauseMenus);
		}
		_target.useParameters = EditorGUILayout.Toggle ("Use parameters?", _target.useParameters);
		EditorGUILayout.EndVertical ();

		if (_target.useParameters)
		{
			ShowParametersGUI (_target);
		}
		EditorGUILayout.Space ();

		EditorGUILayout.BeginHorizontal ();
		if (GUILayout.Button ("Expand all", EditorStyles.miniButtonLeft))
		{
			Undo.RecordObject (_target, "Expand actions");
			foreach (AC.Action action in _target.actions)
			{
				action.isDisplayed = true;
			}
		}
		if (GUILayout.Button ("Collapse all", EditorStyles.miniButtonMid))
		{
			Undo.RecordObject (_target, "Collapse actions");
			foreach (AC.Action action in _target.actions)
			{
				action.isDisplayed = false;
			}
		}
		if (GUILayout.Button ("Action List Editor", EditorStyles.miniButtonMid))
		{
			ActionListEditorWindow.Init (_target);
		}
		if (!Application.isPlaying)
		{
			GUI.enabled = false;
		}
		if (GUILayout.Button ("Run now", EditorStyles.miniButtonRight))
		{
			AdvGame.RunActionListAsset (_target);
		}
		GUI.enabled = true;
		EditorGUILayout.EndHorizontal ();
		EditorGUILayout.Space ();

		if (actionsManager == null)
		{
			EditorGUILayout.HelpBox ("An Actions Manager asset file must be assigned in the Game Editor Window", MessageType.Warning);
			OnEnable ();
			return;
		}

		if (!actionsManager.displayActionsInInspector)
		{
			EditorGUILayout.HelpBox ("As set by the Actions Manager, Actions are only displayed in the ActionList Editor window.", MessageType.Info);
			return;
		}

		for (int i=0; i<_target.actions.Count; i++)
		{
			int typeIndex = KickStarter.actionsManager.GetActionTypeIndex (_target.actions[i]);

			if (_target.actions[i] == null)
			{
				_target.actions.Insert (i, ActionListAssetEditor.RebuildAction (_target.actions[i], typeIndex, _target));
			}
			
			_target.actions[i].isAssetFile = true;
			
			EditorGUILayout.BeginVertical("Button");
			
			string actionLabel = " " + (i).ToString() + ": " + actionsManager.EnabledActions[typeIndex].GetFullTitle () + _target.actions[i].SetLabel ();

			EditorGUILayout.BeginHorizontal ();
			_target.actions[i].isDisplayed = EditorGUILayout.Foldout (_target.actions[i].isDisplayed, actionLabel);
			if (!_target.actions[i].isEnabled)
			{
				EditorGUILayout.LabelField ("DISABLED", EditorStyles.boldLabel, GUILayout.Width (100f));
			}

			Texture2D icon = (Texture2D) AssetDatabase.LoadAssetAtPath ("Assets/AdventureCreator/Graphics/Textures/inspector-use.png", typeof (Texture2D));
			if (GUILayout.Button (icon, GUILayout.Width (20f), GUILayout.Height (15f)))
			{
				ActionSideMenu (_target.actions[i]);
			}
			EditorGUILayout.EndHorizontal ();
			
			if (_target.actions[i].isDisplayed)
			{
				if (!actionsManager.DoesActionExist (_target.actions[i].GetType ().ToString ()))
				{
					EditorGUILayout.HelpBox ("This Action type has been disabled in the Actions Manager", MessageType.Warning);
				}
				else
				{
					int newTypeIndex = ActionListEditor.ShowTypePopup (_target.actions[i], typeIndex);
					if (newTypeIndex >= 0)
					{
						// Rebuild constructor if Subclass and type string do not match
						ActionEnd _end = new ActionEnd ();
						_end.resultAction = _target.actions[i].endAction;
						_end.skipAction = _target.actions[i].skipAction;
						_end.linkedAsset = _target.actions[i].linkedAsset;
						_end.linkedCutscene = _target.actions[i].linkedCutscene;

						Undo.RecordObject (_target, "Change Action type");

						_target.actions.Insert (i, ActionListAssetEditor.RebuildAction (_target.actions[i], newTypeIndex, _target, _end.resultAction, _end.skipAction, _end.linkedAsset, _end.linkedCutscene));
					}

					EditorGUILayout.Space ();
					GUI.enabled = _target.actions[i].isEnabled;

					if (_target.useParameters)
					{
						_target.actions[i].ShowGUI (_target.parameters);
					}
					else
					{
						_target.actions[i].ShowGUI (null);
					}
				}
				GUI.enabled = true;
			}
			
			if (_target.actions[i].endAction == AC.ResultAction.Skip || _target.actions[i] is ActionCheck || _target.actions[i] is ActionCheckMultiple || _target.actions[i] is ActionParallel)
			{
				_target.actions[i].SkipActionGUI (_target.actions, _target.actions[i].isDisplayed);
			}
			
			EditorGUILayout.EndVertical();
			EditorGUILayout.Space ();
		}
		
		if (GUILayout.Button("Add new Action"))
		{
			AddAction (actionsManager.GetActionName (actionsManager.defaultClass), _target.actions.Count, _target);
		}

		if (GUI.changed)
		{
			EditorUtility.SetDirty (_target);
		}
	}


	public static Action RebuildAction (AC.Action action, int typeIndex, ActionListAsset _target)
	{
		ActionsManager actionsManager = AdvGame.GetReferences ().actionsManager;
		
		if (actionsManager)
		{
			ActionListAssetEditor.DeleteAction (action, _target);

			string className = actionsManager.AllActions [typeIndex].fileName;

			AC.Action newAction = (AC.Action) CreateInstance (className);
			newAction.hideFlags = HideFlags.HideInHierarchy;

			AssetDatabase.AddObjectToAsset (newAction, _target);
			AssetDatabase.ImportAsset (AssetDatabase.GetAssetPath (newAction));
			AssetDatabase.SaveAssets ();
			AssetDatabase.Refresh ();
			
			return newAction;
		}
		
		return action;
	}


	public static Action RebuildAction (AC.Action action, int typeIndex, ActionListAsset _target, ResultAction _resultAction, int _skipAction, ActionListAsset _linkedAsset, Cutscene _linkedCutscene)
	{
		ActionsManager actionsManager = AdvGame.GetReferences ().actionsManager;

		if (actionsManager)
		{
			ActionListAssetEditor.DeleteAction (action, _target);

			string className = actionsManager.AllActions [typeIndex].fileName;

			AC.Action newAction = (AC.Action) CreateInstance (className);
			newAction.hideFlags = HideFlags.HideInHierarchy;

			newAction.endAction = _resultAction;
			newAction.skipAction = _skipAction;
			newAction.linkedAsset = _linkedAsset;
			newAction.linkedCutscene = _linkedCutscene;

			AssetDatabase.AddObjectToAsset (newAction, _target);
			AssetDatabase.ImportAsset (AssetDatabase.GetAssetPath (newAction));
			AssetDatabase.SaveAssets ();
			AssetDatabase.Refresh ();

			return newAction;
		}
		
		return action;
	}
	
	
	public static void DeleteAction (AC.Action action, ActionListAsset _target)
	{
		UnityEngine.Object.DestroyImmediate (action, true);
		AssetDatabase.SaveAssets ();
		_target.actions.Remove (action);
	}
	
	
	public static void AddAction (string className, int i, ActionListAsset _target)
	{
		List<int> idArray = new List<int>();
		
		foreach (AC.Action _action in _target.actions)
		{
			idArray.Add (_action.id);
		}
		
		idArray.Sort ();
		
		AC.Action newAction = (AC.Action) CreateInstance (className);
		newAction.hideFlags = HideFlags.HideInHierarchy;
		
		// Update id based on array
		foreach (int _id in idArray.ToArray())
		{
			if (newAction.id == _id)
				newAction.id ++;
		}
		
		newAction.name = newAction.title;
		
		_target.actions.Insert (i, newAction);
		AssetDatabase.AddObjectToAsset (newAction, _target);
		AssetDatabase.ImportAsset (AssetDatabase.GetAssetPath (newAction));
		AssetDatabase.Refresh ();
	}
	
	
	private void ActionSideMenu (AC.Action action)
	{
		ActionListAsset _target = (ActionListAsset) target;
		
		int i = _target.actions.IndexOf (action);
		actionToAffect = action;
		GenericMenu menu = new GenericMenu ();
		
		if (action.isEnabled)
		{
			menu.AddItem (new GUIContent ("Disable"), false, Callback, "Disable");
		}
		else
		{
			menu.AddItem (new GUIContent ("Enable"), false, Callback, "Enable");
		}
		menu.AddSeparator ("");
		if (_target.actions.Count > 1)
		{
			menu.AddItem (new GUIContent ("Cut"), false, Callback, "Cut");
		}
		menu.AddItem (new GUIContent ("Copy"), false, Callback, "Copy");
		if (AdvGame.copiedActions.Count > 0)
		{
			menu.AddItem (new GUIContent ("Paste after"), false, Callback, "Paste after");
		}
		menu.AddSeparator ("");
		menu.AddItem (new GUIContent ("Insert after"), false, Callback, "Insert after");
		menu.AddItem (new GUIContent ("Delete"), false, Callback, "Delete");
		if (i > 0 || i < _target.actions.Count-1)
		{
			menu.AddSeparator ("");
		}
		if (i > 0)
		{
			menu.AddItem (new GUIContent ("Re-arrange/Move to top"), false, Callback, "Move to top");
			menu.AddItem (new GUIContent ("Re-arrange/Move up"), false, Callback, "Move up");
		}
		if (i < _target.actions.Count-1)
		{
			menu.AddItem (new GUIContent ("Re-arrange/Move down"), false, Callback, "Move down");
			menu.AddItem (new GUIContent ("Re-arrange/Move to bottom"), false, Callback, "Move to bottom");
		}
		
		menu.ShowAsContext ();
	}
	

	private void Callback (object obj)
	{
		ActionListAsset t = (ActionListAsset) target;
		ModifyAction (t, actionToAffect, obj.ToString ());
		EditorUtility.SetDirty (t);
	}
	
	
	public static void ModifyAction (ActionListAsset _target, AC.Action _action, string callback)
	{
		ActionsManager actionsManager = AdvGame.GetReferences ().actionsManager;
		if (actionsManager == null)
		{
			return;
		}

		int i = -1;
		if (_action != null && _target.actions.IndexOf (_action) > -1)
		{
			i = _target.actions.IndexOf (_action);
		}
		
		switch (callback)
		{
		case "Enable":
			Undo.RecordObject (_target, "Enable action");
			_target.actions [i].isEnabled = true;
			break;
			
		case "Disable":
			Undo.RecordObject (_target, "Disable action");
			_target.actions [i].isEnabled = false;
			break;
			
		case "Cut":
			Undo.RecordObject (_target, "Cut action");
			List<AC.Action> cutList = new List<AC.Action>();
			AC.Action cutAction = Object.Instantiate (_action) as AC.Action;
			cutList.Add (cutAction);
			AdvGame.copiedActions = cutList;
			_target.actions.Remove (_action);
			UnityEngine.Object.DestroyImmediate (_action, true);
			AssetDatabase.SaveAssets ();
			break;
			
		case "Copy":
			List<AC.Action> copyList = new List<AC.Action>();
			AC.Action copyAction = Object.Instantiate (_action) as AC.Action;
			copyAction.name = copyAction.name.Replace ("(Clone)", "");
			copyList.Add (copyAction);
			AdvGame.copiedActions = copyList;
			break;
			
		case "Paste after":
			Undo.RecordObject (_target, "Paste actions");
			List<AC.Action> pasteList = AdvGame.copiedActions;
			int j=i+1;
			foreach (AC.Action action in pasteList)
			{
				AC.Action pastedAction = Object.Instantiate (action) as AC.Action;
				pastedAction.name = pastedAction.name.Replace ("(Clone)", "");
				pastedAction.hideFlags = HideFlags.HideInHierarchy;
				_target.actions.Insert (j, pastedAction);
				j++;
				AssetDatabase.AddObjectToAsset (pastedAction, _target);
				AssetDatabase.ImportAsset (AssetDatabase.GetAssetPath (pastedAction));
			}
			AssetDatabase.SaveAssets ();
			break;
			
		case "Insert after":
			Undo.RecordObject (_target, "Create action");
			ActionListAssetEditor.AddAction (actionsManager.GetDefaultAction (), i+1, _target);
			break;
			
		case "Delete":
			Undo.RecordObject (_target, "Delete action");
			_target.actions.Remove (_action);
			UnityEngine.Object.DestroyImmediate (_action, true);
			AssetDatabase.SaveAssets ();
			break;
			
		case "Move to top":
			Undo.RecordObject (_target, "Move action to top");
			_target.actions.Remove (_action);
			_target.actions.Insert (0, _action);
			break;
			
		case "Move up":
			Undo.RecordObject (_target, "Move action up");
			_target.actions.Remove (_action);
			_target.actions.Insert (i-1, _action);
			break;
			
		case "Move to bottom":
			Undo.RecordObject (_target, "Move action to bottom");
			_target.actions.Remove (_action);
			_target.actions.Insert (_target.actions.Count, _action);
			break;
			
		case "Move down":
			Undo.RecordObject (_target, "Move action down");
			_target.actions.Remove (_action);
			_target.actions.Insert (i+1, _action);
			break;
		}
	}

		
	public static List<AC.Action> ResizeList (ActionListAsset _target, int listSize)
	{
		ActionsManager actionsManager = AdvGame.GetReferences ().actionsManager;
		
		string defaultAction = "";
		
		if (actionsManager)
		{
			defaultAction = actionsManager.GetDefaultAction ();
		}
		
		if (_target.actions.Count < listSize)
		{
			// Increase size of list
			while (_target.actions.Count < listSize)
			{
				List<int> idArray = new List<int>();
				
				foreach (AC.Action _action in _target.actions)
				{
					idArray.Add (_action.id);
				}
				
				idArray.Sort ();

				Action newAction = (Action) CreateInstance (defaultAction);
				AssetDatabase.AddObjectToAsset (newAction, _target);
				_target.actions.Add (newAction);
				
				// Update id based on array
				foreach (int _id in idArray.ToArray())
				{
					if (_target.actions [_target.actions.Count -1].id == _id)
						_target.actions [_target.actions.Count -1].id ++;
				}
			}
			AssetDatabase.SaveAssets ();
		}
		else if (_target.actions.Count > listSize)
		{
			// Decrease size of list
			while (_target.actions.Count > listSize)
			{
				Action removeAction = _target.actions [_target.actions.Count - 1];
				_target.actions.Remove (removeAction);;
				UnityEngine.Object.DestroyImmediate (removeAction, true);
			}
			AssetDatabase.SaveAssets ();
		}

		return (_target.actions);
	}


	public static void ResetList (ActionListAsset _targetAsset)
	{
		if (_targetAsset.actions.Count == 0 || (_targetAsset.actions.Count == 1 && _targetAsset.actions[0] == null))
		{
			if (_targetAsset.actions.Count == 1)
			{
				ActionListAssetEditor.DeleteAction (_targetAsset.actions[0], _targetAsset);
			}
			Action newAction = ActionList.GetDefaultAction ();
			_targetAsset.actions.Add (newAction);
			newAction.hideFlags = HideFlags.HideInHierarchy;
			
			AssetDatabase.AddObjectToAsset (newAction, _targetAsset);
			AssetDatabase.ImportAsset (AssetDatabase.GetAssetPath (newAction));
			AssetDatabase.SaveAssets ();
			AssetDatabase.Refresh ();
		}
	}


	private void ShowParametersGUI (ActionListAsset _targetAsset)
	{
		EditorGUILayout.Space ();
		EditorGUILayout.BeginVertical ("Button");
		EditorGUILayout.LabelField ("Parameters", EditorStyles.boldLabel);
		ActionListEditor.ShowParametersGUI (_targetAsset.parameters);
		EditorGUILayout.EndVertical ();
	}

}