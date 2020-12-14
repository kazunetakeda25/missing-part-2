/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"DragBase.cs"
 * 
 *	This the base class of draggable/pickup-able objects
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	public class DragBase : Moveable
	{

		public bool isHeld = false;
		public bool invertInput = false;
		public float maxSpeed = 200f;

		public bool allowZooming = false;
		public float zoomSpeed = 60f;
		public float minZoom = 1f;
		public float maxZoom = 3f;
		public float rotationFactor = 1f;

		public Rigidbody _rigidbody;

		public bool showIcon = false;
		public int iconID = -1;

		public AudioClip moveSoundClip;
		public AudioClip collideSoundClip;

		public float slideSoundThreshold = 0.03f;
		public float slidePitchFactor = 1f;
		public bool onlyPlayLowerCollisionSound = false;

		public bool ignoreMoveableRigidbodies;
		public bool ignorePlayerCollider;
		public bool childrenShareLayer;

		protected Transform grabPoint;
		protected Transform cameraTransform;
		protected float distanceToCamera;
		
		protected float speedFactor = 0.16f;
		
		protected float originalDrag;
		protected float originalAngularDrag;

		protected int numCollisions = 0;

		protected CursorIconBase icon;
		protected Sound collideSound;
		protected Sound moveSound;

				
		protected virtual void Awake ()
		{
			GameObject newOb = new GameObject ();
			newOb.name = this.name + " (Grab point)";
			grabPoint = newOb.transform;
			grabPoint.parent = this.transform;

			if (moveSoundClip)
			{
				GameObject newSoundOb = new GameObject ();
				newSoundOb.name = this.name + " (Move sound)";
				newSoundOb.transform.parent = this.transform;
				newSoundOb.AddComponent <Sound>();
				newSoundOb.GetComponent <AudioSource>().playOnAwake = false;
				moveSound = newSoundOb.GetComponent <Sound>();
			}

			icon = GetMainIcon ();

			if (Camera.main)
			{
				cameraTransform = Camera.main.transform;
			}

			if (GetComponent <Sound>())
			{
				collideSound = GetComponent <Sound>();
			}

			if (GetComponent <Rigidbody>())
			{
				_rigidbody = GetComponent <Rigidbody>();
			}
			else
			{
				Debug.LogWarning ("A Rigidbody component is required for " + this.name);
			}
		}


		public void TurnOn ()
		{
			PlaceOnLayer (LayerMask.NameToLayer (KickStarter.settingsManager.hotspotLayer));
      	}


		public void TurnOff ()
		{
			PlaceOnLayer (LayerMask.NameToLayer (KickStarter.settingsManager.deactivatedLayer));
		}


		private void PlaceOnLayer (int layerName)
		{
			gameObject.layer = layerName;
			if (childrenShareLayer)
			{
				foreach (Transform child in transform)
				{
					child.gameObject.layer = layerName;
				}
			}
		}


		public virtual void UpdateMovement ()
		{}


		private void OnCollisionEnter (Collision collision)
		{
			if (collision.gameObject.tag != Tags.player)
			{
				numCollisions ++;

				if (collideSound && collideSoundClip && Time.time > 0f)
				{
					collideSound.Play (collideSoundClip, false);
				}
			}
		}


		private void OnCollisionExit (Collision collision)
		{
			if (collision.gameObject.tag != Tags.player)
			{
				numCollisions --;
			}
		}


		protected void PlayMoveSound (float speed, float trackValue)
		{
			if (speed > slideSoundThreshold)
			{
				moveSound.relativeVolume = (speed - slideSoundThreshold);// * 5f;
				moveSound.SetMaxVolume ();
				if (slidePitchFactor > 0f)
				{
					moveSound.GetComponent <AudioSource>().pitch = Mathf.Lerp (GetComponent <AudioSource>().pitch, Mathf.Min (1f, speed), Time.deltaTime * 5f);
				}
			}

			if (speed > slideSoundThreshold && !moveSound.IsPlaying ())// && trackValue > 0.02f && trackValue < 0.98f)
			{
				moveSound.relativeVolume = (speed - slideSoundThreshold);// * 5f;
				moveSound.Play (moveSoundClip, true);
			}
			else if (speed <= slideSoundThreshold && moveSound.IsPlaying () && !moveSound.IsFading ())
			{
				moveSound.FadeOut (0.2f);
			}
		}


		public virtual void DrawGrabIcon ()
		{
			if (isHeld && showIcon && Camera.main.WorldToScreenPoint (transform.position).z > 0f && icon != null)
			{
				Vector3 screenPosition = Camera.main.WorldToScreenPoint (grabPoint.position);
				icon.Draw (new Vector3 (screenPosition.x, screenPosition.y));
			}
		}


		public virtual void LetGo ()
		{
			Debug.Log ("Letgo");
			isHeld = false;
			_rigidbody.drag = originalDrag;
			_rigidbody.angularDrag = originalAngularDrag;
		}
		
		
		public virtual void Grab (Vector3 grabPosition)
		{
			isHeld = true;
			grabPoint.position = grabPosition;
			originalDrag = _rigidbody.drag;
			originalAngularDrag = _rigidbody.angularDrag;
			_rigidbody.drag = 20f;
			_rigidbody.angularDrag = 20f;
		}


		public bool IsOnScreen ()
		{
			Vector2 screenPosition = Camera.main.WorldToScreenPoint (grabPoint.position);
			if (screenPosition.x < 0f || screenPosition.y < 0f || screenPosition.x > AdvGame.GetMainGameViewSize ().x || screenPosition.y > AdvGame.GetMainGameViewSize ().y)
			{
				return false;
			}
			return true;
		}


		public bool IsCloseToCamera (float maxDistance)
		{
			if ((GetGrabPosition () - KickStarter.mainCamera.transform.position).magnitude < maxDistance)
			{
				return true;
			}
			return false;
		}


		public virtual void ApplyDragForce (Vector3 force, Vector3 mousePosition, float distanceToCamera)
		{}


		protected void UpdateZoom ()
		{
			float zoom = Input.GetAxis ("ZoomMoveable");
			Vector3 moveVector = (transform.position - cameraTransform.position).normalized;
			
			if (distanceToCamera - minZoom < 1f && zoom < 0f)
			{
				moveVector *= (distanceToCamera - minZoom);
			}
			else if (maxZoom - distanceToCamera < 1f && zoom > 0f)
			{
				moveVector *= (maxZoom - distanceToCamera);
			}
			
			if ((distanceToCamera < minZoom && zoom < 0f) || (distanceToCamera > maxZoom && zoom > 0f))
			{
				_rigidbody.AddForce (-moveVector * zoom * zoomSpeed);
				_rigidbody.velocity = Vector3.zero;
			}
			else
			{
				_rigidbody.AddForce (moveVector * zoom * zoomSpeed);
			}
		}


		protected void LimitZoom ()
		{
			if (distanceToCamera < minZoom)
			{
				transform.position = cameraTransform.position + (transform.position - cameraTransform.position) / (distanceToCamera / minZoom);
			}
			else if (distanceToCamera > maxZoom)
			{
				transform.position = cameraTransform.position + (transform.position - cameraTransform.position) / (distanceToCamera / maxZoom);
			}
		}


		public Vector3 GetGrabPosition ()
		{
			return grabPoint.position;
		}


		private CursorIconBase GetMainIcon ()
		{
			if (KickStarter.cursorManager == null || iconID < 0)
			{
				return null;
			}
			
			return KickStarter.cursorManager.GetCursorIconFromID (iconID);
		}

	}

}