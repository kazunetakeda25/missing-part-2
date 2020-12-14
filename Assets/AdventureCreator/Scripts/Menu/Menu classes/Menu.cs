/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"Menu.cs"
 * 
 *	This script is a container of MenuElement subclasses, which together make up a menu.
 *	When menu elements are added, this script updates the size, positioning etc automatically.
 *	The handling of menu visibility, element clicking, etc is all handled in MenuSystem,
 *	rather than the Menu class itself.
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
	public class Menu : ScriptableObject
	{

		public MenuSource menuSource = MenuSource.AdventureCreator;
		public Canvas canvas;
		public int canvasID = 0;
		public RectTransform rectTransform;
		public int rectTransformID = 0;
		public UITransition uiTransitionType = UITransition.None;
		public UIPositionType uiPositionType = UIPositionType.Manual;

		public bool isEditing = false;
		public bool isLocked = false;
		public int id;
		public string title;
		public Vector2 manualSize = Vector2.zero;
		public AC_PositionType positionType = AC_PositionType.Centred;
		public Vector2 manualPosition = Vector2.zero;
		public TextAnchor alignment = TextAnchor.MiddleCenter;
		public string toggleKey = "";
		public bool ignoreMouseClicks = false;
		public bool pauseWhenEnabled = false;
		public bool enabledOnStart = false;
		public ActionListAsset actionListOnTurnOn = null;
		public ActionListAsset actionListOnTurnOff = null;
		
		public Texture2D backgroundTexture;
		public Texture2D sliderTexture;
		
		public List<MenuElement> visibleElements = new List<MenuElement>();
		public float transitionProgress = 0f;
		public AppearType appearType;
		public SpeechMenuType speechMenuType = SpeechMenuType.All;
		
		public MenuElement selected_element;
		public int selected_slot = 0;
		public string firstSelectedElement;
		
		public List<MenuElement> elements = new List<MenuElement>();
		
		[SerializeField] private Vector2 biggestElementSize;
		
		public float spacing;
		private bool isEnabled;
		public AC_SizeType sizeType;
		
		public MenuOrientation orientation;
		[SerializeField] private Rect rect = new Rect ();
		
		public MenuTransition transitionType = MenuTransition.None;
		public PanDirection panDirection = PanDirection.Up;
		public PanMovement panMovement = PanMovement.Linear;
		public float panDistance = 0.5f;
		public float fadeSpeed = 0f;
		private float fadeStartTime = 0f;
		public TextAnchor zoomAnchor = TextAnchor.MiddleCenter;
		private bool isFading = false;
		private FadeType fadeType = FadeType.fadeIn;
		private Vector2 panOffset = Vector2.zero;
		private Vector2 dragOffset = Vector2.zero;
		private float zoomAmount = 1f;
		public bool zoomElements = false;
		private Rect aspectCorrectedRect = new Rect ();

		public Speech speech;
		public bool oneMenuPerSpeech = false;


		public void Declare (int[] idArray)
		{
			menuSource = MenuSource.AdventureCreator;
			canvas = null;
			canvasID = 0;
			uiPositionType = UIPositionType.Manual;
			uiTransitionType = UITransition.None;

			spacing = 0.5f;
			orientation = MenuOrientation.Vertical;
			appearType = AppearType.Manual;
			oneMenuPerSpeech = false;

			elements = new List<MenuElement>();
			visibleElements = new List<MenuElement>();
			enabledOnStart = false;
			isEnabled = false;
			sizeType = AC_SizeType.Automatic;
			speechMenuType = SpeechMenuType.All;
			actionListOnTurnOn = null;
			actionListOnTurnOff = null;
			firstSelectedElement = "";
			
			fadeSpeed = 0f;
			transitionType = MenuTransition.Fade;
			panDirection = PanDirection.Up;
			panMovement = PanMovement.Linear;
			panDistance = 0.5f;
			zoomAnchor = TextAnchor.MiddleCenter;
			zoomElements = false;
			ignoreMouseClicks = false;
			
			pauseWhenEnabled = false;
			id = 0;
			isLocked = false;
			
			// Update id based on array
			foreach (int _id in idArray)
			{
				if (id == _id)
				{
					id ++;
				}
			}
			
			title = "Menu " + (id + 1).ToString ();
		}

		
		public void Copy (AC.Menu _menu)
		{
			menuSource = _menu.menuSource;
			canvas = _menu.canvas;
			canvasID = _menu.canvasID;
			rectTransform = _menu.rectTransform;
			rectTransformID = _menu.rectTransformID;
			uiTransitionType = _menu.uiTransitionType;
			uiPositionType = _menu.uiPositionType;

			isEditing = false;
			id = _menu.id;
			isLocked = _menu.isLocked;
			title = _menu.title;
			manualSize = _menu.manualSize;
			positionType = _menu.positionType;
			manualPosition = _menu.manualPosition;
			alignment = _menu.alignment;
			toggleKey = _menu.toggleKey;

			backgroundTexture = _menu.backgroundTexture;
			sliderTexture = _menu.sliderTexture;
			visibleElements = new List<MenuElement>();
			transitionProgress = 0f;
			appearType = _menu.appearType;
			oneMenuPerSpeech = _menu.oneMenuPerSpeech;
			selected_element = null;
			selected_slot = 0;
			firstSelectedElement = _menu.firstSelectedElement;

			spacing = _menu.spacing;
			sizeType = _menu.sizeType;
			orientation = _menu.orientation;
			fadeSpeed = _menu.fadeSpeed;
			transitionType = _menu.transitionType;
			panDirection = _menu.panDirection;
			panMovement = _menu.panMovement;
			panDistance = _menu.panDistance;
			zoomAnchor = _menu.zoomAnchor;
			zoomElements = _menu.zoomElements;

			pauseWhenEnabled = _menu.pauseWhenEnabled;
			speechMenuType = _menu.speechMenuType;
			enabledOnStart = _menu.enabledOnStart;
			actionListOnTurnOn = _menu.actionListOnTurnOn;
			actionListOnTurnOff = _menu.actionListOnTurnOff;
			ignoreMouseClicks = _menu.ignoreMouseClicks;
			
			elements = new List<MenuElement>();
			foreach (MenuElement _element in _menu.elements)
			{
				MenuElement newElement = _element.DuplicateSelf ();
				elements.Add (newElement);
			}

			Recalculate ();
		}


		public void LoadUnityUI ()
		{
			Canvas localCanvas = null;

			if (menuSource == MenuSource.UnityUiPrefab)
			{
				if (canvas != null)
				{
					localCanvas = (Canvas) Instantiate (canvas);
					localCanvas.gameObject.name = canvas.name;
					DontDestroyOnLoad (localCanvas.gameObject);
				}
			}
			else if (menuSource == MenuSource.UnityUiInScene)
			{
				localCanvas = Serializer.returnComponent <Canvas> (canvasID);
			}

			canvas = localCanvas;
			EnableUI ();

			if (localCanvas != null)
			{
				rectTransform = Serializer.returnComponent <RectTransform> (rectTransformID);
				if (localCanvas.renderMode != RenderMode.ScreenSpaceOverlay && localCanvas.worldCamera == null)
				{
					localCanvas.worldCamera = Camera.main;
				}

				if (localCanvas.renderMode != RenderMode.WorldSpace)
				{
					SetParent ();
				}
			}

			if (IsUnityUI ())
			{
				foreach (MenuElement _element in elements)
				{
					_element.LoadUnityUI (this);
				}
			}

			DisableUI ();
		}


		private void SetAnimState ()
		{
			if (IsUnityUI () && uiTransitionType == UITransition.CustomAnimation && fadeSpeed > 0f && canvas != null && canvas.GetComponent <Animator>())
			{
				Animator animator = canvas.GetComponent <Animator>();
				
				if (isFading)
				{
					if (fadeType == FadeType.fadeIn)
					{
						animator.Play ("On", -1, transitionProgress);
					}
					else
					{
						animator.Play ("Off", -1, 1f - transitionProgress);
					}
				}
				else
				{
					if (isEnabled)
					{
						animator.Play ("OnInstant", -1, 0f);
					}
					else
					{
						animator.Play ("OffInstant", -1, 0f);
					}
				}
			}
		}


		public void SetParent ()
		{
			GameObject uiOb = GameObject.Find ("_UI");
			if (uiOb != null && canvas != null)
			{
				uiOb.transform.position = Vector3.zero;
				canvas.transform.SetParent (uiOb.transform);
			}
		}


		public void ClearParent ()
		{
			GameObject uiOb = GameObject.Find ("_UI");
			if (uiOb != null && canvas != null)
			{
				if (canvas.transform.parent == uiOb.transform)
				{
					canvas.transform.SetParent (null);
				}
		    }
		}


		public void Initalise ()
		{
			if (appearType == AppearType.Manual && enabledOnStart && !isLocked)
			{
				transitionProgress = 1f;
				EnableUI ();
				TurnOn (false);
			}
			else
			{
				transitionProgress = 0f;
				DisableUI ();
				TurnOff (false);
			}
			if (transitionType == MenuTransition.Zoom)
			{
				zoomAmount = 0f;
			}

			SetAnimState ();
			UpdateTransition ();
		}


		private void EnableUI ()
		{
			if (canvas)
			{
				canvas.gameObject.SetActive (true);
				canvas.enabled = true;
				KickStarter.playerMenus.FindFirstSelectedElement ();
			}
		}


		private void DisableUI ()
		{
			if (canvas)
			{
				isEnabled = false;
				isFading = false;
				SetAnimState ();

				canvas.gameObject.SetActive (false);
				KickStarter.playerMenus.FindFirstSelectedElement ();
			}
		}


		#if UNITY_EDITOR
		
		public void ShowGUI ()
		{
			title = EditorGUILayout.TextField ("Menu name:", title);
			menuSource = (MenuSource) EditorGUILayout.EnumPopup ("Source:", menuSource);

			isLocked = EditorGUILayout.Toggle ("Start game locked off?", isLocked);
			ignoreMouseClicks = EditorGUILayout.Toggle ("Ignore Cursor clicks?", ignoreMouseClicks);
			actionListOnTurnOn = ActionListAssetMenu.AssetGUI ("ActionList when turn on:", actionListOnTurnOn);
			actionListOnTurnOff = ActionListAssetMenu.AssetGUI ("ActionList when turn off:", actionListOnTurnOff);
			
			appearType = (AppearType) EditorGUILayout.EnumPopup ("Appear type:", appearType);
			if (appearType == AppearType.OnInputKey)
			{
				toggleKey = EditorGUILayout.TextField ("Toggle key:", toggleKey);
			}
			if (appearType == AppearType.Manual || appearType == AppearType.OnInputKey)
			{
				if (appearType == AppearType.Manual)
				{
					enabledOnStart = EditorGUILayout.Toggle ("Enabled on start?", enabledOnStart);
				}
				pauseWhenEnabled = EditorGUILayout.Toggle ("Pause game when enabled?", pauseWhenEnabled);
			}
			else if (appearType == AppearType.OnInteraction || appearType == AppearType.OnContainer)
			{
				pauseWhenEnabled = EditorGUILayout.Toggle ("Pause game when enabled?", pauseWhenEnabled);
			}
			else if (appearType == AppearType.WhenSpeechPlays)
			{
				speechMenuType = (SpeechMenuType) EditorGUILayout.EnumPopup ("Display speech of type:", speechMenuType);
				oneMenuPerSpeech = EditorGUILayout.Toggle ("Duplicate for multiple lines?", oneMenuPerSpeech);
			}

			if (menuSource == MenuSource.AdventureCreator)
			{
				spacing = EditorGUILayout.Slider ("Spacing (%):", spacing, 0f, 10f);
				orientation = (MenuOrientation) EditorGUILayout.EnumPopup ("Element orientation:", orientation);
				
				positionType = (AC_PositionType) EditorGUILayout.EnumPopup ("Position:", positionType);
				if (positionType == AC_PositionType.Aligned)
				{
					alignment = (TextAnchor) EditorGUILayout.EnumPopup ("Alignment:", alignment);
				}
				else if (positionType == AC_PositionType.Manual || positionType == AC_PositionType.FollowCursor || positionType == AC_PositionType.AppearAtCursorAndFreeze || positionType == AC_PositionType.OnHotspot || positionType == AC_PositionType.AboveSpeakingCharacter || positionType == AC_PositionType.AbovePlayer)
				{
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("X:", GUILayout.Width (20f));
					manualPosition.x = EditorGUILayout.Slider (manualPosition.x, 0f, 100f);
					EditorGUILayout.LabelField ("Y:", GUILayout.Width (20f));
					manualPosition.y = EditorGUILayout.Slider (manualPosition.y, 0f, 100f);
					EditorGUILayout.EndHorizontal ();
				}
				
				sizeType = (AC_SizeType) EditorGUILayout.EnumPopup ("Size:", sizeType);
				if (sizeType == AC_SizeType.Manual)
				{
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("W:", GUILayout.Width (15f));
					manualSize.x = EditorGUILayout.Slider (manualSize.x, 0f, 100f);
					EditorGUILayout.LabelField ("H:", GUILayout.Width (15f));
					manualSize.y = EditorGUILayout.Slider (manualSize.y, 0f, 100f);
					EditorGUILayout.EndHorizontal ();
				}
				else if (sizeType == AC_SizeType.AbsolutePixels)
				{
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("Width:", GUILayout.Width (50f));
					manualSize.x = EditorGUILayout.FloatField (manualSize.x);
					EditorGUILayout.LabelField ("Height:", GUILayout.Width (50f));
					manualSize.y = EditorGUILayout.FloatField (manualSize.y);
					EditorGUILayout.EndHorizontal ();
				}
				
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Background texture:", GUILayout.Width (145f));
				backgroundTexture = (Texture2D) EditorGUILayout.ObjectField (backgroundTexture, typeof (Texture2D), false, GUILayout.Width (70f), GUILayout.Height (30f));
				EditorGUILayout.EndHorizontal ();
				
				transitionType = (MenuTransition) EditorGUILayout.EnumPopup ("Transition type:", transitionType);
				if (transitionType == MenuTransition.Pan || transitionType == MenuTransition.FadeAndPan)
				{
					panDirection = (PanDirection) EditorGUILayout.EnumPopup ("Pan from:", panDirection);
					panMovement= (PanMovement) EditorGUILayout.EnumPopup ("Pan movement:", panMovement);
					panDistance = EditorGUILayout.Slider ("Pan distance:", panDistance, 0f, 1f);
				}
				else if (transitionType == MenuTransition.Zoom)
				{
					zoomAnchor = (TextAnchor) EditorGUILayout.EnumPopup ("Zoom from:", zoomAnchor);
					zoomElements = EditorGUILayout.Toggle ("Adjust elements?", zoomElements);
				}
				if (transitionType != MenuTransition.None)
				{
					fadeSpeed = EditorGUILayout.Slider ("Transition time (s):", fadeSpeed, 0f, 2f);
				}
			}
			else
			{
				uiPositionType = (UIPositionType) EditorGUILayout.EnumPopup ("Position type:", uiPositionType);
				uiTransitionType = (UITransition) EditorGUILayout.EnumPopup ("Transition type:", uiTransitionType);
				if (uiTransitionType != UITransition.None)
				{
					fadeSpeed = EditorGUILayout.Slider ("Transition time (s):", fadeSpeed, 0f, 2f);
					if (uiTransitionType == UITransition.CanvasGroupFade)
					{
						if (canvas == null || canvas.GetComponent <CanvasGroup>() == null)
						{
							EditorGUILayout.HelpBox ("A Canvas Group component must be attached to the Canvas object.", MessageType.Info);
						}
					}
					else if (uiTransitionType == UITransition.CustomAnimation)
					{
						EditorGUILayout.HelpBox ("The Canvas must have an Animator with 4 States: On, Off, OnInstant and OffInstant.", MessageType.Info);
					}
				}

				bool isInScene = false;
				if (menuSource == MenuSource.UnityUiInScene)
				{
					isInScene = true;
				}

				canvas = (Canvas) EditorGUILayout.ObjectField ("Linked Canvas:", canvas, typeof (Canvas), isInScene);
				if (isInScene)
				{
					canvasID = Menu.FieldToID <Canvas> (canvas, canvasID);
					canvas = Menu.IDToField <Canvas> (canvas, canvasID, menuSource);
				}

				rectTransform = (RectTransform) EditorGUILayout.ObjectField ("RectTransform boundary:", rectTransform, typeof (RectTransform), true);
				rectTransformID = Menu.FieldToID <RectTransform> (rectTransform, rectTransformID);
				rectTransform = Menu.IDToField <RectTransform> (rectTransform, rectTransformID, menuSource);

				firstSelectedElement = EditorGUILayout.TextField ("First selected Element:", firstSelectedElement);
			}
		}


		public static int FieldToID <T> (T field, int _constantID) where T : Component
		{
			if (field == null)
			{
				return _constantID;
			}
			
			if (field.GetComponent <ConstantID>())
			{
				if (!field.gameObject.activeInHierarchy && field.GetComponent <ConstantID>().constantID == 0)
				{
					field.GetComponent <ConstantID>().AssignInitialValue (true);
				}
				_constantID = field.GetComponent <ConstantID>().constantID;
			}
			else
			{
				field.gameObject.AddComponent <ConstantID>();
				_constantID = field.GetComponent <ConstantID>().AssignInitialValue (true);
				AssetDatabase.SaveAssets ();
			}
			
			return _constantID;
		}
		
		
		public static T IDToField <T> (T field, int _constantID, MenuSource source) where T : Component
		{
			if (Application.isPlaying || source == MenuSource.AdventureCreator)
			{
				return field;
			}
			
			T newField = field;
			if (_constantID != 0)
			{
				newField = Serializer.returnComponent <T> (_constantID);
				if (newField != null && source == MenuSource.UnityUiInScene)
				{
					field = newField;
				}
				
				EditorGUILayout.BeginVertical ("Button");
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Recorded ConstantID: " + _constantID.ToString (), EditorStyles.miniLabel);
				if (field == null && source == MenuSource.UnityUiInScene)
				{
					if (GUILayout.Button ("Search scenes", EditorStyles.miniButton))
					{
						AdvGame.FindObjectWithConstantID (_constantID);
					}
				}
				EditorGUILayout.EndHorizontal ();
				EditorGUILayout.EndVertical ();
			}
			return field;
		}
		
		#endif


		public bool IsUnityUI ()
		{
			if (menuSource == MenuSource.UnityUiPrefab || menuSource == MenuSource.UnityUiInScene)
			{
				return true;
			}
			return false;
		}

		
		public void DrawOutline (MenuElement _selectedElement)
		{
			DrawStraightLine.DrawBox (rect, Color.yellow, 1f, false, 1);
			
			foreach (MenuElement element in visibleElements)
			{
				if (element == _selectedElement)
				{
					element.DrawOutline (true, this);
				}
				{
					element.DrawOutline (false, this);
				}
			}
		}
		
		
		public void StartDisplay ()
		{
			if (isFading)
			{
				GUI.BeginGroup (new Rect (dragOffset.x + panOffset.x + GetRect ().x, dragOffset.y + panOffset.y + GetRect ().y, GetRect ().width * zoomAmount, GetRect ().height * zoomAmount));
			}
			else
			{
				GUI.BeginGroup (new Rect (dragOffset.x + GetRect ().x, dragOffset.y + GetRect ().y, GetRect ().width * zoomAmount, GetRect ().height * zoomAmount));
			}

			if (backgroundTexture)
			{
				Rect texRect = new Rect (0f, 0f, rect.width, rect.height);
				GUI.DrawTexture (texRect, backgroundTexture, ScaleMode.StretchToFill, true, 0f);
			}
		}
		
		
		public void EndDisplay ()
		{
			GUI.EndGroup ();
		}
		
		
		public void SetPosition (Vector2 _position)
		{
			rect.x = _position.x * AdvGame.GetMainGameViewSize ().x;
			rect.y = _position.y * AdvGame.GetMainGameViewSize ().y;
			
			FitMenuInsideScreen ();
			UpdateAspectRect ();
		}


		public void SetCentre (Vector3 _position)
		{
			if (IsUnityUI ())
			{
				if (canvas != null && rectTransform != null && canvas.renderMode == RenderMode.WorldSpace)
				{
					rectTransform.transform.position = _position;
				}
				
				return;
			}
			
			Vector2 centre = new Vector2 (_position.x * AdvGame.GetMainGameViewSize ().x, _position.y * AdvGame.GetMainGameViewSize ().y);
			
			rect.x = centre.x - (rect.width / 2);
			rect.y = centre.y - (rect.height / 2);
			
			FitMenuInsideScreen ();
			UpdateAspectRect ();
		}
		
		
		public void SetCentre (Vector2 _position)
		{
			if (IsUnityUI ())
			{
				if (canvas != null && rectTransform != null)
				{
					if (canvas.renderMode != RenderMode.WorldSpace)
					{
						float minLeft = rectTransform.sizeDelta.x / 2f * canvas.scaleFactor * rectTransform.localScale.x;
						float minTop = rectTransform.sizeDelta.y / 2f * canvas.scaleFactor * rectTransform.localScale.y;
						_position.x = Mathf.Clamp (_position.x, minLeft, Screen.width - minLeft);
						_position.y = Mathf.Clamp (_position.y, minTop, Screen.height - minTop);
					}
					rectTransform.transform.position = new Vector3 (_position.x, _position.y, rectTransform.transform.position.z);
				}

				return;
			}

			Vector2 centre = new Vector2 (_position.x * AdvGame.GetMainGameViewSize ().x, _position.y * AdvGame.GetMainGameViewSize ().y);
			
			rect.x = centre.x - (rect.width / 2);
			rect.y = centre.y - (rect.height / 2);
			
			FitMenuInsideScreen ();
			UpdateAspectRect ();
		}
		
		
		private Vector2 GetCentre ()
		{
			Vector2 centre = Vector2.zero;
			
			centre.x = (rect.x + (rect.width / 2)) / AdvGame.GetMainGameViewSize ().x * 100f;
			centre.y = (rect.y + (rect.height / 2)) / AdvGame.GetMainGameViewSize ().y * 100f;
			
			return centre;
		}
		
		
		private void FitMenuInsideScreen ()
		{
			if (rect.x < 0f)
			{
				rect.x = 0f;
			}
			
			if (rect.y < 0f)
			{
				rect.y = 0f;
			}
			
			if ((rect.x + rect.width) > AdvGame.GetMainGameViewSize ().x)
			{
				rect.x = AdvGame.GetMainGameViewSize ().x - rect.width;
			}
			
			if ((rect.y + rect.height) > AdvGame.GetMainGameViewSize ().y)
			{
				rect.y = AdvGame.GetMainGameViewSize ().y - rect.height;
			}
		}
		
		
		public void Align (TextAnchor _anchor)
		{
			// X
			if (_anchor == TextAnchor.LowerLeft || _anchor == TextAnchor.MiddleLeft || _anchor == TextAnchor.UpperLeft)
			{
				rect.x = 0;
			}
			else if (_anchor == TextAnchor.LowerCenter || _anchor == TextAnchor.MiddleCenter || _anchor == TextAnchor.UpperCenter)
			{
				rect.x = (AdvGame.GetMainGameViewSize ().x - rect.width) / 2;
			}
			else
			{
				rect.x = AdvGame.GetMainGameViewSize ().x - rect.width;
			}
			
			// Y
			if (_anchor == TextAnchor.LowerLeft || _anchor == TextAnchor.LowerCenter || _anchor == TextAnchor.LowerRight)
			{
				rect.y = AdvGame.GetMainGameViewSize ().y - rect.height;
			}
			else if (_anchor == TextAnchor.MiddleLeft || _anchor == TextAnchor.MiddleCenter || _anchor == TextAnchor.MiddleRight)
			{
				rect.y = (AdvGame.GetMainGameViewSize ().y - rect.height) / 2;
			}
			else
			{
				rect.y = 0;
			}
		}
		
		
		private void SetManualSize (Vector2 _size)
		{
			rect.width = _size.x * AdvGame.GetMainGameViewSize ().x;
			rect.height = _size.y * AdvGame.GetMainGameViewSize ().y;
		}


		public bool IsPointInside (Vector2 _point)
		{
			if (menuSource == MenuSource.AdventureCreator)
			{
				return GetRect ().Contains (_point);
			}
			else if (rectTransform != null && canvas != null)
			{
				bool turnOffAgain = false;
				bool answer = false;
				if (!canvas.gameObject.activeSelf)
				{
					canvas.gameObject.SetActive (true);
					turnOffAgain = true;
				}

				if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
				{
					answer = RectTransformUtility.RectangleContainsScreenPoint (rectTransform, new Vector2 (_point.x, Screen.height - _point.y), null);
				}
				else
				{
					answer = RectTransformUtility.RectangleContainsScreenPoint (rectTransform, new Vector2 (_point.x, Screen.height - _point.y), canvas.worldCamera);
				}

				if (turnOffAgain)
				{
					canvas.gameObject.SetActive (false);
				}
				return answer;
			}
			return false;
		}
		
		
		public Rect GetRect ()
		{
			if (!Application.isPlaying)
			{
				return MainCamera._LimitMenuToAspect (rect);
			}
			
			if (aspectCorrectedRect == new Rect ())
			{
				UpdateAspectRect ();
			}
			
			return aspectCorrectedRect;
		}
		
		
		public void UpdateAspectRect ()
		{
			if (IsUnityUI ())
			{
				return;
			}

			// This used to be called every GetRect (), but is now only done when the menu changes position
			aspectCorrectedRect = MainCamera._LimitMenuToAspect (rect);
		}
		
		
		public bool IsPointerOverSlot (MenuElement _element, int slot, Vector2 _point) 
		{
			if (menuSource == MenuSource.AdventureCreator)
			{
				Rect rectRelative = _element.GetSlotRectRelative (slot);
				Rect rectAbsolute = GetRectAbsolute (rectRelative);
				return (rectAbsolute.Contains (_point));
			}
			else if (canvas != null)
			{
				if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
				{
					return RectTransformUtility.RectangleContainsScreenPoint (_element.GetRectTransform (slot), new Vector2 (_point.x, Screen.height - _point.y), null);
				}
				else
				{
					return RectTransformUtility.RectangleContainsScreenPoint (_element.GetRectTransform (slot), new Vector2 (_point.x, Screen.height - _point.y), canvas.worldCamera);
				}
			}
			return false;
		}
		
		
		public Rect GetRectAbsolute (Rect _rectRelative)
		{
			Rect RectAbsolute = new Rect (_rectRelative.x + dragOffset.x + GetRect ().x, _rectRelative.y + dragOffset.y + GetRect ().y, _rectRelative.width, _rectRelative.height);
			return (RectAbsolute);
		}
		
		
		public void ResetVisibleElements ()
		{
			visibleElements.Clear ();
			foreach (MenuElement element in elements)
			{
				if (element.isVisible)
				{
					visibleElements.Add (element);
				}
			}
		}
		
		
		public void Recalculate ()
		{
			if (IsUnityUI ())
			{
				AutoResize ();
				return;
			}

			ResetVisibleElements ();
			PositionElements ();
			
			if (sizeType == AC_SizeType.Automatic)
			{
				AutoResize ();
			}
			else if (sizeType == AC_SizeType.Manual)
			{
				SetManualSize (new Vector2 (manualSize.x / 100f, manualSize.y / 100f));
			}
			else if (sizeType == AC_SizeType.AbsolutePixels)
			{
				rect.width = manualSize.x;
				rect.height = manualSize.y;
			}
			
			if (positionType == AC_PositionType.Centred)
			{
				Centre ();
				manualPosition = GetCentre ();
			}
			else if (positionType == AC_PositionType.Aligned)
			{
				Align (alignment);
				manualPosition = GetCentre ();
			}
			else if (positionType == AC_PositionType.Manual || !Application.isPlaying)
			{
				SetCentre (new Vector2 (manualPosition.x / 100f, manualPosition.y / 100f));
			}

			if (sizeType == AC_SizeType.Automatic)
			{
				UpdateAspectRect ();
			}
		}
		
		
		public void AutoResize ()
		{
			visibleElements.Clear ();
			biggestElementSize = new Vector2 ();
			
			foreach (MenuElement element in elements)
			{
				if (element != null)
				{
					element.RecalculateSize (menuSource);

					if (element.isVisible)
					{
						visibleElements.Add (element);

						if (menuSource == MenuSource.AdventureCreator)
						{
							if (element.GetSizeFromCorner ().x > biggestElementSize.x)
							{
								biggestElementSize.x = element.GetSizeFromCorner ().x;
							}
							
							if (element.GetSizeFromCorner ().y > biggestElementSize.y)
							{
								biggestElementSize.y = element.GetSizeFromCorner ().y;
							}
						}
					}
				}
			}

			if (menuSource == MenuSource.AdventureCreator)
			{
				rect.width = (spacing / 100 * AdvGame.GetMainGameViewSize ().x) + biggestElementSize.x;
				rect.height = (spacing / 100 * AdvGame.GetMainGameViewSize ().x) + biggestElementSize.y;
				manualSize = new Vector2 (rect.width * 100f / AdvGame.GetMainGameViewSize ().x, rect.height * 100f / AdvGame.GetMainGameViewSize ().y);
			}
		}
		
		
		private void PositionElements ()
		{
			float totalLength = 0f;
			
			foreach (MenuElement element in visibleElements)
			{
				if (menuSource != MenuSource.AdventureCreator)
				{
					element.RecalculateSize (menuSource);
					return;
				}

				if (element == null)
				{
					Debug.Log ("Null element found");
					break;
				}
				
				if (element.positionType == AC_PositionType2.RelativeToMenuSize && sizeType == AC_SizeType.Automatic)
				{
					Debug.LogError ("Menu " + title + " cannot display because it's size is Automatic, while it's Element " + element.title + "'s Position is set to Relative");
					return;
				}
				
				element.RecalculateSize (menuSource);
				
				if (element.positionType == AC_PositionType2.RelativeToMenuSize)
				{
					element.SetRelativePosition (new Vector2 (rect.width / 100f, rect.height / 100f));
				}
				else if (orientation == MenuOrientation.Horizontal)
				{
					if (element.positionType == AC_PositionType2.Aligned)
					{
						element.SetPosition (new Vector2 ((spacing / 100 * AdvGame.GetMainGameViewSize ().x) + totalLength, (spacing / 100 * AdvGame.GetMainGameViewSize ().x)));
					}
					
					totalLength += element.GetSize().x + (spacing / 100 * AdvGame.GetMainGameViewSize ().x);
				}
				else
				{
					if (element.positionType == AC_PositionType2.Aligned)
					{
						element.SetPosition (new Vector2 ((spacing / 100 * AdvGame.GetMainGameViewSize ().x), (spacing / 100 * AdvGame.GetMainGameViewSize ().x) + totalLength));
					}
					
					totalLength += element.GetSize().y + (spacing / 100 * AdvGame.GetMainGameViewSize ().x);
				}
			}
		}
		
		
		public void Centre ()
		{
			SetCentre (new Vector2 (0.5f, 0.5f));
		}
		
		
		public bool IsEnabled ()
		{
			if (isLocked)
			{
				if (isFading && fadeType == FadeType.fadeOut)
				{
					return isEnabled;
				}
				
				return false;
			}
			
			return (isEnabled);
		}
		
		
		public bool IsVisible ()
		{
			if (transitionProgress == 1f && isEnabled)
			{
				return true;
			}
			
			return false;
		}
		
		
		public void HandleTransition ()
		{
			if (isFading && isEnabled)
			{
				if (fadeType == FadeType.fadeIn)
				{
					transitionProgress = ((Time.realtimeSinceStartup - fadeStartTime) / fadeSpeed);

					if (transitionProgress > 1f)
					{
						transitionProgress = 1f;
						UpdateTransition ();
						EndTransitionOn ();
						return;
					}
					else
					{
						UpdateTransition ();
					}
				}
				else
				{
					transitionProgress = 1f - ((Time.realtimeSinceStartup - fadeStartTime) / fadeSpeed);

					if (transitionProgress < 0f)
					{
						transitionProgress = 0f;
						UpdateTransition ();
						EndTransitionOff ();
						return;
					}
					else
					{
						UpdateTransition ();
					}
				}
			}
		}
		
		
		private void EndTransitionOn ()
		{
			transitionProgress = 1f;
			isEnabled = true;
			isFading = false;
		}
		
		
		private void EndTransitionOff ()
		{
			transitionProgress = 0f;
			isFading = false;
			isEnabled = false;
			SetAnimState ();
			ReturnGameState ();
			DisableUI ();
			ClearSpeechText ();

			KickStarter.playerMenus.CheckCrossfade (this);
		}
		
		
		public bool IsOn ()
		{
			if (!isLocked && isEnabled && !isFading)
			{
				return true;
			}
			return false;
		}
		
		
		public bool IsOff ()
		{
			if (isLocked)
			{
				return true;
			}
			if (!isEnabled)
			{
				return true;
			}
			return false;
		}
		
		
		public bool TurnOn (bool doFade)
		{
			if (IsOn ())
			{
				return false;
			}

			if (menuSource == MenuSource.AdventureCreator)
			{
				KickStarter.playerMenus.UpdateMenuPosition (this, Vector2.zero);

				if (transitionType == MenuTransition.None || fadeSpeed == 0f)
				{
					doFade = false;
				}
			}
			else
			{
				if (uiTransitionType == UITransition.None || fadeSpeed == 0f)
				{
					doFade = false;
				}
			}
			
			// Setting selected_slot to -2 will cause PlayerInput's selected_option to reset
			if (isLocked)
			{
				#if UNITY_EDITOR
				Debug.Log ("Cannot turn on menu " + title + " as it is locked.");
				#endif
			}
			else if (!isEnabled || (isFading && fadeType == FadeType.fadeOut))// && appearType == AppearType.OnHotspot))
			{
				if (KickStarter.playerInput)
				{
					if (menuSource == MenuSource.AdventureCreator && positionType == AC_PositionType.AppearAtCursorAndFreeze)
					{
						SetCentre (new Vector2 ((KickStarter.playerInput.GetInvertedMouse ().x / Screen.width) + ((manualPosition.x - 50f) / 100f),
						                        (KickStarter.playerInput.GetInvertedMouse ().y / Screen.height) + ((manualPosition.y - 50f) / 100f)));
					}
					else if (menuSource != MenuSource.AdventureCreator && uiPositionType == UIPositionType.AppearAtCursorAndFreeze)
					{
						SetCentre (new Vector2 (KickStarter.playerInput.GetInvertedMouse ().x, Screen.height + 1f - KickStarter.playerInput.GetInvertedMouse ().y));
					}
				}

				selected_slot = -2;
				
				MenuSystem.OnMenuEnable (this);
				ChangeGameState ();
				Recalculate ();
				
				dragOffset = Vector2.zero;
				isEnabled = true;
				isFading = doFade;
				EnableUI ();
				
				if (actionListOnTurnOn != null)
				{
					AdvGame.RunActionListAsset (actionListOnTurnOn);
				}
				
				if (doFade && fadeSpeed > 0f)
				{
					fadeType = FadeType.fadeIn;
					fadeStartTime = Time.realtimeSinceStartup - (transitionProgress * fadeSpeed);
				}
				else
				{
					transitionProgress = 1f;
					isEnabled = true;
					isFading = false;

					if (IsUnityUI ())
					{
						UpdateTransition ();
					}
				}
				SetAnimState ();
			}

			return true;
		}


		public bool IsFadingIn ()
		{
			if (isFading && fadeType == FadeType.fadeIn)
			{
				return true;
			}
			return false;
		}


		public bool IsFadingOut ()
		{
			if (isFading && fadeType == FadeType.fadeOut)
			{
				return true;
			}
			return false;
		}
		
		
		public bool IsFading ()
		{
			return isFading;
		}
		
		
		public bool TurnOff (bool doFade)
		{
			if (IsOff ())
			{
				return false;
			}

			if (actionListOnTurnOff != null)
			{
				AdvGame.RunActionListAsset (actionListOnTurnOff);
			}
			
			if (appearType == AppearType.OnContainer)
			{
				KickStarter.playerInput.activeContainer = null;
			}

			if (transitionType == MenuTransition.None || fadeSpeed == 0f)
			{
				doFade = false;
			}
			if (isEnabled && (!isFading || (isFading && fadeType == FadeType.fadeIn)))// && appearType == AppearType.OnHotspot)))
			{
				isFading = doFade;
				
				if (doFade && fadeSpeed > 0f)
				{
					fadeType = FadeType.fadeOut;
					fadeStartTime = Time.realtimeSinceStartup - ((1f - transitionProgress) * fadeSpeed);
					SetAnimState ();
				}
				else
				{
					transitionProgress = 0f;
					UpdateTransition ();
					isFading = false;
					isEnabled = false;
					ReturnGameState ();
					DisableUI ();
					ClearSpeechText ();
				}
			}

			return true;
		}
		
		
		public void ForceOff ()
		{
			if (isEnabled || isFading)
			{
				transitionProgress = 0f;
				UpdateTransition ();
				isFading = false;
				isEnabled = false;
				DisableUI ();
				ClearSpeechText ();
			}
		}
		
		
		public void UpdateTransition ()
		{
			if (IsUnityUI ())
			{
				if (uiTransitionType == UITransition.CanvasGroupFade && canvas != null)
				{
					CanvasGroup canvasGroup = canvas.GetComponent <CanvasGroup>();
					canvasGroup.alpha = transitionProgress;
				}
				return;
			}

			if (transitionType == MenuTransition.Fade)
			{
				return;
			}
			
			if (transitionType == MenuTransition.FadeAndPan || transitionType == MenuTransition.Pan)
			{
				float amount = 0f;
				
				if (panMovement == PanMovement.Linear)
				{
					amount = (1f - transitionProgress) * panDistance;
				}
				if (panMovement == PanMovement.Smooth)
				{
					amount = ((transitionProgress * transitionProgress) - (2 * transitionProgress) + 1) * panDistance;
				}
				else if (panMovement == PanMovement.Overshoot)
				{
					amount = ((4f / 3f * transitionProgress * transitionProgress) - (7f / 3f * transitionProgress) + 1) * panDistance;
				}
				
				if (panDirection == PanDirection.Down)
				{
					panOffset = new Vector2 (0f, amount);
				}
				else if (panDirection == PanDirection.Left)
				{
					panOffset = new Vector2 (-amount, 0f);
				}
				else if (panDirection == PanDirection.Up)
				{
					panOffset = new Vector2 (0f, -amount);
				}
				else if (panDirection == PanDirection.Right)
				{
					panOffset = new Vector2 (amount, 0f);
				}
				
				panOffset = new Vector2 (panOffset.x * AdvGame.GetMainGameViewSize ().x, panOffset.y * AdvGame.GetMainGameViewSize ().y);
			}
			
			else if (transitionType == MenuTransition.Zoom)
			{
				zoomAmount = transitionProgress;
				
				if (zoomAnchor == TextAnchor.UpperLeft)
				{
					panOffset = Vector2.zero;
				}
				else if (zoomAnchor == TextAnchor.UpperCenter)
				{
					panOffset = new Vector2 ((1f - zoomAmount) * rect.width / 2f, 0f);
				}
				else if (zoomAnchor == TextAnchor.UpperRight)
				{
					panOffset = new Vector2 ((1f - zoomAmount) * rect.width, 0f);
				}
				else if (zoomAnchor == TextAnchor.MiddleLeft)
				{
					panOffset = new Vector2 (0f, (1f - zoomAmount) * rect.height / 2f);
				}
				else if (zoomAnchor == TextAnchor.MiddleCenter)
				{
					panOffset = new Vector2 ((1f - zoomAmount) * rect.width / 2f, (1f - zoomAmount) * rect.height / 2f);
				}
				else if (zoomAnchor == TextAnchor.MiddleRight)
				{
					panOffset = new Vector2 ((1f - zoomAmount) * rect.width, (1f - zoomAmount) * rect.height / 2f);
				}
				else if (zoomAnchor == TextAnchor.LowerLeft)
				{
					panOffset = new Vector2 (0, (1f - zoomAmount) * rect.height);
				}
				else if (zoomAnchor == TextAnchor.LowerCenter)
				{
					panOffset = new Vector2 ((1f - zoomAmount) * rect.width / 2f, (1f - zoomAmount) * rect.height);
				}
				else if (zoomAnchor == TextAnchor.LowerRight)
				{
					panOffset = new Vector2 ((1f - zoomAmount) * rect.width, (1f - zoomAmount) * rect.height);
				}
			}
		}
		
		
		private void ChangeGameState ()
		{
			if (IsBlocking () && Application.isPlaying)
			{
				if (appearType != AppearType.OnInteraction)
				{
					KickStarter.playerInteraction.DisableHotspot (true);
				}
				KickStarter.mainCamera.FadeIn (0);
				KickStarter.mainCamera.PauseGame ();
			}
		}
		
		
		private void ReturnGameState ()
		{
			if (IsBlocking () && !KickStarter.playerMenus.ArePauseMenusOn (this) && Application.isPlaying)
			{
				KickStarter.stateHandler.RestoreLastNonPausedState ();
			}
		}
		
		
		public bool IsBlocking ()
		{
			if (pauseWhenEnabled && IsManualControlled ())
			{
				return true;
			}
			return false;
		}


		public bool IsManualControlled ()
		{
			if (appearType == AppearType.Manual || appearType == AppearType.OnInputKey || appearType == AppearType.OnContainer)
			{
				return true;
			}
			return false;
		}
		
		
		public void MatchInteractions (List<Button> buttons, bool includeInventory)
		{
			foreach (MenuElement element in elements)
			{
				if (element is MenuInteraction)
				{
					MenuInteraction interaction = (MenuInteraction) element;
					interaction.MatchInteractions (buttons);
				}
				else if (element is MenuInventoryBox)
				{
					if (includeInventory)
					{
						element.RecalculateSize (menuSource);
						Recalculate ();
						element.AutoSetVisibility ();
					}
					else
					{
						element.isVisible = false;
					}
				}
			}
			
			Recalculate ();
			Recalculate ();
		}
		
		
		public void MatchInteractions (InvItem item, bool includeInventory)
		{
			foreach (MenuElement element in elements)
			{
				if (element is MenuInteraction)
				{
					MenuInteraction interaction = (MenuInteraction) element;
					interaction.MatchInteractions (item);
				}
				else if (element is MenuInventoryBox)
				{
					if (includeInventory)
					{
						element.RecalculateSize (menuSource);
						Recalculate ();
						element.AutoSetVisibility ();
					}
					else
					{
						element.isVisible = false;
					}
				}
			}
			
			Recalculate ();
			Recalculate ();
		}
		
		
		public void HideInteractions ()
		{
			foreach (MenuElement element in elements)
			{
				if (element is MenuInteraction)
				{
					element.isVisible = false;
					element.isClickable = false; // This function is only called for Context-Sensitive anyway
				}
			}
		}
		
		
		public void MatchLookInteraction (Button button)
		{
			foreach (MenuElement element in elements)
			{
				if (element is MenuInteraction)
				{
					MenuInteraction interaction = (MenuInteraction) element;
					interaction.MatchLookInteraction (KickStarter.cursorManager.lookCursor_ID);
				}
			}
		}
		
		
		public void MatchUseInteraction (Button button)
		{
			foreach (MenuElement element in elements)
			{
				if (element is MenuInteraction)
				{
					MenuInteraction interaction = (MenuInteraction) element;
					interaction.MatchUseInteraction (button);
				}
			}
		}
		
		
		public void SetDragOffset (Vector2 pos, Rect dragRect)
		{
			if (pos.x < dragRect.x)
			{
				pos.x = dragRect.x;
			}
			else if (pos.x > (dragRect.x + dragRect.width - GetRect ().width))
			{
				pos.x = dragRect.x + dragRect.width - GetRect ().width;
			}
			
			if (pos.y < dragRect.y)
			{
				pos.y = dragRect.y;
			}
			else if (pos.y > (dragRect.y + dragRect.height - GetRect ().height))
			{
				pos.y = dragRect.y + dragRect.height - GetRect ().height;
			}
			
			dragOffset = pos;
			
			UpdateAspectRect ();
		}
		
		
		public Vector2 GetDragStart ()
		{
			return dragOffset;
		}
		
		
		public float GetZoom ()
		{
			if (transitionType == MenuTransition.Zoom && zoomElements)
			{
				return zoomAmount;
			}
			
			else return 1f;
		}
		
		
		public int ControlSelected (int selected_option)
		{
			if (selected_slot == -2)
			{
				selected_option = 0;
			}
			
			if (selected_option < 0)
			{
				selected_option = 0;
				selected_element = visibleElements[0];
				selected_slot = 0;
			}
			else
			{
				int sel = 0;
				selected_slot = -1;
				int element = 0;
				int slot = 0;
				
				for (element=0; element<visibleElements.Count; element++)
				{
					if (visibleElements[element].isClickable)
					{
						for (slot=0; slot<visibleElements[element].GetNumSlots (); slot++)
						{
							if (selected_option == sel)
							{
								selected_slot = slot;
								selected_element = visibleElements[element];
								break;
							}
							sel++;
						}
					}
					
					if (selected_slot != -1)
					{
						break;
					}
				}
				
				if (selected_slot == -1)
				{
					// Couldn't find match, must've maxed out
					selected_slot = slot - 1;
					selected_element = visibleElements[element-1];
					selected_option = sel - 1;
				}
			}
			
			return selected_option;
		}
		
		
		public MenuElement GetElementWithName (string menuElementName)
		{
			foreach (MenuElement menuElement in elements)
			{
				if (menuElement.title == menuElementName)
				{
					return menuElement;
				}
			}
			
			return null;
		}
		
		
		public Vector2 GetSlotCentre (MenuElement _element, int slot)
		{
			foreach (MenuElement menuElement in elements)
			{
				if (menuElement == _element)
				{
					Rect slotRect = _element.GetSlotRectRelative (slot);
					return new Vector2 (GetRect ().x + slotRect.x + (slotRect.width / 2f), GetRect ().y + slotRect.y + (slotRect.height / 2f));
				}
			}
			
			return Vector2.zero;
		}


		private void ClearSpeechText ()
		{
			foreach (MenuElement element in elements)
			{
				element.ClearSpeech ();
			}
		}


		public void SetSpeech (Speech _speech)
		{
			speech = _speech;
			foreach (MenuElement element in elements)
			{
				element.SetSpeech (_speech);
			}
		}


		public GameObject GetObjectToSelect ()
		{
			if (firstSelectedElement == "")
			{
				return null;
			}

			foreach (MenuElement element in visibleElements)
			{
				if (element.title == firstSelectedElement)
				{
					return element.GetObjectToSelect ();
				}
			}
			return null;
		}

	}
	
}
