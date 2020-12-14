/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"PlayerInteraction.cs"
 * 
 *	This script processes cursor clicks over hotspots and NPCs
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{
	
	public class PlayerInteraction : MonoBehaviour
	{
		
		public bool inPreInteractionCutscene = false;
		
		private Hotspot hotspot;
		private Button button = null;
		private int interactionIndex = -1;


		public void UpdateInteraction ()
		{
			if (KickStarter.stateHandler && KickStarter.playerInput && KickStarter.settingsManager && KickStarter.runtimeInventory && KickStarter.stateHandler.gameState == GameState.Normal)			
			{
				if (KickStarter.playerInput.dragState == DragState.Moveable)
				{
					DisableHotspot (true);
					return;
				}
				
				if (KickStarter.playerInput.mouseState == MouseState.RightClick && KickStarter.runtimeInventory.selectedItem != null && !KickStarter.playerInput.mouseOverMenu)
				{
					if (KickStarter.settingsManager.SelectInteractionMethod () == SelectInteractions.CyclingCursorAndClickingHotspot && KickStarter.settingsManager.cycleInventoryCursors)
					{
						// Don't respond to right-clicks
					}
					else if (KickStarter.settingsManager.inventoryInteractions == InventoryInteractions.Multiple || KickStarter.settingsManager.rightClickInventory == RightClickInventory.DeselectsItem)
					{
						KickStarter.playerInput.mouseState = MouseState.Normal;
						KickStarter.runtimeInventory.SetNull ();
					}
					else if (KickStarter.settingsManager.rightClickInventory == RightClickInventory.ExaminesItem)
					{
						KickStarter.playerInput.mouseState = MouseState.Normal;
						KickStarter.runtimeInventory.Look (KickStarter.runtimeInventory.selectedItem);
					}
				}
				
				if (KickStarter.playerInput.IsCursorLocked () && KickStarter.settingsManager.onlyInteractWhenCursorUnlocked)
				{
					DisableHotspot (true);
					return;
				}
				
				if (!KickStarter.playerInput.IsCursorReadable ())
				{
					return;
				}
				
				if (!KickStarter.playerInput.mouseOverMenu && Camera.main && !KickStarter.playerInput.ActiveArrowsDisablingHotspots ())
				{
					if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction)
					{
						if (KickStarter.settingsManager.SelectInteractionMethod () == SelectInteractions.CyclingCursorAndClickingHotspot)
						{
							ContextSensitiveClick ();
						}
						//else// if (!KickStarter.playerInput.interactionMenuIsOn || KickStarter.settingsManager.SelectInteractionMethod () == SelectInteractions.CyclingMenuAndClickingHotspot)
						else if (!KickStarter.playerInput.mouseOverInteractionMenu)
						{
							ChooseHotspotThenInteractionClick ();
						}
					}
					else
					{
						ContextSensitiveClick ();
					}
				}
				else 
				{
					DisableHotspot (false);
				}
				
				if (KickStarter.settingsManager.playerFacesHotspots && KickStarter.player != null)
				{
					if (hotspot)
					{
						KickStarter.player.SetHeadTurnTarget (hotspot.GetIconPosition (), false, HeadFacing.Hotspot);
					}
					else if (button == null)
					{
						KickStarter.player.ClearHeadTurnTarget (HeadFacing.Hotspot, false);
					}
				}
			}
		}
		
		
		public void UpdateInventory ()
		{
			if (hotspot == null && button == null && IsDroppingInventory ())
			{
				KickStarter.runtimeInventory.SetNull ();
			}
		}
		
		
		private Hotspot CheckForHotspots ()
		{
			if (!KickStarter.playerInput.mouseIsOnScreen)
			{
				return null;
			}

			if (KickStarter.settingsManager.inventoryDragDrop && KickStarter.playerInput.GetMousePosition () == Vector2.zero)
			{
				return null;
			}
				
			if (KickStarter.settingsManager.hotspotDetection == HotspotDetection.PlayerVicinity && KickStarter.player.hotspotDetector)
			{
				if (KickStarter.settingsManager.movementMethod == MovementMethod.Direct || KickStarter.settingsManager.IsInFirstPerson ())
				{
					if (KickStarter.settingsManager.hotspotsInVicinity == HotspotsInVicinity.ShowAll)
					{
						// Just highlight the nearest hotspot, but don't make it the "active" one
						KickStarter.player.hotspotDetector.HighlightAll ();
					}
					else
					{
						return (KickStarter.player.hotspotDetector.GetSelected ());
					}
				}
				else
				{
					// Just highlight the nearest hotspot, but don't make it the "active" one
					KickStarter.player.hotspotDetector.HighlightAll ();
				}
			}
			
			if (KickStarter.settingsManager && KickStarter.settingsManager.IsUnity2D ())
			{
				RaycastHit2D hit;
				if (KickStarter.mainCamera.IsOrthographic ())
				{
					hit = Physics2D.Raycast (Camera.main.ScreenToWorldPoint (KickStarter.playerInput.GetMousePosition ()), Vector2.zero, KickStarter.settingsManager.navMeshRaycastLength, 1 << LayerMask.NameToLayer (KickStarter.settingsManager.hotspotLayer));
				}
				else
				{
					Vector3 pos = KickStarter.playerInput.GetMousePosition ();
					pos.z = -Camera.main.transform.position.z;
					hit = Physics2D.Raycast (Camera.main.ScreenToWorldPoint (pos), Vector2.zero, KickStarter.settingsManager.navMeshRaycastLength, 1 << LayerMask.NameToLayer (KickStarter.settingsManager.hotspotLayer));
				}
				
				if (hit.collider != null && hit.collider.gameObject.GetComponent <Hotspot>())
				{
					Hotspot hitHotspot = hit.collider.gameObject.GetComponent <Hotspot>();
					if (KickStarter.settingsManager.hotspotDetection != HotspotDetection.PlayerVicinity)
					{
						return (hitHotspot);
					}
					else if (KickStarter.player.hotspotDetector && KickStarter.player.hotspotDetector.IsHotspotInTrigger (hitHotspot))
					{
						return (hitHotspot);
					}
				}
			}
			else
			{
				Camera _camera = Camera.main;
				if (KickStarter.settingsManager.movementMethod == MovementMethod.UltimateFPS)
				{
					if (KickStarter.settingsManager.onlyInteractWhenCursorUnlocked && !UltimateFPSIntegration.IsCursorForced ())
					{
						_camera = null;
					}
				}
				
				if (_camera)
				{
					Ray ray = _camera.ScreenPointToRay (KickStarter.playerInput.GetMousePosition ());
					RaycastHit hit;
					
					if (Physics.Raycast (ray, out hit, KickStarter.settingsManager.hotspotRaycastLength, 1 << LayerMask.NameToLayer (KickStarter.settingsManager.hotspotLayer)))
					{
						if (hit.collider.gameObject.GetComponent <Hotspot>())
						{
							Hotspot hitHotspot = hit.collider.gameObject.GetComponent <Hotspot>();
							if (KickStarter.settingsManager.hotspotDetection != HotspotDetection.PlayerVicinity)
							{
								return (hitHotspot);
							}
							else if (KickStarter.player.hotspotDetector && KickStarter.player.hotspotDetector.IsHotspotInTrigger (hitHotspot))
							{
								return (hitHotspot);
							}
						}
					}
				}
			}
			
			return null;
		}
		
		
		private bool CanDoDoubleTap ()
		{
			if (KickStarter.runtimeInventory.selectedItem != null && KickStarter.settingsManager.inventoryDragDrop)
				return false;
			
			if (KickStarter.settingsManager.inputMethod == InputMethod.TouchScreen && KickStarter.settingsManager.doubleTapHotspots)
				return true;
			
			return false;
		}
		
		
		private void ChooseHotspotThenInteractionClick ()
		{
			if (CanDoDoubleTap ())
			{
				if (KickStarter.playerInput.mouseState == MouseState.SingleClick)
				{
					ChooseHotspotThenInteractionClick_Process (true);
				}
			}
			else
			{
				ChooseHotspotThenInteractionClick_Process (false);
			}
		}
		
		
		private void ChooseHotspotThenInteractionClick_Process (bool doubleTap)
		{
			Hotspot newHotspot = CheckForHotspots ();
			
			if (hotspot != null && newHotspot == null)
			{
				DisableHotspot (false);
			}
			else if (newHotspot != null)
			{
				if (newHotspot.IsSingleInteraction ())
				{
					ContextSensitiveClick ();
					return;
				}
				
				if (KickStarter.playerInput.mouseState == MouseState.HeldDown && KickStarter.playerInput.dragState == DragState.Player)
				{
					// Disable hotspots while dragging player
					DisableHotspot (false);
				}
				else
				{
					bool clickedNew = false;
					if (newHotspot != hotspot)
					{
						clickedNew = true;
						
						if (hotspot)
						{
							hotspot.Deselect ();
							KickStarter.playerMenus.DisableHotspotMenus ();
						}
						
						if (hotspot != null)
						{
							KickStarter.playerMenus.SetInteractionMenus (false);
						}
						
						hotspot = newHotspot;
						hotspot.Select ();
					}
					
					if (hotspot)
					{
						if (KickStarter.playerInput.mouseState == MouseState.SingleClick ||
						    (KickStarter.settingsManager.inventoryDragDrop && IsDroppingInventory ()) ||
						    (KickStarter.settingsManager.MouseOverForInteractionMenu () && KickStarter.runtimeInventory.hoverItem == null && KickStarter.runtimeInventory.selectedItem == null && clickedNew && !IsDroppingInventory ()))
						{
							if (KickStarter.runtimeInventory.hoverItem == null && KickStarter.playerInput.mouseState == MouseState.SingleClick && 
							    KickStarter.settingsManager.MouseOverForInteractionMenu () && KickStarter.settingsManager.SelectInteractionMethod () == SelectInteractions.ClickingMenu &&
							    KickStarter.settingsManager.cancelInteractions != CancelInteractions.ClickOffMenu)
							{
								return;
							}
							
							if (KickStarter.runtimeInventory.selectedItem != null)
							{
								if (!KickStarter.settingsManager.inventoryDragDrop && clickedNew && doubleTap)
								{
									return;
								} 
								else
								{
									HandleInteraction ();
								}
							}
							else if (KickStarter.playerMenus)
							{
								if (KickStarter.playerInput.interactionMenuIsOn && KickStarter.settingsManager.SelectInteractionMethod () == SelectInteractions.CyclingMenuAndClickingHotspot)
								{
									ClickHotspotToInteract ();
									return;
								}
								
								if (clickedNew && doubleTap)
								{
									return;
								}
								
								KickStarter.playerMenus.SetInteractionMenus (true);
								
								if (KickStarter.settingsManager.seeInteractions == SeeInteractions.ClickOnHotspot)
								{
									if (KickStarter.settingsManager.stopPlayerOnClickHotspot)
									{
										KickStarter.player.Halt ();
									}
									
									KickStarter.playerInput.hotspotMovingTo = null;
									StopInteraction ();
									KickStarter.runtimeInventory.SetNull ();
								}
							}
						}
						else if (KickStarter.playerInput.mouseState == MouseState.RightClick)
						{
							hotspot.Deselect ();
						}
					}
				}
			}
		}
		
		
		private void ContextSensitiveClick ()
		{
			if (CanDoDoubleTap ())
			{
				// Detect Hotspots only on mouse click
				if (KickStarter.playerInput.mouseState == MouseState.SingleClick)
				{
					// Check Hotspots only when click/tap
					ContextSensitiveClick_Process (true, CheckForHotspots ());
				}
				else if (KickStarter.playerInput.mouseState == MouseState.RightClick)
				{
					HandleInteraction ();
				}
			}
			else
			{
				// Always detect Hotspots
				ContextSensitiveClick_Process (false, CheckForHotspots ());
				
				if (!KickStarter.playerInput.mouseOverMenu && hotspot)
				{
					if (KickStarter.playerInput.mouseState == MouseState.SingleClick || KickStarter.playerInput.mouseState == MouseState.DoubleClick || KickStarter.playerInput.mouseState == MouseState.RightClick || IsDroppingInventory ())
					{
						if (KickStarter.settingsManager.SelectInteractionMethod () == SelectInteractions.CyclingCursorAndClickingHotspot &&
						    (KickStarter.runtimeInventory.selectedItem == null || (KickStarter.runtimeInventory.selectedItem != null && KickStarter.settingsManager.cycleInventoryCursors)))
						{
							if (KickStarter.playerInput.mouseState != MouseState.RightClick)
							{
								ClickHotspotToInteract ();
							}
						}
						else
						{
							HandleInteraction ();
						}
					}
				}
			}
			
		}
		
		
		private void ContextSensitiveClick_Process (bool doubleTap, Hotspot newHotspot)
		{
			if (hotspot != null && newHotspot == null)
			{
				DisableHotspot (false);
			}
			else if (newHotspot != null)
			{
				if (KickStarter.playerInput.mouseState == MouseState.HeldDown && KickStarter.playerInput.dragState == DragState.Player)
				{
					// Disable hotspots while dragging player
					DisableHotspot (false); 
				}
				else if (newHotspot != hotspot)
				{
					DisableHotspot (false); 
					
					hotspot = newHotspot;
					hotspot.Select ();
					
					if (KickStarter.settingsManager.SelectInteractionMethod () == SelectInteractions.CyclingCursorAndClickingHotspot)
					{
						KickStarter.runtimeInventory.MatchInteractions ();
						RestoreHotspotInteraction ();
					}
				}
				else if (hotspot != null && doubleTap)
				{
					// Still work if not clicking on the active Hotspot
					HandleInteraction ();
				}
			}
		}
		
		
		public void DisableHotspot (bool isInstant)
		{
			if (hotspot)
			{
				if (isInstant)
				{
					hotspot.DeselectInstant ();
				}
				else
				{
					hotspot.Deselect ();
				}
				hotspot = null;
			}
		}
		
		
		public bool DoesHotspotHaveInventoryInteraction ()
		{
			if (hotspot && KickStarter.runtimeInventory && KickStarter.runtimeInventory.selectedItem != null)
			{
				foreach (Button _button in hotspot.invButtons)
				{
					if (_button.invID == KickStarter.runtimeInventory.selectedItem.id && !_button.isDisabled)
					{
						return true;
					}
				}
			}
			
			return false;
		}
		
		
		private void HandleInteraction ()
		{
			if (hotspot)
			{
				if (KickStarter.settingsManager == null || KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive)
				{
					if (KickStarter.playerInput.mouseState == MouseState.SingleClick || KickStarter.playerInput.mouseState == MouseState.DoubleClick)
					{
						if (KickStarter.runtimeInventory.selectedItem == null && hotspot.HasContextUse ())
						{
							// Perform "Use" interaction
							ClickButton (InteractionType.Use, -1, -1);
						}
						
						else if (KickStarter.runtimeInventory.selectedItem != null)
						{
							// Perform "Use Inventory" interaction
							ClickButton (InteractionType.Inventory, -1, KickStarter.runtimeInventory.selectedItem.id);
							
							if (KickStarter.settingsManager.inventoryDisableLeft)
							{
								KickStarter.runtimeInventory.SetNull ();
							}
						}
						
						else if (hotspot.HasContextLook () && KickStarter.cursorManager.leftClickExamine)
						{
							// Perform "Look" interaction
							ClickButton (InteractionType.Examine, -1, -1);
						}
					}
					else if (KickStarter.playerInput.mouseState == MouseState.RightClick)
					{
						if (hotspot.HasContextLook () && KickStarter.runtimeInventory.selectedItem == null)
						{
							// Perform "Look" interaction
							ClickButton (InteractionType.Examine, -1, -1);
						}
					}
					else if (KickStarter.settingsManager.inventoryDragDrop && IsDroppingInventory ())
					{
						// Perform "Use Inventory" interaction (Drag n' drop mode)
						ClickButton (InteractionType.Inventory, -1, KickStarter.runtimeInventory.selectedItem.id);
						KickStarter.runtimeInventory.SetNull ();
					}
				}
				
				else if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot && KickStarter.playerCursor && KickStarter.cursorManager)
				{
					if (KickStarter.playerInput.mouseState == MouseState.SingleClick)
					{
						if (KickStarter.runtimeInventory.selectedItem == null && hotspot.provideUseInteraction)
						{
							// Perform "Use" interaction
							if (GetActiveHotspot () != null && GetActiveHotspot ().IsSingleInteraction ())
							{
								ClickButton (InteractionType.Use, -1, -1);
							}
							else if (KickStarter.playerCursor.GetSelectedCursor () >= 0)
							{
								ClickButton (InteractionType.Use, KickStarter.cursorManager.cursorIcons [KickStarter.playerCursor.GetSelectedCursor ()].id, -1);
							}
						}
						else if (KickStarter.runtimeInventory.selectedItem != null && KickStarter.playerCursor.GetSelectedCursor () == -2)
						{
							// Perform "Use Inventory" interaction
							KickStarter.playerCursor.ResetSelectedCursor ();
							ClickButton (InteractionType.Inventory, -1, KickStarter.runtimeInventory.selectedItem.id);
							
							if (KickStarter.settingsManager.inventoryDisableLeft)
							{
								KickStarter.runtimeInventory.SetNull ();
							}
						}
					}
					else if (KickStarter.settingsManager.inventoryDragDrop && IsDroppingInventory ())
					{
						// Perform "Use Inventory" interaction (Drag n' drop mode)
						ClickButton (InteractionType.Inventory, -1, KickStarter.runtimeInventory.selectedItem.id);
					}
				}
				
				else if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction)
				{
					if (KickStarter.runtimeInventory.selectedItem != null && KickStarter.settingsManager.CanSelectItems (false))
					{
						if (KickStarter.playerInput.mouseState == MouseState.SingleClick || KickStarter.playerInput.mouseState == MouseState.DoubleClick)
						{
							// Perform "Use Inventory" interaction
							ClickButton (InteractionType.Inventory, -1, KickStarter.runtimeInventory.selectedItem.id);
							
							if (KickStarter.settingsManager.inventoryDisableLeft)
							{
								KickStarter.runtimeInventory.SetNull ();
							}
							return;
						}
						else if (KickStarter.settingsManager.inventoryDragDrop && IsDroppingInventory ())
						{
							// Perform "Use Inventory" interaction
							ClickButton (InteractionType.Inventory, -1, KickStarter.runtimeInventory.selectedItem.id);
							
							KickStarter.runtimeInventory.SetNull ();
							return;
						}
					}
					else if (KickStarter.runtimeInventory.selectedItem == null && hotspot.IsSingleInteraction ())
					{
						// Perform "Use" interaction
						ClickButton (InteractionType.Use, -1, -1);
						
						if (KickStarter.settingsManager.inventoryDisableLeft)
						{
							KickStarter.runtimeInventory.SetNull ();
						}
					}
				}
			}
		}
		
		
		public void ClickButton (InteractionType _interactionType, int selectedCursorID, int selectedItemID)
		{
			inPreInteractionCutscene = false;
			StopCoroutine ("UseObject");
			
			if (KickStarter.player)
			{
				KickStarter.player.EndPath ();
			}
			
			if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction && KickStarter.settingsManager.selectInteractions == SelectInteractions.CyclingCursorAndClickingHotspot)
			{
				if (KickStarter.settingsManager.autoCycleWhenInteract)
				{
					SetNextInteraction ();
				}
				else
				{
					ResetInteractionIndex ();
				}
			}
			else if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot && KickStarter.settingsManager.autoCycleWhenInteract)
			{
				KickStarter.playerCursor.ResetSelectedCursor ();
			}
			
			KickStarter.playerInput.mouseState = MouseState.Normal;
			KickStarter.playerInput.ResetClick ();
			button = null;
			
			if (_interactionType == InteractionType.Use)
			{
				if (selectedCursorID == -1)
				{
					button = hotspot.GetFirstUseButton ();
				}
				else
				{
					foreach (Button _button in hotspot.useButtons)
					{
						if (_button.iconID == selectedCursorID)
						{
							button = _button;
							break;
						}
					}
					
					if (button == null && KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot)
					{
						AdvGame.RunActionListAsset (KickStarter.cursorManager.GetUnhandledInteraction (selectedCursorID));
						KickStarter.runtimeInventory.SetNull ();
						KickStarter.player.ClearHeadTurnTarget (HeadFacing.Hotspot, false);
						return;
					}
				}
			}
			else if (_interactionType == InteractionType.Examine)
			{
				button = hotspot.lookButton;
			}
			else if (_interactionType == InteractionType.Inventory && selectedItemID >= 0)
			{
				foreach (Button invButton in hotspot.invButtons)
				{
					if (invButton.invID == selectedItemID && !invButton.isDisabled)
					{
						if ((KickStarter.runtimeInventory.IsGivingItem () && invButton.selectItemMode == SelectItemMode.Give) ||
						    (!KickStarter.runtimeInventory.IsGivingItem () && invButton.selectItemMode == SelectItemMode.Use))
						{
							button = invButton;
							break;
						}
					}
				}
			}
			
			if (button != null && button.isDisabled)
			{
				button = null;
				
				if (_interactionType != InteractionType.Inventory)
				{
					KickStarter.player.ClearHeadTurnTarget (HeadFacing.Hotspot, false);
					return;
				}
			}
			
			StartCoroutine ("UseObject", selectedItemID);
		}
		
		
		private IEnumerator UseObject (int selectedItemID)
		{
			bool doRun = false;
			if (KickStarter.playerInput.hotspotMovingTo == hotspot)
			{
				doRun = true;
			}
			
			if (KickStarter.playerInput != null && KickStarter.playerInput.runLock == PlayerMoveLock.AlwaysWalk)
			{
				doRun = false;
			}
			
			if (KickStarter.player)
			{
				if (button != null && !button.isBlocking && (button.playerAction == PlayerAction.WalkToMarker || button.playerAction == PlayerAction.WalkTo) && KickStarter.settingsManager.movementMethod != MovementMethod.UltimateFPS)
				{
					KickStarter.stateHandler.gameState = GameState.Normal;
					KickStarter.playerInput.hotspotMovingTo = hotspot;
				}
				else
				{
					if (button != null && button.playerAction != PlayerAction.DoNothing)
					{
						inPreInteractionCutscene = true;
						KickStarter.stateHandler.gameState = GameState.Cutscene;
					}
					KickStarter.playerInput.hotspotMovingTo = null;
				}
			}
			
			Hotspot _hotspot = hotspot;
			hotspot.Deselect ();
			hotspot = null;
			
			if (KickStarter.player)
			{
				if (button != null && button.playerAction != PlayerAction.DoNothing)
				{
					Vector3 lookVector = Vector3.zero;
					Vector3 targetPos = _hotspot.transform.position;
					
					if (KickStarter.settingsManager.ActInScreenSpace ())
					{
						lookVector = AdvGame.GetScreenDirection (KickStarter.player.transform.position, _hotspot.transform.position);
					}
					else
					{
						lookVector = targetPos - KickStarter.player.transform.position;
						lookVector.y = 0;
					}
					
					KickStarter.player.SetLookDirection (lookVector, false);
					
					if (button.playerAction == PlayerAction.TurnToFace)
					{
						while (KickStarter.player.IsTurning ())
						{
							yield return new WaitForFixedUpdate ();			
						}
					}
					
					if (button.playerAction == PlayerAction.WalkToMarker && _hotspot.walkToMarker)
					{
						if (Vector3.Distance (KickStarter.player.transform.position, _hotspot.walkToMarker.transform.position) > (1.05f - KickStarter.settingsManager.destinationAccuracy))
						{
							if (KickStarter.navigationManager)
							{
								Vector3[] pointArray;
								Vector3 targetPosition = _hotspot.walkToMarker.transform.position;
								
								if (KickStarter.settingsManager.ActInScreenSpace ())
								{
									targetPosition = AdvGame.GetScreenNavMesh (targetPosition);
								}
								
								pointArray = KickStarter.navigationManager.navigationEngine.GetPointsArray (KickStarter.player, KickStarter.player.transform.position, targetPosition);
								KickStarter.player.MoveAlongPoints (pointArray, doRun);
								targetPos = pointArray [pointArray.Length - 1];
							}
							
							while (KickStarter.player.activePath)
							{
								yield return new WaitForFixedUpdate ();
							}
						}
						
						if (button.faceAfter)
						{
							lookVector = _hotspot.walkToMarker.transform.forward;
							lookVector.y = 0;
							KickStarter.player.EndPath (); //Halt ();
							KickStarter.player.SetLookDirection (lookVector, false);
							
							while (KickStarter.player.IsTurning ())
							{
								yield return new WaitForFixedUpdate ();			
							}
						}
					}
					
					else if (lookVector.magnitude > 2f && button.playerAction == PlayerAction.WalkTo)
					{
						if (KickStarter.navigationManager)
						{
							Vector3[] pointArray;
							Vector3 targetPosition = _hotspot.transform.position;
							if (_hotspot.walkToMarker)
							{
								targetPosition = _hotspot.walkToMarker.transform.position;
							}
							
							if (KickStarter.settingsManager.ActInScreenSpace ())
							{
								targetPosition = AdvGame.GetScreenNavMesh (targetPosition);
							}
							
							pointArray = KickStarter.navigationManager.navigationEngine.GetPointsArray (KickStarter.player, KickStarter.player.transform.position, targetPosition);
							KickStarter.player.MoveAlongPoints (pointArray, doRun);
							targetPos = pointArray [pointArray.Length - 1];
						}
						
						if (button.setProximity)
						{
							button.proximity = Mathf.Max (button.proximity, 1f);
							targetPos.y = KickStarter.player.transform.position.y;
							
							while (Vector3.Distance (KickStarter.player.transform.position, targetPos) > button.proximity && KickStarter.player.activePath)
							{
								yield return new WaitForFixedUpdate ();
							}
						}
						else
						{
							yield return new WaitForSeconds (0.6f);
						}
						
						if (button.faceAfter)
						{
							if (KickStarter.settingsManager.ActInScreenSpace ())
							{
								lookVector = AdvGame.GetScreenDirection (KickStarter.player.transform.position, _hotspot.transform.position);
							}
							else
							{
								lookVector = _hotspot.transform.position - KickStarter.player.transform.position;
								lookVector.y = 0;
							}
							
							KickStarter.player.EndPath (); ///Halt ();
							KickStarter.player.SetLookDirection (lookVector, false);
							
							while (KickStarter.player.IsTurning ())
							{
								yield return new WaitForFixedUpdate ();			
							}
						}
					}
				}
				else
				{
					KickStarter.player.charState = CharState.Decelerate;
				}
				
				KickStarter.player.EndPath ();
				KickStarter.playerInput.hotspotMovingTo = null;
				yield return new WaitForSeconds (0.1f);
				KickStarter.player.EndPath ();
				KickStarter.playerInput.hotspotMovingTo = null;
			}
			
			_hotspot.Deselect ();
			hotspot = null;
			inPreInteractionCutscene = false;
			KickStarter.playerMenus.SetInteractionMenus (false);
			
			if (KickStarter.player)
			{
				KickStarter.player.ClearHeadTurnTarget (HeadFacing.Hotspot, false);
			}
			
			if (button == null)
			{
				// Unhandled event
				if (selectedItemID >= 0 && KickStarter.runtimeInventory.GetItem (selectedItemID) != null && KickStarter.runtimeInventory.GetItem (selectedItemID).unhandledActionList)
				{
					ActionListAsset unhandledActionList = KickStarter.runtimeInventory.GetItem (selectedItemID).unhandledActionList;
					KickStarter.runtimeInventory.SetNull ();
					AdvGame.RunActionListAsset (unhandledActionList);	
				}
				else if (selectedItemID >= 0 && KickStarter.runtimeInventory.unhandledGive && KickStarter.runtimeInventory.IsGivingItem ())
				{
					KickStarter.runtimeInventory.SetNull ();
					AdvGame.RunActionListAsset (KickStarter.runtimeInventory.unhandledGive);
				}
				else if (selectedItemID >= 0 && KickStarter.runtimeInventory.unhandledHotspot && !KickStarter.runtimeInventory.IsGivingItem ())
				{
					KickStarter.runtimeInventory.SetNull ();
					AdvGame.RunActionListAsset (KickStarter.runtimeInventory.unhandledHotspot);	
				}
				else
				{
					KickStarter.stateHandler.gameState = GameState.Normal;
					if (KickStarter.settingsManager.inventoryDragDrop)
					{
						KickStarter.runtimeInventory.SetNull ();
					}
				}
			}
			else
			{
				KickStarter.runtimeInventory.SetNull ();
				
				if (_hotspot.interactionSource == InteractionSource.AssetFile)
				{
					AdvGame.RunActionListAsset (button.assetFile);
				}
				else if (_hotspot.interactionSource == InteractionSource.CustomScript)
				{
					if (button.customScriptObject != null && button.customScriptFunction != "")
					{
						button.customScriptObject.SendMessage (button.customScriptFunction);
					}
				}
				else if (_hotspot.interactionSource == InteractionSource.InScene)
				{
					if (button.interaction)
					{
						button.interaction.Interact ();
					}
					else
					{
						KickStarter.stateHandler.gameState = GameState.Normal;
					}
				}
			}
			
			button = null;
		}
		
		
		public string GetLabel (int languageNumber)
		{
			string label = "";
			
			if (KickStarter.stateHandler.gameState == GameState.DialogOptions)
			{
				return "";
			}
			
			if (KickStarter.cursorManager && KickStarter.runtimeInventory.selectedItem != null && KickStarter.cursorManager.inventoryHandling != InventoryHandling.ChangeCursor && KickStarter.settingsManager.CanSelectItems (false))
			{
				label = KickStarter.runtimeInventory.GetHotspotPrefixLabel (KickStarter.runtimeInventory.selectedItem, KickStarter.runtimeInventory.selectedItem.GetLabel (languageNumber), languageNumber, true);
			}
			else if (KickStarter.cursorManager && KickStarter.cursorManager.addHotspotPrefix)
			{
				if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive)
				{
					if (hotspot && hotspot.provideUseInteraction)
					{
						label = KickStarter.cursorManager.GetLabelFromID (hotspot.GetFirstUseButton ().iconID, languageNumber);
					}
				}
				else if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot)
				{
					label = KickStarter.cursorManager.GetLabelFromID (KickStarter.playerCursor.GetSelectedCursorID (), languageNumber);
				}
				else if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction)
				{
					if (KickStarter.settingsManager.selectInteractions == SelectInteractions.CyclingCursorAndClickingHotspot)
					{
						label = KickStarter.cursorManager.GetLabelFromID (KickStarter.playerCursor.GetSelectedCursorID (), languageNumber);
					}
				}
			}
			
			if (KickStarter.playerCursor.GetSelectedCursor () == -1 && KickStarter.cursorManager.addWalkPrefix && !KickStarter.playerInput.interactionMenuIsOn)
			{
				label = SpeechManager.GetTranslation (KickStarter.cursorManager.walkPrefix.label, KickStarter.cursorManager.walkPrefix.lineID, languageNumber) + " ";
			}
			
			if (KickStarter.runtimeInventory.hoverItem != null)
			{
				if (KickStarter.runtimeInventory.selectedItem == null || KickStarter.runtimeInventory.hoverItem != KickStarter.runtimeInventory.selectedItem)
				{
					label += KickStarter.runtimeInventory.hoverItem.GetLabel (languageNumber);
				}
				else
				{
					label = "";
				}
			}
			else if (hotspot != null)
			{
				label += hotspot.GetName (languageNumber);
			}
			else if (KickStarter.cursorManager && KickStarter.runtimeInventory.selectedItem != null && KickStarter.cursorManager.inventoryHandling != InventoryHandling.ChangeCursor && KickStarter.settingsManager.CanSelectItems (false) && KickStarter.cursorManager.onlyShowInventoryLabelOverHotspots)
			{
				label = "";
			}
			
			return (label);		
		}
		
		
		public void StopInteraction ()
		{
			button = null;
			inPreInteractionCutscene = false;
			StopCoroutine ("UseObject");
		}
		
		
		public Vector2 GetHotspotScreenCentre ()
		{
			if (hotspot)
			{
				Vector2 screenPos = hotspot.GetIconScreenPosition ();
				return new Vector2 (screenPos.x / Screen.width, 1f - (screenPos.y / Screen.height));
			}
			return Vector2.zero;
		}
		
		
		public bool IsMouseOverHotspot ()
		{
			// Return false if we're in "Walk mode" anyway
			if (KickStarter.settingsManager && KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot
			    && KickStarter.playerCursor && KickStarter.playerCursor.GetSelectedCursor () == -1)
			{
				return false;
			}
			
			if (KickStarter.settingsManager && KickStarter.settingsManager.IsUnity2D ())
			{
				RaycastHit2D hit = new RaycastHit2D ();
				
				if (KickStarter.mainCamera.IsOrthographic ())
				{
					hit = Physics2D.Raycast (Camera.main.ScreenToWorldPoint (KickStarter.playerInput.GetMousePosition ()), Vector2.zero, KickStarter.settingsManager.navMeshRaycastLength);
				}
				else
				{
					Vector3 pos = KickStarter.playerInput.GetMousePosition ();
					pos.z = -Camera.main.transform.position.z;
					hit = Physics2D.Raycast (Camera.main.ScreenToWorldPoint(pos), Vector2.zero, KickStarter.settingsManager.navMeshRaycastLength, 1 << LayerMask.NameToLayer (KickStarter.settingsManager.hotspotLayer));
				}
				
				if (hit.collider != null && hit.collider.gameObject.GetComponent <Hotspot>())
				{
					return true;
				}
			}
			else
			{
				Ray ray = Camera.main.ScreenPointToRay (KickStarter.playerInput.GetMousePosition ());
				RaycastHit hit;
				
				if (Physics.Raycast (ray, out hit, KickStarter.settingsManager.hotspotRaycastLength, 1 << LayerMask.NameToLayer (KickStarter.settingsManager.hotspotLayer)))
				{
					if (hit.collider.gameObject.GetComponent <Hotspot>())
					{
						return true;
					}
				}
				
				// Include moveables in query
				if (Physics.Raycast (ray, out hit, KickStarter.settingsManager.moveableRaycastLength, 1 << LayerMask.NameToLayer (KickStarter.settingsManager.hotspotLayer)))
				{
					if (hit.collider.gameObject.GetComponent <DragBase>())
					{
						return true;
					}
				}
			}
			
			return false;
		}
		
		
		public bool IsDroppingInventory ()
		{
			if (!KickStarter.settingsManager.CanSelectItems (false))
			{
				return false;
			}
			
			if (KickStarter.stateHandler.gameState == GameState.Cutscene || KickStarter.stateHandler.gameState == GameState.DialogOptions)
			{
				return false;
			}
			
			if (KickStarter.runtimeInventory.selectedItem == null || !KickStarter.runtimeInventory.localItems.Contains (KickStarter.runtimeInventory.selectedItem))
			{
				return false;
			}
			
			if (KickStarter.settingsManager.inventoryDragDrop && KickStarter.playerInput.mouseState == MouseState.Normal && KickStarter.playerInput.dragState == DragState.Inventory)
			{
				return true;
			}
			
			if (KickStarter.settingsManager.inventoryDragDrop && KickStarter.playerInput.CanClick () && KickStarter.playerInput.mouseState == MouseState.Normal && KickStarter.playerInput.dragState == DragState.None)
			{
				return true;
			}
			
			if (KickStarter.playerInput.mouseState == MouseState.SingleClick && KickStarter.settingsManager.inventoryDisableLeft)
			{
				return true;
			}
			
			if (KickStarter.playerInput.mouseState == MouseState.RightClick && KickStarter.settingsManager.rightClickInventory == RightClickInventory.DeselectsItem && (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive || KickStarter.settingsManager.inventoryInteractions == InventoryInteractions.Single))
			{
				return true;
			}
			
			return false;
		}
		
		
		public Hotspot GetActiveHotspot ()
		{
			return hotspot;
		}
		
		
		public int GetActiveUseButtonIconID ()
		{
			if (KickStarter.settingsManager.SelectInteractionMethod () == SelectInteractions.CyclingCursorAndClickingHotspot)
			{
				if (KickStarter.runtimeInventory.hoverItem != null && KickStarter.settingsManager.inventoryInteractions == AC.InventoryInteractions.Multiple)
				{
					if (interactionIndex == -1)
					{
						if (KickStarter.runtimeInventory.hoverItem.interactions.Count == 0)
						{
							return -1;
						}
						else
						{
							interactionIndex = 0;
							return 0;
						}
					}
					
					if (interactionIndex < KickStarter.runtimeInventory.hoverItem.interactions.Count)
					{
						return KickStarter.runtimeInventory.hoverItem.interactions [interactionIndex].icon.id;
					}
				}
				else if (GetActiveHotspot ())
				{
					if (interactionIndex == -1)
					{
						if (GetActiveHotspot ().GetFirstUseButton () == null)
						{
							return -1;
						}
						else
						{
							interactionIndex = GetActiveHotspot ().FindFirstEnabledInteraction ();
							return interactionIndex;
						}
					}
					
					if (interactionIndex < GetActiveHotspot ().useButtons.Count)
					{
						return GetActiveHotspot ().useButtons [interactionIndex].iconID;
					}
				}
			}
			else if (KickStarter.settingsManager.SelectInteractionMethod () == SelectInteractions.CyclingMenuAndClickingHotspot)
			{
				if (KickStarter.runtimeInventory.hoverItem != null && KickStarter.settingsManager.inventoryInteractions == AC.InventoryInteractions.Multiple)
				{
					if (interactionIndex == -1)
					{
						return -1;
					}
					
					if (interactionIndex < KickStarter.runtimeInventory.hoverItem.interactions.Count)
					{
						return KickStarter.runtimeInventory.hoverItem.interactions [interactionIndex].icon.id;
					}
				}
				else if (GetActiveHotspot ())
				{
					if (interactionIndex == -1)
					{
						if (GetActiveHotspot ().GetFirstUseButton () == null)
						{
							//return -1;
							return GetActiveHotspot ().FindFirstEnabledInteraction ();
						}
						else
						{
							interactionIndex = 0;
							return 0;
						}
					}
					
					if (interactionIndex < GetActiveHotspot ().useButtons.Count)
					{
						return GetActiveHotspot ().useButtons [interactionIndex].iconID;
					}
				}
			}
			return -1;
		}
		
		
		public int GetActiveInvButtonID ()
		{
			if (KickStarter.settingsManager.SelectInteractionMethod () == SelectInteractions.CyclingCursorAndClickingHotspot)
			{
				if (KickStarter.runtimeInventory.hoverItem != null && KickStarter.settingsManager.inventoryInteractions == AC.InventoryInteractions.Multiple)
				{
					if (interactionIndex >= KickStarter.runtimeInventory.hoverItem.interactions.Count && KickStarter.runtimeInventory.matchingInvInteractions.Count > 0)
					{
						int combineIndex = KickStarter.runtimeInventory.matchingInvInteractions [interactionIndex - KickStarter.runtimeInventory.hoverItem.interactions.Count];
						return KickStarter.runtimeInventory.hoverItem.combineID [combineIndex];
					}
				}
				else if (GetActiveHotspot ())
				{
					if (interactionIndex >= GetActiveHotspot ().useButtons.Count)
					{
						int matchingIndex = interactionIndex - GetActiveHotspot ().useButtons.Count;
						if (matchingIndex < KickStarter.runtimeInventory.matchingInvInteractions.Count)
						{
							return GetActiveHotspot ().invButtons [KickStarter.runtimeInventory.matchingInvInteractions [matchingIndex]].invID;
						}
					}
				}
			}
			else
			{
				// Cycle menus
				
				if (KickStarter.runtimeInventory.hoverItem != null && KickStarter.settingsManager.inventoryInteractions == AC.InventoryInteractions.Multiple)
				{
					if (interactionIndex >= KickStarter.runtimeInventory.hoverItem.interactions.Count && KickStarter.runtimeInventory.matchingInvInteractions.Count > 0)
					{
						return KickStarter.runtimeInventory.hoverItem.combineID [KickStarter.runtimeInventory.matchingInvInteractions [interactionIndex - KickStarter.runtimeInventory.hoverItem.interactions.Count]];
					}
				}
				else if (GetActiveHotspot ())
				{
					if (interactionIndex >= GetActiveHotspot ().useButtons.Count)
					{
						return GetActiveHotspot ().invButtons [KickStarter.runtimeInventory.matchingInvInteractions [interactionIndex - GetActiveHotspot ().useButtons.Count]].invID;
					}
				}
			}
			return -1;
		}
		
		
		public void SetNextInteraction ()
		{
			if (KickStarter.settingsManager.SelectInteractionMethod () == SelectInteractions.CyclingCursorAndClickingHotspot)
			{
				if (KickStarter.runtimeInventory.selectedItem != null && KickStarter.runtimeInventory.hoverItem == null && hotspot == null)
				{
					return;
				}
				
				if (KickStarter.runtimeInventory.hoverItem != null && KickStarter.settingsManager.inventoryInteractions == InventoryInteractions.Single)
				{
					return;
				}
				
				if (KickStarter.runtimeInventory.hoverItem != null)
				{
					interactionIndex = KickStarter.runtimeInventory.hoverItem.GetNextInteraction (interactionIndex, KickStarter.runtimeInventory.matchingInvInteractions.Count);
				}
				else if (GetActiveHotspot () != null)
				{
					interactionIndex = GetActiveHotspot ().GetNextInteraction (interactionIndex, KickStarter.runtimeInventory.matchingInvInteractions.Count);
				}
				
				if (!KickStarter.settingsManager.cycleInventoryCursors && GetActiveInvButtonID () >= 0)
				{
					interactionIndex = -1;
				}
				else
				{
					KickStarter.runtimeInventory.SelectItemByID (GetActiveInvButtonID (), SelectItemMode.Use);
				}
				
				if (KickStarter.runtimeInventory.hoverItem != null)
				{
					KickStarter.runtimeInventory.hoverItem.lastInteractionIndex = interactionIndex;
				}
				else if (GetActiveHotspot () != null)
				{
					GetActiveHotspot ().lastInteractionIndex = interactionIndex;
				}
			}
			else
			{
				// Cycle menus
				if (KickStarter.runtimeInventory.hoverItem != null)
				{
					interactionIndex = KickStarter.runtimeInventory.hoverItem.GetNextInteraction (interactionIndex, KickStarter.runtimeInventory.matchingInvInteractions.Count);
				}
				else if (GetActiveHotspot () != null)
				{
					if (KickStarter.settingsManager.cycleInventoryCursors)
					{
						interactionIndex = GetActiveHotspot ().GetNextInteraction (interactionIndex, KickStarter.runtimeInventory.matchingInvInteractions.Count);
					}
					else
					{
						interactionIndex = GetActiveHotspot ().GetNextInteraction (interactionIndex, 0);
					}
				}
			}
		}
		
		
		public void RestoreInventoryInteraction ()
		{
			if (KickStarter.runtimeInventory.selectedItem != null && KickStarter.settingsManager.CanSelectItems (false))
			{
				return;
			}
			
			if (KickStarter.settingsManager.SelectInteractionMethod () != AC.SelectInteractions.CyclingCursorAndClickingHotspot)
			{
				return;
			}
			
			if (KickStarter.runtimeInventory.hoverItem != null)
			{
				interactionIndex = KickStarter.runtimeInventory.hoverItem.lastInteractionIndex;
				if (!KickStarter.settingsManager.cycleInventoryCursors && GetActiveInvButtonID () >= 0)
				{
					interactionIndex = -1;
				}
				else
				{
					int invID = GetActiveInvButtonID ();
					if (invID >= 0)
					{
						KickStarter.runtimeInventory.SelectItemByID (invID, SelectItemMode.Use);
					}
					else if (KickStarter.settingsManager.cycleInventoryCursors && KickStarter.settingsManager.inventoryInteractions == InventoryInteractions.Multiple)
					{
						KickStarter.runtimeInventory.SetNull ();
					}
				}
			}
		}
		
		
		private void RestoreHotspotInteraction ()
		{
			if (!KickStarter.settingsManager.cycleInventoryCursors && KickStarter.runtimeInventory.selectedItem != null)
			{
				return;
			}
			
			if (hotspot != null)
			{
				interactionIndex = hotspot.lastInteractionIndex;
				
				if (!KickStarter.settingsManager.cycleInventoryCursors && GetActiveInvButtonID () >= 0)
				{
					interactionIndex = -1;
				}
				else
				{
					int invID = GetActiveInvButtonID ();
					if (invID >= 0)
					{
						KickStarter.runtimeInventory.SelectItemByID (invID, SelectItemMode.Use);
					}
				}
			}
		}
		
		
		public void SetPreviousInteraction ()
		{
			if (KickStarter.runtimeInventory.hoverItem != null)
			{
				interactionIndex = KickStarter.runtimeInventory.hoverItem.GetPreviousInteraction (interactionIndex, KickStarter.runtimeInventory.matchingInvInteractions.Count);
			}
			else if (GetActiveHotspot () != null)
			{
				if (KickStarter.settingsManager.cycleInventoryCursors)
				{
					interactionIndex = GetActiveHotspot ().GetPreviousInteraction (interactionIndex, KickStarter.runtimeInventory.matchingInvInteractions.Count);
				}
				else
				{
					interactionIndex = GetActiveHotspot ().GetPreviousInteraction (interactionIndex, 0);
				}
			}
		}
		
		
		public void ResetInteractionIndex ()
		{
			interactionIndex = -1;
			
			if (GetActiveHotspot ())
			{
				interactionIndex = GetActiveHotspot ().FindFirstEnabledInteraction ();
			}
			else if (KickStarter.runtimeInventory.hoverItem != null)// && KickStarter.settingsManager.SelectInteractionMethod () == SelectInteractions.CyclingMenuAndClickingHotspot && KickStarter.settingsManager.inventoryInteractions == InventoryInteractions.Multiple)
			{
				interactionIndex = 0;
			}
		}
		
		
		public int GetInteractionIndex ()
		{
			return interactionIndex;
		}
		
		
		public void SetInteractionIndex (int _interactionIndex)
		{
			interactionIndex = _interactionIndex;
		}
		
		
		private void ClickHotspotToInteract ()
		{
			int invID = GetActiveInvButtonID ();
			
			if (invID == -1)
			{
				ClickButton (InteractionType.Use, GetActiveUseButtonIconID (), -1);
			}
			else
			{
				ClickButton (InteractionType.Inventory, -1, invID);
			}
		}
		
		
		public void ClickInvItemToInteract ()
		{
			int invID = GetActiveInvButtonID ();
			if (invID == -1)
			{
				KickStarter.runtimeInventory.RunInteraction (GetActiveUseButtonIconID ());
			}
			else
			{
				KickStarter.runtimeInventory.Combine (KickStarter.runtimeInventory.hoverItem, invID);
			}
		}
		
		
		public void ClickInteractionIcon (AC.Menu _menu, int iconID)
		{
			if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive)
			{
				Debug.LogWarning ("This element is not compatible with the Context-Sensitive interaction method.");
			}
			else if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot)
			{
				KickStarter.runtimeInventory.SetNull ();
				KickStarter.playerCursor.SetCursorFromID (iconID);
			}
			else if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction)
			{
				if (KickStarter.settingsManager.SelectInteractionMethod () != SelectInteractions.ClickingMenu)
				{
					return;
				}
				if (KickStarter.runtimeInventory.hoverItem != null)
				{
					_menu.ForceOff ();
					KickStarter.runtimeInventory.RunInteraction (iconID);
				}
				else if (GetActiveHotspot ())
				{
					_menu.ForceOff ();
					ClickButton (InteractionType.Use, iconID, -1);
				}
			}
		}
		
	}
	
}