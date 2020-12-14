/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"DragTrack_Hinge.cs"
 * 
 *	This track fixes a Moveable_Drag's position, so it can only be rotated
 *	in a circle.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	public class DragTrack_Hinge : DragTrack
	{
	
		public float maxAngle = 60f;
		public float radius = 2f;
		public bool doLoop = false;
		public bool limitRevolutions = false;
		public int maxRevolutions = 0;
		public bool alignDragToFront = false;


		public override void AssignColliders (Moveable_Drag draggable)
		{
			return;
		}
		
		
		public override void Connect (Moveable_Drag draggable)
		{
			LimitCollisions (draggable);

			if (doLoop)
			{
				maxAngle = 360f;
			}
		}


		public override void ApplyAutoForce (float _position, float _speed, Moveable_Drag draggable)
		{
			Vector3 tangentForce = draggable.transform.forward * _speed;
			if (draggable.trackValue < _position)
			{
				draggable._rigidbody.AddTorque (tangentForce * Time.deltaTime);
			}
			else
			{
				draggable._rigidbody.AddTorque (-tangentForce * Time.deltaTime);
			}
		}


		public override void ApplyDragForce (Vector3 force, Moveable_Drag draggable)
		{
			float dotProduct = 0f;
			Vector3 axisOffset = Vector2.zero;

			if (!alignDragToFront)
			{
				dotProduct = Vector3.Dot (force, draggable.transform.up);

				// Invert force if on the "back" side
				axisOffset = GetAxisOffset (draggable.GetGrabPosition ());
				if (Vector3.Dot (draggable.transform.right, axisOffset) < 0f)
				{
					dotProduct *= -1f;
				}
			}
			else
			{
				// Use the Hinge's transform, not the Draggable's
				dotProduct = Vector3.Dot (force, transform.up);

				// Invert force if on the "back" side
				axisOffset = GetAxisOffset (draggable._dragVector);
				if (Vector3.Dot (transform.right, axisOffset) < 0f)
				{
					dotProduct *= -1f;
				}
			}

			// Calculate the amount of force along the tangent
			Vector3 tangentForce = (draggable.transform.forward * dotProduct).normalized;
			tangentForce *= force.magnitude;

			// Take radius into account
			tangentForce /= axisOffset.magnitude / 0.43f;

			draggable._rigidbody.AddTorque (tangentForce);
		}


		private Vector3 GetAxisOffset (Vector3 grabPosition)
		{
			float dist = Vector3.Dot (grabPosition, transform.forward);
			Vector3 axisPoint = transform.position + (transform.forward * dist);
			return (grabPosition - axisPoint);
		}
		
		
		public override void SetPositionAlong (float proportionAlong, Moveable_Drag draggable)
		{
			draggable.transform.position = transform.position;
			draggable.transform.rotation = Quaternion.AngleAxis (proportionAlong * maxAngle, transform.forward) * transform.rotation;
		}
		
		
		public override float GetDecimalAlong (Moveable_Drag draggable)
		{
			float angle = Vector3.Angle (transform.up, draggable.transform.up);

			if (Vector3.Dot (-transform.right, draggable.transform.up) < 0f)
			{
				angle = 360f - angle;
			}
			if (angle > 180f + maxAngle / 2f)
			{
				angle = 0f;
			}

			return (angle / maxAngle);
		}
		
		
		public override void SnapToTrack (Moveable_Drag draggable, bool onStart)
		{
			draggable.transform.position = transform.position;

			if (onStart)
			{
				draggable.transform.rotation = transform.rotation;
				draggable.trackValue = 0f;
			}
		}
		
		
		public override void UpdateDraggable (Moveable_Drag draggable)
		{
			float oldValue = draggable.trackValue;

			draggable.transform.position = transform.position;
			draggable.trackValue = GetDecimalAlong (draggable);

			if (draggable.trackValue <= 0f || draggable.trackValue > 1f)
			{
				if (draggable.trackValue < 0f)
				{
					draggable.trackValue = 0f;
				}
				else if (draggable.trackValue > 1f)
				{
					draggable.trackValue = 1f;
				}

				SetPositionAlong (draggable.trackValue, draggable);
				draggable._rigidbody.angularVelocity = Vector3.zero;
			}

			if (doLoop && limitRevolutions)
			{
				if (oldValue < 0.1f && draggable.trackValue > 0.9f)
				{
					draggable.revolutions --;
				}
				else if (oldValue > 0.9f && draggable.trackValue < 0.1f)
				{
					draggable.revolutions ++;
				}

				if (draggable.revolutions < 0)
				{
					draggable.revolutions = 0;
					draggable.trackValue = 0f;
					SetPositionAlong (draggable.trackValue, draggable);
					draggable._rigidbody.angularVelocity = Vector3.zero;
				}
				else if (draggable.revolutions > maxRevolutions - 1)
				{
					draggable.revolutions = maxRevolutions - 1;
					draggable.trackValue = 1f;
					SetPositionAlong (draggable.trackValue, draggable);
					draggable._rigidbody.angularVelocity = Vector3.zero;
				}
			}
		}

	}

}
