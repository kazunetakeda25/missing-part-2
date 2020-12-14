/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"MenuToggle.cs"
 * 
 *	This MenuElement toggles between On and Off when clicked on.
 *	It can be used for changing boolean options.
 * 
 */

using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{
	
	public class MenuToggle : MenuElement
	{

		public Toggle uiToggle;
		private Text uiText;

		public string label;
		public bool isOn;
		public TextEffects textEffects;
		public TextAnchor anchor;
		public int varID;
		public AC_ToggleType toggleType;
		public bool appendState = true;
		public Texture2D onTexture = null;
		public Texture2D offTexture = null;

		private string fullText;


		public override void Declare ()
		{
			uiToggle = null;
			uiText = null;
			label = "Toggle";
			isOn = false;
			isVisible = true;
			isClickable = true;
			toggleType = AC_ToggleType.CustomScript;
			numSlots = 1;
			varID = 0;
			SetSize (new Vector2 (15f, 5f));
			anchor = TextAnchor.MiddleLeft;
			appendState = true;
			onTexture = null;
			offTexture = null;
			textEffects = TextEffects.None;
			
			base.Declare ();
		}


		public override MenuElement DuplicateSelf ()
		{
			MenuToggle newElement = CreateInstance <MenuToggle>();
			newElement.Declare ();
			newElement.CopyToggle (this);
			return newElement;
		}
		
		
		public void CopyToggle (MenuToggle _element)
		{
			uiToggle = _element.uiToggle;
			uiText = null;
			label = _element.label;
			isOn = _element.isOn;
			textEffects = _element.textEffects;
			anchor = _element.anchor;
			toggleType = _element.toggleType;
			varID = _element.varID;
			appendState = _element.appendState;
			onTexture = _element.onTexture;
			offTexture = _element.offTexture;
			
			base.Copy (_element);
		}


		public override void LoadUnityUI (AC.Menu _menu)
		{
			uiToggle = LinkUIElement <Toggle>();

			if (uiToggle)
			{
				if (uiToggle.GetComponentInChildren <Text>())
				{
					uiText = uiToggle.GetComponentInChildren <Text>();
				}
				uiToggle.onValueChanged.AddListener ((isOn) => {
					ProcessClick (_menu, 0, KickStarter.playerInput.mouseState);
				});
			}
		}


		public override GameObject GetObjectToSelect ()
		{
			if (uiToggle)
			{
				return uiToggle.gameObject;
			}
			return null;
		}
		
		
		public override RectTransform GetRectTransform (int _slot)
		{
			if (uiToggle)
			{
				return uiToggle.GetComponent <RectTransform>();
			}
			return null;
		}
		
		
		#if UNITY_EDITOR
		
		public override void ShowGUI (MenuSource source)
		{
			EditorGUILayout.BeginVertical ("Button");

			if (source != MenuSource.AdventureCreator)
			{
				uiToggle = LinkedUiGUI <Toggle> (uiToggle, "Linked Toggle:", source);
				EditorGUILayout.EndVertical ();
				EditorGUILayout.BeginVertical ("Button");
			}

			label = EditorGUILayout.TextField ("Label text:", label);
			appendState = EditorGUILayout.Toggle ("Append state to label?", appendState);

			if (source == MenuSource.AdventureCreator)
			{
				anchor = (TextAnchor) EditorGUILayout.EnumPopup ("Text alignment:", anchor);
				textEffects = (TextEffects) EditorGUILayout.EnumPopup ("Text effect:", textEffects);
			
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("'On' texture:", GUILayout.Width (145f));
				onTexture = (Texture2D) EditorGUILayout.ObjectField (onTexture, typeof (Texture2D), false, GUILayout.Width (70f), GUILayout.Height (30f));
				EditorGUILayout.EndHorizontal ();
				
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("'Off' texture:", GUILayout.Width (145f));
				offTexture = (Texture2D) EditorGUILayout.ObjectField (offTexture, typeof (Texture2D), false, GUILayout.Width (70f), GUILayout.Height (30f));
				EditorGUILayout.EndHorizontal ();
			}

			toggleType = (AC_ToggleType) EditorGUILayout.EnumPopup ("Toggle type:", toggleType);
			if (toggleType == AC_ToggleType.CustomScript)
			{
				isOn = EditorGUILayout.Toggle ("On by default?", isOn);
				ShowClipHelp ();
			}
			else if (toggleType == AC_ToggleType.Variable)
			{
				varID = AdvGame.GlobalVariableGUI ("Boolean variable:", varID);
				if (varID >= 0 && AdvGame.GetReferences () && AdvGame.GetReferences ().variablesManager)
				{
					GVar _var = AdvGame.GetReferences ().variablesManager.GetVariable (varID);
					if (_var != null && _var.type != VariableType.Boolean)
					{
						EditorGUILayout.HelpBox ("The chosen Variable must be a Boolean.", MessageType.Warning);
					}
				}
			}
			EditorGUILayout.EndVertical ();
			
			base.ShowGUI (source);
		}
		
		#endif
		
		
		public override void PreDisplay (int _slot, int languageNumber, bool isActive)
		{
			CalculateValue ();

			fullText = TranslateLabel (label, languageNumber);
			if (appendState)
			{
				if (isOn)
				{
					fullText += " : On";
				}
				else
				{
					fullText += " : Off";
				}
			}

			if (uiToggle)
			{
				if (uiText)
				{
					uiText.text = fullText;
				}
				uiToggle.isOn = isOn;
				UpdateUIElement (uiToggle);
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
			
			Rect rect = ZoomRect (relativeRect, zoom);
			if (isOn && onTexture != null)
			{
				GUI.DrawTexture (rect, onTexture, ScaleMode.StretchToFill, true, 0f);
			}
			else if (!isOn && offTexture != null)
			{
				GUI.DrawTexture (rect, offTexture, ScaleMode.StretchToFill, true, 0f);
			}
			
			if (textEffects != TextEffects.None)
			{
				AdvGame.DrawTextEffect (rect, fullText, _style, Color.black, _style.normal.textColor, 2, textEffects);
			}
			else
			{
				GUI.Label (rect, fullText, _style);
			}
		}
		
		
		public override string GetLabel (int slot, int languageNumber)
		{
			if (isOn)
			{
				return TranslateLabel (label, languageNumber) + " : " + "On";
			}
			
			return TranslateLabel (label, languageNumber) + " : " + "Off";
		}
		

		public override void ProcessClick (AC.Menu _menu, int _slot, MouseState _mouseState)
		{
			if (KickStarter.stateHandler.gameState == GameState.Cutscene)
			{
				return;
			}

			if (uiToggle != null)
			{
				isOn = uiToggle.isOn;
			}
			else
			{
				if (isOn)
				{
					isOn = false;
				}
				else
				{
					isOn = true;
				}
			}

			if (toggleType == AC_ToggleType.Subtitles)
			{
				KickStarter.options.optionsData.showSubtitles = isOn;
				KickStarter.options.SavePrefs ();
			}
			else if (toggleType == AC_ToggleType.Variable)
			{
				if (varID >= 0)
				{
					GVar var = RuntimeVariables.GetVariable (varID);
					if (var.type == VariableType.Boolean)
					{
						if (isOn)
						{
							var.val = 1;
						}
						else
						{
							var.val = 0;
						}
						var.Upload ();
					}
				}
			}
			
			if (toggleType == AC_ToggleType.CustomScript)
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

			if (toggleType == AC_ToggleType.Subtitles)
			{	
				if (KickStarter.options != null && KickStarter.options.optionsData != null)
				{
					isOn = KickStarter.options.optionsData.showSubtitles;
				}
			}
			else if (toggleType == AC_ToggleType.Variable)
			{
				if (varID >= 0)
				{
					if (RuntimeVariables.GetVariable (varID).type != VariableType.Boolean)
					{
						Debug.LogWarning ("Cannot link MenuToggle " + title + " to Variable " + varID + " as it is not a Boolean.");
					}
					else
					{
						isOn = RuntimeVariables.GetBooleanValue (varID);;
					}
				}
			}
		}

		
		protected override void AutoSize ()
		{
			int languageNumber = Options.GetLanguage ();
			if (appendState)
			{
				AutoSize (new GUIContent (TranslateLabel (label, languageNumber) + " : Off"));
			}
			else
			{
				AutoSize (new GUIContent (TranslateLabel (label, languageNumber)));
			}
		}
		
	}
	
}