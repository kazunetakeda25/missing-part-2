/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"MenuInventoryBox.cs"
 * 
 *	This MenuElement lists all inventory items held by the player.
 * 
 */

using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	public class MenuInventoryBox : MenuElement
	{

		public UISlot[] uiSlots;

		public TextEffects textEffects;
		public AC_InventoryBoxType inventoryBoxType;
		public int maxSlots;
		public bool limitToCategory;
		public int categoryID;
		public List<InvItem> items = new List<InvItem>();
		public bool selectItemsAfterTaking = true;
		public ConversationDisplayType displayType = ConversationDisplayType.IconOnly;

		private string[] labels = null;


		public override void Declare ()
		{
			uiSlots = null;

			isVisible = true;
			isClickable = true;
			inventoryBoxType = AC_InventoryBoxType.Default;
			numSlots = 0;
			SetSize (new Vector2 (6f, 10f));
			maxSlots = 10;
			limitToCategory = false;
			selectItemsAfterTaking = true;
			categoryID = -1;
			displayType = ConversationDisplayType.IconOnly;
			textEffects = TextEffects.None;
			items = new List<InvItem>();
		}


		public override MenuElement DuplicateSelf ()
		{
			MenuInventoryBox newElement = CreateInstance <MenuInventoryBox>();
			newElement.Declare ();
			newElement.CopyInventoryBox (this);
			return newElement;
		}
		
		
		public void CopyInventoryBox (MenuInventoryBox _element)
		{
			uiSlots = _element.uiSlots;
			isClickable = _element.isClickable;
			textEffects = _element.textEffects;
			inventoryBoxType = _element.inventoryBoxType;
			numSlots = _element.numSlots;
			maxSlots = _element.maxSlots;
			limitToCategory = _element.limitToCategory;
			categoryID = _element.categoryID;
			selectItemsAfterTaking = _element.selectItemsAfterTaking;
			displayType = _element.displayType;

			PopulateList ();

			base.Copy (_element);
		}


		public override void LoadUnityUI (AC.Menu _menu)
		{
			int i=0;
			foreach (UISlot uiSlot in uiSlots)
			{
				uiSlot.LinkUIElements ();
				if (uiSlot != null && uiSlot.uiButton != null)
				{
					int j=i;
					uiSlot.uiButton.onClick.AddListener (() => {
						ProcessClick (_menu, j, MouseState.SingleClick);
					});
					uiSlot.AddClickHandler (_menu, this, j);
				}
				i++;
			}
		}


		public override GameObject GetObjectToSelect ()
		{
			if (uiSlots != null && uiSlots.Length > 0 && uiSlots[0].uiButton != null)
			{
				return uiSlots[0].uiButton.gameObject;
			}
			return null;
		}
		
		
		public override RectTransform GetRectTransform (int _slot)
		{
			if (uiSlots != null && uiSlots.Length > _slot)
			{
				return uiSlots[_slot].GetRectTransform ();
			}
			return null;
		}
		
		
		#if UNITY_EDITOR
		
		public override void ShowGUI (MenuSource source)
		{
			EditorGUILayout.BeginVertical ("Button");
				if (source == MenuSource.AdventureCreator)
				{
					textEffects = (TextEffects) EditorGUILayout.EnumPopup ("Text effect:", textEffects);
				}
				displayType = (ConversationDisplayType) EditorGUILayout.EnumPopup ("Display:", displayType);
				inventoryBoxType = (AC_InventoryBoxType) EditorGUILayout.EnumPopup ("Inventory box type:", inventoryBoxType);
				if (inventoryBoxType == AC_InventoryBoxType.Default || inventoryBoxType == AC_InventoryBoxType.CustomScript)
				{
					limitToCategory = EditorGUILayout.Toggle ("Limit to category?", limitToCategory);
					if (limitToCategory)
					{
						if (AdvGame.GetReferences ().inventoryManager)
						{
							List<string> binList = new List<string>();
							List<InvBin> bins = AdvGame.GetReferences ().inventoryManager.bins;
							foreach (InvBin bin in bins)
							{
								binList.Add (bin.label);
							}

							EditorGUILayout.BeginHorizontal ();
								EditorGUILayout.LabelField ("Category:", GUILayout.Width (146f));
								if (binList.Count > 0)
								{
									int binNumber = GetBinSlot (categoryID, bins);
									binNumber = EditorGUILayout.Popup (binNumber, binList.ToArray());
									categoryID = bins[binNumber].id;
								}
								else
								{
									categoryID = -1;
									EditorGUILayout.LabelField ("No categories defined!", EditorStyles.miniLabel, GUILayout.Width (146f));
								}
							EditorGUILayout.EndHorizontal ();
						}
						else
						{
							EditorGUILayout.HelpBox ("No Inventory Manager defined!", MessageType.Warning);
							categoryID = -1;
						}
					}
					else
					{
						categoryID = -1;
					}

					maxSlots = EditorGUILayout.IntSlider ("Max number slots:", maxSlots, 1, 30);
					
					isClickable = true;
				}
				else if (inventoryBoxType == AC_InventoryBoxType.DisplaySelected)
				{
					isClickable = false;
					maxSlots = 1;
				}
				else if (inventoryBoxType == AC_InventoryBoxType.DisplayLastSelected)
				{
					isClickable = true;
					maxSlots = 1;
				}
				else if (inventoryBoxType == AC_InventoryBoxType.Container)
				{
					isClickable = true;
					maxSlots = EditorGUILayout.IntSlider ("Max number of slots:", maxSlots, 1, 30);
					selectItemsAfterTaking = EditorGUILayout.Toggle ("Select item after taking?", selectItemsAfterTaking);
				}
				else
				{
					isClickable = true;
					if (source == MenuSource.AdventureCreator)
					{
						numSlots = EditorGUILayout.IntField ("Test slots:", numSlots);
					}
					maxSlots = EditorGUILayout.IntSlider ("Max number slots:", maxSlots, 1, 30);
				}

				if (inventoryBoxType != AC_InventoryBoxType.DisplaySelected && inventoryBoxType != AC_InventoryBoxType.DisplayLastSelected && source == MenuSource.AdventureCreator)
				{
					slotSpacing = EditorGUILayout.Slider ("Slot spacing:", slotSpacing, 0f, 20f);
					orientation = (ElementOrientation) EditorGUILayout.EnumPopup ("Slot orientation:", orientation);
					if (orientation == ElementOrientation.Grid)
					{
						gridWidth = EditorGUILayout.IntSlider ("Grid size:", gridWidth, 1, 10);
					}
				}
				
				if (inventoryBoxType == AC_InventoryBoxType.CustomScript)
				{
					ShowClipHelp ();
				}

				if (source != MenuSource.AdventureCreator)
				{
					EditorGUILayout.EndVertical ();
					EditorGUILayout.BeginVertical ("Button");
					
					uiSlots = ResizeUISlots (uiSlots, maxSlots);
					
					for (int i=0; i<uiSlots.Length; i++)
					{
						uiSlots[i].LinkedUiGUI (i, source);
					}
				}
			EditorGUILayout.EndVertical ();


			base.ShowGUI (source);
		}


		private int GetBinSlot (int _id, List<InvBin> bins)
		{
			int i = 0;
			foreach (InvBin bin in bins)
			{
				if (bin.id == _id)
				{
					return i;
				}
				i++;
			}
			
			return 0;
		}
		
		#endif


		public override void HideAllUISlots ()
		{
			LimitUISlotVisibility (uiSlots, 0);
		}
		
		
		public override void PreDisplay (int _slot, int languageNumber, bool isActive)
		{
			if (items.Count > 0 && items.Count > (_slot+offset) && items [_slot+offset] != null)
			{
				string fullText = "";

				if (displayType == ConversationDisplayType.TextOnly)
				{
					fullText = items [_slot+offset].label;
					if (KickStarter.runtimeInventory != null)
					{
						fullText = KickStarter.runtimeInventory.GetLabel (items [_slot+offset], languageNumber);
					}
					string countText = GetCount (_slot);
					if (countText != "")
					{
						fullText += " (" + countText + ")";
					}
				}
				else
				{
					string countText = GetCount (_slot);
					if (countText != "")
					{
						fullText = countText;
					}
				}

				if (labels == null || labels.Length != numSlots)
				{
					labels = new string [numSlots];
				}
				labels [_slot] = fullText;
			}

			if (Application.isPlaying)
			{
				if (uiSlots != null && uiSlots.Length > _slot)
				{
					LimitUISlotVisibility (uiSlots, numSlots);

					uiSlots[_slot].SetText (labels [_slot]);
					if (displayType == ConversationDisplayType.IconOnly)
					{
						Texture2D tex = null;
						if (items.Count > (_slot+offset) && items [_slot+offset] != null)
						{
							if (isActive)
							{
								tex = items [_slot+offset].activeTex;
							}
							if (tex == null)
							{
								tex = items [_slot+offset].tex;
							}
						}
						uiSlots[_slot].SetImage (tex);
					}
				}
			}
		}
		
		
		public override void Display (GUIStyle _style, int _slot, float zoom, bool isActive)
		{
			base.Display (_style, _slot, zoom, isActive);
			_style.wordWrap = true;
			
			if (items.Count > 0 && items.Count > (_slot+offset) && items [_slot+offset] != null)
			{
				if (displayType == ConversationDisplayType.IconOnly)
				{
					GUI.Label (GetSlotRectRelative (_slot), "", _style);
					DrawTexture (ZoomRect (GetSlotRectRelative (_slot), zoom), items [_slot+offset], isActive);
					_style.normal.background = null;
					
					if (textEffects != TextEffects.None)
					{
						AdvGame.DrawTextEffect (ZoomRect (GetSlotRectRelative (_slot), zoom), GetCount (_slot), _style, Color.black, _style.normal.textColor, 2, textEffects);
					}
					else
					{
						GUI.Label (ZoomRect (GetSlotRectRelative (_slot), zoom), GetCount (_slot), _style);
					}
				}
				else if (displayType == ConversationDisplayType.TextOnly)
				{
					if (textEffects != TextEffects.None)
					{
						AdvGame.DrawTextEffect (ZoomRect (GetSlotRectRelative (_slot), zoom), labels[_slot], _style, Color.black, _style.normal.textColor, 2, textEffects);
					}
					else
					{
						GUI.Label (ZoomRect (GetSlotRectRelative (_slot), zoom), labels[_slot], _style);
					}
				}
			}
		}
		
		
		public void HandleDefaultClick (MouseState _mouseState, int _slot, AC_InteractionMethod interactionMethod)
		{
			if (KickStarter.runtimeInventory != null)
			{
				KickStarter.runtimeInventory.StopHighlighting ();
				KickStarter.runtimeInventory.SetFont (font, GetFontSize (), fontColor, textEffects);

				if (items.Count <= (_slot + offset) || items[_slot+offset] == null)
				{
					// Blank space
					KickStarter.runtimeInventory.MoveItemToIndex (KickStarter.runtimeInventory.selectedItem, KickStarter.runtimeInventory.localItems, _slot + offset);
					return;
				}

				if (interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction)
				{
					if (KickStarter.runtimeInventory.selectedItem != null)
					{
						if (_mouseState == MouseState.SingleClick)
						{
							KickStarter.runtimeInventory.Combine (KickStarter.runtimeInventory.selectedItem, items [_slot + offset]);
						}
						else if (_mouseState == MouseState.RightClick)
						{
							KickStarter.runtimeInventory.SetNull ();
						}
					}
					else
					{
						KickStarter.runtimeInventory.ShowInteractions (items [_slot + offset]);
					}
				}
				else if (interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot)
				{
					if (_mouseState == MouseState.SingleClick)
					{
						int cursorID = KickStarter.playerCursor.GetSelectedCursorID ();
						int cursor = KickStarter.playerCursor.GetSelectedCursor ();

						if (cursor == -2 && KickStarter.runtimeInventory.selectedItem != null)
						{
							if (items [_slot + offset] == KickStarter.runtimeInventory.selectedItem)
							{
								KickStarter.runtimeInventory.SelectItem (items [_slot + offset], SelectItemMode.Use);
							}
							else
							{
								KickStarter.runtimeInventory.Combine (KickStarter.runtimeInventory.selectedItem, items [_slot + offset]);
							}
						}
						else if (cursor == -1 && !KickStarter.settingsManager.selectInvWithUnhandled)
						{
							KickStarter.runtimeInventory.SelectItem (items [_slot + offset], SelectItemMode.Use);
						}
						else if (cursorID > -1)
						{
							KickStarter.runtimeInventory.RunInteraction (items [_slot + offset], cursorID);
						}
					}
				}
				else if (interactionMethod == AC_InteractionMethod.ContextSensitive)
				{
					if (_mouseState == MouseState.SingleClick)
					{
						if (KickStarter.runtimeInventory.selectedItem == null)
						{
							KickStarter.runtimeInventory.Use (items [_slot + offset]);
						}
						else
						{
							KickStarter.runtimeInventory.Combine (KickStarter.runtimeInventory.selectedItem, items [_slot + offset]);
						}
					}
					else if (_mouseState == MouseState.RightClick)
					{
						if (KickStarter.runtimeInventory.selectedItem == null)
						{
							KickStarter.runtimeInventory.Look (items [_slot + offset]);
						}
						else
						{
							KickStarter.runtimeInventory.SetNull ();
						}
					}
				}
			}
		}
		
		
		public override void RecalculateSize (MenuSource source)
		{
			PopulateList ();

			if (inventoryBoxType == AC_InventoryBoxType.HostpotBased)
			{
				if (!Application.isPlaying)
				{
					if (numSlots < 0)
					{
						numSlots = 0;
					}
					if (numSlots > maxSlots)
					{
						numSlots = maxSlots;
					}
				}
				else
				{
					numSlots = items.Count;
				}
			}
			else
			{
				numSlots = maxSlots;
				LimitOffset (items.Count);
			}

			labels = new string [numSlots];

			if (Application.isPlaying && uiSlots != null)
			{
				ClearSpriteCache (uiSlots);
			}

			if (!isVisible)
			{
				LimitUISlotVisibility (uiSlots, 0);
			}

			base.RecalculateSize (source);
		}
		
		
		private void PopulateList ()
		{
			if (Application.isPlaying)
			{
				if (inventoryBoxType == AC_InventoryBoxType.HostpotBased)
				{
					items = KickStarter.runtimeInventory.MatchInteractions ();
				}
				else if (inventoryBoxType == AC_InventoryBoxType.DisplaySelected)
				{
					items = KickStarter.runtimeInventory.GetSelected ();
				}
				else if (inventoryBoxType == AC_InventoryBoxType.DisplayLastSelected)
				{
					if (KickStarter.runtimeInventory.selectedItem != null)
					{
						items = new List<InvItem>();
						items = KickStarter.runtimeInventory.GetSelected ();
					}
					else if (items.Count == 1 && !KickStarter.runtimeInventory.IsItemCarried (items[0]))
					{
						items.Clear ();
					}
				}
				else if (inventoryBoxType == AC_InventoryBoxType.Container)
				{
					if (KickStarter.playerInput.activeContainer)
					{
						items.Clear ();
						foreach (ContainerItem containerItem in KickStarter.playerInput.activeContainer.items)
						{
							InvItem referencedItem = new InvItem (KickStarter.inventoryManager.GetItem (containerItem.linkedID));
							referencedItem.count = containerItem.count;
							items.Add (referencedItem);
						}
					}
				}
				else
				{
					items = new List<InvItem>();
					foreach (InvItem _item in KickStarter.runtimeInventory.localItems)
					{
						if (KickStarter.settingsManager.hideSelectedFromMenu && KickStarter.runtimeInventory.selectedItem == _item)
						{
							items.Add (null);
						}
						else
						{
							items.Add (_item);
						}
					}
				}
			}
			else
			{
				items = new List<InvItem>();
				if (AdvGame.GetReferences ().inventoryManager)
				{
					foreach (InvItem _item in AdvGame.GetReferences ().inventoryManager.items)
					{
						items.Add (_item);
						if (_item != null)
						{
							_item.recipeSlot = -1;
						}
					}
				}
			}

			if (inventoryBoxType == AC_InventoryBoxType.Default || inventoryBoxType == AC_InventoryBoxType.CustomScript)
			{
				if (limitToCategory && categoryID > -1)
				{
					while (AreAnyItemsInWrongCategory ())
					{
						foreach (InvItem _item in items)
						{
							if (_item != null && _item.binID != categoryID)
							{
								items.Remove (_item);
								break;
							}
						}
					}
				}

				while (AreAnyItemsInRecipe ())
				{
					foreach (InvItem _item in items)
					{
						if (_item != null && _item.recipeSlot > -1)
						{
							if (AdvGame.GetReferences ().settingsManager.canReorderItems)
								items [items.IndexOf (_item)] = null;
							else
								items.Remove (_item);
							break;
						}
					}
				}
			}
		}


		public override bool CanBeShifted (AC_ShiftInventory shiftType)
		{
			if (shiftType == AC_ShiftInventory.ShiftLeft)
			{
				if (offset == 0)
				{
					return false;
				}
			}
			else
			{
				if ((maxSlots + offset) >= items.Count)
				{
					return false;
				}
			}
			return true;
		}


		private bool AreAnyItemsInRecipe ()
		{
			foreach (InvItem item in items)
			{
				if (item != null && item.recipeSlot >= 0)
				{
					return true;
				}
			}
			return false;
		}


		private bool AreAnyItemsInWrongCategory ()
		{
			foreach (InvItem item in items)
			{
				if (item != null && item.binID != categoryID)
				{
					return true;
				}
			}
			return false;
		}
		
		
		public override void Shift (AC_ShiftInventory shiftType, int amount)
		{
			if (numSlots >= maxSlots)
			{
				Shift (shiftType, maxSlots, items.Count, amount);
			}
		}


		private void DrawTexture (Rect rect, InvItem _item, bool isActive)
		{
			if (_item == null) return;

			Texture2D tex = null;
			if (Application.isPlaying && KickStarter.runtimeInventory != null && inventoryBoxType != AC_InventoryBoxType.DisplaySelected)
			{
				if (_item == KickStarter.runtimeInventory.highlightItem && _item.activeTex != null)
				{
					KickStarter.runtimeInventory.DrawHighlighted (rect);
					return;
				}

				if (_item.activeTex != null && ((isActive && KickStarter.settingsManager.activeWhenHover) || _item == KickStarter.runtimeInventory.selectedItem))
				{
					tex = _item.activeTex;
				}
				else if (_item.tex != null)
				{
					tex = _item.tex;
				}
			}
			else if (_item.tex != null)
			{
				tex = _item.tex;
			}

			if (tex != null)
			{
				GUI.DrawTexture (rect, tex, ScaleMode.StretchToFill, true, 0f);
			}
		}


		public override string GetLabel (int i, int languageNumber)
		{
			if (items.Count <= (i+offset) || items [i+offset] == null)
			{
				return null;
			}

			return items [i+offset].GetLabel (languageNumber);
		}


		public InvItem GetItem (int i)
		{
			if (items.Count <= (i+offset) || items [i+offset] == null)
			{
				return null;
			}

			return items [i+offset];
		}


		private string GetCount (int i)
		{
			if (items.Count <= (i+offset) || items [i+offset] == null)
			{
				return "";
			}

			if (items [i+offset].count < 2)
			{
				return "";
			}
			return items [i + offset].count.ToString ();
		}


		public void ResetOffset ()
		{
			offset = 0;
		}
		
		
		protected override void AutoSize ()
		{
			if (items.Count > 0)
			{
				foreach (InvItem _item in items)
				{
					if (_item != null)
					{
						if (displayType == ConversationDisplayType.IconOnly)
						{
							AutoSize (new GUIContent (_item.tex));
						}
						else if (displayType == ConversationDisplayType.TextOnly)
						{
							AutoSize (new GUIContent (_item.label));
						}
						return;
					}
				}
			}
			AutoSize (GUIContent.none);
		}


		public void ClickContainer (MouseState _mouseState, int _slot, Container container)
		{
			if (container == null || KickStarter.runtimeInventory == null) return;

			KickStarter.runtimeInventory.SetFont (font, GetFontSize (), fontColor, textEffects);

			if (_mouseState == MouseState.SingleClick)
			{
				if (KickStarter.runtimeInventory.selectedItem == null)
				{
					if (container.items.Count > (_slot+offset) && container.items [_slot+offset] != null)
					{
						ContainerItem containerItem = container.items [_slot + offset];
						KickStarter.runtimeInventory.Add (containerItem.linkedID, containerItem.count, selectItemsAfterTaking, -1);
						container.items.Remove (containerItem);
					}
				}
				else
				{
					// Placing an item inside the container
					container.InsertAt (KickStarter.runtimeInventory.selectedItem, _slot+offset);
					KickStarter.runtimeInventory.Remove (KickStarter.runtimeInventory.selectedItem);
				}
			}

			else if (_mouseState == MouseState.RightClick)
			{
				if (KickStarter.runtimeInventory.selectedItem != null)
				{
					KickStarter.runtimeInventory.SetNull ();
				}
			}
		}


		public override void ProcessClick (AC.Menu _menu, int _slot, MouseState _mouseState)
		{
			if (KickStarter.stateHandler.gameState == GameState.Cutscene)
			{
				return;
			}

			if (inventoryBoxType == AC_InventoryBoxType.CustomScript)
			{
				MenuSystem.OnElementClick (_menu, this, _slot, (int) _mouseState);
			}
			else
			{
				KickStarter.runtimeInventory.ProcessInventoryBoxClick (_menu, this, _slot, _mouseState);
			}
		}

	}

}