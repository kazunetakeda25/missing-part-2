/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"Char.cs"
 * 
 *	This is the base class for both NPCs and the Player.
 *	It contains the functions needed for animation and movement.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if SalsaIsPresent
using CrazyMinnow.SALSA;
#endif

namespace AC
{
	
	[RequireComponent (typeof (Paths))]
	public class Char : MonoBehaviour
	{
		
		public AnimationEngine animationEngine = AnimationEngine.SpritesUnity;	// Enum
		public AnimEngine animEngine;											// ScriptableObject
		public string customAnimationClass = "";
		public MotionControl motionControl = MotionControl.Automatic;
		public TalkingAnimation talkingAnimation = TalkingAnimation.Standard;
		
		public string speechLabel = "";
		public CursorIconBase portraitIcon = new CursorIconBase ();
		public Color speechColor = Color.white;
		protected Quaternion newRotation;
		private float prevHeight;
		private float prevHeight2;
		public float heightChange;
		
		// Lip sync variables
		
		private Shapeable shapeable = null;
		private LipSyncTexture lipSyncTexture = null;
		private List<LipSyncShape> lipSyncShapes = new List<LipSyncShape>();
		public bool isLipSyncing = false;
		public string phonemeParameter = "";
		public int lipSyncGroupID;
		
		#if SalsaIsPresent
		private Salsa2D salsa2D;
		#endif
		
		// 3D variables
		
		public Transform leftHandBone;
		public Transform rightHandBone;
		
		// Legacy variables
		
		public AnimationClip idleAnim;
		public AnimationClip walkAnim;
		public AnimationClip runAnim;
		public AnimationClip talkAnim;
		public AnimationClip turnLeftAnim;
		public AnimationClip turnRightAnim;
		public AnimationClip headLookLeftAnim;
		public AnimationClip headLookRightAnim;
		public AnimationClip headLookUpAnim;
		public AnimationClip headLookDownAnim;
		
		public Animation _animation;
		
		public Transform upperBodyBone;
		public Transform leftArmBone;
		public Transform rightArmBone;
		public Transform neckBone;
		
		public float animCrossfadeSpeed = 0.2f;
		
		// Mecanim variables
		
		public string moveSpeedParameter = "Speed";
		public string verticalMovementParameter = "";
		public string turnParameter = "";
		public string talkParameter = "IsTalking";
		public string directionParameter = "Direction";
		public string angleParameter = "Angle";
		public string headYawParameter = "";
		public string headPitchParameter = "";
		public bool relyOnRootMotion = false;
		public float rootTurningFactor = 0f;
		public string lastPlayedAnim = "";
		public int headLayer = 1;
		public int mouthLayer = 2;
		
		// 2D variables
		
		public Animator animator;
		public Transform spriteChild;
		
		public string idleAnimSprite = "idle";
		public string walkAnimSprite = "walk";
		public string runAnimSprite = "run";
		public string talkAnimSprite = "talk";
		
		public bool lockScale = false;
		public float spriteScale = 1f;
		public bool lockDirection = false;
		public string spriteDirection = "D";
		private float spriteAngle = 0f;
		
		public bool doDirections = true;
		public bool crossfadeAnims = false;
		public bool doDiagonals = false;
		public bool isTalking = false;
		public AC_2DFrameFlipping frameFlipping = AC_2DFrameFlipping.None;
		public bool flipCustomAnims = false;
		
		private Vector3 originalScale;
		private bool flipFrames = false;
		
		// Movement variables
		
		public float walkSpeedScale = 2f;
		public float runSpeedScale = 6f;
		public float turnSpeed = 7f;
		public float acceleration = 6f;
		public float deceleration = 0f;
		public float sortingMapScale = 1f;
		public bool isReversing = false;
		public bool turnBeforeWalking = false;
		public bool isJumping = false;
		public float turnFloat = 0f;
		
		// Rigidbody variables
		
		public bool ignoreGravity = false;
		public bool freezeRigidbodyWhenIdle = false;
		protected Rigidbody _rigidbody = null;
		private Rigidbody2D _rigidbody2D = null;
		public Collider _collider = null;
		protected CharacterController _characterController;
		
		// Wall detection variables
		
		public bool doWallReduction = false;
		public string wallLayer = "Default";
		public float wallDistance = 0.5f;
		private float wallReduction = 1f;
		private Vector3 wallRayOrigin = Vector3.zero;
		private float wallRayForward = 0f;
		
		// Sound variables
		
		public AudioClip walkSound;
		public AudioClip runSound;
		public Sound soundChild;
		protected AudioSource audioSource;
		
		public Paths activePath = null;
		public bool isRunning { get; set; }
		
		public CharState charState;
		
		protected float moveSpeed;
		protected Vector3 moveDirection; 
		
		protected int targetNode = 0;
		protected bool pausePath = false;
		
		private Vector3 lookDirection;
		private float pausePathTime;
		private ActionList nodeActionList;
		private int prevNode = 0;
		
		// Resume path
		private int lastPathPrevNode = 0;
		private int lastPathTargetNode = 0;
		private Paths lastPathActivePath = null;
		
		private bool tankTurning = false;
		
		private Vector2 targetHeadAngles;
		private Vector2 actualHeadAngles;
		private float headTurnWeight = 0f;
		public Vector3 headTurnTarget;
		public HeadFacing headFacing = HeadFacing.None;
		public bool ikHeadTurning = false;
		
		
		protected void _Awake ()
		{
			if (GetComponent <CharacterController>())
			{
				_characterController = GetComponent <CharacterController>();
				wallRayOrigin = _characterController.center;
				wallRayForward = _characterController.radius;
			}
			else if (GetComponent <CapsuleCollider>())
			{
				CapsuleCollider capsuleCollider = GetComponent <CapsuleCollider>();
				wallRayOrigin = capsuleCollider.center;
				wallRayForward = capsuleCollider.radius;
			}
			
			if (GetComponentInChildren <FollowSortingMap>())
			{
				transform.localScale = Vector3.one;
			}
			originalScale = transform.localScale;
			charState = CharState.Idle;
			shapeable = GetShapeable ();
			if (GetComponent <LipSyncTexture>())
			{
				lipSyncTexture = GetComponent <LipSyncTexture>();
			}
			
			ResetAnimationEngine ();
			ResetBaseClips ();
			
			if (spriteChild && spriteChild.GetComponent <Animator>())
			{
				animator = spriteChild.GetComponent <Animator>();
			}
			
			if (soundChild && soundChild.gameObject.GetComponent <AudioSource>())
			{
				audioSource = soundChild.gameObject.GetComponent <AudioSource>();
			}
			
			if (GetComponent <Animator>())
			{
				animator = GetComponent <Animator>();
			}
			
			if (GetComponent <Animation>())
			{
				_animation = GetComponent <Animation>();
			}
			
			if (GetComponent <Rigidbody>())
			{
				_rigidbody = GetComponent <Rigidbody>();
			}
			else if (GetComponent <Rigidbody2D>())
			{
				_rigidbody2D = GetComponent <Rigidbody2D>();
			}
			
			if (GetComponent <Collider>())
			{
				_collider = GetComponent <Collider>();
			}
			
			AdvGame.AssignMixerGroup (GetComponent <AudioSource>(), SoundType.Other, true);
			AdvGame.AssignMixerGroup (audioSource, SoundType.SFX);
		}
		
		
		protected void _Update ()
		{
			UpdateHeadTurnAngle ();
			CalcHeightChange ();
			
			if (KickStarter.settingsManager != null && spriteChild)
			{
				UpdateSpriteChild (KickStarter.settingsManager.IsTopDown (), KickStarter.settingsManager.IsUnity2D ());
			}
			
			AnimUpdate ();
			SpeedUpdate ();
		}
		
		
		private void OnAnimatorIK (int layerIndex)
		{
			if (ikHeadTurning)
			{
				if (headTurnWeight > 0f)
				{
					Quaternion rot = Quaternion.Euler (0f, actualHeadAngles.x, 0f);
					Vector3 position = rot * transform.forward;
					
					position.y += actualHeadAngles.y;
					
					if (neckBone != null)
					{
						position += neckBone.position;
					}
					else
					{
						position += transform.position;
						
						if (_collider is CapsuleCollider)
						{
							CapsuleCollider capsuleCollder = (CapsuleCollider) _collider;
							position += new Vector3 (0f, capsuleCollder.height * transform.localScale.y * 0.8f, 0f);
						}
					}

					if (headFacing != AC.HeadFacing.None)
					{
						animator.SetLookAtPosition (position);
					}
				}

				animator.SetLookAtWeight (headTurnWeight);
			}
		}
		
		
		protected void _FixedUpdate ()
		{
			PathUpdate ();
			PhysicsUpdate ();
			MoveUpdate ();
		}
		
		
		protected void PathUpdate ()
		{
			if (activePath && activePath.nodes.Count > 0)
			{
				if (pausePath)
				{
					if (nodeActionList != null)
					{
						if (!KickStarter.actionListManager.IsListRunning (nodeActionList))
						{
							SetNextNodes ();
						}
					}
					else if (Time.time > pausePathTime)
					{
						SetNextNodes ();
					}
					return;
				}
				else
				{
					Vector3 direction = activePath.nodes[targetNode] - transform.position;
					Vector3 lookDir = new Vector3 (direction.x, 0f, direction.z);
					
					if (KickStarter.settingsManager && KickStarter.settingsManager.IsUnity2D ())
					{
						direction.z = 0f;
						SetMoveDirection (direction);
						lookDir = new Vector3 (direction.x, 0f, direction.y);
						SetLookDirection (lookDir, false);
					}
					else if (activePath.affectY)
					{
						SetMoveDirection (direction);
						SetLookDirection (lookDir, false);
					}
					else
					{
						SetLookDirection (lookDir, false);
						SetMoveDirectionAsForward ();
					}
					
					float nodeThreshold = 0.1f;
					if (KickStarter.settingsManager)
					{
						nodeThreshold = 1.05f - KickStarter.settingsManager.destinationAccuracy;
					}
					if (isRunning && GetMotionControl () == MotionControl.Automatic)
					{
						nodeThreshold *= runSpeedScale / walkSpeedScale;
					}
					
					if ((KickStarter.settingsManager.IsUnity2D () && direction.magnitude < nodeThreshold) ||
					    (activePath.affectY && direction.magnitude < nodeThreshold) ||
					    (!activePath.affectY && lookDir.magnitude < nodeThreshold))
					{
						if (targetNode == 0 && prevNode == 0)
						{
							SetNextNodes ();
						}
						else if (activePath.nodePause > 0f)
						{
							PausePath (activePath.nodePause);
						}
						else if (activePath.nodeCommands.Count > targetNode)
						{
							if (activePath.commandSource == ActionListSource.InScene && activePath.nodeCommands [targetNode].cutscene != null)
							{
								PausePath (activePath.nodeCommands [targetNode].cutscene, activePath.nodeCommands [targetNode].parameterID);
							}
							else if (activePath.commandSource == ActionListSource.AssetFile && activePath.nodeCommands [targetNode].actionListAsset != null)
							{
								PausePath (activePath.nodeCommands [targetNode].actionListAsset, activePath.nodeCommands [targetNode].parameterID);
							}
							else
							{
								SetNextNodes ();
							}
						}
						else
						{
							SetNextNodes ();
						}
					}
				}
			}
		}
		
		
		private void SpeedUpdate ()
		{
			if (charState == CharState.Move)
			{
				Accelerate ();
			}
			else if (charState == CharState.Decelerate || charState == CharState.Custom)
			{
				Decelerate ();
			}
			else if (charState == CharState.Idle && moveSpeed > 0f)
			{
				moveSpeed = 0f;
			}
		}
		
		
		private void PhysicsUpdate ()
		{
			if (_rigidbody)
			{
				if (ignoreGravity)
				{
					_rigidbody.useGravity = false;
				}
				
				else if (charState == CharState.Custom && moveSpeed < 0.01f)
				{
					_rigidbody.useGravity = false;
				}
				else
				{
					if (activePath && activePath.affectY)
					{
						_rigidbody.useGravity = false;
					}
					else
					{
						_rigidbody.useGravity = true;
					}
				}
			}
			else if (_rigidbody2D)
			{
				if (ignoreGravity)
				{
					_rigidbody2D.gravityScale = 0f;
				}
				
				else if (charState == CharState.Custom && moveSpeed < 0.01f)
				{
					_rigidbody2D.gravityScale = 0f;
				}
				else
				{
					if (activePath && activePath.affectY)
					{
						_rigidbody2D.gravityScale = 0f;
					}
					else
					{
						_rigidbody2D.gravityScale = 1f;
					}
				}
			}
		}
		
		
		private void AnimUpdate ()
		{
			if (isTalking)
			{
				ProcessLipSync ();
			}
			
			if (isJumping)
			{
				animEngine.PlayJump ();
				StopStandardAudio ();
			}
			else
			{
				if (charState == CharState.Idle || charState == CharState.Decelerate)
				{
					if (IsTurning ())
					{
						if (turnFloat < 0f)
						{
							animEngine.PlayTurnLeft ();
						}
						else
						{
							animEngine.PlayTurnRight ();
						}
					}
					else
					{
						if (isTalking && (talkingAnimation == TalkingAnimation.Standard || animationEngine == AnimationEngine.Custom))
						{
							animEngine.PlayTalk ();
						}
						else
						{
							animEngine.PlayIdle ();
						}
					}
					
					StopStandardAudio ();
				}
				else if (charState == CharState.Move)
				{
					if (isRunning)
					{
						animEngine.PlayRun ();
					}
					else
					{
						animEngine.PlayWalk ();
					}
					
					PlayStandardAudio ();
				}
				else
				{
					StopStandardAudio ();
				}
				
				animEngine.PlayVertical ();
			}
		}
		
		
		private void MoveUpdate ()
		{
			if (animEngine)
			{
				if (GetMotionControl () == MotionControl.Automatic)
				{
					if (moveSpeed > 0.01f && (!animEngine.rootMotion || !relyOnRootMotion))
					{
						Vector3 newVel;
						newVel = moveDirection * moveSpeed * walkSpeedScale * sortingMapScale;
						
						if (KickStarter.settingsManager)
						{
							if (KickStarter.settingsManager.IsTopDown ())
							{
								float upAmount = Mathf.Abs (Vector3.Dot (newVel.normalized, Vector3.forward));
								float mag = (newVel.magnitude * (1f - upAmount)) + (newVel.magnitude * KickStarter.settingsManager.verticalReductionFactor * upAmount);
								newVel *= mag / newVel.magnitude;
							}
							else if (KickStarter.settingsManager.IsUnity2D ())
							{
								newVel.z = 0f;
								float upAmount = Mathf.Abs (Vector3.Dot (newVel.normalized, Vector3.up));
								float mag = (newVel.magnitude * (1f - upAmount)) + (newVel.magnitude * KickStarter.settingsManager.verticalReductionFactor * upAmount);
								newVel *= mag / newVel.magnitude;
							}
						}
						
						if (DoRigidbodyMovement ())
						{
							_rigidbody.MovePosition (_rigidbody.position + newVel * Time.deltaTime);
						}
						else if (_characterController)
						{
							if (!_characterController.isGrounded && !ignoreGravity)
							{
								newVel += Physics.gravity;
							}
							_characterController.Move (newVel * Time.deltaTime);
						}
						else
						{
							transform.position += (newVel * Time.deltaTime);
						}
					}
					else
					{
						if (_characterController)
						{
							if (this is Player && KickStarter.settingsManager.movementMethod == MovementMethod.UltimateFPS)
							{}
							else
							{
								if (!_characterController.isGrounded && !ignoreGravity)
								{
									_characterController.Move (Physics.gravity * Time.deltaTime);
								}
							}
						}
					}
				}
			}
			
			if (animEngine && animEngine.turningStyle == TurningStyle.Linear && moveSpeed > 0.01f && !IsUFPSPlayer ())
			{
				Turn (true);
			}
			else
			{
				Turn (false);
			}
			
			DoTurn ();
			
			if (_rigidbody)
			{
				if (freezeRigidbodyWhenIdle && !isJumping && (charState == CharState.Custom || charState == CharState.Idle))
				{
					_rigidbody.constraints = RigidbodyConstraints.FreezeAll;
				}
				else
				{
					_rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
				}
			}
		}
		
		
		protected virtual void Accelerate ()
		{
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
			
			moveSpeed = Mathf.Lerp (moveSpeed, targetSpeed, Time.deltaTime * acceleration);
		}
		
		
		public void SetMoveSpeed (float newSpeed)
		{
			moveSpeed = newSpeed;
		}
		
		
		private void Decelerate ()
		{
			if (deceleration <= 0f)
			{
				moveSpeed = Mathf.Lerp (moveSpeed, 0f, Time.deltaTime * acceleration);
			}
			else
			{
				moveSpeed = Mathf.Lerp (moveSpeed, 0f, Time.deltaTime * deceleration);
			}
			
			if (moveSpeed < 0.01f)
			{
				moveSpeed = 0f;
				
				if (charState != CharState.Custom)
				{
					charState = CharState.Idle;
				}
			}
		}
		
		
		public bool IsTurning ()
		{
			if (lookDirection == Vector3.zero || Quaternion.Angle (Quaternion.LookRotation (lookDirection), transform.rotation) < 4f)
			{
				return false;
			}
			return true;
		}
		
		
		public void TankTurnLeft ()
		{
			lookDirection = -transform.right;
			tankTurning = true;
		}
		
		
		public void TankTurnRight ()
		{
			lookDirection = transform.right;
			tankTurning = true;
		}
		
		
		public void StopTurning ()
		{
			lookDirection = transform.forward;
			tankTurning = false;
		}
		
		
		protected bool IsUFPSPlayer ()
		{
			if (this is Player && KickStarter.settingsManager.movementMethod == MovementMethod.UltimateFPS)
			{
				return true;
			}
			return false;
		}
		
		
		public void Teleport (Vector3 _position)
		{
			if (IsUFPSPlayer ())
			{
				UltimateFPSIntegration.Teleport (_position);
			}
			else
			{
				transform.position = _position;
			}
		}
		
		
		public void SetRotation (Quaternion _rotation)
		{
			transform.rotation = _rotation;
			SetLookDirection (transform.forward, true);
		}
		
		
		public void SetRotation (float angle)
		{
			transform.rotation = Quaternion.AngleAxis (angle, Vector3.up);
			SetLookDirection (transform.forward, true);
		}
		
		
		public void Turn (bool isInstant)
		{
			if (lookDirection == Vector3.zero)
			{
				return;
			}
			
			if (isInstant)
			{
				turnFloat = 0f;
				
				if (IsUFPSPlayer ())
				{
					UltimateFPSIntegration.SetRotation (lookDirection); // GetTargetRotation ().eulerAngles??
				}
				/*else if (DoRigidbodyMovement ())
				{
					_rigidbody.rotation = GetTargetRotation ();
				}*/
				else
				{
					transform.rotation = GetTargetRotation ();
				}
				
				if (KickStarter.settingsManager != null && spriteChild)
				{
					UpdateSpriteChild (KickStarter.settingsManager.IsTopDown (), KickStarter.settingsManager.IsUnity2D ());
				}
				return;
			}
			
			float targetAngle = Mathf.Atan2 (lookDirection.x, lookDirection.z);
			float currentAngle = Mathf.Atan2 (transform.forward.x, transform.forward.z);

			float angleDiff = targetAngle - currentAngle;
			if (angleDiff < -Mathf.PI)
			{
				targetAngle += Mathf.PI * 2f;
				angleDiff += Mathf.PI * 2f;
			}
			else if (angleDiff > Mathf.PI)
			{
				targetAngle -= Mathf.PI * 2f;
				angleDiff -= Mathf.PI * 2f;
			}
			
			turnFloat = Mathf.Lerp (turnFloat, turnSpeed * Mathf.Min (angleDiff/2f, 1f), turnSpeed * Time.deltaTime); // Min prevents turn direction flipping when slowing down
			float newAngle = currentAngle;

			if (IsUFPSPlayer () && KickStarter.stateHandler.gameState == GameState.Normal)
			{
				lookDirection = transform.forward;
				newRotation = transform.rotation;
				return;
			}

			if (animEngine && animEngine.turningStyle == TurningStyle.Linear && !IsUFPSPlayer ())
			{
				if (DoRigidbodyMovement ())
				{
					newRotation = Quaternion.RotateTowards (_rigidbody.rotation, GetTargetRotation (), turnSpeed);
				}
				else
				{
					newRotation = Quaternion.RotateTowards (transform.rotation, GetTargetRotation (), turnSpeed);
				}
			}
			else
			{
				if ((tankTurning || moveSpeed == 0f) && animEngine && animEngine.turningStyle == AC.TurningStyle.Script)
				{
					newAngle = Mathf.Lerp (currentAngle, targetAngle, turnSpeed * Time.deltaTime * GetScriptTurningFactor () / 2f);
				}
				else
				{
					newAngle = Mathf.Lerp (currentAngle, targetAngle, turnSpeed * Time.deltaTime * GetScriptTurningFactor ());
				}
				newRotation = Quaternion.AngleAxis (newAngle * Mathf.Rad2Deg, Vector3.up);
			}
		}
		
		
		private float GetScriptTurningFactor ()
		{
			if (relyOnRootMotion)
			{
				return (1f - rootTurningFactor);
			}
			return 1f;
		}
		
		
		private void DoTurn ()
		{
			if (GetMotionControl () == MotionControl.Manual || IsUFPSPlayer () || lookDirection == Vector3.zero)
			{
				return;
			}
			
			if (DoRigidbodyMovement ())
			{
				_rigidbody.MoveRotation (newRotation);
			}
			else
			{
				transform.rotation = newRotation;
			}
		}
		
		
		public void SetLookDirection (Vector3 _direction, bool isInstant)
		{
			lookDirection = new Vector3 (_direction.x, 0f, _direction.z);
			Turn (isInstant);

			if (IsUFPSPlayer ())
			{
				transform.forward = lookDirection.normalized;
			}
		}
		
		
		public void SetMoveDirection (Vector3 _direction)
		{
			if (_direction != Vector3.zero)
			{
				Quaternion targetRotation = Quaternion.LookRotation (_direction, Vector3.up);
				moveDirection = targetRotation * Vector3.forward;
				moveDirection.Normalize ();
			}
		}
		
		
		public void SetMoveDirectionAsForward ()
		{
			isReversing = false;
			moveDirection = transform.forward;
			if (KickStarter.settingsManager && KickStarter.settingsManager.IsUnity2D ())
			{
				moveDirection = new Vector3 (moveDirection.x, moveDirection.z, 0f);
			}
			moveDirection.Normalize ();
		}
		
		
		public void SetMoveDirectionAsBackward ()
		{
			isReversing = true;
			moveDirection = -transform.forward;
			if (KickStarter.settingsManager && KickStarter.settingsManager.IsUnity2D ())
			{
				moveDirection = new Vector3 (moveDirection.x, moveDirection.z, 0f);
			}
			moveDirection.Normalize ();
		}
		
		
		public Vector3 GetMoveDirection ()
		{
			return moveDirection;	
		}
		
		
		private void SetNextNodes ()
		{
			pausePath = false;
			nodeActionList = null;
			
			int tempPrev = targetNode;
			
			if (this is Player && KickStarter.stateHandler.gameState == GameState.Normal)
			{
				targetNode = activePath.GetNextNode (targetNode, prevNode, true);
			}
			else
			{
				targetNode = activePath.GetNextNode (targetNode, prevNode, false);
			}
			
			prevNode = tempPrev;
			
			if (targetNode == 0 && activePath.pathType == AC_PathType.Loop && activePath.teleportToStart)
			{
				Teleport (activePath.transform.position);
				
				// Set rotation if there is more than one node
				if (activePath.nodes.Count > 1)
				{
					SetLookDirection (activePath.nodes[1] - activePath.nodes[0], true);
				}
				SetNextNodes ();
				return;
			}
			
			if (targetNode == -1)
			{
				EndPath ();
			}
		}
		
		
		public void EndPath ()
		{
			if (GetComponent <Paths>() && activePath == GetComponent <Paths>())
			{
				activePath.nodes.Clear ();
			}
			else
			{
				lastPathPrevNode = prevNode;
				lastPathTargetNode = targetNode;
				lastPathActivePath = activePath;
			}
			
			activePath = null;
			targetNode = 0;
			
			if (charState == CharState.Move)
			{
				charState = CharState.Decelerate;
			}
		}
		
		
		public void ResumeLastPath ()
		{
			if (lastPathActivePath != null)
			{
				SetPath (lastPathActivePath, lastPathTargetNode, lastPathPrevNode);
			}
		}
		
		
		public void SetLastPath (Paths _lastPathActivePath, int _lastPathTargetNode, int _lastPathPrevNode)
		{
			lastPathActivePath = _lastPathActivePath;
			lastPathTargetNode = _lastPathTargetNode;
			lastPathPrevNode = _lastPathPrevNode;
		}
		
		
		public void Halt ()
		{
			if (GetComponent <Paths>() && activePath == GetComponent <Paths>()) {}
			else
			{
				lastPathPrevNode = prevNode;
				lastPathTargetNode = targetNode;
				lastPathActivePath = activePath;
			}
			
			activePath = null;
			targetNode = 0;
			moveSpeed = 0f;
			
			if (charState == CharState.Move || charState == CharState.Decelerate)
			{
				charState = CharState.Idle;
			}
		}
		
		
		public void ForceIdle ()
		{
			charState = CharState.Idle;
		}
		
		
		protected void ReverseDirection ()
		{
			int tempPrev = targetNode;
			targetNode = prevNode;
			prevNode = tempPrev;
		}
		
		
		private void PausePath (float pauseTime)
		{
			charState = CharState.Decelerate;
			pausePath = true;
			pausePathTime = Time.time + pauseTime;
			nodeActionList = null;
		}
		
		
		private void PausePath (Cutscene pauseCutscene, int parameterID)
		{
			charState = CharState.Decelerate;
			pausePath = true;
			pausePathTime = 0f;
			
			if (pauseCutscene.useParameters && parameterID >= 0 && pauseCutscene.parameters.Count > parameterID)
			{
				pauseCutscene.parameters [parameterID].SetValue (this.gameObject);
			}
			
			pauseCutscene.Interact ();
			nodeActionList = pauseCutscene;
		}
		
		
		private void PausePath (ActionListAsset pauseAsset, int parameterID)
		{
			charState = CharState.Decelerate;
			pausePath = true;
			pausePathTime = 0f;
			
			if (pauseAsset.useParameters && parameterID >= 0 && pauseAsset.parameters.Count > parameterID)
			{
				int idToSend = 0;
				if (this.gameObject.GetComponent <ConstantID>())
				{
					idToSend = this.gameObject.GetComponent <ConstantID>().constantID;
				}
				else
				{
					Debug.LogWarning (this.gameObject.name + " requires a ConstantID script component!");
				}
				pauseAsset.parameters [parameterID].SetValue (idToSend);
			}
			
			nodeActionList = AdvGame.RunActionListAsset (pauseAsset);
		}
		
		
		private void TurnBeforeWalking ()
		{
			Vector3 direction = activePath.nodes[1] - transform.position;
			if (KickStarter.settingsManager && KickStarter.settingsManager.IsUnity2D ())
			{
				SetLookDirection (new Vector3 (direction.x, 0f, direction.y), false);
			}
			else
			{
				SetLookDirection (new Vector3 (direction.x, 0f, direction.z), false);
			}
			Turn (false);
		}
		
		
		protected bool IsTurningBeforeWalking ()
		{
			if (Mathf.Abs (turnFloat) > 0.3f && CanTurnBeforeMoving ())
			{
				return true;
			}
			return false;
		}
		
		
		private bool CanTurnBeforeMoving ()
		{
			if (turnBeforeWalking && activePath == GetComponent <Paths>() && targetNode <= 1 && activePath.nodes.Count > 1)
			{
				return true;
			}
			return false;
		}
		
		
		public void SetPath (Paths pathOb, int _targetNode, int _prevNode, bool affectY)
		{
			if (pathOb)
			{
				SetPath (pathOb, pathOb.pathSpeed, _targetNode, _prevNode);
				activePath.affectY = affectY;
			}
		}
		
		
		private void SetPath (Paths pathOb, PathSpeed _speed, int _targetNode, int _prevNode)
		{
			activePath = pathOb;
			targetNode = _targetNode;
			prevNode = _prevNode;
			
			if (CanTurnBeforeMoving ())
			{
				TurnBeforeWalking ();
			}
			
			if (pathOb)
			{
				if (_speed == PathSpeed.Run)
				{
					isRunning = true;
				}
				else
				{
					isRunning = false;
				}
			}
			
			charState = CharState.Idle;
		}
		
		
		public void SetPath (Paths pathOb, PathSpeed _speed)
		{
			SetPath (pathOb, _speed, 0, 0);
		}
		
		
		public void SetPath (Paths pathOb)
		{
			SetPath (pathOb, pathOb.pathSpeed, 0, 0);
		}
		
		
		public void SetPath (Paths pathOb, int _targetNode, int _prevNode)
		{
			SetPath (pathOb, pathOb.pathSpeed, _targetNode, _prevNode);
		}
		
		
		protected void CheckIfStuck ()
		{
			// Check for null movement error: if not moving on a path, end the path
			
			/*if (_rigidbody)
			{
				Vector3 newPosition = _rigidbody.position;
				if (oldPosition == newPosition)
				{
					Debug.Log ("Stuck in active path - removing");
					EndPath ();
				}
				
				oldPosition = newPosition;
			}  */
		}
		
		
		public bool IsPathfinding ()
		{
			if (GetPath () != null)
			{
				return true;
			}
			return false;
		}
		
		
		public Paths GetPath ()
		{
			return activePath;
		}
		
		
		public Paths GetPath (bool isCurrent)
		{
			if (isCurrent)
			{
				return activePath;
			}
			return lastPathActivePath;
		}
		
		
		public int GetTargetNode (bool isCurrent)
		{
			if (isCurrent)
			{
				return targetNode;
			}
			return lastPathTargetNode;
		}
		
		
		public int GetPrevNode (bool isCurrent)
		{
			if (isCurrent)
			{
				return prevNode;
			}
			return lastPathPrevNode;
		}
		
		
		public void MoveToPoint (Vector3 point, bool run)
		{
			List<Vector3> pointData = new List<Vector3>();
			pointData.Add (point);
			MoveAlongPoints (pointData.ToArray (), run);
		}
		
		
		protected void ChangePathfindSpeed (bool run)
		{
			Paths path = GetComponent <Paths>();
			if (path)
			{
				if (run && !isRunning)
				{
					SetPath (path, PathSpeed.Run);
				}
				else if (!run && isRunning)
				{
					SetPath (path, PathSpeed.Walk);
				}
			}
		}
		
		
		public void MoveAlongPoints (Vector3[] pointData, bool run)
		{
			Paths path = GetComponent <Paths>();
			if (path)
			{
				path.BuildNavPath (pointData);
				
				if (run)
				{
					SetPath (path, PathSpeed.Run);
				}
				else
				{
					SetPath (path, PathSpeed.Walk);
				}
			}
			else
			{
				Debug.LogWarning (this.name + " cannot pathfind without a Paths component");
			}
		}
		
		
		public void ResetBaseClips ()
		{
			// Remove all animations except Idle, Walk, Run and Talk
			
			if (spriteChild && spriteChild.GetComponent <Animation>())
			{
				List <string> clipsToRemove = new List <string>();
				
				foreach (AnimationState state in spriteChild.GetComponent <Animation>())
				{
					if ((idleAnim == null || state.name != idleAnim.name) && (walkAnim == null || state.name != walkAnim.name) && (runAnim == null || state.name != runAnim.name))
					{
						clipsToRemove.Add (state.name);
					}
				}
				
				foreach (string _clip in clipsToRemove)
				{
					spriteChild.GetComponent <Animation>().RemoveClip (_clip);
				}
			}
			
			if (_animation)
			{
				List <string> clipsToRemove = new List <string>();
				
				foreach (AnimationState state in _animation)
				{
					if ((idleAnim == null || state.name != idleAnim.name) && (walkAnim == null || state.name != walkAnim.name) && (runAnim == null || state.name != runAnim.name))
					{
						clipsToRemove.Add (state.name);
					}
				}
				
				foreach (string _clip in clipsToRemove)
				{
					_animation.RemoveClip (_clip);
				}
			}
			
		}
		
		
		public float GetSpriteAngle ()
		{
			return spriteAngle;
		}
		
		
		public string GetSpriteDirection ()
		{
			return ("_" + spriteDirection);
		}
		
		
		public int GetSpriteDirectionInt ()
		{
			if (spriteDirection == "D")
			{
				return 0;
			}
			if (spriteDirection == "L")
			{
				return 1;
			}
			if (spriteDirection == "R")
			{
				return 2;
			}
			if (spriteDirection == "U")
			{
				return 3;
			}
			if (spriteDirection == "DL")
			{
				return 4;
			}
			if (spriteDirection == "DR")
			{
				return 5;
			}
			if (spriteDirection == "UL")
			{
				return 6;
			}
			if (spriteDirection == "UR")
			{
				return 7;
			}
			
			return 0;
		}
		
		
		public void SetSpriteDirection (CharDirection direction)
		{
			if (direction == CharDirection.Down)
			{
				spriteDirection = "D";
			}
			else if (direction == CharDirection.Left)
			{
				spriteDirection = "L";
			}
			else if (direction == CharDirection.Right)
			{
				spriteDirection = "R";
			}
			else if (direction == CharDirection.Up)
			{
				spriteDirection = "U";
			}
			else if (direction == CharDirection.DownLeft)
			{
				spriteDirection = "DL";
			}
			else if (direction == CharDirection.DownRight)
			{
				spriteDirection = "DR";
			}
			else if (direction == CharDirection.UpLeft)
			{
				spriteDirection = "UL";
			}
			else if (direction == CharDirection.UpRight)
			{
				spriteDirection = "UR";
			}
		}
		
		
		private string SetSpriteDirection (float rightAmount, float forwardAmount)
		{
			float angle = Vector2.Angle (new Vector2 (1f, 0f), new Vector2 (rightAmount, forwardAmount));
			if (doDiagonals)
			{
				if (forwardAmount > 0f)
				{
					if (angle > 22.5f && angle < 67.5f)
					{
						return "UR";
					}
					else if (angle > 112.5f && angle < 157.5f)
					{
						return "UL";
					}
				}
				else
				{
					if (angle > 22.5f && angle < 67.55f)
					{
						return "DR";
					}
					else if (angle > 112.5f && angle < 157.5f)
					{
						return "DL";
					}
				}
			}
			
			if (forwardAmount > 0f)
			{
				if (angle > 45f && angle < 135f)
				{
					return "U";
				}
			}
			else
			{
				if (angle > 45f && angle < 135f)
				{
					return "D";
				}
			}
			
			if (rightAmount > 0f)
			{
				return "R";
			}
			
			return "L";
		}
		
		
		private void CalcHeightChange ()
		{
			float currentHeight = transform.position.y;
			
			if (currentHeight != prevHeight && currentHeight != prevHeight2 && prevHeight != prevHeight2)
			{
				// Is changing height, but not teleporting
				heightChange = currentHeight - prevHeight;
			}
			else
			{
				heightChange = 0f;
			}
			
			prevHeight2 = prevHeight;
			prevHeight = currentHeight;
		}
		
		
		protected void StopStandardAudio ()
		{
			if (audioSource && audioSource.isPlaying)
			{
				if ((runSound && audioSource.clip == runSound) || (walkSound && audioSource.clip == walkSound))
				{
					audioSource.Stop ();
				}
			}
		}
		
		
		protected void PlayStandardAudio ()
		{
			if (audioSource)
			{
				if (isRunning && runSound)
				{
					if (audioSource.isPlaying && audioSource.clip == runSound)
					{
						return;
					}
					
					audioSource.loop = false;
					audioSource.clip = runSound;
					audioSource.Play ();
				}
				
				else if (walkSound)
				{
					if (audioSource.isPlaying && audioSource.clip == walkSound)
					{
						return;
					}
					
					audioSource.loop = false;
					audioSource.clip = walkSound;
					audioSource.Play ();
				}
			}
		}
		
		
		public void ResetAnimationEngine ()
		{
			string className = "AnimEngine";
			
			if (animationEngine == AnimationEngine.Custom)
			{
				if (customAnimationClass.Length > 0)
				{
					className = customAnimationClass;
				}
			}
			else
			{
				className += "_" + animationEngine.ToString ();
			}
			
			if (animEngine == null || animEngine.ToString () != className)
			{
				try
				{
					animEngine = (AnimEngine) ScriptableObject.CreateInstance (className);
					if (animEngine != null)
					{
						animEngine.Declare (this);
					}
				} catch {}
			}
		}
		
		
		public void UpdateSpriteChild (bool isTopDown, bool isUnity2D)
		{
			float forwardAmount = 0f;
			float rightAmount = 0f;
			
			if (isTopDown || isUnity2D)
			{
				forwardAmount = Vector3.Dot (Vector3.forward, transform.forward.normalized);
				rightAmount = Vector3.Dot (Vector3.right, transform.forward.normalized);
			}
			else
			{
				forwardAmount = Vector3.Dot (KickStarter.mainCamera.ForwardVector ().normalized, transform.forward.normalized);
				rightAmount = Vector3.Dot (KickStarter.mainCamera.RightVector ().normalized, transform.forward.normalized);
			}
			
			spriteAngle = Mathf.Atan (rightAmount / forwardAmount) * Mathf.Rad2Deg;
			if (forwardAmount > 0f) spriteAngle += 180f;
			else if (rightAmount > 0f) spriteAngle += 360f;
			
			if (charState == CharState.Custom && !flipCustomAnims)
			{
				flipFrames = false;
			}
			else
			{
				if (!lockDirection)
				{
					spriteDirection = SetSpriteDirection (rightAmount, forwardAmount);
				}
				
				if (!doDirections)
				{
					flipFrames = false;
				}
				else if (frameFlipping == AC_2DFrameFlipping.LeftMirrorsRight && spriteDirection.Contains ("L"))
				{
					spriteDirection = spriteDirection.Replace ("L", "R");
					flipFrames = true;
				}
				else if (frameFlipping == AC_2DFrameFlipping.RightMirrorsLeft && spriteDirection.Contains ("R"))
				{
					spriteDirection = spriteDirection.Replace ("R", "L");
					flipFrames = true;
				}
				else
				{
					flipFrames = false;
				}
			}
			
			if (frameFlipping != AC_2DFrameFlipping.None)
			{
				if ((flipFrames && spriteChild.localScale.x > 0f) || (!flipFrames && spriteChild.localScale.x < 0f))
				{
					spriteChild.localScale = new Vector3 (-spriteChild.localScale.x, spriteChild.localScale.y, spriteChild.localScale.z);
				}
			}
			
			if (isTopDown)
			{
				if (animEngine && !animEngine.isSpriteBased)
				{
					spriteChild.rotation = transform.rotation;
					spriteChild.RotateAround (transform.position, Vector3.right, 90f);
				}
				else
				{
					spriteChild.rotation = Quaternion.Euler (90f, 0, 0);
				}
			}
			else if (isUnity2D)
			{
				spriteChild.rotation = Quaternion.Euler (0f, 0f, 0f);
			}
			else
			{
				spriteChild.rotation = Quaternion.Euler (spriteChild.rotation.eulerAngles.x, KickStarter.mainCamera.transform.rotation.eulerAngles.y, spriteChild.rotation.eulerAngles.z);
			}
			
			if (spriteChild.GetComponent <FollowSortingMap>())
			{	
				if (!lockScale)
				{
					spriteScale = spriteChild.GetComponent <FollowSortingMap>().GetLocalScale ();
				}
				
				if (spriteScale != 0f)
				{
					transform.localScale = originalScale * spriteScale;
					
					if (lockScale)
					{
						sortingMapScale = spriteScale;
					}
					else
					{
						sortingMapScale = spriteChild.GetComponent <FollowSortingMap>().GetLocalSpeed ();
					}
				}
			}
		}
		
		
		public void SetSorting (int order)
		{
			if (spriteChild)
			{
				if (spriteChild.GetComponent <FollowSortingMap>())
				{
					spriteChild.GetComponent <FollowSortingMap>().LockSortingOrder (order);
				}
				else
				{
					spriteChild.GetComponent <Renderer>().sortingOrder = order;
				}
			}
			
			if (GetComponent <Renderer>())
			{
				if (GetComponent <FollowSortingMap>())
				{
					GetComponent <FollowSortingMap>().LockSortingOrder (order);
				}
				else
				{
					GetComponent <Renderer>().sortingOrder = order;
				}
			}
		}
		
		
		public void SetSorting (string layer)
		{
			if (spriteChild)
			{
				if (spriteChild.GetComponent <FollowSortingMap>())
				{
					spriteChild.GetComponent <FollowSortingMap>().LockSortingLayer (layer);
				}
				else
				{
					spriteChild.GetComponent <Renderer>().sortingLayerName = layer;
				}
			}
			
			if (GetComponent <Renderer>())
			{
				if (GetComponent <FollowSortingMap>())
				{
					GetComponent <FollowSortingMap>().LockSortingLayer (layer);
				}
				else
				{
					GetComponent <Renderer>().sortingLayerName = layer;
				}
			}
		}
		
		
		public void ReleaseSorting ()
		{
			if (spriteChild && spriteChild.GetComponent <Renderer>() && spriteChild.GetComponent <FollowSortingMap>())
			{
				spriteChild.GetComponent <FollowSortingMap>().lockSorting = false;
			}
			
			if (GetComponent <Renderer>() && GetComponent <FollowSortingMap>())
			{
				GetComponent <FollowSortingMap>().lockSorting = false;
			}
		}
		
		
		public float GetMoveSpeed ()
		{
			if (doWallReduction)
			{
				Vector3 origin = transform.position + wallRayOrigin + (transform.forward * wallRayForward);
				RaycastHit hit;
				if (Physics.Raycast (origin, transform.forward, out hit, wallDistance, 1 << LayerMask.NameToLayer (wallLayer)))
				{
					wallReduction = Mathf.Lerp (wallReduction, (hit.point - origin).magnitude / wallDistance, Time.deltaTime * 10f);
				}
				else
				{
					wallReduction = Mathf.Lerp (wallReduction, 1f, Time.deltaTime * 10f);
				}
				return moveSpeed * wallReduction;
				
			}
			
			return moveSpeed;
		}
		
		
		virtual public void SetHeadTurnTarget (Vector3 position, bool isInstant, HeadFacing _headFacing = HeadFacing.Manual)
		{
			if (_headFacing == HeadFacing.Hotspot && headFacing == HeadFacing.Manual)
			{
				// Don't look at Hotspots if manually-set
				return;
			}
			
			headTurnTarget = position;
			headFacing = _headFacing;
			
			if (isInstant)
			{
				CalculateHeadTurn ();
				SnapHeadMovement ();
			}
		}
		
		
		public void ClearHeadTurnTarget (HeadFacing _headFacing, bool isInstant)
		{
			if (headFacing == _headFacing)
			{
				ClearHeadTurnTarget (isInstant);
			}
		}
		
		
		public void ClearHeadTurnTarget (bool isInstant)
		{
			headFacing = HeadFacing.None;
			
			if (isInstant)
			{
				targetHeadAngles = Vector2.zero;
				SnapHeadMovement ();
			}
		}
		
		
		public void SnapHeadMovement ()
		{
			actualHeadAngles = targetHeadAngles;
			AnimateHeadTurn ();
		}
		
		
		public bool IsMovingHead ()
		{
			if (actualHeadAngles != targetHeadAngles)
			{
				return true;
			}
			return false;
		}
		
		
		public Shapeable GetShapeable ()
		{
			Shapeable shapeable = GetComponent <Shapeable> ();
			if (shapeable == null)
			{
				shapeable = GetComponentInChildren <Shapeable>();
			}
			return shapeable;
		}
		
		
		private void UpdateHeadTurnAngle ()
		{
			CalculateHeadTurn ();
			
			if (IsMovingHead ())
			{
				AnimateHeadTurn ();
			}
		}
		
		
		private void CalculateHeadTurn ()
		{
			if (headFacing == HeadFacing.None)
			{
				targetHeadAngles = Vector2.Lerp (targetHeadAngles, Vector2.zero, Time.deltaTime * 4f);
				headTurnWeight = Mathf.Lerp (headTurnWeight, 0f, Time.deltaTime * 4f);
			}
			else
			{
				headTurnWeight = Mathf.Lerp (headTurnWeight, 1f, Time.deltaTime * 4f);

				// Horizontal
				Vector3 pointForward = headTurnTarget - transform.position;
				pointForward.y = 0f;
				targetHeadAngles.x = Vector3.Angle (transform.forward, pointForward);
				targetHeadAngles.x = Mathf.Min (targetHeadAngles.x, 60f);
				
				Vector3 crossProduct = Vector3.Cross (transform.forward, pointForward);
				float sideOn = Vector3.Dot (crossProduct, Vector2.up);
				
				if (sideOn < 0f)
				{
					targetHeadAngles.x *= -1f;
				}
				
				// Vertical
				Vector3 pointPitch = headTurnTarget;
				if (neckBone != null)
				{
					pointPitch -= neckBone.position;
				}
				else
				{
					pointPitch -= transform.position;
					if (_collider is CapsuleCollider)
					{
						CapsuleCollider capsuleCollder = (CapsuleCollider) _collider;
						pointPitch -= new Vector3 (0f, capsuleCollder.height * transform.localScale.y * 0.8f, 0f);
					}
				}
				
				targetHeadAngles.y = Vector3.Angle (pointPitch, pointForward);
				targetHeadAngles.y = Mathf.Min (targetHeadAngles.y, 30f);
				
				if (pointPitch.y < pointForward.y)
				{
					targetHeadAngles.y *= -1f;
				}
				
				targetHeadAngles.y *= (Vector3.Dot (transform.forward, pointForward.normalized) / 2f) + 0.5f;
				
				
				if (!ikHeadTurning)
				{
					targetHeadAngles.x /= 60f;
					targetHeadAngles.y /= 30f;
				}
				else
				{
					targetHeadAngles.y /= 60f;
				}
			}
		}
		
		
		private void AnimateHeadTurn ()
		{
			if (targetHeadAngles.x == 0f && KickStarter.stateHandler != null && KickStarter.stateHandler.gameState == GameState.Normal)
			{
				actualHeadAngles = Vector2.Lerp (actualHeadAngles, targetHeadAngles, Time.deltaTime * 3f);
			}
			else
			{
				actualHeadAngles = Vector2.Lerp (actualHeadAngles, targetHeadAngles, Time.deltaTime * 5f);
			}
			
			if (!ikHeadTurning)
			{
				animEngine.TurnHead (actualHeadAngles);
			}
		}
		
		
		public Vector2 GetScreenCentre ()
		{
			Vector3 worldPosition = transform.position;
			
			if (_collider && _collider is CapsuleCollider)
			{
				CapsuleCollider capsuleCollder = (CapsuleCollider) _collider;
				float addedHeight = capsuleCollder.height * transform.localScale.y;
				
				if (spriteChild != null && spriteChild.GetComponent <SpriteRenderer>())
				{
					addedHeight *= spriteChild.localScale.y;
				}
				
				if (KickStarter.settingsManager && KickStarter.settingsManager.IsTopDown ())
				{
					worldPosition.z += addedHeight;
				}
				else
				{
					worldPosition.y += addedHeight;
				}
			}
			else
			{
				if (spriteChild != null)
				{
					if (spriteChild.GetComponent <SpriteRenderer>())
					{
						worldPosition.y = spriteChild.GetComponent <SpriteRenderer>().bounds.extents.y + spriteChild.GetComponent <SpriteRenderer>().bounds.center.y;
					}
					else if (spriteChild.GetComponent <Renderer>())
					{
						worldPosition.y = spriteChild.GetComponent <Renderer>().bounds.extents.y + spriteChild.GetComponent <Renderer>().bounds.center.y;
					}
				}
			}
			
			Vector3 screenPosition = Camera.main.WorldToViewportPoint (worldPosition);
			return (new Vector2 (screenPosition.x, 1 - screenPosition.y));
		}
		
		
		private bool DoRigidbodyMovement ()
		{
			if (_rigidbody && (spriteChild == null || spriteChild.GetComponent <FollowSortingMap>() == null))
			{
				// Don't use Rigidbody's MovePosition etc if the localScale is being set - Unity bug 
				return true;
			}
			return false;
		}
		
		
		private void OnLevelWasLoaded ()
		{
			headFacing = HeadFacing.None;
			lockDirection = false;
			lockScale = false;
			isLipSyncing = false;
			lipSyncShapes.Clear ();
			ReleaseSorting ();
		}
		
		
		public void StartLipSync (List<LipSyncShape> _lipSyncShapes)
		{
			#if SalsaIsPresent
			if (KickStarter.speechManager.lipSyncMode == LipSyncMode.Salsa2D)
			{
				salsa2D = GetComponent <Salsa2D>();
				if (salsa2D == null)
				{
					Debug.LogWarning ("To perform Salsa 2D lipsyncing, Character GameObjects must have the 'Salsa2D' component attached.");
				}
			}
			else
			{
				salsa2D = null;
			}
			#endif
			
			lipSyncShapes = _lipSyncShapes;
			isLipSyncing = true;
		}
		
		
		public int GetLipSyncFrame ()
		{
			#if SalsaIsPresent
				if (isTalking && salsa2D != null)
				{
					return salsa2D.sayIndex;
				}
				return 0;
			#else
				if (isTalking && lipSyncShapes.Count > 0)
				{
					return lipSyncShapes[0].frame;
				}
				return 0;
			#endif
		}
		
		
		public float GetLipSyncNormalised ()
		{
			#if SalsaIsPresent
			if (salsa2D != null)
			{
				return ((float) salsa2D.sayIndex / (float) (KickStarter.speechManager.phonemes.Count - 1));
			}
			#endif
			
			if (lipSyncShapes.Count > 0)
			{
				return ((float) lipSyncShapes[0].frame / (float) (KickStarter.speechManager.phonemes.Count - 1));
			}
			return 0f;
		}
		
		
		public bool LipSyncGameObject ()
		{
			if (isLipSyncing && KickStarter.speechManager.lipSyncOutput == LipSyncOutput.PortraitAndGameObject)
			{
				return true;
			}
			return false;
		}
		
		
		private void ProcessLipSync ()
		{
			if (lipSyncShapes.Count > 0)
			{
				#if SalsaIsPresent
				if (salsa2D != null)
				{
					if (KickStarter.speechManager.lipSyncOutput == LipSyncOutput.GameObjectTexture && lipSyncTexture)
					{
						lipSyncTexture.SetFrame (GetLipSyncFrame ());
					}
					return;
				}
				#endif
				
				if (Time.time > lipSyncShapes[0].timeIndex)
				{
					if (KickStarter.speechManager.lipSyncOutput == LipSyncOutput.PortraitAndGameObject && shapeable)
					{
						if (lipSyncShapes.Count > 1)
						{
							float moveTime = lipSyncShapes[1].timeIndex - lipSyncShapes[0].timeIndex;
							shapeable.SetActiveKey (lipSyncGroupID, lipSyncShapes[1].frame, 100f, moveTime, MoveMethod.Smooth, null);
						}
						else
						{
							shapeable.SetActiveKey (lipSyncGroupID, 0, 100f, 0.2f, MoveMethod.Smooth, null);
						}
					}
					else if (KickStarter.speechManager.lipSyncOutput == LipSyncOutput.GameObjectTexture && lipSyncTexture)
					{
						lipSyncTexture.SetFrame (lipSyncShapes[0].frame);
					}
					
					lipSyncShapes.RemoveAt (0);
				}
			}
		}
		
		
		public void StopSpeaking ()
		{
			isTalking = false;
			
			if (_animation && !isLipSyncing)
			{
				foreach (AnimationState state in _animation)
				{
					if (state.layer == (int) AnimLayer.Mouth)
					{
						state.normalizedTime = 1f;
						state.weight = 0f;
					}
				}
			}
			
			if (shapeable != null && KickStarter.speechManager.lipSyncOutput == LipSyncOutput.PortraitAndGameObject)
			{
				shapeable.DisableAllKeys (lipSyncGroupID, 0.1f, MoveMethod.Curved, null);
			}
			else if (lipSyncTexture != null && KickStarter.speechManager.lipSyncOutput == LipSyncOutput.GameObjectTexture)
			{
				lipSyncTexture.SetFrame (0);
			}
			
			lipSyncShapes.Clear ();
			
			if (GetComponent <AudioSource>())
			{
				GetComponent<AudioSource>().Stop();
			}
		}
		
		
		public int GetCurrentPhoneme ()
		{
			if (lipSyncShapes != null && lipSyncShapes.Count > 0)
			{
				return lipSyncShapes[0].frame;
			}
			return 0;
		}
		
		
		public MotionControl GetMotionControl ()
		{
			if (animationEngine == AnimationEngine.Custom)
			{
				return motionControl;
			}
			return MotionControl.Automatic;
		}
		
		
		public virtual bool CanBeDirectControlled ()
		{
			return false;
		}
		
		
		public Vector3 GetTargetPosition ()
		{
			if (activePath && targetNode >= 0 && activePath.nodes.Count > targetNode)
			{
				return activePath.nodes[targetNode];
			}
			return transform.position;
		}
		
		
		public Quaternion GetTargetRotation ()
		{
			return Quaternion.LookRotation (lookDirection, Vector3.up);
		}
		
	}
}