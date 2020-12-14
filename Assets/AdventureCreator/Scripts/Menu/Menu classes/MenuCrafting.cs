/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"MenuCrafting.cs"
 * 
 *	This MenuElement stores multiple Inventory Items to be combined.
 * 
 */

using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{
	
	public class MenuCrafting : MenuElement
	{

		public UISlot[] uiSlots;

		public TextEffects textEffects;
		public CraftingElementType craftingType = CraftingElementType.Ingredients;
		public int categoryID;
		public List<InvItem> items = new List<InvItem>();
		public ConversationDisplayType displayType = ConversationDisplayType.IconOnly;

		private Recipe activeRecipe;
		private bool[] isFilled;
		private string[] labels = null;


		public override void Declare ()
		{
			uiSlots = null;
			isVisible = true;
			isClickable = true;
			numSlots = 4;
			SetSize (new Vector2 (6f, 10f));
			textEffects = TextEffects.None;
			craftingType = CraftingElementType.Ingredients;
			displayType = ConversationDisplayType.IconOnly;
			items = new List<InvItem>();
		}


		public override MenuElement DuplicateSelf ()
		{
			MenuCrafting newElement = CreateInstance <MenuCrafting>();
			newElement.Declare ();
			newElement.CopyCrafting (this);
			return newElement;
		}
		
		
		public void CopyCrafting (MenuCrafting _element)
		{
			uiSlots = _element.uiSlots;
			isClickable = _element.isClickable;
			textEffects = _element.textEffects;
			numSlots = _element.numSlots;
			craftingType = _element.craftingType;
			displayType = _element.displayType;

			PopulateList (MenuSource.AdventureCreator);
			
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
			craftingType = (CraftingElementType) EditorGUILayout.EnumPopup ("Crafting element type:", craftingType);

			if (craftingType == CraftingElementType.Ingredients)
			{
				numSlots = EditorGUILayout.IntSlider ("Number of slots:", numSlots, 1, 12);
				if (source == MenuSource.AdventureCreator)
				{
					slotSpacing = EditorGUILayout.Slider ("Slot spacing:", slotSpacing, 0f, 20f);
					orientation = (ElementOrientation) EditorGUILayout.EnumPopup ("Slot orientation:", orientation);
					if (orientation == ElementOrientation.Grid)
					{
						gridWidth = EditorGUILayout.IntSlider ("Grid size:", gridWidth, 1, 10);
					}
				}
			}
			else
			{
				categoryID = -1;
				numSlots = 1;
			}

			if (source != MenuSource.AdventureCreator)
			{
				EditorGUILayout.EndVertical ();
				EditorGUILayout.BeginVertical ("Button");
				
				uiSlots = ResizeUISlots (uiSlots, numSlots);
				
				for (int i=0; i<uiSlots.Length; i++)
				{
					uiSlots[i].LinkedUiGUI (i, source);
				}
			}

			isClickable = true;
			EditorGUILayout.EndVertical ();
			
			PopulateList (source);
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
			string fullText = "";
			if (displayType == ConversationDisplayType.TextOnly)
			{
				InvItem item = GetItem (_slot);
				if (item != null)
				{
					fullText = item.label;
					if (KickStarter.runtimeInventory != null)
					{
						fullText = KickStarter.runtimeInventory.GetLabel (item, languageNumber);
					}
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

			if (craftingType == CraftingElementType.Ingredients)
			{
				if (isFilled == null || isFilled.Length != numSlots)
				{
					isFilled = new bool [numSlots];
				}
			
				// Is slot filled?
				isFilled [_slot] = false;
				foreach (InvItem _item in items)
				{
					if (_item.recipeSlot == _slot)
					{
						isFilled [_slot] = true;
						break;
					}
				}
			}

			if (Application.isPlaying)
			{
				if (uiSlots != null && uiSlots.Length > _slot)
				{
					LimitUISlotVisibility (uiSlots, numSlots);

					uiSlots[_slot].SetText (labels [_slot]);

					if (displayType == ConversationDisplayType.IconOnly)
					{
						if ((craftingType == CraftingElementType.Ingredients && isFilled [_slot]) || (craftingType == CraftingElementType.Output && items.Count > 0))
						{
							uiSlots[_slot].SetImage (GetTexture (_slot));
						}
						else
						{
							uiSlots[_slot].SetImage (null);
						}
					}
				}
			}
		}

		
		public override void Display (GUIStyle _style, int _slot, float zoom, bool isActive)
		{
			base.Display (_style, _slot, zoom, isActive);

			if (craftingType == CraftingElementType.Ingredients)
			{
				if (displayType == ConversationDisplayType.IconOnly)
				{
					GUI.Label (GetSlotRectRelative (_slot), "", _style);

					if (!isFilled [_slot])
					{
						return;
					}
					DrawTexture (ZoomRect (GetSlotRectRelative (_slot), zoom), _slot);
					_style.normal.background = null;
									}
				else
				{
					if (!isFilled [_slot])
					{
						GUI.Label (GetSlotRectRelative (_slot), "", _style);
						return;
					}
				}

				DrawText (_style, _slot, zoom);
			}
			else if (craftingType == CraftingElementType.Output)
			{
				GUI.Label (GetSlotRectRelative (_slot), "", _style);
				if (items.Count > 0)
				{
					if (displayType == ConversationDisplayType.IconOnly)
					{
						DrawTexture (ZoomRect (GetSlotRectRelative (_slot), zoom), _slot);
					}
					DrawText (_style, _slot, zoom);
				}
			}
		}


		private void DrawText (GUIStyle _style, int _slot, float zoom)
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
		

		public void HandleDefaultClick (MouseState _mouseState, int _slot)
		{
			if (craftingType == CraftingElementType.Ingredients)
			{
				if (_mouseState == MouseState.SingleClick)
				{
					if (KickStarter.runtimeInventory.selectedItem == null)
					{
						if (GetItem (_slot) != null)
						{
							KickStarter.runtimeInventory.TransferCraftingToLocal (GetItem (_slot).recipeSlot, true);
						}
					}
					else
					{
						if (GetItem (_slot) != null)
						{
							KickStarter.runtimeInventory.TransferCraftingToLocal (GetItem (_slot).recipeSlot, false);
						}

						KickStarter.runtimeInventory.TransferLocalToCrafting (KickStarter.runtimeInventory.selectedItem, _slot);
					}
				}
				else if (_mouseState == MouseState.RightClick)
				{
					if (KickStarter.runtimeInventory.selectedItem != null)
					{
						KickStarter.runtimeInventory.SetNull ();
					}
				}

				PlayerMenus.ResetInventoryBoxes ();
			}
		}
		

		public void ClickOutput (AC.Menu _menu, MouseState _mouseState)
		{
			if (items.Count > 0)
			{
				if (_mouseState == MouseState.SingleClick)
				{
					if (KickStarter.runtimeInventory.selectedItem == null)
					{
						// Pick up created item
						if (activeRecipe.onCreateRecipe == OnCreateRecipe.SelectItem)
						{
							KickStarter.runtimeInventory.PerformCrafting (activeRecipe, true);
						}
						else if (activeRecipe.onCreateRecipe == OnCreateRecipe.RunActionList)
						{
							KickStarter.runtimeInventory.PerformCrafting (activeRecipe, false);
							if (activeRecipe.invActionList != null)
							{
								AdvGame.RunActionListAsset (activeRecipe.invActionList);
							}
						}
						else
						{
							KickStarter.runtimeInventory.PerformCrafting (activeRecipe, false);
						}
					}
				}
				PlayerMenus.ResetInventoryBoxes ();
			}
		}


		public override void RecalculateSize (MenuSource source)
		{
			PopulateList (source);

			isFilled = new bool [numSlots];

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
		
		
		private void PopulateList (MenuSource source)
		{
			if (Application.isPlaying)
			{
				if (craftingType == CraftingElementType.Ingredients)
				{
					items = new List<InvItem>();
					foreach (InvItem _item in KickStarter.runtimeInventory.craftingItems)
					{
						items.Add (_item);
					}
				}
				else if (craftingType == CraftingElementType.Output)
				{
					SetOutput (source, true);
					return;
				}
			}
			else
			{
				items = new List<InvItem>();
				return;
			}
		}


		public void SetOutput (MenuSource source, bool autoCreate)
		{
			items = new List<InvItem>();
			activeRecipe = KickStarter.runtimeInventory.CalculateRecipe (autoCreate);
			if (activeRecipe != null)
			{
				foreach (InvItem assetItem in AdvGame.GetReferences ().inventoryManager.items)
				{
					if (assetItem.id == activeRecipe.resultID)
					{
						InvItem newItem = new InvItem (assetItem);
						newItem.count = 1;
						items.Add (newItem);
					}
				}
			}

			if (!autoCreate)
			{
				base.RecalculateSize (source);
			}
		}

		
		private bool AreAnyItemsInWrongCategory ()
		{
			foreach (InvItem item in items)
			{
				if (item.binID != categoryID)
				{
					return true;
				}
			}
			
			return false;
		}


		private Texture2D GetTexture ( int i)
		{
			Texture2D tex = null;
			
			if (Application.isPlaying)
			{
				tex = GetItem (i).tex;
			}
			else if (items [i].tex != null)
			{
				tex = items [i].tex;
			}
			
			return tex;
		}

		
		private void DrawTexture (Rect rect, int i)
		{
			Texture2D tex = GetTexture (i);

			if (tex != null)
			{
				GUI.DrawTexture (rect, tex, ScaleMode.StretchToFill, true, 0f);
			}
		}
		
		
		public override string GetLabel (int i, int languageNumber)
		{
			if (languageNumber > 0)
			{
				return SpeechManager.GetTranslation (GetItem (i).label, GetItem (i).lineID, languageNumber);
			}
			if (GetItem (i).altLabel != "")
			{
				return GetItem (i).altLabel;
			}
			
			return GetItem (i).label;
		}
		
		
		public InvItem GetItem (int i)
		{
			if (craftingType == CraftingElementType.Output)
			{
				if (items.Count > i)
				{
					return items [i];
				}
			}
			else if (craftingType == CraftingElementType.Ingredients)
			{
				foreach (InvItem _item in items)
				{
					if (_item.recipeSlot == i)
					{
						return _item;
					}
				}
			}
			return null;
		}
		
		
		private string GetCount (int i)
		{
			InvItem item = GetItem (i);
			if (item != null)
			{
				if (GetItem (i).count < 2)
				{
					return "";
				}
				return GetItem (i).count.ToString ();
			}
			return "";
		}


		public override void ProcessClick (AC.Menu _menu, int _slot, MouseState _mouseState)
		{
			if (KickStarter.stateHandler.gameState == GameState.Cutscene)
			{
				return;
			}

			if (craftingType == CraftingElementType.Ingredients)
			{
				HandleDefaultClick (_mouseState, _slot);
			}
			else if (craftingType == CraftingElementType.Output)
			{
				ClickOutput (_menu, _mouseState);
			}
			
			_menu.Recalculate ();
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
				//AutoSize (new GUIContent (items[0].tex));
			}
			else
			{
				AutoSize (GUIContent.none);
			}
		}
		
	}
	
}