/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"MenuButton.cs"
 * 
 *	This MenuElement can be clicked on to perform a specified function.
 * 
 */

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;	
#endif

namespace AC
{

	[System.Serializable]
	public class MenuButton : MenuElement
	{

		public UnityEngine.UI.Button uiButton;
		private Text uiText;

		public string label = "Element";

		public string hotspotLabel = "";
		public int hotspotLabelID = -1;

		public TextAnchor anchor;
		public TextEffects textEffects;
		public AC_ButtonClickType buttonClickType;
		public SimulateInputType simulateInput = SimulateInputType.Button;
		public float simulateValue = 1f;
		public bool doFade;
		public string switchMenuTitle;
		public string inventoryBoxTitle;
		public AC_ShiftInventory shiftInventory;
		public int shiftAmount = 1;
		public bool loopJournal = false;
		public ActionListAsset actionList;
		public string inputAxis;
		public Texture2D clickTexture;
		public bool onlyShowWhenEffective;
		public bool allowContinuousClick = false;

		private MenuElement elementToShift;
		private float clickAlpha = 0f;
		private string fullText;

		
		public override void Declare ()
		{
			uiButton = null;
			uiText = null;
			label = "Button";
			hotspotLabel = "";
			hotspotLabelID = -1;
			isVisible = true;
			isClickable = true;
			textEffects = TextEffects.None;
			buttonClickType = AC_ButtonClickType.RunActionList;
			simulateInput = SimulateInputType.Button;
			simulateValue = 1f;
			numSlots = 1;
			anchor = TextAnchor.MiddleCenter;
			SetSize (new Vector2 (10f, 5f));
			doFade = false;
			switchMenuTitle = "";
			inventoryBoxTitle = "";
			shiftInventory = AC_ShiftInventory.ShiftLeft;
			loopJournal = false;
			actionList = null;
			inputAxis = "";
			clickTexture = null;
			clickAlpha = 0f;
			shiftAmount = 1;
			onlyShowWhenEffective = false;
			allowContinuousClick = false;

			base.Declare ();
		}


		public override MenuElement DuplicateSelf ()
		{
			MenuButton newElement = CreateInstance <MenuButton>();
			newElement.Declare ();
			newElement.CopyButton (this);
			return newElement;
		}
		
		
		public void CopyButton (MenuButton _element)
		{
			uiButton = _element.uiButton;
			uiText = _element.uiText;
			label = _element.label;
			hotspotLabel = _element.hotspotLabel;
			hotspotLabelID = _element.hotspotLabelID;
			anchor = _element.anchor;
			textEffects = _element.textEffects;
			buttonClickType = _element.buttonClickType;
			simulateInput = _element.simulateInput;
			simulateValue = _element.simulateValue;
			doFade = _element.doFade;
			switchMenuTitle = _element.switchMenuTitle;
			inventoryBoxTitle = _element.inventoryBoxTitle;
			shiftInventory = _element.shiftInventory;
			loopJournal = _element.loopJournal;
			actionList = _element.actionList;
			inputAxis = _element.inputAxis;
			clickTexture = _element.clickTexture;
			clickAlpha = _element.clickAlpha;
			shiftAmount = _element.shiftAmount;
			onlyShowWhenEffective = _element.onlyShowWhenEffective;
			allowContinuousClick = _element.allowContinuousClick;
				
			base.Copy (_element);
		}


		public override void LoadUnityUI (AC.Menu _menu)
		{
			uiButton = LinkUIElement <UnityEngine.UI.Button>();
			if (uiButton)
			{
				if (uiButton.GetComponentInChildren <Text>())
				{
					uiText = uiButton.GetComponentInChildren <Text>();
				}
				uiButton.onClick.AddListener (() => {
					ProcessClick (_menu, 0, KickStarter.playerInput.mouseState);
				});
			}
		}


		public override GameObject GetObjectToSelect ()
		{
			if (uiButton)
			{
				return uiButton.gameObject;
			}
			return null;
		}


		public override RectTransform GetRectTransform (int _slot)
		{
			if (uiButton)
			{
				return uiButton.GetComponent <RectTransform>();
			}
			return null;
		}


		#if UNITY_EDITOR
		
		public override void ShowGUI (MenuSource source)
		{
			EditorGUILayout.BeginVertical ("Button");

			if (source != MenuSource.AdventureCreator)
			{
				uiButton = LinkedUiGUI <UnityEngine.UI.Button> (uiButton, "Linked Button:", source);
				EditorGUILayout.EndVertical ();
				EditorGUILayout.BeginVertical ("Button");
			}

			label = EditorGUILayout.TextField ("Button text:", label);

			if (source == MenuSource.AdventureCreator)
			{
				anchor = (TextAnchor) EditorGUILayout.EnumPopup ("Text alignment:", anchor);
				textEffects = (TextEffects) EditorGUILayout.EnumPopup ("Text effect:", textEffects);
			}

			hotspotLabel = EditorGUILayout.TextField ("Hotspot label override:", hotspotLabel);

			if (source == MenuSource.AdventureCreator)
			{
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Click texture:", GUILayout.Width (145f));
				clickTexture = (Texture2D) EditorGUILayout.ObjectField (clickTexture, typeof (Texture2D), false, GUILayout.Width (70f), GUILayout.Height (30f));
				EditorGUILayout.EndHorizontal ();
			}

			buttonClickType = (AC_ButtonClickType) EditorGUILayout.EnumPopup ("Click type:", buttonClickType);
		
			if (buttonClickType == AC_ButtonClickType.TurnOffMenu)
			{
				doFade = EditorGUILayout.Toggle ("Do transition?", doFade);
			}
			else if (buttonClickType == AC_ButtonClickType.Crossfade)
			{
				switchMenuTitle = EditorGUILayout.TextField ("Menu to switch to:", switchMenuTitle);
			}
			else if (buttonClickType == AC_ButtonClickType.OffsetInventoryOrDialogue)
			{
				inventoryBoxTitle = EditorGUILayout.TextField ("Element to affect:", inventoryBoxTitle);
				shiftInventory = (AC_ShiftInventory) EditorGUILayout.EnumPopup ("Offset type:", shiftInventory);
				shiftAmount = EditorGUILayout.IntField ("Offset amount:", shiftAmount);
				onlyShowWhenEffective = EditorGUILayout.Toggle ("Only show when effective?", onlyShowWhenEffective);
			}
			else if (buttonClickType == AC_ButtonClickType.OffsetJournal)
			{
				inventoryBoxTitle = EditorGUILayout.TextField ("Journal to affect:", inventoryBoxTitle);
				shiftInventory = (AC_ShiftInventory) EditorGUILayout.EnumPopup ("Offset type:", shiftInventory);
				loopJournal = EditorGUILayout.Toggle ("Cycle pages?", loopJournal);
			}
			else if (buttonClickType == AC_ButtonClickType.RunActionList)
			{
				actionList = ActionListAssetMenu.AssetGUI ("ActionList to run:", actionList);
			}
			else if (buttonClickType == AC_ButtonClickType.CustomScript)
			{
				allowContinuousClick = EditorGUILayout.Toggle ("Accept held-down clicks?", allowContinuousClick);
				ShowClipHelp ();
			}
			else if (buttonClickType == AC_ButtonClickType.SimulateInput)
			{
				simulateInput = (SimulateInputType) EditorGUILayout.EnumPopup ("Simulate:", simulateInput);
				inputAxis = EditorGUILayout.TextField ("Input axis:", inputAxis);
				if (simulateInput == SimulateInputType.Axis)
				{
					simulateValue = EditorGUILayout.FloatField ("Input value:", simulateValue);
				}
			}
			EditorGUILayout.EndVertical ();
			
			base.ShowGUI (source);
		}
		
		#endif


		public void ShowClick ()
		{
			if (isClickable)
			{
				clickAlpha = 1f;
			}
		}

		
		public override void PreDisplay (int _slot, int languageNumber, bool isActive)
		{
			if (buttonClickType == AC_ButtonClickType.OffsetInventoryOrDialogue && onlyShowWhenEffective && inventoryBoxTitle != "" && Application.isPlaying)
			{
				if (elementToShift == null)
				{
					foreach (AC.Menu _menu in PlayerMenus.GetMenus ())
					{
						if (_menu != null && _menu.elements.Contains (this))
						{
							elementToShift = PlayerMenus.GetElementWithName (_menu.title, inventoryBoxTitle);
							break;
						}
					}
				}
				if (elementToShift != null)
				{
					isVisible = elementToShift.CanBeShifted (shiftInventory);
				}
			}

			fullText = TranslateLabel (label, languageNumber);

			if (uiButton != null && uiText != null)
			{
				uiText.text = fullText;
			}

			UpdateUIElement (uiButton);
		}
		
		
		public override void Display (GUIStyle _style, int _slot, float zoom, bool isActive)
		{
			if (buttonClickType == AC_ButtonClickType.OffsetInventoryOrDialogue && onlyShowWhenEffective && inventoryBoxTitle != "" && Application.isPlaying)
			{
				if (elementToShift != null)
				{
					if (!elementToShift.CanBeShifted (shiftInventory))
					{
						return;
					}
				}
			}

			if (clickAlpha > 0f)
			{
				if (clickTexture)
				{
					Color tempColor = GUI.color;
					tempColor.a = clickAlpha;
					GUI.color = tempColor;
					GUI.DrawTexture (ZoomRect (GetSlotRectRelative (_slot), zoom), clickTexture, ScaleMode.StretchToFill, true, 0f);
					tempColor.a = 1f;
					GUI.color = tempColor;
				}
				clickAlpha -= Time.deltaTime;
				if (clickAlpha < 0f)
				{
					clickAlpha = 0f;
				}
			}

			base.Display (_style, _slot, zoom, isActive);

			_style.alignment = anchor;
			if (zoom < 1f)
			{
				_style.fontSize = (int) ((float) _style.fontSize * zoom);
			}
			
			if (textEffects != TextEffects.None)
			{
				AdvGame.DrawTextEffect (ZoomRect (relativeRect, zoom), fullText, _style, Color.black, _style.normal.textColor, 2, textEffects);
			}
			else
			{
				GUI.Label (ZoomRect (relativeRect, zoom), fullText, _style);
			}
		}


		public override string GetLabel (int slot, int languageNumber)
		{
			return TranslateLabel (label, languageNumber);
		}

		
		protected override void AutoSize ()
		{
			if (label == "" && backgroundTexture != null)
			{
				GUIContent content = new GUIContent (backgroundTexture);
				AutoSize (content);
			}
			else
			{
				GUIContent content = new GUIContent (TranslateLabel (label, Options.GetLanguage ()));
				AutoSize (content);
			}
		}


		public override void RecalculateSize (MenuSource source)
		{
			clickAlpha = 0f;
			base.RecalculateSize (source);
		}


		public override void ProcessClick (AC.Menu _menu, int _slot, MouseState _mouseState)
		{
			if (KickStarter.stateHandler.gameState == GameState.Cutscene)
			{
				return;
			}

			ShowClick ();

			if (buttonClickType == AC_ButtonClickType.TurnOffMenu)
			{
				_menu.TurnOff (doFade);
			}
			else if (buttonClickType == AC_ButtonClickType.Crossfade)
			{
				AC.Menu menuToSwitchTo = PlayerMenus.GetMenuWithName (switchMenuTitle);
				
				if (menuToSwitchTo != null)
				{
					KickStarter.playerMenus.CrossFade (menuToSwitchTo);
				}
				else
				{
					Debug.LogWarning ("Cannot find any menu of name '" + switchMenuTitle + "'");
				}
			}
			else if (buttonClickType == AC_ButtonClickType.OffsetInventoryOrDialogue)
			{
				if (elementToShift == null)
				{
					elementToShift = PlayerMenus.GetElementWithName (_menu.title, inventoryBoxTitle);
				}
				if (elementToShift != null)
				{
					elementToShift.Shift (shiftInventory, shiftAmount);
					elementToShift.RecalculateSize (_menu.menuSource);
				}
				else
				{
					Debug.LogWarning ("Cannot find '" + inventoryBoxTitle + "' inside '" + _menu.title + "'");
				}
			}
			else if (buttonClickType == AC_ButtonClickType.OffsetJournal)
			{
				MenuJournal journalToShift = (MenuJournal) PlayerMenus.GetElementWithName (_menu.title, inventoryBoxTitle);
				
				if (journalToShift != null)
				{
					journalToShift.Shift (shiftInventory, loopJournal);
					journalToShift.RecalculateSize (_menu.menuSource);
				}
				else
				{
					Debug.LogWarning ("Cannot find '" + inventoryBoxTitle + "' inside '" + _menu.title + "'");
				}
			}
			else if (buttonClickType == AC_ButtonClickType.RunActionList)
			{
				if (actionList)
				{
					AdvGame.RunActionListAsset (actionList);
				}
			}
			else if (buttonClickType == AC_ButtonClickType.CustomScript)
			{
				MenuSystem.OnElementClick (_menu, this, _slot, (int) _mouseState);
			}
			else if (buttonClickType == AC_ButtonClickType.SimulateInput)
			{
				KickStarter.playerInput.SimulateInput (simulateInput, inputAxis, simulateValue);
			}
		}


		public override void ProcessContinuousClick (AC.Menu _menu, MouseState _mouseState)
		{
			if (buttonClickType == AC_ButtonClickType.SimulateInput)
			{
				KickStarter.playerInput.SimulateInput (simulateInput, inputAxis, simulateValue);
			}
			else if (buttonClickType == AC_ButtonClickType.CustomScript && allowContinuousClick)
			{
				MenuSystem.OnElementClick (_menu, this, 0, (int) _mouseState);
			}
		}


		public string GetHotspotLabel (int languageNumber)
		{
			return SpeechManager.GetTranslation (hotspotLabel, hotspotLabelID, languageNumber);
		}
		
	}

}