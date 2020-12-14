/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"GameCamera25D.cs"
 * 
 *	This GameCamera is fixed, but allows for a background image to be displayed.
 * 
 */

using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	public class GameCamera25D : _Camera
	{
		
		public BackgroundImage backgroundImage;
		public bool isActiveEditor = false;

		
		public void SetActiveBackground ()
		{
			if (backgroundImage)
			{
				// Move background images onto correct layer
				BackgroundImage[] backgroundImages = FindObjectsOfType (typeof (BackgroundImage)) as BackgroundImage[];
				foreach (BackgroundImage image in backgroundImages)
				{
					if (image == backgroundImage)
					{
						image.TurnOn ();
					}
					else
					{
						image.TurnOff ();
					}
				}
				
				// Set MainCamera's Clear Flags
				KickStarter.mainCamera.PrepareForBackground ();
			}
		}


		new public void ResetTarget ()
		{}


		public void UpdateCameraSnap ()
		{
			if (KickStarter.mainCamera)
			{
				KickStarter.mainCamera.transform.position = transform.position;
				KickStarter.mainCamera.transform.rotation = transform.rotation;

				Camera _camera = KickStarter.mainCamera.GetComponent <Camera>();
				_camera.orthographic = GetComponent <Camera>().orthographic;
				_camera.fieldOfView = GetComponent <Camera>().fieldOfView;
				_camera.farClipPlane = GetComponent <Camera>().farClipPlane;
				_camera.nearClipPlane = GetComponent <Camera>().nearClipPlane;
				_camera.orthographicSize = GetComponent <Camera>().orthographicSize;
			}
		}


		public void SnapCameraInEditor ()
		{
			GameCamera25D[] camera25Ds = FindObjectsOfType (typeof (GameCamera25D)) as GameCamera25D[];
			foreach (GameCamera25D camera25D in camera25Ds)
			{
				if (camera25D == this)
				{
					isActiveEditor = true;
				}
				else
				{
					camera25D.isActiveEditor = false;
				}
			}

			UpdateCameraSnap ();
		}
		
	}
		
}