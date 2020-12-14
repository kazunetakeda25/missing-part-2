/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"MenuTimer.cs"
 * 
 *	This MenuElement can be used in conjunction with MenuDialogList to create
 *	timed conversations, "Walking Dead"-style.
 * 
 */

using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	public class MenuTimer : MenuElement
	{

		public Slider uiSlider;
		public bool doInvert;
		public Texture2D timerTexture;
		public AC_TimerType timerType = AC_TimerType.Conversation;

		private Rect timerRect;


		public override void Declare ()
		{
			uiSlider = null;
			doInvert = false;
			isVisible = true;
			isClickable = false;
			timerType = AC_TimerType.Conversation;
			numSlots = 1;
			SetSize (new Vector2 (20f, 5f));
			
			base.Declare ();
		}


		public override MenuElement DuplicateSelf ()
		{
			MenuTimer newElement = CreateInstance <MenuTimer>();
			newElement.Declare ();
			newElement.CopyTimer (this);
			return newElement;
		}
		
		
		public void CopyTimer (MenuTimer _element)
		{
			uiSlider = _element.uiSlider;
			doInvert = _element.doInvert;
			timerTexture = _element.timerTexture;
			timerType = _element.timerType;
			
			base.Copy (_element);
		}


		public override void LoadUnityUI (AC.Menu _menu)
		{
			uiSlider = LinkUIElement <Slider>();

			if (uiSlider)
			{
				uiSlider.minValue = 0f;
				uiSlider.maxValue = 1f;
				uiSlider.wholeNumbers = false;
				uiSlider.value = 1f;
			}
		}

		
		public override RectTransform GetRectTransform (int _slot)
		{
			if (uiSlider)
			{
				return uiSlider.GetComponent <RectTransform>();
			}
			return null;
		}


		#if UNITY_EDITOR
		
		public override void ShowGUI (MenuSource source)
		{
			EditorGUILayout.BeginVertical ("Button");
			timerType = (AC_TimerType) EditorGUILayout.EnumPopup ("Timer type:", timerType);
			doInvert = EditorGUILayout.Toggle ("Invert value?", doInvert);

			if (source == MenuSource.AdventureCreator)
			{
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Timer texture:", GUILayout.Width (145f));
				timerTexture = (Texture2D) EditorGUILayout.ObjectField (timerTexture, typeof (Texture2D), false, GUILayout.Width (70f), GUILayout.Height (30f));
				EditorGUILayout.EndHorizontal ();
			}
			else
			{
				uiSlider = LinkedUiGUI <Slider> (uiSlider, "Linked Slider:", source);
			}
			EditorGUILayout.EndVertical ();

			if (source == MenuSource.AdventureCreator)
			{
				EndGUI ();
			}
		}
		
		#endif
		
		
		public override void PreDisplay (int _slot, int languageNumber, bool isActive)
		{
			if (Application.isPlaying)
			{
				float progress = 0f;

				if (timerType == AC_TimerType.Conversation)
				{
					if (KickStarter.playerInput.activeConversation && KickStarter.playerInput.activeConversation.isTimed)
					{
						progress = KickStarter.playerInput.activeConversation.GetTimeRemaining ();
					}
				}
				else if (timerType == AC_TimerType.QuickTimeEventProgress)
				{
					progress = KickStarter.playerQTE.GetProgress ();
				}
				else if (timerType == AC_TimerType.QuickTimeEventRemaining)
				{
					progress = KickStarter.playerQTE.GetRemainingTimeFactor ();
				}

				if (doInvert)
				{
					progress = 1f - progress;
				}

				if (uiSlider)
				{
					uiSlider.value = progress;
					UpdateUIElement (uiSlider);
				}
				else
				{
					timerRect = relativeRect;
					timerRect.width *= progress;
				}
			}
			else
			{
				timerRect = relativeRect;
			}
		}
		
		
		public override void Display (GUIStyle _style, int _slot, float zoom, bool isActive)
		{
			if (timerTexture)
			{
				GUI.DrawTexture (ZoomRect (timerRect, zoom), timerTexture, ScaleMode.StretchToFill, true, 0f);
			}
			
			base.Display (_style, _slot, zoom, isActive);
		}

	}

}