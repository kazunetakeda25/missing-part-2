using UnityEngine;
using UnityEditor;

namespace AC
{

	public class NewGameWizardWindow : EditorWindow
	{

		private string gameName = "";

		private int cameraPerspective_int;
		private string[] cameraPerspective_list = { "2D", "2.5D", "3D" };
		private bool screenSpace = false;
		private bool oneScenePerBackground = false;
		private MovementMethod movementMethod = MovementMethod.PointAndClick;

		private InputMethod inputMethod = InputMethod.MouseAndKeyboard;
		private AC_InteractionMethod interactionMethod = AC_InteractionMethod.ContextSensitive;
		private HotspotDetection hotspotDetection = HotspotDetection.MouseOver;

		public bool directControl;
		public bool touchScreen;
		public WizardMenu wizardMenu = WizardMenu.DefaultAC;

		private int pageNumber = 0;
		private References references;

		private int numPages = 6;
		private Texture2D logo = (Texture2D) AssetDatabase.LoadAssetAtPath ("Assets/AdventureCreator/Graphics/Textures/logo.png", typeof (Texture2D));

		// To process
		private CameraPerspective cameraPerspective = CameraPerspective.ThreeD;
		private MovingTurning movingTurning = MovingTurning.Unity2D;


		[MenuItem ("Adventure Creator/Getting started/New Game wizard", false, 4)]
		public static void Init ()
		{
			NewGameWizardWindow window = (NewGameWizardWindow) EditorWindow.GetWindow (typeof (NewGameWizardWindow));
			window.GetReferences ();
			window.title = "New Game Wizard";
			window.position = new Rect (300, 200, 350, 300);
		}
		
		
		private void GetReferences ()
		{
			references = (References) Resources.Load (Resource.references);
		}


		public void OnInspectorUpdate()
		{
			Repaint();
		}


		private void OnGUI ()
		{
			GUILayout.Label (GetTitle (), EditorStyles.largeLabel);
			if (GetTitle () != "")
			{
				EditorGUILayout.Separator ();
				GUILayout.Space (10f);
			}

			ShowPage ();

			GUILayout.Space (15f);
			GUILayout.BeginHorizontal ();
			if (pageNumber < 1)
			{
				if (pageNumber < 0)
				{
					pageNumber = 0;
				}
				GUI.enabled = false;
			}
			if (pageNumber < numPages)
			{
				if (GUILayout.Button ("Previous", EditorStyles.miniButtonLeft))
				{
					pageNumber --;
				}
			}
			else
			{
				if (GUILayout.Button ("Restart", EditorStyles.miniButtonLeft))
				{
					pageNumber = 1;
					gameName = "";
				}
			}
			GUI.enabled = true;
			if (pageNumber < numPages - 1)
			{
				if (pageNumber == 1 && gameName == "")
				{
					GUI.enabled = false;
				}
				if (GUILayout.Button ("Next", EditorStyles.miniButtonRight))
				{
					pageNumber ++;
					if (pageNumber == numPages - 1)
					{
						Process ();
					}
				}
				GUI.enabled = true;
			}
			else
			{
				if (pageNumber == numPages)
				{
					GUI.enabled = false;
				}
				if (GUILayout.Button ("Finish", EditorStyles.miniButtonRight))
				{
					pageNumber ++;
					Finish ();
				}
				GUI.enabled = true;
			}
			GUILayout.EndHorizontal ();

			GUILayout.Label ("Page " + (pageNumber + 1) + " of " + (numPages + 1));
		}


		private string GetTitle ()
		{
			if (pageNumber == 1)
			{
				return "Game name";
			}
			else if (pageNumber == 2)
			{
				return "Camera perspective";
			}
			else if (pageNumber == 3)
			{
				return "Interface";
			}
			else if (pageNumber == 4)
			{
				return "GUI system";
			}
			else if (pageNumber == 5)
			{
				return "Confirm choices";
			}
			else if (pageNumber == 6)
			{
				return "Complete";
			}

			return "";
		}


		private void Finish ()
		{
			if (!references)
			{
				GetReferences ();
			}
			
			if (!references)
			{
				return;
			}

			string managerPath = gameName + "/Managers";
			try
			{
				System.IO.Directory.CreateDirectory (Application.dataPath + "/" + managerPath);
			}
			catch
			{
				Debug.LogWarning ("Could not create directory: " + Application.dataPath + "/" + managerPath);
				pageNumber --;
				return;
			}

			try
			{
				ScriptableObject t = CustomAssetUtility.CreateAsset<SceneManager> ("SceneManager", managerPath);
				AssetDatabase.RenameAsset ("Assets/" + managerPath + "/SceneManager.asset", gameName + "_SceneManager");
				references.sceneManager = (SceneManager) t;

				t = CustomAssetUtility.CreateAsset<SettingsManager> ("SettingsManager", managerPath);
				AssetDatabase.RenameAsset ("Assets/" + managerPath + "/SettingsManager.asset", gameName + "_SettingsManager");
				references.settingsManager = (SettingsManager) t;

				references.settingsManager.saveFileName = gameName;
				references.settingsManager.cameraPerspective = cameraPerspective;
				references.settingsManager.movementMethod = movementMethod;
				references.settingsManager.inputMethod = inputMethod;
				references.settingsManager.interactionMethod = interactionMethod;
				references.settingsManager.hotspotDetection = hotspotDetection;
				references.settingsManager.movingTurning = movingTurning;
				if (cameraPerspective == CameraPerspective.TwoPointFiveD)
				{
					references.settingsManager.forceAspectRatio = true;
				}

				t = CustomAssetUtility.CreateAsset<ActionsManager> ("ActionsManager", managerPath);
				AssetDatabase.RenameAsset ("Assets/" + managerPath + "/ActionsManager.asset", gameName + "_ActionsManager");
				references.actionsManager = (ActionsManager) t;
				AdventureCreator.RefreshActions ();
				ActionsManager demoActionsManager = AssetDatabase.LoadAssetAtPath("Assets/AdventureCreator/Demo/Managers/Demo_ActionsManager.asset", typeof(ActionsManager)) as ActionsManager;
				if (demoActionsManager != null)
				{
					references.actionsManager.defaultClass = demoActionsManager.defaultClass;
				}

				t = CustomAssetUtility.CreateAsset<VariablesManager> ("VariablesManager", managerPath);
				AssetDatabase.RenameAsset ("Assets/" + managerPath + "/VariablesManager.asset", gameName + "_VariablesManager");
				references.variablesManager = (VariablesManager) t;

				t = CustomAssetUtility.CreateAsset<InventoryManager> ("InventoryManager", managerPath);
				AssetDatabase.RenameAsset ("Assets/" + managerPath + "/InventoryManager.asset", gameName + "_InventoryManager");
				references.inventoryManager = (InventoryManager) t;

				t = CustomAssetUtility.CreateAsset<SpeechManager> ("SpeechManager", managerPath);
				AssetDatabase.RenameAsset ("Assets/" + managerPath + "/SpeechManager.asset", gameName + "_SpeechManager");
				references.speechManager = (SpeechManager) t;

				references.speechManager.ClearLanguages ();

				t = CustomAssetUtility.CreateAsset<CursorManager> ("CursorManager", managerPath);
				AssetDatabase.RenameAsset ("Assets/" + managerPath + "/CursorManager.asset", gameName + "_CursorManager");
				references.cursorManager = (CursorManager) t;

				t = CustomAssetUtility.CreateAsset<MenuManager> ("MenuManager", managerPath);
				AssetDatabase.RenameAsset ("Assets/" + managerPath + "/MenuManager.asset", gameName + "_MenuManager");
				references.menuManager = (MenuManager) t;

				if (wizardMenu != WizardMenu.Blank)
				{
					CursorManager demoCursorManager = AssetDatabase.LoadAssetAtPath("Assets/AdventureCreator/Demo/Managers/Demo_CursorManager.asset", typeof(CursorManager)) as CursorManager;
					if (demoCursorManager != null)
					{
						foreach (CursorIcon demoIcon in demoCursorManager.cursorIcons)
						{
							CursorIcon newIcon = new CursorIcon ();
							newIcon.Copy (demoIcon);
							references.cursorManager.cursorIcons.Add (newIcon);
						}
					}
					else
					{
						Debug.LogWarning ("Cannot find Demo_CursorManager asset to copy from!");
					}	
					references.cursorManager.allowMainCursor = true;

					CursorIconBase pointerIcon = new CursorIconBase ();
					pointerIcon.Copy (demoCursorManager.pointerIcon);
					references.cursorManager.pointerIcon = pointerIcon;
					EditorUtility.SetDirty (references.cursorManager);

					MenuManager demoMenuManager = AssetDatabase.LoadAssetAtPath("Assets/AdventureCreator/Demo/Managers/Demo_MenuManager.asset", typeof(MenuManager)) as MenuManager;
					if (demoMenuManager != null)
					{
						references.menuManager.drawOutlines = demoMenuManager.drawOutlines;
						references.menuManager.drawInEditor = demoMenuManager.drawInEditor;
						references.menuManager.pauseTexture = demoMenuManager.pauseTexture;

						if (wizardMenu == WizardMenu.DefaultUnityUI)
						{
							System.IO.Directory.CreateDirectory (Application.dataPath + "/" + gameName + "/UI");
						}

						foreach (Menu demoMenu in demoMenuManager.menus)
						{
							Menu newMenu = ScriptableObject.CreateInstance <Menu>();
							newMenu.Copy (demoMenu);

							if (wizardMenu == WizardMenu.DefaultAC)
							{
								newMenu.menuSource = MenuSource.AdventureCreator;
							}
							else if (wizardMenu == WizardMenu.DefaultUnityUI)
							{
								newMenu.menuSource = MenuSource.UnityUiPrefab;

								if (demoMenu.canvas)
								{
									string oldCanvasPath = AssetDatabase.GetAssetPath (demoMenu.canvas);
									string newCanvasPath = "Assets/" + gameName + "/UI/" + demoMenu.canvas.name + ".prefab";
									if (AssetDatabase.CopyAsset (oldCanvasPath, newCanvasPath))
									{
										AssetDatabase.ImportAsset (newCanvasPath);
										newMenu.canvas = (Canvas) AssetDatabase.LoadAssetAtPath (newCanvasPath, typeof (Canvas));
									}
								}
							}

							newMenu.hideFlags = HideFlags.HideInHierarchy;
							references.menuManager.menus.Add (newMenu);
							EditorUtility.SetDirty (references.menuManager);
							foreach (MenuElement newElement in newMenu.elements)
							{
								newElement.hideFlags = HideFlags.HideInHierarchy;
								AssetDatabase.AddObjectToAsset (newElement, references.menuManager);
							}
							AssetDatabase.AddObjectToAsset (newMenu, references.menuManager);
						}
					}
					else
					{
						Debug.LogWarning ("Cannot find Demo_MenuManager asset to copy from!");
					}
				}

				CreateManagerPackage (gameName);

				AssetDatabase.SaveAssets ();
				references.sceneManager.InitialiseObjects ();
				pageNumber = 0;
			}
			catch
			{
				Debug.LogWarning ("Could not create Manager. Does the subdirectory " + Resource.managersDirectory + " exist?");
				pageNumber --;
			}
		}


		private void Process ()
		{
			if (cameraPerspective_int == 0)
			{
				cameraPerspective = CameraPerspective.TwoD;
				if (screenSpace)
				{
					movingTurning = MovingTurning.ScreenSpace;
				}
				else
				{
					movingTurning = MovingTurning.Unity2D;
				}

				movementMethod = MovementMethod.PointAndClick;
				inputMethod = InputMethod.MouseAndKeyboard;
				hotspotDetection = HotspotDetection.MouseOver;
			}
			else if (cameraPerspective_int == 1)
			{
				if (oneScenePerBackground)
				{
					cameraPerspective = CameraPerspective.TwoD;
					movingTurning = MovingTurning.ScreenSpace;
					movementMethod = MovementMethod.PointAndClick;
					inputMethod = InputMethod.MouseAndKeyboard;
					hotspotDetection = HotspotDetection.MouseOver;
				}
				else
				{
					cameraPerspective = CameraPerspective.TwoPointFiveD;

					if (directControl)
					{
						movementMethod = MovementMethod.Direct;
						inputMethod = InputMethod.KeyboardOrController;
						hotspotDetection = HotspotDetection.PlayerVicinity;
					}
					else
					{
						movementMethod = MovementMethod.PointAndClick;
						inputMethod = InputMethod.MouseAndKeyboard;
						hotspotDetection = HotspotDetection.MouseOver;
					}
				}
			}
			else if (cameraPerspective_int == 2)
			{
				cameraPerspective = CameraPerspective.ThreeD;
				hotspotDetection = HotspotDetection.MouseOver;

				inputMethod = InputMethod.MouseAndKeyboard;
				if (movementMethod == MovementMethod.Drag)
				{
					if (touchScreen)
					{
						inputMethod = InputMethod.TouchScreen;
					}
					else
					{
						inputMethod = InputMethod.MouseAndKeyboard;
					}
				}
			}
		}


		private void ShowPage ()
		{
			GUI.skin.label.wordWrap = true;

			if (pageNumber == 0)
			{
				if (logo != null)
				{
					GUILayout.Label (logo);
				}
				GUILayout.Space (5f);
				GUILayout.Label ("This window can help you get started with making a new Adventure Creator game.");
				GUILayout.Label ("To begin, click 'Next'. Changes will not be implemented until you are finished.");
			}

			else if (pageNumber == 1)
			{
				GUILayout.Label ("Enter a name for your game. This will be used for filenames, so alphanumeric characters only.");
				gameName = GUILayout.TextField (gameName);
			}
			
			else if (pageNumber == 2)
			{
				GUILayout.Label ("What kind of perspective will your game have?");
				cameraPerspective_int = EditorGUILayout.Popup (cameraPerspective_int, cameraPerspective_list);

				if (cameraPerspective_int == 0)
				{
					GUILayout.Space (5f);
					GUILayout.Label ("By default, 2D games are built entirely in the X-Z plane, and characters are scaled to achieve a depth effect.\nIf you prefer, you can position your characters in 3D space, so that they scale accurately due to camera perspective.");
					screenSpace = EditorGUILayout.ToggleLeft ("I'll position my characters in 3D space", screenSpace);
				}
				else if (cameraPerspective_int == 1)
				{
					GUILayout.Space (5f);
					GUILayout.Label ("2.5D games mixes 3D characters with 2D backgrounds. By default, 2.5D games group several backgrounds into one scene, and swap them out according to the camera angle.\nIf you prefer, you can work with just one background in a scene, to create a more traditional 2D-like adventure.");
					oneScenePerBackground = EditorGUILayout.ToggleLeft ("I'll work with one background per scene", oneScenePerBackground);
				}
				else if (cameraPerspective_int == 2)
				{
					GUILayout.Label ("3D games can still have sprite-based Characters, but having a true 3D environment is more flexible so far as Player control goes. How should your Player character be controlled?");
					movementMethod = (MovementMethod) EditorGUILayout.EnumPopup (movementMethod);
				}
			}

			else if (pageNumber == 3)
			{
				if (cameraPerspective_int == 1 && !oneScenePerBackground)
				{
					GUILayout.Label ("Do you want to play the game ONLY with a keyboard or controller?");
					directControl = EditorGUILayout.ToggleLeft ("Yes", directControl);
					GUILayout.Space (5f);
				}
				else if (cameraPerspective_int == 2 && movementMethod == MovementMethod.Drag)
				{
					GUILayout.Label ("Is your game designed for Touch-screen devices?");
					touchScreen = EditorGUILayout.ToggleLeft ("Yes", touchScreen);
					GUILayout.Space (5f);
				}

				GUILayout.Label ("How do you want to interact with Hotspots?");
				interactionMethod = (AC_InteractionMethod) EditorGUILayout.EnumPopup (interactionMethod);
				if (interactionMethod == AC_InteractionMethod.ContextSensitive)
				{
					EditorGUILayout.HelpBox ("This method simplifies interactions to either Use, Examine, or Use Inventory. Hotspots can be interacted with in just one click.", MessageType.Info);
				}
				else if (interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot)
				{
					EditorGUILayout.HelpBox ("This method emulates the classic 'Sierra-style' interface, in which the player chooses from a list of verbs, and then the Hotspot they wish to interact with.", MessageType.Info);
				}
				else if (interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction)
				{
					EditorGUILayout.HelpBox ("This method involves first choosing a Hotspot, and then from a range of available interactions, which can be customised in the Editor.", MessageType.Info);
				}
			}

			else if (pageNumber == 4)
			{
				GUILayout.Label ("Please choose what interface you would like to start with. It can be changed at any time - this is just to help you get started.");
				wizardMenu = (WizardMenu) EditorGUILayout.EnumPopup (wizardMenu);
			}

			else if (pageNumber == 5)
			{
				GUILayout.Label ("The following values have been set based on your choices. Please review them and amend if necessary, then click 'Finish' to create your game template.");
				GUILayout.Space (5f);

				gameName = EditorGUILayout.TextField ("Game name:", gameName);
				cameraPerspective_int = (int) cameraPerspective;
				cameraPerspective_int = EditorGUILayout.Popup ("Camera perspective:", cameraPerspective_int, cameraPerspective_list);
				cameraPerspective = (CameraPerspective) cameraPerspective_int;

				if (cameraPerspective == CameraPerspective.TwoD)
				{
					movingTurning = (MovingTurning) EditorGUILayout.EnumPopup ("Moving and turning:", movingTurning);
				}

				movementMethod = (MovementMethod) EditorGUILayout.EnumPopup ("Movement method:", movementMethod);
				inputMethod = (InputMethod) EditorGUILayout.EnumPopup ("Input method:", inputMethod);
				interactionMethod = (AC_InteractionMethod) EditorGUILayout.EnumPopup ("Interaction method:", interactionMethod);
				hotspotDetection = (HotspotDetection) EditorGUILayout.EnumPopup ("Hotspot detection method:", hotspotDetection);

				wizardMenu = (WizardMenu) EditorGUILayout.EnumPopup ("GUI type:", wizardMenu);
			}

			else if (pageNumber == 6)
			{
				GUILayout.Label ("Congratulations, your game's Managers have been set up!");
				GUILayout.Space (5f);
				GUILayout.Label ("Your scene objects have also been organised for Adventure Creator to use. Your next step is to create and set your Player prefab, which you can assign in your Settings Manager.");
			}
		}


		private void CreateManagerPackage (string folder)
		{
			ManagerPackage managerPackage = CustomAssetUtility.CreateAsset<ManagerPackage> ("ManagerPackage", folder);
			AssetDatabase.RenameAsset ("Assets/" + folder + "/ManagerPackage.asset", folder + "_ManagerPackage");

			managerPackage.sceneManager = references.sceneManager;
			managerPackage.settingsManager = references.settingsManager;
			managerPackage.actionsManager = references.actionsManager;
			managerPackage.variablesManager = references.variablesManager;

			managerPackage.inventoryManager = references.inventoryManager;
			managerPackage.speechManager = references.speechManager;
			managerPackage.cursorManager = references.cursorManager;
			managerPackage.menuManager = references.menuManager;

			EditorUtility.SetDirty (managerPackage);
			AssetDatabase.SaveAssets ();
		}

	}

}