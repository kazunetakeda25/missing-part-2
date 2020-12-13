using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScoreKeeper 
{
	
	private const int SCORE_AMOUNT_FOR_UNBIASED_ANSWER = 100;

	public class QuizAnswer
	{
		public string quiz;
		public virtual string GetCleanQuizValueInString() { return "no value"; }
		public virtual string GetQuizValueInString() { return "no value"; }
	}

	public class QuizAnswerSlider : QuizAnswer
	{
		public float sliderValue;

		public override string GetCleanQuizValueInString() { 
			return Mathf.RoundToInt(sliderValue).ToString();
		}

		public override string GetQuizValueInString() { 
			return Mathf.RoundToInt(sliderValue).ToString();
		}
	}

	public class QuizAnswerBool : QuizAnswer
	{
		public bool answerBool;
		public override string GetQuizValueInString() { return answerBool.ToString(); }
	}

	public class QuizAnswerMultipleChoice : QuizAnswer
	{
		public int multipleChoiceAnswer;

		public override string GetCleanQuizValueInString() { 
			string answerString = QuizDataInterperter.RetrieveAnswerString(quiz, multipleChoiceAnswer);
			return answerString;
		}

		public override string GetQuizValueInString() { 
			string answerString = multipleChoiceAnswer.ToString();
			return answerString;
		}
	}

	private List<QuizAnswer> quizAnswers = new List<QuizAnswer>();
	public List<QuizAnswer> StoredQuizAnswers { get { return quizAnswers; } }

	public void SetQuizAnswers(QuizAnswer[] answers) 
	{
		quizAnswers = new List<QuizAnswer>(answers);
	}

	public void ResetQuizAnswers()
	{
		if(quizAnswers != null) {
			quizAnswers.Clear();
			quizAnswers = null;
		}
	}

	private int score = 0;
	public int Score { get { return score; } }

	public void StoreQuizAnswer(Quiz quiz, bool value)
	{
		StoreQuizAnswer(quiz.ToString(), value);
	}

	public void StoreQuizAnswer(string quiz, bool value)
	{
		QuizAnswerBool newQuiz = new QuizAnswerBool();
		newQuiz.quiz = quiz;
		newQuiz.answerBool = value;
		quizAnswers.Add(newQuiz);

		DumpQuizAnswers();
	}

	private void CheckForDiffsCalculations()
	{
		if(DoesQuizExist(Quiz.GAME_2_HUNTING_DIFF) == false) {
			CheckForHuntingDiff();
		}

		if(DoesQuizExist(Quiz.AAR2_SOLAR_DIFF) == false) {
			CheckForSolarDiff();
		}
	}

	private void CheckForHuntingDiff()
	{
		if(DoesQuizExist(Quiz.GAME_2_HUNTING_ME.ToString()) && DoesQuizExist(Quiz.GAME_2_HUNTING_THEM.ToString())) {
			CreateDiffVar(Quiz.GAME_2_HUNTING_ME, Quiz.GAME_2_HUNTING_THEM, Quiz.GAME_2_HUNTING_DIFF);
		}
	}

	private void CheckForSolarDiff()
	{
		if(DoesQuizExist(Quiz.AAR2_SOLAR_ME.ToString()) && DoesQuizExist(Quiz.AAR2_SOLAR_THEM.ToString())) {
			CreateDiffVar(Quiz.AAR2_SOLAR_ME, Quiz.AAR2_SOLAR_THEM, Quiz.AAR2_SOLAR_DIFF);
		}
	}

	private void CreateDiffVar(Quiz quiz1, Quiz quiz2, Quiz newQuiz)
	{
		float firstVal = 0;
		float secondVal = 0;

		QuizAnswer qa1 = FindQuiz(quiz1);
		if(qa1 is QuizAnswerSlider) {
			firstVal = float.Parse(qa1.GetQuizValueInString());
		} else {
			Debug.LogError("Passed a non-float quiz!");
		}


		QuizAnswer qa2 = FindQuiz(quiz2);
		if(qa2 is QuizAnswerSlider) {
			secondVal = float.Parse(qa2.GetQuizValueInString());
		} else {
			Debug.LogError("Passed a non-float quiz!");
		}
			
		float difference = Mathf.Abs(firstVal - secondVal);
		Debug.Log("Diff: " + difference);
		StoreQuizAnswer(newQuiz, difference);
	}

	public void StoreQuizAnswer(Quiz quiz, float value)
	{
		StoreQuizAnswer(quiz.ToString(), value);
	}

	public void StoreQuizAnswer(string quiz, float value)
	{
		QuizAnswerSlider newQuiz = FindQuiz(quiz) as QuizAnswerSlider;
		if(newQuiz == null)
		{
			Debug.Log ("Creating New Quiz Data Node");
			newQuiz = new QuizAnswerSlider();
			quizAnswers.Add(newQuiz);
		}

		newQuiz.quiz = quiz;
		newQuiz.sliderValue = value;
		//CheckForScoredAnswer(quiz, value);

		ReportEvent.ReportUserInputQuiz(quiz, value);
		CheckForDiffsCalculations();
		DumpQuizAnswers();
	}

	public void StoreQuizAnswer(Quiz quiz, int value) 
	{
		StoreQuizAnswer(quiz.ToString(), value);
	}

	public void StoreQuizAnswer(string quiz, int value) 
	{
		Debug.Log ("Store Int Quiz Answer: " + quiz);
		QuizAnswerMultipleChoice newQuiz = FindQuiz(quiz) as QuizAnswerMultipleChoice;
		if(newQuiz == null)
		{
			Debug.Log ("Creating New Quiz Data Node");
			newQuiz = new QuizAnswerMultipleChoice();
			quizAnswers.Add(newQuiz);
		}

		newQuiz.quiz = quiz;
		newQuiz.multipleChoiceAnswer = value;

		//CheckForScoredAnswer(quiz, value);
		ReportEvent.ReportMultipleChoiceQuiz(quiz, value);
		CheckForDiffsCalculations();
		DumpQuizAnswers();
	}

	public void SetScore(int score)
	{
		this.score = score;
	}

	public void RewardPoints()
	{
		Debug.Log ("Rewarding Points...");
		score += SCORE_AMOUNT_FOR_UNBIASED_ANSWER;
		ReportEvent.PlayerScored(SCORE_AMOUNT_FOR_UNBIASED_ANSWER, score);
	}

	private void CheckForScoredAnswer(Quiz quiz, float value)
	{
		if(QuizDataInterperter.InterpertQuizAnswer(quiz, value)) {
			ReportEvent.PlayerScored(SCORE_AMOUNT_FOR_UNBIASED_ANSWER, score);
			score += SCORE_AMOUNT_FOR_UNBIASED_ANSWER;
			Debug.Log ("Unbiased Answer!  Player scores. Total Score: " + score);
		}
	}

	private void CheckForScoredAnswer(Quiz quiz, int value)
	{
		if(QuizDataInterperter.InterpertQuizAnswer(quiz, value)) {
			score += SCORE_AMOUNT_FOR_UNBIASED_ANSWER;
			Debug.Log ("Unbiased Answer!  Player scores. Total Score: " + score);
		}
	}

	private void DumpQuizAnswers()
	{
		string dump = "Dumping Quiz Answers...\n";

		for (int i = 0; i < quizAnswers.Count; i++) {
			string dumpString = "Quiz " + i + ": ";
			dumpString += quizAnswers[i].quiz.ToString();
			dumpString += " - Value: ";
			dumpString += quizAnswers[i].GetQuizValueInString();
			dumpString += "\n";

			dump += dumpString;
		}

		Debug.Log (dump);
	}

	public bool RetrieveBoolAnswer(Quiz quiz)
	{
		QuizAnswerBool myQuizAnswer =  FindQuiz(quiz.ToString()) as QuizAnswerBool;
		if(myQuizAnswer == null)
			return false;

		Debug.Log ("Found answe for quiz: " + quiz.ToString() + " - " + myQuizAnswer.answerBool);
		return myQuizAnswer.answerBool;
	}

	public float RetrieveFloatAnswer(Quiz quiz)
	{
		Debug.Log ("Retrieving answer for Slider Quiz " + quiz);
		QuizAnswerSlider myQuizAnswer =  FindQuiz(quiz.ToString()) as QuizAnswerSlider;

		if(myQuizAnswer == null) {
			Debug.LogWarning("Warning answer is null!");
			return 0.0f;
		}

		Debug.Log ("Found answer for quiz: " + quiz.ToString() + " - " + myQuizAnswer.sliderValue);
		return myQuizAnswer.sliderValue;
	}

	public int RetrieveMultipleChoiceAnswer(Quiz quiz)
	{
		Debug.Log ("Retrieving answer for MC Quiz " + quiz);
		QuizAnswerMultipleChoice myQuizAnswer = FindQuiz(quiz.ToString()) as QuizAnswerMultipleChoice;

		if(myQuizAnswer == null) {
			Debug.LogWarning("Warning answer is null!");
			return 0;
		}
		
		return myQuizAnswer.multipleChoiceAnswer;
	}

	public string RetrieveAnswerWithString(string quizID)
	{
		QuizAnswer savedQuiz = FindQuiz(quizID);

		if(savedQuiz == null)
			return "1";

		return savedQuiz.GetQuizValueInString();
	}

	public string RetrieveCleanAnswerString(string quizID)
	{
		QuizAnswer savedQuiz = FindQuiz(quizID);

		if(savedQuiz == null)
			return "1";

		return savedQuiz.GetCleanQuizValueInString();
	}

	public void WipeQuizAnswers()
	{
		quizAnswers.Clear();
	}
	 
	private QuizAnswer FindQuiz(Quiz quiz) { return FindQuiz(quiz.ToString()); }
	private QuizAnswer FindQuiz(string quiz)
	{
		for (int i = 0; i < quizAnswers.Count; i++) {
			if(quizAnswers[i].quiz == quiz)
				return quizAnswers[i];
		}

		Debug.LogWarning("Unable to find Quiz: " + quiz);
		return null;
	}

	private bool DoesQuizExist(Quiz quiz) { return DoesQuizExist(quiz.ToString()); }
	private bool DoesQuizExist(string quiz)
	{
		for (int i = 0; i < quizAnswers.Count; i++) {
			if(quizAnswers[i].quiz == quiz) {
				return true;
			}
		}
		return false;
	}

}
