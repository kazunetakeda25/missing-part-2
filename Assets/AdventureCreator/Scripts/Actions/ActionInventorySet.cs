/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionInventorySet.cs"
 * 
 *	This action is used to add or remove items from the player's inventory, defined in the Inventory Manager.
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
	public class ActionInventorySet : Action
	{
		
		public InvAction invAction;

		public int parameterID = -1;
		public int invID;
		private int invNumber;
		
		public bool setAmount = false;
		public int amount = 1;

		public bool setPlayer = false;
		public int playerID;

		#if UNITY_EDITOR
		private InventoryManager inventoryManager;
		private SettingsManager settingsManager;
		#endif


		public ActionInventorySet ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Inventory;
			title = "Add or remove";
			description = "Adds or removes an item from the Player's inventory. Items are defined in the Inventory Manager. If the player can carry multiple amounts of the item, more options will show.";
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			invID = AssignInvItemID (parameters, parameterID, invID);
		}
		
		
		override public float Run ()
		{
			if (KickStarter.runtimeInventory)
			{
				if (!setAmount)
				{
					amount = 1;
				}

				if (KickStarter.settingsManager.playerSwitching == PlayerSwitching.Allow && !KickStarter.settingsManager.shareInventory && setPlayer)
				{
					if (invAction == InvAction.Add)
					{
						KickStarter.runtimeInventory.Add (invID, amount, false, playerID);
					}
					else
					{
						KickStarter.runtimeInventory.Remove (invID, amount, setAmount, playerID);
					}
				}
				else
				{
					if (invAction == InvAction.Add)
					{
						KickStarter.runtimeInventory.Add (invID, amount, false, -1);
					}
					else
					{
						KickStarter.runtimeInventory.Remove (invID, amount, setAmount, -1);
					}
				}

				PlayerMenus.ResetInventoryBoxes ();
			}
			
			return 0f;
		}

		
		#if UNITY_EDITOR

		override public void ShowGUI (List<ActionParameter> parameters)
		{
			if (inventoryManager == null && AdvGame.GetReferences ().inventoryManager)
			{
				inventoryManager = AdvGame.GetReferences ().inventoryManager;
			}
			if (settingsManager == null && AdvGame.GetReferences ().settingsManager)
			{
				settingsManager = AdvGame.GetReferences ().settingsManager;
			}
			
			if (inventoryManager != null)
			{
				// Create a string List of the field's names (for the PopUp box)
				List<string> labelList = new List<string>();
				
				int i = 0;
				if (parameterID == -1)
				{
					invNumber = -1;
				}
				
				if (inventoryManager.items.Count > 0)
				{
					foreach (InvItem _item in inventoryManager.items)
					{
						labelList.Add (_item.label);
						
						// If a item has been removed, make sure selected variable is still valid
						if (_item.id == invID)
						{
							invNumber = i;
						}
						
						i++;
					}
					
					if (invNumber == -1)
					{
						Debug.Log ("Previously chosen item no longer exists!");
						invNumber = 0;
						invID = 0;
					}
					
					invAction = (InvAction) EditorGUILayout.EnumPopup ("Add or remove:", invAction);

					parameterID = Action.ChooseParameterGUI ("Inventory item:", parameters, parameterID, ParameterType.InventoryItem);
					if (parameterID >= 0)
					{
						invNumber = Mathf.Min (invNumber, inventoryManager.items.Count-1);
						invID = -1;
					}
					else
					{
						invNumber = EditorGUILayout.Popup ("Inventory item:", invNumber, labelList.ToArray());
						invID = inventoryManager.items[invNumber].id;
					}

					if (inventoryManager.items[invNumber].canCarryMultiple)
					{
						setAmount = EditorGUILayout.Toggle ("Set amount?", setAmount);
					
						if (setAmount)
						{
							if (invAction == InvAction.Add)
							{
								amount = EditorGUILayout.IntField ("Increase count by:", amount);
							}
							else
							{
								amount = EditorGUILayout.IntField ("Reduce count by:", amount);
							}
						}
					}
				}
				else
				{
					EditorGUILayout.HelpBox ("No inventory items exist!", MessageType.Info);
					invID = -1;
					invNumber = -1;
				}

				if (settingsManager != null && settingsManager.playerSwitching == PlayerSwitching.Allow && !settingsManager.shareInventory)
				{
					EditorGUILayout.Space ();

					setPlayer = EditorGUILayout.Toggle ("Affect specific player?", setPlayer);
					if (setPlayer)
					{
						ChoosePlayerGUI ();
					}
				}
				else
				{
					setPlayer = false;
				}
			}
			else
			{
				EditorGUILayout.HelpBox ("An Inventory Manager must be assigned for this Action to work", MessageType.Warning);
			}

			AfterRunningOption ();
		}
		
		
		override public string SetLabel ()
		{
			string labelAdd = "";
			string labelItem = "";

			if (!inventoryManager)
			{
				inventoryManager = AdvGame.GetReferences ().inventoryManager;
			}

			if (inventoryManager)
			{
				labelItem = " " + inventoryManager.GetLabel (invID);
			}
			
			if (invAction == InvAction.Add)
			{
				labelAdd = " (Add" + labelItem + ")";
			}
			else
			{
				labelAdd = " (Remove" + labelItem + ")";
			}
		
			return labelAdd;
		}


		private void ChoosePlayerGUI ()
		{
			List<string> labelList = new List<string>();
			int i = 0;
			int playerNumber = -1;

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

				string label = "Add to player:";
				if (invAction == InvAction.Remove)
				{
					label = "Remove from player:";
				}

				playerNumber = EditorGUILayout.Popup (label, playerNumber, labelList.ToArray());
				playerID = settingsManager.players[playerNumber].ID;
			}
		}

		#endif

	}

}