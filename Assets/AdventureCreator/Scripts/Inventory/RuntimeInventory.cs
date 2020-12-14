/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"RuntimeInventory.cs"
 * 
 *	This script creates a local copy of the InventoryManager's items.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{
	
	public class RuntimeInventory : MonoBehaviour
	{
		
		[HideInInspector] public List<InvItem> localItems = new List<InvItem>();
		[HideInInspector] public List<InvItem> craftingItems = new List<InvItem>();
		[HideInInspector] public ActionListAsset unhandledCombine;
		[HideInInspector] public ActionListAsset unhandledHotspot;
		[HideInInspector] public ActionListAsset unhandledGive;
		
		[HideInInspector] public InvItem selectedItem = null;
		[HideInInspector] public InvItem hoverItem = null;
		[HideInInspector] public List<int> matchingInvInteractions = new List<int>();
		private SelectItemMode selectItemMode = SelectItemMode.Use;
		private GUIStyle countStyle;
		private TextEffects countTextEffects;
		
		[HideInInspector] public InvItem highlightItem = null;
		private HighlightState highlightState = HighlightState.None;
		private float pulse = 0f;
		private int pulseDirection = 0; // 0 = none, 1 = in, -1 = out
		
		
		public void Start ()
		{
			selectedItem = null;
			hoverItem = null;
			
			craftingItems.Clear ();
			localItems.Clear ();
			GetItemsOnStart ();
			
			if (KickStarter.inventoryManager)
			{
				unhandledCombine = KickStarter.inventoryManager.unhandledCombine;
				unhandledHotspot = KickStarter.inventoryManager.unhandledHotspot;
				unhandledGive = KickStarter.inventoryManager.unhandledGive;
			}
			else
			{
				Debug.LogError ("An Inventory Manager is required - please use the Adventure Creator window to create one.");
			}
		}
		
		
		private void OnLevelWasLoaded ()
		{
			if (!KickStarter.settingsManager.IsInLoadingScene ())
			{
				SetNull ();
			}
		}
		
		
		public void SetNull ()
		{
			selectedItem = null;
			highlightItem = null;
			PlayerMenus.ResetInventoryBoxes ();
		}
		
		
		public void SelectItemByID (int _id, SelectItemMode _mode)
		{
			if (_id == -1)
			{
				SetNull ();
				return;
			}
			
			foreach (InvItem item in localItems)
			{
				if (item != null && item.id == _id)
				{
					SetSelectItemMode (_mode);
					selectedItem = item;
					PlayerMenus.ResetInventoryBoxes ();
					return;
				}
			}
			
			SetNull ();
			Debug.LogWarning ("Want to select inventory item " + KickStarter.inventoryManager.GetLabel (_id) + " but player is not carrying it.");
		}
		
		
		public void SelectItem (InvItem item, SelectItemMode _mode)
		{
			if (selectedItem == item)
			{
				selectedItem = null;
				KickStarter.playerCursor.ResetSelectedCursor ();
			}
			else
			{
				SetSelectItemMode (_mode);
				selectedItem = item;
			}
			PlayerMenus.ResetInventoryBoxes ();
		}
		
		
		private void SetSelectItemMode (SelectItemMode _mode)
		{
			if (KickStarter.settingsManager.CanGiveItems ())
			{
				selectItemMode = _mode;
			}
			else
			{
				selectItemMode = SelectItemMode.Use;
			}
		}
		
		
		public bool IsGivingItem ()
		{
			if (selectItemMode == SelectItemMode.Give)
			{
				return true;
			}
			return false;
		}
		
		
		private void GetItemsOnStart ()
		{
			if (KickStarter.inventoryManager)
			{
				foreach (InvItem item in KickStarter.inventoryManager.items)
				{
					if (item.carryOnStart)
					{
						int playerID = -1;
						if (KickStarter.settingsManager.playerSwitching == PlayerSwitching.Allow && !KickStarter.settingsManager.shareInventory && item.carryOnStartNotDefault && item.carryOnStartID != KickStarter.player.ID)
						{
							playerID = item.carryOnStartID;
						}
						
						if (!item.canCarryMultiple)
						{
							item.count = 1;
						}
						
						if (item.count < 1)
						{
							continue;
						}
						
						item.recipeSlot = -1;
						
						if (item.canCarryMultiple && item.useSeparateSlots)
						{
							for (int i=0; i<item.count; i++)
							{
								InvItem newItem = new InvItem (item);
								newItem.count = 1;
								
								if (playerID != -1)
								{
									Add (newItem.id, newItem.count, false, playerID);
								}
								else
								{
									localItems.Add (newItem);
								}
							}
						}
						else
						{
							if (playerID != -1)
							{
								Add (item.id, item.count, false, playerID);
							}
							else
							{
								localItems.Add (new InvItem (item));
							}
						}
					}
				}
			}
			else
			{
				Debug.LogError ("No Inventory Manager found - please use the Adventure Creator window to create one.");
			}
		}
		
		
		public void Add (int _id, int amount, bool selectAfter, int playerID)
		{
			if (playerID >= 0 && KickStarter.player.ID != playerID)
			{
				AddToOtherPlayer (_id, amount, playerID);
			}
			else
			{
				localItems = Add (_id, amount, localItems, selectAfter);
			}
		}
		
		
		public void Remove (int _id, int amount, bool setAmount, int playerID)
		{
			if (playerID >= 0 && KickStarter.player.ID != playerID)
			{
				RemoveFromOtherPlayer (_id, amount, setAmount, playerID);
			}
			else
			{
				localItems = Remove (_id, amount, setAmount, localItems);
			}
		}
		
		
		private void AddToOtherPlayer (int invID, int amount, int playerID)
		{
			SaveSystem saveSystem = GetComponent <SaveSystem>();
			
			List<InvItem> otherPlayerItems = saveSystem.GetItemsFromPlayer (playerID);
			otherPlayerItems = Add (invID, amount, otherPlayerItems, false);
			saveSystem.AssignItemsToPlayer (otherPlayerItems, playerID);
		}
		
		
		private void RemoveFromOtherPlayer (int invID, int amount, bool setAmount, int playerID)
		{
			SaveSystem saveSystem = GetComponent <SaveSystem>();
			
			List<InvItem> otherPlayerItems = saveSystem.GetItemsFromPlayer (playerID);
			otherPlayerItems = Remove (invID, amount, setAmount, otherPlayerItems);
			saveSystem.AssignItemsToPlayer (otherPlayerItems, playerID);
		}
		
		
		public List<InvItem> Add (int _id, int amount, List<InvItem> itemList, bool selectAfter)
		{
			itemList = ReorderItems (itemList);
			
			// Raise "count" by 1 for appropriate ID
			foreach (InvItem item in itemList)
			{
				if (item != null && item.id == _id)
				{
					if (item.canCarryMultiple)
					{
						if (item.useSeparateSlots)
						{
							break;
						}
						else
						{
							item.count += amount;
						}
					}
					
					if (selectAfter)
					{
						SelectItem (item, SelectItemMode.Use);
					}
					return itemList;
				}
			}
			
			// Not already carrying the item
			foreach (InvItem assetItem in KickStarter.inventoryManager.items)
			{
				if (assetItem.id == _id)
				{
					InvItem newItem = new InvItem (assetItem);
					if (!newItem.canCarryMultiple)
					{
						amount = 1;
					}
					newItem.recipeSlot = -1;
					newItem.count = amount;
					
					if (KickStarter.settingsManager.canReorderItems)
					{
						// Insert into first "blank" space
						for (int i=0; i<itemList.Count; i++)
						{
							if (itemList[i] == null)
							{
								itemList[i] = newItem;
								if (selectAfter)
								{
									SelectItem (newItem, SelectItemMode.Use);
								}
								
								if (newItem.canCarryMultiple && newItem.useSeparateSlots)
								{
									int count = newItem.count-1;
									newItem.count = 1;
									for (int j=0; j<count; j++)
									{
										itemList.Add (newItem);
									}
								}
								return itemList;
							}
						}
					}
					
					if (newItem.canCarryMultiple && newItem.useSeparateSlots)
					{
						int count = newItem.count;
						newItem.count = 1;
						for (int i=0; i<count; i++)
						{
							itemList.Add (newItem);
						}
					}
					else
					{
						itemList.Add (newItem);
					}
					
					if (selectAfter)
					{
						SelectItem (newItem, SelectItemMode.Use);
					}
					return itemList;
				}
			}
			
			itemList = RemoveEmptySlots (itemList);
			return itemList;
		}
		
		
		public void Remove (InvItem _item)
		{
			if (_item != null && localItems.Contains (_item))
			{
				if (_item == selectedItem)
				{
					SetNull ();
				}
				
				localItems [localItems.IndexOf (_item)] = null;
				
				localItems = ReorderItems (localItems);
				localItems = RemoveEmptySlots (localItems);
			}
		}
		
		
		private List<InvItem> Remove (int _id, int amount, bool setAmount, List<InvItem> itemList)
		{
			if (amount <= 0)
			{
				return itemList;
			}
			
			foreach (InvItem item in itemList)
			{
				if (item != null && item.id == _id)
				{
					if (item.canCarryMultiple && item.useSeparateSlots)
					{
						itemList [itemList.IndexOf (item)] = null;
						amount --;
						
						if (amount == 0)
						{
							break;
						}
						
						continue;
					}
					
					if (!item.canCarryMultiple || !setAmount)
					{
						itemList [itemList.IndexOf (item)] = null;
						amount = 0;
					}
					else
					{
						if (item.count > 0)
						{
							int numLeft = item.count - amount;
							item.count -= amount;
							amount = numLeft;
						}
						if (item.count < 1)
						{
							itemList [itemList.IndexOf (item)] = null;
						}
					}
					
					itemList = ReorderItems (itemList);
					itemList = RemoveEmptySlots (itemList);
					
					if (itemList.Count == 0)
					{
						return itemList;
					}
					
					if (amount <= 0)
					{
						return itemList;
					}
				}
			}
			
			itemList = ReorderItems (itemList);
			itemList = RemoveEmptySlots (itemList);
			
			return itemList;
		}


		public string GetHotspotPrefixLabel (InvItem item, string itemName, int languageNumber, bool canGive = false)
		{
			string prefix1 = "";
			string prefix2 = "";
			
			if (canGive && IsGivingItem ())
			{
				prefix1 = SpeechManager.GetTranslation (KickStarter.cursorManager.hotspotPrefix3.label, KickStarter.cursorManager.hotspotPrefix3.lineID, languageNumber);
				prefix2 = SpeechManager.GetTranslation (KickStarter.cursorManager.hotspotPrefix4.label, KickStarter.cursorManager.hotspotPrefix4.lineID, languageNumber);
			}
			else
			{
				if (item != null && item.overrideUseSyntax)
				{
					prefix1 = SpeechManager.GetTranslation (item.hotspotPrefix1.label, item.hotspotPrefix1.lineID, languageNumber);
					prefix2 = SpeechManager.GetTranslation (item.hotspotPrefix2.label, item.hotspotPrefix2.lineID, languageNumber);
				}
				else
				{
					prefix1 = SpeechManager.GetTranslation (KickStarter.cursorManager.hotspotPrefix1.label, KickStarter.cursorManager.hotspotPrefix1.lineID, languageNumber);
					prefix2 = SpeechManager.GetTranslation (KickStarter.cursorManager.hotspotPrefix2.label, KickStarter.cursorManager.hotspotPrefix2.lineID, languageNumber);
				}
			}

			if (prefix1 == "" && prefix2 != "")
			{
				return (prefix2 + " ");
			}

			if (prefix1 != "" && prefix2 == "")
			{
				return (prefix1 + " " + itemName + " ");
			}

			return (prefix1 + " " + itemName + " " + prefix2 + " ");
		}
		
		
		private List<InvItem> ReorderItems (List<InvItem> invItems)
		{
			if (!KickStarter.settingsManager.canReorderItems)
			{
				bool foundNone = false;
				
				while (!foundNone)
				{
					bool foundOne = false;
					
					for (int i=0; i<invItems.Count; i++)
					{
						if (invItems[i] == null)
						{
							invItems.RemoveAt (i);
							foundOne = true;
						}
					}
					
					if (!foundOne)
					{
						foundNone = true;
					}
				}
			}
			return invItems;
		}
		
		
		private void RemoveEmptyCraftingSlots ()
		{
			// Remove empty slots on end
			for (int i=craftingItems.Count-1; i>=0; i--)
			{
				if (localItems[i] == null)
				{
					localItems.RemoveAt (i);
				}
				else
				{
					return;
				}
			}
		}
		
		
		private List<InvItem> RemoveEmptySlots (List<InvItem> itemList)
		{
			// Remove empty slots on end
			for (int i=itemList.Count-1; i>=0; i--)
			{
				if (itemList[i] == null)
				{
					itemList.RemoveAt (i);
				}
				else
				{
					return itemList;
				}
			}
			return itemList;
		}
		
		
		public string GetLabel (InvItem item, int languageNumber)
		{
			if (languageNumber > 0)
			{
				return (SpeechManager.GetTranslation (item.label, item.lineID, languageNumber));
			}
			else if (item.altLabel != "")
			{
				return (item.altLabel);
			}
			
			return (item.label);
		}
		
		
		public int GetCount (int _invID)
		{
			foreach (InvItem item in localItems)
			{
				if (item != null && item.id == _invID)
				{
					return (item.count);
				}
			}
			
			return 0;
		}
		
		
		public int GetCount (int _invID, int _playerID)
		{
			List<InvItem> otherPlayerItems = GetComponent <SaveSystem>().GetItemsFromPlayer (_playerID);
			
			if (otherPlayerItems != null)
			{
				foreach (InvItem item in otherPlayerItems)
				{
					if (item != null && item.id == _invID)
					{
						return (item.count);
					}
				}
			}
			return 0;
		}
		
		
		public InvItem GetCraftingItem (int _id)
		{
			foreach (InvItem item in craftingItems)
			{
				if (item.id == _id)
				{
					return item;
				}
			}
			
			return null;
		}
		
		
		public InvItem GetItem (int _id)
		{
			foreach (InvItem item in localItems)
			{
				if (item != null && item.id == _id)
				{
					return item;
				}
			}
			
			return null;
		}
		
		
		public void Look (InvItem item)
		{
			if (item == null || item.recipeSlot > -1) return;
			
			if (item.lookActionList)
			{
				AdvGame.RunActionListAsset (item.lookActionList);
			}
		}
		
		
		public void Use (InvItem item)
		{
			if (item == null || item.recipeSlot > -1) return;
			
			if (item.useActionList)
			{
				selectedItem = null;
				AdvGame.RunActionListAsset (item.useActionList);
			}
			else if (KickStarter.settingsManager.CanSelectItems (true))
			{
				SelectItem (item, SelectItemMode.Use);
			}
		}
		
		
		public void RunInteraction (InvItem invItem, int iconID)
		{
			if (invItem == null || invItem.recipeSlot > -1) return;
			
			foreach (InvInteraction interaction in invItem.interactions)
			{
				if (interaction.icon.id == iconID)
				{
					if (interaction.actionList)
					{
						AdvGame.RunActionListAsset (interaction.actionList);
						return;
					}
					break;
				}
			}
			
			// Unhandled
			if (KickStarter.settingsManager.interactionMethod != AC_InteractionMethod.ContextSensitive && KickStarter.settingsManager.inventoryInteractions == InventoryInteractions.Multiple && KickStarter.settingsManager.CanSelectItems (false))
			{
				// Auto-select
				if (KickStarter.settingsManager.selectInvWithUnhandled && iconID == KickStarter.settingsManager.selectInvWithIconID)
				{
					SelectItem (invItem, SelectItemMode.Use);
					return;
				}
				if (KickStarter.settingsManager.giveInvWithUnhandled && iconID == KickStarter.settingsManager.giveInvWithIconID)
				{
					SelectItem (invItem, SelectItemMode.Give);
					return;
				}
			}
			
			AdvGame.RunActionListAsset (KickStarter.cursorManager.GetUnhandledInteraction (iconID));
		}
		
		
		public void RunInteraction (int iconID)
		{
			RunInteraction (hoverItem, iconID);
		}
		
		
		public void ShowInteractions (InvItem item)
		{
			hoverItem = item;
			KickStarter.playerMenus.SetInteractionMenus (true);
		}
		
		
		public void Combine (InvItem item1, int ID)
		{
			Combine (item1, GetItem (ID));
		}
		
		
		public void Combine (InvItem item1, InvItem item2)
		{
			if (item2 == null || item1 == null || item2.recipeSlot > -1)
			{
				return;
			}
			
			if (item2 == item1)
			{
				if ((KickStarter.settingsManager.interactionMethod != AC_InteractionMethod.ChooseHotspotThenInteraction || KickStarter.settingsManager.inventoryInteractions == InventoryInteractions.Single) && KickStarter.settingsManager.inventoryDragDrop && KickStarter.settingsManager.inventoryDropLook)
				{
					Look (item2);
				}
				selectedItem = null;
			}
			else
			{
				if (selectedItem == null)
				{
					InvItem tempItem = item1;
					item1 = item2;
					item2 = tempItem;
				}
				selectedItem = null;
				
				for (int i=0; i<item2.combineID.Count; i++)
				{
					if (item2.combineID[i] == item1.id && item2.combineActionList[i] != null)
					{
						PlayerMenus.ForceOffAllMenus (true);
						AdvGame.RunActionListAsset (item2.combineActionList [i]);
						return;
					}
				}
				
				if (KickStarter.settingsManager.reverseInventoryCombinations || (KickStarter.settingsManager.SelectInteractionMethod () == SelectInteractions.CyclingCursorAndClickingHotspot && KickStarter.settingsManager.inventoryInteractions == InventoryInteractions.Multiple))
				{
					// Try opposite: search selected item instead
					for (int i=0; i<item1.combineID.Count; i++)
					{
						if (item1.combineID[i] == item2.id && item1.combineActionList[i] != null)
						{
							ActionListAsset assetFile = item1.combineActionList[i];
							PlayerMenus.ForceOffAllMenus (true);
							AdvGame.RunActionListAsset (assetFile);
							return;
						}
					}
				}
				
				// Found no combine match
				if (item1.unhandledCombineActionList)
				{
					ActionListAsset unhandledActionList = item1.unhandledCombineActionList;
					AdvGame.RunActionListAsset (unhandledActionList);	
				}
				else if (unhandledCombine)
				{
					PlayerMenus.ForceOffAllMenus (true);
					AdvGame.RunActionListAsset (unhandledCombine);
				}
			}
			
			KickStarter.playerCursor.ResetSelectedCursor ();
		}
		
		
		public List<InvItem> GetSelected ()
		{
			List<InvItem> items = new List<InvItem>();
			
			if (selectedItem != null)
			{
				items.Add (selectedItem);
			}
			
			return items;
		}
		
		
		public bool IsItemCarried (InvItem _item)
		{
			if (_item == null) return false;
			foreach (InvItem item in localItems)
			{
				if (item == _item)
				{
					return true;
				}
			}
			
			return false;
		}
		
		
		public void RemoveRecipes ()
		{
			while (craftingItems.Count > 0)
			{
				for (int i=0; i<craftingItems.Count; i++)
				{
					Add (craftingItems[i].id, craftingItems[i].count, false, -1);
					craftingItems.RemoveAt (i);
				}
			}
			PlayerMenus.ResetInventoryBoxes ();
		}
		
		
		public void TransferCraftingToLocal (int _recipeSlot, bool selectAfter)
		{
			foreach (InvItem item in craftingItems)
			{
				if (item.recipeSlot == _recipeSlot)
				{
					Add (item.id, item.count, selectAfter, -1);
					SelectItemByID (item.id, SelectItemMode.Use);
					craftingItems.Remove (item);
					return;
				}
			}
		}
		
		
		public void TransferLocalToContainer (Container _container, InvItem _item, int _index)
		{
			if (_item != null && localItems.Contains (_item))
			{
				localItems [localItems.IndexOf (_item)] = null;
				localItems = ReorderItems (localItems);
				localItems = RemoveEmptySlots (localItems);
				
				SetNull ();
			}
		}
		
		
		public void TransferLocalToCrafting (InvItem _item, int _slot)
		{
			if (_item != null && localItems.Contains (_item))
			{
				_item.recipeSlot = _slot;
				craftingItems.Add (_item);
				
				localItems [localItems.IndexOf (_item)] = null;
				localItems = ReorderItems (localItems);
				localItems = RemoveEmptySlots (localItems);
				
				SetNull ();
			}
		}
		
		
		public List<InvItem> MatchInteractions ()
		{
			List<InvItem> items = new List<InvItem>();
			matchingInvInteractions = new List<int>();
			
			if (!KickStarter.settingsManager.cycleInventoryCursors)
			{
				return items;
			}
			
			if (hoverItem != null)
			{
				items = MatchInteractionsFromItem (items, hoverItem);
			}
			else if (KickStarter.playerInteraction.GetActiveHotspot ())
			{
				List<Button> invButtons = KickStarter.playerInteraction.GetActiveHotspot ().invButtons;
				foreach (Button button in invButtons)
				{
					foreach (InvItem item in localItems)
					{
						if (item != null && item.id == button.invID && !button.isDisabled)
						{
							matchingInvInteractions.Add (invButtons.IndexOf (button));
							items.Add (item);
							break;
						}
					}
				}
			}
			return items;
		}
		
		
		private List<InvItem> MatchInteractionsFromItem (List<InvItem> items, InvItem _item)
		{
			if (_item != null && _item.combineID != null)
			{
				foreach (int combineID in _item.combineID)
				{
					foreach (InvItem item in localItems)
					{
						if (item != null && item.id == combineID)
						{
							matchingInvInteractions.Add (_item.combineID.IndexOf (combineID));
							items.Add (item);
							break;
						}
					}
				}
			}
			return items;
		}
		
		
		public Recipe CalculateRecipe (bool autoCreateMatch)
		{
			if (KickStarter.inventoryManager == null)
			{
				return null;
			}
			
			foreach (Recipe recipe in KickStarter.inventoryManager.recipes)
			{
				if (autoCreateMatch)
				{
					if (!recipe.autoCreate)
					{
						break;
					}
				}

				if (IsRecipeInvalid (recipe))
				{
					continue;
				}
				
				bool canCreateRecipe = true;
				while (canCreateRecipe)
				{
					foreach (Ingredient ingredient in recipe.ingredients)
					{
						// Is ingredient present (and optionally, in correct slot)
						InvItem ingredientItem = GetCraftingItem (ingredient.itemID);
						if (ingredientItem == null)
						{
							canCreateRecipe = false;
							break;
						}
						
						if ((recipe.useSpecificSlots && ingredientItem.recipeSlot == (ingredient.slotNumber -1)) || !recipe.useSpecificSlots)
						{
							if ((ingredientItem.canCarryMultiple && ingredientItem.count >= ingredient.amount) || !ingredientItem.canCarryMultiple)
							{
								if (canCreateRecipe && recipe.ingredients.IndexOf (ingredient) == (recipe.ingredients.Count -1))
								{
									return recipe;
								}
							}
							else canCreateRecipe = false;
						}
						else canCreateRecipe = false;
					}
				}
			}
			
			return null;
		}


		private bool IsRecipeInvalid (Recipe recipe)
		{
			// Are any invalid ingredients present?
			foreach (InvItem item in craftingItems)
			{
				bool found = false;
				foreach (Ingredient ingredient in recipe.ingredients)
				{
					if (ingredient.itemID == item.id)
					{
						found = true;
					}
				}
				if (!found)
				{
					// Not present in recipe
					return true;
				}
			}
			return false;
		}
		
		
		public void PerformCrafting (Recipe recipe, bool selectAfter)
		{
			foreach (Ingredient ingredient in recipe.ingredients)
			{
				for (int i=0; i<craftingItems.Count; i++)
				{
					if (craftingItems [i].id == ingredient.itemID)
					{
						if (craftingItems [i].canCarryMultiple && ingredient.amount > 0)
						{
							craftingItems [i].count -= ingredient.amount;
							if (craftingItems [i].count < 1)
							{
								craftingItems.RemoveAt (i);
							}
						}
						else
						{
							craftingItems.RemoveAt (i);
						}
					}
				}
			}
			
			RemoveEmptyCraftingSlots ();
			Add (recipe.resultID, 1, selectAfter, -1);
		}
		
		
		public void MoveItemToIndex (InvItem item, List<InvItem> items, int index)
		{
			if (item != null)
			{
				if (KickStarter.settingsManager.canReorderItems)
				{
					// Check nothing in place already
					int oldIndex = items.IndexOf (item);
					while (items.Count <= Mathf.Max (index, oldIndex))
					{
						items.Add (null);
					}
					
					if (items [index] == null)
					{
						items [index] = item;
						items [oldIndex] = null;
						
					}
					
					SetNull ();
					items = RemoveEmptySlots (items);
				}
				else if (items.IndexOf (item) == index)
				{
					SetNull ();
				}
			}
		}
		
		
		public void SetFont (Font font, int size, Color color, TextEffects textEffects)
		{
			countStyle = new GUIStyle();
			countStyle.font = font;
			countStyle.fontSize = size;
			countStyle.normal.textColor = color;
			countStyle.alignment = TextAnchor.MiddleCenter;
			countTextEffects = textEffects;
		}
		
		
		public void DrawHighlighted (Rect _rect)
		{
			if (highlightItem == null || highlightItem.activeTex == null) return;
			
			if (highlightState == HighlightState.None)
			{
				GUI.DrawTexture (_rect, highlightItem.activeTex, ScaleMode.StretchToFill, true, 0f);
				return;
			}
			
			if (pulseDirection == 0)
			{
				pulse = 0f;
				pulseDirection = 1;
			}
			else if (pulseDirection == 1)
			{
				pulse += KickStarter.settingsManager.inventoryPulseSpeed * Time.deltaTime;
			}
			else if (pulseDirection == -1)
			{
				pulse -= KickStarter.settingsManager.inventoryPulseSpeed * Time.deltaTime;
			}
			
			if (pulse > 1f)
			{
				pulse = 1f;
				
				if (highlightState == HighlightState.Normal)
				{
					highlightState = HighlightState.None;
					GUI.DrawTexture (_rect, highlightItem.activeTex, ScaleMode.StretchToFill, true, 0f);
					return;
				}
				else
				{
					pulseDirection = -1;
				}
			}
			else if (pulse < 0f)
			{
				pulse = 0f;
				
				if (highlightState == HighlightState.Pulse)
				{
					pulseDirection = 1;
				}
				else
				{
					highlightState = HighlightState.None;
					GUI.DrawTexture (_rect, highlightItem.tex, ScaleMode.StretchToFill, true, 0f);
					highlightItem = null;
					return;
				}
			}
			
			
			Color backupColor = GUI.color;
			Color tempColor = GUI.color;
			
			tempColor.a = pulse;
			GUI.color = tempColor;
			GUI.DrawTexture (_rect, highlightItem.activeTex, ScaleMode.StretchToFill, true, 0f);
			GUI.color = backupColor;
			GUI.DrawTexture (_rect, highlightItem.tex, ScaleMode.StretchToFill, true, 0f);
		}
		
		
		public void HighlightItemOnInstant (int _id)
		{
			highlightItem = GetItem (_id);
			highlightState = HighlightState.None;
			pulse = 1f;
		}
		
		
		public void HighlightItemOffInstant ()
		{
			highlightItem = null;
			highlightState = HighlightState.None;
			pulse = 0f;
		}
		
		
		public void HighlightItem (int _id, HighlightType _type)
		{
			highlightItem = GetItem (_id);
			if (highlightItem == null) return;
			
			if (_type == HighlightType.Enable)
			{
				highlightState = HighlightState.Normal;
				pulseDirection = 1;
			}
			else if (_type == HighlightType.Disable)
			{
				highlightState = HighlightState.Normal;
				pulseDirection = -1;
			}
			else if (_type == HighlightType.PulseOnce)
			{
				highlightState = HighlightState.Flash;
				pulse = 0f;
				pulseDirection = 1;
			}
			else if (_type ==  HighlightType.PulseContinually)
			{
				highlightState = HighlightState.Pulse;
				pulse = 0f;
				pulseDirection = 1;
			}
		}
		
		
		public void StopHighlighting ()
		{
			highlightItem = null;
			highlightState = HighlightState.None;
		}
		
		
		public void DrawInventoryCount (Vector2 cursorPosition, float cursorSize, int count)
		{
			if (count > 1)
			{
				if (countTextEffects != TextEffects.None)
				{
					AdvGame.DrawTextEffect (AdvGame.GUIBox (cursorPosition, cursorSize), count.ToString (), countStyle, Color.black, countStyle.normal.textColor, 2, countTextEffects);
				}
				else
				{
					GUI.Label (AdvGame.GUIBox (cursorPosition, cursorSize), count.ToString (), countStyle);
				}
			}
		}
		
		
		public void ProcessInventoryBoxClick (AC.Menu _menu, MenuInventoryBox inventoryBox, int _slot, MouseState _mouseState)
		{
			if (inventoryBox.inventoryBoxType == AC_InventoryBoxType.Default || inventoryBox.inventoryBoxType == AC_InventoryBoxType.DisplayLastSelected)
			{
				if (KickStarter.settingsManager.inventoryInteractions == InventoryInteractions.Multiple && KickStarter.playerInput.interactionMenuIsOn)
				{
					KickStarter.playerMenus.SetInteractionMenus (false);
					KickStarter.playerInteraction.ClickInvItemToInteract ();
				}
				else if (KickStarter.settingsManager.inventoryInteractions == InventoryInteractions.Multiple && KickStarter.settingsManager.SelectInteractionMethod () == AC.SelectInteractions.CyclingCursorAndClickingHotspot)
				{
					if (KickStarter.settingsManager.autoCycleWhenInteract && _mouseState == MouseState.SingleClick && (selectedItem == null || KickStarter.settingsManager.cycleInventoryCursors))
					{
						int originalIndex = KickStarter.playerInteraction.GetInteractionIndex ();
						KickStarter.playerInteraction.SetNextInteraction ();
						KickStarter.playerInteraction.SetInteractionIndex (originalIndex);
					}

					if (!KickStarter.settingsManager.cycleInventoryCursors && selectedItem != null)
					{
						inventoryBox.HandleDefaultClick (_mouseState, _slot, KickStarter.settingsManager.interactionMethod);
					}
					else if (_mouseState != MouseState.RightClick)
					{
						KickStarter.playerMenus.SetInteractionMenus (false);
						KickStarter.playerInteraction.ClickInvItemToInteract ();
					}
					
					if (KickStarter.settingsManager.autoCycleWhenInteract && _mouseState == MouseState.SingleClick)
					{
						KickStarter.playerInteraction.RestoreInventoryInteraction ();
					}
					
				}
				else if (KickStarter.settingsManager.interactionMethod != AC_InteractionMethod.ContextSensitive && KickStarter.settingsManager.inventoryInteractions == InventoryInteractions.Single)
				{
					inventoryBox.HandleDefaultClick (_mouseState, _slot, AC_InteractionMethod.ContextSensitive);
				}
				else
				{
					inventoryBox.HandleDefaultClick (_mouseState, _slot, KickStarter.settingsManager.interactionMethod);
				}
				
				_menu.Recalculate ();
			}
			else if (inventoryBox.inventoryBoxType == AC_InventoryBoxType.Container)
			{
				inventoryBox.ClickContainer (_mouseState, _slot, KickStarter.playerInput.activeContainer);
				_menu.Recalculate ();
			}
			else if (inventoryBox.inventoryBoxType == AC_InventoryBoxType.HostpotBased)
			{
				if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction)
				{
					if (hoverItem != null)
					{
						Combine (hoverItem, inventoryBox.items [_slot]);
					}
					else if (KickStarter.playerInteraction.GetActiveHotspot ())
					{
						InvItem _item = inventoryBox.items [_slot];
						if (_item != null)
						{
							SelectItem (_item, SelectItemMode.Use);
							_menu.TurnOff (false);
							KickStarter.playerInteraction.ClickButton (InteractionType.Inventory, -2, _item.id);
							KickStarter.playerCursor.ResetSelectedCursor ();
						}
					}
					else
					{
						Debug.LogWarning ("Cannot handle inventory click since there is no active Hotspot.");
					}
				}
				else
				{
					Debug.LogWarning ("This type of InventoryBox only works with the Choose Hotspot Then Interaction method of interaction.");
				}
			}
		}
		
	}
	
}
