/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"ActionPlayerLock.cs"
 * 
 *	This action constrains the player in various ways (movement, saving etc)
 *	In Direct control mode, the player can be assigned a path,
 *	and will only be able to move along that path during gameplay.
 * 
 */

using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	[System.Serializable]
	public class ActionPlayerLock : Action
	{
		
		public LockType doUpLock = LockType.NoChange;
		public LockType doDownLock = LockType.NoChange;
		public LockType doLeftLock = LockType.NoChange;
		public LockType doRightLock = LockType.NoChange;
		
		public PlayerMoveLock doRunLock = PlayerMoveLock.NoChange;
		public LockType freeAimLock = LockType.NoChange;
		public LockType doGravityLock = LockType.NoChange;
		public LockType doHotspotHeadTurnLock = LockType.NoChange;
		public Paths movePath;

		
		public ActionPlayerLock ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Player;
			title = "Constrain";
			description = "Locks and unlocks various aspects of Player control. When using Direct or First Person control, can also be used to specify a Path object to restrict movement to.";
		}
		
		
		override public float Run ()
		{
			Player player = KickStarter.player;

			if (KickStarter.playerInput)
			{
				if (IsSingleLockMovement ())
				{
					doLeftLock = doUpLock;
					doRightLock = doUpLock;
					doDownLock = doUpLock;
				}

				if (doUpLock == LockType.Disabled)
				{
					KickStarter.playerInput.SetUpLock (true);
				}
				else if (doUpLock == LockType.Enabled)
				{
					KickStarter.playerInput.SetUpLock (false);
				}
		
				if (doDownLock == LockType.Disabled)
				{
					KickStarter.playerInput.isDownLocked = true;
				}
				else if (doDownLock == LockType.Enabled)
				{
					KickStarter.playerInput.isDownLocked = false;
				}
				
				if (doLeftLock == LockType.Disabled)
				{
					KickStarter.playerInput.isLeftLocked = true;
				}
				else if (doLeftLock == LockType.Enabled)
				{
					KickStarter.playerInput.isLeftLocked = false;
				}
		
				if (doRightLock == LockType.Disabled)
				{
					KickStarter.playerInput.isRightLocked = true;
				}
				else if (doRightLock == LockType.Enabled)
				{
					KickStarter.playerInput.isRightLocked = false;
				}

				if (IsInFirstPerson ())
				{
					if (freeAimLock == LockType.Disabled)
					{
						CrosshairFader.Instance.Hide();
						KickStarter.playerInput.SetFreeAimLock (true);
					}
					else if (freeAimLock == LockType.Enabled)
					{
						CrosshairFader.Instance.Show();
						KickStarter.playerInput.SetFreeAimLock (false);
					}
				}

				if (IsUltimateFPS ())
				{
					return 0f;
				}

				if (doRunLock != PlayerMoveLock.NoChange)
				{
					KickStarter.playerInput.runLock = doRunLock;
				}
			}
			
			if (player)
			{
				if (movePath)
				{
					player.SetLockedPath (movePath);
					player.SetMoveDirectionAsForward ();
				}
				else if (player.activePath)
				{
					player.EndPath ();
				}

				if (doGravityLock == LockType.Enabled)
				{
					player.ignoreGravity = false;
				}
				else if (doGravityLock == LockType.Disabled)
				{
					player.ignoreGravity = true;
				}

				if (AllowHeadTurning ())
				{
					if (doHotspotHeadTurnLock == LockType.Disabled)
					{
						player.lockHotspotHeadTurning = true;
					}
					else if (doHotspotHeadTurnLock == LockType.Enabled)
					{
						player.lockHotspotHeadTurning = false;
					}
				}
			}
			
			return 0f;
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI ()
		{
			if (IsSingleLockMovement ())
			{
				doUpLock = (LockType) EditorGUILayout.EnumPopup ("Movement:", doUpLock);
			}
			else
			{
				doUpLock = (LockType) EditorGUILayout.EnumPopup ("Up movement:", doUpLock);
				doDownLock = (LockType) EditorGUILayout.EnumPopup ("Down movement:", doDownLock);
				doLeftLock = (LockType) EditorGUILayout.EnumPopup ("Left movement:", doLeftLock);
				doRightLock = (LockType) EditorGUILayout.EnumPopup ("Right movement:", doRightLock);
			}

			if (IsInFirstPerson ())
			{
				freeAimLock = (LockType) EditorGUILayout.EnumPopup ("Free-aiming:", freeAimLock);
			}

			if (!IsUltimateFPS ())
			{
				doRunLock = (PlayerMoveLock) EditorGUILayout.EnumPopup ("Walk / run:", doRunLock);
				doGravityLock = (LockType) EditorGUILayout.EnumPopup ("Affected by gravity?", doGravityLock);
				movePath = (Paths) EditorGUILayout.ObjectField ("Move path:", movePath, typeof (Paths), true);
			}

			if (AllowHeadTurning ())
			{
				doHotspotHeadTurnLock = (LockType) EditorGUILayout.EnumPopup ("Hotspot head-turning?", doHotspotHeadTurnLock);
			}
			
			AfterRunningOption ();
		}
		
		#endif


		private bool AllowHeadTurning ()
		{
			if (AdvGame.GetReferences ().settingsManager != null && AdvGame.GetReferences ().settingsManager.cameraPerspective != CameraPerspective.TwoD && AdvGame.GetReferences ().settingsManager.playerFacesHotspots)
			{
				return true;
			}
			return false;
		}


		private bool IsSingleLockMovement ()
		{
			if (AdvGame.GetReferences ().settingsManager)
			{
				SettingsManager settingsManager = AdvGame.GetReferences ().settingsManager;
				if (settingsManager.movementMethod == MovementMethod.PointAndClick || settingsManager.movementMethod == MovementMethod.Drag || settingsManager.movementMethod == MovementMethod.UltimateFPS || settingsManager.movementMethod == MovementMethod.StraightToCursor)
				{
					return true;
				}
			}
			return false;
		}


		private bool IsInFirstPerson ()
		{
			if (AdvGame.GetReferences ().settingsManager && AdvGame.GetReferences ().settingsManager.IsInFirstPerson ())
			{
				return true;
			}
			return false;
		}


		private bool IsUltimateFPS ()
		{
			if (AdvGame.GetReferences ().settingsManager && AdvGame.GetReferences ().settingsManager.movementMethod == MovementMethod.UltimateFPS)
			{
				return true;
			}
			return false;
		}
	}

}