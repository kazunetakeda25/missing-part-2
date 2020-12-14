/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"UISlot.cs"
 * 
 *	This is a class for Unity UI elements that contain both
 *	Image and Text components that must be linked to AC's Menu system.
 * 
 */

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	[System.Serializable]
	public class UISlot
	{

		public UnityEngine.UI.Button uiButton;
		public int uiButtonID;
		private Text uiText;
		private Image uiImage;

		public Sprite sprite;
		private Sprite emptySprite;
		private Texture2D texture;


		public UISlot ()
		{
			uiButton = null;
			uiButtonID = 0;
			uiText = null;
			uiImage = null;
			sprite = null;
		}


		#if UNITY_EDITOR

		public void LinkedUiGUI (int i, MenuSource source)
		{
			uiButton = (UnityEngine.UI.Button) EditorGUILayout.ObjectField ("Linked Button (" + (i+1).ToString () + "):", uiButton, typeof (UnityEngine.UI.Button), true);
			uiButtonID = Menu.FieldToID <UnityEngine.UI.Button> (uiButton, uiButtonID);
			uiButton = Menu.IDToField <UnityEngine.UI.Button> (uiButton, uiButtonID, source);
		}

		#endif


		public RectTransform GetRectTransform ()
		{
			if (uiButton != null && uiButton.GetComponent <RectTransform>())
			{
				return uiButton.GetComponent <RectTransform>();
			}
			return null;
		}


		public void LinkUIElements ()
		{
			uiButton = Serializer.returnComponent <UnityEngine.UI.Button> (uiButtonID);
			if (uiButton)
			{
				if (uiButton.GetComponentInChildren <Text>())
				{
					uiText = uiButton.GetComponentInChildren <Text>();
				}
				if (uiButton.GetComponentInChildren <Image>())
				{
					uiImage = uiButton.GetComponentInChildren <Image>();
				}
			}
		}


		public void SetText (string _text)
		{
			if (uiText)
			{
				uiText.text = _text;
			}
		}


		public void SetImage (Texture2D _texture)
		{
			if (uiImage)
			{
				if (_texture == null)
				{
					if (emptySprite == null)
					{
						emptySprite = Resources.Load <Sprite> ("EmptySlot");
					}

					sprite = emptySprite;
				}
				else if (sprite == null || sprite == emptySprite || texture != _texture)
				{
					sprite = Sprite.Create (_texture, new Rect (0f, 0f, _texture.width, _texture.height), new Vector2 (0.5f, 0.5f));
				}

				if (_texture != null)
				{
					texture = _texture;
				}

				uiImage.sprite = sprite;
			}
		}


		public void UpdateUIElement (bool isVisible)
		{
			if (Application.isPlaying && uiButton != null && uiButton.gameObject.activeSelf != isVisible)
			{
				uiButton.gameObject.SetActive (isVisible);
			}
		}


		public void AddClickHandler (AC.Menu _menu, MenuElement _element, int _slot)
		{
			UISlotClick uiSlotClick = uiButton.gameObject.AddComponent <UISlotClick>();
			uiSlotClick.Setup (_menu, _element, _slot);
		}

	}

}