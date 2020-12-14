using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

namespace MissingComplete
{
	public class MainMenu : MonoBehaviour 
	{
		private static MainMenu instance;
		public static MainMenu Instance { get { return instance; } }

		[SerializeField] float fadeTime;
		[SerializeField] Image[] menuImages;
		[SerializeField] GameObject rootMainMenu;

		public void OnQuitButton(Button button)
		{
			Debug.Log("On Quit Button");
			MenuSoundBox.Instance.PlayClick(button);
			FadeOutMenu();
			Invoke("Quit", fadeTime);
		}

		public void FadeOutMenu()
		{
			Debug.Log("Fade Out...");
			foreach(Image img in menuImages) {
				img.color = ColorTools.GetAlphaColorChange(img.color, 1f);
				Tweener tween = img.DOFade(0f, fadeTime);
				tween.SetEase(Ease.Linear);
				tween.OnComplete(() => SetMenu(false));
			}
		}

		public void FadeInMenu()
		{
			Debug.Log("Fade In...");
			rootMainMenu.SetActive(true);

			foreach(Image img in menuImages) {
				img.color = ColorTools.GetAlphaColorChange(img.color, 0f);
				Tweener tween = img.DOFade(1f, fadeTime);
				tween.SetEase(Ease.Linear);
			}
		}

		private void SetMenu(bool active)
		{
			rootMainMenu.SetActive(active);
		}

		private void Awake()
		{
			instance = this;

			if(AC.KickStarter.stateHandler != null) {
				GameObject.Destroy(AC.KickStarter.stateHandler.gameObject);
			}

			if(GameObject.FindGameObjectWithTag("Player") != null) {
				GameObject.Destroy(GameObject.FindGameObjectWithTag("Player"));
			}

			Time.timeScale = 1.0f;
			AudioListener.pause = false;

			SetMenu(false);
		}

		private void Quit()
		{
			Debug.Log("Exiting App");
			Application.Quit();
		}

	}
}

