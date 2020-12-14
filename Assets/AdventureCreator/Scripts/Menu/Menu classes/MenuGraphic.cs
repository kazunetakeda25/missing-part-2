/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"MenuGraphic.cs"
 * 
 *	This MenuElement provides a space for
 *	animated and static textures
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
	
	public class MenuGraphic : MenuElement
	{
		
		public Image uiImage;
		
		public AC_GraphicType graphicType = AC_GraphicType.Normal;
		public CursorIconBase graphic;
		
		private Sprite sprite;
		private Speech speech;
		private bool speechIsAnimating;
		private Texture2D speechTex;
		private Rect speechRect;
		private bool isDuppingSpeech;
		
		
		public override void Declare ()
		{
			uiImage = null;
			
			graphicType = AC_GraphicType.Normal;
			isVisible = true;
			isClickable = false;
			graphic = new CursorIconBase ();
			numSlots = 1;
			SetSize (new Vector2 (10f, 5f));
			
			base.Declare ();
		}
		
		
		public override MenuElement DuplicateSelf ()
		{
			MenuGraphic newElement = CreateInstance <MenuGraphic>();
			newElement.Declare ();
			newElement.CopyGraphic (this);
			return newElement;
		}
		
		
		public void CopyGraphic (MenuGraphic _element)
		{
			uiImage = _element.uiImage;
			
			graphicType = _element.graphicType;
			graphic = _element.graphic;
			base.Copy (_element);
		}
		
		
		public override void LoadUnityUI (AC.Menu _menu)
		{
			uiImage = LinkUIElement <Image>();
		}
		
		
		public override RectTransform GetRectTransform (int _slot)
		{
			if (uiImage)
			{
				return uiImage.rectTransform;
			}
			return null;
		}
		
		
		#if UNITY_EDITOR
		
		public override void ShowGUI (MenuSource source)
		{
			EditorGUILayout.BeginVertical ("Button");
			
			if (source != MenuSource.AdventureCreator)
			{
				uiImage = LinkedUiGUI <Image> (uiImage, "Linked Image:", source);
				EditorGUILayout.EndVertical ();
				EditorGUILayout.BeginVertical ("Button");
			}
			
			graphicType = (AC_GraphicType) EditorGUILayout.EnumPopup ("Graphic type:", graphicType);
			if (graphicType == AC_GraphicType.Normal)
			{
				graphic.ShowGUI (false);
			}
			EditorGUILayout.EndVertical ();
			
			base.ShowGUI (source);
		}
		
		#endif


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
		
		
		public override void ClearSpeech ()
		{
			if (graphicType == AC_GraphicType.DialoguePortrait)
			{
				speechTex = null;
			}
		}


		public override void PreDisplay (int _slot, int languageNumber, bool isActive)
		{
			if (graphicType == AC_GraphicType.DialoguePortrait)
			{
				UpdateSpeechLink ();
				if (speech != null)
				{
					speechTex = speech.GetPortrait ();
					speechIsAnimating = speech.IsAnimating ();
				}
			}
			
			if (uiImage != null)
			{
				if (graphicType == AC_GraphicType.Normal)
				{
					uiImage.sprite = graphic.GetAnimatedSprite (true);
				}
				else if (speech != null)
				{
					uiImage.sprite = speech.GetPortraitSprite ();
				}
				UpdateUIElement (uiImage);
			}
		}
		
		
		public override void Display (GUIStyle _style, int _slot, float zoom, bool isActive)
		{
			base.Display (_style, _slot, zoom, isActive);
			
			if (graphicType == AC_GraphicType.Normal)
			{
				if (graphic != null)
				{
					graphic.DrawAsInteraction (ZoomRect (relativeRect, zoom), true);
				}
			}
			else
			{
				if (speechTex != null)
				{
					if (speechIsAnimating)
					{
						if (speech != null)
						{
							speechRect = speech.GetAnimatedRect ();
						}
						GUI.DrawTextureWithTexCoords (ZoomRect (relativeRect, zoom), speechTex, speechRect);
					}
					else
					{
						GUI.DrawTexture (ZoomRect (relativeRect, zoom), speechTex, ScaleMode.StretchToFill, true, 0f);
					}
				}
			}
		}
		
		
		public override void RecalculateSize (MenuSource source)
		{
			graphic.Reset ();
			base.RecalculateSize (source);
		}
		
		
		protected override void AutoSize ()
		{
			if (graphicType == AC_GraphicType.Normal && graphic.texture != null)
			{
				GUIContent content = new GUIContent (graphic.texture);
				AutoSize (content);
			}
		}
		
	}
	
}