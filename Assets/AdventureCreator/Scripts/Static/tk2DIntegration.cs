/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"tk2DIntegration.cs"
 * 
 *	This script contains a number of static functions for use
 *	in playing back 2DToolkit sprite animations.  Requires 2DToolkit to work.
 *
 *	To allow for 2DToolkit integration, the 'tk2DIsPresent'
 *	preprocessor must be defined.  This can be done from
 *	Edit -> Project Settings -> Player, and entering
 *	'tk2DIsPresent' into the Scripting Define Symbols text box
 *	for your game's build platform.
 * 
 */


using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	public class tk2DIntegration : ScriptableObject
	{
		
		public static bool IsDefinePresent ()
		{
			#if tk2DIsPresent
				return true;
			#else
				return false;
			#endif
		}
		
		
		public static bool PlayAnimation (Transform sprite, string clipName, int frame)
		{
			#if tk2DIsPresent
				return (tk2DIntegration.PlayAnimation (sprite, clipName, false, WrapMode.Once, frame));
			#else
				Debug.Log ("The 'tk2DIsPresent' preprocessor is not defined - check your Build Settings.");
				return true;
			#endif
		}
		
		
		public static bool PlayAnimation (Transform sprite, string clipName, bool changeWrapMode, WrapMode wrapMode, int frame)
		{
			#if tk2DIsPresent
			
			tk2dSpriteAnimationClip.WrapMode wrapMode2D = tk2dSpriteAnimationClip.WrapMode.Once;
			if (wrapMode == WrapMode.Loop)
			{
				wrapMode2D = tk2dSpriteAnimationClip.WrapMode.Loop;
			}
			else if (wrapMode == WrapMode.PingPong)
			{
				wrapMode2D = tk2dSpriteAnimationClip.WrapMode.PingPong;
			}
			
			if (sprite && sprite.GetComponent <tk2dSpriteAnimator>())
			{
				tk2dSpriteAnimator anim = sprite.GetComponent <tk2dSpriteAnimator>();
				tk2dSpriteAnimationClip clip = anim.GetClipByName (clipName);

				if (clip != null)
				{
					if (!anim.IsPlaying (clip))
					{
						if (changeWrapMode)
						{
							clip.wrapMode = wrapMode2D;
						}
						
				    	anim.Play (clip);

						if (frame >= 0)
						{
							anim.SetFrame (frame);
							anim.Stop ();
						}
					}
					
					return true;
				}

				return false;	
			}
			
			#else
			Debug.Log ("The 'tk2DIsPresent' preprocessor is not defined - check your Build Settings.");
			#endif
			
			return true;
		}


		public static bool PlayAnimation (Transform sprite, string clipName, bool changeWrapMode, WrapMode wrapMode)
		{
			#if tk2DIsPresent
			PlayAnimation (sprite, clipName, changeWrapMode, wrapMode, -1);
			#else
			Debug.Log ("The 'tk2DIsPresent' preprocessor is not defined - check your Build Settings.");
			#endif
			return true;
		}
		
		
		public static void StopAnimation (Transform sprite)
		{
			#if tk2DIsPresent
			if (sprite && sprite.GetComponent <tk2dSpriteAnimator>())
			{
				tk2dSpriteAnimator anim = sprite.GetComponent <tk2dSpriteAnimator>();

		    	anim.Stop ();
			}
			#else
			Debug.Log ("The 'tk2DIsPresent' preprocessor is not defined - check your Build Settings.");
			#endif
		}
		
		
		public static bool IsAnimationPlaying (Transform sprite, string clipName)
		{
			#if tk2DIsPresent
			tk2dSpriteAnimator anim = sprite.GetComponent <tk2dSpriteAnimator>();
			tk2dSpriteAnimationClip clip = anim.GetClipByName (clipName);
			
			if (clip != null)
			{
				if (anim.IsPlaying (clip))
				{
					return true;
				}
			}
			#endif
			return false;
		}

	}

}