/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"OuyaIntegration.cs"
 * 
 *	This script contains static functions for playing
 *	AC games on the OUYA platform
 *
 *	All credit for this script goes to Tim Graupmann.
 * 
 */

using UnityEngine;
#if UNITY_ANDROID && !UNITY_EDITOR && OUYAIsPresent
using tv.ouya.console.api;
#endif


public static class OuyaIntegration
{
	
	public static bool IsDefinePresent ()
	{
		#if OUYAIsPresent
		return true;
		#else
		return false;
		#endif
	}
	
	
	public static Vector3 mousePosition
	{
		get
		{
			#if UNITY_ANDROID && !UNITY_EDITOR && OUYAIsPresent
			float halfWidth = Screen.width * 0.5f;
			float halfHeight = Screen.height * 0.5f;
			float x = halfWidth + halfWidth * OuyaSDK.OuyaInput.GetAxis(0, OuyaController.AXIS_LS_X);
			float y = halfHeight - halfHeight * OuyaSDK.OuyaInput.GetAxis(0, OuyaController.AXIS_LS_Y);
			return new Vector3(x, y, 0);
			#else
			return Input.mousePosition;
			#endif
		}
	}
	
	
	public static float GetAxis (string axisName)
	{
		#if UNITY_ANDROID && !UNITY_EDITOR && OUYAIsPresent
		switch (axisName)
		{
		case "Horizontal":
			return OuyaSDK.OuyaInput.GetAxis(0, OuyaController.AXIS_LS_X);
		case "Vertical":
			return OuyaSDK.OuyaInput.GetAxis(0, OuyaController.AXIS_LS_Y);
		case "CursorHorizontal":
			return OuyaSDK.OuyaInput.GetAxis(0, OuyaController.AXIS_RS_X);
		case "CursorVertical":
			return OuyaSDK.OuyaInput.GetAxis(0, OuyaController.AXIS_RS_Y);
		default:
			Debug.LogError(string.Format("Unknown Axis: {0}", axisName));
			break;
		}
		return 0f;
		#else
		return Input.GetAxis (axisName);
		#endif
	}
	
	
	public static float GetAxisRaw (string axisName)
	{
		#if UNITY_ANDROID && !UNITY_EDITOR && OUYAIsPresent
		return GetAxis (axisName);
		#else
		return Input.GetAxisRaw (axisName);
		#endif
	}
	
	
	public static bool GetButton (string buttonName)
	{
		#if UNITY_ANDROID && !UNITY_EDITOR && OUYAIsPresent
		switch (buttonName)
		{
		case "InteractionA":
			return OuyaSDK.OuyaInput.GetButton (0, OuyaController.BUTTON_O);
		case "InteractionB":
			return OuyaSDK.OuyaInput.GetButton (0, OuyaController.BUTTON_A);
		case "ToggleCursor":
			return OuyaSDK.OuyaInput.GetButton (0, OuyaController.BUTTON_R1);
		case "EndCutscene":
			return OuyaSDK.OuyaInput.GetButton (0, OuyaController.BUTTON_Y);
		case "Jump":
			return OuyaSDK.OuyaInput.GetButton (0, OuyaController.BUTTON_U);
		case "Run":
			return OuyaSDK.OuyaInput.GetButton (0, OuyaController.BUTTON_L1);
		case "FlashHotspots":
			return OuyaSDK.OuyaInput.GetButton (0, OuyaController.BUTTON_R3);
		case "Menu":
			return OuyaSDK.OuyaInput.GetButton (0, OuyaController.BUTTON_MENU);
		default:
			Debug.LogError(string.Format("Unknown Button: {0}", buttonName));
			return false;
		}
		#else
		return Input.GetButton (buttonName);
		#endif
	}
	
	
	public static bool GetButtonDown (string buttonName)
	{
		#if UNITY_ANDROID && !UNITY_EDITOR && OUYAIsPresent
		switch (buttonName)
		{
		case "InteractionA":
			return OuyaSDK.OuyaInput.GetButtonDown (0, OuyaController.BUTTON_O);
		case "InteractionB":
			return OuyaSDK.OuyaInput.GetButtonDown (0, OuyaController.BUTTON_A);
		case "ToggleCursor":
			return OuyaSDK.OuyaInput.GetButtonDown (0, OuyaController.BUTTON_R1);
		case "EndCutscene":
			return OuyaSDK.OuyaInput.GetButtonDown (0, OuyaController.BUTTON_Y);
		case "Jump":
			return OuyaSDK.OuyaInput.GetButtonDown (0, OuyaController.BUTTON_U);
		case "Run":
			return OuyaSDK.OuyaInput.GetButtonDown (0, OuyaController.BUTTON_L1);
		case "FlashHotspots":
			return OuyaSDK.OuyaInput.GetButtonDown (0, OuyaController.BUTTON_R3);
		case "Menu":
			return OuyaSDK.OuyaInput.GetButtonDown (0, OuyaController.BUTTON_MENU);
		default:
			Debug.LogError(string.Format ("Unknown Button: {0}", buttonName));
			return false;
		}
		#else
		return Input.GetButtonDown (buttonName);
		#endif
	}
	
	
	public static bool GetMouseButton (int button)
	{
		#if UNITY_ANDROID && !UNITY_EDITOR && OUYAIsPresent
		switch (button)
		{
		case 0:
			return OuyaSDK.OuyaInput.GetButton (0, OuyaController.BUTTON_O);
		default:
			Debug.LogError(string.Format ("Unknown Button: {0}", button));
			return false;
		}
		#else
		return Input.GetMouseButton (button);
		#endif
	}
	
	
	public static bool GetMouseButtonDown (int button)
	{
		#if UNITY_ANDROID && !UNITY_EDITOR && OUYAIsPresent
		switch (button)
		{
		case 0:
			return OuyaSDK.OuyaInput.GetButtonDown(0, OuyaController.BUTTON_O);
		default:
			Debug.LogError(string.Format("Unknown Button: {0}", button));
			return false;
		}
		#else
		return Input.GetMouseButtonDown (button);
		#endif
	}
	
}