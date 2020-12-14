using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using DG.Tweening;

namespace MissingComplete
{
	public class Fader : MonoBehaviour 
	{
		public delegate void OnFadeOutEvent();
		public delegate void OnFadeInEvent();
		public event OnFadeOutEvent fadeOutComplete;
		public event OnFadeInEvent fadeInComplete;

		private static Fader instance;
		public static Fader Instance { get { return instance; } }

		[SerializeField] Image fadeImage;
		[SerializeField] float fadeTime;

		public void FadeIn()
		{
			Debug.Log("Starting Fade In");
			fadeImage.gameObject.SetActive(true);
			fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, 1.0f);
			fadeImage.DOFade(0.0f, fadeTime).OnComplete(OnFadeInComplete).SetEase(Ease.Linear);
		}

		private void OnFadeInComplete()
		{
			Debug.Log("OnFadeInComplete");
			fadeImage.gameObject.SetActive(false);
			if(fadeInComplete != null) {
				fadeInComplete();
			}
		}

		public void FadeOut()
		{
			Debug.Log("Starting Fade Out");
			fadeImage.gameObject.SetActive(true);
			fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, 0.0f);
			fadeImage.DOFade(1.0f, fadeTime).OnComplete(OnFadeOutComplete).SetEase(Ease.Linear);
		}

		private void OnFadeOutComplete()
		{
			Debug.Log("OnFadeOutComplete");
			if(fadeOutComplete != null) {
				fadeOutComplete();
			}
		}

		private void Awake()
		{
			instance = this;
		}
	}
}

