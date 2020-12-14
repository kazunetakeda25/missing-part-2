/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"ActionSound.cs"
 * 
 *	This action triggers the sound component of any GameObject, overriding that object's play settings.
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
	public class ActionSound : Action
	{

		public int constantID = 0;
		public int parameterID = -1;
		public Sound soundObject;

		public AudioClip audioClip;
		public enum SoundAction { Play, FadeIn, FadeOut, Stop }
		public float fadeTime;
		public bool loop;
		public bool ignoreIfPlaying = false;
		public SoundAction soundAction;
		public bool affectChildren = false;
		
		
		public ActionSound ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Sound;
			title = "Play";
			description = "Triggers a Sound object to start playing. Can be used to fade sounds in or out.";
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			soundObject = AssignFile <Sound> (parameters, parameterID, constantID, soundObject);
		}

		
		override public float Run ()
		{
			if (soundObject)
			{
				if ((audioClip != null && soundObject.IsPlaying (audioClip)) || (audioClip == null && soundObject.IsPlaying ()))
				{
					// Sound object is already playing the desired clip
					if (ignoreIfPlaying && (soundAction == SoundAction.Play || soundAction == SoundAction.FadeIn))
					{
						return 0f;
					}
				}

				if (audioClip && soundObject.GetComponent <AudioSource>())
				{
					if (soundAction == SoundAction.Play || soundAction == SoundAction.FadeIn)
					{
						soundObject.GetComponent <AudioSource>().clip = audioClip;
					}
				}

				if (soundObject.soundType == SoundType.Music && (soundAction == SoundAction.Play || soundAction == SoundAction.FadeIn))
				{
					Sound[] sounds = FindObjectsOfType (typeof (Sound)) as Sound[];
					foreach (Sound sound in sounds)
					{
						sound.EndOldMusic (soundObject);
					}
				}

				if (soundAction == SoundAction.Play)
				{
                    soundObject.Play (loop);
				}
				else if (soundAction == SoundAction.FadeIn)
				{
					if (fadeTime == 0f)
					{
						soundObject.Play (loop);
					}
					else
					{
						soundObject.FadeIn (fadeTime, loop);
					}
				}
				else if (soundAction == SoundAction.FadeOut)
				{
					if (fadeTime == 0f)
					{
						soundObject.Stop ();
					}
					else
					{
						soundObject.FadeOut (fadeTime);
					}
				}
				else if (soundAction == SoundAction.Stop)
				{
					soundObject.Stop ();

					if (affectChildren)
					{
						foreach (Transform child in soundObject.transform)
						{
							if (child.GetComponent <Sound>())
							{
								child.GetComponent <Sound>().Stop ();
							}
						}
					}
				}
			}
			
			return 0f;
		}


		override public void Skip ()
		{
			if (soundAction == SoundAction.FadeOut || soundAction == SoundAction.Stop)
			{
				Run ();
			}
			else if (loop)
			{
				Run ();
			}
		}
		
		
		#if UNITY_EDITOR

		override public void ShowGUI (List<ActionParameter> parameters)
		{
			parameterID = Action.ChooseParameterGUI ("Sound object:", parameters, parameterID, ParameterType.GameObject);
			if (parameterID >= 0)
			{
				constantID = 0;
				soundObject = null;
			}
			else
			{
				soundObject = (Sound) EditorGUILayout.ObjectField ("Sound object:", soundObject, typeof(Sound), true);
				
				constantID = FieldToID <Sound> (soundObject, constantID);
				soundObject = IDToField <Sound> (soundObject, constantID, false);
			}

			soundAction = (SoundAction) EditorGUILayout.EnumPopup ("Sound action:", (SoundAction) soundAction);
			
			if (soundAction == SoundAction.Play || soundAction == SoundAction.FadeIn)
			{
				loop = EditorGUILayout.Toggle ("Loop?", loop);
				ignoreIfPlaying = EditorGUILayout.Toggle ("Ignore if already playing?", ignoreIfPlaying);
				audioClip = (AudioClip) EditorGUILayout.ObjectField ("New clip (optional)", audioClip, typeof (AudioClip), false);
			}
			
			if (soundAction == SoundAction.FadeIn || soundAction == SoundAction.FadeOut)
			{
				fadeTime = EditorGUILayout.Slider ("Fade time:", fadeTime, 0f, 10f);
			}

			if (soundAction == SoundAction.Stop)
			{
				affectChildren = EditorGUILayout.Toggle ("Stop child Sounds, too?", affectChildren);
			}

			AfterRunningOption ();
		}
		
		
		override public string SetLabel ()
		{
			string labelAdd = "";
			if (soundObject)
			{
				labelAdd = " (" + soundAction.ToString ();
				labelAdd += " " + soundObject.name + ")";
			}
			
			return labelAdd;
		}

		#endif

	}

}