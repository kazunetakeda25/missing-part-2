/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"MenuSavesList.cs"
 * 
 *	This MenuElement handles the display of any saved games recorded.
 * 
 */

using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;	
#endif

namespace AC
{

	public class MenuSavesList : MenuElement
	{

		public UISlot[] uiSlots;

		public enum SaveDisplayType { LabelOnly, ScreenshotOnly, LabelAndScreenshot };

		public string newSaveText = "New save";
		public TextEffects textEffects;
		public TextAnchor anchor;
		public AC_SaveListType saveListType;
		public int maxSaves = 5;
		public ActionListAsset actionListOnSave;
		public SaveDisplayType displayType = SaveDisplayType.LabelOnly;
		public Texture2D blankSlotTexture;

		// Import
		public string importProductName;
		public string importSaveFilename;
		public bool checkImportBool;
		public int checkImportVar;

		public bool fixedOption;
		public int optionToShow;

		private string[] labels = null;
		private bool newSaveSlot = false;

		
		public override void Declare ()
		{
			uiSlots = null;

			newSaveText = "New save";
			isVisible = true;
			isClickable = true;
			numSlots = 1;
			maxSaves = 5;

			SetSize (new Vector2 (20f, 5f));
			anchor = TextAnchor.MiddleCenter;
			saveListType = AC_SaveListType.Save;

			actionListOnSave = null;
			newSaveSlot = false;
			textEffects = TextEffects.None;
			displayType = SaveDisplayType.LabelOnly;
			blankSlotTexture = null;

			fixedOption = false;
			optionToShow = 1;

			importProductName = "";
			importSaveFilename = "";
			checkImportBool = false;
			checkImportVar = 0;

			base.Declare ();
		}


		public override MenuElement DuplicateSelf ()
		{
			MenuSavesList newElement = CreateInstance <MenuSavesList>();
			newElement.Declare ();
			newElement.CopySavesList (this);
			return newElement;
		}
		
		
		public void CopySavesList (MenuSavesList _element)
		{
			uiSlots = _element.uiSlots;

			newSaveText = _element.newSaveText;
			textEffects = _element.textEffects;
			anchor = _element.anchor;
			saveListType = _element.saveListType;
			maxSaves = _element.maxSaves;
			actionListOnSave = _element.actionListOnSave;
			displayType = _element.displayType;
			blankSlotTexture = _element.blankSlotTexture;
			fixedOption = _element.fixedOption;
			optionToShow = _element.optionToShow;
			importProductName = _element.importProductName;
			importSaveFilename = _element.importSaveFilename;
			checkImportBool = _element.checkImportBool;
			checkImportVar = _element.checkImportVar;
			
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
				optionToShow = EditorGUILayout.IntField ("Option to display:", optionToShow);
			}
			else
			{
				maxSaves = EditorGUILayout.IntField ("Max saves:", maxSaves);

				if (source == MenuSource.AdventureCreator)
				{
					numSlots = EditorGUILayout.IntSlider ("Test slots:", numSlots, 1, 10);
					slotSpacing = EditorGUILayout.Slider ("Slot spacing:", slotSpacing, 0f, 20f);
					orientation = (ElementOrientation) EditorGUILayout.EnumPopup ("Slot orientation:", orientation);
					if (orientation == ElementOrientation.Grid)
					{
						gridWidth = EditorGUILayout.IntSlider ("Grid size:", gridWidth, 1, 10);
					}
				}
			}

			if (source == MenuSource.AdventureCreator)
			{
				anchor = (TextAnchor) EditorGUILayout.EnumPopup ("Text alignment:", anchor);
				textEffects = (TextEffects) EditorGUILayout.EnumPopup ("Text effect:", textEffects);
			}

			displayType = (SaveDisplayType) EditorGUILayout.EnumPopup ("Display:", displayType);
			if (displayType != SaveDisplayType.LabelOnly)
			{
				EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("Empty slot texture:", GUILayout.Width (145f));
					blankSlotTexture = (Texture2D) EditorGUILayout.ObjectField (blankSlotTexture, typeof (Texture2D), false, GUILayout.Width (70f), GUILayout.Height (30f));
				EditorGUILayout.EndHorizontal ();
			}
			saveListType = (AC_SaveListType) EditorGUILayout.EnumPopup ("Click action:", saveListType);
			if (saveListType == AC_SaveListType.Save)
			{
				newSaveText = EditorGUILayout.TextField ("'New save' text:", newSaveText);
				actionListOnSave = ActionListAssetMenu.AssetGUI ("ActionList after saving:", actionListOnSave);
			}
			else if (saveListType == AC_SaveListType.Load)
			{
				actionListOnSave = ActionListAssetMenu.AssetGUI ("ActionList after loading:", actionListOnSave);
			}
			else if (saveListType == AC_SaveListType.Import)
			{
				#if UNITY_STANDALONE
				importProductName = EditorGUILayout.TextField ("Import product name:", importProductName);
				importSaveFilename = EditorGUILayout.TextField ("Import save filename:", importSaveFilename);
				actionListOnSave = ActionListAssetMenu.AssetGUI ("ActionList after import:", actionListOnSave);
				checkImportBool = EditorGUILayout.Toggle ("Require Bool to be true?", checkImportBool);
				if (checkImportBool)
				{
					checkImportVar = EditorGUILayout.IntField ("Global Variable ID:", checkImportVar);
				}
				#else
				EditorGUILayout.HelpBox ("This feature is only available for standalone platforms (PC, Mac, Linux)", MessageType.Warning);
				#endif
			}

			if (source != MenuSource.AdventureCreator)
			{
				EditorGUILayout.EndVertical ();
				EditorGUILayout.BeginVertical ("Button");

				if (fixedOption)
				{
					uiSlots = ResizeUISlots (uiSlots, 1);
				}
				else
				{
					uiSlots = ResizeUISlots (uiSlots, maxSaves);
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


		public override string GetLabel (int slot, int languageNumber)
		{
			return SaveSystem.GetSaveSlotLabel (slot, optionToShow, fixedOption);
		}


		public override void HideAllUISlots ()
		{
			LimitUISlotVisibility (uiSlots, 0);
		}


		public override void PreDisplay (int _slot, int languageNumber, bool isActive)
		{
			if (displayType != SaveDisplayType.ScreenshotOnly)
			{
				string fullText;

				if (newSaveSlot)
				{
					if (!fixedOption && _slot == (numSlots -1))
					{
						fullText = TranslateLabel (newSaveText, languageNumber);
					}
					else if (fixedOption && saveListType == AC_SaveListType.Save)
					{
						fullText = TranslateLabel (newSaveText, languageNumber);
					}
					else
					{
						if (saveListType == AC_SaveListType.Import)
						{
							fullText = SaveSystem.GetImportSlotLabel (_slot, optionToShow, fixedOption);
						}
						else
						{
							fullText = SaveSystem.GetSaveSlotLabel (_slot, optionToShow, fixedOption);
						}
					}
				}
				else
				{
					if (saveListType == AC_SaveListType.Import)
					{
						fullText = SaveSystem.GetImportSlotLabel (_slot, optionToShow, fixedOption);
					}
					else
					{
						fullText = SaveSystem.GetSaveSlotLabel (_slot, optionToShow, fixedOption);
					}
				}

				if (!Application.isPlaying)
				{
					if (labels == null || labels.Length != numSlots)
					{
						labels = new string [numSlots];
					}
				}

				labels [_slot] = fullText;
			}

			if (Application.isPlaying)
			{
				if (uiSlots != null && uiSlots.Length > _slot)
				{
					LimitUISlotVisibility (uiSlots, numSlots);
					
					if (displayType != SaveDisplayType.LabelOnly)
					{
						Texture2D tex = null;
						if (saveListType == AC_SaveListType.Import)
						{
							tex = SaveSystem.GetImportSlotScreenshot (_slot, optionToShow, fixedOption);
						}
						else
						{
							tex = SaveSystem.GetSaveSlotScreenshot (_slot, optionToShow, fixedOption);
						}
						if (tex == null)
						{
							tex = blankSlotTexture;
						}
						uiSlots[_slot].SetImage (tex);
					}
					if (displayType != SaveDisplayType.ScreenshotOnly)
					{
						uiSlots[_slot].SetText (labels [_slot]);
					}
				}
			}
		}
		
		
		public override void Display (GUIStyle _style, int _slot, float zoom, bool isActive)
		{
			base.Display (_style, _slot, zoom, isActive);

			if (displayType != SaveDisplayType.LabelOnly)
			{
				Texture2D tex = null;
				if (saveListType == AC_SaveListType.Import)
				{
					tex = SaveSystem.GetImportSlotScreenshot (_slot, optionToShow, fixedOption);
				}
				else
				{
					tex = SaveSystem.GetSaveSlotScreenshot (_slot, optionToShow, fixedOption);
				}
				if (tex == null && blankSlotTexture != null)
				{
					tex = blankSlotTexture;
				}

				if (tex != null)
				{
					GUI.DrawTexture (ZoomRect (GetSlotRectRelative (_slot), zoom), tex, ScaleMode.StretchToFill, true, 0f);
				}
			}

			if (displayType != SaveDisplayType.ScreenshotOnly)
			{
				_style.alignment = anchor;
				if (zoom < 1f)
				{
					_style.fontSize = (int) ((float) _style.fontSize * zoom);
				}
				
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


		public override void ProcessClick (AC.Menu _menu, int _slot, MouseState _mouseState)
		{
			if (KickStarter.stateHandler.gameState == GameState.Cutscene)
			{
				return;
			}

			bool isSuccess = true;

			if (saveListType == AC_SaveListType.Save)
			{
				if (newSaveSlot && _slot == (numSlots - 1))
				{
					SaveSystem.SaveNewGame ();
				}
				else
				{
					SaveSystem.SaveGame (_slot, optionToShow, fixedOption);
				}
			}
			else if (saveListType == AC_SaveListType.Load)
			{
				if (fixedOption && newSaveSlot)
				{
					isSuccess = false;
				}
				else
				{
					isSuccess = SaveSystem.LoadGame (_slot, optionToShow, fixedOption);
				}
			}
			else if (saveListType == AC_SaveListType.Import)
			{
				if (fixedOption && newSaveSlot)
				{
					isSuccess = false;
				}
				else
				{
					isSuccess = SaveSystem.ImportGame (_slot, optionToShow, fixedOption);
				}
			}

			if (isSuccess)
			{
				if (saveListType == AC_SaveListType.Save)
				{
					_menu.TurnOff (true);
				}
				else if (saveListType == AC_SaveListType.Load)
				{
					_menu.TurnOff (false);
				}
				AdvGame.RunActionListAsset (actionListOnSave);
			}
		}

		
		public override void RecalculateSize (MenuSource source)
		{
			newSaveSlot = false;

			if (Application.isPlaying)
			{
				if (saveListType == AC_SaveListType.Import)
				{
					if (checkImportBool)
					{
						KickStarter.saveSystem.GatherImportFiles (importProductName, importSaveFilename, checkImportVar);
					}
					else
					{
						KickStarter.saveSystem.GatherImportFiles (importProductName, importSaveFilename, -1);
					}
				}

				if (fixedOption)
				{
					numSlots = 1;

					if (saveListType == AC_SaveListType.Import)
					{
						if (!SaveSystem.DoesImportExist (optionToShow))
						{
							newSaveSlot = true;
						}
					}
					else
					{
						if (!SaveSystem.DoesSaveExist (optionToShow))
						{
							newSaveSlot = true;
						}
					}
				}
				else
				{
					if (saveListType == AC_SaveListType.Import)
					{
						numSlots = SaveSystem.GetNumImportSlots ();
					}
					else
					{
						numSlots = SaveSystem.GetNumSlots ();

						if (saveListType == AC_SaveListType.Save && numSlots < maxSaves)
						{
							newSaveSlot = true;
							numSlots ++;
						}
					}
				}
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
		
		
		protected override void AutoSize ()
		{
			if (displayType == SaveDisplayType.ScreenshotOnly)
			{
				if (blankSlotTexture != null)
				{
					AutoSize (new GUIContent (blankSlotTexture));
				}
				else
				{
					AutoSize (GUIContent.none);
				}
			}
			else if (displayType == SaveDisplayType.LabelAndScreenshot)
			{
				if (blankSlotTexture != null)
				{
					AutoSize (new GUIContent (blankSlotTexture));
				}
				else
				{
					AutoSize (new GUIContent (SaveSystem.GetSaveSlotLabel (0, optionToShow, fixedOption)));
				}
			}
			else
			{
				AutoSize (new GUIContent (SaveSystem.GetSaveSlotLabel (0, optionToShow, fixedOption)));
			}
		}
		
	}

}