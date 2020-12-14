/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"MenuDialogList.cs"
 * 
 *	This MenuElement lists the available options of the active conversation,
 *	and runs them when clicked on.
 * 
 */

using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;	
#endif

namespace AC
{
	
	public class MenuDialogList : MenuElement
	{

		public UISlot[] uiSlots;

		public TextEffects textEffects;
		public ConversationDisplayType displayType = ConversationDisplayType.TextOnly;
		public Texture2D testIcon = null;
		public TextAnchor anchor;
		public bool fixedOption;
		public int optionToShow;
		public int maxSlots = 10;
		
		private int numOptions = 0;
		private string[] labels = null;
		private Texture2D[] icons;


		public override void Declare ()
		{
			uiSlots = null;

			isVisible = true;
			isClickable = true;
			fixedOption = false;
			displayType = ConversationDisplayType.TextOnly;
			testIcon = null;
			optionToShow = 1;
			numSlots = 0;
			SetSize (new Vector2 (20f, 5f));
			maxSlots = 10;
			anchor = TextAnchor.MiddleLeft;
			textEffects = TextEffects.None;

			base.Declare ();
		}

		
		public override MenuElement DuplicateSelf ()
		{
			MenuDialogList newElement = CreateInstance <MenuDialogList>();
			newElement.Declare ();
			newElement.CopyDialogList (this);
			return newElement;
		}
		
		
		public void CopyDialogList (MenuDialogList _element)
		{
			uiSlots = _element.uiSlots;

			textEffects = _element.textEffects;
			displayType = _element.displayType;
			testIcon = _element.testIcon;
			anchor = _element.anchor;
			labels = _element.labels;
			fixedOption = _element.fixedOption;
			optionToShow = _element.optionToShow;
			maxSlots = _element.maxSlots;

			base.Copy (_element);
		}


		public override void HideAllUISlots ()
		{
			LimitUISlotVisibility (uiSlots, 0);
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
						ProcessClick (_menu, j, KickStarter.playerInput.mouseState);
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
			fixedOption = EditorGUILayout.Toggle ("Fixed option number?", fixedOption);
			if (fixedOption)
			{
				numSlots = 1;
				slotSpacing = 0f;
				optionToShow = EditorGUILayout.IntSlider ("Option to display:", optionToShow, 1, 10);
			}
			else
			{
				maxSlots = EditorGUILayout.IntSlider ("Maximum no. of slots:", maxSlots, 1, 10);

				if (source == MenuSource.AdventureCreator)
				{
					numSlots = EditorGUILayout.IntSlider ("Test slots:", numSlots, 1, maxSlots);
					slotSpacing = EditorGUILayout.Slider ("Slot spacing:", slotSpacing, 0f, 20f);
					orientation = (ElementOrientation) EditorGUILayout.EnumPopup ("Slot orientation:", orientation);
					if (orientation == ElementOrientation.Grid)
					{
						gridWidth = EditorGUILayout.IntSlider ("Grid size:", gridWidth, 1, 10);
					}
				}
			}
			displayType = (ConversationDisplayType) EditorGUILayout.EnumPopup ("Display type:", displayType);

			if (source == MenuSource.AdventureCreator)
			{
				if (displayType == ConversationDisplayType.IconOnly)
				{
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("Test icon:", GUILayout.Width (145f));
					testIcon = (Texture2D) EditorGUILayout.ObjectField (testIcon, typeof (Texture2D), false, GUILayout.Width (70f), GUILayout.Height (30f));
					EditorGUILayout.EndHorizontal ();
				}
				else
				{
					anchor = (TextAnchor) EditorGUILayout.EnumPopup ("Text alignment:", anchor);
					textEffects = (TextEffects) EditorGUILayout.EnumPopup ("Text effect:", textEffects);
				}
			}
			else
			{
				EditorGUILayout.EndVertical ();
				EditorGUILayout.BeginVertical ("Button");

				if (fixedOption)
				{
					uiSlots = ResizeUISlots (uiSlots, 1);
				}
				else
				{
					uiSlots = ResizeUISlots (uiSlots, maxSlots);
				}

				for (int i=0; i<uiSlots.Length; i++)
				{
					uiSlots[i].LinkedUiGUI (i, source);
				}
			}
			EditorGUILayout.EndVertical ();
			
			base.ShowGUI (source);
		}
		
		#endif
		
		
		public override void PreDisplay (int _slot, int languageNumber, bool isActive)
		{
			if (fixedOption)
			{
				_slot = 0;
			}

			if (Application.isPlaying)
			{
				if (uiSlots != null && uiSlots.Length > _slot)
				{
					LimitUISlotVisibility (uiSlots, numSlots);

					if (displayType == ConversationDisplayType.IconOnly)
					{
						uiSlots[_slot].SetImage (icons [_slot]);
					}
					else
					{
						uiSlots[_slot].SetText (labels [_slot]);
					}
				}
			}
			else
			{
				string fullText = "";
				if (fixedOption)
				{
					fullText = "Dialogue option " + optionToShow.ToString ();
				}
				else
				{
					fullText = "Dialogue option " + _slot.ToString ();
				}
				if (labels == null || labels.Length != numSlots)
				{
					labels = new string [numSlots];
				}
				labels [_slot] = fullText;
			}
		}
		
		
		public override void Display (GUIStyle _style, int _slot, float zoom, bool isActive)
		{
			base.Display (_style, _slot, zoom, isActive);

			if (fixedOption)
			{
				_slot = 0;
			}

			_style.wordWrap = true;
			_style.alignment = anchor;
			if (zoom < 1f)
			{
				_style.fontSize = (int) ((float) _style.fontSize * zoom);
			}

			if (displayType == ConversationDisplayType.TextOnly)
			{
				if (textEffects != TextEffects.None)
				{
					AdvGame.DrawTextEffect (ZoomRect (GetSlotRectRelative (_slot), zoom), labels [_slot], _style, Color.black, _style.normal.textColor, 2, textEffects);
				}
				else
				{
					GUI.Label (ZoomRect (GetSlotRectRelative (_slot), zoom), labels [_slot], _style);
				}
			}
			else
			{
				if (Application.isPlaying && icons[_slot] != null)
				{
					GUI.DrawTexture (ZoomRect (GetSlotRectRelative (_slot), zoom), icons[_slot], ScaleMode.StretchToFill, true, 0f);
				}
				else if (testIcon != null)
				{
					GUI.DrawTexture (ZoomRect (GetSlotRectRelative (_slot), zoom), testIcon, ScaleMode.StretchToFill, true, 0f);
				}
				
				GUI.Label (ZoomRect (GetSlotRectRelative (_slot), zoom), "", _style);
			}
		}
		
		
		public override void RecalculateSize (MenuSource source)
		{
			if (Application.isPlaying)
			{
				if (KickStarter.playerInput.activeConversation)
				{
					numOptions = KickStarter.playerInput.activeConversation.GetCount ();
					
					if (fixedOption)
					{
						if (numOptions < optionToShow)
						{
							numSlots = 0;
						}
						else
						{
							numSlots = 1;
							labels = new string [numSlots];
							labels[0] = KickStarter.playerInput.activeConversation.GetOptionName (optionToShow - 1);
							
							icons = new Texture2D [numSlots];
							icons[0] = KickStarter.playerInput.activeConversation.GetOptionIcon (optionToShow - 1);
						}
					}
					else
					{
						numSlots = numOptions;
						if (numSlots > maxSlots)
						{
							numSlots = maxSlots;
						}

						labels = new string [numSlots];
						icons = new Texture2D [numSlots];
						for (int i=0; i<numSlots; i++)
						{
							labels[i] = KickStarter.playerInput.activeConversation.GetOptionName (i + offset);
							icons[i] = KickStarter.playerInput.activeConversation.GetOptionIcon (i + offset);
						}
						
						LimitOffset (numOptions);
					}
				}
				else
				{
					numSlots = 0;
				}
			}
			else if (fixedOption)
			{
				numSlots = 1;
				offset = 0;
				labels = new string [numSlots];
				icons = new Texture2D [numSlots];
			}

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
		
		
		public override void Shift (AC_ShiftInventory shiftType, int amount)
		{
			if (isVisible && numSlots >= maxSlots)
			{
				Shift (shiftType, maxSlots, numOptions, amount);
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
				if ((maxSlots + offset) >= numOptions)
				{
					return false;
				}
			}
			return true;
		}
		
		
		public override string GetLabel (int slot, int languageNumber)
		{
			if (labels.Length > slot)
			{
				return labels[slot];
			}
			
			return "";
		}
		
		
		public override void ProcessClick (AC.Menu _menu, int _slot, MouseState _mouseState)
		{
			if (KickStarter.stateHandler.gameState != GameState.DialogOptions)
			{
				return;
			}

			if (KickStarter.playerInput && KickStarter.playerInput.activeConversation)
			{
				if (fixedOption)
				{
					KickStarter.playerInput.activeConversation.RunOption (optionToShow - 1);
				}
				else
				{
					KickStarter.playerInput.activeConversation.RunOption (_slot + offset);
				}
			}

			offset = 0;
		}
		
		
		protected override void AutoSize ()
		{
			if (displayType == ConversationDisplayType.IconOnly)
			{
				AutoSize (new GUIContent (testIcon));
			}
			else
			{
				AutoSize (new GUIContent ("Dialogue option 0"));
			}
			
		}
		
	}
	
}
