/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"UltimateFPSIntegration.cs"
 * 
 *	This script contains a number of static functions for use
 *	in integrating AC with the Ultimate FPS asset
 *
 *	To allow for UFPS integration, the 'UltimateFPSIsPresent'
 *	preprocessor must be defined.  This can be done from
 *	Edit -> Project Settings -> Player, and entering
 *	'UltimateFPSIsPresent' into the Scripting Define Symbols text box
 *	for your game's build platform.
 *
 *	NOTE: AC is designed for UFPS v1.4.8 or later.
 * 
 */


using UnityEngine;
using System.Collections;


namespace AC
{
	
	public class UltimateFPSIntegration : ScriptableObject
	{

		#if UltimateFPSIsPresent
		private static vp_FPCamera fpCameraObject = null;
		private static vp_FPController fpControllerObject = null;
		private static vp_FPInput fpInputObject = null;

		public static vp_FPCamera fpCamera
		{
			get
			{
				if (fpCameraObject != null) return fpCameraObject;
				else
				{
					fpCameraObject = GameObject.FindObjectOfType <vp_FPCamera>();
					return fpCameraObject;
				}
			}
			set
			{
				fpCameraObject = value;
			}
		}

		public static vp_FPController fpController
		{
			get
			{
				if (fpControllerObject != null) return fpControllerObject;
				else
				{
					fpControllerObject = GameObject.FindObjectOfType <vp_FPController>();
					return fpControllerObject;
				}
			}
			set
			{
				fpControllerObject = value;
			}
		}

		public static vp_FPInput fpInput
		{
			get
			{
				if (fpInputObject != null) return fpInputObject;
				else
				{
					fpInputObject = GameObject.FindObjectOfType <vp_FPInput>();
					return fpInputObject;
				}
			}
			set
			{
				fpInputObject = value;
			}
		}
		#endif


		public static bool IsDefinePresent ()
		{
			#if UltimateFPSIsPresent
			return true;
			#else
			return false;
			#endif
		}


		public static void _Update (GameState gameState)
		{
			bool cursorLock = false;
			bool moveLock = false;
			bool cameraIsOn = false;

			if (gameState == GameState.Normal)
			{
				cursorLock = KickStarter.playerInput.cursorIsLocked;
				moveLock = !KickStarter.playerInput.isUpLocked;
				cameraIsOn = true;
			}

			if (gameState != GameState.Paused)
			{
				UltimateFPSIntegration.SetCameraEnabled (cameraIsOn);
			}

			UltimateFPSIntegration.SetMovementState (moveLock);
			UltimateFPSIntegration.SetCameraState (cursorLock);
		}


		public static Transform GetFPCamTransform ()
		{
			#if UltimateFPSIsPresent
			if (fpCamera)
			{
				return fpCamera.transform;
			}
			#endif
			return Camera.main.transform;
		}


		public static bool IsCursorForced ()
		{
			#if UltimateFPSIsPresent
			if (fpInput)
			{
				return fpInput.MouseCursorForced;
			}
			#endif
			return false;
		}


		public static void SetCameraEnabled (bool state, bool force = false)
		{
			#if UltimateFPSIsPresent
			if (fpCamera)
			{
				if (state)
				{
					KickStarter.mainCamera.attachedCamera = null;
				}

				if (KickStarter.mainCamera.attachedCamera == null && !state && !force)
				{
					// Don't do anything if the MainCamera has nothing else to do
					fpCamera.tag = Tags.mainCamera;
					KickStarter.mainCamera.tag = Tags.untagged;
					return;
				}

				// Need to disable camera, not gameobject, otherwise weapon cams will get wrong FOV
				foreach (Camera _camera in fpCamera.GetComponentsInChildren <Camera>())
				{
					_camera.enabled = state;
				}

				fpCamera.GetComponent <AudioListener>().enabled = state;
				KickStarter.mainCamera.GetComponent <AudioListener>().enabled = !state;

				if (state)
				{
					fpCamera.tag = Tags.mainCamera;
					KickStarter.mainCamera.tag = Tags.untagged;
				}
				else
				{
					fpCamera.tag = Tags.untagged;
					KickStarter.mainCamera.tag = Tags.mainCamera;
				}
			}
			#else
			Debug.Log ("The 'UltimateFPSIsPresent' preprocessor is not defined - check your Player Settings.");
			#endif
		}


		public static void SetMovementState (bool state)
		{
			#if UltimateFPSIsPresent
			if (KickStarter.playerInput.isUpLocked)
			{
				state = false;
			}

			if (fpInput)
			{
				fpInput.AllowGameplayInput = state;
			}
			else
			{
				Debug.LogWarning ("Cannot find 'vp_FPInput' script on Player.");
			}

			if (fpController)
			{
				if (state == false)
				{
					fpController.Stop ();
					if (fpInput != null && fpInput.FPPlayer != null)
					{
						fpInput.FPPlayer.Attack.TryStop ();
					}
				}
			}
			else
			{
				Debug.LogWarning ("Cannot find 'vp_FPController' script on Player.");
			}
			#else
			Debug.LogWarning ("The 'UltimateFPSIsPresent' preprocessor is not defined - check your Player Settings.");
			#endif
		}


		public static void SetCameraState (bool state)
		{
			#if UltimateFPSIsPresent
			if (KickStarter.playerInput.freeAimLock)
			{
				state = false;
			}

			if (fpInput)
			{
				fpInput.MouseCursorForced = !state;
			}
			else
			{
				Debug.LogWarning ("Cannot find 'vp_FPInput' script on Player.");
			}
			#else
			Debug.Log ("The 'UltimateFPSIsPresent' preprocessor is not defined - check your Player Settings.");
			#endif
		}


		public static void Teleport (Vector3 position)
		{
			#if UltimateFPSIsPresent
			if (fpCamera)
			{
				fpController.SetPosition (position);
			}
			else
			{
				Debug.LogWarning ("Cannot find 'vp_FPController' script.");
			}
			#endif
		}


		public static void SetRotation (Vector3 rotation)
		{
			#if UltimateFPSIsPresent
			if (fpCamera)
			{
				fpCamera.SetRotation (new Vector2 (rotation.x, rotation.y), true, true);
			}
			else
			{
				Debug.LogWarning ("Cannot find 'vp_FPCamera' script.");
			}
			#endif
		}


		public static void SetPitch (float pitch)
		{
			#if UltimateFPSIsPresent
			fpCamera.Angle = new Vector2 (fpCamera.transform.eulerAngles.x, pitch);
			#endif
		}


		public static void SetTilt (float tilt)
		{
			#if UltimateFPSIsPresent
			fpCamera.Angle = new Vector2 (tilt, fpCamera.transform.eulerAngles.y);
			#endif
		}

	}
	
}