using UnityEngine;
using System.Collections;

public static class NumberTester 
{

	public static int? GetNextSlide(AARSlideQuizSlider sliderSlide) 
	{ 
		return ProcessNumberTests(sliderSlide.quiz, sliderSlide.answerVar, sliderSlide.tests, (int) sliderSlide.elseIndex);
	}

	public static int? GetNextSlide(AARPathfinder pathfinder) 
	{ 
		return ProcessNumberTests(pathfinder.anchorChoice, pathfinder.answerVar, pathfinder.numberTests, (int) pathfinder.elseIndex);
	}

	private static int? ProcessNumberTests(Quiz primaryQuiz, Quiz secondaryQuiz, NumberTest[] numberTests, int elseIndex)
	{
		if(CheckForCompareTest(numberTests))
			return GetCompareValue(primaryQuiz, secondaryQuiz, numberTests, elseIndex);

		if(secondaryQuiz == Quiz.NONE)
			return ProcessMultiChoices(primaryQuiz, numberTests, elseIndex);

		bool falseChoiceTest = IsTheBaseAnswerDisagree(primaryQuiz);
		float sliderVal = SessionManager.Instance.RetrieveSliderAnswer(secondaryQuiz);

		Debug.Log ("Players Agreed With Question?: " + !falseChoiceTest + " for the slider priming answer." + 
		           "\n" + "Found Value: " + sliderVal + " for the slider value.");
		
		int? nextIndex = null;

		if(falseChoiceTest) {
			foreach(NumberTest test in numberTests) {
				nextIndex = FalseChoiceTest(test, sliderVal);
				if(nextIndex != null)
					return nextIndex;
			}
		}
		
		foreach(NumberTest test in numberTests) {
			nextIndex = RegularTest(test, sliderVal);
			if(nextIndex != null)
				break;
		}
		
		if(nextIndex != null) {
			Debug.Log ("Next Index found: " + nextIndex.ToString());
			return nextIndex;
		}
		
		return (int?) elseIndex;
	}

	private static int? ProcessMultiChoices(Quiz multiChoice, NumberTest[] numberTests, int elseIndex)
	{
		int answeredChoice = SessionManager.Instance.RetrieveMultipleChoiceAnswer(multiChoice);

		Debug.Log ("Processing Multi Choice Path for Quiz: " + multiChoice.ToString() + " - with val: " + answeredChoice);

		foreach(NumberTest test in numberTests) {
			if(test.normalComparisonNumber == answeredChoice)
				return (int?) test.nextSlideIndexIfTrue;
		}

		return (int?) elseIndex;
	}

	private static bool IsTheBaseAnswerDisagree(Quiz choiceQuiz)
	{
		int answer = SessionManager.Instance.RetrieveMultipleChoiceAnswer(choiceQuiz);

		if(answer == 1)
			return true;

		return false;
	}

	//NOTE: This is a mess, I need to rethink how this is calculated.
	private static int? RegularTest(NumberTest test, float sliderVal)
	{
		if(test.type != NumberTest.TypeOfTest.BETWEEN)
			Debug.Log ("Regular NumberTest: " + test.type.ToString() + " " + test.normalComparisonNumber + " with val: " + sliderVal);
		else
			Debug.Log ("Between NumberTest: " + test.betweenHighNumber + " , " + test.betweenLowNumber+ " with val: " + sliderVal);

		switch(test.type)
		{
		case NumberTest.TypeOfTest.BETWEEN:
			if(BetweenTest(test.betweenHighNumber, test.betweenLowNumber, sliderVal))
				return test.nextSlideIndexIfTrue;
			break;
		case NumberTest.TypeOfTest.EQUAL:
			if(EqualTest(test.normalComparisonNumber, sliderVal))
				return test.nextSlideIndexIfTrue;
			break;
		case NumberTest.TypeOfTest.GREATER_EQUAL_THAN:
			if(sliderVal >= test.normalComparisonNumber)
				return test.nextSlideIndexIfTrue;
			break;
		case NumberTest.TypeOfTest.GREATER_THAN:
			if(sliderVal > test.normalComparisonNumber)
				return test.nextSlideIndexIfTrue;
			break;
		case NumberTest.TypeOfTest.LESS_EQUAL_THAN:
			if(sliderVal <= test.normalComparisonNumber)
				return test.nextSlideIndexIfTrue;
			break;
		case NumberTest.TypeOfTest.LESS_THAN:
			if(sliderVal < test.normalComparisonNumber)
				return test.nextSlideIndexIfTrue;
			break;
		}

		return null;
	}
	
	private static int? FalseChoiceTest(NumberTest test, float sliderVal)
	{
		if(test.type != NumberTest.TypeOfTest.BETWEEN)
			Debug.Log ("Regular NumberTest: " + test.type.ToString() + " " + test.normalComparisonNumber + " with val: " + sliderVal);
		else
			Debug.Log ("Between NumberTest: " + test.betweenHighNumber + " , " + test.betweenLowNumber+ " with val: " + sliderVal);

		switch(test.type)
		{
		case NumberTest.TypeOfTest.FALSE_BETWEEN:
			if(BetweenTest(test.betweenHighNumber, test.betweenLowNumber, sliderVal))
				return test.nextSlideIndexIfTrue;
			break;
		case NumberTest.TypeOfTest.FALSE_EQUAL:
			if(EqualTest(test.normalComparisonNumber, sliderVal))
				return test.nextSlideIndexIfTrue;
			break;
		case NumberTest.TypeOfTest.FALSE_GREATER_EQUAL_THAN:
			if(sliderVal >= test.normalComparisonNumber)
				return test.nextSlideIndexIfTrue;
			break;
		case NumberTest.TypeOfTest.FALSE_GREATER_THAN:
			if(sliderVal > test.normalComparisonNumber)
				return test.nextSlideIndexIfTrue;
			break;
		case NumberTest.TypeOfTest.FALSE_LESS_EQUAL_THAN:
			if(sliderVal <= test.normalComparisonNumber)
				return test.nextSlideIndexIfTrue;
			break;
		case NumberTest.TypeOfTest.FALSE_LESS_THAN:
			if(sliderVal < test.normalComparisonNumber)
				return test.nextSlideIndexIfTrue;
			break;
		}
		
		return null;
	}

	private static bool BetweenTest(float max, float min, float sliderVal)
	{
		Debug.Log ("Between Test: Max - " + max + " - Min - " + min + " Val - " + sliderVal);
		if(sliderVal < max && sliderVal > min)
			return true;

		return false;
	}

	private static bool EqualTest(float testVal, float sliderVal)
	{
		if(Mathf.CeilToInt(testVal) == Mathf.CeilToInt(sliderVal))
			return true;

		if(Mathf.CeilToInt(testVal) == Mathf.FloorToInt(sliderVal))
			return true; 

		return false;
	}

	private static bool CheckForCompareTest(NumberTest[] tests)
	{
		//Just need to sample the first one since all other tests will also be Compares (if set up correctly)

		switch(tests[0].type)
		{
		case NumberTest.TypeOfTest.COMPARE_EQUAL:
		case NumberTest.TypeOfTest.COMPARE_GREATER_THAN:
		case NumberTest.TypeOfTest.COMPARE_LESS_THAN:
			return true;
		}

		return false;
	}

	private static int? GetCompareValue(Quiz primaryQuiz, Quiz secondaryQuiz, NumberTest[] numberTests, int elseIndex)
	{
		Debug.Log ("Comparing...");
		int? nextIndex = null;

		float primary = SessionManager.Instance.RetrieveSliderAnswer(primaryQuiz);
		float secondary = SessionManager.Instance.RetrieveSliderAnswer(secondaryQuiz);

		Debug.Log ("Comparing " + primary + " to " + secondary);

		foreach(NumberTest test in numberTests)
		{
			switch(test.type)
			{
			case NumberTest.TypeOfTest.COMPARE_EQUAL:
				if(primary == secondary)
					return (int?) test.nextSlideIndexIfTrue;
				break;
			case NumberTest.TypeOfTest.COMPARE_GREATER_THAN:
				if(Mathf.Abs(primary - secondary) > test.normalComparisonNumber)
					return (int?) test.nextSlideIndexIfTrue;
				break;
			case NumberTest.TypeOfTest.COMPARE_LESS_THAN:
				if(Mathf.Abs(primary - secondary) < test.normalComparisonNumber)
					return (int?) test.nextSlideIndexIfTrue;
				break;
			}
		}

		return (int?) elseIndex;
	}

}
