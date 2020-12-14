/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"ActionCamera.cs"
 * 
 *	This action controls the MainCamera's "activeCamera",
 *	i.e., which GameCamera it is attached to.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{
	
	[System.Serializable]
	public class ActionCamera : Action
	{
		
		public int constantID = 0;
		public int parameterID = -1;
		public _Camera linkedCamera;
		
		public float transitionTime;
		public AnimationCurve timeCurve = new AnimationCurve (new Keyframe(0, 0), new Keyframe(1, 1));
		public MoveMethod moveMethod;
		public bool returnToLast;
		
		
		
		public ActionCamera ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Camera;
			title = "Switch";
			description = "Moves the MainCamera to the position, rotation and field of view of a specified GameCamera. Can be instantaneous or transition over time.";
		}
		
		
		override public void AssignValues (List<ActionParameter> parameters)
		{
			linkedCamera = AssignFile <_Camera> (parameters, parameterID, constantID, linkedCamera);
		}
		
		
		override public float Run ()
		{
			if (!isRunning)
			{
				isRunning = true;
				
				MainCamera mainCam = KickStarter.mainCamera;
				
				if (mainCam)
				{
					_Camera cam = linkedCamera;
					
					if (returnToLast && mainCam.lastNavCamera)
					{
						if (mainCam.lastNavCamera != null && mainCam.attachedCamera == mainCam.lastNavCamera)
						{
							cam = (_Camera) mainCam.lastNavCamera2;
						}
						else
						{
							cam = (_Camera) mainCam.lastNavCamera;
						}
					}
					
					if (cam)
					{
						if (mainCam.attachedCamera != cam)
						{
							if (cam is GameCameraThirdPerson)
							{
								GameCameraThirdPerson tpCam = (GameCameraThirdPerson) cam;
								tpCam.ResetRotation ();
							}
							else if (cam is GameCameraAnimated)
							{
								GameCameraAnimated animCam = (GameCameraAnimated) cam;
								animCam.PlayClip ();
							}
							
							mainCam.SetGameCamera (cam);
							if (transitionTime > 0f)
							{
								if (linkedCamera is GameCamera25D)
								{
									mainCam.SnapToAttached ();
									Debug.LogWarning ("Switching to a 2.5D camera (" + linkedCamera.name + ") must be instantaneous.");
								}
								else
								{
									mainCam.SmoothChange (transitionTime, moveMethod, timeCurve);
									
									if (willWait)
									{
										return (transitionTime);
									}
								}
							}
							else
							{
								cam.MoveCameraInstant ();
								mainCam.SnapToAttached ();
								
								if (linkedCamera is GameCameraAnimated && willWait)
								{
									return (defaultPauseTime);
								}
							}
						}
					}
				}
			}
			else
			{
				if (linkedCamera is GameCameraAnimated && willWait)
				{
					GameCameraAnimated animatedCamera = (GameCameraAnimated) linkedCamera;
					if (animatedCamera.isPlaying ())
					{
						return defaultPauseTime;
					}
					else
					{
						isRunning = false;
						return 0f;
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
			MainCamera mainCam = KickStarter.mainCamera;
			if (mainCam)
			{
				_Camera cam = linkedCamera;
				
				if (returnToLast && mainCam.lastNavCamera)
				{
					if (mainCam.lastNavCamera != null && mainCam.attachedCamera == mainCam.lastNavCamera)
					{
						cam = (_Camera) mainCam.lastNavCamera2;
					}
					else
					{
						cam = (_Camera) mainCam.lastNavCamera;
					}
				}
				
				if (cam)
				{
					if (cam is GameCameraThirdPerson)
					{
						GameCameraThirdPerson tpCam = (GameCameraThirdPerson) cam;
						tpCam.ResetRotation ();
					}
					
					mainCam.SetGameCamera (cam);
					
					cam.MoveCameraInstant ();
					mainCam.SnapToAttached ();
				}
			}
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			bool showWaitOption = false;
			returnToLast = EditorGUILayout.Toggle ("Return to last gameplay?", returnToLast);
			
			if (!returnToLast)
			{
				parameterID = Action.ChooseParameterGUI ("New camera:", parameters, parameterID, ParameterType.GameObject);
				if (parameterID >= 0)
				{
					constantID = 0;
					linkedCamera = null;
				}
				else
				{
					linkedCamera = (_Camera) EditorGUILayout.ObjectField ("New camera:", linkedCamera, typeof (_Camera), true);
					
					constantID = FieldToID <_Camera> (linkedCamera, constantID);
					linkedCamera = IDToField <_Camera> (linkedCamera, constantID, true);
				}
				
				if (linkedCamera && linkedCamera is GameCameraAnimated)
				{
					GameCameraAnimated animatedCamera = (GameCameraAnimated) linkedCamera;
					if (animatedCamera.animatedCameraType == AnimatedCameraType.PlayWhenActive && transitionTime <= 0f)
					{
						showWaitOption = true;
					}
				}
			}
			
			if (linkedCamera is GameCamera25D && !returnToLast)
			{
				transitionTime = 0f;
			}
			else
			{
				transitionTime = EditorGUILayout.FloatField ("Transition time (s):", transitionTime);
				
				if (transitionTime > 0f)
				{
					moveMethod = (MoveMethod) EditorGUILayout.EnumPopup ("Move method:", moveMethod);
					showWaitOption = true;
					
					if (moveMethod == MoveMethod.CustomCurve)
					{
						timeCurve = EditorGUILayout.CurveField ("Time curve:", timeCurve);
					}
				}
			}
			
			if (showWaitOption)
			{
				willWait = EditorGUILayout.Toggle ("Wait until finish?", willWait);
			}
			
			AfterRunningOption ();
		}
		
		
		override public string SetLabel ()
		{
			string labelAdd = "";
			if (linkedCamera && !returnToLast)
			{
				labelAdd = " (" + linkedCamera.name + ")";
			}
			
			return labelAdd;
		}
		
		#endif
		
	}
	
}