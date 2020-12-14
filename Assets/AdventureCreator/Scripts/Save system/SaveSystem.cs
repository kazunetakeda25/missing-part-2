/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"SaveSystem.cs"
 * 
 *	This script processes saved game data to and from the scene objects.
 * 
 * 	It is partially based on Zumwalt's code here:
 * 	http://wiki.unity3d.com/index.php?title=Save_and_Load_from_XML
 *  and uses functions by Nitin Pande:
 *  http://www.eggheadcafe.com/articles/system.xml.xmlserialization.asp 
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace AC
{

	public class SaveSystem : MonoBehaviour
	{

		public LoadingGame loadingGame;
		public List<SaveFile> foundSaveFiles = new List<SaveFile>();
		public List<SaveFile> foundImportFiles = new List<SaveFile>();

		#if !UNITY_WEBPLAYER && !UNITY_WINRT
		private string saveDirectory;
		#endif
		
		private SaveData saveData;

		
		private void Awake ()
		{
			GatherSaveFiles ();

			saveDirectory = Application.persistentDataPath;
		}


		private void GatherSaveFiles ()
		{
			foundSaveFiles = new List<SaveFile>();

			for (int i=0; i<50; i++)
			{
				#if UNITY_WEBPLAYER || UNITY_WINRT
			
				if (PlayerPrefs.HasKey (GetProjectName () + "_" + i.ToString ()))
				{
					string label = "Save " + i.ToString ();
					if (i == 0)
					{
						label = "Autosave";
					}
					foundSaveFiles.Add (new SaveFile (i, label, null, ""));
				}
			
				#else
			
				SettingsManager settingsManager = KickStarter.settingsManager;
				string filename = saveDirectory + Path.DirectorySeparatorChar.ToString () + GetProjectName () + "_" + i.ToString () + GetSaveExtension ();
				if (File.Exists (filename))
				{

					string label = "Save " + i.ToString ();
					if (i == 0)
					{
						label = "Autosave";
					}

					if (settingsManager.saveTimeDisplay != SaveTimeDisplay.None)
					{
						DirectoryInfo dir = new DirectoryInfo (saveDirectory);
						FileInfo[] info = dir.GetFiles (GetProjectName () + "_" + i.ToString () + GetSaveExtension ());

						string creationTime = info [0].LastWriteTime.ToString ();
						if (settingsManager.saveTimeDisplay == SaveTimeDisplay.DateOnly)
						{
							creationTime = creationTime.Substring (0, creationTime.IndexOf (" "));
						}

						label += " (" + creationTime + ")";
					}

					Texture2D screenShot = null;
					if (settingsManager.takeSaveScreenshots)
					{
						screenShot = Serializer.LoadScreenshot (GetSaveScreenshotName (i));
					}

					foundSaveFiles.Add (new SaveFile (i, label, screenShot, filename));
				}

				#endif
			}
		}


		#if UNITY_STANDALONE

		private string GetImportDirectory (string importProjectName)
		{
			string[] s = Application.persistentDataPath.Split ('/');
			string currentProjectName = s[s.Length - 1];
			string importDirectory = saveDirectory.Replace (currentProjectName, importProjectName);
			return importDirectory;
		}

		#endif


		public void GatherImportFiles (string projectName, string saveFilename, int boolID)
		{
			#if !UNITY_STANDALONE
			Debug.LogWarning ("Cannot import save files unless running on Windows, Mac or Linux standalone platforms.");
			return;
			#else

			foundImportFiles = new List<SaveFile>();

			if (projectName == "" || saveFilename == "")
			{
				return;
			}
			string importDirectory = GetImportDirectory (projectName);
			SettingsManager settingsManager = KickStarter.settingsManager;
			
			for (int i=0; i<50; i++)
			{
				string filename = importDirectory + Path.DirectorySeparatorChar.ToString () + saveFilename + "_" + i.ToString () + GetSaveExtension ();
				if (File.Exists (filename))
				{
					if (boolID >= 0 && !DoImportCheck (filename, boolID))
					{
						continue;
					}

					string label = "Import " + i.ToString ();
					if (i == 0)
					{
						label = "Autosave";
					}

					if (settingsManager.saveTimeDisplay != SaveTimeDisplay.None)
					{
						DirectoryInfo dir = new DirectoryInfo (importDirectory);
						FileInfo[] info = dir.GetFiles (saveFilename + "_" + i.ToString () + GetSaveExtension ());
						
						string creationTime = info [0].LastWriteTime.ToString ();
						if (settingsManager.saveTimeDisplay == SaveTimeDisplay.DateOnly)
						{
							creationTime = creationTime.Substring (0, creationTime.IndexOf (" "));
						}
						
						label += " (" + creationTime + ")";
					}
					
					Texture2D screenShot = null;
					if (settingsManager.takeSaveScreenshots)
					{
						screenShot = Serializer.LoadScreenshot (GetImportScreenshotName (i, importDirectory, saveFilename));
					}
				
					foundImportFiles.Add (new SaveFile (i, label, screenShot, filename));
				}
			}
			#endif
		}


		private bool DoImportCheck (string filename, int boolID)
		{
			string allData = Serializer.LoadSaveFile (filename, false);
			if (allData.ToString () != "")
			{
				int divider = allData.IndexOf ("||");
				string mainData = allData.Substring (0, divider);
				SaveData tempSaveData = new SaveData ();

				if (SaveSystem.GetSaveMethod () == SaveMethod.XML)
				{
					tempSaveData = (SaveData) Serializer.DeserializeObjectXML <SaveData> (mainData);
				}
				else
				{
					tempSaveData = Serializer.DeserializeObjectBinary <SaveData> (mainData);
				}

				string varData = tempSaveData.mainData.runtimeVariablesData;
				if (varData.Length > 0)
				{
					string[] varsArray = varData.Split ("|"[0]);
					
					foreach (string chunk in varsArray)
					{
						string[] chunkData = chunk.Split (":"[0]);
						
						int _id = 0;
						int.TryParse (chunkData[0], out _id);

						if (_id == boolID)
						{
							int _value = 0;
							int.TryParse (chunkData[1], out _value);

							if (_value == 1)
							{
								return true;
							}
							return false;
						}
					}
				}
			}
			return false;
		}


		private IEnumerator TakeScreenshot (string fileName)
		{
			bool originalMenuState = KickStarter.stateHandler.menuIsOff;
			bool originalCursorState = KickStarter.stateHandler.cursorIsOff;
			KickStarter.stateHandler.menuIsOff = true;
			KickStarter.stateHandler.cursorIsOff = true;
			
			yield return new WaitForEndOfFrame ();
			
			Texture2D screenshotTex = new Texture2D (Screen.width, Screen.height, TextureFormat.RGB24, false);
			
			screenshotTex.ReadPixels (new Rect (0f, 0f, Screen.width, Screen.height), 0, 0);
			screenshotTex.Apply ();

			Serializer.SaveScreenshot (screenshotTex, fileName);
			Destroy (screenshotTex);
			
			KickStarter.stateHandler.menuIsOff = originalMenuState;
			KickStarter.stateHandler.cursorIsOff = originalCursorState;

			GatherSaveFiles ();
		}


		private string GetSaveExtension ()
		{
			if (GetSaveMethod () == SaveMethod.XML)
			{
				return ".savx";
			}
			return ".save";
		}


		public static SaveMethod GetSaveMethod ()
		{
			#if UNITY_IPHONE || UNITY_WP8 || UNITY_WINRT
			return SaveMethod.XML;
			#else
			return SaveMethod.Binary;
			#endif
		}


		public static bool DoesImportExist (int saveID)
		{
			if (KickStarter.saveSystem)
			{
				foreach (SaveFile file in KickStarter.saveSystem.foundImportFiles)
				{
					if (file.ID == saveID)
					{
						return true;
					}
				}
			}
			return false;
		}


		public static bool DoesSaveExist (int saveID)
		{
			if (KickStarter.saveSystem)
			{
				foreach (SaveFile file in KickStarter.saveSystem.foundSaveFiles)
				{
					if (file.ID == saveID)
					{
						return true;
					}
				}
			}
			return false;
		}


		public static void LoadAutoSave ()
		{
			if (KickStarter.saveSystem)
			{
				if (File.Exists (KickStarter.saveSystem.GetSaveFileName (0)))
				{
					KickStarter.saveSystem.LoadSaveGame (0);
				}
				else
				{
					Debug.LogWarning ("Could not load game: file " + KickStarter.saveSystem.GetSaveFileName (0) + " does not exist.");
				}
			}
		}


		public static bool ImportGame (int elementSlot, int saveID, bool useSaveID)
		{
			if (KickStarter.saveSystem)
			{
				if (!useSaveID)
				{
					if (KickStarter.saveSystem.foundImportFiles.Count > elementSlot)
					{
						saveID = KickStarter.saveSystem.foundImportFiles[elementSlot].ID;
					}
				}
				
				if (saveID >= 0)
				{
					KickStarter.saveSystem.ImportSaveGame (saveID);
					return true;
				}
			}
			return false;
		}


		public static bool LoadGame (int saveID)
		{
			return LoadGame (0, saveID, true);
		}
		
		
		public static bool LoadGame (int elementSlot, int saveID, bool useSaveID)
		{
			if (KickStarter.saveSystem)
			{
				if (!useSaveID)
				{
					saveID = KickStarter.saveSystem.foundSaveFiles[elementSlot].ID;
				}

				if (saveID == -1)
				{
					Debug.LogWarning ("Could not load game: file " + KickStarter.saveSystem.GetSaveFileName (saveID) + " does not exist.");
				}
				else
				{
					KickStarter.saveSystem.LoadSaveGame (saveID);
					return true;
				}
			}
			return false;
		}


		public void ClearAllData ()
		{
			saveData = new SaveData ();
		}


		public void ImportSaveGame (int saveID)
		{
			string allData = "";

			foreach (SaveFile saveFile in foundImportFiles)
			{
				if (saveFile.ID == saveID)
				{
					allData = Serializer.LoadSaveFile (saveFile.fileName, true);
				}
			}

			if (allData.ToString () != "")
			{
				string mainData;

				int divider = allData.IndexOf ("||");
				mainData = allData.Substring (0, divider);

				if (SaveSystem.GetSaveMethod () == SaveMethod.XML)
				{
					saveData = (SaveData) Serializer.DeserializeObjectXML <SaveData> (mainData);
				}
				else
				{
					saveData = Serializer.DeserializeObjectBinary <SaveData> (mainData);
				}
				
				// Stop any current-running ActionLists, dialogs and interactions
				KillActionLists ();
				SaveSystem.AssignVariables (saveData.mainData.runtimeVariablesData);
			}
		}
		
		
		public void LoadSaveGame (int saveID)
		{
			string allData = Serializer.LoadSaveFile (GetSaveFileName (saveID), true);

			if (allData.ToString () != "")
			{
				string mainData;
				string roomData;
				
				int divider = allData.IndexOf ("||");
				mainData = allData.Substring (0, divider);
				roomData = allData.Substring (divider + 2);
				
				if (SaveSystem.GetSaveMethod () == SaveMethod.XML)
				{
					saveData = (SaveData) Serializer.DeserializeObjectXML <SaveData> (mainData);
					KickStarter.levelStorage.allLevelData = (List<SingleLevelData>) Serializer.DeserializeObjectXML <List<SingleLevelData>> (roomData);
				}
				else
				{
					saveData = Serializer.DeserializeObjectBinary <SaveData> (mainData);
					KickStarter.levelStorage.allLevelData = Serializer.DeserializeRoom (roomData);
				}

				// Stop any current-running ActionLists, dialogs and interactions
				KillActionLists ();
				
				// If player has changed, destroy the old one and load in the new one
				if (KickStarter.settingsManager.playerSwitching == PlayerSwitching.Allow)
				{
					if ((KickStarter.player == null && saveData.mainData.currentPlayerID != KickStarter.settingsManager.GetEmptyPlayerID ()) ||
						(KickStarter.player != null && KickStarter.player.ID != saveData.mainData.currentPlayerID))
					{
						KickStarter.ResetPlayer (GetPlayerByID (saveData.mainData.currentPlayerID), saveData.mainData.currentPlayerID, true, Quaternion.identity);
					}
				}

				int newScene = GetPlayerScene (saveData.mainData.currentPlayerID, saveData.playerData);
				
				// Load correct scene
				if (newScene != Application.loadedLevel)
				{
					loadingGame = LoadingGame.InNewScene;
					KickStarter.sceneChanger.ChangeScene ("", newScene, false);
				}
				else
				{
					loadingGame = LoadingGame.InSameScene;

					// Already in the scene
					Sound[] sounds = FindObjectsOfType (typeof (Sound)) as Sound[];
					foreach (Sound sound in sounds)
					{
						if (sound.GetComponent <AudioSource>())
						{
							if (sound.soundType != SoundType.Music && !sound.GetComponent <AudioSource>().loop)
							{
								sound.Stop ();
							}
						}
					}

					OnLevelWasLoaded ();
				}
			}
		}


		private Player GetPlayerByID (int id)
		{
			SettingsManager settingsManager = KickStarter.settingsManager;

			foreach (PlayerPrefab playerPrefab in settingsManager.players)
			{
				if (playerPrefab.ID == id)
				{
					if (playerPrefab.playerOb)
					{
						return playerPrefab.playerOb;
					}

					return null;
				}
			}

			return null;
		}


		private int GetPlayerScene (int playerID, List<PlayerData> _playerData)
		{
			SettingsManager settingsManager = KickStarter.settingsManager;
			if (settingsManager.playerSwitching == PlayerSwitching.DoNotAllow)
			{
				if (_playerData.Count > 0)
				{
					return _playerData[0].currentScene;
				}
			}
			else
			{
				foreach (PlayerData _data in _playerData)
				{
					if (_data.playerID == playerID)
					{
						return (_data.currentScene);
					}
				}
			}

			return Application.loadedLevel;
		}


		private string GetPlayerSceneName (int playerID, List<PlayerData> _playerData)
		{
			SettingsManager settingsManager = KickStarter.settingsManager;
			if (settingsManager.playerSwitching == PlayerSwitching.DoNotAllow)
			{
				if (_playerData.Count > 0)
				{
					return _playerData[0].currentSceneName;
				}
			}
			else
			{
				foreach (PlayerData _data in _playerData)
				{
					if (_data.playerID == playerID)
					{
						return (_data.currentSceneName);
					}
				}
			}
			
			return Application.loadedLevelName;
		}
		
		
		private void OnLevelWasLoaded ()
		{
			if (KickStarter.settingsManager.IsInLoadingScene ())
			{
				return;
			}

			if (loadingGame == LoadingGame.InNewScene || loadingGame == LoadingGame.InSameScene)
			{
				if (KickStarter.dialog)
				{
					KickStarter.dialog.KillDialog (true, true);
				}
				
				if (KickStarter.playerInteraction)
				{
					KickStarter.playerInteraction.StopInteraction ();
				}

				ReturnMainData ();
				KickStarter.levelStorage.ReturnCurrentLevelData (true);
				CustomLoadHook ();

				if (loadingGame == LoadingGame.InSameScene)
				{
					loadingGame = LoadingGame.No;
				}
			}

			if (KickStarter.runtimeInventory)
		    {
				KickStarter.runtimeInventory.RemoveRecipes ();
			}


			if (loadingGame == LoadingGame.JustSwitchingPlayer)
			{
				foreach (PlayerData _data in saveData.playerData)
				{
					if (_data.playerID == KickStarter.player.ID)
					{
						ReturnCameraData (_data);
						break;
					}
				}

				KickStarter.stateHandler.gameState = GameState.Cutscene;
				KickStarter.mainCamera.FadeIn (0.5f);

				Invoke ("ReturnToGameplay", 0.01f);
			}

			AssetLoader.UnloadAssets ();
		}


		public static void SaveNewGame ()
		{
			if (KickStarter.saveSystem)
			{
				KickStarter.saveSystem.SaveNewSaveGame ();
			}
		}
		
		
		public void SaveNewSaveGame ()
		{
			if (foundSaveFiles != null && foundSaveFiles.Count > 0)
			{
				int expectedID = -1;

				for (int i=0; i<foundSaveFiles.Count; i++)
				{
					if (expectedID != -1 && expectedID != foundSaveFiles[i].ID)
					{
						SaveSaveGame (expectedID);
						return;
					}

					expectedID = foundSaveFiles[i].ID + 1;
				}

				// Saves present, but no gap
				int newSaveID = (foundSaveFiles [foundSaveFiles.Count-1].ID+1);
				SaveSaveGame (newSaveID);
				return;
			}

			SaveSaveGame (1);
		}


		public static void SaveAutoSave ()
		{
			if (KickStarter.saveSystem)
			{
				KickStarter.saveSystem.SaveSaveGame (0);
			}
		}


		public static void SaveGame (int saveID)
		{
			SaveSystem.SaveGame (0, saveID, true);
		}
		
		
		public static void SaveGame (int elementSlot, int saveID, bool useSaveID)
		{
			if (KickStarter.saveSystem)
			{
				if (!useSaveID)
				{
					saveID = KickStarter.saveSystem.foundSaveFiles[elementSlot].ID;
				}
				KickStarter.saveSystem.SaveSaveGame (saveID);
			}
		}
		
		
		public void SaveSaveGame (int saveID)
		{
			CustomSaveHook ();
			KickStarter.levelStorage.StoreCurrentLevelData ();
			
			Player player = KickStarter.player;

			if (KickStarter.playerInput && KickStarter.runtimeInventory && KickStarter.sceneChanger && KickStarter.settingsManager && KickStarter.stateHandler)
			{
				if (saveData != null && saveData.playerData != null && saveData.playerData.Count > 0)
				{
					foreach (PlayerData _data in saveData.playerData)
					{
						if (player != null && _data.playerID == player.ID)
						{
							saveData.playerData.Remove (_data);
							break;
						}
					}
				}
				else
				{
					saveData = new SaveData ();
					saveData.mainData = new MainData ();
					saveData.playerData = new List<PlayerData>();
				}

				PlayerData playerData = SavePlayerData (player);
				saveData.playerData.Add (playerData);

				// Main data
				saveData.mainData.cursorIsOff = KickStarter.stateHandler.cursorIsOff;
				saveData.mainData.inputIsOff = KickStarter.stateHandler.inputIsOff;
				saveData.mainData.interactionIsOff = KickStarter.stateHandler.interactionIsOff;
				saveData.mainData.menuIsOff = KickStarter.stateHandler.menuIsOff;
				saveData.mainData.movementIsOff = KickStarter.stateHandler.movementIsOff;
				saveData.mainData.cameraIsOff = KickStarter.stateHandler.cameraIsOff;
				saveData.mainData.triggerIsOff = KickStarter.stateHandler.triggerIsOff;
				saveData.mainData.playerIsOff = KickStarter.stateHandler.playerIsOff;

				saveData.mainData.movementMethod = (int) KickStarter.settingsManager.movementMethod;

				if (player != null)
				{
					saveData.mainData.currentPlayerID = player.ID;
				}
				else
				{
					saveData.mainData.currentPlayerID = KickStarter.settingsManager.GetEmptyPlayerID ();
				}

				saveData.mainData.timeScale = KickStarter.playerInput.timeScale;

				if (KickStarter.playerInput.activeArrows)
				{
					saveData.mainData.activeArrows = Serializer.GetConstantID (KickStarter.playerInput.activeArrows.gameObject);
				}
				
				if (KickStarter.playerInput.activeConversation)
				{
					saveData.mainData.activeConversation = Serializer.GetConstantID (KickStarter.playerInput.activeConversation.gameObject);
				}
				
				if (KickStarter.runtimeInventory.selectedItem != null)
				{
					saveData.mainData.selectedInventoryID = KickStarter.runtimeInventory.selectedItem.id;
					saveData.mainData.isGivingItem = KickStarter.runtimeInventory.IsGivingItem ();
				}
				else
				{
					saveData.mainData.selectedInventoryID = -1;
				}
				RuntimeVariables.DownloadAll ();
				saveData.mainData.runtimeVariablesData = SaveSystem.CreateVariablesData (RuntimeVariables.GetAllVars (), false, VariableLocation.Global);

				saveData.mainData.menuLockData = CreateMenuLockData (PlayerMenus.GetMenus ());
				saveData.mainData.menuVisibilityData = CreateMenuVisibilityData (PlayerMenus.GetMenus ());
				saveData.mainData.menuElementVisibilityData = CreateMenuElementVisibilityData (PlayerMenus.GetMenus ());
				saveData.mainData.menuJournalData = CreateMenuJournalData (PlayerMenus.GetMenus ());
				
				string mainData = "";
				string levelData = "";
				
				if (SaveSystem.GetSaveMethod () == SaveMethod.XML)
				{
					mainData = Serializer.SerializeObjectXML <SaveData> (saveData);
					levelData = Serializer.SerializeObjectXML <List<SingleLevelData>> (KickStarter.levelStorage.allLevelData);
				}
				else
				{
					mainData = Serializer.SerializeObjectBinary (saveData);
					levelData = Serializer.SerializeObjectBinary (KickStarter.levelStorage.allLevelData);
				}
				string allData = mainData + "||" + levelData;
		
				Serializer.CreateSaveFile (GetSaveFileName (saveID), allData);
				#if !UNITY_WEBPLAYER && !UNITY_WINRT
				if (KickStarter.settingsManager.takeSaveScreenshots)
				{
					StartCoroutine ("TakeScreenshot", GetSaveScreenshotName (saveID));
				}
				else
				{
					GatherSaveFiles ();
				}
				#else
				GatherSaveFiles ();
				#endif
			}
			else
			{
				if (KickStarter.playerInput == null)
				{
					Debug.LogWarning ("Save failed - no PlayerInput found.");
				}
				if (KickStarter.runtimeInventory == null)
				{
					Debug.LogWarning ("Save failed - no RuntimeInventory found.");
				}
				if (KickStarter.sceneChanger == null)
				{
					Debug.LogWarning ("Save failed - no SceneChanger found.");
				}
				if (KickStarter.settingsManager == null)
				{
					Debug.LogWarning ("Save failed - no Settings Manager found.");
				}
			}
		}


		public void SaveCurrentPlayerData ()
		{
			if (saveData != null && saveData.playerData != null && saveData.playerData.Count > 0)
			{
				foreach (PlayerData _data in saveData.playerData)
				{
					if ((KickStarter.player != null && _data.playerID == KickStarter.player.ID) ||
						(KickStarter.player == null && _data.playerID == KickStarter.settingsManager.GetEmptyPlayerID ()))
					{
						saveData.playerData.Remove (_data);
						break;
					}
				}
			}
			else
			{
				saveData = new SaveData ();
				saveData.mainData = new MainData ();
				saveData.playerData = new List<PlayerData>();
			}
			
			PlayerData playerData = SavePlayerData (KickStarter.player);
			saveData.playerData.Add (playerData);
		}


		private PlayerData SavePlayerData (Player player)
		{
			PlayerData playerData = new PlayerData ();

			playerData.currentScene = Application.loadedLevel;
			playerData.currentSceneName = Application.loadedLevelName;

			playerData.previousScene = KickStarter.sceneChanger.previousScene;
			playerData.previousSceneName = KickStarter.sceneChanger.previousSceneName;

			playerData.playerUpLock = KickStarter.playerInput.isUpLocked;
			playerData.playerDownLock = KickStarter.playerInput.isDownLocked;
			playerData.playerLeftlock = KickStarter.playerInput.isLeftLocked;
			playerData.playerRightLock = KickStarter.playerInput.isRightLocked;
			playerData.playerRunLock = (int) KickStarter.playerInput.runLock;
			playerData.playerFreeAimLock = KickStarter.playerInput.freeAimLock;

			KickStarter.runtimeInventory.RemoveRecipes ();
			playerData.inventoryData = CreateInventoryData (KickStarter.runtimeInventory.localItems);

			if (player == null)
			{
				playerData.playerPortraitGraphic = "";
				playerData.playerID = KickStarter.settingsManager.GetEmptyPlayerID ();
				return playerData;
			}

			playerData.playerID = player.ID;
			
			playerData.playerLocX = player.transform.position.x;
			playerData.playerLocY = player.transform.position.y;
			playerData.playerLocZ = player.transform.position.z;
			playerData.playerRotY = player.transform.eulerAngles.y;
			
			playerData.playerWalkSpeed = player.walkSpeedScale;
			playerData.playerRunSpeed = player.runSpeedScale;
			
			// Animation clips
			if (player.animationEngine == AnimationEngine.Sprites2DToolkit || player.animationEngine == AnimationEngine.SpritesUnity)
			{
				playerData.playerIdleAnim = player.idleAnimSprite;
				playerData.playerWalkAnim = player.walkAnimSprite;
				playerData.playerRunAnim = player.runAnimSprite;
				playerData.playerTalkAnim = player.talkAnimSprite;
			}
			else if (player.animationEngine == AnimationEngine.Legacy)
			{
				playerData.playerIdleAnim = AssetLoader. GetAssetInstanceID (player.idleAnim);
				playerData.playerWalkAnim = AssetLoader. GetAssetInstanceID (player.walkAnim);
				playerData.playerRunAnim = AssetLoader. GetAssetInstanceID (player.runAnim);
				playerData.playerTalkAnim = AssetLoader. GetAssetInstanceID (player.talkAnim);
			}
			else if (player.animationEngine == AnimationEngine.Mecanim)
			{
				playerData.playerWalkAnim = player.moveSpeedParameter;
				playerData.playerTalkAnim = player.talkParameter;
				playerData.playerRunAnim = player.turnParameter;
			}

			// Sound
			playerData.playerWalkSound = AssetLoader. GetAssetInstanceID (player.walkSound);
			playerData.playerRunSound = AssetLoader. GetAssetInstanceID (player.runSound);

			// Portrait graphic
			playerData.playerPortraitGraphic = AssetLoader. GetAssetInstanceID (player.portraitIcon.texture);
			playerData.playerSpeechLabel = player.speechLabel;

			// Rendering
			playerData.playerLockDirection = player.lockDirection;
			playerData.playerLockScale = player.lockScale;
			if (player.spriteChild && player.spriteChild.GetComponent <FollowSortingMap>())
			{
				playerData.playerLockSorting = player.spriteChild.GetComponent <FollowSortingMap>().lockSorting;
			}
			else if (player.GetComponent <FollowSortingMap>())
			{
				playerData.playerLockSorting = player.GetComponent <FollowSortingMap>().lockSorting;
			}
			else
			{
				playerData.playerLockSorting = false;
			}
			playerData.playerSpriteDirection = player.spriteDirection;
			playerData.playerSpriteScale = player.spriteScale;
			if (player.spriteChild && player.spriteChild.GetComponent <Renderer>())
			{
				playerData.playerSortingOrder = player.spriteChild.GetComponent <Renderer>().sortingOrder;
				playerData.playerSortingLayer = player.spriteChild.GetComponent <Renderer>().sortingLayerName;
			}
			else if (player.GetComponent <Renderer>())
			{
				playerData.playerSortingOrder = player.GetComponent <Renderer>().sortingOrder;
				playerData.playerSortingLayer = player.GetComponent <Renderer>().sortingLayerName;
			}

			playerData.playerActivePath = 0;
			playerData.lastPlayerActivePath = 0;
			if (player.GetPath (true))
			{
				playerData.playerTargetNode = player.GetTargetNode (true);
				playerData.playerPrevNode = player.GetPrevNode (true);
				playerData.playerIsRunning = player.isRunning;
				playerData.playerPathAffectY = player.activePath.affectY;
				
				if (player.GetComponent <Paths>() && player.GetPath (true) == player.GetComponent <Paths>())
				{
					playerData.playerPathData = Serializer.CreatePathData (player.GetComponent <Paths>());
					playerData.playerLockedPath = false;
				}
				else
				{
					playerData.playerPathData = "";
					playerData.playerActivePath = Serializer.GetConstantID (player.GetPath (true).gameObject);
					playerData.playerLockedPath = player.lockedPath;
				}
			}

			if (player.GetPath (false))
			{
				playerData.lastPlayerTargetNode = player.GetTargetNode (false);
				playerData.lastPlayerPrevNode = player.GetPrevNode (false);
				playerData.lastPlayerActivePath = Serializer.GetConstantID (player.GetPath (false).gameObject);
			}
			
			playerData.playerIgnoreGravity = player.ignoreGravity;

			// Head target
			playerData.playerLockHotspotHeadTurning = player.lockHotspotHeadTurning;
			if (player.headFacing == HeadFacing.Manual)
			{
				playerData.isHeadTurning = true;
				playerData.headTargetX = player.headTurnTarget.x;
				playerData.headTargetY = player.headTurnTarget.y;
				playerData.headTargetZ = player.headTurnTarget.z;
			}
			else
			{
				playerData.isHeadTurning = false;
				playerData.headTargetX = 0f;
				playerData.headTargetY = 0f;
				playerData.headTargetZ = 0f;
			}

			// Camera
			MainCamera mainCamera = KickStarter.mainCamera;
			if (mainCamera.attachedCamera)
			{
				playerData.gameCamera = Serializer.GetConstantID (mainCamera.attachedCamera.gameObject);
			}
			if (mainCamera.lastNavCamera)
			{
				playerData.lastNavCamera = Serializer.GetConstantID (mainCamera.lastNavCamera.gameObject);
			}
			if (mainCamera.lastNavCamera2)
			{
				playerData.lastNavCamera2 = Serializer.GetConstantID (mainCamera.lastNavCamera2.gameObject);
			}
			
			mainCamera.StopShaking ();
			playerData.mainCameraLocX = mainCamera.transform.position.x;
			playerData.mainCameraLocY = mainCamera.transform.position.y;
			playerData.mainCameraLocZ = mainCamera.transform.position.z;
			
			playerData.mainCameraRotX = mainCamera.transform.eulerAngles.x;
			playerData.mainCameraRotY = mainCamera.transform.eulerAngles.y;
			playerData.mainCameraRotZ = mainCamera.transform.eulerAngles.z;

			playerData.isSplitScreen = mainCamera.isSplitScreen;
			if (mainCamera.isSplitScreen)
			{
				playerData.isTopLeftSplit = mainCamera.isTopLeftSplit;
				playerData.splitAmountMain = mainCamera.splitAmountMain;
				playerData.splitAmountOther = mainCamera.splitAmountOther;
				
				if (mainCamera.splitOrientation == MenuOrientation.Vertical)
				{
					playerData.splitIsVertical = true;
				}
				else
				{
					playerData.splitIsVertical = false;
				}
				if (mainCamera.splitCamera && mainCamera.splitCamera.GetComponent <ConstantID>())
				{
					playerData.splitCameraID = mainCamera.splitCamera.GetComponent <ConstantID>().constantID;
				}
				else
				{
					playerData.splitCameraID = 0;
				}
			}

			return playerData;
		}


		public static int GetNumImportSlots ()
		{
			return KickStarter.saveSystem.foundImportFiles.Count;
		}


		public static int GetNumSlots ()
		{
			return KickStarter.saveSystem.foundSaveFiles.Count;
		}

		
		private string GetProjectName ()
		{
			SettingsManager settingsManager = KickStarter.settingsManager;
			if (settingsManager)
			{
				if (settingsManager.saveFileName == "")
				{
					settingsManager.saveFileName = SetProjectName ();
				}
				
				if (settingsManager.saveFileName != "")
				{
					return settingsManager.saveFileName;
				}
			}
			
			return SetProjectName ();
		}
		
		
		
		public static string SetProjectName ()
		{
			string[] s = Application.dataPath.Split ('/');
			string projectName = s[s.Length - 2];
			return projectName;
		}
		
		
		private string GetSaveFileName (int saveID)
		{
			string fileName = "";

			#if UNITY_WEBPLAYER || UNITY_WINRT
			fileName = GetProjectName () + "_" + saveID.ToString ();
			#else
			fileName = saveDirectory + Path.DirectorySeparatorChar.ToString () + GetProjectName () + "_" + saveID.ToString () + GetSaveExtension ();
			#endif

			return (fileName);
		}


		private string GetImportScreenshotName (int saveID, string importDirectory, string saveFileName)
		{
			return (importDirectory + Path.DirectorySeparatorChar.ToString () + saveFileName + "_" + saveID.ToString () + ".jpg");
		}


		private string GetSaveScreenshotName (int saveID)
		{
			string fileName = "";
			
			#if UNITY_WEBPLAYER || UNITY_WINRT
			fileName = GetProjectName () + "_" + saveID.ToString ();
			#else
			fileName = saveDirectory + Path.DirectorySeparatorChar.ToString () + GetProjectName () + "_" + saveID.ToString () + ".jpg";
			#endif
			
			return (fileName);
		}
		
		
		private void KillActionLists ()
		{
			KickStarter.actionListManager.KillAllLists ();

			Moveable[] moveables = FindObjectsOfType (typeof (Moveable)) as Moveable[];
			foreach (Moveable moveable in moveables)
			{
				moveable.Kill ();
			}
		}


		public string GetSlotLabel (int elementSlot, int saveID, bool useSaveID, SaveFile[] saveFiles)
		{
			if (Application.isPlaying)
			{
				if (useSaveID)
				{
					foreach (SaveFile saveFile in saveFiles)
					{
						if (saveFile.ID == saveID)
						{
							return saveFile.label;
						}
					}
				}
				else if (elementSlot >= 0)
				{
					if (elementSlot < saveFiles.Length)
					{
						return saveFiles [elementSlot].label;
					}
				}
				return "";
			}
			return ("Save test (01/01/2001 12:00:00)");
		}


		public static string GetImportSlotLabel (int elementSlot, int saveID, bool useSaveID)
		{
			if (Application.isPlaying && KickStarter.saveSystem.foundImportFiles != null)
			{
				return KickStarter.saveSystem.GetSlotLabel (elementSlot, saveID, useSaveID, KickStarter.saveSystem.foundImportFiles.ToArray ());
			}
			return ("Save test (01/01/2001 12:00:00)"); 
		}

		
		public static string GetSaveSlotLabel (int elementSlot, int saveID, bool useSaveID)
		{
			if (Application.isPlaying && KickStarter.saveSystem.foundSaveFiles != null)
			{
				return KickStarter.saveSystem.GetSlotLabel (elementSlot, saveID, useSaveID, KickStarter.saveSystem.foundSaveFiles.ToArray ());
			}
			return ("Save test (01/01/2001 12:00:00)"); 
		}


		public Texture2D GetScreenshot (int elementSlot, int saveID, bool useSaveID, SaveFile[] saveFiles)
		{
			if (Application.isPlaying)
			{
				if (useSaveID)
				{
					foreach (SaveFile saveFile in saveFiles)
					{
						if (saveFile.ID == saveID)
						{
							return saveFile.screenShot;
						}
					}
				}
				else if (elementSlot >= 0)
				{
					if (elementSlot < saveFiles.Length)
					{
						return saveFiles [elementSlot].screenShot;
					}
				}
			}
			return null;
		}


		public static Texture2D GetImportSlotScreenshot (int elementSlot, int saveID, bool useSaveID)
		{
			if (Application.isPlaying && KickStarter.saveSystem.foundImportFiles != null)
			{
				return KickStarter.saveSystem.GetScreenshot (elementSlot, saveID, useSaveID, KickStarter.saveSystem.foundImportFiles.ToArray ());
			}
			return null;
		}
		

		public static Texture2D GetSaveSlotScreenshot (int elementSlot, int saveID, bool useSaveID)
		{
			if (Application.isPlaying && KickStarter.saveSystem.foundSaveFiles != null)
			{
				return KickStarter.saveSystem.GetScreenshot (elementSlot, saveID, useSaveID, KickStarter.saveSystem.foundSaveFiles.ToArray ());
			}
			return null;
		}


		private void ReturnMainData ()
		{
			if (KickStarter.playerInput && KickStarter.runtimeInventory && KickStarter.settingsManager && KickStarter.stateHandler)
			{
				PlayerData playerData = new PlayerData ();

				if (KickStarter.settingsManager.playerSwitching == PlayerSwitching.DoNotAllow)
				{
					if (saveData.playerData.Count > 0)
					{
						playerData = saveData.playerData[0];
					}
				}
				else
				{
					foreach (PlayerData _data in saveData.playerData)
					{
						if (_data.playerID == saveData.mainData.currentPlayerID)
						{
							playerData = _data;
						}
					}
				}

				ReturnPlayerData (playerData, KickStarter.player);
				ReturnCameraData (playerData);

				KickStarter.stateHandler.cursorIsOff = saveData.mainData.cursorIsOff;
				KickStarter.stateHandler.inputIsOff = saveData.mainData.inputIsOff;
				KickStarter.stateHandler.interactionIsOff = saveData.mainData.interactionIsOff;
				KickStarter.stateHandler.menuIsOff = saveData.mainData.menuIsOff;
				KickStarter.stateHandler.movementIsOff = saveData.mainData.movementIsOff;
				KickStarter.stateHandler.cameraIsOff = saveData.mainData.cameraIsOff;
				KickStarter.stateHandler.triggerIsOff = saveData.mainData.triggerIsOff;
				KickStarter.stateHandler.playerIsOff = saveData.mainData.playerIsOff;

				KickStarter.settingsManager.movementMethod = (MovementMethod) saveData.mainData.movementMethod;

				KickStarter.sceneChanger.previousScene = playerData.previousScene;
				
				KickStarter.playerInput.SetUpLock (playerData.playerUpLock);
				KickStarter.playerInput.isDownLocked = playerData.playerDownLock;
				KickStarter.playerInput.isLeftLocked = playerData.playerLeftlock;
				KickStarter.playerInput.isRightLocked = playerData.playerRightLock;
				KickStarter.playerInput.runLock = (PlayerMoveLock) playerData.playerRunLock;
				KickStarter.playerInput.freeAimLock = playerData.playerFreeAimLock;

				// Inventory
				KickStarter.runtimeInventory.RemoveRecipes ();
				KickStarter.runtimeInventory.localItems = AssignInventory (KickStarter.runtimeInventory, playerData.inventoryData);
				if (saveData.mainData.selectedInventoryID > -1)
				{
					if (saveData.mainData.isGivingItem)
					{
						KickStarter.runtimeInventory.SelectItemByID (saveData.mainData.selectedInventoryID, SelectItemMode.Give);
					}
					else
					{
						KickStarter.runtimeInventory.SelectItemByID (saveData.mainData.selectedInventoryID, SelectItemMode.Use);
					}
				}
				else
				{
					KickStarter.runtimeInventory.SetNull ();
				}
				KickStarter.runtimeInventory.RemoveRecipes ();

				// Active screen arrows
				KickStarter.playerInput.RemoveActiveArrows ();
				ArrowPrompt loadedArrows = Serializer.returnComponent <ArrowPrompt> (saveData.mainData.activeArrows);
				if (loadedArrows)
				{
					loadedArrows.TurnOn ();
				}
				
				// Active conversation
				KickStarter.playerInput.activeConversation = Serializer.returnComponent <Conversation> (saveData.mainData.activeConversation);
				KickStarter.playerInput.timeScale = saveData.mainData.timeScale;

				// Variables
				SaveSystem.AssignVariables (saveData.mainData.runtimeVariablesData);

				// Menus
				foreach (AC.Menu menu in PlayerMenus.GetMenus ())
				{
					foreach (MenuElement element in menu.elements)
					{
						if (element is MenuInventoryBox)
						{
							MenuInventoryBox invBox = (MenuInventoryBox) element;
							invBox.ResetOffset ();
						}
					}
				}

				AssignMenuLocks (PlayerMenus.GetMenus (), saveData.mainData.menuLockData);
				AssignMenuVisibility (PlayerMenus.GetMenus (), saveData.mainData.menuVisibilityData);
				AssignMenuElementVisibility (PlayerMenus.GetMenus (), saveData.mainData.menuElementVisibilityData);
				AssignMenuJournals (PlayerMenus.GetMenus (), saveData.mainData.menuJournalData);

				KickStarter.mainCamera.HideScene ();
				KickStarter.playerMenus.HideSaveMenus ();
				KickStarter.stateHandler.gameState = GameState.Cutscene;
				KickStarter.mainCamera.FadeIn (0.5f);

				Invoke ("ReturnToGameplay", 0.01f);
			}
			else
			{
				if (KickStarter.playerInput == null)
				{
					Debug.LogWarning ("Load failed - no PlayerInput found.");
				}
				if (KickStarter.runtimeInventory == null)
				{
					Debug.LogWarning ("Load failed - no RuntimeInventory found.");
				}
				if (KickStarter.sceneChanger == null)
				{
					Debug.LogWarning ("Load failed - no SceneChanger found.");
				}
				if (KickStarter.settingsManager == null)
				{
					Debug.LogWarning ("Load failed - no Settings Manager found.");
				}
			}
		}


		public bool DoesPlayerDataExist (int ID, bool doSceneCheck)
		{
			if (saveData != null && saveData.playerData.Count > 0)
			{
				foreach (PlayerData _data in saveData.playerData)
				{
					if (_data.playerID == ID)
					{
						if (doSceneCheck && _data.currentScene == -1)
						{
							return false;
						}
						return true;
					}
				}
			}

			return false;
		}


		public int GetPlayerScene (int ID)
		{
			if (KickStarter.player)
			{
				if (saveData.playerData.Count > 0)
				{
					foreach (PlayerData _data in saveData.playerData)
					{
						if (_data.playerID == ID)
						{
							return (_data.currentScene);
						}
					}
				}
			}
			
			return Application.loadedLevel;
		}


		public string GetPlayerSceneName (int ID)
		{
			if (KickStarter.player)
			{
				if (saveData.playerData.Count > 0)
				{
					foreach (PlayerData _data in saveData.playerData)
					{
						if (_data.playerID == ID)
						{
							return (_data.currentSceneName);
						}
					}
				}
			}
			
			return Application.loadedLevelName;
		}


		public int AssignPlayerData (int ID, bool doInventory)
		{
			if (KickStarter.player)
			{
				if (saveData.playerData.Count > 0)
				{
					foreach (PlayerData _data in saveData.playerData)
					{
						if (_data.playerID == ID)
						{
							if (_data.currentScene != -1)
							{
								// If -1, data only exists because we updated inventory, so only restore Inventory in this case

								ReturnPlayerData (_data, KickStarter.player);
								ReturnCameraData (_data);

								KickStarter.playerInput.SetUpLock (_data.playerUpLock);
								KickStarter.playerInput.isDownLocked = _data.playerDownLock;
								KickStarter.playerInput.isLeftLocked = _data.playerLeftlock;
								KickStarter.playerInput.isRightLocked = _data.playerRightLock;
								KickStarter.playerInput.runLock = (PlayerMoveLock) _data.playerRunLock;
								KickStarter.playerInput.freeAimLock = _data.playerFreeAimLock;
							}

							KickStarter.runtimeInventory.SetNull ();
							KickStarter.runtimeInventory.RemoveRecipes ();

							if (doInventory)
							{
								KickStarter.runtimeInventory.localItems = AssignInventory (KickStarter.runtimeInventory, _data.inventoryData);
							}

							return (_data.currentScene);
						}
					}
				}
			}

			AssetLoader.UnloadAssets ();

			return Application.loadedLevel;
		}


		private void ReturnPlayerData (PlayerData playerData, Player player)
		{
			if (player == null)
			{
				return;
			}

			player.Teleport (new Vector3 (playerData.playerLocX, playerData.playerLocY, playerData.playerLocZ));
			player.SetRotation (playerData.playerRotY);
			//player.SetLookDirection (Vector3.zero, true);
			
			player.walkSpeedScale = playerData.playerWalkSpeed;
			player.runSpeedScale = playerData.playerRunSpeed;
			
			// Animation clips
			if (player.animationEngine == AnimationEngine.Sprites2DToolkit || player.animationEngine == AnimationEngine.SpritesUnity)
			{
				player.idleAnimSprite = playerData.playerIdleAnim;
				player.walkAnimSprite = playerData.playerWalkAnim;
				player.talkAnimSprite = playerData.playerTalkAnim;
				player.runAnimSprite = playerData.playerRunAnim;
			}
			else if (player.animationEngine == AnimationEngine.Legacy)
			{
				player.idleAnim = AssetLoader.RetrieveAsset <AnimationClip> (player.idleAnim, playerData.playerIdleAnim);
				player.walkAnim = AssetLoader.RetrieveAsset <AnimationClip> (player.walkAnim, playerData.playerWalkAnim);
				player.talkAnim = AssetLoader.RetrieveAsset <AnimationClip> (player.talkAnim, playerData.playerTalkAnim);
				player.runAnim = AssetLoader.RetrieveAsset <AnimationClip> (player.runAnim, playerData.playerRunAnim);
			}
			else if (player.animationEngine == AnimationEngine.Mecanim)
			{
				player.moveSpeedParameter = playerData.playerWalkAnim;
				player.talkParameter = playerData.playerTalkAnim;
				player.turnParameter = playerData.playerRunAnim;
			}

			// Sound
			player.walkSound = AssetLoader.RetrieveAsset (player.walkSound, playerData.playerWalkSound);
			player.runSound = AssetLoader.RetrieveAsset (player.runSound, playerData.playerRunSound);

			// Portrait graphic
			player.portraitIcon.texture = AssetLoader.RetrieveAsset (player.portraitIcon.texture, playerData.playerPortraitGraphic);
			player.speechLabel = playerData.playerSpeechLabel;

			// Rendering
			player.lockDirection = playerData.playerLockDirection;
			player.lockScale = playerData.playerLockScale;
			if (player.spriteChild && player.spriteChild.GetComponent <FollowSortingMap>())
			{
				player.spriteChild.GetComponent <FollowSortingMap>().lockSorting = playerData.playerLockSorting;
			}
			else if (player.GetComponent <FollowSortingMap>())
			{
				player.GetComponent <FollowSortingMap>().lockSorting = playerData.playerLockSorting;
			}
			else
			{
				player.ReleaseSorting ();
			}
			
			if (playerData.playerLockDirection)
			{
				player.spriteDirection = playerData.playerSpriteDirection;
			}
			if (playerData.playerLockScale)
			{
				player.spriteScale = playerData.playerSpriteScale;
			}
			if (playerData.playerLockSorting)
			{
				if (player.spriteChild && player.spriteChild.GetComponent <Renderer>())
				{
					player.spriteChild.GetComponent <Renderer>().sortingOrder = playerData.playerSortingOrder;
					player.spriteChild.GetComponent <Renderer>().sortingLayerName = playerData.playerSortingLayer;
				}
				else if (player.GetComponent <Renderer>())
				{
					player.GetComponent <Renderer>().sortingOrder = playerData.playerSortingOrder;
					player.GetComponent <Renderer>().sortingLayerName = playerData.playerSortingLayer;
				}
			}
			
			// Active path
			player.Halt ();
			player.ForceIdle ();

			if (playerData.playerPathData != null && playerData.playerPathData != "" && player.GetComponent <Paths>())
			{
				Paths savedPath = player.GetComponent <Paths>();
				savedPath = Serializer.RestorePathData (savedPath, playerData.playerPathData);
				player.SetPath (savedPath, playerData.playerTargetNode, playerData.playerPrevNode, playerData.playerPathAffectY);
				player.isRunning = playerData.playerIsRunning;
				player.lockedPath = false;
			}
			else if (playerData.playerActivePath != 0)
			{
				Paths savedPath = Serializer.returnComponent <Paths> (playerData.playerActivePath);
				if (savedPath)
				{
					player.lockedPath = playerData.playerLockedPath;
					
					if (player.lockedPath)
					{
						player.SetLockedPath (savedPath);
					}
					else
					{
						player.SetPath (savedPath, playerData.playerTargetNode, playerData.playerPrevNode);
					}
				}
			}

			// Previous path
			if (playerData.lastPlayerActivePath != 0)
			{
				Paths savedPath = Serializer.returnComponent <Paths> (playerData.lastPlayerActivePath);
				if (savedPath)
				{
					player.SetLastPath (savedPath, playerData.lastPlayerTargetNode, playerData.lastPlayerPrevNode);
				}
			}

			// Head target
			player.lockHotspotHeadTurning = playerData.playerLockHotspotHeadTurning;
			if (playerData.isHeadTurning)
			{
				player.SetHeadTurnTarget (new Vector3 (playerData.headTargetX, playerData.headTargetY, playerData.headTargetZ), true);
			}
			else
			{
				player.ClearHeadTurnTarget (true);
			}

			player.ignoreGravity = playerData.playerIgnoreGravity;
		}


		private void ReturnCameraData (PlayerData playerData)
		{
			// Camera
			MainCamera mainCamera = KickStarter.mainCamera;

			if (mainCamera.isSplitScreen)
			{
				mainCamera.RemoveSplitScreen ();
			}
			mainCamera.StopShaking ();
			mainCamera.SetGameCamera (Serializer.returnComponent <_Camera> (playerData.gameCamera));
			mainCamera.lastNavCamera = Serializer.returnComponent <_Camera> (playerData.lastNavCamera);
			mainCamera.lastNavCamera2 = Serializer.returnComponent <_Camera> (playerData.lastNavCamera2);
			mainCamera.ResetMoving ();

			mainCamera.transform.position = new Vector3 (playerData.mainCameraLocX, playerData.mainCameraLocY, playerData.mainCameraLocZ);
			mainCamera.transform.eulerAngles = new Vector3 (playerData.mainCameraRotX, playerData.mainCameraRotY, playerData.mainCameraRotZ);
			mainCamera.ResetProjection ();

			if (mainCamera.attachedCamera)
			{
				mainCamera.attachedCamera.MoveCameraInstant ();
			}
			mainCamera.SnapToAttached ();
			
			mainCamera.isSplitScreen = playerData.isSplitScreen;
			if (mainCamera.isSplitScreen)
			{
				mainCamera.isTopLeftSplit = playerData.isTopLeftSplit;
				if (playerData.splitIsVertical)
				{
					mainCamera.splitOrientation = MenuOrientation.Vertical;
				}
				else
				{
					mainCamera.splitOrientation = MenuOrientation.Horizontal;
				}
				if (playerData.splitCameraID != 0)
				{
					_Camera splitCamera = Serializer.returnComponent <_Camera> (playerData.splitCameraID);
					if (splitCamera)
					{
						mainCamera.splitCamera = splitCamera;
					}
				}
				mainCamera.StartSplitScreen (playerData.splitAmountMain, playerData.splitAmountOther);
			}
		}


		private void ReturnToGameplay ()
		{
			if (KickStarter.playerInput.activeConversation)
			{
				KickStarter.stateHandler.gameState = GameState.DialogOptions;
			}
			else
			{
				KickStarter.stateHandler.gameState = GameState.Normal;
			}

			KickStarter.playerInput.mouseState = MouseState.Normal;

			if (KickStarter.sceneSettings)
			{
				KickStarter.sceneSettings.OnLoad ();
			}
		}
		
		
		public static void AssignVariables (string runtimeVariablesData, bool fromOptions = false)
		{
			if (runtimeVariablesData == null)
			{
				return;
			}

			KickStarter.runtimeVariables.ClearSpeechLog ();
			
			if (runtimeVariablesData.Length > 0)
			{
				string[] varsArray = runtimeVariablesData.Split ("|"[0]);
				foreach (string chunk in varsArray)
				{
					string[] chunkData = chunk.Split (":"[0]);
					
					int _id = 0;
					int.TryParse (chunkData[0], out _id);
					GVar var = RuntimeVariables.GetVariable (_id);
				
					if (var == null)
					{
						continue;
					}
					if (fromOptions && var.link != VarLink.OptionsData)
					{
						continue;
					}

					if (var.type == VariableType.String)
					{
						string _text = chunkData[1];
						var.SetValue (_text);
					}
					else if (var.type == VariableType.Float)
					{
						float _value = 0f;
						float.TryParse (chunkData[1], out _value);
						var.SetValue (_value, SetVarMethod.SetValue);
					}
					else
					{
						int _value = 0;
						int.TryParse (chunkData[1], out _value);
						var.SetValue (_value, SetVarMethod.SetValue);
					}
				}
			}
			
			RuntimeVariables.UploadAll ();
		}

		
		private List<InvItem> AssignInventory (RuntimeInventory _runtimeInventory, string inventoryData)
		{
			List<InvItem> invItems = new List<InvItem>();

			if (inventoryData != null && inventoryData.Length > 0)
			{
				string[] countArray = inventoryData.Split ("|"[0]);
				
				foreach (string chunk in countArray)
				{
					string[] chunkData = chunk.Split (":"[0]);
					
					int _id = 0;
					int.TryParse (chunkData[0], out _id);
		
					int _count = 0;
					int.TryParse (chunkData[1], out _count);
					
					invItems = _runtimeInventory.Add (_id, _count, invItems, false);
				}
			}

			return invItems;
		}


		private void AssignMenuLocks (List<Menu> menus, string menuLockData)
		{
			if (menuLockData.Length > 0)
			{
				string[] lockArray = menuLockData.Split ("|"[0]);

				foreach (string chunk in lockArray)
				{
					string[] chunkData = chunk.Split (":"[0]);
					
					int _id = 0;
					int.TryParse (chunkData[0], out _id);
					
					bool _lock = false;
					bool.TryParse (chunkData[1], out _lock);
					
					foreach (AC.Menu _menu in menus)
					{
						if (_menu.id == _id)
						{
							_menu.isLocked = _lock;
							break;
						}
					}
				}
			}
		}


		private void AssignMenuVisibility (List<Menu> menus, string menuVisibilityData)
		{
			if (menuVisibilityData.Length > 0)
			{
				string[] visArray = menuVisibilityData.Split ("|"[0]);
				
				foreach (string chunk in visArray)
				{
					string[] chunkData = chunk.Split (":"[0]);
					
					int _id = 0;
					int.TryParse (chunkData[0], out _id);
					
					bool _lock = false;
					bool.TryParse (chunkData[1], out _lock);
					
					foreach (AC.Menu _menu in menus)
					{
						if (_menu.id == _id)
						{
							if (_menu.IsManualControlled ())
							{
								if (_lock)
								{
									_menu.TurnOn (false);
								}
								else
								{
									_menu.TurnOff (false);
								}
							}
							break;
						}
					}
				}
			}
		}


		private void AssignMenuElementVisibility (List<Menu> menus, string menuElementVisibilityData)
		{
			if (menuElementVisibilityData.Length > 0)
			{
				string[] visArray = menuElementVisibilityData.Split ("|"[0]);
				
				foreach (string chunk in visArray)
				{
					string[] chunkData = chunk.Split (":"[0]);
					
					int _menuID = 0;
					int.TryParse (chunkData[0], out _menuID);

					foreach (AC.Menu _menu in menus)
					{
						if (_menu.id == _menuID)
						{
							// Found a match
							string[] perMenuData = chunkData[1].Split ("+"[0]);
							
							foreach (string perElementData in perMenuData)
							{
								string [] chunkData2 = perElementData.Split ("="[0]);
								
								int _elementID = 0;
								int.TryParse (chunkData2[0], out _elementID);
								
								bool _elementVisibility = false;
								bool.TryParse (chunkData2[1], out _elementVisibility);
								
								foreach (MenuElement _element in _menu.elements)
								{
									if (_element.ID == _elementID && _element.isVisible != _elementVisibility)
									{
										_element.isVisible = _elementVisibility;
										break;
									}
								}
							}

							_menu.ResetVisibleElements ();
							_menu.Recalculate ();
							break;
						}
					}
				}
			}
		}


		private void AssignMenuJournals (List<Menu> menus, string menuJournalData)
		{
			if (menuJournalData.Length > 0)
			{
				string[] journalArray = menuJournalData.Split ("|"[0]);
				
				foreach (string chunk in journalArray)
				{
					string[] chunkData = chunk.Split (":"[0]);
					
					int menuID = 0;
					int.TryParse (chunkData[0], out menuID);
					
					int elementID = 0;
					int.TryParse (chunkData[1], out elementID);

					foreach (AC.Menu _menu in menus)
					{
						if (_menu.id == menuID)
						{
							foreach (MenuElement _element in _menu.elements)
							{
								if (_element.ID == elementID && _element is MenuJournal)
								{
									MenuJournal journal = (MenuJournal) _element;
									journal.pages = new List<JournalPage>();
									journal.showPage = 1;

									string[] pageArray = chunkData[2].Split ("~"[0]);

									foreach (string chunkData2 in pageArray)
									{
										string[] chunkData3 = chunkData2.Split ("*"[0]);

										int lineID = -1;
										int.TryParse (chunkData3[0], out lineID);

										journal.pages.Add (new JournalPage (lineID, chunkData3[1]));
									}

									break;
								}
							}
						}
					}
				}
			}
		}


		private string CreateInventoryData (List<InvItem> invItems)
		{
			System.Text.StringBuilder inventoryString = new System.Text.StringBuilder ();
			
			foreach (InvItem item in invItems)
			{
				if (item != null)
				{
					inventoryString.Append (item.id.ToString ());
					inventoryString.Append (":");
					inventoryString.Append (item.count.ToString ());
					inventoryString.Append ("|");
				}
			}
			
			if (invItems != null && invItems.Count > 0)
			{
				inventoryString.Remove (inventoryString.Length-1, 1);
			}
			
			return inventoryString.ToString ();		
		}
		
			
		public static string CreateVariablesData (List<GVar> vars, bool isOptionsData, VariableLocation location)
		{
			System.Text.StringBuilder variablesString = new System.Text.StringBuilder ();

			foreach (GVar _var in vars)
			{
				if ((isOptionsData && _var.link == VarLink.OptionsData) || (!isOptionsData && _var.link != VarLink.OptionsData) || location == VariableLocation.Local)
				{
					variablesString.Append (_var.id.ToString ());
					variablesString.Append (":");
					if (_var.type == VariableType.String)
					{
						string textVal = _var.textVal;
						if (textVal.Contains ("|"))
						{
							textVal = textVal.Replace ("|", "");
							Debug.LogWarning ("Removed pipe delimeter from variable " + _var.label);
						}
						variablesString.Append (textVal);
					}
					else if (_var.type == VariableType.Float)
					{
						variablesString.Append (_var.floatVal.ToString ());
					}
					else
					{
						variablesString.Append (_var.val.ToString ());
					}
					variablesString.Append ("|");
				}
			}
			
			if (variablesString.Length > 0)
			{
				variablesString.Remove (variablesString.Length-1, 1);
			}

			return variablesString.ToString ();		
		}


		private string CreateMenuLockData (List<Menu> menus)
		{
			System.Text.StringBuilder menuString = new System.Text.StringBuilder ();

			foreach (AC.Menu _menu in menus)
			{
				menuString.Append (_menu.id.ToString ());
				menuString.Append (":");
				menuString.Append (_menu.isLocked.ToString ());
				menuString.Append ("|");
			}

			if (menus.Count > 0)
			{
				menuString.Remove (menuString.Length-1, 1);
			}

			return menuString.ToString ();
		}


		private string CreateMenuVisibilityData (List<Menu> menus)
		{
			System.Text.StringBuilder menuString = new System.Text.StringBuilder ();
			
			foreach (AC.Menu _menu in menus)
			{
				if (_menu.IsManualControlled ())
				{
					menuString.Append (_menu.id.ToString ());
					menuString.Append (":");
					menuString.Append (_menu.IsEnabled ().ToString ());
					menuString.Append ("|");
				}
			}
			
			if (menus.Count > 0)
			{
				menuString.Remove (menuString.Length-1, 1);
			}
			
			return menuString.ToString ();
		}


		private string CreateMenuElementVisibilityData (List<Menu> menus)
		{
			System.Text.StringBuilder visibilityString = new System.Text.StringBuilder ();
			
			foreach (AC.Menu _menu in menus)
			{
				if (_menu.elements.Count > 0)
				{
					visibilityString.Append (_menu.id.ToString ());
					visibilityString.Append (":");

					foreach (MenuElement _element in _menu.elements)
					{
						visibilityString.Append (_element.ID.ToString ());
						visibilityString.Append ("=");
						visibilityString.Append (_element.isVisible.ToString ());
						visibilityString.Append ("+");
					}

					visibilityString.Remove (visibilityString.Length-1, 1);
					visibilityString.Append ("|");
				}
			}
			
			if (menus.Count > 0)
			{
				visibilityString.Remove (visibilityString.Length-1, 1);
			}

			return visibilityString.ToString ();
		}


		private string CreateMenuJournalData (List<Menu> menus)
		{
			System.Text.StringBuilder journalString = new System.Text.StringBuilder ();

			foreach (AC.Menu _menu in menus)
			{
				foreach (MenuElement _element in _menu.elements)
				{
					if (_element is MenuJournal)
					{
						MenuJournal journal = (MenuJournal) _element;
						journalString.Append (_menu.id.ToString ());
						journalString.Append (":");
						journalString.Append (journal.ID);
						journalString.Append (":");

						foreach (JournalPage page in journal.pages)
						{
							journalString.Append (page.lineID);
							journalString.Append ("*");
							journalString.Append (page.text);
							journalString.Append ("~");
						}

						if (journal.pages.Count > 0)
						{
							journalString.Remove (journalString.Length-1, 1);
						}

						journalString.Append ("|");
					}
				}
			}

			if (journalString.ToString () != "")
			{
				journalString.Remove (journalString.Length-1, 1);
			}

			return journalString.ToString ();
		}


		public List<InvItem> GetItemsFromPlayer (int _playerID)
		{
			if (KickStarter.player.ID == _playerID)
			{
				return KickStarter.runtimeInventory.localItems;
			}

			if (saveData != null && saveData.playerData != null)
			{
				foreach (PlayerData _data in saveData.playerData)
				{
					if (_data.playerID == _playerID)
					{
						return AssignInventory (KickStarter.runtimeInventory, _data.inventoryData);
					}
				}
			}
			return new List<InvItem>();
		}


		public void AssignItemsToPlayer (List<InvItem> invItems, int _playerID)
		{
			string invData = CreateInventoryData (invItems);

			if (saveData != null && saveData.playerData != null)
			{
				foreach (PlayerData data in saveData.playerData)
				{
					if (data.playerID == _playerID)
					{
						PlayerData newPlayerData = new PlayerData ();
						newPlayerData.CopyData (data);
						newPlayerData.inventoryData = invData;

						saveData.playerData.Remove (data);
						saveData.playerData.Add (newPlayerData);

						return;
					}
				}
			}

			PlayerData playerData = new PlayerData ();
			playerData.playerID = _playerID;
			playerData.inventoryData = invData;
			playerData.currentScene = -1;

			if (saveData == null)
			{
				ClearAllData ();
			}

			saveData.playerData.Add (playerData);
		}


		private void CustomSaveHook ()
		{
			ISave[] saveHooks = GetSaveHooks (GetComponents (typeof (ISave)));
			if (saveHooks != null && saveHooks.Length > 0)
			{
				foreach (ISave saveHook in saveHooks)
				{
					saveHook.PreSave ();
				}
			}
		}


		private void CustomLoadHook ()
		{
			ISave[] saveHooks = GetSaveHooks (GetComponents (typeof (ISave)));
			if (saveHooks != null && saveHooks.Length > 0)
			{
				foreach (ISave saveHook in saveHooks)
				{
					saveHook.PostLoad ();
				}
			}
		}


		private ISave[] GetSaveHooks (IList list)
		{
			ISave[] ret = new ISave[list.Count];
			list.CopyTo (ret, 0);
			return ret;
		}

	}


	public struct SaveFile
	{

		public int ID;
		public string label;
		public Texture2D screenShot;
		public string fileName;


		public SaveFile (int _ID, string _label, Texture2D _screenShot, string _fileName)
		{
			ID = _ID;
			label = _label;
			screenShot = _screenShot;
			fileName = _fileName;
		}

	}

}