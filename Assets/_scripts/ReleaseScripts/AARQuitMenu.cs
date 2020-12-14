using UnityEngine;
using System.Collections;


namespace MissingComplete 
{
	public class AARQuitMenu : MonoBehaviour 
	{
		private static AARQuitMenu instance;
		public static AARQuitMenu Instance { get { return instance; } }

		[SerializeField] GameObject overlay;
		[SerializeField] UICamera aarUICamera;
		[SerializeField] AudioSource aarSource;
		[SerializeField] Camera fireworksCamera;
		[SerializeField] GameObject scoreGO;

		private bool toggledOn = false;
		public bool IsPaused { get { return toggledOn; } }
		private MoviePlayer moviePlaying;

		public void OnQuitHit()
		{
			Debug.Log("On Quit");
			SaveGameManager.Instance.UnloadSavedGame();
			UnityEngine.SceneManagement.SceneManager.LoadScene("MAIN_MENU");
		}

		private void Awake()
		{
			instance = this;
		}

		private void Update()
		{
			if(Input.GetKeyUp(KeyCode.Escape)) {
				if(toggledOn == false) {
					Pause();
				} else {
					UnPause();
				}
			}
		}

		private void Pause()
		{
			toggledOn = true;

			moviePlaying = GameObject.FindObjectOfType<MoviePlayer>();

			if(moviePlaying != null) {
				moviePlaying.PauseMovie();
			}

			if(aarSource.time > 0) {
				aarSource.Pause();
			}

			fireworksCamera.enabled = false;

			overlay.SetActive(true);
			aarUICamera.enabled = false;

			scoreGO.SetActive(false);
		}

		private void UnPause()
		{
			toggledOn = false;

			if(moviePlaying != null) {
				moviePlaying.PlayMovie();
				moviePlaying = null;
			}

			overlay.SetActive(false);
			aarUICamera.enabled = true;

			fireworksCamera.enabled = true;

			if(aarSource.time > 0) {
				aarSource.Play();
			}

			scoreGO.SetActive(true);
		}
	}
}
