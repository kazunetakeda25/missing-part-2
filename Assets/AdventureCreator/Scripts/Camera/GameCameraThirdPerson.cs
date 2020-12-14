/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ThirdPersonCamera.cs"
 * 
 *	This is attached to a scene-based camera, similar to GameCamera and GameCamera2D.
 *	It should not be a child of the Player, but instead scene-specific.
 * 
 */

using UnityEngine;
using System.Collections;
using AC;

public class GameCameraThirdPerson : _Camera
{

	public enum RotationLock { Free, Locked, Limited };
	public RotationLock spinLock = RotationLock.Free;
	public RotationLock pitchLock = RotationLock.Locked;

	public float horizontalOffset = 0f;
	public float verticalOffset = 2f;

	public float distance = 2f;
	public bool allowMouseWheelZooming = false;
	public bool detectCollisions = true;
	public float minDistance = 1f;
	public float maxDistance = 3f;

	public bool toggleCursor = false;

	public float spinSpeed = 5f;
	public float spinAccleration = 5f;
	public float spinDeceleration = 5f;
	public string spinAxis = "";
	public bool invertSpin = false;
	public bool alwaysBehind = false;
	public bool resetSpinWhenSwitch = false;
	public float spinOffset = 0f;
	public float maxSpin = 40f;

	public float pitchSpeed = 3f;
	public float pitchAccleration = 20f;
	public float pitchDeceleration = 20f;
	public float maxPitch = 40f;
	public string pitchAxis = "";
	public bool invertPitch = false;
	public bool resetPitchWhenSwitch = false;

	private float collisionOffset = 0f;

	private float deltaDistance = 0f;
	private float deltaSpin = 0f;
	private float deltaPitch = 0f;

	private float roll = 0f;
	private float spin = 0f;
	private float pitch = 0f;

	private float initialPitch = 0f;
	private float initialSpin = 0f;

	private Vector3 centrePosition;
	private Vector3 targetPosition;
	private Quaternion targetRotation;


	protected override void Awake ()
	{
		base.Awake ();

		initialPitch = transform.eulerAngles.x;
		initialSpin = transform.eulerAngles.y;
	}


	private void Start ()
	{
		ResetTarget ();

		Vector3 angles = transform.eulerAngles;
		roll = angles.x; 
		spin = angles.y;

		UpdateTargets ();
		SnapMovement ();
	}



	public void ResetRotation ()
	{
		if (pitchLock != RotationLock.Locked && resetPitchWhenSwitch)
		{
			pitch = initialPitch;
		}
		if (spinLock != RotationLock.Locked && resetSpinWhenSwitch)
		{
			spin = initialSpin;
		}
	}


	private void DetectCollisions ()
	{
		if (detectCollisions && target != null)
		{
			RaycastHit hit;
			if (Physics.Linecast (target.position + new Vector3 (0, verticalOffset, 0f), targetPosition, out hit))
			{
				collisionOffset = (targetPosition - hit.point).magnitude;
			}
			else
			{
				collisionOffset = 0f;
			}
		}
	}


	private void FixedUpdate ()
	{
		UpdateTargets ();
		DetectCollisions ();

		transform.rotation = Quaternion.Lerp (transform.rotation, targetRotation, Time.deltaTime * 10f);
		transform.position = Vector3.Lerp (transform.position, targetPosition - (targetPosition - centrePosition).normalized * collisionOffset, Time.deltaTime * 10f);
	}


	private void UpdateTargets ()
	{
		if (!target)
		{
			return;
		}
		
		if (KickStarter.stateHandler == null || KickStarter.stateHandler.gameState == AC.GameState.Normal)
		{
			if (allowMouseWheelZooming && minDistance < maxDistance)
			{
				if (Input.GetAxis ("Mouse ScrollWheel") < 0f)
				{
					deltaDistance = Mathf.Lerp (deltaDistance, Mathf.Min (spinSpeed, maxDistance - distance), spinAccleration/5f * Time.deltaTime);
				}
				else if (Input.GetAxis ("Mouse ScrollWheel") > 0f)
				{
					deltaDistance = Mathf.Lerp (deltaDistance, -Mathf.Min (spinSpeed, distance - minDistance), spinAccleration/5f * Time.deltaTime);
				}
				else
				{
					deltaDistance = Mathf.Lerp (deltaDistance, 0f, spinAccleration * Time.deltaTime);
				}
				
				distance += deltaDistance;
				distance = Mathf.Clamp (distance, minDistance, maxDistance);
			}
			
			if (KickStarter.playerInput.cursorIsLocked || !toggleCursor)
			{
				if (!isDragControlled)
				{
					inputMovement = new Vector2 (Input.GetAxis (spinAxis), Input.GetAxis (pitchAxis));
				}
				else
				{
					if (KickStarter.playerInput.dragState == DragState._Camera)
					{
						inputMovement = KickStarter.playerInput.GetDragVector ();
					}
					else
					{
						inputMovement = Vector2.zero;
					}
				}
				
				
				if (spinLock != RotationLock.Locked)
				{
					if (inputMovement.x > 0f)
					{
						deltaSpin = Mathf.Lerp (deltaSpin, spinSpeed, spinAccleration * Time.deltaTime * inputMovement.x);
					}
					else if (inputMovement.x < 0f)
					{
						deltaSpin = Mathf.Lerp (deltaSpin, -spinSpeed, spinAccleration * Time.deltaTime * -inputMovement.x);
					}
					else
					{
						deltaSpin = Mathf.Lerp (deltaSpin, 0f, spinDeceleration * Time.deltaTime);
					}
					
					if (spinLock == RotationLock.Limited)
					{
						if ((invertSpin && deltaSpin > 0f) || (!invertSpin && deltaSpin < 0f))
						{
							if (maxSpin - spin < 5f)
							{
								deltaSpin *= (maxSpin - spin) / 5f;
							}
						}
						else if ((invertSpin && deltaSpin < 0f) || (!invertSpin && deltaSpin > 0f))
						{
							if (maxSpin + spin < 5f)
							{
								deltaSpin *= (maxSpin + spin) / 5f;
							}
						}
					}
					
					if (invertSpin)
					{
						spin += deltaSpin;
					}
					else
					{
						spin -= deltaSpin;
					}
					
					if (spinLock == RotationLock.Limited)
					{
						spin = Mathf.Clamp (spin, -maxSpin, maxSpin);
					}
				}
				else
				{
					if (alwaysBehind)
					{
						spin = Mathf.LerpAngle (spin, target.eulerAngles.y + spinOffset, spinAccleration * Time.deltaTime);
					}
				}
				
				if (pitchLock != RotationLock.Locked)
				{
					if (inputMovement.y > 0f)
					{
						deltaPitch = Mathf.Lerp (deltaPitch, pitchSpeed, pitchAccleration * Time.deltaTime * inputMovement.y);
					}
					else if (inputMovement.y < 0f)
					{
						deltaPitch = Mathf.Lerp (deltaPitch, -pitchSpeed, pitchAccleration * Time.deltaTime * -inputMovement.y);
					}
					else
					{
						deltaPitch = Mathf.Lerp (deltaPitch, 0f, pitchDeceleration * Time.deltaTime);
					}
					
					if (pitchLock == RotationLock.Limited)
					{
						if ((invertPitch && deltaPitch > 0f) || (!invertPitch && deltaPitch < 0f))
						{
							if (maxPitch - pitch < 5f)
							{
								deltaPitch *= (maxPitch - pitch) / 5f;
							}
						}
						else if ((invertPitch && deltaPitch < 0f) || (!invertPitch && deltaPitch > 0f))
						{
							if (maxPitch + pitch < 5f)
							{
								deltaPitch *= (maxPitch + pitch) / 5f;
							}
						}
					}
					
					if (invertPitch)
					{
						pitch += deltaPitch;
					}
					else
					{
						pitch -= deltaPitch;
					}
					
					if (pitchLock == RotationLock.Limited)
					{
						pitch = Mathf.Clamp (pitch, -maxPitch, maxPitch);
					}
				}
			}
			
			if (pitchLock == RotationLock.Locked)
			{
				pitch = maxPitch;
			}
		}
		
		float finalSpin = spin;
		float finalPitch = pitch;
		if (!targetIsPlayer)
		{
			if (spinLock != RotationLock.Locked)
			{
				finalSpin += target.eulerAngles.y;
			}
			if (pitchLock != RotationLock.Locked)
			{
				finalPitch += target.eulerAngles.x;
			}
		}
		
		Quaternion rotation = Quaternion.Euler (finalPitch, finalSpin, roll);
		targetRotation = rotation;

		centrePosition = target.position + (Vector3.up * verticalOffset) + (rotation * Vector3.right * horizontalOffset);
		targetPosition = centrePosition - (rotation * Vector3.forward * distance);
	}


	private void SnapMovement ()
	{
		transform.rotation = targetRotation;
		transform.position = targetPosition;
	}

}