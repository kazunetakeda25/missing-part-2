/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"ActionPlayerCheck.cs"
 * 
 *	This action checks to see which
 *	Player prefab is currently being controlled.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	[System.Serializable]
	public class ActionPlayerCheck : ActionCheck
	{
		
		public int playerID;
		public int playerNumber;

		#if UNITY_EDITOR
		private SettingsManager settingsManager;
		#endif

		
		public ActionPlayerCheck ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Player;
			title = "Check";
			description = "Queries which Player prefab is currently being controlled. This only applies to games for which 'Player switching' has been allowed in the Settings Manager.";
		}
		

		override public bool CheckCondition ()
		{
			if (KickStarter.player && KickStarter.player.ID == playerID)
			{
				return true;
			}

			return false;
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI ()
		{
			if (!settingsManager)
			{
				settingsManager = AdvGame.GetReferences ().settingsManager;
			}
			
			if (!settingsManager)
			{
				return;
			}

			if (settingsManager.playerSwitching == PlayerSwitching.DoNotAllow)
			{
				EditorGUILayout.HelpBox ("This Action requires Player Switching to be allowed, as set in the Settings Manager.", MessageType.Info);
				return;
			}

			// Create a string List of the field's names (for the PopUp box)
			List<string> labelList = new List<string>();
			
			int i = 0;
			playerNumber = -1;
			
			if (settingsManager.players.Count > 0)
			{
				foreach (PlayerPrefab playerPrefab in settingsManager.players)
				{
					if (playerPrefab.playerOb != null)
					{
						labelList.Add (playerPrefab.playerOb.name);
					}
					else
					{
						labelList.Add ("(Undefined prefab)");
					}
					
					// If a player has been removed, make sure selected player is still valid
					if (playerPrefab.ID == playerID)
					{
						playerNumber = i;
					}
					
					i++;
				}
				
				if (playerNumber == -1)
				{
					// Wasn't found (item was possibly deleted), so revert to zero
					Debug.LogWarning ("Previously chosen Player no longer exists!");
					
					playerNumber = 0;
					playerID = 0;
				}
				
				playerNumber = EditorGUILayout.Popup ("Current Player is:", playerNumber, labelList.ToArray());
				playerID = settingsManager.players[playerNumber].ID;
			}
			
			else
			{
				EditorGUILayout.LabelField ("No players exist!");
				playerID = -1;
				playerNumber = -1;
			}
		}


		override public string SetLabel ()
		{
			if (settingsManager && settingsManager.playerSwitching == PlayerSwitching.Allow)
			{
				if (settingsManager.players.Count > 0 && settingsManager.players.Count > playerNumber)
				{
					if (playerNumber > -1)
					{
						if (settingsManager.players[playerNumber].playerOb != null)
						{
							return " (" + settingsManager.players[playerNumber].playerOb.name + ")";
						}
						else
						{
							return (" (Undefined prefab");
						}
					}
				}
			}
			
			return "";
		}
		
		#endif
		
	}

}