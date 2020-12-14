/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"MenuElement.cs"
 * 
 *	This is the base class for all menu elements.  It should never
 *	be added itself to a menu, as it is only a container of shared data.
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
	public class MenuElement : ScriptableObject
	{
		public int linkedUiID;

		public int ID;
		public bool isEditing = false;
		public string title = "Element";
		public Vector2 slotSize;
		public AC_SizeType sizeType;
		public AC_PositionType2 positionType;
		public float slotSpacing = 0f;
		public int lineID = -1;

		public Font font;
		public float fontScaleFactor = 60f;
		public Color fontColor = Color.white;
		public Color fontHighlightColor = Color.white;
		public Texture2D highlightTexture;
		
		public bool isVisible;
		public bool isClickable;
		public ElementOrientation orientation = ElementOrientation.Vertical;
		public int gridWidth = 3;

		public Texture2D backgroundTexture;

		public AudioClip hoverSound;
		public AudioClip clickSound;

		protected int offset = 0;
		private Vector2 dragOffset;

		[SerializeField] protected Rect relativeRect;
		[SerializeField] protected Vector2 relativePosition;
		[SerializeField] protected int numSlots;
		

		public virtual void Declare ()
		{
			linkedUiID = 0;
			fontScaleFactor = 2f;
			fontColor = Color.white;
			fontHighlightColor = Color.white;
			highlightTexture = null;
			orientation = ElementOrientation.Vertical;
			positionType = AC_PositionType2.Aligned;
			sizeType = AC_SizeType.Automatic;
			gridWidth = 3;
			lineID = -1;
			hoverSound = null;
			clickSound = null;
			dragOffset = Vector2.zero;
		}


		public virtual MenuElement DuplicateSelf ()
		{
			return null;
		}

		
		public virtual void Copy (MenuElement _element)
		{
			linkedUiID = _element.linkedUiID;
			ID = _element.ID;
			isEditing = false;
			title = _element.title;
			slotSize = _element.slotSize;
			sizeType = _element.sizeType;
			positionType = _element.positionType;
			relativeRect = _element.relativeRect;
			numSlots = _element.numSlots;
			lineID = _element.lineID;
			slotSpacing = _element.slotSpacing;
		
			font = _element.font;
			fontScaleFactor = _element.fontScaleFactor;
			fontColor = _element.fontColor;
			fontHighlightColor = _element.fontHighlightColor;
			highlightTexture = _element.highlightTexture;

			isVisible = _element.isVisible;
			isClickable = _element.isClickable;
			orientation = _element.orientation;
			gridWidth = _element.gridWidth;

			backgroundTexture = _element.backgroundTexture;

			hoverSound = _element.hoverSound;
			clickSound = _element.clickSound;

			relativePosition = _element.relativePosition;
			dragOffset = Vector2.zero;
		}


		public virtual void LoadUnityUI (AC.Menu _menu)
		{}


		public virtual void ProcessClick (AC.Menu _menu, int _slot, MouseState _mouseState)
		{}


		public virtual void ProcessContinuousClick (AC.Menu _menu, MouseState _mouseState)
		{}


		public virtual GameObject GetObjectToSelect ()
		{
			return null;
		}


		public virtual RectTransform GetRectTransform (int _slot)
		{
			return null;
		}


		public virtual void ClearSpeech ()
		{}


		public virtual void SetSpeech (Speech _speech)
		{}


		public void UpdateID (int[] idArray)
		{
			foreach (int _id in idArray)
			{
				if (ID == _id)
				{
					ID ++;
				}
			}
		}


		protected string TranslateLabel (string label, int languageNumber)
		{
			return (SpeechManager.GetTranslation (label, lineID, languageNumber));
		}


		public virtual string GetLabel (int slot, int languageNumber)
		{
			return "";
		}

		
		#if UNITY_EDITOR
		
		public void ShowGUIStart (MenuSource source)
		{
			EditorGUILayout.BeginVertical ("Button");
				title = EditorGUILayout.TextField ("Element name:", title);
				isVisible = EditorGUILayout.Toggle ("Is visible?", isVisible);
			EditorGUILayout.EndVertical ();

			ShowGUI (source);
		}
		
		
		public virtual void ShowGUI (MenuSource source)
		{
			if (source != MenuSource.AdventureCreator)
			{
				if (isClickable)
				{
					EditorGUILayout.BeginVertical ("Button");
					hoverSound = (AudioClip) EditorGUILayout.ObjectField ("Hover sound:", hoverSound, typeof (AudioClip), false);
					clickSound = (AudioClip) EditorGUILayout.ObjectField ("Click sound:", clickSound, typeof (AudioClip), false);
					EditorGUILayout.EndVertical ();
				}
				return;
			}

			if (!(this is MenuGraphic))
			{
				EditorGUILayout.BeginVertical ("Button");
					font = (Font) EditorGUILayout.ObjectField ("Font:", font, typeof (Font), false);
					fontScaleFactor = EditorGUILayout.Slider ("Text size:", fontScaleFactor, 1f, 4f);
					fontColor = EditorGUILayout.ColorField ("Text colour:", fontColor);
					if (isClickable)
					{
						fontHighlightColor = EditorGUILayout.ColorField ("Text colour (highlighted):", fontHighlightColor);
					}
				EditorGUILayout.EndVertical ();
			}

			EditorGUILayout.BeginVertical ("Button");
				if (isClickable)
				{
					EditorGUILayout.BeginHorizontal ();
						EditorGUILayout.LabelField ("Highlight texture:", GUILayout.Width (145f));
						highlightTexture = (Texture2D) EditorGUILayout.ObjectField (highlightTexture, typeof (Texture2D), false, GUILayout.Width (70f), GUILayout.Height (30f));
					EditorGUILayout.EndHorizontal ();

					hoverSound = (AudioClip) EditorGUILayout.ObjectField ("Hover sound:", hoverSound, typeof (AudioClip), false);
					clickSound = (AudioClip) EditorGUILayout.ObjectField ("Click sound:", clickSound, typeof (AudioClip), false);
				}
				
				EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("Background texture:", GUILayout.Width (145f));
					backgroundTexture = (Texture2D) EditorGUILayout.ObjectField (backgroundTexture, typeof (Texture2D), false, GUILayout.Width (70f), GUILayout.Height (30f));
				EditorGUILayout.EndHorizontal ();
			EditorGUILayout.EndVertical ();
			
			EndGUI ();
		}
		
		
		public void EndGUI ()
		{
			EditorGUILayout.BeginVertical ("Button");
				positionType = (AC_PositionType2) EditorGUILayout.EnumPopup ("Position:", positionType);
				if (positionType == AC_PositionType2.AbsolutePixels)
				{
					EditorGUILayout.BeginHorizontal ();
						EditorGUILayout.LabelField ("X:", GUILayout.Width (15f));
						relativeRect.x = EditorGUILayout.FloatField (relativeRect.x);
						EditorGUILayout.LabelField ("Y:", GUILayout.Width (15f));
						relativeRect.y = EditorGUILayout.FloatField (relativeRect.y);
					EditorGUILayout.EndHorizontal ();
				}
				else if (positionType == AC_PositionType2.RelativeToMenuSize)
				{
					EditorGUILayout.BeginHorizontal ();
						EditorGUILayout.LabelField ("X:", GUILayout.Width (15f));
						relativePosition.x = EditorGUILayout.Slider (relativePosition.x, 0f, 100f);
						EditorGUILayout.LabelField ("Y:", GUILayout.Width (15f));
						relativePosition.y = EditorGUILayout.Slider (relativePosition.y, 0f, 100f);
					EditorGUILayout.EndHorizontal ();
				}
			EditorGUILayout.EndVertical ();
			
			EditorGUILayout.BeginVertical ("Button");
				sizeType = (AC_SizeType) EditorGUILayout.EnumPopup ("Size:", sizeType);
				if (sizeType == AC_SizeType.Manual)
				{
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("W:", GUILayout.Width (17f));
					slotSize.x = EditorGUILayout.Slider (slotSize.x, 0f, 100f);
					EditorGUILayout.LabelField ("H:", GUILayout.Width (15f));
					slotSize.y = EditorGUILayout.Slider (slotSize.y, 0f, 100f);
					EditorGUILayout.EndHorizontal ();
				}
				else if (sizeType == AC_SizeType.AbsolutePixels)
				{
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("Width:", GUILayout.Width (50f));
					slotSize.x = EditorGUILayout.FloatField (slotSize.x);
					EditorGUILayout.LabelField ("Height:", GUILayout.Width (50f));
					slotSize.y = EditorGUILayout.FloatField (slotSize.y);
					EditorGUILayout.EndHorizontal ();
				}
			EditorGUILayout.EndVertical ();
		}
		
		
		protected void ShowClipHelp ()
		{
			EditorGUILayout.HelpBox ("MenuSystem.OnElementClick will be run when this element is clicked.", MessageType.Info);
		}


		protected T LinkedUiGUI <T> (T field, string label, MenuSource source) where T : Component
		{
			field = (T) EditorGUILayout.ObjectField (label, field, typeof (T), true);
			linkedUiID = Menu.FieldToID <T> (field, linkedUiID);
			return Menu.IDToField <T> (field, linkedUiID, source);
		}


		protected UISlot[] ResizeUISlots (UISlot[] uiSlots, int maxSlots)
		{
			List<UISlot> uiSlotsList = new List<UISlot>();
			if (uiSlots == null)
			{
				return uiSlotsList.ToArray ();
			}

			if (maxSlots < 0)
			{
				maxSlots = 0;
			}

			if (uiSlots.Length == maxSlots)
			{
				return uiSlots;
			}

			// Convert to list
			foreach (UISlot uiSlot in uiSlots)
			{
				uiSlotsList.Add (uiSlot);
			}

			if (maxSlots < uiSlotsList.Count)
			{
				uiSlotsList.RemoveRange (maxSlots, uiSlotsList.Count - maxSlots);
			}
			else if (maxSlots > uiSlotsList.Count)
			{
				if (maxSlots > uiSlotsList.Capacity)
				{
					uiSlotsList.Capacity = maxSlots;
				}
				for (int i=uiSlotsList.Count; i<maxSlots; i++)
				{
					UISlot newUISlot = new UISlot ();
					uiSlotsList.Add (newUISlot);
				}
			}

			return uiSlotsList.ToArray ();
		}
		
		#endif


		public virtual void HideAllUISlots ()
		{}


		protected void LimitUISlotVisibility (UISlot[] uiSlots, int numSlots)
		{
			for (int i=0; i<uiSlots.Length; i++)
			{
				if (i < numSlots)
				{
					uiSlots[i].UpdateUIElement (true);
				}
				else
				{
					uiSlots[i].UpdateUIElement (false);
				}
			}
		}


		public virtual void PreDisplay (int _slot, int languageNumber, bool isActive)
		{}


		public virtual void Display (GUIStyle _style, int _slot, float zoom, bool isActive)
		{
			if (backgroundTexture && _slot == 0)
			{
				GUI.DrawTexture (ZoomRect (relativeRect, zoom), backgroundTexture, ScaleMode.StretchToFill, true, 0f);
			}
		}


		public virtual void DrawOutline (bool isSelected, AC.Menu _menu)
		{
			Color boxColor = Color.yellow;
			if (isSelected)
			{
				boxColor = Color.red;
			}
			for (int i=0; i<GetNumSlots (); i++)
			{
				if (i > 0)
				{
					boxColor = Color.blue;
				}
				DrawStraightLine.DrawBox (_menu.GetRectAbsolute (GetSlotRectRelative (i)), boxColor, 1f, false, 0);
			}

		}


		protected Rect ZoomRect (Rect rect, float zoom)
		{
			if (zoom == 1f)
			{
				if (!Application.isPlaying)
				{
					dragOffset = Vector2.zero;
				}

				if (dragOffset != Vector2.zero)
				{
					rect.x += dragOffset.x;
					rect.y += dragOffset.y;
				}

				return rect;
			}

			return (new Rect (rect.x * zoom, rect.y * zoom, rect.width * zoom, rect.height * zoom));
		}


		protected void LimitOffset (int maxValue)
		{
			if (offset > 0 && (numSlots + offset) > maxValue)
			{
				offset = maxValue - numSlots;
			}
			if (offset < 0)
			{
				offset = 0;
			}
		}


		protected void Shift (AC_ShiftInventory shiftType, int maxSlots, int arraySize, int amount)
		{
			if (shiftType == AC_ShiftInventory.ShiftRight)
			{
				offset += amount;

				if ((maxSlots + offset) >= arraySize)
				{
					offset = arraySize - maxSlots;
				}
			}
			else if (shiftType == AC_ShiftInventory.ShiftLeft && offset > 0)
			{
				offset -= amount;

				if (offset < 0)
				{
					offset = 0;
				}
			}
		}


		public virtual void Shift (AC_ShiftInventory shiftType, int amount)
		{
			Debug.LogWarning ("The MenuElement " + this.title + " cannot be 'Shifted'");
		}


		public virtual bool CanBeShifted (AC_ShiftInventory shiftType)
		{
			return true;
		}
		
		
		public Vector2 GetSize ()
		{
			Vector2 size = new Vector2 (relativeRect.width, relativeRect.height);
			return (size);
		}
		
		
		public Vector2 GetSizeFromCorner ()
		{
			Vector2 size = new Vector2 (relativeRect.width + relativeRect.x, relativeRect.height + relativeRect.y);
			return (size);
		}
		
		
		public void SetSize (Vector2 _size)
		{
			slotSize = new Vector2 (_size.x, _size.y);
		}
		
		
		protected void SetAbsoluteSize (Vector2 _size)
		{
			slotSize = new Vector2 (_size.x * 100f / AdvGame.GetMainGameViewSize ().x, _size.y * 100f / AdvGame.GetMainGameViewSize ().y);
		}


		public int GetNumSlots ()
		{
			return numSlots;
		}
		
		
		public Rect GetSlotRectRelative (int _slot)
		{
			Vector2 screenFactor = Vector2.one;
			if (sizeType != AC_SizeType.AbsolutePixels)
			{
				screenFactor = new Vector2 (AdvGame.GetMainGameViewSize ().x / 100f, AdvGame.GetMainGameViewSize ().y / 100f);
			}

			Rect positionRect = relativeRect;
			positionRect.width = slotSize.x * screenFactor.x;
			positionRect.height = slotSize.y * screenFactor.y;

			if (_slot > numSlots)
			{
				_slot = numSlots;
			}
			
			if (orientation == ElementOrientation.Horizontal)
			{
				positionRect.x += (slotSize.x + slotSpacing) * _slot * screenFactor.x;
			}
			else if (orientation == ElementOrientation.Vertical)
			{
				positionRect.y += (slotSize.y + slotSpacing) * _slot * screenFactor.y;
			}
			else if (orientation == ElementOrientation.Grid)
			{
				int xOffset = _slot + 1;
				float numRows = Mathf.CeilToInt ((float) xOffset / gridWidth) - 1;
				while (xOffset > gridWidth)
				{
					xOffset -= gridWidth;
				}
				xOffset -= 1;

				positionRect.x += (slotSize.x + slotSpacing) * (float) xOffset * screenFactor.x;
				positionRect.y += (slotSize.y + slotSpacing) * numRows * screenFactor.y;
			}

			return (positionRect);
		}
		
		
		public virtual void RecalculateSize (MenuSource source)
		{
			if (source != MenuSource.AdventureCreator)
			{
				return;
			}

			dragOffset = Vector2.zero;
			Vector2 screenSize = Vector2.one;

			if (sizeType == AC_SizeType.Automatic)
			{
				AutoSize ();
			}

			if (sizeType != AC_SizeType.AbsolutePixels)
			{
				screenSize = new Vector2 (AdvGame.GetMainGameViewSize ().x / 100f, AdvGame.GetMainGameViewSize ().y / 100f);
			}

			if (orientation == ElementOrientation.Horizontal)
			{
				relativeRect.width = slotSize.x * screenSize.x * numSlots;
				relativeRect.height = slotSize.y * screenSize.y;
				if (numSlots > 1)
				{
					relativeRect.width += slotSpacing * screenSize.x * (numSlots - 1);
				}
			}
			else if (orientation == ElementOrientation.Vertical)
			{
				relativeRect.width = slotSize.x * screenSize.x;
				relativeRect.height = slotSize.y * screenSize.y * numSlots;
				if (numSlots > 1)
				{
					relativeRect.height += slotSpacing * screenSize.y * (numSlots - 1);
				}
			}
			else if (orientation == ElementOrientation.Grid)
			{
				if (numSlots < gridWidth)
				{
					relativeRect.width = (slotSize.x + slotSpacing) * screenSize.x * numSlots;
					relativeRect.height = slotSize.y * screenSize.y;
				}
				else
				{
					float numRows = Mathf.CeilToInt ((float) numSlots / gridWidth);

					relativeRect.width = slotSize.x * screenSize.x * gridWidth;
					relativeRect.height = slotSize.y * screenSize.y * numRows;

					if (numSlots > 1)
					{
						relativeRect.width += slotSpacing * screenSize.x * (gridWidth - 1);
						relativeRect.height += slotSpacing * screenSize.y * (numRows - 1);
					}
				}
			}
		}


		public int GetFontSize ()
		{
			if (sizeType == AC_SizeType.AbsolutePixels)
			{
				return (int) (fontScaleFactor * 10f);
			}

			return (int) (AdvGame.GetMainGameViewSize ().x * fontScaleFactor / 100);
		}

		
		protected void AutoSize (GUIContent content)
		{
			GUIStyle normalStyle = new GUIStyle();
			normalStyle.font = font;
			normalStyle.fontSize = GetFontSize ();
		
			Vector2 size = GetSize ();
			size = normalStyle.CalcSize (content);
			
			SetAbsoluteSize (size);
		}


		protected virtual void AutoSize ()
		{
			GUIContent content = new GUIContent (backgroundTexture);
			AutoSize (content);
		}
		
		
		public void SetPosition (Vector2 _position)
		{
			relativeRect.x = _position.x;
			relativeRect.y = _position.y;
		}


		public void SetRelativePosition (Vector2 _size)
		{
			relativeRect.x = relativePosition.x * _size.x;
			relativeRect.y = relativePosition.y * _size.y;
		}


		public void ResetDragOffset ()
		{
			dragOffset = Vector2.zero;
		}


		public void SetDragOffset (Vector2 pos, Rect dragRect)
		{
			if (pos.x < dragRect.x)
			{
				pos.x = dragRect.x;
			}
			else if (pos.x > (dragRect.x + dragRect.width - relativeRect.width))
			{
				pos.x = dragRect.x + dragRect.width - relativeRect.width;
			}
			
			if (pos.y < dragRect.y)
			{
				pos.y = dragRect.y;
			}
			else if (pos.y > (dragRect.y + dragRect.height - relativeRect.height))
			{
				pos.y = dragRect.y + dragRect.height - relativeRect.height;
			}

			dragOffset = pos;
		}

		public Vector2 GetDragStart ()
		{
			return new Vector2 (-dragOffset.x, dragOffset.y);
		}

		
		public void AutoSetVisibility ()
		{
			if (numSlots == 0)
			{
				isVisible = false;
			}
			else
			{
				isVisible = true;
			}
		}


		protected T LinkUIElement <T> () where T : Behaviour
		{
			T field = Serializer.returnComponent <T> (linkedUiID);
			return field;
		}


		protected void UpdateUIElement <T> (T field) where T : Behaviour
		{
			if (Application.isPlaying && field != null && field.gameObject.activeSelf != isVisible)
			{
				field.gameObject.SetActive (isVisible);
			}
		}


		protected void ClearSpriteCache (UISlot[] uiSlots)
		{
			foreach (UISlot uiSlot in uiSlots)
			{
				uiSlot.sprite = null;
			}
		}

	}

}