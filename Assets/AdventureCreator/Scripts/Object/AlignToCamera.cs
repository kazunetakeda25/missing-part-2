/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"AlignToCamera.cs"
 * 
 *	Attach this script to an object you wish to align to a camera's view.
 *	This works best with sprites being used as foreground objects in 2.5D games.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	[ExecuteInEditMode]
	public class AlignToCamera : MonoBehaviour
	{

		public _Camera cameraToAlignTo;
		public float distanceToCamera;
		public bool lockScale;
		public float scaleFactor = 0f;


		private void Awake ()
		{
			Align ();
		}


		#if UNITY_EDITOR
		private void Update ()
		{
			if (!Application.isPlaying)
			{
				Align ();
			}
		}


		public void CentreToCamera ()
		{
			float distanceFromCamera = Vector3.Dot (cameraToAlignTo.transform.forward, transform.position - cameraToAlignTo.transform.position);
			if (distanceFromCamera == 0f)
			{
				return;
			}
			
			Vector3 newPosition = cameraToAlignTo.transform.position + (cameraToAlignTo.transform.forward * distanceFromCamera);
			transform.position = newPosition;
		}
		#endif


		private void Align ()
		{
			if (cameraToAlignTo)
			{
				transform.rotation = Quaternion.Euler (transform.rotation.eulerAngles.x, cameraToAlignTo.transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);

				if (distanceToCamera > 0f)
				{
					Vector3 relativePosition = transform.position - cameraToAlignTo.transform.position;
					float currentDistance = relativePosition.magnitude;
					if (currentDistance != distanceToCamera)
					{
						if (currentDistance != 0)
						{
							transform.position = cameraToAlignTo.transform.position + (relativePosition * distanceToCamera / currentDistance);
						}
						else
						{
							transform.position = cameraToAlignTo.transform.position + cameraToAlignTo.transform.forward * distanceToCamera;
						}
					}

					if (lockScale)
					{
						CalculateScale ();

						if (scaleFactor != 0f)
						{
							transform.localScale = Vector3.one * scaleFactor * distanceToCamera;
						}
					}
				}
				else if (distanceToCamera < 0f)
				{
					distanceToCamera = 0f;
				}
				else if (distanceToCamera == 0f)
				{
					Vector3 relativePosition = transform.position - cameraToAlignTo.transform.position;
					if (relativePosition.magnitude != 0f)
					{
						distanceToCamera = relativePosition.magnitude;
					}
				}
			}

			if (!lockScale || cameraToAlignTo == null)
			{
				scaleFactor = 0f;
			}
		}


		private void CalculateScale ()
		{
			if (scaleFactor == 0f)
			{
				scaleFactor = transform.localScale.y / distanceToCamera;
			}
		}

	}

}