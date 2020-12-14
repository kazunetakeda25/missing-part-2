/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"Moveable_PickUp.cs"
 * 
 *	Attaching this script to a GameObject allows it to be
 *	picked up and manipulated freely by the player.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	[RequireComponent (typeof (Rigidbody))]
	public class Moveable_PickUp : DragBase
	{
		
		public bool allowRotation = false;
		public float breakForce = 300f;

		public bool allowThrow = false;
		public float chargeTime = 0.5f;
		public float pullbackDistance = 0.6f;
		public float throwForce = 400f;

		private bool isChargingThrow = false;
		private float throwCharge = 0f;
		private float chargeStartTime;
		private bool inRotationMode = false;
		private FixedJoint fixedJoint;
		private float originalDistanceToCamera;
		private Vector3 lastPosition;

		
		protected override void Awake ()
		{
			base.Awake ();
			LimitCollisions ();
		}


		public override void UpdateMovement ()
		{
			if (moveSound && moveSoundClip && !inRotationMode)
			{
				if (numCollisions > 0)
			    {
					PlayMoveSound (_rigidbody.velocity.magnitude, 0.5f);
				}
				else if (moveSound.IsPlaying ())
				{
					moveSound.Stop ();
				}
			}
		}


		private void ChargeThrow ()
		{
			if (!isChargingThrow)
			{
				isChargingThrow = true;
				chargeStartTime = Time.time;
				throwCharge = 0f;
			}
			else if (throwCharge < 1f)
			{
				throwCharge = (Time.time - chargeStartTime) / chargeTime;
			}

			if (throwCharge > 1f)
			{
				throwCharge = 1f;
			}
		}


		private void ReleaseThrow ()
		{
			LetGo ();

			_rigidbody.useGravity = true;
			_rigidbody.drag = originalDrag;
			_rigidbody.angularDrag = originalAngularDrag;

			//isHeld = false;
			Vector3 moveVector = (transform.position - cameraTransform.position).normalized;
			_rigidbody.AddForce (throwForce * throwCharge * moveVector);
		}
		
		
		private void CreateFixedJoint ()
		{
			GameObject go = new GameObject (this.name + " (Joint)");
			Rigidbody body = go.AddComponent <Rigidbody>();
			body.constraints = RigidbodyConstraints.FreezeAll;
			body.useGravity = false;
			fixedJoint = go.AddComponent <FixedJoint>();
			fixedJoint.breakForce = fixedJoint.breakTorque = breakForce;
			//fixedJoint.enableCollision = false;
			go.AddComponent <JointBreaker>();
		}
		
		
		public override void Grab (Vector3 grabPosition)
		{
			base.Grab (grabPosition);

			inRotationMode = false;
			isChargingThrow = false;
			throwCharge = 0f;
			
			if (fixedJoint == null)
			{
				CreateFixedJoint ();
			}

			_rigidbody.velocity = _rigidbody.angularVelocity = Vector3.zero;
			lastPosition = transform.position;
			originalDistanceToCamera = (grabPosition - cameraTransform.position).magnitude;
		}
		
		
		public override void LetGo ()
		{
			if (fixedJoint != null && fixedJoint.connectedBody)
			{
				fixedJoint.connectedBody = null;
			}

			_rigidbody.drag = originalDrag;
			_rigidbody.angularDrag = originalAngularDrag;

			if (inRotationMode)
			{
				_rigidbody.velocity = Vector3.zero;
			}
			else if (!isChargingThrow)
			{
				Vector3 deltaPosition = transform.position - lastPosition;
				_rigidbody.AddForce (deltaPosition * Time.deltaTime * 100000f);
			}

			_rigidbody.useGravity = true;
			isHeld = false;
		}


		private void SetRotationMode (bool on)
		{
			_rigidbody.velocity = Vector3.zero;
			_rigidbody.useGravity = !on;
			inRotationMode = on;

			if (on)
			{
				KickStarter.playerInput.StartRotatingObject ();
				fixedJoint.connectedBody = null;
			}
			else
			{
				fixedJoint.connectedBody = _rigidbody;
			}
		}


		public override void ApplyDragForce (Vector3 force, Vector3 mousePos, float _distanceToCamera)
		{
			distanceToCamera = _distanceToCamera;

			if (allowThrow)
			{
				try
				{
					if (KickStarter.playerInput.InputGetButton ("ThrowMoveable"))
					{
						ChargeThrow ();
					}
					else if (isChargingThrow)
					{
						ReleaseThrow ();
					}
				}
				catch {}
			}

			if (allowRotation)
			{
				try
				{
					if (KickStarter.playerInput.InputGetButton ("RotateMoveable"))
					{
						SetRotationMode (true);
					}
					else if (KickStarter.playerInput.InputGetButtonUp ("RotateMoveable"))
					{
						SetRotationMode (false);
					}
				}
				catch {}
				try
				{
					if (KickStarter.playerInput.InputGetButtonDown ("RotateMoveableToggle"))
					{
						SetRotationMode (!inRotationMode);
					}
				}
				catch {}
			}

			// Scale force
			force *= speedFactor * _rigidbody.drag * distanceToCamera * Time.deltaTime;
			
			// Limit magnitude
			if (force.magnitude > maxSpeed)
			{
				force *= maxSpeed / force.magnitude;
			}

			lastPosition = transform.position;

			if (inRotationMode)
			{
				Vector3 newRot = Vector3.Cross (force, cameraTransform.forward);
				//newRot *= Mathf.Sqrt ((grabPoint.position - transform.position).magnitude) * 0.6f * rotationFactor;
				newRot /= Mathf.Sqrt ((grabPoint.position - transform.position).magnitude) * 2.4f * rotationFactor;
				_rigidbody.AddTorque (newRot);
			}
			else
			{
				mousePos.z = originalDistanceToCamera - (throwCharge * pullbackDistance);
				Vector3 worldPos = Camera.main.ScreenToWorldPoint (mousePos);
				fixedJoint.transform.position = worldPos;
				fixedJoint.connectedBody = _rigidbody;
			}

			if (allowZooming)
			{
				UpdateZoom ();
			}
		}


		new private void UpdateZoom ()
		{
			float zoom = Input.GetAxis ("ZoomMoveable");
			Vector3 moveVector = (transform.position - cameraTransform.position).normalized;
			
			if (distanceToCamera - minZoom < 1f && zoom < 0f)
			{
				moveVector *= (originalDistanceToCamera - minZoom);
			}
			else if (maxZoom - originalDistanceToCamera < 1f && zoom > 0f)
			{
				moveVector *= (maxZoom - originalDistanceToCamera);
			}
			
			if ((originalDistanceToCamera <= minZoom && zoom < 0f) || (originalDistanceToCamera >= maxZoom && zoom > 0f))
			{}
			else
			{
				originalDistanceToCamera += (zoom * zoomSpeed / 10f * Time.deltaTime);
			}
		}


		public void UnsetFixedJoint ()
		{
			fixedJoint = null;
			isHeld = false;
		}


		protected void LimitCollisions ()
		{
			Collider[] ownColliders = GetComponentsInChildren <Collider>();

			foreach (Collider _collider1 in ownColliders)
			{
				foreach (Collider _collider2 in ownColliders)
				{
					if (_collider1 == _collider2)
					{
						continue;
					}
					Physics.IgnoreCollision (_collider1, _collider2, true);
					Physics.IgnoreCollision (_collider1, _collider2, true);
				}

				if (ignorePlayerCollider)
				{
					if (GameObject.FindWithTag (Tags.player) && GameObject.FindWithTag (Tags.player).GetComponent <Collider>())
					{
						Collider playerCollider = GameObject.FindWithTag (Tags.player).GetComponent <Collider>();
						Physics.IgnoreCollision (playerCollider, _collider1, true);
					}
				}
			}

		}
		
		
		private void OnDestroy ()
		{
			if (fixedJoint)
			{
				Destroy (fixedJoint.gameObject);
				fixedJoint = null;
			}
		}

	}

}