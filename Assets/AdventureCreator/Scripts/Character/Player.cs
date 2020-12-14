/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"Player.cs"
 * 
 *	This is attached to the Player GameObject, which must be tagged as Player.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{
	
	public class Player : Char
	{
		
		// Legacy variables
		public AnimationClip jumpAnim;
		public bool lockHotspotHeadTurning = false;
		
		// Mecanim variables
		public string jumpParameter = "Jump";
		
		public int ID;
		public bool lockedPath;
		public DetectHotspots hotspotDetector;

		private bool isTilting = false;
		private float actualTilt;
		private float targetTilt;
		private float tiltSpeed;
		private float tiltStartTime;
		
		private Transform fpCam;

		public void ResetTilt()
		{
			fpCam.localEulerAngles = Vector3.zero;
			actualTilt = 0f;
		}
		
		public void Awake ()
		{
			if (soundChild && soundChild.gameObject.GetComponent <AudioSource>())
			{
				audioSource = soundChild.gameObject.GetComponent <AudioSource>();
			}

			if (GetComponentInChildren<FirstPersonCamera>())
			{
				fpCam = GetComponentInChildren<FirstPersonCamera>().transform;
			}
			
			_Awake ();
		}


		private void Update ()
		{

			if (KickStarter.stateHandler && KickStarter.stateHandler.playerIsOff)
			{
				return;
			}
			
			if (hotspotDetector)
			{
				hotspotDetector._Update ();
			}
			
			_Update ();
		}

		
		private void FixedUpdate ()
		{
			if (KickStarter.stateHandler && KickStarter.stateHandler.playerIsOff)
			{
				return;
			}
			if (activePath && !pausePath)
			{
				if (IsTurningBeforeWalking ())
				{
					charState = CharState.Idle;
				}
				else if ((KickStarter.stateHandler && KickStarter.stateHandler.gameState == GameState.Cutscene && !lockedPath) || 
				         (KickStarter.settingsManager && KickStarter.settingsManager.movementMethod == MovementMethod.PointAndClick) || 
				         (KickStarter.settingsManager && KickStarter.settingsManager.movementMethod == MovementMethod.StraightToCursor && KickStarter.settingsManager.singleTapStraight) || 
				         IsMovingToHotspot ())
				{
					charState = CharState.Move;
				}
				
				if (!lockedPath)
				{
					CheckIfStuck ();
				}
			}
			else if (activePath == null && KickStarter.stateHandler.gameState == GameState.Cutscene && charState == CharState.Move)
			{
				charState = CharState.Decelerate;
			}
			
			if (isJumping)
			{
				if (IsGrounded ())
				{
					isJumping = false;
				}
			}
			
			if (isTilting)
			{
				actualTilt = Mathf.Lerp (actualTilt, targetTilt, AdvGame.Interpolate (tiltStartTime, tiltSpeed, MoveMethod.Smooth, null));
				if (Mathf.Abs (targetTilt - actualTilt) < 2f)
				{
					isTilting = false;
				}
			}
			
			_FixedUpdate ();
			
			if (IsUFPSPlayer ())
			{
				if (isTilting)
				{
					UltimateFPSIntegration.SetRotation (new Vector2 (actualTilt, newRotation.eulerAngles.y));
				}
				else
				{
					UltimateFPSIntegration.SetPitch (newRotation.eulerAngles.y);
				}
			}
			else if (isTilting)
			{
				UpdateTilt ();
			}
			
			if (IsUFPSPlayer () && activePath != null && charState == CharState.Move)
			{
				UltimateFPSIntegration.Teleport (transform.position);
			}
		}
		
		
		private bool IsGrounded ()
		{
			if (_characterController != null)
			{
				return _characterController.isGrounded;
			}

			if (_rigidbody != null && Mathf.Abs (_rigidbody.velocity.y) > 0.1f)
			{
				return false;
			}
			
			if (_collider != null)
			{
				return Physics.CheckCapsule (transform.position + new Vector3 (0f, _collider.bounds.size.y, 0f), transform.position + new Vector3 (0f, _collider.bounds.size.x / 4f, 0f), _collider.bounds.size.x / 2f);
			}
			
			Debug.Log ("Player has no Collider component");
			return false;
		}
		
		
		public void Jump ()
		{
			if (isJumping)
			{
				return;
			}
			
			if (IsGrounded () && activePath == null)
			{
				if (_rigidbody != null)
				{
					_rigidbody.velocity = new Vector3 (0f, KickStarter.settingsManager.jumpSpeed, 0f);
					isJumping = true;
				}
				else
				{
					Debug.Log ("Player cannot jump without a Rigidbody component.");
				}
			}
		}
		
		
		private bool IsMovingToHotspot ()
		{
			if (KickStarter.playerInput != null && KickStarter.playerInput.hotspotMovingTo != null)
			{
				return true;
			}
			
			return false;
		}


		new public void EndPath ()
		{
			lockedPath = false;
			base.EndPath ();
		}
		
		
		public void SetLockedPath (Paths pathOb)
		{
			// Ignore if using "point and click" or first person methods
			if (KickStarter.settingsManager)
			{
				if (KickStarter.settingsManager.movementMethod == MovementMethod.Direct)
				{
					lockedPath = true;
					
					if (pathOb.pathSpeed == PathSpeed.Run)
					{
						isRunning = true;
					}
					else
					{
						isRunning = false;
					}
					
					if (pathOb.affectY)
					{
						transform.position = pathOb.transform.position;
					}
					else
					{
						transform.position = new Vector3 (pathOb.transform.position.x, transform.position.y, pathOb.transform.position.z);
					}
					
					activePath = pathOb;
					targetNode = 1;
					charState = CharState.Idle;
				}
				else
				{
					Debug.LogWarning ("Path-constrained player movement is only available with Direct control.");
				}
			}
		}
		
		
		protected override void Accelerate ()
		{
			if (KickStarter.settingsManager.movementMethod == MovementMethod.UltimateFPS && activePath == null)
			{
				// Fixes "stuttering" effect
				moveSpeed = 0f;
				return;
			}
			
			//base.Accelerate ();
			float targetSpeed;
			
			if (GetComponent <Animator>())
			{
				if (isRunning)
				{
					targetSpeed = runSpeedScale;
				}
				else
				{
					targetSpeed = walkSpeedScale;
				}
			}
			else
			{
				if (isRunning)
				{
					targetSpeed = moveDirection.magnitude * runSpeedScale / walkSpeedScale;
				}
				else
				{
					targetSpeed = moveDirection.magnitude;
				}
			}

			if (KickStarter.settingsManager.magnitudeAffectsDirect && KickStarter.settingsManager.movementMethod == MovementMethod.Direct && KickStarter.stateHandler.gameState == GameState.Normal && !IsMovingToHotspot ())
			{
				targetSpeed -= (1f - KickStarter.playerInput.moveKeys.magnitude) / 2f;
			}
			
			moveSpeed = Mathf.Lerp (moveSpeed, targetSpeed, Time.deltaTime * acceleration);
		}
		
		
		private void UpdateTilt ()
		{
			if (fpCam && fpCam.GetComponent <FirstPersonCamera>())
			{
				fpCam.GetComponent <FirstPersonCamera>().SetRotationY (actualTilt);
			}
			else if (KickStarter.settingsManager.movementMethod == MovementMethod.UltimateFPS)
			{
				UltimateFPSIntegration.SetTilt (actualTilt);
			}
		}
		
		
		public void SetTilt (Vector3 lookAtPosition, bool isInstant)
		{
			if (KickStarter.settingsManager.movementMethod == MovementMethod.UltimateFPS)
			{
				fpCam = UltimateFPSIntegration.GetFPCamTransform ();
			}

			if (fpCam == null)
			{
				return;
			}
			
			if (isInstant)
			{
				isTilting = false;
				
				transform.LookAt (lookAtPosition);
				float tilt = transform.localEulerAngles.x;
				if (targetTilt > 180)
				{
					targetTilt = targetTilt - 360;
				}
				
				if (fpCam && fpCam.GetComponent <FirstPersonCamera>())
				{
					fpCam.GetComponent <FirstPersonCamera>().SetRotationY (tilt);
				}
				else if (KickStarter.settingsManager.movementMethod == MovementMethod.UltimateFPS)
				{
					UltimateFPSIntegration.SetTilt (tilt);
				}
			}
			else
			{
				// Base the speed of tilt change on how much horizontal rotation is needed
				
				actualTilt = fpCam.eulerAngles.x;
				if (actualTilt > 180)
				{
					actualTilt = actualTilt - 360;
				}
				
				Quaternion oldRotation = fpCam.rotation;
				fpCam.transform.LookAt (lookAtPosition);
				targetTilt = fpCam.localEulerAngles.x;
				fpCam.rotation = oldRotation;
				if (targetTilt > 180)
				{
					targetTilt = targetTilt - 360;
				}
				
				Vector3 flatLookVector = lookAtPosition - transform.position;
				flatLookVector.y = 0f;
				
				tiltSpeed = Mathf.Abs (2f / Vector3.Dot (fpCam.forward.normalized, flatLookVector.normalized)) * turnSpeed / 100f;
				tiltSpeed = Mathf.Min (tiltSpeed, 2f);
				tiltStartTime = Time.time;
				isTilting = true;
			}
		}

		public void ResetCamera()
		{
			fpCam.transform.rotation = Quaternion.identity;
		}

		public override bool CanBeDirectControlled ()
		{
			if (KickStarter.stateHandler.gameState == GameState.Normal)
			{
				if (KickStarter.settingsManager.movementMethod == MovementMethod.Direct || KickStarter.settingsManager.movementMethod == MovementMethod.FirstPerson)
				{
					return !KickStarter.playerInput.isUpLocked;
				}
			}
			return false;
		}


		override public void SetHeadTurnTarget (Vector3 position, bool isInstant, HeadFacing _headFacing = HeadFacing.Manual)
		{
			if (_headFacing == HeadFacing.Hotspot && lockHotspotHeadTurning)
			{
				ClearHeadTurnTarget (HeadFacing.Hotspot, false);
			}
			else
			{
				base.SetHeadTurnTarget (position, isInstant, _headFacing);
			}
		}
		
	}
	
	
	[System.Serializable]
	public class PlayerPrefab
	{
		public Player playerOb;
		public int ID;
		public bool isDefault;
		
		public PlayerPrefab (int[] idArray)
		{
			ID = 0;
			playerOb = null;
			
			if (idArray.Length > 0)
			{
				isDefault = false;
				
				foreach (int _id in idArray)
				{
					if (ID == _id)
						ID ++;
				}
			}
			else
			{
				isDefault = true;
			}
		}
	}
	
}