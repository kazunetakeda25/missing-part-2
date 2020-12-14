using UnityEngine;
using UnityEngine.UI;

namespace MissingComplete
{
	[RequireComponent(typeof(AudioSource))]
	public class SoundAdjuster : MonoBehaviour 
	{
		[SerializeField] Button doneButton;
		[SerializeField] Slider slider;
		[SerializeField] CanvasGroup canvas;
		private bool loopSound = false;

		public void OnSliderAdjusted()
		{
			if(doneButton.interactable == false) {
				doneButton.interactable = true;
			}

			SetVolume();
			loopSound = true;
		}

		public void OnMosueDown()
		{
			this.GetComponent<AudioSource>().Play();
		}

		public void OnMouseReleased()
		{
			loopSound = false;
		}

		public void OnDoneButton()
		{
			Debug.Log("Done Button Hit");
			Fader.Instance.FadeOut();
			Fader.Instance.fadeOutComplete += LoadMenu;
		}

		private void LoadMenu()
		{
			UnityEngine.SceneManagement.SceneManager.LoadScene("MAIN_MENU");
		}

		private void SetVolume()
		{
			float newVolumeLevel = slider.value;
			newVolumeLevel += 0.10f;

			AudioListener.volume = newVolumeLevel;
		}

		private void Start()
		{
			Fader.Instance.FadeIn();
			Fader.Instance.fadeInComplete += ShowBar;
		}

		private void ShowBar()
		{
			Fader.Instance.fadeInComplete -= ShowBar;
			slider.gameObject.SetActive(true);
		}

		private void Update()
		{
			if(loopSound == true) {
				if(this.GetComponent<AudioSource>().isPlaying == false) {
					this.GetComponent<AudioSource>().Play();
				}
			}
		}

		private void OnApplicationPause(bool paused)
		{
			this.GetComponent<AudioSource>().Stop();
		}

		private void OnApplicationFocus(bool focus)
		{
			this.GetComponent<AudioSource>().Stop();
		}

	}
}

