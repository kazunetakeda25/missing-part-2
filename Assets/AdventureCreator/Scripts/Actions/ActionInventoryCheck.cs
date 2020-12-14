/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"ActionInventoryCheck.cs"
 * 
 *	This action checks to see if a particular inventory item
 *	is held by the player, and performs something accordingly.
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
	public class ActionInventoryCheck : ActionCheck
	{

		public int parameterID = -1;
		public int invID;
		private int invNumber;
		
		public bool doCount;
		public int intValue = 1;
		public enum IntCondition { EqualTo, NotEqualTo, LessThan, MoreThan };
		public IntCondition intCondition;

		public bool setPlayer = false;
		public int playerID;

		#if UNITY_EDITOR
		private InventoryManager inventoryManager;
		private SettingsManager settingsManager;
		#endif

		
		public ActionInventoryCheck ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Inventory;
			title = "Check";
			description = "Queries whether or not the player is carrying an item. If the player can carry multiple amounts of the item, more options will show.";
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			invID = AssignInvItemID (parameters, parameterID, invID);
		}

		
		override public bool CheckCondition ()
		{
			int count = 0;
			
			if (KickStarter.settingsManager.playerSwitching == PlayerSwitching.Allow && !KickStarter.settingsManager.shareInventory && setPlayer)
			{
				count = KickStarter.runtimeInventory.GetCount (invID, playerID);
			}
			else
			{
				count = KickStarter.runtimeInventory.GetCount (invID);
			}
			
			if (doCount)
			{
				if (intCondition == IntCondition.EqualTo)
				{
					if (count == intValue)
					{
						return true;
					}
				}
				
				else if (intCondition == IntCondition.NotEqualTo)
				{
					if (count != intValue)
					{
						return true;
					}
				}
				
				else if (intCondition == IntCondition.LessThan)
				{
					if (count < intValue)
					{
						return true;
					}
				}
				
				else if (intCondition == IntCondition.MoreThan)
				{
					if (count > intValue)
					{
						return true;
					}
				}
			}
			
			else if (count > 0)
			{
				return true;
			}
			
			return false;	
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
			
			if (inventoryManager)
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
						
						// If an item has been removed, make sure selected variable is still valid
						if (_item.id == invID)
						{
							invNumber = i;
						}
						
						i++;
					}
					
					if (invNumber == -1)
					{
						// Wasn't found (item was possibly deleted), so revert to zero
						Debug.LogWarning ("Previously chosen item no longer exists!");
						
						invNumber = 0;
						invID = 0;
					}

					//
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
					//
					
					if (inventoryManager.items[invNumber].canCarryMultiple)
					{
						doCount = EditorGUILayout.Toggle ("Query count?", doCount);
					
						if (doCount)
						{
							EditorGUILayout.BeginHorizontal ();
								EditorGUILayout.LabelField ("Count is:", GUILayout.MaxWidth (70));
								intCondition = (IntCondition) EditorGUILayout.EnumPopup (intCondition);
								intValue = EditorGUILayout.IntField (intValue);
							
								if (intValue < 1)
								{
									intValue = 1;
								}
							EditorGUILayout.EndHorizontal ();
						}
					}
					else
					{
						doCount = false;
					}

					if (settingsManager != null && settingsManager.playerSwitching == PlayerSwitching.Allow && !settingsManager.shareInventory)
					{
						EditorGUILayout.Space ();
						
						setPlayer = EditorGUILayout.Toggle ("Check specific player?", setPlayer);
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
					EditorGUILayout.LabelField ("No inventory items exist!");
					invID = -1;
					invNumber = -1;
				}
			}
			else
			{
				EditorGUILayout.HelpBox ("An Inventory Manager must be assigned for this Action to work", MessageType.Warning);
			}
		}

		
		override public string SetLabel ()
		{
			if (!inventoryManager)
			{
				inventoryManager = AdvGame.GetReferences ().inventoryManager;
			}

			if (inventoryManager)
			{
				return (" (" + inventoryManager.GetLabel (invID) + ")");
			}
			
			return "";
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

				playerNumber = EditorGUILayout.Popup ("Player to check:", playerNumber, labelList.ToArray());
				playerID = settingsManager.players[playerNumber].ID;
			}
		}

		#endif
		
	}

}