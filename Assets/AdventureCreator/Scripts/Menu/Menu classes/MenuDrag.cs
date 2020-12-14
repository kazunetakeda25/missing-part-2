/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"MenuDrag.cs"
 * 
 *	This MenuElement can be used to drag around it's parent Menu.
 * 
 */

using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;	
#endif

namespace AC
{
	
	[System.Serializable]
	public class MenuDrag : MenuElement
	{
		
		public string label = "Element";
		public TextAnchor anchor;
		public TextEffects textEffects;
		public Rect dragRect;
		public DragElementType dragType = DragElementType.EntireMenu;
		public string elementName;

		private Vector2 dragStartPosition;
		private AC.Menu menuToDrag;
		private MenuElement elementToDrag;
		private string fullText;


		public override void Declare ()
		{
			label = "Button";
			isVisible = true;
			isClickable = true;
			textEffects = TextEffects.None;
			numSlots = 1;
			anchor = TextAnchor.MiddleCenter;
			SetSize (new Vector2 (10f, 5f));
			dragRect = new Rect (0,0,0,0);
			dragType = DragElementType.EntireMenu;
			elementName = "";

			base.Declare ();
		}


		public override MenuElement DuplicateSelf ()
		{
			MenuDrag newElement = CreateInstance <MenuDrag>();
			newElement.Declare ();
			newElement.CopyDrag (this);
			return newElement;
		}
		
		
		public void CopyDrag (MenuDrag _element)
		{
			label = _element.label;
			anchor = _element.anchor;
			textEffects = _element.textEffects;
			dragRect = _element.dragRect;
			dragType = _element.dragType;
			elementName = _element.elementName;

			base.Copy (_element);
		}
		
		
		#if UNITY_EDITOR
		
		public override void ShowGUI (MenuSource source)
		{
			if (source != MenuSource.AdventureCreator)
			{
				EditorGUILayout.HelpBox ("This Element type is not necessary in Unity's UI, as it can be recreated using ScrollBars and ScrollRects.", MessageType.Info);
				return;
			}

			EditorGUILayout.BeginVertical ("Button");
			label = EditorGUILayout.TextField ("Button text:", label);
			anchor = (TextAnchor) EditorGUILayout.EnumPopup ("Text alignment:", anchor);
			textEffects = (TextEffects) EditorGUILayout.EnumPopup ("Text effect:", textEffects);

			dragType = (DragElementType) EditorGUILayout.EnumPopup ("Drag type:", dragType);
			if (dragType == DragElementType.SingleElement)
			{
				elementName = EditorGUILayout.TextField ("Element name:", elementName);
			}

			dragRect = EditorGUILayout.RectField ("Drag boundary:", dragRect);
			EditorGUILayout.EndVertical ();
			
			base.ShowGUI (source);
		}


		public override void DrawOutline (bool isSelected, AC.Menu _menu)
		{
			if (dragType == DragElementType.EntireMenu)
			{
				DrawStraightLine.DrawBox (_menu.GetRectAbsolute (GetDragRectRelative ()), Color.white, 1f, false, 1);
			}
			else
			{
				if (elementName != "")
				{
					MenuElement element = MenuManager.GetElementWithName (_menu.title, elementName);
					if (element != null)
					{
						Rect dragBox = _menu.GetRectAbsolute (GetDragRectRelative ());
						dragBox.x += element.GetSlotRectRelative (0).x;
						dragBox.y += element.GetSlotRectRelative (0).y;
						DrawStraightLine.DrawBox (dragBox, Color.white, 1f, false, 1);
					}
				}
			}
			
			base.DrawOutline (isSelected, _menu);
		}

		#endif


		public override void PreDisplay (int _slot, int languageNumber, bool isActive)
		{
			fullText = TranslateLabel (label, languageNumber);
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


		public void StartDrag (AC.Menu _menu)
		{
			menuToDrag = _menu;

			if (dragType == DragElementType.SingleElement)
			{
				if (elementName != "")
				{
					MenuElement element = PlayerMenus.GetElementWithName (_menu.title, elementName);
					if (element == null)
					{
						Debug.LogWarning ("Cannot drag " + elementName + " as it cannot be found on " + _menu.title);
					}
					else if (element.positionType == AC_PositionType2.Aligned)
					{
						Debug.LogWarning ("Cannot drag " + elementName + " as it's Position is set to Aligned");
					}
					else if (_menu.sizeType == AC_SizeType.Automatic)
					{
						Debug.LogWarning ("Cannot drag " + elementName + " as it's parent Menu's Size is set to Automatic");
					}
					else
					{
						elementToDrag = element;
						dragStartPosition = elementToDrag.GetDragStart ();
					}
				}
			}
			else
			{
				dragStartPosition = _menu.GetDragStart ();
			}
		}


		public bool DoDrag (Vector2 _dragVector)
		{
			if (dragType == DragElementType.EntireMenu)
			{
				if (menuToDrag == null)
				{
					return false;
				}
				
				if (!menuToDrag.IsEnabled () || menuToDrag.IsFading ())
				{
					return false;
				}
			}
			
			if (elementToDrag == null && dragType == DragElementType.SingleElement)
			{
				return false;
			}
			
			// Convert dragRect to screen co-ordinates
			Rect dragRectAbsolute = dragRect;
			if (sizeType != AC_SizeType.AbsolutePixels)
			{
				dragRectAbsolute = new Rect (dragRect.x * AdvGame.GetMainGameViewSize ().x / 100f,
				                             dragRect.y * AdvGame.GetMainGameViewSize ().y / 100f,
				                             dragRect.width * AdvGame.GetMainGameViewSize ().x / 100f,
				                             dragRect.height * AdvGame.GetMainGameViewSize ().y / 100f);
			}
			
			if (dragType == DragElementType.EntireMenu)
			{
				menuToDrag.SetDragOffset (_dragVector + dragStartPosition, dragRectAbsolute);
			}
			else if (dragType == AC.DragElementType.SingleElement)
			{
				elementToDrag.SetDragOffset (_dragVector + dragStartPosition, dragRectAbsolute);
			}
			
			return true;
		}


		public bool CheckStop (Vector2 mousePosition)
		{
			if (menuToDrag == null)
			{
				return false;
			}
			if (dragType == DragElementType.EntireMenu && !menuToDrag.IsPointerOverSlot (this, 0, mousePosition))
			{
				return true;
			}
			if (dragType == DragElementType.SingleElement && elementToDrag != null && !menuToDrag.IsPointerOverSlot (this, 0, mousePosition))
			{
				return true;
			}
			return false;
		}


		private Rect GetDragRectRelative ()
		{
			Rect positionRect = dragRect;

			if (sizeType != AC_SizeType.AbsolutePixels)
			{
				positionRect.x = dragRect.x / 100f * AdvGame.GetMainGameViewSize ().x;
				positionRect.y = dragRect.y / 100f * AdvGame.GetMainGameViewSize ().y;

				positionRect.width = dragRect.width / 100f * AdvGame.GetMainGameViewSize ().x;
				positionRect.height = dragRect.height / 100f * AdvGame.GetMainGameViewSize ().y;
			}

			return (positionRect);
		}


		public override void ProcessClick (AC.Menu _menu, int _slot, MouseState _mouseState)
		{
			if (_mouseState == MouseState.SingleClick)
			{
				StartDrag (_menu);
				KickStarter.playerInput.activeDragElement = this;
			}
		}
		
	}
	
}