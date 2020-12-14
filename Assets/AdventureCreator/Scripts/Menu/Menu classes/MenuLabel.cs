/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"MenuLabel.cs"
 * 
 *	This MenuElement provides a basic label.
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

	public class MenuLabel : MenuElement
	{

		public Text uiText;

		public string label = "Element";
		public TextAnchor anchor;
		public TextEffects textEffects = TextEffects.None;
		public AC_LabelType labelType;

		public int variableID;
		public int variableNumber;
		public bool useCharacterColour = false;
		public bool autoAdjustHeight = true;
		public bool updateIfEmpty = false;

		private string newLabel = "";
		private Speech speech;
		private Color speechColour;
		private bool isDuppingSpeech;

		#if UNITY_EDITOR
		private VariablesManager variablesManager;
		#endif


		public override void Declare ()
		{
			uiText = null;

			label = "Label";
			isVisible = true;
			isClickable = false;
			numSlots = 1;
			anchor = TextAnchor.MiddleCenter;
			SetSize (new Vector2 (10f, 5f));
			labelType = AC_LabelType.Normal;
			variableID = 0;
			variableNumber = 0;
			useCharacterColour = false;
			autoAdjustHeight = true;
			textEffects = TextEffects.None;
			newLabel = "";
			updateIfEmpty = false;

			base.Declare ();
		}


		public override MenuElement DuplicateSelf ()
		{
			MenuLabel newElement = CreateInstance <MenuLabel>();
			newElement.Declare ();
			newElement.CopyLabel (this);
			return newElement;
		}
		
		
		public void CopyLabel (MenuLabel _element)
		{
			uiText = _element.uiText;
			label = _element.label;
			anchor = _element.anchor;
			textEffects = _element.textEffects;
			labelType = _element.labelType;
			variableID = _element.variableID;
			variableNumber = _element.variableNumber;
			useCharacterColour = _element.useCharacterColour;
			autoAdjustHeight = _element.autoAdjustHeight;
			updateIfEmpty = _element.updateIfEmpty;
			newLabel = "";

			base.Copy (_element);
		}


		public override void LoadUnityUI (AC.Menu _menu)
		{
			uiText = LinkUIElement <Text>();
		}


		public override RectTransform GetRectTransform (int _slot)
		{
			if (uiText)
			{
				return uiText.rectTransform;
			}
			return null;
		}
		
		
		#if UNITY_EDITOR
		
		public override void ShowGUI (MenuSource source)
		{
			EditorGUILayout.BeginVertical ("Button");

			if (source != MenuSource.AdventureCreator)
			{
				uiText = LinkedUiGUI <Text> (uiText, "Linked Text:", source);
				EditorGUILayout.EndVertical ();
				EditorGUILayout.BeginVertical ("Button");
			}

			labelType = (AC_LabelType) EditorGUILayout.EnumPopup ("Label type:", labelType);
			if (source == MenuSource.AdventureCreator || labelType == AC_LabelType.Normal)
			{
				label = EditorGUILayout.TextField ("Label text:", label);
			}

			if (labelType == AC_LabelType.GlobalVariable)
			{
				variableID = AdvGame.GlobalVariableGUI ("Global Variable:", variableID);
			}
			else if (labelType == AC_LabelType.DialogueLine)
			{
				useCharacterColour = EditorGUILayout.Toggle ("Use Character text colour?", useCharacterColour);
				if (sizeType == AC_SizeType.Manual)
				{
					autoAdjustHeight = EditorGUILayout.Toggle ("Auto-adjust height to fit?", autoAdjustHeight);
				}
			}

			if (labelType == AC_LabelType.Hotspot || labelType == AC_LabelType.DialogueLine || labelType == AC_LabelType.DialogueSpeaker)
			{
				updateIfEmpty = EditorGUILayout.Toggle ("Update if string is empty?", updateIfEmpty);
			}

			if (source == MenuSource.AdventureCreator)
			{
				anchor = (TextAnchor) EditorGUILayout.EnumPopup ("Text alignment:", anchor);
				textEffects = (TextEffects) EditorGUILayout.EnumPopup ("Text effect:", textEffects);
			}
			EditorGUILayout.EndVertical ();

			base.ShowGUI (source);
		}

		#endif


		public override void ClearSpeech ()
		{
			if (labelType == AC_LabelType.DialogueLine || labelType == AC_LabelType.DialogueSpeaker)
			{
				newLabel = "";
			}
		}


		public override void PreDisplay (int _slot, int languageNumber, bool isActive)
		{
			if (Application.isPlaying)
			{
				if (labelType == AC_LabelType.Hotspot)
				{
					if (KickStarter.playerMenus.GetHotspotLabel () != "" || updateIfEmpty)
					{
						newLabel = KickStarter.playerMenus.GetHotspotLabel ();
					}
				}
				else if (labelType == AC_LabelType.Normal)
				{
					newLabel = TranslateLabel (label, languageNumber);
				}
				else if (labelType == AC_LabelType.GlobalVariable)
				{
					newLabel = RuntimeVariables.GetVariable (variableID).GetValue ();
				}
				else
				{
					UpdateSpeechLink ();
				
					if (labelType == AC_LabelType.DialogueLine)
					{
						if (speech != null)
						{
							string line = speech.displayText;
							if (line != "" || updateIfEmpty)
							{
								newLabel = line;
							}

							if (useCharacterColour)
							{
								speechColour = speech.GetColour ();
								if (uiText)
								{
									uiText.color = speechColour;
								}
							}
						}
						else if (!KickStarter.speechManager.keepTextInBuffer)
						{
							newLabel = "";
						}
					}
					else if (labelType == AC_LabelType.DialogueSpeaker)
					{
						if (speech != null)
						{
							string line = speech.GetSpeaker ();

							if (line != "" || updateIfEmpty)
							{
								newLabel = line;
							}
						}
						else if (!KickStarter.speechManager.keepTextInBuffer)
						{
							newLabel = "";
						}
					}
				}
			}
			else
			{
				newLabel = label;
			}
			
			newLabel = AdvGame.ConvertTokens (newLabel);

			if (uiText != null)
			{
				uiText.text = newLabel;
				UpdateUIElement (uiText);
			}
		}

		
		public override void Display (GUIStyle _style, int _slot, float zoom, bool isActive)
		{
			base.Display (_style, _slot, zoom, isActive);
			
			_style.wordWrap = true;
			_style.alignment = anchor;
			if (zoom < 1f)
			{
				_style.fontSize = (int) ((float) _style.fontSize * zoom);
			}

			if (Application.isPlaying)
			{
				if (labelType == AC_LabelType.DialogueLine)
				{
					if (useCharacterColour)
					{
						_style.normal.textColor = speechColour;
					}

					if (newLabel != "" || updateIfEmpty)
					{
						if (sizeType == AC_SizeType.Manual && autoAdjustHeight)
						{
							GUIContent content = new GUIContent (newLabel);
							relativeRect.height = _style.CalcHeight (content, relativeRect.width);
						}
					}
				}
			}

			if (textEffects != TextEffects.None)
			{
				AdvGame.DrawTextEffect (ZoomRect (relativeRect, zoom), newLabel, _style, Color.black, _style.normal.textColor, 2, textEffects);
			}
			else
			{
				GUI.Label (ZoomRect (relativeRect, zoom), newLabel, _style);
			}
		}


		public override string GetLabel (int slot, int languageNumber)
		{
			if (labelType == AC_LabelType.Normal)
			{
				return TranslateLabel (label, languageNumber);
			}
			else if (labelType == AC_LabelType.DialogueSpeaker)
			{
				return KickStarter.dialog.GetSpeaker ();
			}
			else if (labelType == AC_LabelType.GlobalVariable)
			{
				return RuntimeVariables.GetVariable (variableID).GetValue ();
			}
			else if (labelType == AC_LabelType.Hotspot)
			{
				return KickStarter.playerMenus.GetHotspotLabel ();
			}

			return "";
		}


		private void UpdateSpeechLink ()
		{
			if (!isDuppingSpeech && KickStarter.dialog.GetLatestSpeech () != null)
			{
				speech = KickStarter.dialog.GetLatestSpeech ();
			}
		}


		public override void SetSpeech (Speech _speech)
		{
			isDuppingSpeech = true;
			speech = _speech;
		}


		protected override void AutoSize ()
		{
			int languageNumber = Options.GetLanguage ();

			if (labelType == AC_LabelType.DialogueLine)
			{
				GUIContent content = new GUIContent (TranslateLabel (label, languageNumber));

				#if UNITY_EDITOR
				if (!Application.isPlaying)
				{
					AutoSize (content);
					return;
				}
				#endif

				GUIStyle normalStyle = new GUIStyle();
				normalStyle.font = font;
				normalStyle.fontSize = (int) (AdvGame.GetMainGameViewSize ().x * fontScaleFactor / 100);

				UpdateSpeechLink ();
				if (speech != null)
				{
					string line = " " + speech.log.fullText + " ";
					if (line.Length > 40)
					{
						line = line.Insert (line.Length / 2, " \n ");
					}
					content = new GUIContent (line);
					AutoSize (content);
				}
			}
			else if (label == "" && backgroundTexture != null)
			{
				GUIContent content = new GUIContent (backgroundTexture);
				AutoSize (content);
			}
			else
			{
				GUIContent content = new GUIContent (TranslateLabel (label, languageNumber));
				AutoSize (content);
			}
		}
		
	}

}