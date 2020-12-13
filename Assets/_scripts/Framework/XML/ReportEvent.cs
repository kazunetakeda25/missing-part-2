using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ReportEvent : MonoBehaviour {
	
	public enum Event
	{
		PLAYER_INFO,
		EPISODE_STARTED,
		EPISODE_COMPLETE,
		DISPLAYING_CHOICE,
		PLAYER_CHOICE,
		NEXT_BUTTON_DISPLAYED,
		NEXT_BUTTON_PRESSED,
		PLAYER_SCORED,
		QUIZ_MULTIPLE_CHOICE,
		QUIZ_USER_INPUT,
		AAR_STARTED,
		AAR_COMPLETED,
		NEXT_BUTTON_CLICKED,
		GAME_PAUSED,
		GAME_RESUMED
	}
	
	public static void ReportPlayerInfo(Sex subjectGender, string sessionName) 
	{
		//Save TimeStamp
		SessionDataManager.Instance.StartSessionTimer();
		
		Dictionary<string, string> values = new Dictionary<string, string>();
		
		values.Add("subjectGender", subjectGender.ToString());
		values.Add("sessionName", sessionName);
		string timeStr = "UTC: " + System.DateTime.Now.ToUniversalTime().ToString( "o" );
		values.Add("UTCTimestamp", timeStr);
		values.Add("Fidelity: ", Settings.Instance.Fidelity.ToString());
		values.Add("Priming: ", Settings.Instance.Priming.ToString());
		values.Add("Scoring: ", Settings.Instance.Scoring.ToString());
		
		WriteSessionXML.WriteToXML(Event.PLAYER_INFO.ToString(), values);
	}

	public static void ReportMedalEarned(BadgeTracker.BadgeScore score)
	{
		Dictionary<string, string> values = new Dictionary<string, string>();

		values.Add("BadgeID", score.badge.ToString());
		values.Add("CorrectBadgeAnswers", score.correctAnswers.ToString());
		values.Add("TotalBadgeQuizzes", score.totalAnswers.ToString());
		values.Add("MedianCalculated", score.CalculatedMedian.ToString());
		values.Add("BadgeAwarded", BadgeTracker.GetMedalString(score));

		WriteSessionXML.WriteToXML(Event.PLAYER_INFO.ToString(), values);
	}
	
	public static void EpisodeStarted(Episode episode)
	{
		Dictionary<string, string> values = new Dictionary<string, string>();
		
		values.Add("Episode", episode.ToString());
		
		WriteSessionXML.WriteToXML(Event.EPISODE_STARTED.ToString(), values);
	}
	
	public static void EpisodeCompleted(Episode episode)
	{
		Dictionary<string, string> values = new Dictionary<string, string>();
		
		values.Add("Episode", episode.ToString());
		
		WriteSessionXML.WriteToXML(Event.EPISODE_COMPLETE.ToString(), values);
	}

	public static void ReportUserInputQuiz(string quiz, float val)
	{
		Dictionary<string, string> values = new Dictionary<string, string>();
		
		values.Add("Quiz", quiz);
		values.Add("Value", val.ToString());
		
		WriteSessionXML.WriteToXML(Event.QUIZ_USER_INPUT.ToString(), values);
	}

	public static void DisplayingPlayerPrompt(string[] choices)
	{
		Dictionary<string, string> values = new Dictionary<string, string>();

		for (int i = 0; i < choices.Length; i++) {
			values.Add ("Choice" + i.ToString(), choices[i]);
		}

		WriteSessionXML.WriteToXML(Event.DISPLAYING_CHOICE.ToString(), values);
	}

	public static void PlayerSelectedChoice(string choice, int index)
	{
		Dictionary<string, string> values = new Dictionary<string, string>();

		values.Add ("index", index.ToString());
		values.Add ("Choice", choice);
		
		WriteSessionXML.WriteToXML(Event.PLAYER_CHOICE.ToString(), values);
	}

	public static void NextButtonDisplayed(int slideIndex)
	{
		Dictionary<string, string> values = new Dictionary<string, string>();
		
		values.Add ("SlideIndex", slideIndex.ToString());

		WriteSessionXML.WriteToXML(Event.NEXT_BUTTON_DISPLAYED.ToString(), values);
	}

	public static void NextButtonClicked(int slideIndex)
	{
		Dictionary<string, string> values = new Dictionary<string, string>();
		
		values.Add ("SlideIndex", slideIndex.ToString());
		
		WriteSessionXML.WriteToXML(Event.NEXT_BUTTON_CLICKED.ToString(), values);
	}

	public static void PlayerScored(int scoreAmount, int totalScore) {
		Dictionary<string, string> values = new Dictionary<string, string>();
		
		values.Add ("Score Amount", scoreAmount.ToString());
		values.Add ("Total Score", totalScore.ToString());
		
		WriteSessionXML.WriteToXML(Event.PLAYER_SCORED.ToString(), values);
	}
	
	public static void ReportMultipleChoiceQuiz(string quiz, int val)
	{
		Dictionary<string, string> values = new Dictionary<string, string>();
		
		values.Add("Quiz", quiz);
		values.Add("Value", val.ToString());
		//values.Add("Unbiased Answer?", QuizDataInterperter.InterpertQuizAnswer(quiz, val).ToString());

		WriteSessionXML.WriteToXML(Event.QUIZ_MULTIPLE_CHOICE.ToString(), values);
	}

}
