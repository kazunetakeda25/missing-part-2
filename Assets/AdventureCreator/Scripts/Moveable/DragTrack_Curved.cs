/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"DragTrack_Curved.cs"
 * 
 *	This track constrains Moveable_Drag objects to a circular ring.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	public class DragTrack_Curved : DragTrack
	{

		public float maxAngle = 60f;
		public float radius = 2f;
		public bool doLoop = false;

		private Vector3 startPosition;


		public override void AssignColliders (Moveable_Drag draggable)
		{
			if (draggable.maxCollider == null)
			{
				draggable.maxCollider = (Collider) Instantiate (Resources.Load ("DragCollider", typeof (Collider)));
			}
			
			if (draggable.minCollider == null)
			{
				draggable.minCollider = (Collider) Instantiate (Resources.Load ("DragCollider", typeof (Collider)));
			}

			if (maxAngle > 360f)
			{
				maxAngle = 360f;
			}

			float offsetAngle = Mathf.Asin (draggable.colliderRadius / radius) * Mathf.Rad2Deg;

			draggable.maxCollider.transform.position = startPosition;
			draggable.maxCollider.transform.up = -transform.up;
			draggable.maxCollider.transform.RotateAround (transform.position, transform.forward, maxAngle + offsetAngle);

			draggable.minCollider.transform.position = startPosition;
			draggable.minCollider.transform.up = transform.up;
			draggable.minCollider.transform.RotateAround (transform.position, transform.forward, -offsetAngle);

			base.AssignColliders (draggable);
		}


		public override void Connect (Moveable_Drag draggable)
		{
			if (draggable._rigidbody.useGravity)
			{
				draggable._rigidbody.useGravity = false;
				Debug.LogWarning ("Curved tracks do not work with Rigidbodys that obey gravity - disabling");
			}

			startPosition = transform.position + (radius * transform.right);
			
			if (doLoop)
			{
				maxAngle = 360f;
				base.AssignColliders (draggable);
				return;
			}

			AssignColliders (draggable);
		}


		public override void ApplyAutoForce (float _position, float _speed, Moveable_Drag draggable)
		{
			Vector3 tangentForce = draggable.transform.up * _speed;
			if (draggable.trackValue < _position)
			{
				draggable._rigidbody.AddForce (tangentForce * Time.deltaTime);
			}
			else
			{
				draggable._rigidbody.AddForce (-tangentForce * Time.deltaTime);
			}
		}


		public override void ApplyDragForce (Vector3 force, Moveable_Drag draggable)
		{
			float dotProduct = Vector3.Dot (force, draggable.transform.up);
			
			// Calculate the amount of force along the tangent
			Vector3 tangentForce = draggable.transform.up * dotProduct;
			draggable._rigidbody.AddForce (tangentForce);
		}


		public override void SetPositionAlong (float proportionAlong, Moveable_Drag draggable)
		{
			Quaternion rotation = Quaternion.AngleAxis (proportionAlong * maxAngle, transform.forward);
			draggable.transform.position = RotatePointAroundPivot (startPosition, transform.position, rotation);
			draggable.transform.rotation = Quaternion.AngleAxis (proportionAlong * maxAngle, transform.forward) * transform.rotation;

			if (!doLoop)
			{
				UpdateColliders (proportionAlong, draggable);
			}
		}


		private Vector3 RotatePointAroundPivot (Vector3 point, Vector3 pivot, Quaternion angle)
		{
			return angle * (point - pivot) + pivot;
		}


		public override float GetDecimalAlong (Moveable_Drag draggable)
		{
			float angle = Vector3.Angle (-transform.right, draggable.transform.position - transform.position);

			// Sign of angle?
			if (angle < 170f && Vector3.Dot (draggable.transform.position - transform.position, transform.up) < 0f)
			{
				angle *= -1f;
			}

			return ((180f - angle) / maxAngle);
		}


		public override void SnapToTrack (Moveable_Drag draggable, bool onStart)
		{
			Vector3 LookAt = draggable.transform.position - transform.position;

			draggable.transform.position = transform.position + LookAt / (LookAt.magnitude / radius);

			if (onStart)
			{
				float proportionAlong = GetDecimalAlong (draggable);

				if (proportionAlong < 0f)
				{
					proportionAlong = 0f;
				}
				else if (proportionAlong > 1f)
				{
					proportionAlong = 1f;
				}

				draggable.transform.rotation = Quaternion.AngleAxis (proportionAlong * maxAngle, transform.forward) * transform.rotation;
				SetPositionAlong (proportionAlong, draggable);
			}
			else
			{
				draggable.transform.rotation = Quaternion.AngleAxis (draggable.trackValue * maxAngle, transform.forward) * transform.rotation;
			}
		}


		public override void UpdateDraggable (Moveable_Drag draggable)
		{
			draggable.trackValue = GetDecimalAlong (draggable);

			SnapToTrack (draggable, false);

			if (!doLoop)
			{
				UpdateColliders (draggable.trackValue, draggable);
			}
		}


		private void UpdateColliders (float trackValue, Moveable_Drag draggable)
		{
			if (trackValue > 1f)
			{
				return;
			}

			if (trackValue > 0.5f)
			{
				draggable.minCollider.isTrigger = true;
				draggable.maxCollider.isTrigger = false;
			}
			else
			{
				draggable.minCollider.isTrigger = false;
				draggable.maxCollider.isTrigger = true;
			}
		}

	}

}