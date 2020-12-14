/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"ActionMovie.cs"
 * 
 *	Plays movie clips either on a Texture, or full-screen on mobile devices.
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
	public class ActionMovie : Action
	{
		
		#if !(UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_WEBGL)
		public Material material;
		public MovieTexture movieClip;
		public Sound sound;
		public bool includeAudio;
		public string skipKey;
		#endif
		public bool canSkip;

		public string filePath;

		
		public ActionMovie ()
		{
			this.isDisplayed = true;
			title = "Play movie clip";
			category = ActionCategory.Engine;
			description = "Plays movie clips either on a Texture, or full-screen on mobile devices.";
		}
		
		
		override public float Run ()
		{
			#if UNITY_WEBGL

			return 0f;

			#elif UNITY_IOS || UNITY_ANDROID || UNITY_WP8

			if (!isRunning && filePath != "")
			{
				isRunning = true;

				if (canSkip)
				{
					Handheld.PlayFullScreenMovie (filePath, Color.black, FullScreenMovieControlMode.CancelOnInput);
				}
				else
				{
					Handheld.PlayFullScreenMovie (filePath, Color.black, FullScreenMovieControlMode.Full);
				}
				return defaultPauseTime;
			}
			else
			{
				isRunning = false;
				return 0f;
			}

			#elif UNITY_5 || UNITY_PRO

			if (movieClip == null)
			{
				Debug.LogWarning ("No movie clip set");
				return 0f;
			}
			if (material == null)
			{
				Debug.LogWarning ("No material set");
				return 0f;
			}
			if (includeAudio && sound == null)
			{
				Debug.LogWarning ("No sound set");
			}

			if (!isRunning)
			{
				isRunning = true;
				KickStarter.playerInput.skipMovieKey = "";

				material.mainTexture = movieClip;
				movieClip.Play ();

				if (includeAudio)
				{
					sound.GetComponent <AudioSource>().clip = movieClip.audioClip;
					sound.Play (false);
				}

				if (willWait)
				{
					if (canSkip && skipKey != "")
					{
						KickStarter.playerInput.skipMovieKey = skipKey;
						return defaultPauseTime;
					}
					return movieClip.duration;
				}
				return 0f;
			}
			else
			{
				if (canSkip && movieClip.isPlaying)
				{
					if (KickStarter.playerInput.skipMovieKey != "")
					{
						return defaultPauseTime;
					}
				}

				if (includeAudio)
				{
					sound.Stop ();
				}
				movieClip.Stop ();
				isRunning = false;
				KickStarter.playerInput.skipMovieKey = "";
				return 0f;
			}
			#else
			Debug.LogWarning ("On non-mobile platforms, this Action is only available in Unity 5 or Unity Pro.");
			return 0f;
			#endif
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI ()
		{
			#if UNITY_WEBGL

			EditorGUILayout.HelpBox ("This Action is not available on the WebGL platform.", MessageType.Info);

			#elif UNITY_IOS || UNITY_ANDROID || UNITY_WP8

			filePath = EditorGUILayout.TextField ("Path to clip file:", filePath);
			canSkip = EditorGUILayout.Toggle ("Player can skip?", canSkip);

			#elif UNITY_5 || UNITY_PRO

			movieClip = (MovieTexture) EditorGUILayout.ObjectField ("Movie clip filename:", movieClip, typeof (MovieTexture), false);
			EditorGUILayout.HelpBox ("The clip must be placed in a folder named 'SharedAssets'.", MessageType.Info);
			material = (Material) EditorGUILayout.ObjectField ("Play on material:", material, typeof (Material), true);
			includeAudio = EditorGUILayout.Toggle ("Include audio?", includeAudio);
			if (includeAudio)
			{
				sound = (Sound) EditorGUILayout.ObjectField ("Audio source:", sound, typeof (Sound), true);
			}

			willWait = EditorGUILayout.Toggle ("Wait until finish?", willWait);
			if (willWait)
			{
				canSkip = EditorGUILayout.Toggle ("Player can skip?", canSkip);
				if (canSkip)
				{
					skipKey = EditorGUILayout.TextField ("Skip with Input Button:", skipKey);
				}
			}

			#else
			EditorGUILayout.HelpBox ("On non-mobile platforms, this Action is only available in Unity 5 or Unity Pro.", MessageType.Warning);
			#endif

			AfterRunningOption ();
		}
		
		
		public override string SetLabel ()
		{
			#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8
			if (filePath != "")
			{
				return " (" + filePath + ")";
			}
			#elif !UNITY_WEBGL
			if (movieClip)
			{
				return " (" + movieClip.name + ")";
			}
			#endif
			return "";
		}
		
		#endif
		
	}
	
}