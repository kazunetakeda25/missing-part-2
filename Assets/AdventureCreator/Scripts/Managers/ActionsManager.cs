/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionsManager.cs"
 * 
 *	This script handles the "Actions" tab of the main wizard.
 *	Custom actions can be added and removed by selecting them with this.
 * 
 */

using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{
	
	[System.Serializable]
	public class ActionsManager : ScriptableObject
	{
		
		#if UNITY_EDITOR
		public string customFolderPath = "AdventureCreator/Scripts/Actions";
		public string folderPath = "AdventureCreator/Scripts/Actions";
		#endif
		
		public bool displayActionsInInspector = true;
		public DisplayActionsInEditor displayActionsInEditor = DisplayActionsInEditor.ArrangedVertically;
		public bool allowMultipleActionListWindows = false;
		public ActionListEditorScrollWheel actionListEditorScrollWheel = ActionListEditorScrollWheel.PansWindow;
		public bool invertPanning = false;
		
		public int defaultClass;
		
		public List<ActionType> AllActions = new List<ActionType>();
		public List<ActionType> EnabledActions = new List<ActionType>();

		#if UNITY_EDITOR
		private ActionType selectedClass = null;
		private List<ActionListAsset> searchedAssets = new List<ActionListAsset>();
		#endif

		
		public string GetDefaultAction ()
		{
			if (EnabledActions.Count > 0 && EnabledActions.Count > defaultClass)
			{
				return EnabledActions[defaultClass].fileName;
			}
			
			return "";
		}
		
		
		#if UNITY_EDITOR

		public void ShowGUI ()
		{
			EditorGUILayout.BeginVertical ("Button");
			GUILayout.Label ("Actionlist editing settings", EditorStyles.boldLabel);
			displayActionsInInspector = EditorGUILayout.ToggleLeft ("List Actions in Inspector window?", displayActionsInInspector);
			displayActionsInEditor = (DisplayActionsInEditor) EditorGUILayout.EnumPopup ("Actions in Editor are:", displayActionsInEditor);
			actionListEditorScrollWheel = (ActionListEditorScrollWheel) EditorGUILayout.EnumPopup ("Using scroll-wheel:", actionListEditorScrollWheel);
			invertPanning = EditorGUILayout.ToggleLeft ("Invert panning in ActionList Editor?", invertPanning);
			allowMultipleActionListWindows = EditorGUILayout.ToggleLeft ("Allow multiple ActionList Editor windows?", allowMultipleActionListWindows);
			EditorGUILayout.EndVertical ();
			EditorGUILayout.Space ();
			
			EditorGUILayout.BeginVertical ("Button");
			GUILayout.Label ("Custom Action scripts", EditorStyles.boldLabel);

			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Folder to search:", GUILayout.Width (110f));
			GUILayout.Label (customFolderPath, EditorStyles.textField);
			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Set directory"))
			{
				string path = EditorUtility.OpenFolderPanel("Set custom Actions directory", "Assets", "");
				string dataPath = Application.dataPath;
				if (path.Contains (dataPath))
				{
					if (path == dataPath)
					{
						customFolderPath = "";
					}
					else
					{
						customFolderPath = path.Replace (dataPath + "/", "");
					}
				}
				else
				{
					Debug.LogError ("Cannot set new directory - be sure to select within the Assets directory.");
				}
			}
			GUILayout.EndHorizontal ();
			EditorGUILayout.EndVertical ();
			
			if (AllActions.Count > 0)
			{
				GUILayout.Space (10);
				
				foreach (ActionType subclass in AllActions)
				{
					int enabledIndex = -1;
					if (EnabledActions.Contains (subclass))
					{
						enabledIndex = EnabledActions.IndexOf (subclass);
					}

					if (selectedClass != null && subclass.category == selectedClass.category && subclass.title == selectedClass.title)
					{
						EditorGUILayout.BeginVertical ("Button");
						SpeechLine.ShowField ("Name:", subclass.GetFullTitle (), false);
						SpeechLine.ShowField ("Filename:", subclass.fileName, false);
						SpeechLine.ShowField ("Description:", subclass.description, true);
						subclass.isEnabled = true; // This is being set OnEnable anyway, because Action Types are now refreshed/generated automatically, so can't disable
						//subclass.isEnabled = EditorGUILayout.Toggle ("Is enabled?", subclass.isEnabled);

						EditorGUILayout.BeginHorizontal ();
						if (enabledIndex >= 0)
						{
							if (enabledIndex == defaultClass)
							{
								EditorGUILayout.LabelField ("DEFAULT", EditorStyles.boldLabel, GUILayout.Width (70f));
							}
							else if (subclass.isEnabled)
							{
								if (GUILayout.Button ("Make default?"))
								{
									if (EnabledActions.Contains (subclass))
									{
										defaultClass = EnabledActions.IndexOf (subclass);
									}
								}
							}
						}

						if (GUILayout.Button ("Search local instances"))
						{
							SearchForInstances (true, subclass);
						}
						if (GUILayout.Button ("Search all instances"))
						{
							SearchForInstances (false, subclass);
						}

						EditorGUILayout.EndHorizontal ();
						EditorGUILayout.EndVertical ();
					}
					else
					{
						EditorGUILayout.BeginHorizontal ();
						if (GUILayout.Button (subclass.GetFullTitle (), EditorStyles.label, GUILayout.Width (200f)))
						{
							selectedClass = subclass;
						}
						if (enabledIndex >= 0 && enabledIndex == defaultClass)
						{
							EditorGUILayout.LabelField ("DEFAULT", EditorStyles.boldLabel, GUILayout.Width (60f));
						}
						EditorGUILayout.EndHorizontal ();
						GUILayout.Box ("", GUILayout.ExpandWidth (true), GUILayout.Height(1));
					}
				}

				if (defaultClass > EnabledActions.Count - 1)
				{
					defaultClass = EnabledActions.Count - 1;
				}
			}
			else
			{
				EditorGUILayout.HelpBox ("No Action subclass files found.", MessageType.Warning);
			}

			if (GUI.changed)
			{
				SetEnabled ();
				EditorUtility.SetDirty (this);
			}
		}


		private void SearchForInstances (bool justLocal, ActionType actionType)
		{
			if (searchedAssets != null)
			{
				searchedAssets.Clear ();
			}
			
			if (justLocal)
			{
				SearchSceneForType ("", actionType);
				return;
			}
			
			string[] sceneFiles = AdvGame.GetSceneFiles ();
			
			// First look for lines that already have an assigned lineID
			foreach (string sceneFile in sceneFiles)
			{
				SearchSceneForType (sceneFile, actionType);
			}
			
			// Settings
			if (KickStarter.settingsManager)
			{
				SearchAssetForType (KickStarter.settingsManager.actionListOnStart, actionType);
				if (KickStarter.settingsManager.activeInputs != null)
				{
					foreach (ActiveInput activeInput in KickStarter.settingsManager.activeInputs)
					{
						SearchAssetForType (activeInput.actionListAsset, actionType);
					}
				}
			}
			
			// Inventory
			if (KickStarter.inventoryManager)
			{
				SearchAssetForType (KickStarter.inventoryManager.unhandledCombine, actionType);
				SearchAssetForType (KickStarter.inventoryManager.unhandledHotspot, actionType);
				SearchAssetForType (KickStarter.inventoryManager.unhandledGive, actionType);
				
				// Item-specific events
				if (KickStarter.inventoryManager.items.Count > 0)
				{
					foreach (InvItem item in (KickStarter.inventoryManager.items))
					{
						SearchAssetForType (item.useActionList, actionType);
						SearchAssetForType (item.lookActionList, actionType);
						SearchAssetForType (item.unhandledActionList, actionType);
						SearchAssetForType (item.unhandledCombineActionList, actionType);
						
						foreach (ActionListAsset actionList in item.combineActionList)
						{
							SearchAssetForType (actionList, actionType);
						}
					}
				}
				
				foreach (Recipe recipe in KickStarter.inventoryManager.recipes)
				{
					SearchAssetForType (recipe.invActionList, actionType);
				}
			}
			
			// Cursor
			if (KickStarter.cursorManager)
			{
				// Prefixes
				foreach (ActionListAsset actionListAsset in KickStarter.cursorManager.unhandledCursorInteractions)
				{
					SearchAssetForType (actionListAsset, actionType);
				}
			}
			
			// Menus
			if (KickStarter.menuManager)
			{
				// Gather elements
				if (KickStarter.menuManager.menus.Count > 0)
				{
					foreach (AC.Menu menu in KickStarter.menuManager.menus)
					{
						SearchAssetForType (menu.actionListOnTurnOff, actionType);
						SearchAssetForType (menu.actionListOnTurnOn, actionType);
						
						foreach (MenuElement element in menu.elements)
						{
							if (element is MenuButton)
							{
								MenuButton button = (MenuButton) element;
								if (button.buttonClickType == AC_ButtonClickType.RunActionList)
								{
									SearchAssetForType (button.actionList, actionType);
								}
							}
							else if (element is MenuSavesList)
							{
								MenuSavesList button = (MenuSavesList) element;
								SearchAssetForType (button.actionListOnSave, actionType);
							}
						}
					}
				}
			}
			
			searchedAssets.Clear ();
		}
		
		
		private void SearchSceneForType (string sceneFile, ActionType actionType)
		{
			string sceneLabel = "";
			
			if (sceneFile != "")
			{
				sceneLabel = "(Scene: " + sceneFile + ") ";
				if (EditorApplication.currentScene != sceneFile)
				{
					EditorApplication.OpenScene (sceneFile);
				}
			}
			
			
			// Speech lines and journal entries
			ActionList[] actionLists = GameObject.FindObjectsOfType (typeof (ActionList)) as ActionList[];
			foreach (ActionList list in actionLists)
			{
				int numFinds = SearchActionsForType (list.GetActions (), actionType);
				if (numFinds > 0)
				{
					Debug.Log (sceneLabel + " Found " + numFinds + " instances in '" + list.gameObject.name + "'");
				}
			}
		}
		
		
		private void SearchAssetForType (ActionListAsset actionListAsset, ActionType actionType)
		{
			if (searchedAssets.Contains (actionListAsset))
			{
				return;
			}
			
			searchedAssets.Add (actionListAsset);
			if (actionListAsset != null)
			{
				int numFinds = SearchActionsForType (actionListAsset.actions, actionType);
				if (numFinds > 0)
				{
					Debug.Log ("(Asset: " + actionListAsset.name + ") Found " + numFinds + " instances of '" + actionType.GetFullTitle () + "'");
				}
			}
		}
		
		
		private int SearchActionsForType (List<Action> actionList, ActionType actionType)
		{
			if (actionList == null)
			{
				return 0;
			}
			int numFinds = 0;
			foreach (Action action in actionList)
			{
				if ((action.category == actionType.category && action.title == actionType.title) ||
				    (action.category == actionType.category && action.title.Contains (actionType.title)))
				{
					numFinds ++;
				}
			}
			
			return numFinds;
		}
		
		#endif
		
		
		public void SetEnabled ()
		{
			EnabledActions.Clear ();
			
			foreach (ActionType subclass in AllActions)
			{
				if (subclass.isEnabled)
				{
					EnabledActions.Add (subclass);
				}
			}
		}
		
		
		public string GetActionName (int i)
		{
			return (EnabledActions [i].fileName);
		}


		public bool DoesActionExist (string _name)
		{
			foreach (ActionType actionType in EnabledActions)
			{
				if (_name == actionType.fileName || _name == ("AC." + actionType.fileName))
				{
					return true;
				}
			}
			return false;
		}
		
		
		public int GetActionsSize ()
		{
			return (EnabledActions.Count);
		}


		public int GetActionTypeIndex (Action _action)
		{
			string className = _action.GetType ().ToString ();
			className = className.Replace ("AC.", "");
			foreach (ActionType actionType in EnabledActions)
			{
				if (actionType.fileName == className)
				{
					return EnabledActions.IndexOf (actionType);
				}
			}
			return defaultClass;
		}


		public int GetActionTypeIndex (ActionCategory _category, int subCategoryIndex)
		{
			List<ActionType> types = new List<ActionType>();
			foreach (ActionType type in EnabledActions)
			{
				if (type.category == _category)
				{
					types.Add (type);
				}
			}
			if (types.Count > subCategoryIndex)
			{
				return EnabledActions.IndexOf (types[subCategoryIndex]);
			}
			return 0;
		}
		
		
		public string[] GetActionTitles ()
		{
			List<string> titles = new List<string>();
			
			foreach (ActionType type in EnabledActions)
			{
				titles.Add (type.title);
			}
			
			return (titles.ToArray ());
		}

		
		public string[] GetActionSubCategories (ActionCategory _category)
		{
			List<string> titles = new List<string>();

			foreach (ActionType type in EnabledActions)
			{
				if (type.category == _category)
				{
					titles.Add (type.title);
				}
			}
			
			return (titles.ToArray ());
		}
		
		
		public ActionCategory GetActionCategory (int number)
		{
			if (EnabledActions.Count == 0 || EnabledActions.Count < number)
			{
				return 0;
			}
			return EnabledActions[number].category;
		}
		
		
		public int GetActionSubCategory (Action _action)
		{
			int i=0;
			string fileName = _action.GetType ().ToString ().Replace ("AC.", "");
			ActionCategory _category = _action.category;

			foreach (ActionType type in EnabledActions)
			{
				if (type.category == _category)
				{
					if (type.fileName == fileName)
					{
						return i;
					}
					i++;
				}
			}

			return 0;
		}

	}
	
}
