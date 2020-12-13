using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

public static class QuizDataInterperter
{
	private const string QUIZ_DATA_JSON_PATH = "JSON/QuizData";
	
	private static QuizDataCollection quizDataCollection;
	public static QuizDataCollection GetQuizData()
	{
		if(quizDataCollection == null)
			FetchQuizDataFromJSON();
		
		return quizDataCollection;
	}
	
	private static void FetchQuizDataFromJSON()
	{
		TextAsset jsonFile = Resources.Load(QUIZ_DATA_JSON_PATH) as TextAsset;
		quizDataCollection = JsonConvert.DeserializeObject<QuizDataCollection>(jsonFile.text);
	}
	
	public static bool InterpertQuizAnswer(Quiz quiz, float value) 
	{ 
		return InterpertQuizAnswer(quiz, (int) value); 
	}
	
	public static bool InterpertQuizAnswer(Quiz quiz, int value)
	{
		InitData();
		QuizData myQuizData = FindQuizData(quiz);
		
		if(myQuizData == null)
			return false;
		
		if(myQuizData is MultipleChoiceQuizData)
			return InterpertMultipleChoiceAnswer(quiz, value, myQuizData as MultipleChoiceQuizData);
		
		return InteprertSliderAnswer(quiz, value, myQuizData as SliderQuizData);
	}

	public static string RetrieveAnswerString(Quiz quiz, int value)
	{
		return RetrieveAnswerString(quiz.ToString(), value);
	}

	public static string RetrieveAnswerString(string quiz, int value)
	{
		InitData();
		QuizData myQuizData = FindQuizData(quiz);
		
		if(myQuizData is MultipleChoiceQuizData) {
			MultipleChoiceQuizData mcData = myQuizData as MultipleChoiceQuizData;
			return mcData.answers[value].answerContent;
		}
		
		
		Debug.LogWarning ("Attempted to retrieve Answer Data for a non-multiple choice question!  " + quiz.ToString());
		return "";
	}

	public static string[] RetrieveAnswerStrings(Quiz quiz)
	{
		InitData();
		QuizData myQuizData = FindQuizData(quiz);

		List<string> strings = new List<string>();

		if(myQuizData is MultipleChoiceQuizData) {
			MultipleChoiceQuizData mcData = myQuizData as MultipleChoiceQuizData;

			for (int i = 0; i < mcData.answers.Length; i++) {
				strings.Add(mcData.answers[i].answerContent);
			}

			return strings.ToArray();
		}
		
		Debug.LogWarning ("Attempted to retrieve Answer Data for a non-multiple choice question!  " + quiz.ToString());
		return new string[0];
	}
	
	public static bool IsMultipleChoice(Quiz quiz)
	{
		InitData();
		
		foreach(MultipleChoiceQuizData data in quizDataCollection.multipleChoiceQuizData) {
			if(data.quiz == quiz)
				return true;
		}
		
		return false;
	}
	
	private static void InitData()
	{
		if(quizDataCollection == null)
			GetQuizData();
	}
	
	private static bool InterpertMultipleChoiceAnswer(Quiz quiz, int value, MultipleChoiceQuizData quizAnswer)
	{
		string dump = "Interperting data for Quiz: " + quiz + " - with answer value: " + value;
		dump += "\n";
		dump += "Using QuizData for : " + quizAnswer.quiz;
		Debug.Log (dump);
		
		foreach(MultipleChoiceQuizData.Answer answer in quizAnswer.answers) {
			if(answer.answerIndex == value) {
				Debug.Log ("Answer is unbiased? " + answer.unbiasedAnswer);
				return (answer.unbiasedAnswer);
			}
		}
		
		return false;
	}
	
	private static bool InteprertSliderAnswer(Quiz quiz, int value, SliderQuizData quizAnswer)
	{
		string dump = "Interperting data for Quiz: " + quiz + " - with answer value: " + value;
		dump += "\n";
		dump += "Using QuizData for : " + quizAnswer.quiz;
		Debug.Log (dump);
		
		foreach(SliderQuizData.UnbiasedRange range in quizAnswer.unbiasedRanges) {
			if(value >= range.unbiasedMinRange &&
			   value <= range.unbiasedMaxRange) {
				Debug.Log ("Unbiased Answer Found!");
				return true;
			}
		}
		
		Debug.Log ("That's a Biased or N/A Answer.");
		
		return false;
	}

	private static QuizData FindQuizData(Quiz quizTarget)
	{
		return FindQuizData(quizTarget.ToString());
	}

	private static QuizData FindQuizData(string quizTarget)
	{
		foreach(QuizData qd in quizDataCollection.multipleChoiceQuizData) {
			if(qd.quiz.ToString() == quizTarget)
				return qd;
		}
		
		foreach(QuizData qd in quizDataCollection.sliderQuizData) {
			if(qd.quiz.ToString() == quizTarget)
				return qd;
		}
		
		Debug.LogError("Unable to find Quiz Data: " + quizTarget);
		return null;
	}
}
