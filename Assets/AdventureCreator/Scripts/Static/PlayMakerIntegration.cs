/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"PlayMakerIntegration.cs"
 * 
 *	This script contains static functions for use
 *	in calling PlayMaker FSMs.
 *
 *	To allow for PlayMaker integration, the 'PlayMakerIsPresent'
 *	preprocessor must be defined.  This can be done from
 *	Edit -> Project Settings -> Player, and entering
 *	'PlayMakerIsPresent' into the Scripting Define Symbols text box
 *	for your game's build platform.
 * 
 */

using UnityEngine;
using System.Collections;
#if PlayMakerIsPresent
using HutongGames.PlayMaker;
#endif

namespace AC
{
	
	public class PlayMakerIntegration : ScriptableObject
	{
		
		public static bool IsDefinePresent ()
		{
			#if PlayMakerIsPresent
			return true;
			#else
			return false;
			#endif
		}


		public static void CallEvent (GameObject linkedObject, string eventName, string fsmName)
		{
			#if PlayMakerIsPresent
			PlayMakerFSM[] playMakerFsms = linkedObject.GetComponents<PlayMakerFSM>();
			foreach (PlayMakerFSM playMakerFSM in playMakerFsms)
			{
				if (playMakerFSM.FsmName == fsmName)
				{
					playMakerFSM.Fsm.Event (eventName);
				}
			}
			#endif
		}
		
		
		public static void CallEvent (GameObject linkedObject, string eventName)
		{
			#if PlayMakerIsPresent
			if (linkedObject.GetComponent <PlayMakerFSM>())
			{
				PlayMakerFSM playMakerFSM = linkedObject.GetComponent <PlayMakerFSM>();
				playMakerFSM.Fsm.Event (eventName);
			}
			#endif
		}
		
		
		public static int GetGlobalInt (string _name)
		{
			#if PlayMakerIsPresent
			return (FsmVariables.GlobalVariables.GetFsmInt (_name).Value);
			#else
			return 0;
			#endif
		}
		
		
		public static bool GetGlobalBool (string _name)
		{
			#if PlayMakerIsPresent
			return (FsmVariables.GlobalVariables.GetFsmBool (_name).Value);
			#else
			return false;
			#endif
		}
		
		
		public static string GetGlobalString (string _name)
		{
			#if PlayMakerIsPresent
			return (FsmVariables.GlobalVariables.GetFsmString (_name).Value);
			#else
			return "";
			#endif
		}
		
		
		public static float GetGlobalFloat (string _name)
		{
			#if PlayMakerIsPresent
			return (FsmVariables.GlobalVariables.GetFsmFloat (_name).Value);
			#else
			return 0f;
			#endif
		}
		
		
		public static void SetGlobalInt (string _name, int _val)
		{
			#if PlayMakerIsPresent
			FsmVariables.GlobalVariables.FindFsmInt (_name).Value = _val;
			#endif
		}
		
		
		public static void SetGlobalBool (string _name, bool _val)
		{
			#if PlayMakerIsPresent
			FsmVariables.GlobalVariables.FindFsmBool (_name).Value = _val;
			#endif
		}
		
		
		public static void SetGlobalString (string _name, string _val)
		{
			#if PlayMakerIsPresent
			FsmVariables.GlobalVariables.FindFsmString (_name).Value = _val;
			#endif
		}
		
		
		public static void SetGlobalFloat (string _name, float _val)
		{
			#if PlayMakerIsPresent
			FsmVariables.GlobalVariables.FindFsmFloat (_name).Value = _val;
			#endif
		}
		
	}
	
}