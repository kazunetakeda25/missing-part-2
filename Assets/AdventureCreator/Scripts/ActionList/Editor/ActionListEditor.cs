using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace AC
{

	[CustomEditor (typeof (ActionList))]

	[System.Serializable]
	public class ActionListEditor : Editor
	{

		private int typeNumber;
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
			ActionList _target = (ActionList) target;

			DrawSharedElements ();

			if (GUI.changed)
			{
				EditorUtility.SetDirty (_target);
			}
		}
		
		
		protected void DrawSharedElements ()
		{
			ActionList _target = (ActionList) target;

			if (!_target.gameObject.activeInHierarchy)
			{
				EditorGUILayout.HelpBox ("Scene-based Actions can not live in prefabs - use ActionList assets instead.", MessageType.Info);
				return;
			}
				
			if (_target.source == ActionListSource.AssetFile)
			{
				return;
			}
			
			int numActions = _target.actions.Count;
			if (numActions < 1)
			{
				numActions = 1;
				AC.Action newAction = ActionList.GetDefaultAction ();
				_target.actions.Add (newAction);
			}

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
				_target.Interact ();
			}
			GUI.enabled = true;
			EditorGUILayout.EndHorizontal ();
			EditorGUILayout.Space ();

			ActionListEditor.ResetList (_target);

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
				if (_target.actions[i] == null)
				{
					Debug.LogWarning ("An empty Action was found, and was deleted");
					_target.actions.RemoveAt (i);
					continue;
				}

				EditorGUILayout.BeginVertical ("Button");
				EditorGUILayout.BeginHorizontal ();
				int typeIndex = KickStarter.actionsManager.GetActionTypeIndex (_target.actions[i]);
				string actionLabel = " (" + (i).ToString() + ") " + actionsManager.EnabledActions[typeIndex].GetFullTitle () + _target.actions[i].SetLabel ();
				_target.actions[i].isDisplayed = EditorGUILayout.Foldout (_target.actions[i].isDisplayed, actionLabel);
				if (!_target.actions[i].isEnabled)
				{
					EditorGUILayout.LabelField ("DISABLED", EditorStyles.boldLabel, GUILayout.MaxWidth (100f));
				}
				Texture2D icon = (Texture2D) AssetDatabase.LoadAssetAtPath ("Assets/AdventureCreator/Graphics/Textures/inspector-use.png", typeof (Texture2D));
				if (GUILayout.Button (icon, GUILayout.Width (20f), GUILayout.Height (15f)))
				{
					ActionSideMenu (i);
				}

				_target.actions[i].isAssetFile = false;
				
				EditorGUILayout.EndHorizontal ();

				if (_target.actions[i].isDisplayed)
				{
					GUI.enabled = _target.actions[i].isEnabled;

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
							_target.actions[i] = ActionListEditor.RebuildAction (_target.actions[i], newTypeIndex, _end.resultAction, _end.skipAction, _end.linkedAsset, _end.linkedCutscene);
						}

						if (_target.useParameters && _target.parameters != null && _target.parameters.Count > 0)
						{
							_target.actions[i].ShowGUI (_target.parameters);
						}
						else
						{
							_target.actions[i].ShowGUI (null);
						}
					}
				}

				if (_target.actions[i].endAction == AC.ResultAction.Skip || _target.actions[i].numSockets == 2 || _target.actions[i] is ActionCheckMultiple || _target.actions[i] is ActionParallel)
				{
					_target.actions[i].SkipActionGUI (_target.actions, _target.actions[i].isDisplayed);
				}

				GUI.enabled = true;
				
				EditorGUILayout.EndVertical ();
				EditorGUILayout.Space ();
			}

			if (GUILayout.Button("Add new action"))
			{
				Undo.RecordObject (_target, "Create action");
				numActions += 1;
			}
			
			_target.actions = ActionListEditor.ResizeList (_target.actions, numActions);
		}


		public static int ShowTypePopup (AC.Action action, int typeIndex)
		{
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Action type:", GUILayout.Width (80));
			
			ActionCategory oldCategory = KickStarter.actionsManager.GetActionCategory (typeIndex);
			ActionCategory category = oldCategory;
			int subcategory = KickStarter.actionsManager.GetActionSubCategory (action);
			
			category = (ActionCategory) EditorGUILayout.EnumPopup (category);
			if (category != oldCategory) subcategory = 0;
			subcategory = EditorGUILayout.Popup (subcategory, KickStarter.actionsManager.GetActionSubCategories (category));
			int newTypeIndex = KickStarter.actionsManager.GetActionTypeIndex (category, subcategory);

			EditorGUILayout.EndHorizontal ();
			GUILayout.Space (4f);

			if (newTypeIndex != typeIndex)
			{
				return newTypeIndex;
			}
			return -1;
		}


		public static AC.Action RebuildAction (AC.Action action, int typeIndex, ResultAction _resultAction, int _skipAction, ActionListAsset _linkedAsset, Cutscene _linkedCutscene)
		{
			ActionsManager actionsManager = AdvGame.GetReferences ().actionsManager;
			if (actionsManager)
			{
				string className = actionsManager.AllActions [typeIndex].fileName;
				
				if (action.GetType ().ToString () != className && action.GetType ().ToString () != ("AC." + className))
				{
					action = (AC.Action) CreateInstance (className);
					action.endAction = _resultAction;
					action.skipAction = _skipAction;
					action.linkedAsset = _linkedAsset;
					action.linkedCutscene = _linkedCutscene;
				}
			}
			
			return action;
		}

		
		private void ActionSideMenu (int i)
		{
			ActionList _target = (ActionList) target;
			actionToAffect = _target.actions[i];
			GenericMenu menu = new GenericMenu ();
			
			if (_target.actions[i].isEnabled)
			{
				menu.AddItem (new GUIContent ("Disable"), false, Callback, "Disable");
			}
			else
			{
				menu.AddItem (new GUIContent ("Enable"), false, Callback, "Enable");
			}
			menu.AddSeparator ("");
			if (!Application.isPlaying)
			{
				if (_target.actions.Count > 1)
				{
					menu.AddItem (new GUIContent ("Cut"), false, Callback, "Cut");
				}
				menu.AddItem (new GUIContent ("Copy"), false, Callback, "Copy");
			}
			if (AdvGame.copiedActions.Count > 0)
			{
				menu.AddItem (new GUIContent ("Paste after"), false, Callback, "Paste after");
			}
			menu.AddSeparator ("");
			menu.AddItem (new GUIContent ("Insert after"), false, Callback, "Insert after");
			if (_target.actions.Count > 1)
			{
				menu.AddItem (new GUIContent ("Delete"), false, Callback, "Delete");
			}
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
			ActionList t = (ActionList) target;
			ModifyAction (t, actionToAffect, obj.ToString ());
			EditorUtility.SetDirty (t);
		}
		
		
		public static void ModifyAction (ActionList _target, AC.Action _action, string callback)
		{
			int i = -1;
			if (_action != null && _target.actions.IndexOf (_action) > -1)
			{
				i = _target.actions.IndexOf (_action);
			}
			
			switch (callback)
			{
			case "Enable":
				Undo.RecordObject (_target, "Enable action");
				_action.isEnabled = true;
				break;
				
			case "Disable":
				Undo.RecordObject (_target, "Disable action");
				_action.isEnabled = false;
				break;
				
			case "Cut":
				Undo.RecordObject (_target, "Cut action");
				List<AC.Action> cutList = new List<AC.Action>();
				AC.Action cutAction = Object.Instantiate (_action) as AC.Action;
				cutList.Add (cutAction);
				AdvGame.copiedActions = cutList;
				_target.actions.Remove (_action);
				break;
				
			case "Copy":
				List<AC.Action> copyList = new List<AC.Action>();
				AC.Action copyAction = Object.Instantiate (_action) as AC.Action;
				copyAction.nodeRect = new Rect (0,0,300,60);
				copyList.Add (copyAction);
				AdvGame.copiedActions = copyList;
				break;
				
			case "Paste after":
				Undo.RecordObject (_target, "Paste actions");
				List<AC.Action> pasteList = AdvGame.copiedActions;
				_target.actions.InsertRange (i+1, pasteList);
				AdvGame.DuplicateActionsBuffer ();
				break;

			case "Insert end":
				Undo.RecordObject (_target, "Create action");
				AC.Action newAction = ActionList.GetDefaultAction ();
				_target.actions.Add (newAction);
				break;
				
			case "Insert after":
				Undo.RecordObject (_target, "Create action");
				_target.actions.Insert (i+1, ActionList.GetDefaultAction ());
				break;
				
			case "Delete":
				Undo.RecordObject (_target, "Delete action");
				_target.actions.Remove (_action);
				break;
				
			case "Move to top":
				Undo.RecordObject (_target, "Move action to top");
				_target.actions[0].nodeRect.x += 30f;
				_target.actions[0].nodeRect.y += 30f;
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
		

		public static void PushNodes (List<AC.Action> list, float xPoint, int count)
		{
			foreach (AC.Action action in list)
			{
				if (action.nodeRect.x > xPoint)
				{
					action.nodeRect.x += 350 * count;
				}
			}
		}
		
		
		public static List<AC.Action> ResizeList (List<AC.Action> list, int listSize)
		{
			ActionsManager actionsManager = AdvGame.GetReferences ().actionsManager;
			
			string defaultAction = "";
			
			if (actionsManager)
			{
				defaultAction = actionsManager.GetDefaultAction ();
			}
			
			if (list.Count < listSize)
			{
				// Increase size of list
				while (list.Count < listSize)
				{
					List<int> idArray = new List<int>();
					
					foreach (AC.Action _action in list)
					{
						idArray.Add (_action.id);
					}
					
					idArray.Sort ();
					
					list.Add ((AC.Action) CreateInstance (defaultAction));
					
					// Update id based on array
					foreach (int _id in idArray.ToArray())
					{
						if (list [list.Count -1].id == _id)
							list [list.Count -1].id ++;
					}
				}
			}
			else if (list.Count > listSize)
			{
				// Decrease size of list
				while (list.Count > listSize)
				{
					list.RemoveAt (list.Count - 1);
				}
			}
			
			return (list);
		}


		private void ShowParametersGUI (ActionList _target)
		{
			if (_target is AC_Trigger)
			{
				if (_target.parameters.Count != 1)
				{
					ActionParameter newParameter = new ActionParameter (0);
					newParameter.parameterType = ParameterType.GameObject;
					newParameter.label = "Collision object";
					_target.parameters.Clear ();
					_target.parameters.Add (newParameter);
				}
				return;
			}

			EditorGUILayout.Space ();
			EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Parameters", EditorStyles.boldLabel);
			ActionListEditor.ShowParametersGUI (_target.parameters);

			EditorGUILayout.EndVertical ();
		}


		public static int[] GetParameterIDArray (List<ActionParameter> parameters)
		{
			List<int> idArray = new List<int>();
			foreach (ActionParameter _parameter in parameters)
			{
				idArray.Add (_parameter.ID);
			}
			idArray.Sort ();
			return idArray.ToArray ();
		}


		public static void ShowParametersGUI (List<ActionParameter> parameters)
		{
			int numParameters = parameters.Count;
			numParameters = EditorGUILayout.IntField ("Number of parameters:", numParameters);
			if (numParameters < 0)
			{
				numParameters = 0;
			}
			
			if (numParameters < parameters.Count)
			{
				parameters.RemoveRange (numParameters, parameters.Count - numParameters);
			}
			else if (numParameters > parameters.Count)
			{
				if (numParameters > parameters.Capacity)
				{
					parameters.Capacity = numParameters;
				}
				for (int i=parameters.Count; i<numParameters; i++)
				{
					ActionParameter newParameter = new ActionParameter (ActionListEditor.GetParameterIDArray (parameters));
					parameters.Add (newParameter);
				}
			}
			
			foreach (ActionParameter _parameter in parameters)
			{
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField (_parameter.ID.ToString (), GUILayout.Width (10f));
				_parameter.label = EditorGUILayout.TextField (_parameter.label);
				_parameter.parameterType = (ParameterType) EditorGUILayout.EnumPopup (_parameter.parameterType);
				if (GUILayout.Button ("-"))
				{
					parameters.Remove (_parameter);
					break;
				}
				EditorGUILayout.EndHorizontal ();
			}
		}


		public static void ResetList (ActionList _target)
		{
			if (_target.actions.Count == 0 || (_target.actions.Count == 1 && _target.actions[0] == null))
			{
				_target.actions.Clear ();
				AC.Action newAction = ActionList.GetDefaultAction ();
				_target.actions.Add (newAction);
			}
		}
	}

}