/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"MenuCycle.cs"
 * 
 *	This MenuElement is like a label, only it's text cycles through an array when clicked on.
 * 
 */

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{
	
	public class MenuCycle : MenuElement
	{

		public UnityEngine.UI.Button uiButton;
		private Text uiText;

		public string label = "Element";
		public TextEffects textEffects;
		public TextAnchor anchor;
		public int selected;
		public List<string> optionsArray = new List<string>();
		public AC_CycleType cycleType;
		public int varID;

		private string cycleText;

		
		public override void Declare ()
		{
			uiButton = null;
			uiText = null;
			label = "Cycle";
			selected = 0;
			isVisible = true;
			isClickable = true;
			numSlots = 1;
			textEffects = TextEffects.None;
			SetSize (new Vector2 (15f, 5f));
			anchor = TextAnchor.MiddleLeft;
			cycleType = AC_CycleType.CustomScript;
			varID = 0;
			optionsArray = new List<string>();
			cycleText = "";
			
			base.Declare ();
		}


		public override MenuElement DuplicateSelf ()
		{
			MenuCycle newElement = CreateInstance <MenuCycle>();
			newElement.Declare ();
			newElement.CopyCycle (this);
			return newElement;
		}
		
		
		public void CopyCycle (MenuCycle _element)
		{
			uiButton = _element.uiButton;
			uiText = null;
			label = _element.label;
			textEffects = _element.textEffects;
			anchor = _element.anchor;
			selected = _element.selected;
			optionsArray = _element.optionsArray;
			cycleType = _element.cycleType;
			varID = _element.varID;
			cycleText = "";

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

			if (source != AC.MenuSource.AdventureCreator)
			{
				uiButton = LinkedUiGUI <UnityEngine.UI.Button> (uiButton, "Linked Button:", source);
				EditorGUILayout.EndVertical ();
				EditorGUILayout.BeginVertical ("Button");
			}

			label = EditorGUILayout.TextField ("Label text:", label);

			if (source == MenuSource.AdventureCreator)
			{
				anchor = (TextAnchor) EditorGUILayout.EnumPopup ("Text alignment:", anchor);
				textEffects = (TextEffects) EditorGUILayout.EnumPopup ("Text effect:", textEffects);
			}

			cycleType = (AC_CycleType) EditorGUILayout.EnumPopup ("Cycle type:", cycleType);
			if (cycleType == AC_CycleType.CustomScript || cycleType == AC_CycleType.Variable)
			{
				int numOptions = optionsArray.Count;
				numOptions = EditorGUILayout.IntField ("Number of choices:", optionsArray.Count);
				if (numOptions < 0)
				{
					numOptions = 0;
				}
				
				if (numOptions < optionsArray.Count)
				{
					optionsArray.RemoveRange (numOptions, optionsArray.Count - numOptions);
				}
				else if (numOptions > optionsArray.Count)
				{
					if(numOptions > optionsArray.Capacity)
					{
						optionsArray.Capacity = numOptions;
					}
					for (int i=optionsArray.Count; i<numOptions; i++)
					{
						optionsArray.Add ("");
					}
				}
				
				for (int i=0; i<optionsArray.Count; i++)
				{
					optionsArray [i] = EditorGUILayout.TextField ("Choice #" + i.ToString () + ":", optionsArray [i]);
				}
				
				if (cycleType == AC_CycleType.CustomScript)
				{
					if (optionsArray.Count > 0)
					{
						selected = EditorGUILayout.IntField ("Default option #:", selected);
					}
					ShowClipHelp ();
				}
				else if (cycleType == AC_CycleType.Variable)
				{
					varID = EditorGUILayout.IntField ("Global Variable ID:", varID);
				}
			}
			EditorGUILayout.EndVertical ();
			
			base.ShowGUI (source);
		}
		
		#endif
		
		
		public override void PreDisplay (int _slot, int languageNumber, bool isActive)
		{
			CalculateValue ();

			cycleText = TranslateLabel (label, languageNumber) + " : ";
			
			if (Application.isPlaying)
			{
				if (optionsArray.Count > selected && selected > -1)
				{
					cycleText += optionsArray [selected];
				}
				else
				{
					Debug.Log ("Could not gather options for MenuCycle " + label);
					selected = 0;
				}
			}
			else if (optionsArray.Count > 0)
			{
				if (selected >= 0 && selected < optionsArray.Count)
				{
					cycleText += optionsArray [selected];
				}
				else
				{
					cycleText += optionsArray [0];
				}
			}
			else
			{
				cycleText += "Default option";	
			}

			if (uiButton)
			{
				if (uiText)
				{
					uiText.text = cycleText;
				}
				UpdateUIElement (uiButton);
			}
		}
		
		
		public override void Display (GUIStyle _style, int _slot, float zoom, bool isActive)
		{
			base.Display (_style, _slot, zoom, isActive);
			
			_style.alignment = anchor;
			if (zoom < 1f)
			{
				_style.fontSize = (int) ((float) _style.fontSize * zoom);
			}

			if (textEffects != TextEffects.None)
			{
				AdvGame.DrawTextEffect (ZoomRect (relativeRect, zoom), cycleText, _style, Color.black, _style.normal.textColor, 2, textEffects);
			}
			else
			{
				GUI.Label (ZoomRect (relativeRect, zoom), cycleText, _style);
			}
		}
		
		
		public override string GetLabel (int slot, int languageNumber)
		{
			if (optionsArray.Count > selected && selected > -1)
			{
				return TranslateLabel (label, languageNumber) + " : " + optionsArray [selected];
			}
			
			return TranslateLabel (label, languageNumber);
		}
		
		
		public override void ProcessClick (AC.Menu _menu, int _slot, MouseState _mouseState)
		{
			if (KickStarter.stateHandler.gameState == GameState.Cutscene)
			{
				return;
			}

			selected ++;
			if (selected > optionsArray.Count-1)
			{
				selected = 0;
			}
			
			if (cycleType == AC_CycleType.Language)
			{
				if (selected == 0 && KickStarter.speechManager.ignoreOriginalText && KickStarter.speechManager.languages.Count > 1)
				{
					// Ignore original text by skipping to first language
					selected = 1;
				}
				Options.SetLanguage (selected);
			}
			else if (cycleType == AC_CycleType.Variable)
			{
				if (varID >= 0)
				{
					GVar var = RuntimeVariables.GetVariable (varID);
					if (var.type == VariableType.Integer)
					{
						var.val = selected;
						var.Upload ();
					}
				}
			}
			
			if (cycleType == AC_CycleType.CustomScript)
			{
				MenuSystem.OnElementClick (_menu, this, _slot, (int) _mouseState);
			}
		}


		private void CalculateValue ()
		{
			if (!Application.isPlaying)
			{
				return;
			}

			if (cycleType == AC_CycleType.Language)
			{
				optionsArray = KickStarter.speechManager.languages;
				if (KickStarter.options.optionsData != null)
				{
					selected = KickStarter.options.optionsData.language;
				}
			}
			else if (cycleType == AC_CycleType.Variable)
			{
				if (varID >= 0)
				{
					if (RuntimeVariables.GetVariable (varID) == null || RuntimeVariables.GetVariable (varID).type != VariableType.Integer)
					{
						Debug.LogWarning ("Cannot link MenuToggle " + title + " to Variable " + varID + " as it is not an Integer.");
					}
					else if (optionsArray.Count > 0)
					{
						selected = Mathf.Clamp (RuntimeVariables.GetIntegerValue (varID), 0, optionsArray.Count - 1);
					}
					else
					{
						selected = 0;
					}
				}
			}
		}


		protected override void AutoSize ()
		{
			AutoSize (new GUIContent (TranslateLabel (label, Options.GetLanguage ()) + " : Default option"));
		}
		
	}
	
}