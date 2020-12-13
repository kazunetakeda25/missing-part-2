using UnityEngine;
using System;
using System.Collections;
using MissingComplete;

public class FinalScreen : MonoBehaviour {

	public dfLabel timerLabel;
	public dfLabel scoreLabel;

	public void OnQuit()
	{
		SaveGameManager sgm = SaveGameManager.Instance;
		if(sgm != null) {
			sgm.UnloadSavedGame();
		}

		UnityEngine.SceneManagement.SceneManager.LoadScene("MAIN_MENU");
	}

	private void Start()
	{
		if(Settings.Instance.Scoring) {
			string score = "SCORE: ";
			score += SessionManager.Instance.GetScore().ToString();
			scoreLabel.Text = score;
		} else {
			GameObject.Destroy(scoreLabel);
		}

		if(SaveGameManager.Instance != null) {
			SaveGameManager.SaveGame currentSave = SaveGameManager.Instance.GetCurrentSaveGame();
			if(currentSave.gameCompleted == false) {
				currentSave.gameCompleted = true;
				currentSave.dateCompleted = DateTime.Now;
				SaveGameManager.Instance.SaveCurrentGame();
			}
		}
	}

	private void Update()
	{
		if(SaveGameManager.Instance == null)
			return;

		TimeSpan time = TimeSpan.FromSeconds(SaveGameManager.Instance.GetCurrentSaveGame().playTime);

		string answer = string.Format("{0:D2}:{1:D2}:{2:D2}", 
		                              time.Hours, 
		                              time.Minutes, 
		                              time.Seconds);

		timerLabel.Text = answer;
	}

}
