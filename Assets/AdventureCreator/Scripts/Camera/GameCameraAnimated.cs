using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{
	
	public class GameCameraAnimated : _Camera
	{

		public bool followCursor = false;
		public Vector2 cursorInfluence = new Vector2 (0.3f, 0.1f);

		public AnimationClip clip;
		public bool loopClip;
		public bool playOnStart;
		public AnimatedCameraType animatedCameraType = AnimatedCameraType.PlayWhenActive;
		public Paths pathToFollow;
		
		private float pathLength;
		
		
		public void Start ()
		{
			if (animatedCameraType == AnimatedCameraType.PlayWhenActive)
			{
				if (playOnStart)
				{
					PlayClip ();
				}
			}
			else if (pathToFollow)
			{
				pathLength = pathToFollow.GetTotalLength ();
				ResetTarget ();
				
				if (target)
				{
					MoveCameraInstant ();
				}
			}
		}
		
		
		private void Update ()
		{
			MoveCamera ();
		}


		public void SwitchTarget (Transform _target)
		{
			target = _target;
		}


		public bool isPlaying ()
		{
			if (clip && GetComponent <Animation>() && GetComponent <Animation>().IsPlaying (clip.name))
			{
				return true;
			}

			return false;
		}
		
		
		public void PlayClip ()
		{
			if (GetComponent <Animation>() == null)
			{
				Debug.LogError ("Cannot play animation on " + this.name + " - no Animation component is attached.");
				return;
			}
			
			if (clip && animatedCameraType == AnimatedCameraType.PlayWhenActive)
			{
				WrapMode wrapMode = WrapMode.Once;
				if (loopClip)
				{
					wrapMode = WrapMode.Loop;
				}
				AdvGame.PlayAnimClip (GetComponent <Animation>(), 0, clip, AnimationBlendMode.Blend, wrapMode, 0f, null, false);
			}
		}
		
		
		public override void MoveCameraInstant ()
		{
			MoveCamera ();
		}
		
		
		private void MoveCamera ()
		{
			if (target && animatedCameraType == AnimatedCameraType.SyncWithTargetMovement && clip && target)
			{
				AdvGame.PlayAnimClipFrame (GetComponent <Animation>(), 0, clip, AnimationBlendMode.Blend, WrapMode.Once, 0f, null, GetProgress ());
			}
		}


		private float GetProgress ()
		{
			if (pathToFollow.nodes.Count <= 1)
			{
				return 0f;
			}

			double nearest_dist = 1000f;
			Vector3 nearestPoint = Vector3.zero;
			int i =0;

			for (i=1; i <pathToFollow.nodes.Count; i++)
			{
				Vector3 p1 = pathToFollow.nodes[i-1];
				Vector3 p2 = pathToFollow.nodes[i];
				
				Vector3 p = GetNearestPointOnSegment (p1, p2);
				if (p != nearestPoint)
				{
					float d = Mathf.Sqrt (Vector3.Distance (target.position, p));
					if (d < nearest_dist)
					{
						nearest_dist = d;
						nearestPoint = p;
					}
					else
						break;
				}
			}
			
			return (pathToFollow.GetLengthToNode (i-2) + Vector3.Distance (pathToFollow.nodes[i-2], nearestPoint)) / pathLength;
		}

		
		private Vector3 GetNearestPointOnSegment (Vector3 p1, Vector3 p2)
		{
			float d2 = (p1.x - p2.x)*(p1.x - p2.x) + (p1.z - p2.z)*(p1.z - p2.z);
			float t = ((target.position.x - p1.x) * (p2.x - p1.x) + (target.position.z - p1.z) * (p2.z - p1.z)) / d2;
			
			if (t < 0)
			{
				return p1;
			}
			if (t > 1)
			{
				return p2;
			}
			
			return new Vector3 ((p1.x + t * (p2.x - p1.x)), 0f, (p1.z + t * (p2.z - p1.z)));
		}

	}
	
}

