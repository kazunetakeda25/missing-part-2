/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"FaceFXIntegration.cs"
 * 
 *	This script contains a number of static functions for use
 *	in integrating AC with the FaceFX asset.
 *
 *	To allow for FaceFX integration, the 'FaceFXIsPresent'
 *	preprocessor must be defined.  This can be done from
 *	Edit -> Project Settings -> Player, and entering
 *	'FaceFXIsPresent' into the Scripting Define Symbols text box
 *	for your game's build platform.
 *
 *	The FaceFX plugin for Unity can be downloaded here:
 *	http://unitydemos.facefx.com.s3.amazonaws.com/FaceFXBonesMorph.unitypackage
 * 
 */


using UnityEngine;
using System.Collections;


namespace AC
{
	
	public class FaceFXIntegration : ScriptableObject
	{
		
		public static bool IsDefinePresent ()
		{
			#if FaceFXIsPresent
			return true;
			#else
			return false;
			#endif
		}
		
		
		public static void Play (AC.Char speaker, string name, AudioClip audioClip)
		{
			#if FaceFXIsPresent
			FaceFXControllerScript_Base fcs = speaker.GetComponent <FaceFXControllerScript_Base>();
			if (fcs == null)
			{
				fcs = speaker.GetComponentInChildren <FaceFXControllerScript_Base>();
			}
			if (fcs != null)
			{
				speaker.isLipSyncing = true;
				fcs.PlayAnim ("Default_" + name, audioClip);
			}
			else
			{
				Debug.LogError ("No FaceFXControllerScript_Base script found on " + speaker.gameObject.name);
			}
			#else
			Debug.LogError ("The 'FaceFXIsPresent' preprocessor define must be declared in the Player Settings.");
			#endif
		}
		
	}
	
}