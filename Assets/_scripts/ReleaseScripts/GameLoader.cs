using UnityEngine;
using System.Collections;

namespace MissingComplete
{
	public class GameLoader 
	{
		private static GameLoader instance;
		private GameLoader() { }
		public static GameLoader Instance 
		{ 
			get 
			{ 
				if(instance == null) {
					instance = new GameLoader();
				}

				return instance; 
			} 
		}

		private bool isGameLoaded = false;
		public bool IsGameLoaded { get { return isGameLoaded; } }
		private int checkpointLoaded = -1;

		private bool faderSet = false;

		public int GetLoadedCheckpoint()
		{
			if(checkpointLoaded == -1) {
				Debug.LogError("No Checkpoint Loaded!!");
			}

			int checkPoint = checkpointLoaded;
			checkpointLoaded = -1;
			isGameLoaded = false;
			return checkPoint;
		}

		public void Load(int checkPoint)
		{
			faderSet = false;

			if(CheckCheckpointValid(checkPoint) == false) {
				Debug.LogError("Invalid Checkpoint Passed! Over max allowed.");
				return;
			}

			Debug.Log("Loading Game...");

			this.checkpointLoaded = checkPoint;

			if(Fader.Instance != null) {
				Fader.Instance.FadeOut();
				Fader.Instance.fadeOutComplete += LoadGame;
				faderSet = true;
				return;
			}

			LoadGame();
		}

		private bool CheckCheckpointValid(int checkPoint)
		{
			if(checkPoint > SaveGameManager.SaveGame.TOTAL_CHECKPOINTS) {
				return false;
			}

			return true;
		}

		private void LoadGame()
		{
			if(faderSet == true) {
				Fader.Instance.fadeOutComplete -= LoadGame;
			}

			isGameLoaded = true;

			switch(checkpointLoaded) {
			case 1:
					UnityEngine.SceneManagement.SceneManager.LoadScene("INTRO_VIDEO");
				SessionManager.Instance.SetStartScene(Episode.INTRO_VIDEO);
				break;
			case 3:
					UnityEngine.SceneManagement.SceneManager.LoadScene("ORIENTATION");
				SessionManager.Instance.SetStartScene(Episode.ORIENTATION);
				break;
			case 10:
			case 11:
			case 12:
			case 13:
			case 14:
					UnityEngine.SceneManagement.SceneManager.LoadScene("TERRYS_APARTMENT");
				SessionManager.Instance.SetStartScene(Episode.TERRYS_APARTMENT);
				break;
			case 20:
					UnityEngine.SceneManagement.SceneManager.LoadScene("NEWAAR1");
				SessionManager.Instance.SetStartScene(Episode.NEWAAR1);
				break;
			case 29:
					UnityEngine.SceneManagement.SceneManager.LoadScene("ORIENTATION2");
				SessionManager.Instance.SetStartScene(Episode.ORIENTATION2);
				break;
			case 30:
			case 31:
			case 32:
			case 33:
					UnityEngine.SceneManagement.SceneManager.LoadScene("THUNDERJAW");
				SessionManager.Instance.SetStartScene(Episode.THUNDERJAW);
				break;
			case 40:
					UnityEngine.SceneManagement.SceneManager.LoadScene("NEWAAR2");
				SessionManager.Instance.SetStartScene(Episode.NEWAAR2);
				break;
			case 49:
					UnityEngine.SceneManagement.SceneManager.LoadScene("ORIENTATION3");
				SessionManager.Instance.SetStartScene(Episode.ORIENTATION3);
				break;
			case 50:
			case 51:
			case 52:
			case 53:
			case 54:
					UnityEngine.SceneManagement.SceneManager.LoadScene("WAREHOUSE");
				SessionManager.Instance.SetStartScene(Episode.WAREHOUSE);
				break;
			case 60:
					UnityEngine.SceneManagement.SceneManager.LoadScene("NEWAAR3");
				SessionManager.Instance.SetStartScene(Episode.NEWAAR3);
				break;
			case 69:
					UnityEngine.SceneManagement.SceneManager.LoadScene("CONCLUSION");
				SessionManager.Instance.SetStartScene(Episode.CONCLUSION);
				break;
			default:
				Debug.LogWarning("Invalid Checkpoint!");
				break;
			}
		}
	}

}
