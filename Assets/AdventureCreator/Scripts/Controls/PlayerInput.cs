/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"PlayerInput.cs"
 * 
 *	This script records all input and processes it for other scripts.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{
	
	public class PlayerInput : MonoBehaviour
	{

		private AnimationCurve timeCurve;
		private float changeTimeStart;

		[HideInInspector] public MouseState mouseState = MouseState.Normal;
		[HideInInspector] public DragState dragState = DragState.None;
		[HideInInspector] public Hotspot hotspotMovingTo;
		[HideInInspector] public bool mouseOverMenu = false;
		[HideInInspector] public bool mouseOverInteractionMenu = false;
		[HideInInspector] public bool interactionMenuIsOn = false;
		
		[HideInInspector] public Vector2 moveKeys = new Vector2 (0f, 0f);
		[HideInInspector] public bool isRunning = false;
		[HideInInspector] public float timeScale = 1f;
		
		[HideInInspector] public bool isUpLocked = false;
		[HideInInspector] public bool isDownLocked = false;
		[HideInInspector] public bool isLeftLocked = false;
		[HideInInspector] public bool isRightLocked = false;
		[HideInInspector] public PlayerMoveLock runLock = PlayerMoveLock.Free;
		[HideInInspector] public bool freeAimLock = false;
		
		[HideInInspector] public int selected_option;
		[HideInInspector] public string skipMovieKey = "";
		
		public float clickDelay = 0.3f;
		public float doubleClickDelay = 1f;
		private float clickTime = 0f;
		private float doubleClickTime = 0;
		[HideInInspector] public MenuDrag activeDragElement;
		[HideInInspector] public bool hasUnclickedSinceClick = false;
		
		// Menu input override
		[HideInInspector] public string menuButtonInput;
		[HideInInspector] public float menuButtonValue;
		[HideInInspector] public SimulateInputType menuInput;
		
		// Controller movement
		private Vector2 xboxCursor;
		public float cursorMoveSpeed = 4f;
		[HideInInspector] public bool cameraLockSnap = false;
		private Vector2 mousePosition;
		private bool scrollingLocked = false;
		
		// Touch-Screen movement
		private Vector2 dragStartPosition = Vector2.zero;
		[HideInInspector] public float dragSpeed = 0f;
		private Vector2 dragVector;
		private float touchTime = 0f;
		private float touchThreshold = 0.2f;
		
		// 1st person movement
		[HideInInspector] public Vector2 freeAim;
		[HideInInspector] public bool cursorIsLocked = false;
		private bool toggleRun = false;
		
		// Draggable
		private bool canDragMoveable = false;
		private float cameraInfluence = 100000f;
		private DragBase dragObject = null;
		private Vector2 lastMousePosition;
		private Vector3 lastCameraPosition;
		private Vector3 dragForce;
		private Vector2 deltaDragMouse;

		[HideInInspector] public Conversation activeConversation = null;
		[HideInInspector] public int lastConversationOption = -1;
		[HideInInspector] public bool ignoreNextConversationSkip = false;
		[HideInInspector] public ArrowPrompt activeArrows = null;
		[HideInInspector] public Container activeContainer = null;
		[HideInInspector] public bool mouseIsOnScreen = true;
		

		private void Awake ()
		{
			if (KickStarter.settingsManager)
			{
				cursorIsLocked = KickStarter.settingsManager.lockCursorOnStart;
			}
		
			ResetClick ();
			
			xboxCursor.x = Screen.width / 2;
			xboxCursor.y = Screen.height / 2;
		}
		
		
		private void Start ()
		{
			if (KickStarter.settingsManager.movementMethod == MovementMethod.UltimateFPS)
			{
				UltimateFPSIntegration.SetCameraState (cursorIsLocked);
			}
			
			if (KickStarter.settingsManager.offsetTouchCursor)
			{
				mousePosition = xboxCursor;
			}
		}


		public void UpdateInput ()
		{
			if (timeCurve != null && timeCurve.length > 0)
			{
				float timeIndex = Time.time - changeTimeStart;
				if (timeCurve [timeCurve.length -1].time < timeIndex)
				{
					SetTimeScale (timeCurve [timeCurve.length -1].time);
					timeCurve = null;
				}
				else
				{
					SetTimeScale (timeCurve.Evaluate (timeIndex));
				}
			}

			if (clickTime > 0f)
			{
				clickTime -= 4f * GetDeltaTime ();
			}
			if (clickTime < 0f)
			{
				clickTime = 0f;
			}
			
			if (doubleClickTime > 0f)
			{
				doubleClickTime -= 4f * GetDeltaTime ();
			}
			if (doubleClickTime < 0f)
			{
				doubleClickTime = 0f;
			}

			if (skipMovieKey != "" && InputGetButtonDown (skipMovieKey))
			{
				skipMovieKey = "";
			}
			
			if (KickStarter.stateHandler && KickStarter.settingsManager)
			{
				try
				{
					if (InputGetButtonDown ("ToggleCursor") && KickStarter.stateHandler.gameState == GameState.Normal)
					{
						ToggleCursor ();
					}
				}
				catch
				{
					cursorIsLocked = false;
				}
				
				if (KickStarter.stateHandler.gameState == GameState.Cutscene && InputGetButtonDown ("EndCutscene"))
				{
					KickStarter.actionListManager.EndCutscene ();
				}

				#if UNITY_EDITOR
				if (KickStarter.settingsManager.inputMethod == InputMethod.MouseAndKeyboard || KickStarter.settingsManager.inputMethod == InputMethod.TouchScreen)
				#else
				if (KickStarter.settingsManager.inputMethod == InputMethod.MouseAndKeyboard)
				#endif
				{
					// Cursor position
					#if UNITY_5
					bool shouldLockCursor = false;
					if (Cursor.lockState == CursorLockMode.Locked)
					{
						shouldLockCursor = true;
					}
					#else
					bool shouldLockCursor = Screen.lockCursor;
					#endif

					if (!cursorIsLocked || KickStarter.stateHandler.gameState == AC.GameState.Paused || KickStarter.stateHandler.gameState == AC.GameState.DialogOptions || (freeAimLock && KickStarter.settingsManager.IsInFirstPerson ()))
					{
						if (shouldLockCursor)
						{
							shouldLockCursor = false;
						}
						mousePosition = InputMousePosition ();
						freeAim = Vector2.zero;
					}
					else if (dragObject != null && KickStarter.settingsManager.IsInFirstPerson () && KickStarter.settingsManager.disableFreeAimWhenDragging)
					{
						if (shouldLockCursor)
						{
							shouldLockCursor = false;
						}
						mousePosition = InputMousePosition ();
						freeAim = Vector2.zero;
					}
					else if (cursorIsLocked && KickStarter.stateHandler.gameState == GameState.Normal)
					{
						if (!shouldLockCursor && dragObject == null && KickStarter.settingsManager.IsInFirstPerson ())
						{
							shouldLockCursor = true;
						}
						mousePosition = new Vector2 (Screen.width / 2, Screen.height / 2);
						float sensitivity = 0.25f;
						freeAim = new Vector2 (InputGetAxis ("CursorHorizontal") * sensitivity, InputGetAxis ("CursorVertical") * sensitivity);
					}

					#if UNITY_5
					if (shouldLockCursor)
					{
						Cursor.lockState = CursorLockMode.Locked;
					}
					else
					{
						Cursor.lockState = CursorLockMode.None;
					}
					#else
					Screen.lockCursor = shouldLockCursor;
					#endif

					// Cursor state
					if (mouseState == MouseState.Normal)
					{
						dragState = DragState.None;
					}

					if (InputGetMouseButtonDown (0) || InputGetButtonDown ("InteractionA"))
					{
						if (mouseState == MouseState.Normal)
						{
							if (CanDoubleClick ())
							{
								mouseState = MouseState.DoubleClick;
								ResetClick ();
							}
							else if (CanClick ())
							{
								dragStartPosition = GetInvertedMouse ();
								
								mouseState = MouseState.SingleClick;
								ResetClick ();
								ResetDoubleClick ();
							}
						}
					}
					else if (InputGetMouseButtonDown (1) || InputGetButtonDown ("InteractionB"))
					{
						mouseState = MouseState.RightClick;
					}
					else if (InputGetMouseButton (0) || InputGetButton ("InteractionA"))
					{
						mouseState = MouseState.HeldDown;
						SetDragState ();
					}
					else
					{
						if (mouseState == MouseState.HeldDown && dragState == DragState.None && CanClick ())
						{
							mouseState = MouseState.LetGo;
						}
						else
						{
							mouseState = MouseState.Normal;
						}
					}
					
					if (KickStarter.settingsManager.inputMethod == InputMethod.TouchScreen)
					{
						if (dragState == DragState.Player)
						{
							if (KickStarter.settingsManager.IsFirstPersonDragMovement ())
							{
								freeAim = new Vector2 (dragVector.x * KickStarter.settingsManager.freeAimTouchSpeed, 0f);
							}
							else
							{
								freeAim = new Vector2 (dragVector.x * KickStarter.settingsManager.freeAimTouchSpeed, -dragVector.y * KickStarter.settingsManager.freeAimTouchSpeed);
							}
						}
						else
						{
							freeAim = Vector2.zero; //
						}
					}
				}
				else if (KickStarter.settingsManager.inputMethod == InputMethod.TouchScreen)
				{
					int touchCount = Input.touchCount;
					
					// Cursor position
					if (cursorIsLocked)
					{
						mousePosition = new Vector2 (Screen.width / 2f, Screen.height / 2f);
					}
					else if (touchCount > 0)
					{
						if (KickStarter.settingsManager.offsetTouchCursor)
						{
							if (touchTime > touchThreshold)
							{
								Touch t = Input.GetTouch (0);
								if (t.phase == TouchPhase.Moved && touchCount == 1)
								{
									if (KickStarter.stateHandler.gameState == GameState.Paused)
									{
										mousePosition += t.deltaPosition * 1.7f;
									}
									else
									{
										mousePosition += t.deltaPosition * Time.deltaTime / t.deltaTime;
									}
									
									if (mousePosition.x < 0f)
									{
										mousePosition.x = 0f;
									}
									else if (mousePosition.x > Screen.width)
									{
										mousePosition.x = Screen.width;
									}
									if (mousePosition.y < 0f)
									{
										mousePosition.y = 0f;
									}
									else if (mousePosition.y > Screen.height)
									{
										mousePosition.y = Screen.height;
									}
								}
							}
						}
						else
						{
							mousePosition = Input.GetTouch (0).position;
						}
					}
					
					// Cursor state
					if (mouseState == MouseState.Normal)
					{
						dragState = DragState.None;
					}
					
					if (touchTime > 0f && touchTime < touchThreshold)
						dragStartPosition = GetInvertedMouse ();
					
					if ((touchCount == 1 && KickStarter.stateHandler.gameState == GameState.Cutscene && Input.GetTouch (0).phase == TouchPhase.Began)
					    || (touchCount == 1 && !KickStarter.settingsManager.offsetTouchCursor && Input.GetTouch (0).phase == TouchPhase.Began)
					    || touchTime == -1f)
					{
						if (mouseState == MouseState.Normal)
						{
							if (CanDoubleClick ())
							{
								mouseState = MouseState.DoubleClick;
								ResetClick ();
							}
							else if (CanClick ())
							{
								dragStartPosition = GetInvertedMouse ();
								
								mouseState = MouseState.SingleClick;
								ResetClick ();
								ResetDoubleClick ();
							}
						}
					}
					else if (touchCount == 2 && Input.GetTouch (1).phase == TouchPhase.Began)
					{
						mouseState = MouseState.RightClick;
					}
					else if (touchCount == 1 && (Input.GetTouch (0).phase == TouchPhase.Stationary || Input.GetTouch (0).phase == TouchPhase.Moved))
					{
						mouseState = MouseState.HeldDown;
						SetDragState ();
					}
					else
					{
						if (mouseState == MouseState.HeldDown && dragState == DragState.None && CanClick ())
						{
							mouseState = MouseState.LetGo;
						}
						else
						{
							mouseState = MouseState.Normal;
						}
					}
					
					if (KickStarter.settingsManager.offsetTouchCursor)
					{
						if (touchCount > 0)
						{
							touchTime += GetDeltaTime ();
						}
						else
						{
							if (touchTime > 0f && touchTime < touchThreshold)
							{
								touchTime = -1f;
							}
							else
							{
								touchTime = 0f;
							}
						}
					}
					
					if (dragState == DragState.Player)
					{
						if (KickStarter.settingsManager.IsFirstPersonDragMovement ())
						{
							freeAim = new Vector2 (dragVector.x * KickStarter.settingsManager.freeAimTouchSpeed, 0f);
						}
						else
						{
							freeAim = new Vector2 (dragVector.x * KickStarter.settingsManager.freeAimTouchSpeed, -dragVector.y * KickStarter.settingsManager.freeAimTouchSpeed);
						}
					}
					else
					{
						freeAim = Vector2.zero; //
					}
				}
				else if (KickStarter.settingsManager.inputMethod == InputMethod.KeyboardOrController)
				{
					// Cursor position
					if (cursorIsLocked && KickStarter.stateHandler.gameState == GameState.Normal)
					{
						mousePosition = new Vector2 (Screen.width / 2, Screen.height / 2);
						freeAim = new Vector2 (InputGetAxis ("CursorHorizontal") * 50f, InputGetAxis ("CursorVertical") * 50f);
					}
					else
					{
						xboxCursor.x += InputGetAxis ("CursorHorizontal") * cursorMoveSpeed / Screen.width * 5000f;
						xboxCursor.y += InputGetAxis ("CursorVertical") * cursorMoveSpeed / Screen.height * 5000f;

						xboxCursor.x = Mathf.Clamp (xboxCursor.x, 0f, Screen.width);
						xboxCursor.y = Mathf.Clamp (xboxCursor.y, 0f, Screen.height);
						
						mousePosition = xboxCursor;
						freeAim = Vector2.zero;
					}
					
					// Cursor state
					if (mouseState == MouseState.Normal)
					{
						dragState = DragState.None;
					}
					
					if (InputGetButtonDown ("InteractionA"))
					{
						if (mouseState == MouseState.Normal)
						{
							if (CanDoubleClick ())
							{
								mouseState = MouseState.DoubleClick;
								ResetClick ();
							}
							else if (CanClick ())
							{
								dragStartPosition = GetInvertedMouse ();
								
								mouseState = MouseState.SingleClick;
								ResetClick ();
								ResetDoubleClick ();
							}
						}
					}
					else if (InputGetButtonDown ("InteractionB"))
					{
						mouseState = MouseState.RightClick;
					}
					else if (InputGetButton ("InteractionA"))
					{
						mouseState = MouseState.HeldDown;
						SetDragState ();
					}
					else
					{
						mouseState = MouseState.Normal;
					}
					
					// Menu option changing
					if (KickStarter.stateHandler.gameState == GameState.DialogOptions || KickStarter.stateHandler.gameState == GameState.Paused)
					{
						if (!scrollingLocked)
						{
							if (InputGetAxisRaw ("Vertical") > 0.1 || InputGetAxisRaw ("Horizontal") < -0.1)
							{
								// Up / Left
								scrollingLocked = true;
								selected_option --;
							}
							else if (InputGetAxisRaw ("Vertical") < -0.1 || InputGetAxisRaw ("Horizontal") > 0.1)
							{
								// Down / Right
								scrollingLocked = true;
								selected_option ++;
							}
						}
						else if (InputGetAxisRaw ("Vertical") < 0.05 && InputGetAxisRaw ("Vertical") > -0.05 && InputGetAxisRaw ("Horizontal") < 0.05 && InputGetAxisRaw ("Horizontal") > -0.05)
						{
							scrollingLocked = false;
						}
					}
				}
				
				if (hotspotMovingTo != null)
				{
					freeAim = Vector2.zero;
				}

				if (KickStarter.stateHandler.gameState == GameState.Normal)
				{
					DetectCursorInputs ();
				}

				if (KickStarter.settingsManager.SelectInteractionMethod () == SelectInteractions.CyclingMenuAndClickingHotspot && interactionMenuIsOn)
				{
					try
					{
						if (InputGetButtonDown ("CycleInteractionsRight"))
						{
							KickStarter.playerInteraction.SetNextInteraction ();
						}
						else if (InputGetButtonDown ("CycleInteractionsLeft"))
						{
							KickStarter.playerInteraction.SetPreviousInteraction ();
						}
						else if (InputGetAxis ("CycleInteractions") > 0.1f)
						{
							KickStarter.playerInteraction.SetNextInteraction ();
						}
						else if (InputGetAxis ("CycleInteractions") < -0.1f)
						{
							KickStarter.playerInteraction.SetPreviousInteraction ();
						}
					}
					catch {}
				}
				
				mousePosition = KickStarter.mainCamera.LimitMouseToAspect (mousePosition);
				
				if (mouseState == MouseState.Normal && !hasUnclickedSinceClick)
				{
					hasUnclickedSinceClick = true;
				}
				
				if (mouseState == MouseState.Normal)
				{
					canDragMoveable = true;
				}
				
				UpdateDrag ();
				
				if (dragState != DragState.None)
				{
					dragVector = GetInvertedMouse () - dragStartPosition;
					dragSpeed = dragVector.magnitude;
				}
				else
				{
					dragSpeed = 0f;
				}

				UpdateActiveInputs ();

				if (mousePosition.x < 0f || mousePosition.x > Screen.width || mousePosition.y < 0f || mousePosition.y > Screen.height)
				{
					mouseIsOnScreen = false;
				}
				else
				{
					mouseIsOnScreen = true;
				}
			}
		}


		private void UpdateActiveInputs ()
		{
			if (KickStarter.settingsManager.activeInputs != null)
			{
				foreach (ActiveInput activeInput in KickStarter.settingsManager.activeInputs)
				{
					if (InputGetButtonDown (activeInput.inputName))
					{
						if (KickStarter.stateHandler.gameState == activeInput.gameState && activeInput.actionListAsset != null && !KickStarter.actionListManager.IsListRunning (activeInput.actionListAsset))
						{
							AdvGame.RunActionListAsset (activeInput.actionListAsset);
						}
					}
				}
			}
		}


		private void DetectCursorInputs ()
		{
			if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot)
			{
				if (KickStarter.cursorManager.allowWalkCursor)
				{
					if (InputGetButtonDown ("Icon_Walk"))
					{
						KickStarter.runtimeInventory.SetNull ();
						KickStarter.playerCursor.ResetSelectedCursor ();
						return;
					}
				}

				foreach (CursorIcon icon in KickStarter.cursorManager.cursorIcons)
				{
					if (InputGetButtonDown (icon.GetButtonName ()))
					{
						KickStarter.runtimeInventory.SetNull ();
						KickStarter.playerCursor.SetCursor (icon);
						return;
					}
				}
			}
		}
		
		
		public Vector2 GetMousePosition ()
		{
			return mousePosition;
		}
		
		
		public Vector2 GetInvertedMouse ()
		{
			return new Vector2 (GetMousePosition ().x, Screen.height - GetMousePosition ().y);
		}
		
		
		public bool IsCursorReadable ()
		{
			if (KickStarter.settingsManager.inputMethod == InputMethod.TouchScreen)
			{
				if (mouseState == MouseState.Normal)
				{
					if (KickStarter.runtimeInventory.selectedItem != null && KickStarter.settingsManager.inventoryDragDrop)
					{
						return true;
					}
					
					return KickStarter.settingsManager.offsetTouchCursor;
				}
			}
			return true;
		}


		public void DetectNumerics ()
		{		
			if (activeConversation != null && KickStarter.settingsManager.runConversationsWithKeys)
			{
				Event e = Event.current;
				if (e.isKey && e.type == EventType.KeyDown)
				{
					if (e.keyCode == KeyCode.Alpha1 || e.keyCode == KeyCode.Keypad1)
					{
						activeConversation.RunOption (0);
					}
					else if (e.keyCode == KeyCode.Alpha2 || e.keyCode == KeyCode.Keypad2)
					{
						activeConversation.RunOption (1);
					}
					else if (e.keyCode == KeyCode.Alpha3 || e.keyCode == KeyCode.Keypad3)
					{
						activeConversation.RunOption (2);
					}
					else if (e.keyCode == KeyCode.Alpha4 || e.keyCode == KeyCode.Keypad4)
					{
						activeConversation.RunOption (3);
					}
					else if (e.keyCode == KeyCode.Alpha5 || e.keyCode == KeyCode.Keypad5)
					{
						activeConversation.RunOption (4);
					}
					else if (e.keyCode == KeyCode.Alpha6 || e.keyCode == KeyCode.Keypad6)
					{
						activeConversation.RunOption (5);
					}
					else if (e.keyCode == KeyCode.Alpha7 || e.keyCode == KeyCode.Keypad7)
					{
						activeConversation.RunOption (6);
					}
					else if (e.keyCode == KeyCode.Alpha8 || e.keyCode == KeyCode.Keypad8)
					{
						activeConversation.RunOption (7);
					}
					else if (e.keyCode == KeyCode.Alpha9 || e.keyCode == KeyCode.Keypad9)
					{
						activeConversation.RunOption (8);
					}
				}
			}

		}
		
		
		public void DrawDragLine ()
		{
			if (dragState == DragState.Player && KickStarter.settingsManager.movementMethod != MovementMethod.StraightToCursor && KickStarter.settingsManager.drawDragLine)
			{
				Vector2 pointA = dragStartPosition;
				Vector2 pointB = GetInvertedMouse ();
				
				if (pointB.x >= 0f)
				{
					DrawStraightLine.Draw (pointA, pointB, KickStarter.settingsManager.dragLineColor, KickStarter.settingsManager.dragLineWidth, true);
				}
			}
			
			if (activeDragElement != null)
			{
				if (mouseState == MouseState.HeldDown)
				{
					if (!activeDragElement.DoDrag (GetDragVector ()))
					{
						activeDragElement = null;
					}
				}
				else if (mouseState == MouseState.Normal)
				{
					if (activeDragElement.CheckStop (GetInvertedMouse ()))
					{
						activeDragElement = null;
					}
				}
			}
		}
		
		
		public void UpdateDirectInput ()
		{
			if (KickStarter.settingsManager != null)
			{
				if (activeArrows != null)
				{
					if (activeArrows.arrowPromptType == ArrowPromptType.KeyOnly || activeArrows.arrowPromptType == ArrowPromptType.KeyAndClick)
					{
						Vector2 normalizedVector = new Vector2 (InputGetAxis ("Horizontal"), InputGetAxis ("Vertical"));

						if (normalizedVector.magnitude > 0f)
						{
							if (KickStarter.settingsManager.inputMethod == InputMethod.TouchScreen && dragState == DragState.ScreenArrows)
							{
								normalizedVector = GetDragVector () / KickStarter.settingsManager.dragRunThreshold / KickStarter.settingsManager.dragWalkThreshold;
							}

							float threshold = 0.95f;
							if (KickStarter.settingsManager.inputMethod == InputMethod.MouseAndKeyboard)
							{
								threshold = 0.05f;
							}

							if (normalizedVector.x > threshold)
							{
								activeArrows.DoRight ();
							}
							else if (normalizedVector.x < -threshold)
							{
								activeArrows.DoLeft ();
							}
							else if (normalizedVector.y < -threshold)
							{
								activeArrows.DoDown();
							}
							else if (normalizedVector.y > threshold)
							{
								activeArrows.DoUp ();
							}
						}
					}
					
					if (activeArrows != null && (activeArrows.arrowPromptType == ArrowPromptType.ClickOnly || activeArrows.arrowPromptType == ArrowPromptType.KeyAndClick))
					{
						// Arrow Prompt is displayed: respond to mouse clicks
						Vector2 invertedMouse = GetInvertedMouse ();
						if (mouseState == MouseState.SingleClick)
						{
							if (activeArrows.upArrow.rect.Contains (invertedMouse))
							{
								activeArrows.DoUp ();
							}
							
							else if (activeArrows.downArrow.rect.Contains (invertedMouse))
							{
								activeArrows.DoDown ();
							}
							
							else if (activeArrows.leftArrow.rect.Contains (invertedMouse))
							{
								activeArrows.DoLeft ();
							}
							
							else if (activeArrows.rightArrow.rect.Contains (invertedMouse))
							{
								activeArrows.DoRight ();
							}
						}
					}
				}
				
				if (activeArrows == null && KickStarter.settingsManager.movementMethod != MovementMethod.PointAndClick)
				{
					float h = 0f;
					float v = 0f;
					bool run;
					
					if (KickStarter.settingsManager.inputMethod == InputMethod.TouchScreen || KickStarter.settingsManager.movementMethod == MovementMethod.Drag)
					{
						if (dragState != DragState.None)
						{
							h = dragVector.x;
							v = -dragVector.y;
						}
					}
					else
					{
						h = InputGetAxis ("Horizontal");
						v = InputGetAxis ("Vertical");
					}
					try
					{
						if (InputGetButtonDown ("Jump") && KickStarter.stateHandler.gameState == GameState.Normal && KickStarter.settingsManager.movementMethod != MovementMethod.UltimateFPS)
						{
							KickStarter.player.Jump ();
						}
					}
					catch
					{}
					
					if ((isUpLocked && v > 0f) || (isDownLocked && v < 0f))
					{
						v = 0f;
					}
					
					if ((isLeftLocked && h > 0f) || (isRightLocked && h < 0f))
					{
						h = 0f;
					}
					
					if (runLock == PlayerMoveLock.Free)
					{
						if (KickStarter.settingsManager.inputMethod == InputMethod.TouchScreen || KickStarter.settingsManager.movementMethod == MovementMethod.Drag)
						{
							if (dragStartPosition != Vector2.zero && dragSpeed > KickStarter.settingsManager.dragRunThreshold * 10f)
							{
								run = true;
							}
							else
							{
								run = false;
							}
						}
						else
						{
							try
							{
								if (Input.GetAxis ("Run") > 0.1f)
								{
									run = true;
								}
								else
								{
									run = InputGetButton ("Run");
								}
							}
							catch
							{
								run = InputGetButton ("Run");
							}

							try
							{
								if (InputGetButtonDown ("ToggleRun"))
								{
									toggleRun = !toggleRun;
								}
							}
							catch
							{
								toggleRun = false;
							}
						}
					}
					else if (runLock == PlayerMoveLock.AlwaysWalk)
					{
						run = false;
					}
					else
					{
						run = true;
					}
					
					if (KickStarter.settingsManager.inputMethod != InputMethod.TouchScreen && (KickStarter.settingsManager.movementMethod == MovementMethod.FirstPerson || KickStarter.settingsManager.movementMethod == MovementMethod.Direct) && runLock == PlayerMoveLock.Free && toggleRun)
					{
						isRunning = !run;
					}
					else
					{
						isRunning = run;
					}

					moveKeys = CreateMoveKeys (h, v);
				}
				
				if (InputGetButtonDown ("FlashHotspots"))
				{
					FlashHotspots ();
				}
			}
		}


		private Vector2 CreateMoveKeys (float h, float v)
		{
			if (KickStarter.settingsManager.movementMethod == MovementMethod.Direct && KickStarter.settingsManager.inputMethod != InputMethod.TouchScreen && KickStarter.settingsManager.directMovementType == DirectMovementType.RelativeToCamera)
			{
				if (KickStarter.settingsManager.limitDirectMovement == LimitDirectMovement.FourDirections)
				{
					if (Mathf.Abs (h) > Mathf.Abs (v))
					{
						v = 0f;
					}
					else
					{
						h = 0f;
					}
				}
				else if (KickStarter.settingsManager.limitDirectMovement == LimitDirectMovement.EightDirections)
				{
					if (Mathf.Abs (h) > Mathf.Abs (v))
					{
						v = 0f;
					}
					else if (Mathf.Abs (h) < Mathf.Abs (v))
					{
						h = 0f;
					}
					else if (Mathf.Abs (h) > 0.4f && Mathf.Abs (v) > 0.4f)
					{
						if (h*v > 0)
						{
							h = v;
						}
						else
						{
							h = -v;
						}
					}
					else
					{
						h = v = 0f;
					}
				}
			}

			if (cameraLockSnap)
			{
				Vector2 newMoveKeys = new Vector2 (h, v);
				if (newMoveKeys.magnitude < 0.1f || Vector2.Angle (newMoveKeys, moveKeys) > 5f)
				{
					cameraLockSnap = false;
				}
				return newMoveKeys;
			}

			return new Vector2 (h, v);
		}
		
		
		private void FlashHotspots ()
		{
			Hotspot[] hotspots = FindObjectsOfType (typeof (Hotspot)) as Hotspot[];
			
			foreach (Hotspot hotspot in hotspots)
			{
				if (hotspot.IsOn () && hotspot.highlight && hotspot != KickStarter.playerInteraction.GetActiveHotspot ())
				{
					hotspot.highlight.Flash ();
				}
			}
		}
		
		
		public void RemoveActiveArrows ()
		{
			if (activeArrows)
			{
				activeArrows.TurnOff ();
			}
		}
		
		
		public void ResetClick ()
		{
			clickTime = clickDelay;
			hasUnclickedSinceClick = false;
		}
		
		
		private void ResetDoubleClick ()
		{
			doubleClickTime = doubleClickDelay;
		}
		
		
		public bool CanClick ()
		{
			if (clickTime == 0f)
			{
				return true;
			}
			
			return false;
		}
		
		
		public bool CanDoubleClick ()
		{
			if (doubleClickTime > 0f && clickTime == 0f)
			{
				return true;
			}
			
			return false;
		}


		public void SimulateInputButton (string button)
		{
			SimulateInput (SimulateInputType.Button, button, 1f);
		}
		
		
		public void SimulateInputAxis (string axis, float val)
		{
			SimulateInput (SimulateInputType.Axis, axis, val);
		}
		
		
		public void SimulateInput (SimulateInputType input, string axis, float value)
		{
			if (axis != "")
			{
				menuInput = input;
				menuButtonInput = axis;
				
				if (input == SimulateInputType.Button)
				{
					menuButtonValue = 1f;
				}
				else
				{
					menuButtonValue = value;
				}
				
				CancelInvoke ();
				Invoke ("StopSimulatingInput", 0.1f);
			}
		}
		
		
		public bool IsCursorLocked ()
		{
			#if UNITY_5
			if (Cursor.lockState == CursorLockMode.Locked)
			{
				return true;
			}
			else
			{
				return false;
			}
			#else
			return Screen.lockCursor;
			#endif
		}
		
		
		private void StopSimulatingInput ()
		{
			menuButtonInput = "";
		}
		
		
		private float InputGetAxisRaw (string axis)
		{
			try
			{
				if (KickStarter.settingsManager.useOuya)
				{
					if (OuyaIntegration.GetAxisRaw (axis) != 0f)
					{
						return OuyaIntegration.GetAxisRaw (axis);
					}
				}
				
				else if (Input.GetAxisRaw (axis) != 0f)
				{
					return Input.GetAxisRaw (axis);
				}
			}
			catch {}
			
			if (menuButtonInput != "" && menuButtonInput == axis && menuInput == SimulateInputType.Axis)
			{
				return menuButtonValue;
			}
			
			return 0f;
		}
		
		
		public float InputGetAxis (string axis)
		{
			try
			{
				if (KickStarter.settingsManager.useOuya)
				{
					if (OuyaIntegration.GetAxis (axis) != 0f)
					{
						return OuyaIntegration.GetAxis (axis);
					}
				}
				
				else if (Input.GetAxis (axis) != 0f)
				{
					return Input.GetAxis (axis);
				}
			}
			catch {}
			
			if (menuButtonInput != "" && menuButtonInput == axis && menuInput == SimulateInputType.Axis)
			{
				return menuButtonValue;
			}
			
			return 0f;
		}
		
		
		private bool InputGetMouseButton (int button)
		{
			if (KickStarter.settingsManager.useOuya)
			{
				return OuyaIntegration.GetMouseButton (button);
			}
			return Input.GetMouseButton (button);
		}
		
		
		private Vector2 InputMousePosition ()
		{
			if (KickStarter.settingsManager.useOuya)
			{
				return OuyaIntegration.mousePosition;
			}
			return Input.mousePosition;
		}
		
		
		private bool InputGetMouseButtonDown (int button)
		{
			if (KickStarter.settingsManager.useOuya)
			{
				return OuyaIntegration.GetMouseButtonDown (button);
			}
			return Input.GetMouseButtonDown (button);
		}
		
		
		public bool InputGetButton (string axis)
		{
			try
			{
				if (KickStarter.settingsManager.useOuya)
				{
					if (OuyaIntegration.GetButton (axis))
					{
						return true;
					}
				}
				
				else if (Input.GetButton (axis))
				{
					return true;
				}
			}
			catch {}
			return false;
		}
		
		
		public bool InputGetButtonDown (string axis, bool showError = false)
		{
			if (axis == "")
			{
				return false;
			}

			try
			{
				if (KickStarter.settingsManager.useOuya)
				{
					if (OuyaIntegration.GetButtonDown (axis))
					{
						return true;
					}
				}
				
				else if (Input.GetButtonDown (axis))
				{
					return true;
				}
			}
			catch
			{
				if (showError)
				{
					Debug.LogError ("Cannot find Input button '" + axis + "' - please define it in Unity's Input Manager (Edit -> Project settings -> Input).");
				}
			}
			
			if (menuButtonInput != "" && menuButtonInput == axis && menuInput == SimulateInputType.Button)
			{
				if (menuButtonValue > 0f)
				{
					ResetClick ();
					StopSimulatingInput ();	
					return true;
				}
				
				StopSimulatingInput ();
			}
			
			return false;
		}


		public bool InputGetButtonUp (string axis)
		{
			if (axis == "")
			{
				return false;
			}
			
			try
			{
				if (Input.GetButtonUp (axis))
				{
					return true;
				}
			}
			catch {}
			return false;
		}
		
		
		private void SetDragState ()
		{
			if (KickStarter.runtimeInventory.selectedItem != null && KickStarter.settingsManager.inventoryDragDrop && (KickStarter.stateHandler.gameState == GameState.Normal || KickStarter.stateHandler.gameState == GameState.Paused))
			{
				dragState = DragState.Inventory;
			}
			else if (activeDragElement != null && (KickStarter.stateHandler.gameState == GameState.Normal || KickStarter.stateHandler.gameState == GameState.Paused))
			{
				dragState = DragState.Menu;
			}
			else if (activeArrows != null && KickStarter.settingsManager.inputMethod == InputMethod.TouchScreen)
			{
				dragState = DragState.ScreenArrows;
			}
			else if (dragObject != null)
			{
				dragState = DragState.Moveable;
			}
			else if (KickStarter.mainCamera.attachedCamera && KickStarter.mainCamera.attachedCamera.isDragControlled)
			{
				if (!KickStarter.playerInteraction.IsMouseOverHotspot ())
				{
					dragState = DragState._Camera;
					if (deltaDragMouse.magnitude * Time.deltaTime <= 1f && (GetInvertedMouse () - dragStartPosition).magnitude < 10f)
					{
						dragState = DragState.None;
					}
				}
			}
			else if ((KickStarter.settingsManager.movementMethod == MovementMethod.Drag || KickStarter.settingsManager.movementMethod == MovementMethod.StraightToCursor || (KickStarter.settingsManager.movementMethod != MovementMethod.PointAndClick && KickStarter.settingsManager.inputMethod == InputMethod.TouchScreen))
			         && KickStarter.stateHandler.gameState == GameState.Normal)
			{
				if (!mouseOverMenu && !interactionMenuIsOn)
				{
					if (KickStarter.playerInteraction.IsMouseOverHotspot ())
					{}
					else
					{
						dragState = DragState.Player;
					}
				}
			}
			else
			{
				dragState = DragState.None;
			}
		}
		
		
		private void UpdateDrag ()
		{
			if (dragState != DragState.None)
			{
				// Calculate change in mouse position
				if (freeAim.magnitude != 0f)
				{
					deltaDragMouse = freeAim * 500f / Time.deltaTime;
				}
				else
				{
					deltaDragMouse = ((Vector2) mousePosition - lastMousePosition) / Time.deltaTime;
				}
			}
			
			if (dragObject && KickStarter.stateHandler.gameState != GameState.Normal)
			{
				LetGo (false);
			}
			
			if (mouseState == MouseState.HeldDown && dragState == DragState.None)
			{
				Grab ();
			}
			else if (dragState == DragState.Moveable)
			{
				if (dragObject)
				{
					if (dragObject.isHeld && dragObject.IsOnScreen () && dragObject.IsCloseToCamera (KickStarter.settingsManager.moveableRaycastLength))
					{
						Drag ();
					}
					else
					{
						LetGo (true);
					}
				}
			}
			else if (dragObject)
			{
				LetGo (true);
			}
			
			if (dragState != DragState.None)
			{
				lastMousePosition = mousePosition;
			}
		}


		public void SetFreeAimLock (bool _state)
		{
			freeAimLock = _state;

			if (KickStarter.settingsManager.movementMethod == MovementMethod.UltimateFPS)
			{
				UltimateFPSIntegration.SetCameraState (!_state);
			}
		}


		private void LetGo (bool unlockFPSCamera)
		{
			dragObject.LetGo ();
			dragObject = null;
			
			if (unlockFPSCamera && KickStarter.settingsManager.movementMethod == MovementMethod.UltimateFPS && KickStarter.settingsManager.disableFreeAimWhenDragging)
			{
				UltimateFPSIntegration.SetCameraState (true);
			}
		}
		
		
		private void Grab ()
		{
			if (dragObject)
			{
				dragObject.LetGo ();
				dragObject = null;
			}
			else if (canDragMoveable)
			{
				canDragMoveable = false;
				
				Ray ray = Camera.main.ScreenPointToRay (mousePosition); 
				RaycastHit hit = new RaycastHit ();
				
				if (Physics.Raycast (ray, out hit, KickStarter.settingsManager.moveableRaycastLength))
				{
					if (hit.transform.GetComponent <DragBase>())
					{
						dragObject = hit.transform.GetComponent <DragBase>();
						dragObject.Grab (hit.point);
						lastMousePosition = mousePosition;
						lastCameraPosition = KickStarter.mainCamera.transform.position;
					}
				}

				if (KickStarter.settingsManager.movementMethod == MovementMethod.UltimateFPS && KickStarter.settingsManager.disableFreeAimWhenDragging)
				{
					UltimateFPSIntegration.SetCameraState (false);
				}
			}
		}
		
		
		private void Drag ()
		{
			// Convert to a 3D force
			if (dragObject.invertInput)
			{
				dragForce = (-KickStarter.mainCamera.transform.right * deltaDragMouse.x) + (-KickStarter.mainCamera.transform.up * deltaDragMouse.y);
			}
			else
			{
				dragForce = (KickStarter.mainCamera.transform.right * deltaDragMouse.x) + (KickStarter.mainCamera.transform.up * deltaDragMouse.y);
			}
			
			// Scale force with distance to camera, to lessen effects when close
			float distanceToCamera = (KickStarter.mainCamera.transform.position - dragObject.transform.position).magnitude;
			
			// Incoporate camera movement
			Vector3 deltaCamera = KickStarter.mainCamera.transform.position - lastCameraPosition;
			dragForce += deltaCamera * cameraInfluence;
			
			dragObject.ApplyDragForce (dragForce, mousePosition, distanceToCamera);
			
			lastCameraPosition = KickStarter.mainCamera.transform.position;
		}
		
		
		public Vector2 GetDragVector ()
		{
			if (dragState == AC.DragState._Camera)
			{
				return deltaDragMouse;
			}
			return dragVector;
		}
		
		
		public void SetUpLock (bool state)
		{
			isUpLocked = state;
			
			if (KickStarter.settingsManager.movementMethod == MovementMethod.UltimateFPS)
			{
				UltimateFPSIntegration.SetMovementState (state);
			}
		}
		
		
		public bool ActiveArrowsDisablingHotspots ()
		{
			if (activeArrows != null && activeArrows.disableHotspots)
			{
				return true;
			}
			return false;
		}
		
		
		public void StartRotatingObject ()
		{
			if (cursorIsLocked)
			{
				ToggleCursor ();
			}
		}
		
		
		private void ToggleCursor ()
		{
			cursorIsLocked = !cursorIsLocked;

			if (KickStarter.settingsManager.movementMethod == MovementMethod.UltimateFPS)
			{
				UltimateFPSIntegration.SetCameraState (cursorIsLocked);
			}
		}
		
		
		public bool IsDragObjectHeld (DragBase _dragBase)
		{
			if (_dragBase == null || dragObject == null)
			{
				return false;
			}
			if (_dragBase == dragObject)
			{
				return true;
			}
			return false;
		}
		
		
		private float GetDeltaTime ()
		{
			if (Time.deltaTime == 0f)
			{
				return 0.02f;
			}
			return Time.deltaTime;
		}


		public void SetTimeScale (float _timeScale)
		{
			if (_timeScale > 0f)
			{
				timeScale = _timeScale;
				if (KickStarter.stateHandler.gameState != GameState.Paused)
				{
					Time.timeScale = _timeScale;
				}
			}
		}


		public void SetTimeCurve (AnimationCurve _timeCurve)
		{
			timeCurve = _timeCurve;
			changeTimeStart = Time.time;
		}


		public bool HasTimeCurve ()
		{
			if (timeCurve != null)
			{
				return true;
			}
			return false;
		}

	}
	
}

