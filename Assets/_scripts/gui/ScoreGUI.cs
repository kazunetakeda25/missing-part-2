using UnityEngine;
using System;
using System.Collections;

public class ScoreGUI : MonoBehaviour 
{
	public dfPanel myPanel;
	public dfLabel scoreNumberLabel;

	private int previousScore = 0;

	private void Awake()
	{
		CheckGUI();
		previousScore = SessionManager.Instance.GetScore();;
	}

	private void CheckGUI()
	{
		Debug.Log("OnLevelWasLoaded");
		if(Settings.Instance.Scoring == false) {
			Debug.Log("Scoring is Off!");
			HideScore();
			return;
		}
	}

	public void HideScore() { myPanel.IsVisible = false; }
	public void ShowScore() {
		myPanel.IsVisible = true;
	}

	private void Update()
	{
		if(myPanel.IsVisible == true)
			UpdateScore();
	}

	private void UpdateScore()
	{
		if(Settings.Instance.Scoring == false)
			return;

		int currentScore = SessionManager.Instance.GetScore();

		if(currentScore != previousScore)
		{
			previousScore = currentScore;
			this.GetComponent<AudioSource>().Play();
		}

		string formattedString = String.Format("{0:#,00}", currentScore);

		if(Debug.isDebugBuild)
			formattedString = AppendDebugText();

		scoreNumberLabel.Text = formattedString;
	}
	 
	private string AppendDebugText()
	{
		string retString = "";

		BadgeTracker.Badge badge = BadgeTracker.Badge.NONE;

		switch(Application.loadedLevelName) {
		case "NEWAAR1":
			badge = BadgeTracker.Badge.EP1;
			break;
		case "NEWAAR2":
			badge = BadgeTracker.Badge.EP2;
			break;
		case "NEWAAR3":
			badge = BadgeTracker.Badge.EP3;
			break;
		}

		BadgeTracker.BadgeScore epScore = BadgeTracker.Instance.GetBadgeScore(badge);

		retString += (epScore.correctAnswers * 100).ToString();

		return retString;
	}

}
