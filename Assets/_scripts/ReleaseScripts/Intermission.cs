using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

namespace MissingComplete
{
	public class Intermission : MonoBehaviour 
	{
		private const float FADE_TIME = 1.0f;

		[SerializeField] Fader fader;
		[SerializeField] Button nextButton;
		[SerializeField] Text nextButtonText;
		[SerializeField] AudioSource intermissionAudio;

		[SerializeField] Button quitButton;
		[SerializeField] Text quitText;

		[SerializeField] float delayForNextButton;
		[SerializeField] int checkpointOnNext;

		private Tweener buttonTween;
		private Tweener textTween;

		public void OnNextHit()
		{
			intermissionAudio.DOFade(0.0f, 3.0f);
			fader.FadeOut();
			fader.fadeOutComplete += LoadNext;
			SaveGameManager.Instance.GetCurrentSaveGame().checkPoint = checkpointOnNext;
			SaveGameManager.Instance.SaveCurrentGame();
		}

		public void OnQuitHit()
		{
			
			intermissionAudio.DOFade(0.0f, 3.0f);
			fader.FadeOut();
			fader.fadeOutComplete += QuitToMainMenu;
		}

		private void QuitToMainMenu()
		{
			Debug.Log("Q");
			fader.fadeOutComplete -= QuitToMainMenu;
			SaveGameManager.Instance.UnloadSavedGame();
			UnityEngine.SceneManagement.SceneManager.LoadScene("MAIN_MENU");
		}

		private void LoadNext()
		{
			fader.fadeOutComplete -= LoadNext;
			SessionManager.Instance.GotoNextLevel();
		}

		private void ShowNextButton()
		{
			nextButton.gameObject.SetActive(true);
			textTween = nextButtonText.DOFade(1.0f, FADE_TIME);
			textTween.SetDelay(delayForNextButton);
			buttonTween = nextButton.image.DOFade(1.0f, FADE_TIME);
			buttonTween.SetDelay(delayForNextButton);
			buttonTween.OnStart(() => nextButton.interactable = true);

			quitButton.gameObject.SetActive(true);
			textTween = quitText.DOFade(1.0f, FADE_TIME);
			textTween.SetDelay(delayForNextButton);
			buttonTween = quitButton.image.DOFade(1.0f, FADE_TIME);
			buttonTween.SetDelay(delayForNextButton);
			buttonTween.OnStart(() => quitButton.interactable = true);
		}

		private void Start()
		{
			ShowNextButton();
			fader.FadeIn();
			GameLoader.Instance.GetLoadedCheckpoint();
		}

		private void Update()
		{
			if(Debug.isDebugBuild) {
				if(Input.GetMouseButtonUp(0)) {
					nextButton.gameObject.SetActive(true);
					nextButton.image.color = new Color(nextButton.image.color.r, nextButton.image.color.g, nextButton.image.color.b, 1.0f);
					nextButtonText.color = new Color(nextButtonText.color.r, nextButtonText.color.g, nextButtonText.color.b, 1.0f);
					nextButton.interactable = true;

					quitButton.gameObject.SetActive(true);
					quitButton.image.color = new Color(quitButton.image.color.r, quitButton.image.color.g, quitButton.image.color.b, 1.0f);
					quitText.color = new Color(quitText.color.r, quitText.color.g, quitText.color.b, 1.0f);
					quitButton.interactable = true;
				}
			}
		}
	}
}
