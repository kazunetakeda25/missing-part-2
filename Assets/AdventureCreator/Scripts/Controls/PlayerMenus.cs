/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"PlayerMenus.cs"
 * 
 *	This script handles the displaying of each of the menus defined in MenuSystem.
 *	It avoids referencing specific menus and menu elements as much as possible,
 *	so that the menu can be completely altered using just the MenuSystem script.
 * 
 */

using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

namespace AC
{
	
	public class PlayerMenus : MonoBehaviour
	{

		[HideInInspector] public bool lockSave = false;

		private bool foundMouseOverMenu = false;
		private bool foundMouseOverInteractionMenu = false;
		private bool foundMouseOverInventory = false;
		private bool mouseOverInventory = false;

		private bool isPaused;
		private string hotspotLabel = "";
		private float pauseAlpha = 0f;
		private List<Menu> menus = new List<Menu>();
		private List<Menu> dupMenus = new List<Menu>();
		private Texture2D pauseTexture;
		private string elementIdentifier;
		private string lastElementIdentifier;
		private MenuInput selectedInputBox;
		private string selectedInputBoxMenuName;
		private Vector2 activeInventoryBoxCentre = Vector2.zero;
		private InvItem oldHoverItem;
		private int doResizeMenus = 0;
		
		private Menu crossFadeTo;
		private Menu crossFadeFrom;
		private EventSystem eventSystem;

		private GUIStyle normalStyle = new GUIStyle ();
		private GUIStyle highlightedStyle = new GUIStyle();
		
		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		private TouchScreenKeyboard keyboard;
		#endif

		
		private void Start ()
		{
			menus = new List<Menu>();
		
			if (KickStarter.menuManager)
			{
				pauseTexture = KickStarter.menuManager.pauseTexture;

				foreach (AC.Menu _menu in KickStarter.menuManager.menus)
				{
					Menu newMenu = ScriptableObject.CreateInstance <Menu>();
					newMenu.Copy (_menu);

					if (_menu.appearType == AppearType.WhenSpeechPlays && _menu.oneMenuPerSpeech)
					{
						// Don't make canvas object yet!
					}
					else if (newMenu.IsUnityUI ())
					{
						newMenu.LoadUnityUI ();
					}
					newMenu.Initalise ();
					menus.Add (newMenu);
				}
			}

			CreateEventSystem ();

			foreach (AC.Menu menu in menus)
			{
				menu.Recalculate ();
			}
			
			#if UNITY_WEBPLAYER && !UNITY_EDITOR
			// WebPlayer takes another second to get the correct screen dimensions
			foreach (AC.Menu menu in menus)
			{
				menu.Recalculate ();
			}
			#endif
		}


		private void CreateEventSystem ()
		{
			if (GameObject.FindObjectOfType <EventSystem>() == null)
			{
				EventSystem _eventSystem = null;

				if (KickStarter.menuManager.eventSystem != null)
				{
					_eventSystem = (EventSystem) Instantiate (KickStarter.menuManager.eventSystem);
					_eventSystem.gameObject.name = KickStarter.menuManager.eventSystem.name;
				}
				else if (AreAnyMenusUI ())
				{
					GameObject eventSystemObject = new GameObject ();
					eventSystemObject.name = "EventSystem";
					_eventSystem = eventSystemObject.AddComponent <EventSystem>();
					eventSystemObject.AddComponent <StandaloneInputModule>();
					eventSystemObject.AddComponent <TouchInputModule>();
				}

				if (_eventSystem != null)
				{
					if (GameObject.Find ("_UI"))
					{
						_eventSystem.transform.SetParent (GameObject.Find ("_UI").transform);
					}
					eventSystem = _eventSystem;
				}
			}
		}


		private bool AreAnyMenusUI ()
		{
			foreach (AC.Menu menu in menus)
			{
				if (menu.menuSource == MenuSource.UnityUiInScene || menu.menuSource == MenuSource.UnityUiPrefab)
				{
					return true;
				}
			}
			return false;
		}
		
		
		private void OnLevelWasLoaded ()
		{
			if (KickStarter.settingsManager != null && KickStarter.settingsManager.IsInLoadingScene ())
			{
				return;
			}

			CreateEventSystem ();

			foreach (AC.Menu _menu in menus)
			{
				if (_menu.menuSource == MenuSource.UnityUiInScene)
				{
					_menu.LoadUnityUI ();
					_menu.Initalise ();
				}
				else if (_menu.menuSource == MenuSource.UnityUiPrefab)
				{
					_menu.SetParent ();
				}
			}
		}


		public void ClearParents ()
		{
			foreach (AC.Menu _menu in menus)
			{
				if (_menu.IsUnityUI () && _menu.canvas != null)
				{
					_menu.ClearParent ();
				}
			}
		}


		public void ShowPauseBackground (bool fadeIn)
		{
			float fadeSpeed = 0.5f;
			if (fadeIn)
			{
				if (pauseAlpha < 1f)
				{
					pauseAlpha += (0.2f * fadeSpeed);
				}				
				else
				{
					pauseAlpha = 1f;
				}
			}
			
			else
			{
				if (pauseAlpha > 0f)
				{
					pauseAlpha -= (0.2f * fadeSpeed);
				}
				else
				{
					pauseAlpha = 0f;
				}
			}
			
			Color tempColor = GUI.color;
			tempColor.a = pauseAlpha;
			GUI.color = tempColor;
			GUI.DrawTexture (AdvGame.GUIRect (0.5f, 0.5f, 1f, 1f), pauseTexture, ScaleMode.ScaleToFit, true, 0f);
		}
		
		
		public void DrawMenus ()
		{
			if (doResizeMenus > 0)
			{
				return;
			}
			
			if (KickStarter.playerInteraction && KickStarter.playerInput && KickStarter.menuSystem && KickStarter.stateHandler && KickStarter.settingsManager)
			{
				GUI.depth = KickStarter.menuManager.globalDepth;
				
				if (pauseTexture)
				{
					isPaused = false;
					foreach (AC.Menu menu in menus)
					{
						if (menu.IsEnabled () && menu.IsBlocking ())
						{
							isPaused = true;
						}
					}
					
					if (isPaused)
					{
						ShowPauseBackground (true);
					}
					else
					{
						ShowPauseBackground (false);
					}
				}
				
				if (selectedInputBox)
				{
					Event currentEvent = Event.current;
					if (currentEvent.isKey && currentEvent.type == EventType.KeyDown)
					{
						selectedInputBox.CheckForInput (currentEvent.keyCode.ToString (), currentEvent.shift, selectedInputBoxMenuName);
					}
				}
				
				int languageNumber = Options.GetLanguage ();

				foreach (AC.Menu menu in menus)
				{
					DrawMenu (menu, languageNumber);
				}

				foreach (AC.Menu menu in dupMenus)
				{
					DrawMenu (menu, languageNumber);
				}
			}
		}


		private void DrawMenu (AC.Menu menu, int languageNumber)
		{
			Color tempColor = GUI.color;
			
			bool isACMenu = !menu.IsUnityUI ();
			
			if (menu.IsEnabled ())
			{
				if (menu.transitionType == MenuTransition.None && menu.IsFading ())
				{
					// Stop until no longer "fading" so that it appears in right place
					return;
				}
				
				if (isACMenu)
				{
					if (menu.transitionType == MenuTransition.Fade || menu.transitionType == MenuTransition.FadeAndPan)
					{
						tempColor.a = menu.transitionProgress;
						GUI.color = tempColor;
					}
					else
					{
						tempColor.a = 1f;
						GUI.color = tempColor;
					}
					
					menu.StartDisplay ();
				}
				
				foreach (MenuElement element in menu.elements)
				{
					if (element.isVisible)
					{
						if (isACMenu)
						{
							SetStyles (element);
						}
						
						for (int i=0; i<element.GetNumSlots (); i++)
						{
							if (menu.IsEnabled () && KickStarter.stateHandler.gameState != GameState.Cutscene && KickStarter.settingsManager.SelectInteractionMethod () == SelectInteractions.CyclingMenuAndClickingHotspot && menu.appearType == AppearType.OnInteraction)
							{
								if (element is MenuInteraction)
								{
									MenuInteraction menuInteraction = (MenuInteraction) element;
									if (menuInteraction.iconID == KickStarter.playerInteraction.GetActiveUseButtonIconID ())
									{
										if (KickStarter.cursorManager.addHotspotPrefix)
										{
											if (KickStarter.runtimeInventory.hoverItem != null)
											{
												hotspotLabel = KickStarter.cursorManager.GetLabelFromID (menuInteraction.iconID, languageNumber) + KickStarter.runtimeInventory.hoverItem.GetLabel (languageNumber);
											}
											else
											{
												hotspotLabel = KickStarter.cursorManager.GetLabelFromID (menuInteraction.iconID, languageNumber) + KickStarter.playerInteraction.GetLabel (languageNumber);
											}
										}
										if (isACMenu)
										{
											element.Display (highlightedStyle, i, menu.GetZoom (), true);
										}
									}
									else
									{
										if (isACMenu)
										{
											element.Display (normalStyle, i, menu.GetZoom (), false);
										}
									}
								}
								else if (element is MenuInventoryBox)
								{
									MenuInventoryBox menuInventoryBox = (MenuInventoryBox) element;
									if (menuInventoryBox.inventoryBoxType == AC_InventoryBoxType.HostpotBased && menuInventoryBox.items[i].id == KickStarter.playerInteraction.GetActiveInvButtonID ())
									{
										if (KickStarter.cursorManager.addHotspotPrefix)
										{
											hotspotLabel = KickStarter.runtimeInventory.GetHotspotPrefixLabel (menuInventoryBox.GetItem (i), menuInventoryBox.GetLabel (i, languageNumber), languageNumber);
											
											if (KickStarter.runtimeInventory.selectedItem != null)
											{
												hotspotLabel += KickStarter.runtimeInventory.selectedItem.GetLabel (languageNumber);
											}
											else
											{
												hotspotLabel += KickStarter.playerInteraction.GetLabel (languageNumber);
											}
										}
										if (isACMenu)
										{
											element.Display (highlightedStyle, i, menu.GetZoom (), true);
										}
									}
									else if (isACMenu)
									{
										element.Display (normalStyle, i, menu.GetZoom (), false);
									}
								}
								else if (isACMenu)
								{
									element.Display (normalStyle, i, menu.GetZoom (), false);
								}
							}
							
							else if (isACMenu && menu.IsVisible () && !menu.ignoreMouseClicks && element.isClickable && KickStarter.playerInput.IsCursorReadable () && KickStarter.stateHandler.gameState != GameState.Cutscene && 
							         ((KickStarter.settingsManager.inputMethod == InputMethod.MouseAndKeyboard && menu.IsPointerOverSlot (element, i, KickStarter.playerInput.GetInvertedMouse ())) ||
							 (KickStarter.settingsManager.inputMethod == InputMethod.TouchScreen && menu.IsPointerOverSlot (element, i, KickStarter.playerInput.GetInvertedMouse ())) ||
							 (KickStarter.settingsManager.inputMethod == InputMethod.KeyboardOrController && KickStarter.stateHandler.gameState == GameState.Normal && menu.IsPointerOverSlot (element, i, KickStarter.playerInput.GetInvertedMouse ())) ||
							 ((KickStarter.settingsManager.inputMethod == InputMethod.KeyboardOrController && KickStarter.stateHandler.gameState != GameState.Normal && menu.selected_element == element && menu.selected_slot == i))))
							{
								float zoom = 1;
								if (menu.transitionType == MenuTransition.Zoom)
								{
									zoom = menu.GetZoom ();
								}
								
								if ((!KickStarter.playerInput.interactionMenuIsOn || menu.appearType == AppearType.OnInteraction)
								    && (KickStarter.playerInput.dragState == DragState.None || (KickStarter.playerInput.dragState == DragState.Inventory && CanElementBeDroppedOnto (element))))
								{
									element.Display (highlightedStyle, i, zoom, true);
								}
								else
								{
									element.Display (normalStyle, i, zoom, false);
								}
							}
							
							else if (isACMenu)
							{
								element.Display (normalStyle, i, menu.GetZoom (), false);
							}
						}
						
						if (element is MenuInput)
						{
							if (selectedInputBox == null)
							{
								if (!menu.IsUnityUI ())
								{
									MenuInput input = (MenuInput) element;
									SelectInputBox (input);
								}
								
								selectedInputBoxMenuName = menu.title;
							}
						}
					}
				}
				
				if (isACMenu)
				{
					menu.EndDisplay ();
				}
			}
			
			if (isACMenu)
			{
				tempColor.a = 1f;
				GUI.color = tempColor;
			}
		}
		
		
		public void UpdateMenuPosition (AC.Menu menu, Vector2 invertedMouse)
		{
			if (menu.IsUnityUI ())
			{
				if (Application.isPlaying)
				{
					Vector2 screenPosition = Vector2.zero;

					if (menu.uiPositionType == UIPositionType.Manual)
					{
						return;
					}
					else if (menu.uiPositionType == UIPositionType.FollowCursor)
					{
						screenPosition = new Vector2 (invertedMouse.x, Screen.height + 1f - invertedMouse.y);
						menu.SetCentre (screenPosition);
					}
					else if (menu.uiPositionType == UIPositionType.OnHotspot)
					{
						if (!menu.IsFadingOut ())
						{
							if (mouseOverInventory)
							{
								screenPosition = new Vector2 (activeInventoryBoxCentre.x, Screen.height + 1f - activeInventoryBoxCentre.y);
								menu.SetCentre (screenPosition);
							}
							else if (KickStarter.playerInteraction.GetActiveHotspot ())
							{
								if (menu.canvas.renderMode == RenderMode.WorldSpace)
								{
									menu.SetCentre (KickStarter.playerInteraction.GetActiveHotspot ().transform.position);
								}
								else
								{
									screenPosition = KickStarter.playerInteraction.GetHotspotScreenCentre ();
									screenPosition = new Vector2 (screenPosition.x * Screen.width, (1f - screenPosition.y) * Screen.height);
									menu.SetCentre (screenPosition);
								}
							}
						}
					}
					else if (menu.uiPositionType == UIPositionType.AboveSpeakingCharacter)
					{
						Char speaker = null;
						if (dupMenus.Contains (menu))
						{
							if (menu.speech != null)
							{
								speaker = menu.speech.GetSpeakingCharacter ();
							}
						}
						else
						{
							speaker = KickStarter.dialog.GetSpeakingCharacter ();
						}

						if (speaker != null)
						{
							if (menu.canvas.renderMode == RenderMode.WorldSpace)
							{
								menu.SetCentre (speaker.transform.position);
							}
							else
							{
								screenPosition = speaker.GetScreenCentre ();
								screenPosition = new Vector2 (screenPosition.x * Screen.width, (1f - screenPosition.y) * Screen.height);
								menu.SetCentre (screenPosition);
							}
						}
					}
					else if (menu.uiPositionType == UIPositionType.AbovePlayer)
					{
						if (KickStarter.player)
						{
							if (menu.canvas.renderMode == RenderMode.WorldSpace)
							{
								menu.SetCentre (KickStarter.player.transform.position);
							}
							else
							{
								screenPosition = KickStarter.player.GetScreenCentre ();
								screenPosition = new Vector2 (screenPosition.x * Screen.width, (1f - screenPosition.y) * Screen.height);
								menu.SetCentre (screenPosition);
							}
						}
					}

				}

				return;
			}

			if (invertedMouse == Vector2.zero)
			{
				invertedMouse = KickStarter.playerInput.GetInvertedMouse ();
			}
			
			if (menu.positionType == AC_PositionType.FollowCursor)
			{
				menu.SetCentre (new Vector2 ((invertedMouse.x / Screen.width) + (menu.manualPosition.x / 100f) - 0.5f,
				                             (invertedMouse.y / Screen.height) + (menu.manualPosition.y / 100f) - 0.5f));
			}
			else if (menu.positionType == AC_PositionType.OnHotspot)
			{
				if (!menu.IsFadingOut ())
				{
					if (mouseOverInventory)
					{
						Vector2 screenPosition = new Vector2 (activeInventoryBoxCentre.x / Screen.width, activeInventoryBoxCentre.y / Screen.height);
						menu.SetCentre (new Vector2 (screenPosition.x + (menu.manualPosition.x / 100f) - 0.5f,
						                             screenPosition.y + (menu.manualPosition.y / 100f) - 0.5f));
					}
					else if (KickStarter.playerInteraction.GetActiveHotspot ())
					{
						Vector2 screenPosition = KickStarter.playerInteraction.GetHotspotScreenCentre ();
						menu.SetCentre (new Vector2 (screenPosition.x + (menu.manualPosition.x / 100f) - 0.5f,
						                             screenPosition.y + (menu.manualPosition.y / 100f) - 0.5f));
					}
				}
			}
			else if (menu.positionType == AC_PositionType.AboveSpeakingCharacter)
			{
				Char speaker = null;
				if (dupMenus.Contains (menu))
				{
					if (menu.speech != null)
					{
						speaker = menu.speech.GetSpeakingCharacter ();
					}
				}
				else
				{
					speaker = KickStarter.dialog.GetSpeakingCharacter ();
				}

				if (speaker != null)
				{
					Vector2 screenPosition = speaker.GetScreenCentre ();
					menu.SetCentre (new Vector2 (screenPosition.x + (menu.manualPosition.x / 100f) - 0.5f,
					                             screenPosition.y + (menu.manualPosition.y / 100f) - 0.5f));
				}
			}
			else if (menu.positionType == AC_PositionType.AbovePlayer)
			{
				if (KickStarter.player)
				{
					Vector2 screenPosition = KickStarter.player.GetScreenCentre ();
					menu.SetCentre (new Vector2 (screenPosition.x + (menu.manualPosition.x / 100f) - 0.5f,
					                             screenPosition.y + (menu.manualPosition.y / 100f) - 0.5f));
				}
			}
		}


		private void UpdateMenu (AC.Menu menu)
		{
			Vector2 invertedMouse = KickStarter.playerInput.GetInvertedMouse ();
			UpdateMenuPosition (menu, invertedMouse);
			
			menu.HandleTransition ();
			
			if (KickStarter.settingsManager)
			{
				if (KickStarter.settingsManager.inputMethod == InputMethod.KeyboardOrController && menu.IsEnabled () &&
				    ((KickStarter.stateHandler.gameState == GameState.Paused && menu.pauseWhenEnabled) || (KickStarter.stateHandler.gameState == GameState.DialogOptions && menu.appearType == AppearType.DuringConversation)))
				{
					KickStarter.playerInput.selected_option = menu.ControlSelected (KickStarter.playerInput.selected_option);
				}
			}
			else
			{
				Debug.LogWarning ("A settings manager is not present.");
			}
			
			if (menu.appearType == AppearType.Manual)
			{
				if (menu.IsVisible () && !menu.isLocked && menu.IsPointInside (invertedMouse) && !menu.ignoreMouseClicks)
				{
					foundMouseOverMenu = true;
				}
			}
			
			else if (menu.appearType == AppearType.DuringGameplay)
			{
				if (KickStarter.stateHandler.gameState == GameState.Normal && !menu.isLocked)
				{
					if (menu.IsOff ())
					{
						menu.TurnOn (true);
					}

					if (menu.IsOn () && menu.IsPointInside (invertedMouse))
					{
						foundMouseOverMenu = true;
					}
				}
				else if (KickStarter.stateHandler.gameState == GameState.Paused)
				{
					menu.TurnOff (true);
				}
				else if (KickStarter.stateHandler.gameState != GameState.Normal && menu.IsOn () && (KickStarter.actionListManager.AreActionListsRunning () || KickStarter.playerInput.activeConversation != null))
				{
					menu.TurnOff (true);
				}
			}
			
			else if (menu.appearType == AppearType.MouseOver)
			{
				if (KickStarter.stateHandler.gameState == GameState.Normal && !menu.isLocked && menu.IsPointInside (invertedMouse))
				{
					if (menu.IsOff ())
					{
						menu.TurnOn (true);
					}
					
					if (!menu.ignoreMouseClicks)
					{
						foundMouseOverMenu = true;
					}
				}
				else if (KickStarter.stateHandler.gameState == GameState.Paused)
				{
					menu.ForceOff ();
				}
				else
				{
					menu.TurnOff (true);
				}
			}
			
			else if (menu.appearType == AppearType.OnContainer)
			{
				if (KickStarter.playerInput.activeContainer != null && !menu.isLocked && (KickStarter.stateHandler.gameState == GameState.Normal || (KickStarter.stateHandler.gameState == AC.GameState.Paused && menu.pauseWhenEnabled)))
				{
					if (menu.IsVisible () && menu.IsPointInside (invertedMouse) && !menu.ignoreMouseClicks)
					{
						foundMouseOverMenu = true;
					}
					menu.TurnOn (true);
				}
				else
				{
					menu.TurnOff (true);
				}
			}
			
			else if (menu.appearType == AppearType.DuringConversation)
			{
				if (KickStarter.playerInput.activeConversation != null && KickStarter.stateHandler.gameState == GameState.DialogOptions)
				{
					menu.TurnOn (true);
				}
				else if (KickStarter.stateHandler.gameState == GameState.Paused)
				{
					menu.ForceOff ();
				}
				else
				{
					menu.TurnOff (true);
				}
			}
			
			else if (menu.appearType == AppearType.OnInputKey)
			{
				if (menu.IsEnabled () && !menu.isLocked && menu.IsPointInside (invertedMouse) && !menu.ignoreMouseClicks)
				{
					foundMouseOverMenu = true;
				}
				
				try
				{
					if (KickStarter.playerInput.InputGetButtonDown (menu.toggleKey, true))
					{
						if (!menu.IsEnabled ())
						{
							if (KickStarter.stateHandler.gameState == GameState.Paused)
							{
								CrossFade (menu);
							}
							else
							{
								menu.TurnOn (true);
							}
						}
						else
						{
							menu.TurnOff (true);
						}
					}
				}
				catch
				{
					if (KickStarter.settingsManager.inputMethod != InputMethod.TouchScreen)
					{
						Debug.LogWarning ("No '" + menu.toggleKey + "' button exists - please define one in the Input Manager.");
					}
				}
			}
			
			else if (menu.appearType == AppearType.OnHotspot)
			{
				if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive && !menu.isLocked && KickStarter.runtimeInventory.selectedItem == null)
				{
					Hotspot hotspot = KickStarter.playerInteraction.GetActiveHotspot ();
					if (hotspot != null)
					{
						menu.HideInteractions ();
						
						if (hotspot.HasContextUse ())
						{
							menu.MatchUseInteraction (hotspot.GetFirstUseButton ());
						}
						
						if (hotspot.HasContextLook ())
						{
							menu.MatchLookInteraction (hotspot.lookButton);
						}
						
						menu.Recalculate ();
					}
				}
				
				if (hotspotLabel != "" && !menu.isLocked && (KickStarter.stateHandler.gameState == GameState.Normal || KickStarter.stateHandler.gameState == GameState.DialogOptions))
				{
					menu.TurnOn (true);
				}
				else if (KickStarter.stateHandler.gameState == GameState.Paused)
				{
					menu.ForceOff ();
				}
				else
				{
					menu.TurnOff (true);
				}
			}
			
			else if (menu.appearType == AppearType.OnInteraction)
			{
				if (KickStarter.settingsManager.CanClickOffInteractionMenu ())
				{
					if (menu.IsEnabled () && (KickStarter.stateHandler.gameState == GameState.Normal || menu.pauseWhenEnabled))
					{
						KickStarter.playerInput.interactionMenuIsOn = true;

						if (menu.IsPointInside (invertedMouse) && !menu.ignoreMouseClicks)
						{
							foundMouseOverInteractionMenu = true;
						}
						else if (KickStarter.playerInput.mouseState == MouseState.SingleClick)
						{
							KickStarter.playerInput.mouseState = MouseState.Normal;
							KickStarter.playerInput.interactionMenuIsOn = false;
							menu.TurnOff (true);
						}
					}
					else if (KickStarter.stateHandler.gameState == GameState.Paused)
					{
						KickStarter.playerInput.interactionMenuIsOn = false;
						menu.ForceOff ();
					}
					else if (KickStarter.playerInteraction.GetActiveHotspot () == null)
					{
						KickStarter.playerInput.interactionMenuIsOn = false;
						menu.TurnOff (true);
					}
				}
				else
				{
					if (menu.IsEnabled () && (KickStarter.stateHandler.gameState == GameState.Normal || menu.pauseWhenEnabled))
					{
						if (menu.IsPointInside (invertedMouse) && !menu.ignoreMouseClicks)
						{
							foundMouseOverInteractionMenu = true;
						}
						else if (!menu.IsPointInside (invertedMouse) && !menu.ignoreMouseClicks && KickStarter.playerInteraction.GetActiveHotspot () == null && KickStarter.runtimeInventory.hoverItem == null &&
						    (KickStarter.settingsManager.interactionMethod != AC_InteractionMethod.ChooseHotspotThenInteraction || KickStarter.settingsManager.cancelInteractions == CancelInteractions.CursorLeavesMenuOrHotspot))
						{
							KickStarter.playerInput.interactionMenuIsOn = false;
							menu.TurnOff (true);
						}
						else if (!menu.IsPointInside (invertedMouse) && !menu.ignoreMouseClicks && KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction && KickStarter.settingsManager.cancelInteractions == CancelInteractions.CursorLeavesMenu && !menu.IsFadingIn ())
						{
							KickStarter.playerInput.interactionMenuIsOn = false;
							menu.TurnOff (true);
						}
						else if (KickStarter.playerInteraction.GetActiveHotspot () == null && KickStarter.runtimeInventory.hoverItem == null &&
						    KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction && KickStarter.settingsManager.selectInteractions == AC.SelectInteractions.CyclingMenuAndClickingHotspot)
						{
							KickStarter.playerInput.interactionMenuIsOn = false;
							menu.TurnOff (true);
						}
						else if (KickStarter.settingsManager.SelectInteractionMethod () == SelectInteractions.CyclingMenuAndClickingHotspot && KickStarter.playerInteraction.GetActiveHotspot () != null)
						{}
						else if (KickStarter.settingsManager.SelectInteractionMethod () == SelectInteractions.CyclingMenuAndClickingHotspot && KickStarter.runtimeInventory.hoverItem != null)
						{}
						else if (KickStarter.playerInteraction.GetActiveHotspot () == null || KickStarter.settingsManager.inputMethod == InputMethod.TouchScreen)
						{}
						else if (KickStarter.runtimeInventory.selectedItem == null && KickStarter.playerInteraction.GetActiveHotspot () != null && KickStarter.runtimeInventory.hoverItem != null)
						{
							KickStarter.playerInput.interactionMenuIsOn = false;
							menu.TurnOff (true);
						}
						else if (KickStarter.runtimeInventory.selectedItem != null && KickStarter.runtimeInventory.selectedItem != KickStarter.runtimeInventory.hoverItem)
						{
							KickStarter.playerInput.interactionMenuIsOn = false;
							menu.TurnOff (true);
						}
					}
					else if (KickStarter.stateHandler.gameState == GameState.Paused)
					{
						KickStarter.playerInput.interactionMenuIsOn = false;
						menu.ForceOff ();
					}
					else if (KickStarter.playerInteraction.GetActiveHotspot () == null)
					{
						KickStarter.playerInput.interactionMenuIsOn = false;
						menu.TurnOff (true);
					}
				}
			}
			
			else if (menu.appearType == AppearType.WhenSpeechPlays)
			{
				if (KickStarter.stateHandler.gameState == GameState.Paused)
				{
					menu.ForceOff ();
				}
				else
				{
					Speech speech = menu.speech;
					if (!menu.oneMenuPerSpeech)
					{
						speech = KickStarter.dialog.GetLatestSpeech ();
					}

					if (speech != null &&
					   (menu.speechMenuType == SpeechMenuType.All ||
					   (menu.speechMenuType == SpeechMenuType.CharactersOnly && speech.GetSpeakingCharacter () != null) ||
					   (menu.speechMenuType == SpeechMenuType.NarrationOnly && speech.GetSpeakingCharacter () == null)))
					{
						if (KickStarter.options.optionsData == null || (KickStarter.options.optionsData != null && KickStarter.options.optionsData.showSubtitles) || (KickStarter.speechManager.forceSubtitles && !KickStarter.dialog.FoundAudio ())) 
						{
							menu.TurnOn (true);
						}
						else
						{
							menu.TurnOff (true);	
						}
					}
					else
					{
						menu.TurnOff (true);
					}
				}
			}
		}
		
		
		private void UpdateElements (AC.Menu menu, int languageNumber)
		{
			if (menu.transitionType == MenuTransition.None && menu.IsFading ())
			{
				// Stop until no longer "fading" so that it appears in right place
				return;
			}
			
			if (KickStarter.settingsManager.inputMethod == InputMethod.MouseAndKeyboard && menu.IsPointInside (KickStarter.playerInput.GetInvertedMouse ()))
			{
				elementIdentifier = menu.id.ToString ();
			}

			foreach (MenuElement element in menu.elements)
			{
				if (element.GetNumSlots () == 0 && menu.menuSource != MenuSource.AdventureCreator)
				{
					element.HideAllUISlots ();
				}

				for (int i=0; i<element.GetNumSlots (); i++)
				{
					element.PreDisplay (i, languageNumber, menu.IsPointerOverSlot (element, i, KickStarter.playerInput.GetInvertedMouse ()));

					if (element.isVisible && SlotIsInteractive (menu, element, i))
					{
						if ((!KickStarter.playerInput.interactionMenuIsOn || menu.appearType == AppearType.OnInteraction)
						    && (KickStarter.playerInput.dragState == DragState.None || (KickStarter.playerInput.dragState == DragState.Inventory && CanElementBeDroppedOnto (element))))
						{
							if (KickStarter.sceneSettings && element.hoverSound && lastElementIdentifier != (menu.id.ToString () + element.ID.ToString () + i.ToString ()))
							{
								KickStarter.sceneSettings.PlayDefaultSound (element.hoverSound, false);
							}
							
							elementIdentifier = menu.id.ToString () + element.ID.ToString () + i.ToString ();
						}
						
						if (KickStarter.stateHandler.gameState != GameState.Cutscene)
						{
							if (element is MenuInventoryBox)
							{
								if (KickStarter.stateHandler.gameState == GameState.Normal)
								{
									if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot && KickStarter.settingsManager.inventoryInteractions == InventoryInteractions.Single && KickStarter.runtimeInventory.selectedItem == null)
									{
										KickStarter.playerCursor.ResetSelectedCursor ();
									}
									
									MenuInventoryBox inventoryBox = (MenuInventoryBox) element;
									if (inventoryBox.inventoryBoxType == AC_InventoryBoxType.HostpotBased)
									{
										if (KickStarter.cursorManager.addHotspotPrefix)
										{
											if (KickStarter.runtimeInventory.hoverItem != null)
											{
												hotspotLabel = KickStarter.runtimeInventory.hoverItem.GetLabel (languageNumber);
											}
											else
											{
												hotspotLabel = KickStarter.playerInteraction.GetLabel (languageNumber);
											}
											
											if ((KickStarter.runtimeInventory.selectedItem == null && !KickStarter.playerInput.interactionMenuIsOn) || KickStarter.playerInput.interactionMenuIsOn)
											{
												hotspotLabel = KickStarter.runtimeInventory.GetHotspotPrefixLabel (inventoryBox.GetItem (i), inventoryBox.GetLabel (i, languageNumber), languageNumber) + hotspotLabel;
											}
										}
									}
									else
									{
										foundMouseOverInventory = true;
										//activeInventoryBoxCentre = menu.GetSlotCentre (inventoryBox, i);

										if (!KickStarter.playerInput.mouseOverInteractionMenu) // Was interactionMenuIsOn
										{
											InvItem newHoverItem = inventoryBox.GetItem (i);
											KickStarter.runtimeInventory.hoverItem = newHoverItem;
											if (oldHoverItem != newHoverItem)
											{
												KickStarter.runtimeInventory.MatchInteractions ();
												KickStarter.playerInteraction.RestoreInventoryInteraction ();
												activeInventoryBoxCentre = menu.GetSlotCentre (inventoryBox, i);

												if (KickStarter.playerInput.interactionMenuIsOn)
												{
													SetInteractionMenus (false);
												}
											}
										}

										if (KickStarter.settingsManager.SelectInteractionMethod () == SelectInteractions.CyclingCursorAndClickingHotspot)
										{}
										else
										{
											if (!KickStarter.playerInput.interactionMenuIsOn)
											{
												if (inventoryBox.displayType == ConversationDisplayType.IconOnly)
												{
													if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot)
													{
														if (KickStarter.playerCursor.GetSelectedCursor () >= 0)
														{
															hotspotLabel = KickStarter.cursorManager.GetCursorIconFromID (KickStarter.playerCursor.GetSelectedCursorID ()).label + " " + inventoryBox.GetLabel (i, languageNumber);
														}
														else if (KickStarter.runtimeInventory.selectedItem == null)
														{
															hotspotLabel = inventoryBox.GetLabel (i, languageNumber);
														}
													}
													else
													{
														if (KickStarter.runtimeInventory.hoverItem != null && KickStarter.runtimeInventory.hoverItem == KickStarter.runtimeInventory.selectedItem)
														{
															hotspotLabel = inventoryBox.GetLabel (i, languageNumber);
														}
													}
												}
											}
											else if (KickStarter.runtimeInventory.selectedItem != null)
											{
												hotspotLabel = KickStarter.runtimeInventory.selectedItem.GetLabel (languageNumber);
											}
										}
									}
								}
							}
							else if (element is MenuCrafting)
							{
								if (KickStarter.stateHandler.gameState == GameState.Normal)
								{
									MenuCrafting crafting = (MenuCrafting) element;
									KickStarter.runtimeInventory.hoverItem = crafting.GetItem (i);
									
									if (KickStarter.runtimeInventory.hoverItem != null)
									{
										if (!KickStarter.playerInput.interactionMenuIsOn)
										{
											hotspotLabel = crafting.GetLabel (i, languageNumber);
										}
										else if (KickStarter.runtimeInventory.selectedItem != null)
										{
											hotspotLabel = KickStarter.runtimeInventory.selectedItem.GetLabel (languageNumber);
										}
									}
								}
							}
							else if (element is MenuInteraction)
							{
								if (KickStarter.runtimeInventory.hoverItem != null)
								{
									hotspotLabel = KickStarter.runtimeInventory.hoverItem.GetLabel (languageNumber);
								}
								else
								{
									hotspotLabel = KickStarter.playerInteraction.GetLabel (languageNumber);
								}

								if (KickStarter.cursorManager.addHotspotPrefix && KickStarter.playerInput.interactionMenuIsOn && KickStarter.settingsManager.SelectInteractionMethod () == SelectInteractions.ClickingMenu)
								{
									MenuInteraction interaction = (MenuInteraction) element;
									hotspotLabel = KickStarter.cursorManager.GetLabelFromID (interaction.iconID, languageNumber) + hotspotLabel;
								}
							}
							else if (element is MenuDialogList)
							{
								if (KickStarter.stateHandler.gameState == GameState.DialogOptions)
								{
									MenuDialogList dialogList = (MenuDialogList) element;
									if (dialogList.displayType == ConversationDisplayType.IconOnly)
									{
										hotspotLabel = dialogList.GetLabel (i, languageNumber);
									}
								}
							}
							else if (element is MenuButton)
							{
								MenuButton button = (MenuButton) element;
								if (button.hotspotLabel != "")
								{
									hotspotLabel = button.GetHotspotLabel (languageNumber);
								}
							}
						}
					}
				}
			}
		}
		
		
		private bool SlotIsInteractive (AC.Menu menu, MenuElement element, int i)
		{
			if (menu.IsVisible () && element.isClickable && 
			    ((KickStarter.settingsManager.inputMethod == InputMethod.MouseAndKeyboard && menu.IsPointerOverSlot (element, i, KickStarter.playerInput.GetInvertedMouse ())) ||
			 (KickStarter.settingsManager.inputMethod == InputMethod.TouchScreen && menu.IsPointerOverSlot (element, i, KickStarter.playerInput.GetInvertedMouse ())) ||
			 (KickStarter.settingsManager.inputMethod == InputMethod.KeyboardOrController && KickStarter.stateHandler.gameState == GameState.Normal && menu.IsPointerOverSlot (element, i, KickStarter.playerInput.GetInvertedMouse ())) ||
			 ((KickStarter.settingsManager.inputMethod == InputMethod.KeyboardOrController && KickStarter.stateHandler.gameState != GameState.Normal && menu.selected_element == element && menu.selected_slot == i))))
			{
				return true;
			}

			return false;
		}
		
		
		private void CheckClicks (AC.Menu menu)
		{
			if (menu.transitionType == MenuTransition.None && menu.IsFading ())
			{
				// Stop until no longer "fading" so that it appears in right place
				return;
			}
			
			if (KickStarter.settingsManager.inputMethod == InputMethod.MouseAndKeyboard && menu.IsPointInside (KickStarter.playerInput.GetInvertedMouse ()))
			{
				elementIdentifier = menu.id.ToString ();
			}
			
			foreach (MenuElement element in menu.elements)
			{
				if (element.isVisible)
				{
					for (int i=0; i<element.GetNumSlots (); i++)
					{
						if (SlotIsInteractive (menu, element, i))
						{
							if (!menu.IsUnityUI () && KickStarter.playerInput.mouseState != MouseState.Normal && (KickStarter.playerInput.dragState == DragState.None || KickStarter.playerInput.dragState == DragState.Menu))
							{
								if (KickStarter.playerInput.mouseState == MouseState.SingleClick || KickStarter.playerInput.mouseState == MouseState.LetGo || KickStarter.playerInput.mouseState == MouseState.RightClick)
								{
									if (element is MenuInput) {}
									else DeselectInputBox ();
									
									CheckClick (menu, element, i, KickStarter.playerInput.mouseState);
								}
								else if (KickStarter.playerInput.mouseState == MouseState.HeldDown)
								{
									CheckContinuousClick (menu, element, i, KickStarter.playerInput.mouseState);
								}
							}
							else if (menu.IsUnityUI () && KickStarter.runtimeInventory.selectedItem == null && KickStarter.settingsManager.inventoryDragDrop && KickStarter.playerInput.mouseState == MouseState.HeldDown && KickStarter.playerInput.dragState == DragState.None)
							{
								if (element is MenuInventoryBox || element is MenuCrafting)
								{
									// Begin UI drag drop
									CheckClick (menu, element, i, MouseState.SingleClick);
								}
							}
							else if (KickStarter.playerInteraction.IsDroppingInventory () && CanElementBeDroppedOnto (element))
							{
								if (menu.IsUnityUI () && KickStarter.settingsManager.inventoryDragDrop && (element is MenuInventoryBox || element is MenuCrafting))
								{
									// End UI drag drop
									element.ProcessClick (menu, i, MouseState.SingleClick);
								}
								else
								{
									DeselectInputBox ();
									CheckClick (menu, element, i, MouseState.SingleClick);
								}
							}
						}
					}
				}
			}
		}
		
		
		public void UpdateAllMenus ()
		{
			#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
			if (keyboard != null && selectedInputBox != null)
			{
				selectedInputBox.label = keyboard.text;
			}
			#endif
			
			if (doResizeMenus > 0)
			{
				doResizeMenus ++;
				
				if (doResizeMenus == 4)
				{
					doResizeMenus = 0;
					foreach (AC.Menu menu in PlayerMenus.GetMenus ())
					{
						menu.Recalculate ();
						menu.UpdateAspectRect ();
						KickStarter.mainCamera.SetCameraRect ();
						menu.Recalculate ();
					}
				}
			}
			
			if (Time.time > 0f)
			{
				int languageNumber = Options.GetLanguage ();
				hotspotLabel = KickStarter.playerInteraction.GetLabel (languageNumber);

				if (!KickStarter.playerInput.interactionMenuIsOn || !KickStarter.playerInput.mouseOverInteractionMenu)
				{
					oldHoverItem = KickStarter.runtimeInventory.hoverItem;
					KickStarter.runtimeInventory.hoverItem = null;
				}
				
				if (KickStarter.stateHandler.gameState == GameState.Paused)
				{
					if (Time.timeScale != 0f)
					{
						KickStarter.sceneSettings.PauseGame ();
					}
				}
				else if (Time.timeScale == 0f)
				{
					KickStarter.sceneSettings.UnpauseGame (KickStarter.playerInput.timeScale);
				}
				
				foundMouseOverMenu = false;
				foundMouseOverInteractionMenu = false;
				foundMouseOverInventory = false;
				
				foreach (AC.Menu menu in menus)
				{
					UpdateMenu (menu);
					if (menu.IsEnabled ())
					{
						UpdateElements (menu, languageNumber);
					}
				}

				for (int i=0; i<dupMenus.Count; i++)
				{
					UpdateMenu (dupMenus[i]);
					UpdateElements (dupMenus[i], languageNumber);

					if (dupMenus[i].IsOff () && KickStarter.stateHandler.gameState != GameState.Paused)
					{
						Menu oldMenu = dupMenus[i];
						dupMenus.RemoveAt (i);
						if (oldMenu.menuSource != MenuSource.AdventureCreator && oldMenu.canvas && oldMenu.canvas.gameObject.activeInHierarchy)
						{
							DestroyImmediate (oldMenu.canvas.gameObject);
						}
						DestroyImmediate (oldMenu);
						i=0;
					}

				}

				KickStarter.playerInput.mouseOverMenu = foundMouseOverMenu;
				KickStarter.playerInput.mouseOverInteractionMenu = foundMouseOverInteractionMenu;
				mouseOverInventory = foundMouseOverInventory;

				lastElementIdentifier = elementIdentifier;
				
				// Check clicks in reverse order
				for (int i=menus.Count-1; i>=0; i--)
				{
					if (menus[i].IsEnabled () && !menus[i].ignoreMouseClicks/* && !menus[i].IsUnityUI ()*/)
					{
						CheckClicks (menus[i]);
					}
				}
			}
		}
		
		
		public void CheckCrossfade (AC.Menu _menu)
		{
			if (crossFadeFrom == _menu && crossFadeTo != null)
			{
				crossFadeFrom.ForceOff ();
				crossFadeTo.TurnOn (true);
				crossFadeTo = null;
			}
		}
		
		
		public void SelectInputBox (MenuInput input)
		{
			selectedInputBox = input;
			
			// Mobile keyboard
			#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
			if (input.inputType == AC_InputType.NumbericOnly)
			{
				keyboard = TouchScreenKeyboard.Open (input.label, TouchScreenKeyboardType.NumberPad, false, false, false, false, "");
			}
			else
			{
				keyboard = TouchScreenKeyboard.Open (input.label, TouchScreenKeyboardType.ASCIICapable, false, false, false, false, "");
			}
			#endif
		}
		
		
		private void DeselectInputBox ()
		{
			if (selectedInputBox)
			{
				selectedInputBox.Deselect ();
				selectedInputBox = null;
				
				// Mobile keyboard
				#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
				if (keyboard != null)
				{
					keyboard.active = false;
					keyboard = null;
				}
				#endif
			}
		}
		
		
		private void CheckClick (AC.Menu _menu, MenuElement _element, int _slot, MouseState _mouseState)
		{
			KickStarter.playerInput.mouseState = MouseState.Normal;

			if (_mouseState == MouseState.LetGo)
			{
				if (KickStarter.settingsManager.inputMethod == InputMethod.TouchScreen && !KickStarter.settingsManager.offsetTouchCursor && KickStarter.runtimeInventory.selectedItem == null && !(_element is MenuInventoryBox) && !(_element is MenuCrafting))
				{
					_mouseState = MouseState.SingleClick;
				}
				else
				{
					_mouseState = MouseState.Normal;
					return;
				}
			}
			
			if (_mouseState != MouseState.Normal)
			{
				if (_element.clickSound != null && KickStarter.sceneSettings != null)
				{
					KickStarter.sceneSettings.PlayDefaultSound (_element.clickSound, false);
				}

				_element.ProcessClick (_menu, _slot, _mouseState);
				PlayerMenus.ResetInventoryBoxes ();
			}
		}
		
		
		private void CheckContinuousClick (AC.Menu _menu, MenuElement _element, int _slot, MouseState _mouseState)
		{
			_element.ProcessContinuousClick (_menu, _mouseState);
		}


		public void RemoveSpeechFromMenu (Speech speech)
		{
			foreach (Menu menu in dupMenus)
			{
				if (menu.speech == speech)
				{
					menu.speech = null;
				}
			}
		}


		public void AssignSpeechToMenu (Speech speech)
		{
			foreach (Menu menu in menus)
			{
				if (menu.appearType == AppearType.WhenSpeechPlays && menu.oneMenuPerSpeech)
				{
					Menu dupMenu = ScriptableObject.CreateInstance <Menu>();
					dupMenu.Copy (menu);
					if (dupMenu.IsUnityUI ())
					{
						dupMenu.LoadUnityUI ();
					}
					dupMenu.title += " (Duplicate)";
					dupMenu.SetSpeech (speech);
					dupMenu.TurnOn (true);
					dupMenus.Add (dupMenu);
				}
			}
		}
		
		
		public void CrossFade (AC.Menu _menuTo)
		{
			if (_menuTo.isLocked)
			{
				Debug.Log ("Cannot crossfade to menu " + _menuTo.title + " as it is locked.");
			}
			else if (!_menuTo.IsEnabled())
			{
				// Turn off all other menus
				crossFadeFrom = null;
				
				foreach (AC.Menu menu in menus)
				{
					if (menu.IsVisible ())
					{
						if (menu.appearType == AppearType.OnHotspot || menu.fadeSpeed == 0)
						{
							menu.ForceOff ();
						}
						else
						{
							menu.TurnOff (true);
							crossFadeFrom = menu;
						}
					}
					else
					{
						menu.ForceOff ();
					}
				}
				
				if (crossFadeFrom != null)
				{
					crossFadeTo = _menuTo;
				}
				else
				{
					_menuTo.TurnOn (true);
				}
			}
		}
		
		
		public void SetInteractionMenus (bool turnOn)
		{
			KickStarter.playerInput.interactionMenuIsOn = turnOn;

			foreach (AC.Menu _menu in menus)
			{
				if (_menu.appearType == AppearType.OnInteraction)
				{
					if (turnOn)
					{
						StopCoroutine ("SwapInteractionMenu");
						StartCoroutine ("SwapInteractionMenu", _menu);
					}
					else
					{
						_menu.TurnOff (true);
					}
				}
			}
		}


		private IEnumerator SwapInteractionMenu (Menu _menu)
		{
			_menu.TurnOff (true);

			while (_menu.IsFading ())
			{
				yield return new WaitForFixedUpdate ();
			}

			KickStarter.playerInteraction.ResetInteractionIndex ();
			if (KickStarter.runtimeInventory.hoverItem != null)
			{
				_menu.MatchInteractions (KickStarter.runtimeInventory.hoverItem, KickStarter.settingsManager.cycleInventoryCursors);
			}
			else if (KickStarter.playerInteraction.GetActiveHotspot ())
			{
				_menu.MatchInteractions (KickStarter.playerInteraction.GetActiveHotspot ().useButtons, KickStarter.settingsManager.cycleInventoryCursors);
			}

			_menu.TurnOn (true);
		}
		
		
		public void DisableHotspotMenus ()
		{
			foreach (AC.Menu _menu in menus)
			{
				if (_menu.appearType == AppearType.OnHotspot)
				{
					_menu.ForceOff ();
				}
			}
		}
		
		
		public string GetHotspotLabel ()
		{
			return hotspotLabel;
		}
		
		
		private void SetStyles (MenuElement element)
		{
			normalStyle.normal.textColor = element.fontColor;
			normalStyle.font = element.font;
			normalStyle.fontSize = element.GetFontSize ();
			normalStyle.alignment = TextAnchor.MiddleCenter;
			
			highlightedStyle.font = element.font;
			highlightedStyle.fontSize = element.GetFontSize ();
			highlightedStyle.normal.textColor = element.fontHighlightColor;
			highlightedStyle.normal.background = element.highlightTexture;
			highlightedStyle.alignment = TextAnchor.MiddleCenter;
		}
		
		
		private bool CanElementBeDroppedOnto (MenuElement element)
		{
			if (element is MenuInventoryBox)
			{
				MenuInventoryBox inventoryBox = (MenuInventoryBox) element;
				if (inventoryBox.inventoryBoxType == AC_InventoryBoxType.Default || inventoryBox.inventoryBoxType == AC_InventoryBoxType.CustomScript)
				{
					return true;
				}
			}
			else if (element is MenuCrafting)
			{
				MenuCrafting crafting = (MenuCrafting) element;
				if (crafting.craftingType == CraftingElementType.Ingredients)
				{
					return true;
				}
			}
			
			return false;
		}
		
		
		private void OnDestroy ()
		{
			menus = null;
		}
		
		
		public static List<Menu> GetMenus ()
		{
			if (KickStarter.playerMenus)
			{
				return KickStarter.playerMenus.menus;
			}
			return null;
		}
		
		
		public static Menu GetMenuWithName (string menuName)
		{
			if (KickStarter.playerMenus)
			{
				foreach (AC.Menu menu in KickStarter.playerMenus.menus)
				{
					if (menu.title == menuName)
					{
						return menu;
					}
				}
			}
			
			return null;
		}
		
		
		public static MenuElement GetElementWithName (string menuName, string menuElementName)
		{
			if (KickStarter.playerMenus)
			{
				foreach (AC.Menu menu in KickStarter.playerMenus.menus)
				{
					if (menu.title == menuName)
					{
						foreach (MenuElement menuElement in menu.elements)
						{
							if (menuElement.title == menuElementName)
							{
								return menuElement;
							}
						}
					}
				}
			}
			
			return null;
		}
		
		
		public static bool IsSavingLocked ()
		{
			if (KickStarter.stateHandler)
			{
				if (KickStarter.stateHandler.GetLastNonPausedState () != GameState.Normal)
				{
					return true;
				}
			}
			
			if (KickStarter.playerMenus)
			{
				return (KickStarter.playerMenus.lockSave);
			}
			
			return false;
		}
		
		
		public static void ResetInventoryBoxes ()
		{
			if (KickStarter.playerMenus)
			{
				foreach (AC.Menu menu in KickStarter.playerMenus.menus)
				{
					foreach (MenuElement menuElement in menu.elements)
					{
						if (menuElement is MenuInventoryBox)
						{
							menuElement.RecalculateSize (menu.menuSource);
						}
					}
				}
			}
		}
		
		
		public static void CreateRecipe ()
		{
			if (KickStarter.playerMenus)
			{
				foreach (AC.Menu menu in KickStarter.playerMenus.menus)
				{
					foreach (MenuElement menuElement in menu.elements)
					{
						if (menuElement is MenuCrafting)
						{
							MenuCrafting crafting = (MenuCrafting) menuElement;
							if (crafting.craftingType == CraftingElementType.Output)
							{
								crafting.SetOutput (menu.menuSource, false);
							}
						}
					}
				}
			}
		}
		
		
		public static void ForceOffAllMenus (bool onlyPausing)
		{
			if (KickStarter.playerMenus)
			{
				foreach (AC.Menu menu in KickStarter.playerMenus.menus)
				{
					if (menu.IsEnabled ())
					{
						if (!onlyPausing || (onlyPausing && menu.pauseWhenEnabled))
						{
							menu.ForceOff ();
						}
					}
				}
			}
		}
		
		
		public static void SimulateClick (string menuName, string menuElementName, int slot)
		{
			if (KickStarter.playerMenus)
			{
				AC.Menu menu = PlayerMenus.GetMenuWithName (menuName);
				MenuElement element = PlayerMenus.GetElementWithName (menuName, menuElementName);
				KickStarter.playerMenus.CheckClick (menu, element, slot, MouseState.SingleClick);
			}
		}
		
		
		public static void SimulateClick (string menuName, MenuElement _element, int _slot)
		{
			if (KickStarter.playerMenus)
			{
				AC.Menu menu = PlayerMenus.GetMenuWithName (menuName);
				KickStarter.playerMenus.CheckClick (menu, _element, _slot, MouseState.SingleClick);
			}
		}
		
		
		public bool ArePauseMenusOn (Menu excludingMenu)
		{
			foreach (AC.Menu menu in menus)
			{
				if (menu.IsEnabled () && menu.IsBlocking () && menu != excludingMenu)
				{
					return true;
				}
			}
			return false;
		}
		
		
		public void ForceOffSubtitles ()
		{
			foreach (AC.Menu menu in menus)
			{
				if (menu.IsEnabled () && menu.appearType == AppearType.WhenSpeechPlays)
				{
					menu.ForceOff ();
				}
			}
		}
		
		
		public static void RecalculateAll ()
		{
			if (KickStarter.playerMenus)
			{
				KickStarter.playerMenus._RecorrectAll ();
			}

			// Border camera
			KickStarter.mainCamera.SetCameraRect ();
		}
		
		
		public void _RecorrectAll ()
		{
			doResizeMenus = 1;
		}


		public void HideSaveMenus ()
		{
			foreach (AC.Menu menu in menus)
			{
				foreach (MenuElement element in menu.elements)
				{
					if (element is MenuSavesList && menu.IsManualControlled ())
					{
						MenuSavesList saveList = (MenuSavesList) element;
						if (saveList.saveListType == AC_SaveListType.Save)
						{
							menu.ForceOff ();
							break;
						}
					}
				}
			}
		}


		public void FindFirstSelectedElement ()
		{
			if (eventSystem == null || menus.Count == 0)
			{
				return;
			}

			GameObject objectToSelect = null;
			for (int i=menus.Count-1; i>=0; i--)
			{
				Menu menu = menus[i];

				if (menu.IsEnabled ())
				{
					objectToSelect = menu.GetObjectToSelect ();
					if (objectToSelect != null)
					{
						break;
					}
				}
			}

			eventSystem.SetSelectedGameObject (objectToSelect);
		}
		
	}
	
}