/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"SpriteFader.cs"
 * 
 *	Attachthis to any sprite you wish to fade.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	[RequireComponent (typeof (SpriteRenderer))]
	public class SpriteFader : MonoBehaviour
	{

		[HideInInspector] public bool isFading = true;
		[HideInInspector] public float fadeStartTime;
		[HideInInspector] public float fadeTime;
		[HideInInspector] public FadeType fadeType;

		private SpriteRenderer spriteRenderer;


		private void Awake ()
		{
			spriteRenderer = GetComponent <SpriteRenderer>();
		}


		public void SetAlpha (float _alpha)
		{
			Color color = GetComponent <SpriteRenderer>().color;
			color.a = _alpha;
			GetComponent <SpriteRenderer>().color = color;
		}


		public void Fade (FadeType _fadeType, float _fadeTime, float startAlpha = -1)
		{
			StopCoroutine ("DoFade");

			float currentAlpha = spriteRenderer.color.a;

			if (startAlpha >= 0)
			{
				currentAlpha = startAlpha;
				SetAlpha (startAlpha);
			}

			if (_fadeType == FadeType.fadeOut)
			{
				fadeStartTime = Time.time - (currentAlpha * _fadeTime);
			}
			else
			{
				fadeStartTime = Time.time - ((1f - currentAlpha) * _fadeTime);
			}
		
			fadeTime = _fadeTime;
			fadeType = _fadeType;

			StartCoroutine ("DoFade");
		}


		public void EndFade ()
		{
			StopCoroutine ("DoFade");
			isFading = false;
			Color color = spriteRenderer.color;
			if (fadeType == FadeType.fadeIn)
			{
				color.a = 1f;
			}
			else
			{
				color.a = 0f;
			}
			spriteRenderer.color = color;
		}


		private IEnumerator DoFade ()
		{
			spriteRenderer.enabled = true;
			isFading = true;
			Color color = spriteRenderer.color;
			if (fadeType == FadeType.fadeIn)
			{
				while (color.a < 1f)
				{
					color.a = -1f + AdvGame.Interpolate (fadeStartTime, fadeTime, MoveMethod.Linear, null);
					spriteRenderer.color = color;
					yield return new WaitForFixedUpdate ();
				}
			}
			else
			{
				while (color.a > 0f)
				{
					color.a = 2f - AdvGame.Interpolate (fadeStartTime, fadeTime, MoveMethod.Linear, null);
					spriteRenderer.color = color;
					yield return new WaitForFixedUpdate ();
				}
			}
			isFading = false;
		}

	}

}