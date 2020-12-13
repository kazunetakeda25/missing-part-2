using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

public class QuizData 
{
	public const float MAX_INPUT_VALUE = float.MaxValue;
	public Quiz quiz;
}

public class MultipleChoiceQuizData : QuizData
{
	public class Answer
	{
		public int answerIndex;
		public bool unbiasedAnswer;
		public string answerContent;
	}

	public Answer[] answers;
}

public class SliderQuizData : QuizData
{
	public class UnbiasedRange
	{
		public float unbiasedMaxRange;
		public float unbiasedMinRange;
	}

	public UnbiasedRange[] unbiasedRanges;
}

public class QuizDataCollection
{
	public List<MultipleChoiceQuizData> multipleChoiceQuizData = new List<MultipleChoiceQuizData>();
	public List<SliderQuizData> sliderQuizData = new List<SliderQuizData>();
}
