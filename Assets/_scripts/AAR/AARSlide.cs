using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SlideContainer
{
	public AARSlide[] basicSlides;
	public AARSlideMovie[] movieSlides;
	public AARSlideQuizSlider[] sliderQuizzes;
	public AARSlideQuizMultipleChoice[] multipleChoiceQuizzes;
	public AARPathfinder[] pathfinders;
	public SpecialSlide[] specialSlides;
}

//For Deserialization form JSON
public class AARSlide 
{
	public int AAR;

	public int slideIndex;
	public int nextSlideIndex;
	public string titleContent;
	public string textContent;
	public bool soundClip;
	public bool nextButtonRequired;

	public BadgeTracker.Badge badge;

	public bool fireWorks;

	public Quiz answerVar;
}

public class AARSlideMovie : AARSlide
{
	public string movieClip;
}

public class AARSlideQuizMultipleChoice : AARSlide
{
	public Quiz quiz;

	public string imagePath;

	public Choice[] choices;
}

public class AARSlideQuizSlider : AARSlide
{
	public Quiz quiz;

	public Choice[] choices; 

	public string SliderLabel;
	public float highAmount;
	public float lowAmount;
	
	public string imagePath;
	
	public NumberTest[] tests;
	public float elseIndex;
}

public class SpecialSlide : AARSlide
{
	public enum Type
	{
		SPINNER,
		FULL_IMAGE,
		BADGE_PRESENTATION
	}

	public Type type;
	public string[] content;
	public string imagePath;
	public bool useUpperNextButton;
}

public class AARPathfinder : AARSlide
{
	public Quiz anchorChoice;
	public NumberTest[] numberTests;
	public float elseIndex;
}

public class NumberTest
{
	public enum TypeOfTest
	{
		GREATER_THAN,
		GREATER_EQUAL_THAN,
		LESS_THAN,
		LESS_EQUAL_THAN,
		EQUAL,
		BETWEEN,
		FALSE_GREATER_THAN,
		FALSE_GREATER_EQUAL_THAN,
		FALSE_LESS_THAN,
		FALSE_LESS_EQUAL_THAN,
		FALSE_EQUAL,
		FALSE_BETWEEN,
		COMPARE_GREATER_THAN,
		COMPARE_LESS_THAN,
		COMPARE_EQUAL
	}

	public TypeOfTest type;

	public float normalComparisonNumber;
	public float betweenHighNumber;
	public float betweenLowNumber;
	public int nextSlideIndexIfTrue;
}

public class Choice
{
	public string choice;
	public int nextIndexIfSelected;
}


