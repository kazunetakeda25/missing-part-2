/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionListManager.cs"
 * 
 *	This script keeps track of which ActionLists
 *	are running in a scene.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{
	
	public class ActionListManager : MonoBehaviour
	{
		
		private bool playCutsceneOnVarChange = false;
		private bool saveAfterCutscene = false;
		
		private Conversation conversationOnEnd;
		private List<ActionList> activeLists = new List<ActionList>();
		private List<SkipList> skipQueue = new List<SkipList>();
		private SkipList activeConversationPoint = new SkipList ();

		
		private void Awake ()
		{
			activeLists.Clear ();
		}
		
		
		public void UpdateActionListManager ()
		{
			if (saveAfterCutscene && !IsGameplayBlocked ())
			{
				saveAfterCutscene = false;
				SaveSystem.SaveAutoSave ();
			}
			
			if (playCutsceneOnVarChange && KickStarter.stateHandler && (KickStarter.stateHandler.gameState == GameState.Normal || KickStarter.stateHandler.gameState == GameState.DialogOptions))
			{
				playCutsceneOnVarChange = false;
				
				if (KickStarter.sceneSettings.cutsceneOnVarChange != null)
				{
					KickStarter.sceneSettings.cutsceneOnVarChange.Interact ();
				}
			}
		}
		
		
		public void EndCutscene ()
		{
			if (!IsGameplayBlocked ())
			{
				return;
			}
			
			if (AdvGame.GetReferences ().settingsManager.blackOutWhenSkipping)
			{
				KickStarter.mainCamera.HideScene ();
			}
			
			// Stop all non-looping sound
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

			for (int i=0; i<activeLists.Count; i++)
			{
				if (!ListIsInSkipQueue (activeLists[i]) && activeLists[i].IsSkippable ())
				{
					// Kill, but do isolated, to bypass setting GameState etc
					ActionList listToRemove = activeLists[i];
					listToRemove.Reset ();
					activeLists.RemoveAt (i);
					i-=1;
					
					if (listToRemove is RuntimeActionList)
					{
						Destroy (listToRemove.gameObject);
					}
				}
			}
			
			for (int i=0; i<skipQueue.Count; i++)
			{
				skipQueue[i].Skip ();
			}
		}
			
		
		public bool AreActionListsRunning ()
		{
			if (activeLists.Count > 0)
			{
				return true;
			}
			return false;
		}
		
		
		#if UNITY_EDITOR
		
		private void OnGUI ()
		{
			if (KickStarter.settingsManager.showActiveActionLists)
			{
				GUILayout.BeginVertical ("Button");
				GUILayout.Label ("Current game state: " + KickStarter.stateHandler.gameState.ToString ());
				GUILayout.Space (4f);

				if (activeLists.Count > 0)
				{
					GUILayout.Label ("Current ActionLists running:");

					foreach (ActionList list in activeLists)
					{
						GUILayout.Label (list.gameObject.name, "Button");
					}

					if (IsGameplayBlocked ())
					{
						GUILayout.Space (4f);
						GUILayout.Label ("Gameplay is blocked");
					}
				}

				/*if (skipQueue.Count > 0)
				{
					GUILayout.Label ("In skip queue:");
					
					foreach (SkipList list in skipQueue)
					{
						GUILayout.Label (list.GetName (), "Button");
					}
				}*/

				GUILayout.EndVertical ();
			}
		}
		
		#endif


		private bool ListIsInSkipQueue (ActionList _list)
		{
			foreach (SkipList skipList in skipQueue)
			{
				if (skipList.actionList == _list)
				{
					return true;
				}
			}
			return false;
		}
		
		
		public void AddToList (ActionList _list, bool addToSkipQueue, int _startIndex)
		{
			if (skipQueue.Count == 0 && _list.IsSkippable ())
			{
				addToSkipQueue = true;
			}

			if (!IsListRunning (_list))
			{
				activeLists.Add (_list);

				if (addToSkipQueue && !ListIsInSkipQueue (_list) && _list.IsSkippable ())
				{
					skipQueue.Add (new SkipList (_list, _startIndex));
				}
			}
			
			if (_list.conversation)
			{
				conversationOnEnd = _list.conversation;
			}
			
			if (_list is RuntimeActionList && _list.actionListType == ActionListType.PauseGameplay && !_list.unfreezePauseMenus && KickStarter.playerMenus.ArePauseMenusOn (null))
			{
				// Don't affect the gamestate if we want to remain frozen
				return;
			}
			
			SetCorrectGameState ();
		}
		
		
		public void EndList (ActionList _list)
		{
			if (IsListRunning (_list))
			{
				activeLists.Remove (_list);
			}

			_list.Reset ();
			
			if (_list.conversation == conversationOnEnd && _list.conversation != null)
			{
				if (KickStarter.stateHandler)
				{
					KickStarter.stateHandler.gameState = GameState.Cutscene;
				}
				else
				{
					Debug.LogWarning ("Could not set correct GameState!");
				}

				ResetSkipVars ();
				conversationOnEnd.Interact ();
				conversationOnEnd = null;
			}
			else
			{
				if (_list is RuntimeActionList && _list.actionListType == ActionListType.PauseGameplay && !_list.unfreezePauseMenus && KickStarter.playerMenus.ArePauseMenusOn (null))
				{
					// Don't affect the gamestate if we want to remain frozen
					if (KickStarter.stateHandler.gameState != GameState.Cutscene)
					{
						ResetSkipVars ();
					}
				}
				else
				{
					SetCorrectGameStateEnd ();
				}
			}
			
			if (_list.autosaveAfter)
			{
				if (!IsGameplayBlocked ())
				{
					SaveSystem.SaveAutoSave ();
				}
				else
				{
					saveAfterCutscene = true;
				}
			}
			
			if (_list is RuntimeActionList)
			{
				RuntimeActionList runtimeActionList = (RuntimeActionList) _list;
				runtimeActionList.DestroySelf ();
			}
		}


		public void DestroyAssetList (ActionListAsset asset)
		{
			RuntimeActionList[] runtimeActionLists = FindObjectsOfType (typeof (RuntimeActionList)) as RuntimeActionList[];
			foreach (RuntimeActionList runtimeActionList in runtimeActionLists)
			{
				if (runtimeActionList.assetSource == asset)
				{
					if (activeLists.Contains (runtimeActionList))
					{
						activeLists.Remove (runtimeActionList);
					}
					Destroy (runtimeActionList.gameObject);
				}
			}
		}


		public void EndAssetList (ActionListAsset asset, Action _action)
		{
			RuntimeActionList[] runtimeActionLists = FindObjectsOfType (typeof (RuntimeActionList)) as RuntimeActionList[];
			foreach (RuntimeActionList runtimeActionList in runtimeActionLists)
			{
				if (runtimeActionList.assetSource == asset)
				{
					if (_action == null || !runtimeActionList.actions.Contains (_action))
					{
						EndList (runtimeActionList);
					}
					else if (_action != null) Debug.Log ("Left " + runtimeActionList.gameObject.name + " alone.");
				}
			}
		}
		
		
		public void EndAssetList (ActionListAsset asset)
		{
			EndAssetList (asset, null);
		}
		
		
		public void VariableChanged ()
		{
			playCutsceneOnVarChange = true;
		}


		public bool IsListRunning (ActionListAsset _assetSource)
		{
			RuntimeActionList[] runtimeActionLists = FindObjectsOfType (typeof (RuntimeActionList)) as RuntimeActionList[];
			foreach (RuntimeActionList runtimeActionList in runtimeActionLists)
			{
				if (runtimeActionList.assetSource == _assetSource)
				{
					return true;
				}
			}
			return false;
		}
		
		
		public bool IsListRunning (ActionList _list)
		{
			foreach (ActionList list in activeLists)
			{
				if (list == _list)
				{
					return true;
				}
			}
			
			return false;
		}
		
		
		public void KillAllLists ()
		{
			foreach (ActionList _list in activeLists)
			{
				_list.Reset ();
				
				if (_list is RuntimeActionList)
				{
					RuntimeActionList runtimeActionList = (RuntimeActionList) _list;
					runtimeActionList.DestroySelf ();
				}
			}
			
			activeLists.Clear ();
		}
		
		
		public static void KillAll ()
		{
			KickStarter.actionListManager.KillAllLists ();
		}
		
		
		private void SetCorrectGameStateEnd ()
		{
			if (KickStarter.stateHandler != null)
			{
				if (KickStarter.playerMenus.ArePauseMenusOn (null))
				{
					KickStarter.mainCamera.PauseGame ();
				}
				else
				{
					KickStarter.stateHandler.RestoreLastNonPausedState ();
				}
			}
			else
			{
				Debug.LogWarning ("Could not set correct GameState!");
			}

			if (KickStarter.stateHandler.gameState != GameState.Cutscene)
			{
				ResetSkipVars ();
			}
		}


		private void ResetSkipVars ()
		{
			if (!IsGameplayBlocked ())
			{
				KickStarter.playerInput.ignoreNextConversationSkip = false;
				skipQueue.Clear ();
				KickStarter.runtimeVariables.BackupAllValues ();
				KickStarter.localVariables.BackupAllValues ();
			}
		}

		
		private void SetCorrectGameState ()
		{
			if (KickStarter.stateHandler != null)
			{
				if (IsGameplayBlocked ())
				{
					if (KickStarter.stateHandler.gameState != GameState.Cutscene)
					{
						ResetSkipVars ();
					}
					KickStarter.stateHandler.gameState = GameState.Cutscene;
				}
				else if (KickStarter.playerMenus.ArePauseMenusOn (null))
				{
					KickStarter.stateHandler.gameState = GameState.Paused;
					KickStarter.sceneSettings.PauseGame ();
				}
				else
				{
					if (KickStarter.playerInput.activeConversation != null)
					{
						KickStarter.stateHandler.gameState = GameState.DialogOptions;
					}
					else
					{
						KickStarter.stateHandler.gameState = GameState.Normal;
					}
				}
			}
			else
			{
				Debug.LogWarning ("Could not set correct GameState!");
			}
		}
		
		
		public bool IsGameplayBlocked ()
		{
			foreach (ActionList list in activeLists)
			{
				if (list.actionListType == ActionListType.PauseGameplay)
				{
					return true;
				}
			}
			return false;
		}
		
		
		public bool IsInSkippableCutscene ()
		{
			foreach (ActionList list in activeLists)
			{
				if (list.IsSkippable ())
				{
					return true;
				}
			}
			
			return false;
		}
		
		
		private void OnDestroy ()
		{
			activeLists.Clear ();
		}


		public void SetConversationPoint (ActionConversation actionConversation)
		{
			if (actionConversation == null)
			{
				activeConversationPoint = new SkipList();
			}

			foreach (ActionList actionList in activeLists)
			{
	           foreach (Action action in actionList.actions)
				{
					if (action == actionConversation)
					{
						activeConversationPoint = new SkipList (actionList, actionList.actions.IndexOf (action));
						actionList.Kill ();
						return;
					}
				}
			}
		}


		public bool OverrideConversation (int optionIndex)
		{
			KickStarter.playerInput.lastConversationOption = optionIndex;

			SkipList tempPoint = new SkipList (activeConversationPoint);
			activeConversationPoint = new SkipList ();

			if (tempPoint != null && (tempPoint.actionList != null || tempPoint.actionListAsset != null))
			{
				tempPoint.Resume ();
				return true;
			}
			return false;
		}
		
	}
	
}