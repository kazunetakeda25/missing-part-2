/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"DragTrack.cs"
 * 
 *	The base class for "tracks", which are used to
 *	constrain Moveable_Drag objects along set paths
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	public class DragTrack : MonoBehaviour
	{

		public PhysicMaterial colliderMaterial;
		public float discSize = 0.2f;


		public virtual void AssignColliders (Moveable_Drag draggable)
		{
			draggable.maxCollider.transform.rotation = Quaternion.AngleAxis (90f, draggable.maxCollider.transform.right) * draggable.maxCollider.transform.rotation;
			draggable.minCollider.transform.rotation = Quaternion.AngleAxis (90f, draggable.minCollider.transform.right) * draggable.minCollider.transform.rotation;

			if (colliderMaterial)
			{
				draggable.maxCollider.material = colliderMaterial;
				draggable.minCollider.material = colliderMaterial;
			}

			draggable.maxCollider.transform.parent = this.transform;
			draggable.minCollider.transform.parent = this.transform;

			draggable.maxCollider.name = draggable.name + "_UpperLimit";
			draggable.minCollider.name = draggable.name + "_LowerLimit";

			LimitCollisions (draggable);
		}


		public virtual float GetDecimalAlong (Moveable_Drag draggable)
		{
			return 0f;
		}


		public virtual void SetPositionAlong (float dec, Moveable_Drag draggable)
		{}


		public virtual void Connect (Moveable_Drag draggable)
		{}

		public virtual void ApplyDragForce (Vector3 force, Moveable_Drag draggable)
		{}

		public virtual void ApplyAutoForce (float _position, float _speed, Moveable_Drag draggable)
		{}

		public virtual void UpdateDraggable (Moveable_Drag draggable)
		{
			draggable.trackValue = GetDecimalAlong (draggable);
		}

		public virtual void SnapToTrack (Moveable_Drag draggable, bool onStart)
		{}


		protected void LimitCollisions (Moveable_Drag draggable)
		{
			Collider[] allColliders = FindObjectsOfType (typeof(Collider)) as Collider[];
			Collider[] dragColliders = draggable.GetComponentsInChildren <Collider>();

			// Disable all collisions on max/min colliders
			if (draggable.minCollider != null && draggable.maxCollider != null)
			{
				foreach (Collider _collider in allColliders)
				{
					if (_collider.enabled)
					{
						if (_collider != draggable.minCollider)
						{
							Physics.IgnoreCollision (_collider, draggable.minCollider, true);
						}
						if (_collider != draggable.maxCollider)
						{
							Physics.IgnoreCollision (_collider, draggable.maxCollider, true);
						}
					}
				}
			}

			// Set collisions on draggable's colliders
			foreach (Collider _collider in allColliders)
			{
				foreach (Collider dragCollider in dragColliders)
				{
					if (_collider == dragCollider)
					{
						continue;
					}

					bool result = true;

					if ((draggable.minCollider != null && draggable.minCollider == _collider) || (draggable.maxCollider != null && draggable.maxCollider == _collider))
					{
						result = false;
					}
					else if (_collider.gameObject.tag == Tags.player)
					{
						result = draggable.ignorePlayerCollider;
					}
					else if (_collider.GetComponent <Rigidbody>() && _collider.gameObject != draggable.gameObject)
					{
						if (_collider.GetComponent <Moveable>())
						{
							result = draggable.ignoreMoveableRigidbodies;
						}
						else
						{
							result = false;
						}
					}

					if (_collider.enabled && dragCollider.enabled)
					{
						Physics.IgnoreCollision (_collider, dragCollider, result);
					}
				}
			}

			// Enable collisions between max/min collisions and draggable's colliders
			if (draggable.minCollider != null && draggable.maxCollider != null)
			{
				foreach (Collider _collider in dragColliders)
				{
					if (_collider.enabled && draggable.minCollider.enabled)
					{
						Physics.IgnoreCollision (_collider, draggable.minCollider, false);
					}
					if (_collider.enabled && draggable.maxCollider.enabled)
					{
						Physics.IgnoreCollision (_collider, draggable.maxCollider, false);
					}
				}
			}
		}


		public virtual bool IconIsStationary ()
		{
			return false;
		}

	}

}
