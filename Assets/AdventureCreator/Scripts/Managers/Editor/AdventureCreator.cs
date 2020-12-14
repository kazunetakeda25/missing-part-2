using UnityEngine;
using UnityEditor;
using System.IO;
using AC;

public class AdventureCreator : EditorWindow
{
	
	public References references;
	
	public static string version = "1.45a";
 
	private bool showScene = true;
	private bool showSettings = false;
	private bool showActions = false;
	private bool showGVars = false;
	private bool showInvItems = false;
	private bool showSpeech = false;
	private bool showCursor = false;
	private bool showMenu = false;
	
	private Vector2 scroll;
	
	private static GUILayoutOption tabWidth = GUILayout.MinWidth (60f);


	[MenuItem ("Adventure Creator/Editors/Game Editor")]
	static void Init ()
	{
		// Get existing open window or if none, make a new one:
		AdventureCreator window = (AdventureCreator) EditorWindow.GetWindow (typeof (AdventureCreator));
		window.GetReferences ();
		window.title = "Game Editor";
	}
	
	
	private void GetReferences ()
	{
		references = (References) Resources.Load (Resource.references);
	}


	private void OnEnable ()
	{
		RefreshActions ();
	}
	
	
	private void OnInspectorUpdate ()
	{
		Repaint ();
	}
	
	
	private void OnGUI ()
	{
		if (!references)
		{
			GetReferences ();
		}
		
		if (references)
		{
			GUILayout.Space (10);
	
			GUILayout.BeginHorizontal ();
			
				if (GUILayout.Toggle (showScene, "Scene", "toolbarbutton", tabWidth))
				{
					SetTab (0);
				}
				if (GUILayout.Toggle (showSettings, "Settings", "toolbarbutton", tabWidth)) 
				{
					SetTab (1);
				}
				if (GUILayout.Toggle (showActions, "Actions", "toolbarbutton", tabWidth))
				{
					SetTab (2);
				}
				if (GUILayout.Toggle (showGVars, "Variables", "toolbarbutton", tabWidth))
				{
					SetTab (3);
				}
			
			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal ();
			
				if (GUILayout.Toggle (showInvItems, "Inventory", "toolbarbutton", tabWidth))
				{
					SetTab (4);
				}
				if (GUILayout.Toggle (showSpeech, "Speech", "toolbarbutton", tabWidth))
				{
					SetTab (5);
				}
				if (GUILayout.Toggle (showCursor, "Cursor", "toolbarbutton", tabWidth))
				{
					SetTab (6);
				}
				if (GUILayout.Toggle (showMenu, "Menu", "toolbarbutton", tabWidth))
				{
					SetTab (7);
				}
	
			GUILayout.EndHorizontal ();
			GUILayout.Space (5);
			
			scroll = GUILayout.BeginScrollView (scroll);
			
			if (showScene)
			{
				GUILayout.Label ("Scene manager", EditorStyles.largeLabel);
				
				references.sceneManager = (SceneManager) EditorGUILayout.ObjectField ("Asset file: ", references.sceneManager, typeof (SceneManager), false);
				
				EditorGUILayout.Separator ();
				GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
				
				if (!references.sceneManager)
				{
					AskToCreate <SceneManager> ("SceneManager");
				}
				else
				{
					references.sceneManager.ShowGUI ();
				}
			}
			
			else if (showSettings)
			{
				GUILayout.Label ("Settings manager", EditorStyles.largeLabel);
				
				references.settingsManager = (SettingsManager) EditorGUILayout.ObjectField ("Asset file: ", references.settingsManager, typeof (SettingsManager), false);
				
				EditorGUILayout.Separator ();
				GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
				
				if (!references.settingsManager)
				{
					AskToCreate <SettingsManager> ("SettingsManager");
				}
				else
				{
					references.settingsManager.ShowGUI ();
				}
			}
			
			else if (showActions)
			{
				GUILayout.Label ("Actions manager", EditorStyles.largeLabel);
				
				references.actionsManager = (ActionsManager) EditorGUILayout.ObjectField ("Asset file: ", references.actionsManager, typeof (ActionsManager), false);
				
				EditorGUILayout.Separator ();
				GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
				
				if (!references.actionsManager)
				{
					AskToCreate <ActionsManager> ("ActionsManager");
				}
				else
				{
					references.actionsManager.ShowGUI ();
				}
			}
			
			else if (showGVars)
			{
				GUILayout.Label ("Variables manager", EditorStyles.largeLabel);
				
				references.variablesManager = (VariablesManager) EditorGUILayout.ObjectField ("Asset file: ", references.variablesManager, typeof (VariablesManager), false);
				
				EditorGUILayout.Separator ();
				GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
				
				if (!references.variablesManager)
				{
					AskToCreate <VariablesManager> ("VariablesManager");
				}
				else
				{
					references.variablesManager.ShowGUI ();
				}
			}
			
			else if (showInvItems)
			{
				GUILayout.Label ("Inventory items manager", EditorStyles.largeLabel);
				
				references.inventoryManager = (InventoryManager) EditorGUILayout.ObjectField ("Asset file: ", references.inventoryManager, typeof (InventoryManager), false);
				
				EditorGUILayout.Separator ();
				GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));

				if (!references.inventoryManager)
				{
					AskToCreate <InventoryManager> ("InventoryManager");
				}
				else
				{
					references.inventoryManager.ShowGUI ();
				}
			}
			
			else if (showSpeech)
			{
				GUILayout.Label ("Speech manager", EditorStyles.largeLabel);
				
				
				references.speechManager = (SpeechManager) EditorGUILayout.ObjectField ("Asset file: ", references.speechManager, typeof (SpeechManager), false);
				EditorGUILayout.Separator ();
				GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
				
				if (!references.speechManager)
				{
					AskToCreate <SpeechManager> ("SpeechManager");
				}
				else
				{
					references.speechManager.ShowGUI ();
				}
			}
			
			else if (showCursor)
			{
				GUILayout.Label ("Cursor manager", EditorStyles.largeLabel);
				
				
				references.cursorManager = (CursorManager) EditorGUILayout.ObjectField ("Asset file: ", references.cursorManager, typeof (CursorManager), false);
				EditorGUILayout.Separator ();
				GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
				
				if (!references.cursorManager)
				{
					AskToCreate <CursorManager> ("CursorManager");
				}
				else
				{
					references.cursorManager.ShowGUI ();
				}
			}
			
			else if (showMenu)
			{
				GUILayout.Label ("Menu manager", EditorStyles.largeLabel);
				
				
				references.menuManager = (MenuManager) EditorGUILayout.ObjectField ("Asset file: ", references.menuManager, typeof (MenuManager), false);
				EditorGUILayout.Separator ();
				GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
				
				if (!references.menuManager)
				{
					AskToCreate <MenuManager> ("MenuManager");
				}
				else
				{
					references.menuManager.ShowGUI ();
				}
			}

			EditorGUILayout.Separator ();
			GUILayout.Box ("", GUILayout.ExpandWidth (true), GUILayout.Height(1));
			GUILayout.Label ("Adventure Creator - Version " + AdventureCreator.version, EditorStyles.miniLabel);

			GUILayout.EndScrollView ();
		}
		else
		{
			EditorStyles.label.wordWrap = true;
			GUILayout.Label ("No 'References' asset found in the resources folder. Please click to create one.", EditorStyles.label);
			
			if (GUILayout.Button ("Create 'References' file"))
			{
				references = CustomAssetUtility.CreateAsset<References> ("References", "AdventureCreator" + Path.DirectorySeparatorChar.ToString () + "Resources");
			}
		}
		
		if (GUI.changed)
		{
			if (showActions)
			{
				RefreshActions ();
			}

			EditorUtility.SetDirty (this);
			EditorUtility.SetDirty (references);
		}
	}
	
	
	private void SetTab (int tab)
	{
		showScene = false;
		showSettings = false;
		showActions = false;
		showGVars = false;
		showInvItems = false;
		showSpeech = false;
		showCursor = false;
		showMenu = false;
		
		if (tab == 0)
		{
			showScene = true;
		}
		else if (tab == 1)
		{
			showSettings = true;
		}
		else if (tab == 2)
		{
			showActions = true;
		}
		else if (tab == 3)
		{
			showGVars = true;
		}
		else if (tab == 4)
		{
			showInvItems = true;
		}
		else if (tab == 5)
		{
			showSpeech = true;
		}
		else if (tab == 6)
		{
			showCursor = true;
		}
		else if (tab == 7)
		{
			showMenu = true;
		}
	}
	
	
	private void AskToCreate<T> (string obName) where T : ScriptableObject
	{
		EditorStyles.label.wordWrap = true;
		GUILayout.Label ("A '" + obName + "' asset is required for the game to run correctly.", EditorStyles.label);
		
		if (GUILayout.Button ("Create new " + obName + " file"))
		{
			try {
				ScriptableObject t = CustomAssetUtility.CreateAsset<T> (obName, Resource.managersDirectory);
				
				Undo.RecordObject (references, "Assign " + obName);
				
				if (t is SceneManager)
				{
					references.sceneManager = (SceneManager) t;
				}
				else if (t is SettingsManager)
				{
					references.settingsManager = (SettingsManager) t;
				}
				else if (t is ActionsManager)
				{
					references.actionsManager = (ActionsManager) t;
					RefreshActions ();
				}
				else if (t is VariablesManager)
				{
					references.variablesManager = (VariablesManager) t;
				}
				else if (t is InventoryManager)
				{
					references.inventoryManager = (InventoryManager) t;
				}
				else if (t is SpeechManager)
				{
					references.speechManager = (SpeechManager) t;
				}
				else if (t is CursorManager)
				{
					references.cursorManager = (CursorManager) t;
				}
				else if (t is MenuManager)
				{
					references.menuManager = (MenuManager) t;
				}
			}
			catch
			{
				Debug.LogWarning ("Could not create " + obName + ". Does the subdirectory " + Resource.managersDirectory + " exist?");
			}
		}
	}


	public static void RefreshActions ()
	{
		if (AdvGame.GetReferences () == null || AdvGame.GetReferences ().actionsManager == null)
		{
			return;
		}

		// Load default Actions
		ActionsManager actionsManager = AdvGame.GetReferences ().actionsManager;
		DirectoryInfo dir = new DirectoryInfo ("Assets/" + actionsManager.folderPath);
		FileInfo[] info = dir.GetFiles ("*.cs");
		
		actionsManager.AllActions.Clear ();
		
		foreach (FileInfo f in info) 
		{
			int extentionPosition = f.Name.IndexOf (".cs");
			string className = f.Name.Substring (0, extentionPosition);
			Action tempAction = (Action) CreateInstance (className);
			actionsManager.AllActions.Add (new ActionType (className, tempAction));
		}
		
		// Load custom Actions
		if (actionsManager.customFolderPath != actionsManager.folderPath)
		{
			dir = new DirectoryInfo ("Assets/" + actionsManager.customFolderPath);
			info = dir.GetFiles ("*.cs");
			
			foreach (FileInfo f in info) 
			{
				try
				{
					int extentionPosition = f.Name.IndexOf (".cs");
					string className = f.Name.Substring (0, extentionPosition);
					Action tempAction = (Action) CreateInstance (className);
					if (tempAction is Action)
					{
						actionsManager.AllActions.Add (new ActionType (className, tempAction));
					}
				}
				catch {}
			}
		}
		
		actionsManager.AllActions.Sort (delegate(ActionType i1, ActionType i2) { return i1.GetFullTitle (true).CompareTo(i2.GetFullTitle (true)); });
		actionsManager.SetEnabled ();
	}
	
}