/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"KickStarter.cs"
 * 
 *	This script will make sure that PersistentEngine and the Player gameObjects are always created,
 *	regardless of which scene the game is begun from.  It will also check the key gameObjects for
 *	essential scripts and references.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	public class KickStarter : MonoBehaviour
	{

		private static Player playerPrefab = null;
		private static MainCamera mainCameraPrefab = null;
		private static GameObject persistentEnginePrefab = null;
		private static GameObject gameEnginePrefab = null;

		// Managers
		private static SceneManager sceneManagerPrefab = null;
		private static SettingsManager settingsManagerPrefab = null;
		private static ActionsManager actionsManagerPrefab = null;
		private static VariablesManager variablesManagerPrefab = null;
		private static InventoryManager inventoryManagerPrefab = null;
		private static SpeechManager speechManagerPrefab = null;
		private static CursorManager cursorManagerPrefab = null;
		private static MenuManager menuManagerPrefab = null;

		// PersistentEngine components
		private static Options optionsComponent = null;
		private static RuntimeInventory runtimeInventoryComponent = null;
		private static RuntimeVariables runtimeVariablesComponent = null;
		private static PlayerMenus playerMenusComponent = null;
		private static StateHandler stateHandlerComponent = null;
		private static SceneChanger sceneChangerComponent = null;
		private static SaveSystem saveSystemComponent = null;
		private static LevelStorage levelStorageComponent = null;

		// GameEngine components
		private static MenuSystem menuSystemComponent = null;
		private static Dialog dialogComponent = null;
		private static PlayerInput playerInputComponent = null;
		private static PlayerInteraction playerInteractionComponent = null;
		private static PlayerMovement playerMovementComponent = null;
		private static PlayerCursor playerCursorComponent = null;
		private static PlayerQTE playerQTEComponent = null;
		private static SceneSettings sceneSettingsComponent = null;
		private static NavigationManager navigationManagerComponent = null;
		private static ActionListManager actionListManagerComponent = null;
		private static LocalVariables localVariablesComponent = null;
		private static MenuPreview menuPreviewComponent = null;


		private static void SetGameEngine ()
		{
			if (gameEnginePrefab == null && GameObject.FindObjectOfType <SceneSettings>())
		    {
				gameEnginePrefab = GameObject.FindObjectOfType <SceneSettings>().gameObject;
			}
		}


		public static SceneManager sceneManager
		{
			get
			{
				if (sceneManagerPrefab != null) return sceneManagerPrefab;
				else if (AdvGame.GetReferences () && AdvGame.GetReferences ().sceneManager)
				{
					sceneManagerPrefab = AdvGame.GetReferences ().sceneManager;
					return sceneManagerPrefab;
				}
				return null;
			}
			set
			{
				sceneManagerPrefab = value;
			}
		}


		public static SettingsManager settingsManager
		{
			get
			{
				if (settingsManagerPrefab != null) return settingsManagerPrefab;
				else if (AdvGame.GetReferences () && AdvGame.GetReferences ().settingsManager)
				{
					settingsManagerPrefab = AdvGame.GetReferences ().settingsManager;
					return settingsManagerPrefab;
				}
				return null;
			}
			set
			{
				settingsManagerPrefab = value;
			}
		}


		public static ActionsManager actionsManager
		{
			get
			{
				if (actionsManagerPrefab != null) return actionsManagerPrefab;
				else if (AdvGame.GetReferences () && AdvGame.GetReferences ().actionsManager)
				{
					actionsManagerPrefab = AdvGame.GetReferences ().actionsManager;
					return actionsManagerPrefab;
				}
				return null;
			}
			set
			{
				actionsManagerPrefab = value;
			}
		}


		public static VariablesManager variablesManager
		{
			get
			{
				if (variablesManagerPrefab != null) return variablesManagerPrefab;
				else if (AdvGame.GetReferences () && AdvGame.GetReferences ().variablesManager)
				{
					variablesManagerPrefab = AdvGame.GetReferences ().variablesManager;
					return variablesManagerPrefab;
				}
				return null;
			}
			set
			{
				variablesManagerPrefab = value;
			}
		}


		public static InventoryManager inventoryManager
		{
			get
			{
				if (inventoryManagerPrefab != null) return inventoryManagerPrefab;
				else if (AdvGame.GetReferences () && AdvGame.GetReferences ().inventoryManager)
				{
					inventoryManagerPrefab = AdvGame.GetReferences ().inventoryManager;
					return inventoryManagerPrefab;
				}
				return null;
			}
			set
			{
				inventoryManagerPrefab = value;
			}
		}


		public static SpeechManager speechManager
		{
			get
			{
				if (speechManagerPrefab != null) return speechManagerPrefab;
				else if (AdvGame.GetReferences () && AdvGame.GetReferences ().speechManager)
				{
					speechManagerPrefab = AdvGame.GetReferences ().speechManager;
					return speechManagerPrefab;
				}
				return null;
			}
			set
			{
				speechManagerPrefab = value;
			}
		}


		public static CursorManager cursorManager
		{
			get
			{
				if (cursorManagerPrefab != null) return cursorManagerPrefab;
				else if (AdvGame.GetReferences () && AdvGame.GetReferences ().cursorManager)
				{
					cursorManagerPrefab = AdvGame.GetReferences ().cursorManager;
					return cursorManagerPrefab;
				}
				return null;
			}
			set
			{
				cursorManagerPrefab = value;
			}
		}


		public static MenuManager menuManager
		{
			get
			{
				if (menuManagerPrefab != null) return menuManagerPrefab;
				else if (AdvGame.GetReferences () && AdvGame.GetReferences ().menuManager)
				{
					menuManagerPrefab = AdvGame.GetReferences ().menuManager;
					return menuManagerPrefab;
				}
				return null;
			}
			set
			{
				menuManagerPrefab = value;
			}
		}


		public static Options options
		{
			get
			{
				if (optionsComponent != null) return optionsComponent;
				else if (persistentEnginePrefab && persistentEnginePrefab.GetComponent <Options>())
				{
					optionsComponent = persistentEnginePrefab.GetComponent <Options>();
					return optionsComponent;
				}
				return null;
			}
		}


		public static RuntimeInventory runtimeInventory
		{
			get
			{
				if (runtimeInventoryComponent != null) return runtimeInventoryComponent;
				else if (persistentEnginePrefab && persistentEnginePrefab.GetComponent <RuntimeInventory>())
				{
					runtimeInventoryComponent = persistentEnginePrefab.GetComponent <RuntimeInventory>();
					return runtimeInventoryComponent;
				}
				return null;
			}
		}


		public static RuntimeVariables runtimeVariables
		{
			get
			{
				if (runtimeVariablesComponent != null) return runtimeVariablesComponent;
				else if (persistentEnginePrefab && persistentEnginePrefab.GetComponent <RuntimeVariables>())
				{
					runtimeVariablesComponent = persistentEnginePrefab.GetComponent <RuntimeVariables>();
					return runtimeVariablesComponent;
				}
				return null;
			}
		}


		public static PlayerMenus playerMenus
		{
			get
			{
				if (playerMenusComponent != null) return playerMenusComponent;
				else if (persistentEnginePrefab && persistentEnginePrefab.GetComponent <PlayerMenus>())
				{
					playerMenusComponent = persistentEnginePrefab.GetComponent <PlayerMenus>();
					return playerMenusComponent;
				}
				return null;
			}
		}


		public static StateHandler stateHandler
		{
			get
			{
				if (stateHandlerComponent != null) return stateHandlerComponent;
				else if (persistentEnginePrefab && persistentEnginePrefab.GetComponent <StateHandler>())
				{
					stateHandlerComponent = persistentEnginePrefab.GetComponent <StateHandler>();
					return stateHandlerComponent;
				}
				return null;
			}
		}


		public static SceneChanger sceneChanger
		{
			get
			{
				if (sceneChangerComponent != null) return sceneChangerComponent;
				else if (persistentEnginePrefab && persistentEnginePrefab.GetComponent <SceneChanger>())
				{
					sceneChangerComponent = persistentEnginePrefab.GetComponent <SceneChanger>();
					return sceneChangerComponent;
				}
				return null;
			}
		}


		public static SaveSystem saveSystem
		{
			get
			{
				if (saveSystemComponent != null) return saveSystemComponent;
				else if (persistentEnginePrefab && persistentEnginePrefab.GetComponent <SaveSystem>())
				{
					saveSystemComponent = persistentEnginePrefab.GetComponent <SaveSystem>();
					return saveSystemComponent;
				}
				return null;
			}
		}


		public static LevelStorage levelStorage
		{
			get
			{
				if (levelStorageComponent != null) return levelStorageComponent;
				else if (persistentEnginePrefab && persistentEnginePrefab.GetComponent <LevelStorage>())
				{
					levelStorageComponent = persistentEnginePrefab.GetComponent <LevelStorage>();
					return levelStorageComponent;
				}
				return null;
			}
		}


		public static MenuSystem menuSystem
		{
			get
			{
				if (menuSystemComponent != null) return menuSystemComponent;
				else
				{
					SetGameEngine ();
				}

				if (gameEnginePrefab && gameEnginePrefab.GetComponent <MenuSystem>())
				{
					menuSystemComponent = gameEnginePrefab.GetComponent <MenuSystem>();
					return menuSystemComponent;
				}
				return null;
			}
		}


		public static Dialog dialog
		{
			get
			{
				if (dialogComponent != null) return dialogComponent;
				else
				{
					SetGameEngine ();
				}

				if (gameEnginePrefab && gameEnginePrefab.GetComponent <Dialog>())
				{
					dialogComponent = gameEnginePrefab.GetComponent <Dialog>();
					return dialogComponent;
				}
				return null;
			}
		}


		public static PlayerInput playerInput
		{
			get
			{
				if (playerInputComponent != null) return playerInputComponent;
				else
				{
					SetGameEngine ();
				}

				if (gameEnginePrefab && gameEnginePrefab.GetComponent <PlayerInput>())
				{
					playerInputComponent = gameEnginePrefab.GetComponent <PlayerInput>();
					return playerInputComponent;
				}
				return null;
			}
		}


		public static PlayerInteraction playerInteraction
		{
			get
			{
				if (playerInteractionComponent != null) return playerInteractionComponent;
				else
				{
					SetGameEngine ();
				}

				if (gameEnginePrefab && gameEnginePrefab.GetComponent <PlayerInteraction>())
				{
					playerInteractionComponent = gameEnginePrefab.GetComponent <PlayerInteraction>();
					return playerInteractionComponent;
				}
				return null;
			}
		}


		public static PlayerMovement playerMovement
		{
			get
			{
				if (playerMovementComponent != null) return playerMovementComponent;
				else
				{
					SetGameEngine ();
				}

				if (gameEnginePrefab && gameEnginePrefab.GetComponent <PlayerMovement>())
				{
					playerMovementComponent = gameEnginePrefab.GetComponent <PlayerMovement>();
					return playerMovementComponent;
				}
				return null;
			}
		}


		public static PlayerCursor playerCursor
		{
			get
			{
				if (playerCursorComponent != null) return playerCursorComponent;
				else
				{
					SetGameEngine ();
				}

				if (gameEnginePrefab && gameEnginePrefab.GetComponent <PlayerCursor>())
				{
					playerCursorComponent = gameEnginePrefab.GetComponent <PlayerCursor>();
					return playerCursorComponent;
				}
				return null;
			}
		}


		public static PlayerQTE playerQTE
		{
			get
			{
				if (playerQTEComponent != null) return playerQTEComponent;
				else
				{
					SetGameEngine ();
				}

				if (gameEnginePrefab && gameEnginePrefab.GetComponent <PlayerQTE>())
				{
					playerQTEComponent = gameEnginePrefab.GetComponent <PlayerQTE>();
					return playerQTEComponent;
				}
				return null;
			}
		}


		public static SceneSettings sceneSettings
		{
			get
			{
				if (sceneSettingsComponent != null) return sceneSettingsComponent;
				else
				{
					SetGameEngine ();
				}

				if (gameEnginePrefab && gameEnginePrefab.GetComponent <SceneSettings>())
				{
					sceneSettingsComponent = gameEnginePrefab.GetComponent <SceneSettings>();
					return sceneSettingsComponent;
				}
				return null;
			}
		}


		public static NavigationManager navigationManager
		{
			get
			{
				if (navigationManagerComponent != null) return navigationManagerComponent;
				else
				{
					SetGameEngine ();
				}

				if (gameEnginePrefab && gameEnginePrefab.GetComponent <NavigationManager>())
				{
					navigationManagerComponent = gameEnginePrefab.GetComponent <NavigationManager>();
					return navigationManagerComponent;
				}
				return null;
			}
		}


		public static ActionListManager actionListManager
		{
			get
			{
				if (actionListManagerComponent != null) return actionListManagerComponent;
				else
				{
					SetGameEngine ();
				}

				if (gameEnginePrefab && gameEnginePrefab.GetComponent <ActionListManager>())
				{
					actionListManagerComponent = gameEnginePrefab.GetComponent <ActionListManager>();
					return actionListManagerComponent;
				}
				return null;
			}
		}


		public static LocalVariables localVariables
		{
			get
			{
				if (localVariablesComponent != null) return localVariablesComponent;
				else
				{
					SetGameEngine ();
				}

				if (gameEnginePrefab && gameEnginePrefab.GetComponent <LocalVariables>())
				{
					localVariablesComponent = gameEnginePrefab.GetComponent <LocalVariables>();
					return localVariablesComponent;
				}
				return null;
			}
		}


		public static MenuPreview menuPreview
		{
			get
			{
				if (menuPreviewComponent != null) return menuPreviewComponent;
				else
				{
					SetGameEngine ();
				}

				if (gameEnginePrefab && gameEnginePrefab.GetComponent <MenuPreview>())
				{
					menuPreviewComponent = gameEnginePrefab.GetComponent <MenuPreview>();
					return menuPreviewComponent;
				}
				return null;
			}
		}
	

		public static Player player
		{
			get
			{
				if (playerPrefab != null)
				{
					return playerPrefab;
				}
				else
				{
					if (GameObject.FindWithTag (Tags.player) && GameObject.FindWithTag (Tags.player).GetComponent <Player>())
					{
						playerPrefab = GameObject.FindWithTag (Tags.player).GetComponent <Player>();
						return playerPrefab;
					}
				}
				return null;
			}
		}


		public static MainCamera mainCamera
		{
			get
			{
				if (mainCameraPrefab != null)
				{
					return mainCameraPrefab;
				}
				else
				{
					MainCamera _mainCamera = (MainCamera) GameObject.FindObjectOfType (typeof (MainCamera));
					if (_mainCamera)
					{
						mainCameraPrefab = _mainCamera;
					}
					return mainCameraPrefab;
				}
			}
		}


		public static void ResetPlayer (Player ref_player, int ID, bool resetReferences, Quaternion _rotation)
		{
			// Delete current player
			if (GameObject.FindWithTag (Tags.player))
			{
				DestroyImmediate (GameObject.FindWithTag (Tags.player));
			}

			// Load new player
			if (ref_player)
			{
				SettingsManager settingsManager = AdvGame.GetReferences ().settingsManager;

				Player newPlayer = (Player) Instantiate (ref_player, Vector3.zero, _rotation);
				newPlayer.ID = ID;
				newPlayer.name = ref_player.name;
				playerPrefab = newPlayer;
				DontDestroyOnLoad (newPlayer);

				if (KickStarter.runtimeInventory)
				{
					KickStarter.runtimeInventory.SetNull ();
					KickStarter.runtimeInventory.RemoveRecipes ();

					// Clear inventory
					if (!settingsManager.shareInventory)
					{
						KickStarter.runtimeInventory.localItems.Clear ();
					}

					if (KickStarter.saveSystem != null && KickStarter.saveSystem.DoesPlayerDataExist (ID, false))
					{
						saveSystem.AssignPlayerData (ID, !settingsManager.shareInventory);
					}

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
				}

				if (newPlayer.GetComponent <Animation>())
				{
					// Hack: Force idle of Legacy characters
					AdvGame.PlayAnimClip (newPlayer.GetComponent <Animation>(), AdvGame.GetAnimLayerInt (AnimLayer.Base), newPlayer.idleAnim, AnimationBlendMode.Blend, WrapMode.Loop, 0f, null, false);
				}
				else if (newPlayer.spriteChild)
				{
					// Hack: update 2D sprites
					if (newPlayer.spriteChild.GetComponent <FollowSortingMap>())
					{
						newPlayer.spriteChild.GetComponent <FollowSortingMap>().UpdateSortingMap ();
					}
					newPlayer.UpdateSpriteChild (settingsManager.IsTopDown (), settingsManager.IsUnity2D ());
				}
				newPlayer.animEngine.PlayIdle ();
			}

			// Reset player references
			if (resetReferences)
			{
				KickStarter.sceneSettings.ResetPlayerReference ();
				KickStarter.playerMovement.AssignFPCamera ();
				KickStarter.stateHandler.IgnoreNavMeshCollisions ();
				KickStarter.stateHandler.GatherObjects (false);
				_Camera[] cameras = FindObjectsOfType (typeof (_Camera)) as _Camera[];
				foreach (_Camera camera in cameras)
				{
					camera.ResetTarget ();
				}
			}
		}


		private void Awake ()
		{
			// Test for key imports
			References references = (References) Resources.Load (Resource.references);
			if (references)
			{
				SceneManager sceneManager = AdvGame.GetReferences ().sceneManager;
				SettingsManager settingsManager = AdvGame.GetReferences ().settingsManager;
				ActionsManager actionsManager = AdvGame.GetReferences ().actionsManager;
				InventoryManager inventoryManager = AdvGame.GetReferences ().inventoryManager;
				VariablesManager variablesManager = AdvGame.GetReferences ().variablesManager;
				SpeechManager speechManager = AdvGame.GetReferences ().speechManager;
				CursorManager cursorManager = AdvGame.GetReferences ().cursorManager;
				MenuManager menuManager = AdvGame.GetReferences ().menuManager;
				
				if (sceneManager == null)
				{
					Debug.LogError ("No Scene Manager found - please set one using the Adventure Creator Kit wizard");
				}
				
				if (settingsManager == null)
				{
					Debug.LogError ("No Settings Manager found - please set one using the Adventure Creator Kit wizard");
				}
				else
				{
					if (settingsManager.IsInLoadingScene ())
					{
						Debug.Log ("Bypassing regular AC startup because the current scene is the 'Loading' scene.");
						return;
					}
					if (!GameObject.FindGameObjectWithTag (Tags.player))
					{
						KickStarter.ResetPlayer (settingsManager.GetDefaultPlayer (), settingsManager.GetDefaultPlayerID (), false, Quaternion.identity);
					}
					else
					{
						KickStarter.playerPrefab = GameObject.FindWithTag (Tags.player).GetComponent <Player>();

						if (sceneChanger != null && sceneChanger.GetPlayerOnTransition () != null && settingsManager.playerSwitching == PlayerSwitching.DoNotAllow)
						{
							// Replace "prefab" player with a local one if one exists
							GameObject[] playerObs = GameObject.FindGameObjectsWithTag (Tags.player);
							foreach (GameObject playerOb in playerObs)
							{
								if (playerOb.GetComponent <Player>() && sceneChanger.GetPlayerOnTransition () != playerOb.GetComponent <Player>())
								{
									KickStarter.sceneChanger.DestroyOldPlayer ();
									KickStarter.playerPrefab = playerOb.GetComponent <Player>();
									break;
								}
						    }
					    }
					}
				}
				
				if (actionsManager == null)
				{
					Debug.LogError ("No Actions Manager found - please set one using the main Adventure Creator window");
				}
				
				if (inventoryManager == null)
				{
					Debug.LogError ("No Inventory Manager found - please set one using the main Adventure Creator window");
				}
				
				if (variablesManager == null)
				{
					Debug.LogError ("No Variables Manager found - please set one using the main Adventure Creator window");
				}
				
				if (speechManager == null)
				{
					Debug.LogError ("No Speech Manager found - please set one using the main Adventure Creator window");
				}

				if (cursorManager == null)
				{
					Debug.LogError ("No Cursor Manager found - please set one using the main Adventure Creator window");
				}

				if (menuManager == null)
				{
					Debug.LogError ("No Menu Manager found - please set one using the main Adventure Creator window");
				}
				
				if (GameObject.FindWithTag (Tags.player) == null && KickStarter.settingsManager.movementMethod != MovementMethod.None)
				{
					Debug.LogWarning ("No Player found - please set one using the Settings Manager, tagging it as Player and placing it in a Resources folder");
				}
				
			}
			else
			{
				Debug.LogError ("No References object found. Please set one using the main Adventure Creator window");
			}

			if (persistentEnginePrefab == null)
			{
				try
				{
					persistentEnginePrefab = (GameObject) Instantiate (Resources.Load (Resource.persistentEngine));
					persistentEnginePrefab.name = AdvGame.GetName (Resource.persistentEngine);
				}
				catch {}
			}

			if (persistentEnginePrefab == null)
			{
				Debug.LogError ("No PersistentEngine prefab found - please place one in the Resources directory, and tag it as PersistentEngine");
			}
			else
			{
				if (persistentEnginePrefab.GetComponent <Options>() == null)
				{
					Debug.LogError (persistentEnginePrefab.name + " has no Options component attached.");
				}
				if (persistentEnginePrefab.GetComponent <RuntimeInventory>() == null)
				{
					Debug.LogError (persistentEnginePrefab.name + " has no RuntimeInventory component attached.");
				}
				if (persistentEnginePrefab.GetComponent <RuntimeVariables>() == null)
				{
					Debug.LogError (persistentEnginePrefab.name + " has no RuntimeVariables component attached.");
				}
				if (persistentEnginePrefab.GetComponent <PlayerMenus>() == null)
				{
					Debug.LogError (persistentEnginePrefab.name + " has no PlayerMenus component attached.");
				}
				if (persistentEnginePrefab.GetComponent <StateHandler>() == null)
				{
					Debug.LogError (persistentEnginePrefab.name + " has no StateHandler component attached.");
				}
				if (persistentEnginePrefab.GetComponent <SceneChanger>() == null)
				{
					Debug.LogError (persistentEnginePrefab.name + " has no SceneChanger component attached.");
				}
				if (persistentEnginePrefab.GetComponent <SaveSystem>() == null)
				{
					Debug.LogError (persistentEnginePrefab.name + " has no SaveSystem component attached.");
				}
				if (persistentEnginePrefab.GetComponent <LevelStorage>() == null)
				{
					Debug.LogError (persistentEnginePrefab.name + " has no LevelStorage component attached.");
				}
			}
			
			if (GameObject.FindWithTag (Tags.mainCamera) == null)
			{
				Debug.LogError ("No MainCamera found - please click 'Organise room objects' in the Scene Manager to create one.");
			}
			else
			{
				if (GameObject.FindWithTag (Tags.mainCamera).GetComponent <MainCamera>() == null)
				{
					Debug.LogError ("MainCamera has no MainCamera component.");
				}
			}
			
			if (this.GetComponent <MenuSystem>() == null)
			{
				Debug.LogError (this.name + " has no MenuSystem component attached.");
			}
			if (this.GetComponent <Dialog>() == null)
			{
				Debug.LogError (this.name + " has no Dialog component attached.");
			}
			if (this.GetComponent <PlayerInput>() == null)
			{
				Debug.LogError (this.name + " has no PlayerInput component attached.");
			}
			if (this.GetComponent <PlayerInteraction>() == null)
			{
				Debug.LogError (this.name + " has no PlayerInteraction component attached.");
			}
			if (this.GetComponent <PlayerMovement>() == null)
			{
				Debug.LogError (this.name + " has no PlayerMovement component attached.");
			}
			if (this.GetComponent <PlayerCursor>() == null)
			{
				Debug.LogError (this.name + " has no PlayerCursor component attached.");
			}
			if (this.GetComponent <PlayerQTE>() == null)
			{
				Debug.LogError (this.name + " has no PlayerQTE component attached.");
			}
			if (this.GetComponent <SceneSettings>() == null)
			{
				Debug.LogError (this.name + " has no SceneSettings component attached.");
			}
			else
			{
				if (this.GetComponent <SceneSettings>().navigationMethod == AC_NavigationMethod.meshCollider && this.GetComponent <SceneSettings>().navMesh == null)
				{
					// No NavMesh, are there Characters in the scene?
					AC.Char[] allChars = GameObject.FindObjectsOfType (typeof(AC.Char)) as AC.Char[];
					if (allChars.Length > 0)
					{
						Debug.LogWarning ("No NavMesh set. Characters will not be able to PathFind until one is defined - please choose one using the Scene Manager.");
					}
				}
				
				if (this.GetComponent <SceneSettings>().defaultPlayerStart == null)
				{
					Debug.LogWarning ("No default PlayerStart set.  The game may not be able to begin if one is not defined - please choose one using the Scene Manager.");
				}
			}
			if (this.GetComponent <NavigationManager>() == null)
			{
				Debug.LogError (this.name + " has no NavigationManager component attached.");
			}
			if (this.GetComponent <ActionListManager>() == null)
			{
				Debug.LogError (this.name + " has no ActionListManager component attached.");
			}
		}



		private void OnLevelWasLoaded ()
		{
			if (GameObject.FindWithTag (Tags.player) && GameObject.FindWithTag (Tags.player).GetComponent <Player>())
			{
				KickStarter.playerPrefab = GameObject.FindWithTag (Tags.player).GetComponent <Player>();
			}
		}


		public void TurnOnAC ()
		{
			if (KickStarter.stateHandler)
			{
				KickStarter.stateHandler.gameState = GameState.Normal;
			}
		}
		
		
		public void TurnOffAC ()
		{
			KickStarter.actionListManager.KillAllLists ();
			KickStarter.dialog.KillDialog (true, true);

			Moveable[] moveables = FindObjectsOfType (typeof (Moveable)) as Moveable[];
			foreach (Moveable moveable in moveables)
			{
				moveable.Kill ();
			}
			
			Char[] chars = FindObjectsOfType (typeof (Char)) as Char[];
			foreach (Char _char in chars)
			{
				_char.EndPath ();
			}

			if (KickStarter.stateHandler)
			{
				KickStarter.stateHandler.gameState = GameState.Cutscene;
			}
		}
		
	}

}