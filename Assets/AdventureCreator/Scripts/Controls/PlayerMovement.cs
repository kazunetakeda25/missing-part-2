/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"PlayerMovement.cs"
 * 
 *	This script analyses the variables in PlayerInput, and moves the character
 *	based on the control style, defined in the SettingsManager.
 *	To move the Player during cutscenes, a PlayerPath object must be defined.
 *	This Path will dynamically change based on where the Player must travel to.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{

	public class PlayerMovement : MonoBehaviour
	{

		private FirstPersonCamera firstPersonCamera;

		
		private void Start ()
		{
			AssignFPCamera ();
		}


		public void UpdateFPCamera ()
		{
			if (firstPersonCamera != null)
			{
				firstPersonCamera._Update ();
			}
		}


		public void AssignFPCamera ()
		{
			if (KickStarter.player)
			{
				firstPersonCamera = KickStarter.player.GetComponentInChildren<FirstPersonCamera>();
			}
		}


		public void UpdatePlayerMovement ()
		{
			if (KickStarter.settingsManager && KickStarter.player && KickStarter.playerInput && KickStarter.playerInteraction)
			{
				if (KickStarter.settingsManager.movementMethod == MovementMethod.UltimateFPS)
				{
					UFPSControlPlayer ();
					return;
				}

				if (!KickStarter.playerInput.mouseIsOnScreen)
				{
					return;
				}

				if (KickStarter.playerInput.activeArrows == null)
				{
					if (KickStarter.playerInput.mouseState == MouseState.SingleClick && !KickStarter.playerInput.interactionMenuIsOn && !KickStarter.playerInput.mouseOverMenu && !KickStarter.playerInteraction.IsMouseOverHotspot ())
					{
						if (KickStarter.playerInput.hotspotMovingTo != null)
						{
							StopMovingToHotspot ();
						}

						KickStarter.playerInteraction.DisableHotspot (false);
					}

					if (KickStarter.playerInput.hotspotMovingTo != null && KickStarter.settingsManager.movementMethod != MovementMethod.PointAndClick && KickStarter.playerInput.moveKeys != Vector2.zero)
					{
						StopMovingToHotspot ();
					}

					if (KickStarter.settingsManager.movementMethod == MovementMethod.Direct)
					{
						if (KickStarter.settingsManager.inputMethod == InputMethod.TouchScreen)
						{
							DragPlayer (true);
						}
						else
						{
							if (KickStarter.player.GetPath () == null || !KickStarter.player.lockedPath)
							{
								// Normal gameplay
								DirectControlPlayer (true);
							}
							else
							{
								// Move along pre-determined path
								DirectControlPlayerPath ();
							}
						}
					}

					else if (KickStarter.settingsManager.movementMethod == MovementMethod.Drag)
					{
						DragPlayer (true);
					}

					else if (KickStarter.settingsManager.movementMethod == MovementMethod.StraightToCursor)
					{
						MoveStraightToCursor ();
					}
					
					else if (KickStarter.settingsManager.movementMethod == MovementMethod.PointAndClick)
					{
						PointControlPlayer ();
					}
					
					else if (KickStarter.settingsManager.movementMethod == MovementMethod.FirstPerson)
					{
						if (KickStarter.settingsManager.inputMethod == InputMethod.TouchScreen)
						{
							FirstPersonControlPlayer ();

							if (KickStarter.settingsManager.dragAffects == DragAffects.Movement)
							{
								DragPlayer (false);
							}
							else
							{
								DragPlayerLook ();
							}
						}
						else
						{
							FirstPersonControlPlayer ();
							DirectControlPlayer (false);
						}
					}
				}
			}
		}


		// Straight to cursor functions

		private void MoveStraightToCursor ()
		{
			if (KickStarter.playerInput.isDownLocked && KickStarter.playerInput.isUpLocked && KickStarter.playerInput.isLeftLocked && KickStarter.playerInput.isRightLocked)
			{
				if (KickStarter.player.charState == CharState.Move)
				{
					KickStarter.player.charState = CharState.Decelerate;
				}
				return;
			}

			if (KickStarter.playerInput.dragState == DragState.None)
			{
				KickStarter.playerInput.dragSpeed = 0f;
				
				if (KickStarter.player.charState == CharState.Move && KickStarter.player.GetPath () == null)
				{
					KickStarter.player.charState = CharState.Decelerate;
				}
			}

			if (KickStarter.playerInput.mouseState == MouseState.SingleClick && KickStarter.settingsManager.singleTapStraight)
			{
				if (KickStarter.settingsManager.singleTapStraightPathfind)
				{
					PointControlPlayer ();
					return;
				}

				Vector3 clickPoint = ClickPoint (KickStarter.playerInput.GetMousePosition ());
				Vector3 moveDirection = clickPoint - KickStarter.player.transform.position;
				
				if (clickPoint != Vector3.zero)
				{
					if (moveDirection.magnitude < KickStarter.settingsManager.destinationAccuracy / 2f)
					{
						if (KickStarter.player.charState == CharState.Move)
						{
							KickStarter.player.charState = CharState.Decelerate;
						}
					}
					
					else if (moveDirection.magnitude > KickStarter.settingsManager.destinationAccuracy)
					{
						if (KickStarter.settingsManager.IsUnity2D ())
						{
							moveDirection = new Vector3 (moveDirection.x, 0f, moveDirection.y);
						}
						
						bool run = false;
						if (moveDirection.magnitude > KickStarter.settingsManager.dragRunThreshold)
						{
							run = true;
						}
						
						if (KickStarter.playerInput.runLock == PlayerMoveLock.AlwaysRun)
						{
							run = true;
						}
						else if (KickStarter.playerInput.runLock == PlayerMoveLock.AlwaysWalk)
						{
							run = false;
						}

						List<Vector3> pointArray = new List<Vector3>();
						pointArray.Add (clickPoint);
						KickStarter.player.MoveAlongPoints (pointArray.ToArray (), run);
					}
				}
			}

			else if (KickStarter.playerInput.dragState == DragState.Player && (!KickStarter.settingsManager.singleTapStraight || KickStarter.playerInput.CanClick ()))
			{
				Vector3 clickPoint = ClickPoint (KickStarter.playerInput.GetMousePosition ());
				Vector3 moveDirection = clickPoint - KickStarter.player.transform.position;

				if (clickPoint != Vector3.zero)
				{
					if (moveDirection.magnitude < KickStarter.settingsManager.destinationAccuracy / 2f)
					{
						if (KickStarter.player.charState == CharState.Move)
						{
							KickStarter.player.charState = CharState.Decelerate;
						}

						if (KickStarter.player.activePath)
						{
							KickStarter.player.EndPath ();
						}
					}

					else if (moveDirection.magnitude > KickStarter.settingsManager.destinationAccuracy)
					{
						if (KickStarter.settingsManager.IsUnity2D ())
						{
							moveDirection = new Vector3 (moveDirection.x, 0f, moveDirection.y);
						}

						bool run = false;
						if (moveDirection.magnitude > KickStarter.settingsManager.dragRunThreshold)
						{
							run = true;
						}

						if (KickStarter.playerInput.runLock == PlayerMoveLock.AlwaysRun)
						{
							run = true;
						}
						else if (KickStarter.playerInput.runLock == PlayerMoveLock.AlwaysWalk)
						{
							run = false;
						}

						KickStarter.player.isRunning = run;
						KickStarter.player.charState = CharState.Move;
						
						KickStarter.player.SetLookDirection (moveDirection, false);
						KickStarter.player.SetMoveDirectionAsForward ();

						if (KickStarter.player.activePath)
						{
							KickStarter.player.EndPath ();
						}
					}
				}
				else
				{
					if (KickStarter.player.charState == CharState.Move)
					{
						KickStarter.player.charState = CharState.Decelerate;
					}

					if (KickStarter.player.activePath)
					{
						KickStarter.player.EndPath ();
					}
				}
			}
		}


		public Vector3 ClickPoint (Vector2 mousePosition, bool onNavMesh = false)
		{
			if (KickStarter.sceneSettings.navigationMethod == AC_NavigationMethod.PolygonCollider)
			{
				RaycastHit2D hit;
				if (KickStarter.mainCamera.IsOrthographic ())
				{
					if (onNavMesh)
					{
						hit = Physics2D.Raycast (Camera.main.ScreenToWorldPoint (new Vector2 (mousePosition.x, mousePosition.y)), Vector2.zero, KickStarter.settingsManager.navMeshRaycastLength);
					}
					else
					{
						hit = Physics2D.Raycast (Camera.main.ScreenToWorldPoint (new Vector2 (mousePosition.x, mousePosition.y)), Vector2.zero, KickStarter.settingsManager.navMeshRaycastLength, 1 << LayerMask.NameToLayer (KickStarter.settingsManager.navMeshLayer));
					}
				}
				else
				{
					Vector3 pos = mousePosition;
					pos.z = KickStarter.player.transform.position.z - Camera.main.transform.position.z;
					hit = Physics2D.Raycast (Camera.main.ScreenToWorldPoint(pos), Vector2.zero);
				}
				
				if (hit.collider != null)
				{
					return hit.point;
				}
			}
			else
			{
				Ray ray = Camera.main.ScreenPointToRay (mousePosition);
				RaycastHit hit = new RaycastHit();

				if (onNavMesh)
				{
					if (KickStarter.settingsManager && KickStarter.sceneSettings && Physics.Raycast (ray, out hit, KickStarter.settingsManager.navMeshRaycastLength, 1 << LayerMask.NameToLayer (KickStarter.settingsManager.navMeshLayer)))
					{
						return hit.point;
					}
				}
				else
				{
					if (KickStarter.settingsManager && KickStarter.sceneSettings && Physics.Raycast (ray, out hit, KickStarter.settingsManager.navMeshRaycastLength))
					{
						return hit.point;
					}
				}
			}
			
			return Vector3.zero;
		}
		
		
		// Drag functions

		private void DragPlayer (bool doRotation)
		{
			if (KickStarter.playerInput.dragState == DragState.None)
			{
				KickStarter.playerInput.dragSpeed = 0f;
				
				if (KickStarter.player.charState == CharState.Move)
				{
					KickStarter.player.charState = CharState.Decelerate;
				}
			}
			
			if (KickStarter.playerInput.dragState == DragState.Player)
			{
				Vector3 moveDirectionInput = Vector3.zero;
				
				if (KickStarter.settingsManager.IsTopDown ())
				{
					moveDirectionInput = (KickStarter.playerInput.moveKeys.y * Vector3.forward) + (KickStarter.playerInput.moveKeys.x * Vector3.right);
				}
				else
				{
					moveDirectionInput = (KickStarter.playerInput.moveKeys.y * KickStarter.mainCamera.ForwardVector ()) + (KickStarter.playerInput.moveKeys.x * KickStarter.mainCamera.RightVector ());
				}
				
				if (KickStarter.playerInput.dragSpeed > KickStarter.settingsManager.dragWalkThreshold * 10f)
				{
					KickStarter.player.isRunning = KickStarter.playerInput.isRunning;
					KickStarter.player.charState = CharState.Move;
				
					if (doRotation)
					{
						KickStarter.player.SetLookDirection (moveDirectionInput, false);
						KickStarter.player.SetMoveDirectionAsForward ();
					}
					else
					{
						if (KickStarter.playerInput.GetDragVector ().y < 0f)
						{
							KickStarter.player.SetMoveDirectionAsForward ();
						}
						else
						{
							KickStarter.player.SetMoveDirectionAsBackward ();
						}
					}
				}
				else
				{
					if (KickStarter.player.charState == CharState.Move && KickStarter.playerInput.hotspotMovingTo == null)
					{
						KickStarter.player.charState = CharState.Decelerate;
					}
				}
			}
		}
		
		
		// Direct-control functions
		
		private void DirectControlPlayer (bool doRotation)
		{
			if (KickStarter.settingsManager.directMovementType == DirectMovementType.RelativeToCamera)
			{
				if (KickStarter.playerInput.moveKeys != Vector2.zero)
				{
					Vector3 moveDirectionInput = Vector3.zero;

					if (KickStarter.settingsManager.IsTopDown ())
					{
						moveDirectionInput = (KickStarter.playerInput.moveKeys.y * Vector3.forward) + (KickStarter.playerInput.moveKeys.x * Vector3.right);
					}
					else
					{
						if (KickStarter.settingsManager.directMovementPerspective && KickStarter.settingsManager.cameraPerspective == CameraPerspective.ThreeD)
						{
							Vector3 forwardVector = (KickStarter.player.transform.position - KickStarter.mainCamera.transform.position).normalized;
							Vector3 rightVector = -Vector3.Cross (forwardVector, KickStarter.mainCamera.transform.up);
							moveDirectionInput = (KickStarter.playerInput.moveKeys.y * forwardVector) + (KickStarter.playerInput.moveKeys.x * rightVector);
						}
						else
						{
							moveDirectionInput = (KickStarter.playerInput.moveKeys.y * KickStarter.mainCamera.ForwardVector ()) + (KickStarter.playerInput.moveKeys.x * KickStarter.mainCamera.RightVector ());
						}
					}

					KickStarter.player.isRunning = KickStarter.playerInput.isRunning;
					KickStarter.player.charState = CharState.Move;

					if (!KickStarter.playerInput.cameraLockSnap)
					{
						if (doRotation)
						{
							KickStarter.player.SetLookDirection (moveDirectionInput, false);
							KickStarter.player.SetMoveDirectionAsForward ();
						}
						else
						{
							KickStarter.player.SetMoveDirection (moveDirectionInput);
						}
					}
				}
				else if (KickStarter.player.charState == CharState.Move && KickStarter.playerInput.hotspotMovingTo == null)
				{
					KickStarter.player.charState = CharState.Decelerate;
					KickStarter.player.StopTurning ();
				}
			}
			
			else if (KickStarter.settingsManager.directMovementType == DirectMovementType.TankControls)
			{
				if (KickStarter.playerInput.moveKeys.x < -0.5f)
				{
					KickStarter.player.TankTurnLeft ();
				}
				else if (KickStarter.playerInput.moveKeys.x > 0.5f)
				{
					KickStarter.player.TankTurnRight ();
				}
				else
				{
					KickStarter.player.StopTurning ();
				}
				
				if (KickStarter.playerInput.moveKeys.y > 0f)
				{
					KickStarter.player.isRunning = KickStarter.playerInput.isRunning;
					KickStarter.player.charState = CharState.Move;
					KickStarter.player.SetMoveDirectionAsForward ();
				}
				else if (KickStarter.playerInput.moveKeys.y < 0f)
				{
					KickStarter.player.isRunning = KickStarter.playerInput.isRunning;
					KickStarter.player.charState = CharState.Move;
					KickStarter.player.SetMoveDirectionAsBackward ();
				}
				else if (KickStarter.player.charState == CharState.Move)
				{
					KickStarter.player.charState = CharState.Decelerate;
					KickStarter.player.SetMoveDirectionAsForward ();
				}
			}
		}


		private void UFPSControlPlayer ()
		{
			if (KickStarter.playerInput.moveKeys != Vector2.zero)
			{
				KickStarter.player.isRunning = KickStarter.playerInput.isRunning;
				KickStarter.player.charState = CharState.Move;
			}
			else if (KickStarter.player.charState == CharState.Move)
			{
				KickStarter.player.charState = CharState.Decelerate;
			}
		}
		
		
		private void DirectControlPlayerPath ()
		{
			if (KickStarter.playerInput.moveKeys != Vector2.zero)
			{
				Vector3 moveDirectionInput = Vector3.zero;

				if (KickStarter.settingsManager.IsTopDown ())
				{
					moveDirectionInput = (KickStarter.playerInput.moveKeys.y * Vector3.forward) + (KickStarter.playerInput.moveKeys.x * Vector3.right);
				}
				else
				{
					moveDirectionInput = (KickStarter.playerInput.moveKeys.y * KickStarter.mainCamera.ForwardVector ()) + (KickStarter.playerInput.moveKeys.x * KickStarter.mainCamera.RightVector ());
				}

				if (Vector3.Dot (moveDirectionInput, KickStarter.player.GetMoveDirection ()) > 0f)
				{
					// Move along path, because movement keys are in the path's forward direction
					KickStarter.player.isRunning = KickStarter.playerInput.isRunning;
					KickStarter.player.charState = CharState.Move;
				}
			}
			else
			{
				if (KickStarter.player.charState == CharState.Move)
				{
					KickStarter.player.charState = CharState.Decelerate;
				}
			}
		}
		
		
		// Point/click functions
		
		private void PointControlPlayer ()
		{
			if (KickStarter.playerInput.IsCursorLocked ())
			{
				return;
			}

			if (KickStarter.playerInput.isDownLocked && KickStarter.playerInput.isUpLocked && KickStarter.playerInput.isLeftLocked && KickStarter.playerInput.isRightLocked)
			{
				if (KickStarter.player.GetPath () == null && KickStarter.player.charState == CharState.Move)
				{
					KickStarter.player.charState = CharState.Decelerate;
				}
				return;
			}

			if ((KickStarter.playerInput.mouseState == MouseState.SingleClick || KickStarter.playerInput.mouseState == MouseState.DoubleClick) && !KickStarter.playerInput.interactionMenuIsOn && !KickStarter.playerInput.mouseOverMenu && !KickStarter.playerInteraction.IsMouseOverHotspot () && KickStarter.playerCursor)
			{
				if (KickStarter.playerCursor.GetSelectedCursor () < 0)
				{
					if (KickStarter.settingsManager.doubleClickMovement && KickStarter.playerInput.mouseState == MouseState.SingleClick)
					{
						return;
					}

					if (KickStarter.runtimeInventory.selectedItem != null && !KickStarter.settingsManager.canMoveWhenActive)
					{
						return;
					}

					bool doubleClick = false;
					if (KickStarter.playerInput.mouseState == MouseState.DoubleClick && !KickStarter.settingsManager.doubleClickMovement)
					{
						doubleClick = true;
					}

					if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction && KickStarter.playerMenus != null)
					{
						KickStarter.playerMenus.SetInteractionMenus (false);
					}
					if (!RaycastNavMesh (KickStarter.playerInput.GetMousePosition (), doubleClick))
					{
						// Move Ray down screen until we hit something
						Vector3 simulatedMouse = KickStarter.playerInput.GetMousePosition ();
		
						if (((int) Screen.height * KickStarter.settingsManager.walkableClickRange) > 1)
						{
							for (int i=1; i<Screen.height * KickStarter.settingsManager.walkableClickRange; i+=4)
							{
								// Up
								if (RaycastNavMesh (new Vector2 (simulatedMouse.x, simulatedMouse.y - i), doubleClick))
								{
									break;
								}
								// Down
								if (RaycastNavMesh (new Vector2 (simulatedMouse.x, simulatedMouse.y + i), doubleClick))
								{
									break;
								}
								// Left
								if (RaycastNavMesh (new Vector2 (simulatedMouse.x - i, simulatedMouse.y), doubleClick))
								{
									break;
								}
								// Right
								if (RaycastNavMesh (new Vector2 (simulatedMouse.x + i, simulatedMouse.y), doubleClick))
								{
									break;
								}
								// UpLeft
								if (RaycastNavMesh (new Vector2 (simulatedMouse.x - i, simulatedMouse.y - i), doubleClick))
								{
									break;
								}
								// UpRight
								if (RaycastNavMesh (new Vector2 (simulatedMouse.x + i, simulatedMouse.y - i), doubleClick))
								{
									break;
								}
								// DownLeft
								if (RaycastNavMesh (new Vector2 (simulatedMouse.x - i, simulatedMouse.y + i), doubleClick))
								{
									break;
								}
								// DownRight
								if (RaycastNavMesh (new Vector2 (simulatedMouse.x + i, simulatedMouse.y + i), doubleClick))
								{
									break;
								}
							}
						}
					}
				}
				else if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot && KickStarter.settingsManager.autoCycleWhenInteract)
				{
					KickStarter.playerCursor.ResetSelectedCursor ();
				}

			}
			else if (KickStarter.player.GetPath () == null && KickStarter.player.charState == CharState.Move)
			{
				KickStarter.player.charState = CharState.Decelerate;
			}
		}


		private bool ProcessHit (Vector3 hitPoint, GameObject hitObject, bool run)
		{
			if (hitObject.layer != LayerMask.NameToLayer (KickStarter.settingsManager.navMeshLayer))
			{
				return false;
			}
			
			if (!run)
			{
				ShowClick (hitPoint);
			}
			
			if (KickStarter.playerInput.runLock == PlayerMoveLock.AlwaysRun)
			{
				run = true;
			}
			else if (KickStarter.playerInput.runLock == PlayerMoveLock.AlwaysWalk)
			{
				run = false;
			}

			Vector3[] pointArray = KickStarter.navigationManager.navigationEngine.GetPointsArray (KickStarter.player, KickStarter.player.transform.position, hitPoint);
			KickStarter.player.MoveAlongPoints (pointArray, run);

			return true;
		}


		private bool RaycastNavMesh (Vector3 mousePosition, bool run)
		{
			if (KickStarter.sceneSettings.navigationMethod == AC_NavigationMethod.PolygonCollider)
			{
				RaycastHit2D hit;
				if (KickStarter.mainCamera.IsOrthographic ())
				{
					hit = Physics2D.Raycast (Camera.main.ScreenToWorldPoint (new Vector2 (mousePosition.x, mousePosition.y)), Vector2.zero, KickStarter.settingsManager.navMeshRaycastLength);
				}
				else
				{
					Vector3 pos = mousePosition;
					pos.z = KickStarter.player.transform.position.z - Camera.main.transform.position.z;
					hit = Physics2D.Raycast (Camera.main.ScreenToWorldPoint(pos), Vector2.zero);
				}

				if (hit.collider != null)
				{
					return ProcessHit (hit.point, hit.collider.gameObject, run);
				}
			}
			else
			{
				Ray ray = Camera.main.ScreenPointToRay (mousePosition);
				RaycastHit hit = new RaycastHit();
				
				if (KickStarter.settingsManager && KickStarter.sceneSettings && Physics.Raycast (ray, out hit, KickStarter.settingsManager.navMeshRaycastLength))
				{
					return ProcessHit (hit.point, hit.collider.gameObject, run);
				}
			}
			
			return false;
		}


		private void ShowClick (Vector3 clickPoint)
		{
			if (KickStarter.settingsManager && KickStarter.settingsManager.clickPrefab)
			{
				Destroy (GameObject.Find (KickStarter.settingsManager.clickPrefab.name + "(Clone)"));
				Instantiate (KickStarter.settingsManager.clickPrefab, clickPoint, Quaternion.identity);
			}
		}

		
		// First-person functions
		
		private void FirstPersonControlPlayer ()
		{
			if (firstPersonCamera)
			{
				Vector2 freeAim = KickStarter.playerInput.freeAim;
				if (freeAim.magnitude > KickStarter.settingsManager.dragWalkThreshold / 10f)
				{
					freeAim.Normalize ();
					freeAim *= KickStarter.settingsManager.dragWalkThreshold / 10f;
				}

				float rotationX = KickStarter.player.transform.localEulerAngles.y + freeAim.x * firstPersonCamera.sensitivity.x;
				firstPersonCamera.rotationY -= freeAim.y * firstPersonCamera.sensitivity.y;
				//KickStarter.player.transform.localEulerAngles = new Vector3 (0, rotationX, 0);
				Quaternion rot = Quaternion.AngleAxis (rotationX, Vector3.up);
				KickStarter.player.SetRotation (rot);
			}
			else
			{
				Debug.LogWarning ("Could not find first person camera");
			}
		}


		private void DragPlayerLook ()
		{
			if (KickStarter.playerInput.isDownLocked && KickStarter.playerInput.isUpLocked && KickStarter.playerInput.isLeftLocked && KickStarter.playerInput.isRightLocked)
			{
				return;
			}

			if (KickStarter.playerInput.mouseState == MouseState.Normal)
			{
				return;
			}
			
			else if (!KickStarter.playerInput.mouseOverMenu && !KickStarter.playerInput.interactionMenuIsOn && (KickStarter.playerInput.mouseState == MouseState.RightClick || !KickStarter.playerInteraction.IsMouseOverHotspot ()))
			{
				if (KickStarter.playerInput.mouseState == MouseState.SingleClick)
				{
					KickStarter.playerInteraction.DisableHotspot (false);
				}
			}
		}


		private void StopMovingToHotspot ()
		{
			KickStarter.playerInput.hotspotMovingTo = null;
			KickStarter.player.EndPath ();
			KickStarter.player.ClearHeadTurnTarget (HeadFacing.Hotspot, false);
			KickStarter.playerInteraction.StopInteraction ();
		}
		
		
		private void OnDestroy ()
		{
			firstPersonCamera = null;
		}
		
	}

}