/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionsManager.cs"
 * 
 *	This script handles the "Scene" tab of the main wizard.
 *	It is used to create the prefabs needed to run the game,
 *	as well as provide easy-access to game logic.
 * 
 */

using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using System;
using UnityEditor;
using System.Collections.Generic;
#endif

namespace AC
{

	[System.Serializable]
	public class SceneManager : ScriptableObject
	{
		
		#if UNITY_EDITOR

		public int selectedSceneObject;
		private string[] prefabTextArray;

		public int activeScenePrefab;
		private List<ScenePrefab> scenePrefabs;

		public static string assetFolder = "Assets/AdventureCreator/Prefabs/";
			
		private string newFolderName = "";
		private string newPrefabName;

		private bool positionHotspotOverMesh = false;

		private static GUILayoutOption
			buttonWidth = GUILayout.MaxWidth(120f);


		public void ShowGUI ()
		{
			GUILayout.Label ("Basic structure", EditorStyles.boldLabel);

			if (GUILayout.Button ("Organise room objects"))
			{
				InitialiseObjects ();
			}

			if (AdvGame.GetReferences ().settingsManager == null)
			{
				EditorGUILayout.HelpBox ("No Settings Manager defined - cannot display full Editor without it!", MessageType.Warning);
				return;
			}

			if (KickStarter.sceneSettings == null)
			{
				return;
			}
			
			SettingsManager settingsManager = AdvGame.GetReferences ().settingsManager;

			EditorGUILayout.BeginHorizontal ();
			newFolderName = EditorGUILayout.TextField (newFolderName);
				
			if (GUILayout.Button ("Create new folder", buttonWidth))
			{
				if (newFolderName != "")
				{
					GameObject newFolder = new GameObject();
					
					if (!newFolderName.StartsWith ("_"))
						newFolder.name = "_" + newFolderName;
					else
						newFolder.name = newFolderName;
						
					Undo.RegisterCreatedObjectUndo (newFolder, "Create folder " + newFolder.name);
					
					if (Selection.activeGameObject)
					{
						newFolder.transform.parent = Selection.activeGameObject.transform;
					}
					
					Selection.activeObject = newFolder;
				}
			}
			EditorGUILayout.EndHorizontal ();
			EditorGUILayout.Space ();

			GUILayout.Label ("Scene settings", EditorStyles.boldLabel);
			KickStarter.sceneSettings.navigationMethod = (AC_NavigationMethod) EditorGUILayout.EnumPopup ("Pathfinding method:", KickStarter.sceneSettings.navigationMethod);
			KickStarter.navigationManager.ResetEngine ();
			if (KickStarter.navigationManager.navigationEngine != null)
			{
				KickStarter.navigationManager.navigationEngine.SceneSettingsGUI ();
			}

			if (settingsManager.IsUnity2D () && KickStarter.sceneSettings.navigationMethod != AC_NavigationMethod.PolygonCollider)
			{
				EditorGUILayout.HelpBox ("This pathfinding method is not compatible with 'Unity 2D'.", MessageType.Warning);
			}

			EditorGUILayout.BeginHorizontal ();
			KickStarter.sceneSettings.defaultPlayerStart = (PlayerStart) EditorGUILayout.ObjectField ("Default PlayerStart:", KickStarter.sceneSettings.defaultPlayerStart, typeof (PlayerStart), true);
			if (KickStarter.sceneSettings.defaultPlayerStart == null)
			{
				if (GUILayout.Button ("Create", GUILayout.MaxWidth (60f)))
				{
					PlayerStart newPlayerStart = AddPrefab ("Navigation", "PlayerStart", true, false, true).GetComponent <PlayerStart>();
					newPlayerStart.gameObject.name = "Default PlayerStart";
					KickStarter.sceneSettings.defaultPlayerStart = newPlayerStart;
				}
			}
			EditorGUILayout.EndHorizontal ();
			if (KickStarter.sceneSettings.defaultPlayerStart)
			{
				EditorGUILayout.BeginHorizontal ();
				KickStarter.sceneSettings.defaultPlayerStart.cameraOnStart = (_Camera) EditorGUILayout.ObjectField ("Default Camera:", KickStarter.sceneSettings.defaultPlayerStart.cameraOnStart, typeof (_Camera), true);
				if (KickStarter.sceneSettings.defaultPlayerStart.cameraOnStart == null)
				{
					if (GUILayout.Button ("Create", GUILayout.MaxWidth (60f)))
					{
						if (settingsManager == null || settingsManager.cameraPerspective == CameraPerspective.ThreeD)
						{
							GameCamera newCamera = AddPrefab ("Camera", "GameCamera", true, false, true).GetComponent <GameCamera>();
							newCamera.gameObject.name = "NavCam 1";
							KickStarter.sceneSettings.defaultPlayerStart.cameraOnStart = newCamera;
						}
						else if (settingsManager.cameraPerspective == CameraPerspective.TwoD)
						{
							GameCamera2D newCamera = AddPrefab ("Camera", "GameCamera2D", true, false, true).GetComponent <GameCamera2D>();
							newCamera.gameObject.name = "NavCam 1";
							KickStarter.sceneSettings.defaultPlayerStart.cameraOnStart = newCamera;
						}
						else if (settingsManager.cameraPerspective == CameraPerspective.TwoPointFiveD)
						{
							GameCamera25D newCamera = AddPrefab ("Camera", "GameCamera2.5D", true, false, true).GetComponent <GameCamera25D>();
							newCamera.gameObject.name = "NavCam 1";
							KickStarter.sceneSettings.defaultPlayerStart.cameraOnStart = newCamera;
						}
					}
				}
				EditorGUILayout.EndHorizontal ();
			}
			EditorGUILayout.BeginHorizontal ();
			KickStarter.sceneSettings.sortingMap = (SortingMap) EditorGUILayout.ObjectField ("Default Sorting map:", KickStarter.sceneSettings.sortingMap, typeof (SortingMap), true);
			if (KickStarter.sceneSettings.sortingMap == null)
			{
				if (GUILayout.Button ("Create", GUILayout.MaxWidth (60f)))
				{
					SortingMap newSortingMap = AddPrefab ("Navigation", "SortingMap", true, false, true).GetComponent <SortingMap>();
					newSortingMap.gameObject.name = "Default SortingMap";
					KickStarter.sceneSettings.sortingMap = newSortingMap;
				}
			}
			EditorGUILayout.EndHorizontal ();
			EditorGUILayout.BeginHorizontal ();
			KickStarter.sceneSettings.defaultSound = (Sound) EditorGUILayout.ObjectField ("Default Sound prefab:", KickStarter.sceneSettings.defaultSound, typeof (Sound), true);
			if (KickStarter.sceneSettings.defaultSound == null)
			{
				if (GUILayout.Button ("Create", GUILayout.MaxWidth (60f)))
				{
					Sound newSound = AddPrefab ("Logic", "Sound", true, false, true).GetComponent <Sound>();
					newSound.gameObject.name = "Default Sound";
					KickStarter.sceneSettings.defaultSound = newSound;
					newSound.playWhilePaused = true;
				}
			}
			EditorGUILayout.EndHorizontal ();

			GUILayout.Label ("Scene cutscenes", EditorStyles.boldLabel);
			EditorGUILayout.BeginHorizontal ();
			KickStarter.sceneSettings.cutsceneOnStart = (Cutscene) EditorGUILayout.ObjectField ("On start:", KickStarter.sceneSettings.cutsceneOnStart, typeof (Cutscene), true);
			if (KickStarter.sceneSettings.cutsceneOnStart == null)
			{
				if (GUILayout.Button ("Create", GUILayout.MaxWidth (60f)))
				{
					Cutscene newCutscene = AddPrefab ("Logic", "Cutscene", true, false, true).GetComponent <Cutscene>();
					newCutscene.gameObject.name = "OnStart";
					KickStarter.sceneSettings.cutsceneOnStart = newCutscene;
				}
			}
			EditorGUILayout.EndHorizontal ();
			EditorGUILayout.BeginHorizontal ();
				KickStarter.sceneSettings.cutsceneOnLoad = (Cutscene) EditorGUILayout.ObjectField ("On load:", KickStarter.sceneSettings.cutsceneOnLoad, typeof (Cutscene), true);
				if (KickStarter.sceneSettings.cutsceneOnLoad == null)
				{
					if (GUILayout.Button ("Create", GUILayout.MaxWidth (60f)))
					{
						Cutscene newCutscene = AddPrefab ("Logic", "Cutscene", true, false, true).GetComponent <Cutscene>();
						newCutscene.gameObject.name = "OnLoad";
						KickStarter.sceneSettings.cutsceneOnLoad = newCutscene;
					}
				}
			EditorGUILayout.EndHorizontal ();
			EditorGUILayout.BeginHorizontal ();
				KickStarter.sceneSettings.cutsceneOnVarChange = (Cutscene) EditorGUILayout.ObjectField ("On variable change:", KickStarter.sceneSettings.cutsceneOnVarChange, typeof (Cutscene), true);
				if (KickStarter.sceneSettings.cutsceneOnVarChange == null)
				{
					if (GUILayout.Button ("Create", GUILayout.MaxWidth (60f)))
					{
						Cutscene newCutscene = AddPrefab ("Logic", "Cutscene", true, false, true).GetComponent <Cutscene>();
						newCutscene.gameObject.name = "OnVarChange";
						KickStarter.sceneSettings.cutsceneOnVarChange = newCutscene;
					}
				}
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.Space ();
		
			GUILayout.Label ("Visibility", EditorStyles.boldLabel);

			GUILayout.BeginHorizontal ();
				GUILayout.Label ("Triggers", buttonWidth);
				if (GUILayout.Button ("On", EditorStyles.miniButtonLeft))
				{
					SetTriggerVisibility (true);
				}
				if (GUILayout.Button ("Off", EditorStyles.miniButtonRight))
				{
					SetTriggerVisibility (false);
				}
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
				GUILayout.Label ("Collision", buttonWidth);
				if (GUILayout.Button ("On", EditorStyles.miniButtonLeft))
				{
					SetCollisionVisiblity (true);
				}
				if (GUILayout.Button ("Off", EditorStyles.miniButtonRight))
				{
					SetCollisionVisiblity (false);
				}
			GUILayout.EndHorizontal ();
			
			GUILayout.BeginHorizontal ();
				GUILayout.Label ("Hotspots", buttonWidth);
				if (GUILayout.Button ("On", EditorStyles.miniButtonLeft))
				{
					SetHotspotVisibility (true);
				}
				if (GUILayout.Button ("Off", EditorStyles.miniButtonRight))
				{
					SetHotspotVisibility (false);
				}
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			GUILayout.Label ("NavMesh", buttonWidth);
			if (GUILayout.Button ("On", EditorStyles.miniButtonLeft))
			{
				KickStarter.navigationManager.navigationEngine.SetVisibility (true);
			}
			if (GUILayout.Button ("Off", EditorStyles.miniButtonRight))
			{
				KickStarter.navigationManager.navigationEngine.SetVisibility (false);
			}
			GUILayout.EndHorizontal ();

			ListPrefabs ();

			if (GUI.changed)
			{
				EditorUtility.SetDirty (KickStarter.sceneSettings);
				EditorUtility.SetDirty (KickStarter.playerMovement);
				if (KickStarter.sceneSettings.defaultPlayerStart)
				{
					EditorUtility.SetDirty (KickStarter.sceneSettings.defaultPlayerStart);
				}
			}
		}
		
		
		private void PrefabButton (string subFolder, string prefabName)
		{
			if (GUILayout.Button (prefabName))
			{
				AddPrefab (subFolder, prefabName, true, true, true);
			}	
		}


		private void PrefabButton (string subFolder, string prefabName, Texture icon)
		{
			if (GUILayout.Button (icon))
			{
				AddPrefab (subFolder, prefabName, true, true, true);
			}	
		}

		
		public void InitialiseObjects ()
		{
			CreateFolder ("_Cameras");
			CreateFolder ("_Cutscenes");
			CreateFolder ("_DialogueOptions");
			CreateFolder ("_Interactions");
			CreateFolder ("_Lights");
			CreateFolder ("_Logic");
			CreateFolder ("_Moveables");
			CreateFolder ("_Navigation");
			CreateFolder ("_NPCs");
			CreateFolder ("_Sounds");
			CreateFolder ("_SetGeometry");
			CreateFolder ("_UI");

			// Create subfolders
			CreateSubFolder ("_Cameras", "_GameCameras");

			CreateSubFolder ("_Logic", "_ArrowPrompts");
			CreateSubFolder ("_Logic", "_Conversations");
			CreateSubFolder ("_Logic", "_Containers");
			CreateSubFolder ("_Logic", "_Hotspots");
			CreateSubFolder ("_Logic", "_Triggers");

			CreateSubFolder ("_Moveables", "_Tracks");
			
			CreateSubFolder ("_Navigation", "_CollisionCubes");
			CreateSubFolder ("_Navigation", "_CollisionCylinders");
			CreateSubFolder ("_Navigation", "_Markers");
			CreateSubFolder ("_Navigation", "_NavMeshSegments");
			CreateSubFolder ("_Navigation", "_NavMesh");
			CreateSubFolder ("_Navigation", "_Paths");
			CreateSubFolder ("_Navigation", "_PlayerStarts");
			CreateSubFolder ("_Navigation", "_SortingMaps");

			// Delete default main camera
			if (GameObject.FindWithTag (Tags.mainCamera))
			{
				GameObject mainCam = GameObject.FindWithTag (Tags.mainCamera);
				if (mainCam.GetComponent <MainCamera>() == null)
				{
					DestroyImmediate (mainCam);
				}
			}
			
			// Create main camera
			SettingsManager settingsManager = AdvGame.GetReferences ().settingsManager;
			GameObject mainCamOb = AddPrefab ("Automatic", "MainCamera", false, false, false);
			PrefabUtility.DisconnectPrefabInstance (mainCamOb);
			PutInFolder (GameObject.FindWithTag (Tags.mainCamera), "_Cameras");
			if (settingsManager && settingsManager.IsUnity2D ())
			{
				Camera.main.orthographic = true;
			}
			
			// Create Background Camera (if 2.5D)
			if (settingsManager && settingsManager.cameraPerspective == CameraPerspective.TwoPointFiveD)
			{
				CreateSubFolder ("_SetGeometry", "_BackgroundImages");
				GameObject newOb = AddPrefab ("Automatic", "BackgroundCamera", false, false, false);
				PutInFolder (newOb, "_Cameras");
			}

			// Create Game engine
			AddPrefab ("Automatic", "GameEngine", false, false, false);
			
			// Assign Player Start
			if (KickStarter.sceneSettings && KickStarter.sceneSettings.defaultPlayerStart == null)
			{
				string playerStartPrefab = "PlayerStart";
				if (settingsManager != null && settingsManager.IsUnity2D ())
				{
					playerStartPrefab += "2D";
				}

				PlayerStart playerStart = AddPrefab ("Navigation", playerStartPrefab, true, false, true).GetComponent <PlayerStart>();
				KickStarter.sceneSettings.defaultPlayerStart = playerStart;
			}

			// Pathfinding method
			if (settingsManager != null && settingsManager.IsUnity2D ())
			{
				KickStarter.sceneSettings.navigationMethod = AC_NavigationMethod.PolygonCollider;
				KickStarter.navigationManager.ResetEngine ();
			}
		}

		
		private void SetHotspotVisibility (bool isVisible)
		{
			Hotspot[] hotspots = FindObjectsOfType (typeof (Hotspot)) as Hotspot[];
			Undo.RecordObjects (hotspots, "Hotspot visibility");

			foreach (Hotspot hotspot in hotspots)
			{
				hotspot.showInEditor = isVisible;
				EditorUtility.SetDirty (hotspot);
			}
		}


		private void SetCollisionVisiblity (bool isVisible)
		{
			_Collision[] colls = FindObjectsOfType (typeof (_Collision)) as _Collision[];
			Undo.RecordObjects (colls, "Collision visibility");
			
			foreach (_Collision coll in colls)
			{
				coll.showInEditor = isVisible;
				EditorUtility.SetDirty (coll);
			}
		}

		
		private void SetTriggerVisibility (bool isVisible)
		{
			AC_Trigger[] triggers = FindObjectsOfType (typeof (AC_Trigger)) as AC_Trigger[];
			Undo.RecordObjects (triggers, "Trigger visibility");
			
			foreach (AC_Trigger trigger in triggers)
			{
				trigger.showInEditor = isVisible;
				EditorUtility.SetDirty (trigger);
			}
		}
		
		
		public static void RenameObject (GameObject ob, string resourceName)
		{
			ob.name = AdvGame.GetName (resourceName);
		}
		

		public static GameObject AddPrefab (string folderName, string prefabName, bool canCreateMultiple, bool selectAfter, bool putInFolder)
		{
			if (canCreateMultiple || !GameObject.Find (AdvGame.GetName (prefabName)))
			{
				string fileName = assetFolder + folderName + Path.DirectorySeparatorChar.ToString () + prefabName + ".prefab";

				GameObject newOb = (GameObject) PrefabUtility.InstantiatePrefab (AssetDatabase.LoadAssetAtPath (fileName, typeof (GameObject)));
				newOb.name = "Temp";

				if (folderName != "" && putInFolder)
				{
					if (!PutInFolder (newOb, "_" + prefabName + "s"))
					{
						string newName = "_" + prefabName;
						
						if (newName.Contains ("2D"))
						{
							newName = newName.Substring (0, newName.IndexOf ("2D"));
							
							if (!PutInFolder (newOb, newName + "s"))
							{
								PutInFolder (newOb, newName);
							}
							else
							{
								PutInFolder (newOb, newName);
							}
						}
						else if (newName.Contains ("2.5D"))
						{
							newName = newName.Substring (0, newName.IndexOf ("2.5D"));
							
							if (!PutInFolder (newOb, newName + "s"))
							{
								PutInFolder (newOb, newName);
							}
							else
							{
								PutInFolder (newOb, newName);
							}
						}
						else if (newName.Contains ("Animated"))
						{
							newName = newName.Substring (0, newName.IndexOf ("Animated"));
							
							if (!PutInFolder (newOb, newName + "s"))
							{
								PutInFolder (newOb, newName);
							}
							else
							{
								PutInFolder (newOb, newName);
							}
						}
						else if (newName.Contains ("ThirdPerson"))
						{
							newName = newName.Substring (0, newName.IndexOf ("ThirdPerson"));
							
							if (!PutInFolder (newOb, newName + "s"))
							{
								PutInFolder (newOb, newName);
							}
							else
							{
								PutInFolder (newOb, newName);
							}
						}
						else
						{
							PutInFolder (newOb, newName);
						}
					}
				}

				if (newOb.GetComponent <GameCamera2D>())
				{
					newOb.GetComponent <GameCamera2D>().SetCorrectRotation ();
				}
				
				RenameObject (newOb, prefabName);
				Undo.RegisterCreatedObjectUndo (newOb, "Created " + newOb.name);
				
				// Select the object
				if (selectAfter)
				{
					Selection.activeObject = newOb;
				}
				
				return newOb;
			}

			return null;
		}
		

		public static bool PutInFolder (GameObject ob, string folderName)
		{
			if (ob && GameObject.Find (folderName))
			{
				if (GameObject.Find (folderName).transform.position == Vector3.zero && folderName.Contains ("_"))
				{
					ob.transform.parent = GameObject.Find (folderName).transform;

					return true;
				}
			}
			
			return false;
		}
		

		private void CreateFolder (string folderName)
		{
			if (!GameObject.Find (folderName))
			{
				GameObject newFolder = new GameObject();
				newFolder.name = folderName;
				Undo.RegisterCreatedObjectUndo (newFolder, "Created " + newFolder.name);
			}
		}
		
		
		private void CreateSubFolder (string baseFolderName, string subFolderName)
		{
			CreateFolder (baseFolderName);

			if (!GameObject.Find (subFolderName))
			{
				GameObject newFolder = new GameObject ();
				newFolder.name = subFolderName;
				Undo.RegisterCreatedObjectUndo (newFolder, "Created " + newFolder.name);

				if (newFolder != null && GameObject.Find (baseFolderName))
				{
					newFolder.transform.parent = GameObject.Find (baseFolderName).transform;
				}
				else
				{
					Debug.Log ("Folder " + baseFolderName + " does not exist!");
				}
			}
		}


		private ScenePrefab GetActiveScenePrefab ()
		{
			if (scenePrefabs == null || scenePrefabs.Count <= activeScenePrefab)
			{
				DeclareScenePrefabs ();
			}

			if (scenePrefabs.Count < activeScenePrefab)
			{
				activeScenePrefab = 0;
			}

			return scenePrefabs[activeScenePrefab];
		}


		private void ListPrefabs ()
		{
			if (scenePrefabs == null || GUI.changed)
			{
				DeclareScenePrefabs ();
				GetPrefabsInScene ();
			}

			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Scene prefabs", EditorStyles.boldLabel);

			EditorGUILayout.BeginVertical ("Button");

			GUILayout.BeginHorizontal ();
			GUIContent prefabHeader = new GUIContent ("  " + GetActiveScenePrefab ().subCategory, GetActiveScenePrefab ().icon);
			EditorGUILayout.LabelField (prefabHeader, EditorStyles.boldLabel, GUILayout.Height (40f));

			EditorGUILayout.HelpBox (GetActiveScenePrefab ().helpText, MessageType.Info);
			GUILayout.EndHorizontal ();

			EditorGUILayout.Space ();

			GUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("New prefab name:", GUILayout.Width (120f));
			newPrefabName = EditorGUILayout.TextField (newPrefabName);

			if (GUILayout.Button ("Add new", GUILayout.Width (60f)))
			{
				string fileName = assetFolder + GetActiveScenePrefab ().prefabPath + ".prefab";
				GameObject newOb = (GameObject) PrefabUtility.InstantiatePrefab (AssetDatabase.LoadAssetAtPath (fileName, typeof (GameObject)));

				if (newPrefabName != null && newPrefabName != "" && newPrefabName.Length > 0)
				{
					newOb.name = newPrefabName;
					newPrefabName = "";
				}

				Undo.RegisterCreatedObjectUndo (newOb, "Created " + newOb.name);
				PutInFolder (newOb, GetActiveScenePrefab ().sceneFolder);

				if (CanWrapHotspot () && positionHotspotOverMesh)
				{
					positionHotspotOverMesh = false;

					Renderer r = Selection.activeGameObject.GetComponent <Renderer>();
					newOb.transform.position = r.bounds.center;
					newOb.transform.localScale = r.bounds.size;
				}

				Selection.activeGameObject = newOb;
				GetPrefabsInScene ();
			}
			GUILayout.EndHorizontal ();

			if (CanWrapHotspot ())
			{
				positionHotspotOverMesh = EditorGUILayout.ToggleLeft ("Position over selected mesh?", positionHotspotOverMesh);
			}

			EditorGUILayout.Space ();

			if (GUI.changed || prefabTextArray == null)
			{
				GetPrefabsInScene ();
			}

			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Existing " + GetActiveScenePrefab ().subCategory + " prefabs:");
			EditorGUILayout.BeginHorizontal ();
				selectedSceneObject = EditorGUILayout.Popup (selectedSceneObject, prefabTextArray);

				if (GUILayout.Button ("Select", EditorStyles.miniButtonLeft))
				{
					if (Type.GetType ("AC." + GetActiveScenePrefab ().componentName) != null)
					{
						MonoBehaviour[] objects = FindObjectsOfType (Type.GetType ("AC." + GetActiveScenePrefab ().componentName)) as MonoBehaviour [];
						if (objects != null && objects.Length > selectedSceneObject && objects[selectedSceneObject].gameObject != null)
						{
							Selection.activeGameObject = objects[selectedSceneObject].gameObject;
						}
					}
					else if (GetActiveScenePrefab ().componentName != "")
					{
						MonoBehaviour[] objects = FindObjectsOfType (Type.GetType (GetActiveScenePrefab ().componentName)) as MonoBehaviour [];
						if (objects != null && objects.Length > selectedSceneObject && objects[selectedSceneObject].gameObject != null)
						{
							Selection.activeGameObject = objects[selectedSceneObject].gameObject;
						}
					}
					
				}
				if (GUILayout.Button ("Refresh", EditorStyles.miniButtonRight))
				{
					GetPrefabsInScene ();
				}
			EditorGUILayout.EndHorizontal ();
			EditorGUILayout.EndVertical ();

			EditorGUILayout.Space ();
			ListAllPrefabs ("Camera");
			ListAllPrefabs ("Logic");
			ListAllPrefabs ("Moveable");
			ListAllPrefabs ("Navigation");
		}


		private void ListAllPrefabs (string _category)
		{
			GUISkin testSkin = (GUISkin) Resources.Load ("SceneManagerSkin");
			GUI.skin = testSkin;
			bool isEven = false;

			EditorGUILayout.LabelField (_category);

			EditorGUILayout.BeginHorizontal ();

			foreach (ScenePrefab prefab in scenePrefabs)
			{
				if (prefab.category == _category)
				{
					isEven = !isEven;

					if (prefab.icon)
					{
						if (GUILayout.Button (new GUIContent (" " + prefab.subCategory, prefab.icon)))
						{
							GUI.skin = null;
							ClickPrefabButton (prefab);
							GUI.skin = testSkin;
						}
					}
					else
					{
						if (GUILayout.Button (new GUIContent (" " + prefab.subCategory)))
						{
							GUI.skin = null;
							ClickPrefabButton (prefab);
							GUI.skin = testSkin;
						}
					}

					if (!isEven)
					{
						EditorGUILayout.EndHorizontal ();
						EditorGUILayout.BeginHorizontal ();
					}
				}
			}

			EditorGUILayout.EndHorizontal ();

			GUI.skin = null;
		}


		private void ClickPrefabButton (ScenePrefab _prefab)
		{
			if (activeScenePrefab == scenePrefabs.IndexOf (_prefab))
			{
				// Clicked twice, add new
				string fileName = assetFolder + _prefab.prefabPath + ".prefab";
				GameObject newOb = (GameObject) PrefabUtility.InstantiatePrefab (AssetDatabase.LoadAssetAtPath (fileName, typeof (GameObject)));
				Undo.RegisterCreatedObjectUndo (newOb, "Created " + newOb.name);

				if (newOb.GetComponent <GameCamera2D>())
				{
					newOb.GetComponent <GameCamera2D>().SetCorrectRotation ();
				}

				PutInFolder (newOb, _prefab.sceneFolder);
				EditorGUIUtility.PingObject (newOb);
			}

			activeScenePrefab = scenePrefabs.IndexOf (_prefab);
			GetPrefabsInScene ();
		}


		private bool CanWrapHotspot ()
		{
			if (Selection.activeGameObject != null && GetActiveScenePrefab ().subCategory.Contains ("Hotspot") && Selection.activeGameObject.GetComponent <Renderer>())
			{
				return true;
			}
			return false;
		}


		private void DeclareScenePrefabs ()
		{
			scenePrefabs = new List<ScenePrefab>();
			SettingsManager settingsManager = AdvGame.GetReferences ().settingsManager;

			if (settingsManager == null || settingsManager.cameraPerspective == CameraPerspective.ThreeD)
			{
				scenePrefabs.Add (new ScenePrefab ("Camera", "GameCamera", "Camera/GameCamera", "_GameCameras", "The standard camera type for 3D games.", "GameCamera"));
				scenePrefabs.Add (new ScenePrefab ("Camera", "GameCamera Animated", "Camera/GameCameraAnimated", "_GameCameras", "Plays an Animation Clip when active, or syncs it with its target's position.", "GameCameraAnimated"));
				scenePrefabs.Add (new ScenePrefab ("Camera", "GameCamera Third-person", "Camera/GameCameraThirdPerson", "_GameCameras", "Rigidly follows its target, but can still be rotated.", "GameCameraThirdPerson"));
				scenePrefabs.Add (new ScenePrefab ("Camera", "SimpleCamera", "Camera/SimpleCamera", "_GameCameras", "A stationary but lightweight 3D camera.", "GameCamera"));
			}
			else
			{
				if (settingsManager.cameraPerspective == CameraPerspective.TwoD)
				{
					scenePrefabs.Add (new ScenePrefab ("Camera", "GameCamera 2D", "Camera/GameCamera2D", "_GameCameras", "The standard camera type for 2D games.", "GameCamera2D"));
				}
				else
				{
					scenePrefabs.Add (new ScenePrefab ("Camera", "GameCamera 2.5D", "Camera/GameCamera2.5D", "_GameCameras", "A stationary camera that can display images in the background.", "GameCamera25D"));
					scenePrefabs.Add (new ScenePrefab ("Camera", "Background Image", "SetGeometry/BackgroundImage", "_BackgroundImages", "A container for a 2.5D camera's background image.", "BackgroundImage"));
					scenePrefabs.Add (new ScenePrefab ("Camera", "Scene sprite", "SetGeometry/SceneSprite", "_SetGeometry", "An in-scene sprite for 2.5D games.", "", "SceneSprite"));
				}
			}

			scenePrefabs.Add (new ScenePrefab ("Logic", "Arrow prompt", "Logic/ArrowPrompt", "_ArrowPrompts", "An on-screen directional prompt for the player.", "ArrowPrompt"));
			scenePrefabs.Add (new ScenePrefab ("Logic", "Conversation", "Logic/Conversation", "_Conversations", "Stores a list of Dialogue Options, from which the player can choose.", "Conversation"));
			scenePrefabs.Add (new ScenePrefab ("Logic", "Container", "Logic/Container", "_Containers", "Can store a list of Inventory Items, for the player to retrieve and add to.", "Container"));
			scenePrefabs.Add (new ScenePrefab ("Logic", "Cutscene", "Logic/Cutscene", "_Cutscenes", "A sequence of Actions that can form a cinematic.", "Cutscene"));
			scenePrefabs.Add (new ScenePrefab ("Logic", "Dialogue Option", "Logic/DialogueOption", "_DialogueOptions", "An option available to the player when a Conversation is active.", "DialogueOption"));

			if (settingsManager != null && settingsManager.IsUnity2D ())
			{
				scenePrefabs.Add (new ScenePrefab ("Logic", "Hotspot 2D", "Logic/Hotspot2D", "_Hotspots", "A portion of the scene that can be interacted with.", "Hotspot"));
			}
			else
			{
				scenePrefabs.Add (new ScenePrefab ("Logic", "Hotspot", "Logic/Hotspot", "_Hotspots", "A portion of the scene that can be interacted with.", "Hotspot"));
			}

			scenePrefabs.Add (new ScenePrefab ("Logic", "Interaction", "Logic/Interaction", "_Interactions", "A sequence of Actions that run when a Hotspot is activated.", "Interaction"));
			scenePrefabs.Add (new ScenePrefab ("Logic", "Sound", "Logic/Sound", "_Sounds", "An audio source that syncs with AC's sound levels.", "Sound"));

			if (settingsManager != null && settingsManager.IsUnity2D ())
			{
				scenePrefabs.Add (new ScenePrefab ("Logic", "Trigger 2D", "Logic/Trigger2D", "_Triggers", "A portion of the scene that responds to objects entering it.", "AC_Trigger"));
			}
			else
			{
				scenePrefabs.Add (new ScenePrefab ("Logic", "Trigger", "Logic/Trigger", "_Triggers", "A portion of the scene that responds to objects entering it.", "AC_Trigger"));
			}

			scenePrefabs.Add (new ScenePrefab ("Moveable", "Draggable", "Moveable/Draggable", "_Moveables", "Can move along pre-defined Tracks, along planes, or be rotated about its centre.", "Moveable_Drag"));
			scenePrefabs.Add (new ScenePrefab ("Moveable", "PickUp", "Moveable/PickUp", "_Moveables", "Can be grabbed, rotated and thrown freely in 3D space.", "Moveable_PickUp"));
			scenePrefabs.Add (new ScenePrefab ("Moveable", "Straight Track", "Moveable/StraightTrack", "_Tracks", "Constrains a Drag object along a straight line, optionally adding rolling or screw effects.", "DragTrack_Straight"));
			scenePrefabs.Add (new ScenePrefab ("Moveable", "Curved Track", "Moveable/CurvedTrack", "_Tracks", "Constrains a Drag object along a circular line.", "DragTrack_Curved"));
			scenePrefabs.Add (new ScenePrefab ("Moveable", "Hinge Track", "Moveable/HingeTrack", "_Tracks", "Constrains a Drag object's position, only allowing it to rotate in a circular motion.", "DragTrack_Hinge"));

			scenePrefabs.Add (new ScenePrefab ("Navigation", "SortingMap", "Navigation/SortingMap", "_SortingMaps", "Defines how sprites are scaled and sorted relative to one another.", "SortingMap"));

			if (settingsManager != null && settingsManager.IsUnity2D ())
			{
				scenePrefabs.Add (new ScenePrefab ("Navigation", "Collision Cube 2D", "Navigation/CollisionCube2D", "_CollisionCubes", "Blocks Character movement, as well as cursor clicks if placed on the Default layer.", "_Collision"));
				scenePrefabs.Add (new ScenePrefab ("Navigation", "Marker 2D", "Navigation/Marker2D", "_Markers", "A point in the scene used by Characters and objects.", "Marker"));
			}
			else
			{
				scenePrefabs.Add (new ScenePrefab ("Navigation", "Collision Cube", "Navigation/CollisionCube", "_CollisionCubes", "Blocks Character movement, as well as cursor clicks if placed on the Default layer.", "_Collision"));
				scenePrefabs.Add (new ScenePrefab ("Navigation", "Collision Cylinder", "Navigation/CollisionCylinder", "_CollisionCylinders", "Blocks Character movement, as well as cursor clicks if placed on the Default layer.", "_Collision"));
				scenePrefabs.Add (new ScenePrefab ("Navigation", "Marker", "Navigation/Marker", "_Markers", "A point in the scene used by Characters and objects.", "Marker"));
			}

			if (KickStarter.sceneSettings)
			{
				AC_NavigationMethod engine = KickStarter.sceneSettings.navigationMethod;
				if (engine == AC_NavigationMethod.meshCollider)
				{
					scenePrefabs.Add (new ScenePrefab ("Navigation", "NavMesh", "Navigation/NavMesh", "_NavMesh", "A mesh that defines the area that Characters can move in.", "NavigationMesh"));
				}
				else if (engine == AC_NavigationMethod.PolygonCollider)
				{
					scenePrefabs.Add (new ScenePrefab ("Navigation", "NavMesh 2D", "Navigation/NavMesh2D", "_NavMesh", "A polygon that defines the area that Characters can move in.", "NavigationMesh"));
				}
				else if (engine == AC_NavigationMethod.UnityNavigation)
				{
					scenePrefabs.Add (new ScenePrefab ("Navigation", "NavMesh segment", "Navigation/NavMeshSegment", "_NavMeshSegments", "A plane that defines a portion of the area that Characters can move in.", "NavMeshSegment"));
					scenePrefabs.Add (new ScenePrefab ("Navigation", "Static obstacle", "Navigation/StaticObstacle", "_NavMeshSegments", "A cube that defines a portion of the area that Characters cannot move in.", "", "StaticObstacle"));
				}
			}

			scenePrefabs.Add (new ScenePrefab ("Navigation", "Path", "Navigation/Path", "_Paths", "A sequence of points that describe a Character's movement.", "Paths"));
		
			if (settingsManager != null && settingsManager.IsUnity2D ())
			{
				scenePrefabs.Add (new ScenePrefab ("Navigation", "PlayerStart 2D", "Navigation/PlayerStart2D", "_PlayerStarts", "A point in the scene from which the Player begins.", "PlayerStart"));
			}
			else
			{
				scenePrefabs.Add (new ScenePrefab ("Navigation", "PlayerStart", "Navigation/PlayerStart", "_PlayerStarts", "A point in the scene from which the Player begins.", "PlayerStart"));
			}
		}


		public void GetPrefabsInScene ()
		{
			List<string> titles = new List<string>();
			MonoBehaviour[] objects;
			int i=1;

			if (Type.GetType ("AC." + GetActiveScenePrefab ().componentName) != null)
			{
				objects = FindObjectsOfType (Type.GetType ("AC." + GetActiveScenePrefab ().componentName)) as MonoBehaviour [];
				foreach (MonoBehaviour _object in objects)
				{
					titles.Add (i.ToString () + ": " + _object.gameObject.name);
					i++;
				}
			}
			else if (GetActiveScenePrefab ().componentName != "")
			{
				objects = FindObjectsOfType (Type.GetType (GetActiveScenePrefab ().componentName)) as MonoBehaviour [];
				foreach (MonoBehaviour _object in objects)
				{
					titles.Add (i.ToString () + ": " + _object.gameObject.name);
					i++;
				}
			}

			if (i == 1)
			{
				titles.Add ("(None found in scene)");
			}

			prefabTextArray = titles.ToArray ();
		}

		#endif

	}


	#if UNITY_EDITOR

	public struct ScenePrefab
	{

		public string category;
		public string subCategory;
		public string prefabPath;
		public string sceneFolder;
		public string helpText;
		public string componentName;
		public Texture2D icon;


		public ScenePrefab (string _category, string _subCategory, string _prefabPath, string _sceneFolder, string _helpText, string _componentName, string _graphicName = "")
		{
			category = _category;
			subCategory = _subCategory;
			prefabPath = _prefabPath;
			sceneFolder = _sceneFolder;
			helpText = _helpText;
			componentName = _componentName;

			if (_graphicName != "")
			{
				icon = (Texture2D) AssetDatabase.LoadAssetAtPath ("Assets/AdventureCreator/Graphics/PrefabIcons/" + _graphicName +  ".png", typeof (Texture2D));
			}
			else
			{
				icon = (Texture2D) AssetDatabase.LoadAssetAtPath ("Assets/AdventureCreator/Graphics/PrefabIcons/" + _componentName +  ".png", typeof (Texture2D));
			}

			if (_subCategory == "Collision Cylinder")
			{
				icon = (Texture2D) AssetDatabase.LoadAssetAtPath ("Assets/AdventureCreator/Graphics/PrefabIcons/" + _componentName +  "Cylinder.png", typeof (Texture2D));
			}
		}

	}

	#endif

}