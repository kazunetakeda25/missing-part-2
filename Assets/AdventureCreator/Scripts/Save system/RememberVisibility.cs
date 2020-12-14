/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"RememberVisibility.cs"
 * 
 *	This script is attached to scene objects
 *	whose renderer.enabled state we wish to save.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	public class RememberVisibility : Remember
	{
		
		public AC_OnOff startState = AC_OnOff.On;
		public bool affectChildren = false;

		
		public void Awake ()
		{
			if (GameIsPlaying ())
			{
				bool state = false;
				if (startState == AC_OnOff.On)
				{
					state = true;
				}

				if (GetComponent <Renderer>())
				{
					GetComponent <Renderer>().enabled = state;
				}

				if (affectChildren)
				{
					foreach (Renderer _renderer in GetComponentsInChildren <Renderer>())
					{
						_renderer.enabled = state;
					}
				}
			}
		}


		public override string SaveData ()
		{
			VisibilityData visibilityData = new VisibilityData ();
			visibilityData.objectID = constantID;

			if (GetComponent <SpriteFader>())
			{
				SpriteFader spriteFader = GetComponent <SpriteFader>();
				visibilityData.isFading = spriteFader.isFading;
				if (spriteFader.isFading)
				{
					if (spriteFader.fadeType == FadeType.fadeIn)
					{
						visibilityData.isFadingIn = true;
					}
					else
					{
						visibilityData.isFadingIn = false;
					}

					visibilityData.fadeTime = spriteFader.fadeTime;
					visibilityData.fadeStartTime = spriteFader.fadeStartTime;
				}
				visibilityData.fadeAlpha = GetComponent <SpriteRenderer>().color.a;
			}
			
			if (GetComponent <Renderer>())
			{
				visibilityData.isOn = GetComponent <Renderer>().enabled;
			}
			else if (affectChildren)
			{
				foreach (Renderer _renderer in GetComponentsInChildren <Renderer>())
				{
					visibilityData.isOn = _renderer.enabled;
					break;
				}
			}
			
			return Serializer.SaveScriptData <VisibilityData> (visibilityData);
		}
		
		
		public override void LoadData (string stringData)
		{
			VisibilityData data = Serializer.LoadScriptData <VisibilityData> (stringData);
			if (data == null) return;

			if (GetComponent <SpriteFader>())
			{
				SpriteFader spriteFader = GetComponent <SpriteFader>();
				if (data.isFading)
				{
					if (data.isFadingIn)
					{
						spriteFader.Fade (FadeType.fadeIn, data.fadeTime, data.fadeAlpha);
					}
					else
					{
						spriteFader.Fade (FadeType.fadeOut, data.fadeTime, data.fadeAlpha);
					}
				}
				else
				{
					spriteFader.EndFade ();
					spriteFader.SetAlpha (data.fadeAlpha);
				}
			}
			
			if (GetComponent <Renderer>())
			{
				GetComponent <Renderer>().enabled = data.isOn;
			}

			if (affectChildren)
			{
				foreach (Renderer _renderer in GetComponentsInChildren <Renderer>())
				{
					_renderer.enabled = data.isOn;
				}
			}
		}
		
	}


	[System.Serializable]
	public class VisibilityData : RememberData
	{
		public bool isOn;
		public bool isFading;
		public bool isFadingIn;
		public float fadeTime;
		public float fadeStartTime;
		public float fadeAlpha;

		public VisibilityData () { }
	}

}