/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"ActionCameraShake.cs"
 * 
 *	This action causes the MainCamera to shake,
 *	and also affects the BackgroundImage if one is active.
 * 
 */

using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	[System.Serializable]
	public class ActionCameraShake : Action
	{
		
		public int shakeIntensity;
		public float duration = 1f;
		
		
		public ActionCameraShake ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Camera;
			title = "Shake";
			description = "Causes the camera to shake, giving an earthquake screen effect. The method of shaking, i.e. moving or rotating, depends on the type of camera the Main Camera is linked to.";
		}
		
		
		override public float Run ()
		{
			MainCamera mainCam = KickStarter.mainCamera;
			
			if (mainCam)
			{
				if (!isRunning)
				{
					isRunning = true;
					
					if (mainCam.attachedCamera is GameCamera)
					{
						mainCam.Shake ((float) shakeIntensity / 67f, duration, true);
					}
					
					else if (mainCam.attachedCamera is GameCamera25D)
					{
						mainCam.Shake ((float) shakeIntensity / 67f, duration, true);
						
						GameCamera25D gameCamera = (GameCamera25D) mainCam.attachedCamera;
						if (gameCamera.backgroundImage)
						{
							gameCamera.backgroundImage.Shake (shakeIntensity / 0.67f, duration);
						}
					}
					
					else if (mainCam.attachedCamera is GameCamera2D)
					{
						mainCam.Shake ((float) shakeIntensity / 33f, duration, false);
					}
					
					else
					{
						mainCam.Shake ((float) shakeIntensity / 67f, duration, false);
					}
						
					if (willWait)
					{
						return (duration);
					}
				}
				else
				{
					isRunning = false;
					return 0f;
				}
			}
			
			return 0f;
		}


		override public void Skip ()
		{
			return;
		}

		
		#if UNITY_EDITOR

		override public void ShowGUI ()
		{
			shakeIntensity = EditorGUILayout.IntSlider ("Intensity:", shakeIntensity, 1, 10);
			duration = EditorGUILayout.FloatField ("Duration (s):", duration);
			willWait = EditorGUILayout.Toggle ("Wait until finish?", willWait);
			
			AfterRunningOption ();
		}
		
		
		override public string SetLabel ()
		{
			return "";
		}

		#endif
		
	}

}