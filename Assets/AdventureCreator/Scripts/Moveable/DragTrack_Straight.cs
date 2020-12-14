/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"DragTrack_Linear.cs"
 * 
 *	This track constrains Moveable_Drag objects to a straight line
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	public class DragTrack_Straight : DragTrack
	{

		public DragRotationType rotationType = DragRotationType.None;
		public float maxDistance = 2f;
		public bool dragMustScrew = false;
		public float screwThread = 1f;


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

			draggable.maxCollider.transform.position = transform.position + (transform.up * maxDistance) + (transform.up * draggable.colliderRadius);
			draggable.minCollider.transform.position = transform.position - (transform.up * draggable.colliderRadius);
			
			draggable.minCollider.transform.up = transform.up;
			draggable.maxCollider.transform.up = -transform.up;

			base.AssignColliders (draggable);
		}


		public override void Connect (Moveable_Drag draggable)
		{
			AssignColliders (draggable);
		}


		public override float GetDecimalAlong (Moveable_Drag draggable)
		{
			return (draggable.transform.position - transform.position).magnitude / maxDistance;
		}


		public override void SetPositionAlong (float proportionAlong, Moveable_Drag draggable)
		{
			draggable.transform.position = transform.position + (transform.up * proportionAlong * maxDistance);

			if (rotationType != DragRotationType.None)
			{
				SetRotation (draggable, proportionAlong);
			}
		}


		public override void SnapToTrack (Moveable_Drag draggable, bool onStart)
		{
			Vector3 vec = draggable.transform.position - transform.position;
			float proportionAlong = Vector3.Dot (vec, transform.up) / maxDistance;

			if (onStart)
			{
				if (proportionAlong < 0f)
				{
					proportionAlong = 0f;
				}
				else if (proportionAlong > 1f)
				{
					proportionAlong = 1f;
				}

				if (rotationType != DragRotationType.None)
				{
					SetRotation (draggable, proportionAlong);
				}

				draggable._rigidbody.velocity = draggable._rigidbody.angularVelocity = Vector3.zero;
			}

			draggable.transform.position = transform.position + transform.up * proportionAlong * maxDistance;
		}


		public override void ApplyAutoForce (float _position, float _speed, Moveable_Drag draggable)
		{
			Vector3 tangentForce = transform.up * _speed;
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
			float dotProduct = 0f;

			if (rotationType == DragRotationType.Screw)
			{
				if (dragMustScrew)
				{
					draggable.UpdateScrewVector ();
				}

				dotProduct = Vector3.Dot (force, draggable._dragVector);
			}
			else
			{
				dotProduct = Vector3.Dot (force, transform.up);
			}

			// Calculate the amount of force along the tangent
			Vector3 tangentForce = transform.up * dotProduct;

			if (rotationType == DragRotationType.Screw)
			{
				if (dragMustScrew)
				{
					// Take radius into account
					tangentForce = (transform.up * dotProduct).normalized * force.magnitude;
					tangentForce /= Mathf.Sqrt ((draggable.GetGrabPosition () - draggable.transform.position).magnitude) / 0.4f;
				}
				tangentForce /= Mathf.Sqrt (screwThread);
			}

			draggable._rigidbody.AddForce (tangentForce);
		}


		public override bool IconIsStationary ()
		{
			if (rotationType == DragRotationType.Roll || (rotationType == DragRotationType.Screw && !dragMustScrew))
			{
				return true;
			}
			return false;
		}


		public override void UpdateDraggable (Moveable_Drag draggable)
		{
			SnapToTrack (draggable, false);
			draggable.trackValue = GetDecimalAlong (draggable);

			if (rotationType != DragRotationType.None)
			{
				SetRotation (draggable, draggable.trackValue);
			}
		}


		private void SetRotation (Moveable_Drag draggable, float proportionAlong)
		{
			float angle = proportionAlong * maxDistance / draggable.colliderRadius / 2f * Mathf.Rad2Deg;

			if (rotationType == DragRotationType.Roll)
			{
				draggable._rigidbody.rotation = Quaternion.AngleAxis (angle, transform.forward) * transform.rotation;
			}
			else if (rotationType == DragRotationType.Screw)
			{
				draggable._rigidbody.rotation = Quaternion.AngleAxis (angle * screwThread, transform.up) * transform.rotation;
			}
		}

	}
	
}