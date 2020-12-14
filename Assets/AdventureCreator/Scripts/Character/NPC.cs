/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"NPC.cs"
 * 
 *	This is attached to all non-Player characters.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{

	public class NPC : Char
	{

		public Char followTarget = null;
		public bool followTargetIsPlayer = false;
		public float followFrequency = 0f;
		public float followDistance = 0f;
		public float followDistanceMax = 0f;

		public bool moveOutOfPlayersWay = false;
		public float minPlayerDistance = 1f;

		private LayerMask LayerOn;
		private LayerMask LayerOff;
		
		
		private void Awake ()
		{
			LayerOn = LayerMask.NameToLayer (KickStarter.settingsManager.hotspotLayer);
			LayerOff = LayerMask.NameToLayer (KickStarter.settingsManager.deactivatedLayer);

			_Awake ();
		}

		
		private void FixedUpdate ()
		{
			if (activePath && followTarget)
			{
				FollowCheckDistance ();
				FollowCheckDistanceMax ();
			}

			if (activePath && !pausePath)
			{
				if (IsTurningBeforeWalking ())
				{
					charState = CharState.Idle;
				}
				else 
				{
					charState = CharState.Move;
					CheckIfStuck ();
				}
			}

			_FixedUpdate ();
		}


		private void Update ()
		{
			if (moveOutOfPlayersWay && charState == CharState.Idle)
			{
				StayAwayFromPlayer ();
			}

			_Update ();
		}


		private void StayAwayFromPlayer ()
		{
			if (followTarget == null && KickStarter.player != null && Vector3.Distance (transform.position, KickStarter.player.transform.position) < minPlayerDistance)
			{
				// Move out the way
				Vector3[] pointArray = TryNavPoint (transform.position - KickStarter.player.transform.position);
				int i=0;

				if (pointArray == null)
				{
					// Right
					pointArray = TryNavPoint (Vector3.Cross (transform.up, transform.position - KickStarter.player.transform.position).normalized);
					i++;
				}

				if (pointArray == null)
				{
					// Left
					pointArray = TryNavPoint (Vector3.Cross (-transform.up, transform.position - KickStarter.player.transform.position).normalized);
					i++;
				}

				if (pointArray == null)
				{
					// Towards
					pointArray = TryNavPoint (KickStarter.player.transform.position - transform.position);
					i++;
				}

				if (pointArray != null)
				{
					if (i == 0)
					{
						MoveAlongPoints (pointArray, false);
					}
					else
					{
						MoveToPoint (pointArray [pointArray.Length - 1], false);
					}
				}
			}
		}


		private Vector3[] TryNavPoint (Vector3 _direction)
		{
			Vector3 _targetPosition = transform.position + _direction.normalized * minPlayerDistance * 1.2f;

			if (KickStarter.settingsManager.ActInScreenSpace ())
			{
				_targetPosition = AdvGame.GetScreenNavMesh (_targetPosition);
			}
			else if (KickStarter.settingsManager.cameraPerspective == CameraPerspective.ThreeD)
			{
				_targetPosition.y = transform.position.y;
			}
			
			Vector3[] pointArray = KickStarter.navigationManager.navigationEngine.GetPointsArray (this, transform.position, _targetPosition);
			if (Vector3.Distance (pointArray [pointArray.Length-1], transform.position) < minPlayerDistance * 0.6f)
			{
				// Not far away enough
				return null;
			}
			return pointArray;
		}


		public void FollowReset ()
		{
			FollowStop ();

			followTarget = null;
			followTargetIsPlayer = false;
			followFrequency = 0f;
			followDistance = 0f;
		}


		private void FollowUpdate ()
		{
			if (followTarget)
			{
				FollowMove ();
				Invoke ("FollowUpdate", followFrequency);
			}
		}


		private void FollowMove ()
		{
			float dist = FollowCheckDistance ();
			if (dist > followDistance)
			{
				Paths path = GetComponent <Paths>();
				if (path == null)
				{
					Debug.LogWarning ("Cannot move a character with no Paths component");
				}
				else
				{
					path.pathType = AC_PathType.ForwardOnly;
					path.affectY = true;
					
					Vector3[] pointArray;
					Vector3 targetPosition = followTarget.transform.position;
					
					if (KickStarter.settingsManager && KickStarter.settingsManager.ActInScreenSpace ())
					{
						targetPosition = AdvGame.GetScreenNavMesh (targetPosition);
					}
					
					if (KickStarter.navigationManager)
					{
						pointArray = KickStarter.navigationManager.navigationEngine.GetPointsArray (transform.position, targetPosition);
					}
					else
					{
						List<Vector3> pointList = new List<Vector3>();
						pointList.Add (targetPosition);
						pointArray = pointList.ToArray ();
					}

					if (dist > followDistanceMax)
					{
						MoveAlongPoints (pointArray, true);
					}
					else
					{
						MoveAlongPoints (pointArray, false);
					}
				}
			}
		}


		private float FollowCheckDistance ()
		{
			float dist = Vector3.Distance (followTarget.transform.position, transform.position);

			if (dist < followDistance)
			{
				EndPath ();
			}

			return (dist);
		}


		private void FollowCheckDistanceMax ()
		{
			if (followTarget)
			{
				if (FollowCheckDistance () > followDistanceMax)
				{
					if (!isRunning)
					{
						FollowMove ();
					}
				}
				else if (isRunning)
				{
					FollowMove ();
				}
			}
		}


		private void FollowStop ()
		{
			StopCoroutine ("FollowUpdate");

			if (followTarget != null)
			{
				EndPath ();
			}
		}


		public void FollowAssign (Char _followTarget, bool _followTargetIsPlayer, float _followFrequency, float _followDistance, float _followDistanceMax)
		{
			if (_followTargetIsPlayer)
			{
				_followTarget = KickStarter.player;
			}

			if (_followTarget == null || _followFrequency == 0f || _followFrequency < 0f || _followDistance <= 0f || _followDistanceMax <= 0f)
			{
				FollowReset ();
				return;
			}

			followTarget = _followTarget;
			followTargetIsPlayer = _followTargetIsPlayer;
			followFrequency = _followFrequency;
			followDistance = _followDistance;
			followDistanceMax = _followDistanceMax;

			FollowUpdate ();
		}
		
		
		private void TurnOn ()
		{
			gameObject.layer = LayerOn;
		}
		

		private void TurnOff ()
		{
			gameObject.layer = LayerOff;
		}
		
	}

}