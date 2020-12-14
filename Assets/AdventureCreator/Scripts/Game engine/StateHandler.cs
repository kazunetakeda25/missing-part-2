/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"SceneHandler.cs"
 * 
 *	This script stores the gameState variable, which is used by
 *	other scripts to determine if the game is running normal gameplay,
 *	in a cutscene, paused, or displaying conversation options.
 * 
 */

using UnityEngine;
namespace AC
{

	public class StateHandler : MonoBehaviour
	{
		
		public GameState gameState = GameState.Normal;

		private GameState previousUpdateState = GameState.Normal;
		private GameState lastNonPausedState = GameState.Normal;

		public bool cursorIsOff;
		public bool inputIsOff;
		public bool interactionIsOff;
		public bool menuIsOff;
		public bool movementIsOff;
		public bool cameraIsOff;
		public bool triggerIsOff;
		public bool playerIsOff;

		public bool playedGlobalOnStart = false;
		public Texture2D tempFadeTexture = null;

		private ArrowPrompt[] arrowPrompts;
		private DragBase[] dragBases;
		private Parallax2D[] parallax2Ds;
		private Hotspot[] hotspots;
		private Highlight[] highlights;
		private GameCamera2D[] gameCamera2Ds;
		private Sound[] sounds;


		private void Awake ()
		{
			Time.timeScale = 1f;
			DontDestroyOnLoad (this);
			GetReferences ();
		}


		private void OnLevelWasLoaded ()
		{
			GetReferences ();
		}


		public bool PlayGlobalOnStart ()
		{
			if (playedGlobalOnStart)
			{
				return false;
			}

			if (KickStarter.settingsManager.actionListOnStart)
			{
				AdvGame.RunActionListAsset (KickStarter.settingsManager.actionListOnStart);
				playedGlobalOnStart = true;
				return true;
			}

			return false;
		}


		private void GetReferences ()
		{
			if (KickStarter.settingsManager != null && KickStarter.settingsManager.IsInLoadingScene ())
			{
				return;
			}
			GatherObjects ();
		}


		public void GatherObjects (bool afterDelete = false)
		{
			dragBases = FindObjectsOfType (typeof (DragBase)) as DragBase[];
			hotspots = FindObjectsOfType (typeof (Hotspot)) as Hotspot[];
			arrowPrompts = FindObjectsOfType (typeof (ArrowPrompt)) as ArrowPrompt[];
			parallax2Ds = FindObjectsOfType (typeof (Parallax2D)) as Parallax2D[];
			highlights = FindObjectsOfType (typeof (Highlight)) as Highlight[];
			gameCamera2Ds = FindObjectsOfType (typeof (GameCamera2D)) as GameCamera2D[];
			sounds = FindObjectsOfType (typeof (Sound)) as Sound[];

			if (!afterDelete)
			{
				IgnoreNavMeshCollisions ();
			}
		}


		public void IgnoreNavMeshCollisions ()
		{
			#if UNITY_5
			Collider[] allColliders = FindObjectsOfType (typeof(Collider)) as Collider[];
			NavMeshBase[] navMeshes = FindObjectsOfType (typeof(NavMeshBase)) as NavMeshBase[];
			foreach (NavMeshBase navMesh in navMeshes)
			{
				if (navMesh.ignoreCollisions)
				{
					Collider _collider = navMesh.GetComponent <Collider>();
					if (_collider != null && _collider.enabled)
					{
						foreach (Collider otherCollider in allColliders)
						{
							if (!_collider.isTrigger && !otherCollider.isTrigger && otherCollider.enabled && !(_collider is TerrainCollider) && _collider != otherCollider)
							{
								Physics.IgnoreCollision (_collider, otherCollider);
							}
						}
					}
				}
			}
			#endif
		}


		private void Update ()
		{
			if (KickStarter.settingsManager != null && KickStarter.settingsManager.IsInLoadingScene ())
			{
				return;
			}

			if (gameState != GameState.Paused)
			{
				lastNonPausedState = gameState;
			}

			KickStarter.dialog._Update ();

			if (!inputIsOff)
			{
				KickStarter.playerInput.UpdateInput ();

				if (gameState == GameState.Normal)
				{
					KickStarter.playerInput.UpdateDirectInput ();
				}

				if (gameState != GameState.Paused)
				{
					KickStarter.playerQTE.UpdateQTE ();
					//KickStarter.dialog.UpdateSkipDialogue ();
				}
			}

			if (!cursorIsOff)
			{
				KickStarter.playerCursor.UpdateCursor ();
			}

			if (!menuIsOff)
			{
				KickStarter.playerMenus.UpdateAllMenus ();
			}

			if (!interactionIsOff)
			{
				KickStarter.playerInteraction.UpdateInteraction ();

				foreach (Highlight highlight in highlights)
				{
					highlight._Update ();
				}

				if (KickStarter.settingsManager.hotspotDetection == HotspotDetection.MouseOver && KickStarter.settingsManager.scaleHighlightWithMouseProximity)
				{
					if (gameState == GameState.Normal)
					{
						foreach (Hotspot hotspot in hotspots)
						{
							hotspot.SetProximity (true);
						}
					}
					else
					{
						foreach (Hotspot hotspot in hotspots)
						{
							hotspot.SetProximity (false);
						}
					}
				}
			}

			KickStarter.actionListManager.UpdateActionListManager ();

			if (!movementIsOff)
			{
				foreach (DragBase dragBase in dragBases)
				{
					dragBase.UpdateMovement ();
				}

				if (gameState == GameState.Normal && KickStarter.settingsManager && KickStarter.settingsManager.movementMethod != MovementMethod.None)
				{
					KickStarter.playerMovement.UpdatePlayerMovement ();
				}

				KickStarter.playerMovement.UpdateFPCamera ();
			}

			if (!interactionIsOff)
			{
				KickStarter.playerInteraction.UpdateInventory ();
			}

			if (!cameraIsOff)
			{
				foreach (GameCamera2D gameCamera2D in gameCamera2Ds)
				{
					gameCamera2D._Update ();
				}
			}

			foreach (Sound sound in sounds)
			{
				sound._Update ();
			}

			if (HasGameStateChanged ())
			{
				if (KickStarter.settingsManager.movementMethod == MovementMethod.UltimateFPS)
				{
					UltimateFPSIntegration._Update (gameState);
				}
				else if (KickStarter.settingsManager.movementMethod == MovementMethod.FirstPerson)
				{
					if (gameState == GameState.DialogOptions || gameState == GameState.Normal)
					{
						KickStarter.mainCamera.SetFirstPerson ();
					}
				}

				if (gameState != GameState.Paused)
				{
					AudioListener.pause = false;
				}
			}

			previousUpdateState = gameState;
		}


		private void LateUpdate ()
		{
			if (KickStarter.settingsManager != null && KickStarter.settingsManager.IsInLoadingScene ())
			{
				return;
			}

			foreach (Parallax2D parallax2D in parallax2Ds)
			{
				parallax2D.UpdateOffset ();
			}

			if (!cameraIsOff)
			{
				KickStarter.mainCamera._LateUpdate ();
			}
		}


		public void UpdateAllMaxVolumes ()
		{
			foreach (Sound sound in sounds)
			{
				sound.SetMaxVolume ();
			}
		}


		private bool HasGameStateChanged ()
		{
			if (previousUpdateState != gameState)
			{
				return true;
			}
			return false;
		}


		private void OnGUI ()
		{
			if (KickStarter.settingsManager != null && KickStarter.settingsManager.IsInLoadingScene ())
			{
				return;
			}

			if (!cursorIsOff && gameState == GameState.Normal && KickStarter.settingsManager)
			{
				if (KickStarter.settingsManager.hotspotIconDisplay != HotspotIconDisplay.Never)
				{
					foreach (Hotspot hotspot in hotspots)
					{
						hotspot.DrawHotspotIcon ();
					}
				}

				foreach (DragBase dragBase in dragBases)
				{
					dragBase.DrawGrabIcon ();
				}
			}

			if (!inputIsOff)
			{
				if (gameState == GameState.DialogOptions)
				{
					KickStarter.playerInput.DetectNumerics ();
				}
				KickStarter.playerInput.DrawDragLine ();
				
				foreach (ArrowPrompt arrowPrompt in arrowPrompts)
				{
					arrowPrompt.DrawArrows ();
				}
			}

			if (!menuIsOff)
			{
				KickStarter.playerMenus.DrawMenus ();
			}

			if (!cursorIsOff)
			{
				KickStarter.playerCursor.DrawCursor ();
			}

			if (!cameraIsOff)
			{
				KickStarter.mainCamera.DrawCameraFade ();
			}
		}


		public GameState GetLastNonPausedState ()
		{
			return lastNonPausedState;
		}
		
		
		public void RestoreLastNonPausedState ()
		{
			if (KickStarter.playerInteraction.inPreInteractionCutscene)
			{
				gameState = GameState.Cutscene;
				return;
			}

			KickStarter.playerInteraction.inPreInteractionCutscene = false;
			if (KickStarter.actionListManager.IsGameplayBlocked ())
			{
				gameState = GameState.Cutscene;
			}
			else if (KickStarter.playerInput.activeConversation != null)
			{
				gameState = GameState.DialogOptions;
			}
			else
			{
				gameState = GameState.Normal;
			}
		}
		

		public void TurnOnAC ()
		{
			gameState = GameState.Normal;
		}
		
		
		public void TurnOffAC ()
		{
			if (KickStarter.actionListManager)
			{
				KickStarter.actionListManager.KillAllLists ();
			}

			if (KickStarter.dialog)
			{
				KickStarter.dialog.KillDialog (true, true);
			}

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
			
			gameState = GameState.Cutscene;
		}

	}

}