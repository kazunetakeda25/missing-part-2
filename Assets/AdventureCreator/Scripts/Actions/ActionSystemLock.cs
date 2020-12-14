/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"ActionSystemLock.cs"
 * 
 *	This action handles the enabling / disabling
 *	of individual AC systems, allowing for
 *	minigames or other non-adventure elements
 *	to be run.
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
	public class ActionSystemLock : Action
	{

		public bool changeMovementMethod = false;
		public MovementMethod newMovementMethod;

		public LockType cursorLock = LockType.NoChange;
		public LockType inputLock = LockType.NoChange;
		public LockType interactionLock = LockType.NoChange;
		public LockType menuLock = LockType.NoChange;
		public LockType movementLock = LockType.NoChange;
		public LockType cameraLock = LockType.NoChange;
		public LockType triggerLock = LockType.NoChange;
		public LockType playerLock = LockType.NoChange;
		public LockType saveLock = LockType.NoChange;

		
		public ActionSystemLock ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Engine;
			title = "Manage systems";
			description = "Enables and disables individual systems within Adventure Creator, such as Interactions. Can also be used to change the 'Movement method', as set in the Settings Manager, but note that this change will not be recorded in save games.";
		}
		
		
		override public float Run ()
		{
			if (changeMovementMethod)
			{
				if (KickStarter.settingsManager.IsInFirstPerson () && newMovementMethod != MovementMethod.FirstPerson && newMovementMethod != MovementMethod.UltimateFPS)
				{
					KickStarter.playerInput.cursorIsLocked = false;
				}
				else if (!KickStarter.settingsManager.IsInFirstPerson () && (newMovementMethod == MovementMethod.FirstPerson || newMovementMethod == MovementMethod.UltimateFPS))
				{
					KickStarter.playerInput.cursorIsLocked = KickStarter.settingsManager.lockCursorOnStart;
				}

				KickStarter.settingsManager.movementMethod = newMovementMethod;

				if (newMovementMethod == MovementMethod.UltimateFPS)
				{
					UltimateFPSIntegration.SetCameraState (KickStarter.playerInput.cursorIsLocked);
				}
			}

			if (cursorLock == LockType.Enabled)
			{
				KickStarter.stateHandler.cursorIsOff = false;
			}
			else if (cursorLock == LockType.Disabled)
			{
				KickStarter.stateHandler.cursorIsOff = true;
			}

			if (inputLock == LockType.Enabled)
			{
				KickStarter.stateHandler.inputIsOff = false;
			}
			else if (inputLock == LockType.Disabled)
			{
				KickStarter.stateHandler.inputIsOff = true;
			}

			if (interactionLock == LockType.Enabled)
			{
				KickStarter.stateHandler.interactionIsOff = false;
			}
			else if (interactionLock == LockType.Disabled)
			{
				KickStarter.stateHandler.interactionIsOff = true;
			}

			if (menuLock == LockType.Enabled)
			{
				KickStarter.stateHandler.menuIsOff = false;
			}
			else if (menuLock == LockType.Disabled)
			{
				KickStarter.stateHandler.menuIsOff = true;
			}

			if (movementLock == LockType.Enabled)
			{
				KickStarter.stateHandler.movementIsOff = false;
			}
			else if (movementLock == LockType.Disabled)
			{
				KickStarter.stateHandler.movementIsOff = true;
			}

			if (cameraLock == LockType.Enabled)
			{
				KickStarter.stateHandler.cameraIsOff = false;
			}
			else if (cameraLock == LockType.Disabled)
			{
				KickStarter.stateHandler.cameraIsOff = true;
			}

			if (triggerLock == LockType.Enabled)
			{
				KickStarter.stateHandler.triggerIsOff = false;
			}
			else if (triggerLock == LockType.Disabled)
			{
				KickStarter.stateHandler.triggerIsOff = true;
			}

			if (playerLock == LockType.Enabled)
			{
				KickStarter.stateHandler.playerIsOff = false;
			}
			else if (playerLock == LockType.Disabled)
			{
				KickStarter.stateHandler.playerIsOff = true;
			}

			if (saveLock == LockType.Disabled)
			{
				KickStarter.playerMenus.lockSave = true;
			}
			else if (saveLock == LockType.Enabled)
			{
				KickStarter.playerMenus.lockSave = false;
			}

			return 0f;
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI ()
		{
			changeMovementMethod = EditorGUILayout.BeginToggleGroup ("Change movement method?", changeMovementMethod);
			newMovementMethod = (MovementMethod) EditorGUILayout.EnumPopup ("Movement method:", newMovementMethod);
			EditorGUILayout.EndToggleGroup ();

			EditorGUILayout.Space ();

			cursorLock = (LockType) EditorGUILayout.EnumPopup ("Cursor:", cursorLock);
			inputLock = (LockType) EditorGUILayout.EnumPopup ("Input:", inputLock);
			interactionLock = (LockType) EditorGUILayout.EnumPopup ("Interactions:", interactionLock);
			menuLock = (LockType) EditorGUILayout.EnumPopup ("Menus:", menuLock);
			movementLock = (LockType) EditorGUILayout.EnumPopup ("Movement:", movementLock);
			cameraLock = (LockType) EditorGUILayout.EnumPopup ("Camera:", cameraLock);
			triggerLock = (LockType) EditorGUILayout.EnumPopup ("Triggers:", triggerLock);
			playerLock = (LockType) EditorGUILayout.EnumPopup ("Player:", playerLock);
			saveLock = (LockType) EditorGUILayout.EnumPopup ("Saving:", saveLock);

			AfterRunningOption ();
		}
		
		#endif
		
	}

}