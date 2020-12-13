using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DebugTool : MonoBehaviour 
{

	private static DebugTool instance;

	private void Awake()
	{
		if (instance != null) {
			Debug.Log("Duplicate Debug Tool detected.  Self-destructing.");
			GameObject.Destroy(this.gameObject);
			return;
		}

		instance = this;
		GameObject.DontDestroyOnLoad (this.gameObject);
	}

	private void Update()
	{
		if (Debug.isDebugBuild)
						CheckForKeyPresses ();
	}

	private void CheckForKeyPresses()
	{
		if(Input.GetKeyUp (KeyCode.Space))
			SessionManager.Instance.GotoNextLevel();

		if(Input.GetKeyUp (KeyCode.L))
			ShowStoredAnswers();
	}

	private void ShowStoredAnswers()
	{
		string debugDump = "Dumping Stored Answers.../n";
		List<ScoreKeeper.QuizAnswer> quizAnswers = SessionManager.Instance.RetrieveStoredQuizAnswers();

		foreach(ScoreKeeper.QuizAnswer quizAnswer in quizAnswers) {

			string answer = "";

			if(quizAnswer is ScoreKeeper.QuizAnswerBool)
			{
				ScoreKeeper.QuizAnswerBool qab = (ScoreKeeper.QuizAnswerBool) quizAnswer;
				answer = qab.answerBool.ToString();
			}

			if(quizAnswer is ScoreKeeper.QuizAnswerMultipleChoice)
			{
				ScoreKeeper.QuizAnswerMultipleChoice qab = (ScoreKeeper.QuizAnswerMultipleChoice) quizAnswer;
				answer = qab.multipleChoiceAnswer.ToString();
			}

			if(quizAnswer is ScoreKeeper.QuizAnswerSlider)
			{
				ScoreKeeper.QuizAnswerSlider qab = (ScoreKeeper.QuizAnswerSlider) quizAnswer;
				answer = qab.sliderValue.ToString();
			}

			debugDump += "Quiz: " + quizAnswer.quiz.ToString() + " - Answer: " + answer;
		}
	}

}
