/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"_Camera.cs"
 * 
 *	This is the base class for GameCamera and FirstPersonCamera.
 * 
 */


using UnityEngine;
using System.Collections;

namespace AC
{

	public class _Camera : MonoBehaviour
	{

		public Camera _camera;
		public bool targetIsPlayer = true;
		public Transform target;
		public bool isDragControlled = false;
		public float focalDistance = 10f;

		protected Vector2 inputMovement;


		protected virtual void Awake ()
		{
			if (GetComponent <Camera>())
			{
				_camera = GetComponent <Camera>();

				if (KickStarter.mainCamera)
				{
					_camera.enabled = false;
				}
			}
			else
			{
				Debug.LogWarning (this.name + " has no Camera component!");
			}
		}


		public void SetCameraComponent ()
		{
			if (_camera == null && GetComponent <Camera>())
			{
				_camera = GetComponent <Camera>();
			}
		}


		public void ResetTarget ()
		{
			if (targetIsPlayer && KickStarter.player)
			{
				target = KickStarter.player.transform;
			}
		}


		public Vector3 PositionRelativeToCamera (Vector3 _position)
		{
			return (_position.x * ForwardVector ()) + (_position.z * RightVector ());
		}
		
		
		public Vector3 RightVector ()
		{
			return (transform.right);
		}
		
		
		public Vector3 ForwardVector ()
		{
			Vector3 camForward;
			
			camForward = transform.forward;
			camForward.y = 0;
			
			return (camForward);
		}
		
		
		public virtual void MoveCameraInstant ()
		{ }


		protected float ConstrainAxis (float desired, Vector2 range)
		{
			if (range.x < range.y)
			{
				desired = Mathf.Clamp (desired, range.x, range.y);
			}
			
			else if (range.x > range.y)
			{
				desired = Mathf.Clamp (desired, range.y, range.x);
			}
			
			else
			{
				desired = range.x;
			}
				
			return desired;
		}


		public void SetSplitScreen ()
		{
			_camera.enabled = true;
			_camera.rect = KickStarter.mainCamera.GetSplitScreenRect (false);
		}


		public void RemoveSplitScreen ()
		{
			if (_camera.enabled)
			{
				_camera.rect = new Rect (0f, 0f, 1f, 1f);
				_camera.enabled = false;
			}
		}

	}

}