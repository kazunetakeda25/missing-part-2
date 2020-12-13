using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

public class AARJSONGenerator : MonoBehaviour 
{
	public const string JSON_EXAMPLE_OUTPUT = "{\"basicSlides\":[{\"AAR\":1,\"slideIndex\":100,\"nextSlideIndex\":200,\"titleContent\":\"This is a Test Title.\",\"textContent\":\"This is a Test Slide.\",\"soundClip\":false,\"nextButtonRequired\":false,\"answerVar\":0},{\"AAR\":1,\"slideIndex\":200,\"nextSlideIndex\":300,\"titleContent\":null,\"textContent\":\"This is a the second test slide.\",\"soundClip\":false,\"nextButtonRequired\":false,\"answerVar\":0},{\"AAR\":1,\"slideIndex\":810,\"nextSlideIndex\":900,\"titleContent\":null,\"textContent\":\"This is a response to a Slider Test.\",\"soundClip\":false,\"nextButtonRequired\":true,\"answerVar\":0},{\"AAR\":1,\"slideIndex\":820,\"nextSlideIndex\":900,\"titleContent\":null,\"textContent\":\"This is a response to a Slider Test\",\"soundClip\":false,\"nextButtonRequired\":true,\"answerVar\":0},{\"AAR\":1,\"slideIndex\":830,\"nextSlideIndex\":900,\"titleContent\":null,\"textContent\":\"This is a response to a Slider Test.\",\"soundClip\":false,\"nextButtonRequired\":true,\"answerVar\":0},{\"AAR\":1,\"slideIndex\":840,\"nextSlideIndex\":900,\"titleContent\":null,\"textContent\":\"This is a response to a Slider Test.\",\"soundClip\":false,\"nextButtonRequired\":true,\"answerVar\":0},{\"AAR\":1,\"slideIndex\":510,\"nextSlideIndex\":600,\"titleContent\":null,\"textContent\":\"This is a response to a Multiple Choice Test.\",\"soundClip\":false,\"nextButtonRequired\":true,\"answerVar\":0},{\"AAR\":1,\"slideIndex\":520,\"nextSlideIndex\":600,\"titleContent\":null,\"textContent\":\"This is a response to a Multiple Choice Test.\",\"soundClip\":false,\"nextButtonRequired\":true,\"answerVar\":0},{\"AAR\":1,\"slideIndex\":530,\"nextSlideIndex\":600,\"titleContent\":null,\"textContent\":\"This is a response to a Multiple Choice Test.\",\"soundClip\":false,\"nextButtonRequired\":true,\"answerVar\":0},{\"AAR\":1,\"slideIndex\":540,\"nextSlideIndex\":600,\"titleContent\":null,\"textContent\":\"This is a response to a Multiple Choice Test.\",\"soundClip\":false,\"nextButtonRequired\":true,\"answerVar\":0},{\"AAR\":1,\"slideIndex\":550,\"nextSlideIndex\":600,\"titleContent\":null,\"textContent\":\"This is a response to a Multiple Choice Test.\",\"soundClip\":false,\"nextButtonRequired\":true,\"answerVar\":0},{\"AAR\":2,\"slideIndex\":100,\"nextSlideIndex\":200,\"titleContent\":\"This is a Test Title.\",\"textContent\":\"This is a Test Slide.\",\"soundClip\":false,\"nextButtonRequired\":false,\"answerVar\":0},{\"AAR\":2,\"slideIndex\":200,\"nextSlideIndex\":300,\"titleContent\":null,\"textContent\":\"This is a the second test slide.\",\"soundClip\":false,\"nextButtonRequired\":false,\"answerVar\":0},{\"AAR\":2,\"slideIndex\":810,\"nextSlideIndex\":900,\"titleContent\":null,\"textContent\":\"This is a response to a Slider Test.\",\"soundClip\":false,\"nextButtonRequired\":true,\"answerVar\":0},{\"AAR\":2,\"slideIndex\":820,\"nextSlideIndex\":900,\"titleContent\":null,\"textContent\":\"This is a response to a Slider Test\",\"soundClip\":false,\"nextButtonRequired\":true,\"answerVar\":0},{\"AAR\":2,\"slideIndex\":830,\"nextSlideIndex\":900,\"titleContent\":null,\"textContent\":\"This is a response to a Slider Test.\",\"soundClip\":false,\"nextButtonRequired\":true,\"answerVar\":0},{\"AAR\":2,\"slideIndex\":840,\"nextSlideIndex\":900,\"titleContent\":null,\"textContent\":\"This is a response to a Slider Test.\",\"soundClip\":false,\"nextButtonRequired\":true,\"answerVar\":0},{\"AAR\":2,\"slideIndex\":510,\"nextSlideIndex\":600,\"titleContent\":null,\"textContent\":\"This is a response to a Multiple Choice Test.\",\"soundClip\":false,\"nextButtonRequired\":true,\"answerVar\":0},{\"AAR\":2,\"slideIndex\":520,\"nextSlideIndex\":600,\"titleContent\":null,\"textContent\":\"This is a response to a Multiple Choice Test.\",\"soundClip\":false,\"nextButtonRequired\":true,\"answerVar\":0},{\"AAR\":2,\"slideIndex\":530,\"nextSlideIndex\":600,\"titleContent\":null,\"textContent\":\"This is a response to a Multiple Choice Test.\",\"soundClip\":false,\"nextButtonRequired\":true,\"answerVar\":0},{\"AAR\":2,\"slideIndex\":540,\"nextSlideIndex\":600,\"titleContent\":null,\"textContent\":\"This is a response to a Multiple Choice Test.\",\"soundClip\":false,\"nextButtonRequired\":true,\"answerVar\":0},{\"AAR\":2,\"slideIndex\":550,\"nextSlideIndex\":600,\"titleContent\":null,\"textContent\":\"This is a response to a Multiple Choice Test.\",\"soundClip\":false,\"nextButtonRequired\":true,\"answerVar\":0},{\"AAR\":3,\"slideIndex\":100,\"nextSlideIndex\":200,\"titleContent\":\"This is a Test Title.\",\"textContent\":\"This is a Test Slide.\",\"soundClip\":false,\"nextButtonRequired\":false,\"answerVar\":0},{\"AAR\":3,\"slideIndex\":200,\"nextSlideIndex\":300,\"titleContent\":null,\"textContent\":\"This is a the second test slide.\",\"soundClip\":false,\"nextButtonRequired\":false,\"answerVar\":0},{\"AAR\":3,\"slideIndex\":810,\"nextSlideIndex\":900,\"titleContent\":null,\"textContent\":\"This is a response to a Slider Test.\",\"soundClip\":false,\"nextButtonRequired\":true,\"answerVar\":0},{\"AAR\":3,\"slideIndex\":820,\"nextSlideIndex\":900,\"titleContent\":null,\"textContent\":\"This is a response to a Slider Test\",\"soundClip\":false,\"nextButtonRequired\":true,\"answerVar\":0},{\"AAR\":3,\"slideIndex\":830,\"nextSlideIndex\":900,\"titleContent\":null,\"textContent\":\"This is a response to a Slider Test.\",\"soundClip\":false,\"nextButtonRequired\":true,\"answerVar\":0},{\"AAR\":3,\"slideIndex\":840,\"nextSlideIndex\":900,\"titleContent\":null,\"textContent\":\"This is a response to a Slider Test.\",\"soundClip\":false,\"nextButtonRequired\":true,\"answerVar\":0},{\"AAR\":3,\"slideIndex\":510,\"nextSlideIndex\":600,\"titleContent\":null,\"textContent\":\"This is a response to a Multiple Choice Test.\",\"soundClip\":false,\"nextButtonRequired\":true,\"answerVar\":0},{\"AAR\":3,\"slideIndex\":520,\"nextSlideIndex\":600,\"titleContent\":null,\"textContent\":\"This is a response to a Multiple Choice Test.\",\"soundClip\":false,\"nextButtonRequired\":true,\"answerVar\":0},{\"AAR\":3,\"slideIndex\":530,\"nextSlideIndex\":600,\"titleContent\":null,\"textContent\":\"This is a response to a Multiple Choice Test.\",\"soundClip\":false,\"nextButtonRequired\":true,\"answerVar\":0},{\"AAR\":3,\"slideIndex\":540,\"nextSlideIndex\":600,\"titleContent\":null,\"textContent\":\"This is a response to a Multiple Choice Test.\",\"soundClip\":false,\"nextButtonRequired\":true,\"answerVar\":0},{\"AAR\":3,\"slideIndex\":550,\"nextSlideIndex\":600,\"titleContent\":null,\"textContent\":\"This is a response to a Multiple Choice Test.\",\"soundClip\":false,\"nextButtonRequired\":true,\"answerVar\":0}],\"movieSlides\":[{\"movieClip\":\"TestMovie\",\"AAR\":1,\"slideIndex\":300,\"nextSlideIndex\":400,\"titleContent\":null,\"textContent\":null,\"soundClip\":false,\"nextButtonRequired\":false,\"answerVar\":0},{\"movieClip\":\"TestMovie\",\"AAR\":2,\"slideIndex\":300,\"nextSlideIndex\":400,\"titleContent\":null,\"textContent\":null,\"soundClip\":false,\"nextButtonRequired\":false,\"answerVar\":0},{\"movieClip\":\"TestMovie\",\"AAR\":3,\"slideIndex\":300,\"nextSlideIndex\":400,\"titleContent\":null,\"textContent\":null,\"soundClip\":false,\"nextButtonRequired\":false,\"answerVar\":0}],\"sliderQuizzes\":[{\"quiz\":12,\"choices\":[{\"choice\":\"Chocolate is better.\",\"nextIndexIfSelected\":0},{\"choice\":\"Vanilla is better.\",\"nextIndexIfSelected\":0}],\"SliderLabel\":null,\"highAmount\":100.0,\"lowAmount\":0.0,\"imagePath\":null,\"tests\":[{\"type\":4,\"normalComparisonNumber\":10.0,\"betweenHighNumber\":0.0,\"betweenLowNumber\":0.0,\"nextSlideIndexIfTrue\":810},{\"type\":2,\"normalComparisonNumber\":10.0,\"betweenHighNumber\":0.0,\"betweenLowNumber\":0.0,\"nextSlideIndexIfTrue\":820},{\"type\":5,\"normalComparisonNumber\":0.0,\"betweenHighNumber\":20.0,\"betweenLowNumber\":11.0,\"nextSlideIndexIfTrue\":830},{\"type\":0,\"normalComparisonNumber\":20.0,\"betweenHighNumber\":0.0,\"betweenLowNumber\":0.0,\"nextSlideIndexIfTrue\":840}],\"elseIndex\":0.0,\"AAR\":1,\"slideIndex\":600,\"nextSlideIndex\":0,\"titleContent\":\"The Slider\",\"textContent\":\"This is a test slider quiz!!\",\"soundClip\":false,\"nextButtonRequired\":false,\"answerVar\":13},{\"quiz\":12,\"choices\":[{\"choice\":\"Chocolate is better.\",\"nextIndexIfSelected\":0},{\"choice\":\"Vanilla is better.\",\"nextIndexIfSelected\":0}],\"SliderLabel\":null,\"highAmount\":100.0,\"lowAmount\":0.0,\"imagePath\":null,\"tests\":[{\"type\":4,\"normalComparisonNumber\":10.0,\"betweenHighNumber\":0.0,\"betweenLowNumber\":0.0,\"nextSlideIndexIfTrue\":810},{\"type\":2,\"normalComparisonNumber\":10.0,\"betweenHighNumber\":0.0,\"betweenLowNumber\":0.0,\"nextSlideIndexIfTrue\":820},{\"type\":5,\"normalComparisonNumber\":0.0,\"betweenHighNumber\":20.0,\"betweenLowNumber\":11.0,\"nextSlideIndexIfTrue\":830},{\"type\":0,\"normalComparisonNumber\":20.0,\"betweenHighNumber\":0.0,\"betweenLowNumber\":0.0,\"nextSlideIndexIfTrue\":840}],\"elseIndex\":0.0,\"AAR\":2,\"slideIndex\":600,\"nextSlideIndex\":0,\"titleContent\":\"The Slider\",\"textContent\":\"This is a test slider quiz!!\",\"soundClip\":false,\"nextButtonRequired\":false,\"answerVar\":13},{\"quiz\":12,\"choices\":[{\"choice\":\"Chocolate is better.\",\"nextIndexIfSelected\":0},{\"choice\":\"Vanilla is better.\",\"nextIndexIfSelected\":0}],\"SliderLabel\":null,\"highAmount\":100.0,\"lowAmount\":0.0,\"imagePath\":null,\"tests\":[{\"type\":4,\"normalComparisonNumber\":10.0,\"betweenHighNumber\":0.0,\"betweenLowNumber\":0.0,\"nextSlideIndexIfTrue\":810},{\"type\":2,\"normalComparisonNumber\":10.0,\"betweenHighNumber\":0.0,\"betweenLowNumber\":0.0,\"nextSlideIndexIfTrue\":820},{\"type\":5,\"normalComparisonNumber\":0.0,\"betweenHighNumber\":20.0,\"betweenLowNumber\":11.0,\"nextSlideIndexIfTrue\":830},{\"type\":0,\"normalComparisonNumber\":20.0,\"betweenHighNumber\":0.0,\"betweenLowNumber\":0.0,\"nextSlideIndexIfTrue\":840}],\"elseIndex\":0.0,\"AAR\":3,\"slideIndex\":600,\"nextSlideIndex\":0,\"titleContent\":\"The Slider\",\"textContent\":\"This is a test slider quiz!!\",\"soundClip\":false,\"nextButtonRequired\":false,\"answerVar\":13}],\"multipleChoiceQuizzes\":[{\"quiz\":0,\"imagePath\":null,\"choices\":[{\"choice\":\"Test choice number one.\",\"nextIndexIfSelected\":510},{\"choice\":\"Test choice number two.\",\"nextIndexIfSelected\":520},{\"choice\":\"Test choice number three.\",\"nextIndexIfSelected\":530},{\"choice\":\"Test choice number four.\",\"nextIndexIfSelected\":540},{\"choice\":\"Test choice number five.\",\"nextIndexIfSelected\":550}],\"AAR\":1,\"slideIndex\":400,\"nextSlideIndex\":0,\"titleContent\":\"Multiple Choice Test\",\"textContent\":\"This is a sample multiple choice quiz...\",\"soundClip\":false,\"nextButtonRequired\":false,\"answerVar\":0},{\"quiz\":0,\"imagePath\":null,\"choices\":[{\"choice\":\"Test choice number one.\",\"nextIndexIfSelected\":510},{\"choice\":\"Test choice number two.\",\"nextIndexIfSelected\":520},{\"choice\":\"Test choice number three.\",\"nextIndexIfSelected\":530},{\"choice\":\"Test choice number four.\",\"nextIndexIfSelected\":540},{\"choice\":\"Test choice number five.\",\"nextIndexIfSelected\":550}],\"AAR\":2,\"slideIndex\":400,\"nextSlideIndex\":0,\"titleContent\":\"Multiple Choice Test\",\"textContent\":\"This is a sample multiple choice quiz...\",\"soundClip\":false,\"nextButtonRequired\":false,\"answerVar\":0},{\"quiz\":0,\"imagePath\":null,\"choices\":[{\"choice\":\"Test choice number one.\",\"nextIndexIfSelected\":510},{\"choice\":\"Test choice number two.\",\"nextIndexIfSelected\":520},{\"choice\":\"Test choice number three.\",\"nextIndexIfSelected\":530},{\"choice\":\"Test choice number four.\",\"nextIndexIfSelected\":540},{\"choice\":\"Test choice number five.\",\"nextIndexIfSelected\":550}],\"AAR\":3,\"slideIndex\":400,\"nextSlideIndex\":0,\"titleContent\":\"Multiple Choice Test\",\"textContent\":\"This is a sample multiple choice quiz...\",\"soundClip\":false,\"nextButtonRequired\":false,\"answerVar\":0}],\"imageSlides\":[{\"imagePath\":\"TestImage\",\"AAR\":1,\"slideIndex\":900,\"nextSlideIndex\":-1,\"titleContent\":\"This is a Test Image Title.\",\"textContent\":\"This is a Test Image Slide.\",\"soundClip\":false,\"nextButtonRequired\":false,\"answerVar\":0},{\"imagePath\":\"TestImage\",\"AAR\":2,\"slideIndex\":900,\"nextSlideIndex\":-1,\"titleContent\":\"This is a Test Image Title.\",\"textContent\":\"This is a Test Image Slide.\",\"soundClip\":false,\"nextButtonRequired\":false,\"answerVar\":0},{\"imagePath\":\"TestImage\",\"AAR\":3,\"slideIndex\":900,\"nextSlideIndex\":-1,\"titleContent\":\"This is a Test Image Title.\",\"textContent\":\"This is a Test Image Slide.\",\"soundClip\":false,\"nextButtonRequired\":false,\"answerVar\":0}],\"pathfinders\":[{\"anchorChoice\":1,\"numberTests\":[{\"type\":2,\"normalComparisonNumber\":14.0,\"betweenHighNumber\":0.0,\"betweenLowNumber\":0.0,\"nextSlideIndexIfTrue\":1075},{\"type\":4,\"normalComparisonNumber\":11.0,\"betweenHighNumber\":0.0,\"betweenLowNumber\":0.0,\"nextSlideIndexIfTrue\":1080},{\"type\":0,\"normalComparisonNumber\":30.0,\"betweenHighNumber\":0.0,\"betweenLowNumber\":0.0,\"nextSlideIndexIfTrue\":1090},{\"type\":5,\"normalComparisonNumber\":0.0,\"betweenHighNumber\":16.7,\"betweenLowNumber\":11.0,\"nextSlideIndexIfTrue\":1100}],\"elseIndex\":1110.0,\"AAR\":0,\"slideIndex\":0,\"nextSlideIndex\":0,\"titleContent\":null,\"textContent\":null,\"soundClip\":false,\"nextButtonRequired\":false,\"answerVar\":2},{\"anchorChoice\":1,\"numberTests\":[{\"type\":2,\"normalComparisonNumber\":14.0,\"betweenHighNumber\":0.0,\"betweenLowNumber\":0.0,\"nextSlideIndexIfTrue\":1075},{\"type\":4,\"normalComparisonNumber\":11.0,\"betweenHighNumber\":0.0,\"betweenLowNumber\":0.0,\"nextSlideIndexIfTrue\":1080},{\"type\":0,\"normalComparisonNumber\":30.0,\"betweenHighNumber\":0.0,\"betweenLowNumber\":0.0,\"nextSlideIndexIfTrue\":1090},{\"type\":5,\"normalComparisonNumber\":0.0,\"betweenHighNumber\":16.7,\"betweenLowNumber\":11.0,\"nextSlideIndexIfTrue\":1100}],\"elseIndex\":1110.0,\"AAR\":0,\"slideIndex\":0,\"nextSlideIndex\":0,\"titleContent\":null,\"textContent\":null,\"soundClip\":false,\"nextButtonRequired\":false,\"answerVar\":2},{\"anchorChoice\":1,\"numberTests\":[{\"type\":2,\"normalComparisonNumber\":14.0,\"betweenHighNumber\":0.0,\"betweenLowNumber\":0.0,\"nextSlideIndexIfTrue\":1075},{\"type\":4,\"normalComparisonNumber\":11.0,\"betweenHighNumber\":0.0,\"betweenLowNumber\":0.0,\"nextSlideIndexIfTrue\":1080},{\"type\":0,\"normalComparisonNumber\":30.0,\"betweenHighNumber\":0.0,\"betweenLowNumber\":0.0,\"nextSlideIndexIfTrue\":1090},{\"type\":5,\"normalComparisonNumber\":0.0,\"betweenHighNumber\":16.7,\"betweenLowNumber\":11.0,\"nextSlideIndexIfTrue\":1100}],\"elseIndex\":1110.0,\"AAR\":0,\"slideIndex\":0,\"nextSlideIndex\":0,\"titleContent\":null,\"textContent\":null,\"soundClip\":false,\"nextButtonRequired\":false,\"answerVar\":2}]}";
	public const string QUIZ_DATA_EXAMPLE = "{\n   \"quizDatas\" : [\n      {\n         \"answers\" : [\n            {\n               \"answerContent\" : \"Test Answer 1\",\n               \"answerIndex\" : 0,\n               \"unbiasedAnswer\" : false\n            }\n         ],\n         \"quiz\" : \"GAME_1_2_AVERAGE_FACEBOOK_USE\"\n      },\n      {\n         \"answers\" : [\n            {\n               \"answerContent\" : \"Test Answer 1\",\n               \"answerIndex\" : 0,\n               \"unbiasedAnswer\" : false\n            }\n         ],\n         \"quiz\" : \"GAME_1_2A_AVERAGE_FACEBOOK_USE_SLIDER\"\n      }\n   ]\n}\n";

	private void Awake()
	{
		SlideContainer container = JsonConvert.DeserializeObject<SlideContainer>(GenerateJSONTemplate());
		QuizDataCollection collection = JsonConvert.DeserializeObject<QuizDataCollection>(QUIZ_DATA_EXAMPLE);
	}

	private string GenerateJSONTemplate()
	{
		SlideContainer exampleContainer = TestContainer();

		string example = JsonConvert.SerializeObject(exampleContainer);
		Debug.Log (example);

		string quizDataExample = JsonConvert.SerializeObject(GenerateSampleQuizDataCollection());
		Debug.Log (quizDataExample);

		return example;
	}

	private QuizDataCollection GenerateSampleQuizDataCollection()
	{
		QuizDataCollection collection = new QuizDataCollection();
		List<MultipleChoiceQuizData> quizData = new List<MultipleChoiceQuizData>();

		MultipleChoiceQuizData data1 = new MultipleChoiceQuizData();
		data1.quiz = Quiz.AAR1_CATAMARAN;

		MultipleChoiceQuizData.Answer answer1a = new MultipleChoiceQuizData.Answer();
		answer1a.unbiasedAnswer = false;
		answer1a.answerContent = "Test Answer 1";
		answer1a.answerIndex = 0;
		MultipleChoiceQuizData.Answer answer1b = new MultipleChoiceQuizData.Answer();
		answer1b.unbiasedAnswer = false;
		answer1b.answerContent = "Test Answer 2";
		answer1b.answerIndex = 1;
		MultipleChoiceQuizData.Answer answer1c = new MultipleChoiceQuizData.Answer();
		answer1c.unbiasedAnswer = false;
		answer1c.answerContent = "Test Answer 3";
		answer1c.answerIndex = 2;
		MultipleChoiceQuizData.Answer answer1d = new MultipleChoiceQuizData.Answer();
		answer1d.unbiasedAnswer = false;
		answer1d.answerContent = "Test Answer 4";
		answer1d.answerIndex = 3;
		MultipleChoiceQuizData.Answer answer1e = new MultipleChoiceQuizData.Answer();
		answer1e.unbiasedAnswer = true;
		answer1e.answerContent = "Test Answer 5";
		answer1e.answerIndex = 4;

		data1.answers = new MultipleChoiceQuizData.Answer[] { answer1a, answer1b, answer1c, answer1d, answer1d, answer1e };

		quizData.Add(data1);

		MultipleChoiceQuizData data2 = new MultipleChoiceQuizData();
		data2.quiz = Quiz.AAR1_CATAMARAN;
		
		MultipleChoiceQuizData.Answer answer2a = new MultipleChoiceQuizData.Answer();
		answer2a.unbiasedAnswer = false;
		answer2a.answerContent = "Test Answer 1";
		answer2a.answerIndex = 0;
		MultipleChoiceQuizData.Answer answer2b = new MultipleChoiceQuizData.Answer();
		answer2b.unbiasedAnswer = false;
		answer2b.answerContent = "Test Answer 2";
		answer2b.answerIndex = 1;
		MultipleChoiceQuizData.Answer answer2c = new MultipleChoiceQuizData.Answer();
		answer2c.unbiasedAnswer = false;
		answer2c.answerContent = "Test Answer 3";
		answer2c.answerIndex = 2;
		MultipleChoiceQuizData.Answer answer2d = new MultipleChoiceQuizData.Answer();
		answer2d.unbiasedAnswer = false;
		answer2d.answerContent = "Test Answer 4";
		answer2d.answerIndex = 3;
		MultipleChoiceQuizData.Answer answer2e = new MultipleChoiceQuizData.Answer();
		answer2e.unbiasedAnswer = true;
		answer2e.answerContent = "Test Answer 5";
		answer1e.answerIndex = 4;
		
		data2.answers = new MultipleChoiceQuizData.Answer[] { answer2a, answer2b, answer2c, answer2d, answer2e };

		quizData.Add(data2);

		collection.multipleChoiceQuizData = quizData;

		List<SliderQuizData> sliderQuizData = new List<SliderQuizData>();

		SliderQuizData sliderQuizData1 = new SliderQuizData();
		sliderQuizData1.quiz = Quiz.AAR1_CATAMARAN_SLIDER;
		SliderQuizData.UnbiasedRange range1 = new SliderQuizData.UnbiasedRange();
		range1.unbiasedMaxRange = QuizData.MAX_INPUT_VALUE;
		range1.unbiasedMinRange = 100;
		SliderQuizData.UnbiasedRange range2 = new SliderQuizData.UnbiasedRange();
		range2.unbiasedMaxRange = float.MaxValue;
		range2.unbiasedMinRange = 100;
		sliderQuizData1.unbiasedRanges = new SliderQuizData.UnbiasedRange[] {range1, range2};

		sliderQuizData.Add(sliderQuizData1);

		collection.sliderQuizData = sliderQuizData;

		return collection;
	}

	private SlideContainer TestContainer()
	{
		SlideContainer exampleContainer = new SlideContainer();

		exampleContainer.basicSlides = GenerateBasicSlides();
		exampleContainer.movieSlides = GenerateSlideMovies();
		exampleContainer.multipleChoiceQuizzes = GenerateMultiChoiceQuizzss();
		exampleContainer.sliderQuizzes = GenerateQuizSliders();
		exampleContainer.pathfinders = GeneratePathfinders();

		return exampleContainer;
	}

	private AARPathfinder[] GeneratePathfinders()
	{
		List<AARPathfinder> pathfinders = new List<AARPathfinder>();

		for (int i = 1; i < 4; i++) {
			AARPathfinder pathfinder = new AARPathfinder();
			pathfinder.anchorChoice = Quiz.GAME_1_2_AVERAGE_FACEBOOK_USE;
			pathfinder.answerVar = Quiz.GAME_1_2A_AVERAGE_FACEBOOK_USE_SLIDER;

			NumberTest numberTest1 = new NumberTest();
			numberTest1.type = NumberTest.TypeOfTest.LESS_THAN;
			numberTest1.normalComparisonNumber = 14;
			numberTest1.nextSlideIndexIfTrue = 1075;

			NumberTest numberTest2 = new NumberTest();
			numberTest2.type = NumberTest.TypeOfTest.EQUAL;
			numberTest2.normalComparisonNumber = 11;
			numberTest2.nextSlideIndexIfTrue = 1080;

			NumberTest numberTest3 = new NumberTest();
			numberTest3.type = NumberTest.TypeOfTest.GREATER_THAN;
			numberTest3.normalComparisonNumber = 30;
			numberTest3.nextSlideIndexIfTrue = 1090;

			NumberTest numberTest4 = new NumberTest();
			numberTest4.type = NumberTest.TypeOfTest.BETWEEN;
			numberTest4.betweenLowNumber = 11;
			numberTest4.betweenHighNumber = 16.7f;
			numberTest4.nextSlideIndexIfTrue = 1100;

			pathfinder.numberTests = new NumberTest[] { numberTest1, numberTest2, numberTest3, numberTest4 };
			pathfinder.elseIndex = 1110;

			pathfinders.Add (pathfinder);
		}

		return pathfinders.ToArray();
	}

	private AARSlide[] GenerateBasicSlides()
	{
		List<AARSlide> basicSlides = new List<AARSlide>();

		for (int i = 1; i < 4; i++) {
			AARSlide basicSlide1 = new AARSlide();
			basicSlide1.AAR = i;
			basicSlide1.slideIndex = 100;
			basicSlide1.nextSlideIndex = 200;
			basicSlide1.textContent = "This is a Test Slide.";
			basicSlide1.titleContent = "This is a Test Title.";
			basicSlide1.soundClip = false;
			basicSlide1.nextButtonRequired = false;
			
			AARSlide basicSlide2 = new AARSlide();
			basicSlide2.AAR = i;
			basicSlide2.slideIndex = 200;
			basicSlide2.nextSlideIndex = 300;
			basicSlide2.textContent = "This is a the second test slide.";
			basicSlide2.soundClip = false;
			basicSlide2.nextButtonRequired = false;

			AARSlide slideResp1 = new AARSlide();
			slideResp1.AAR = i;
			slideResp1.slideIndex = 810;
			slideResp1.nextSlideIndex = 900;
			slideResp1.textContent = "This is a response to a Slider Test.";
			slideResp1.soundClip = false;
			slideResp1.nextButtonRequired = true;

			AARSlide slideResp2 = new AARSlide();
			slideResp2.AAR = i;
			slideResp2.slideIndex = 820;
			slideResp2.nextSlideIndex = 900;
			slideResp2.textContent = "This is a response to a Slider Test";
			slideResp2.soundClip = false;
			slideResp2.nextButtonRequired = true;

			AARSlide slideResp3 = new AARSlide();
			slideResp3.AAR = i;
			slideResp3.slideIndex = 830;
			slideResp3.nextSlideIndex = 900;
			slideResp3.textContent = "This is a response to a Slider Test.";
			slideResp3.soundClip = false;
			slideResp3.nextButtonRequired = true;

			AARSlide slideResp4 = new AARSlide();
			slideResp4.AAR = i;
			slideResp4.slideIndex = 840;
			slideResp4.nextSlideIndex = 900;
			slideResp4.textContent = "This is a response to a Slider Test.";
			slideResp4.soundClip = false;
			slideResp4.nextButtonRequired = true;

			AARSlide mcResp1 = new AARSlide();
			mcResp1.AAR = i;
			mcResp1.slideIndex = 510;
			mcResp1.nextSlideIndex = 600;
			mcResp1.textContent = "This is a response to a Multiple Choice Test.";
			mcResp1.soundClip = false;
			mcResp1.nextButtonRequired = true;

			AARSlide mcResp2 = new AARSlide();
			mcResp2.AAR = i;
			mcResp2.slideIndex = 520;
			mcResp2.nextSlideIndex = 600;
			mcResp2.textContent = "This is a response to a Multiple Choice Test.";
			mcResp2.soundClip = false;
			mcResp2.nextButtonRequired = true;

			AARSlide mcResp3 = new AARSlide();
			mcResp3.AAR = i;
			mcResp3.slideIndex = 530;
			mcResp3.nextSlideIndex = 600;
			mcResp3.textContent = "This is a response to a Multiple Choice Test.";
			mcResp3.soundClip = false;
			mcResp3.nextButtonRequired = true;

			AARSlide mcResp4 = new AARSlide();
			mcResp4.AAR = i;
			mcResp4.slideIndex = 540;
			mcResp4.nextSlideIndex = 600;
			mcResp4.textContent = "This is a response to a Multiple Choice Test.";
			mcResp4.soundClip = false;
			mcResp4.nextButtonRequired = true;

			AARSlide mcResp5 = new AARSlide();
			mcResp5.AAR = i;
			mcResp5.slideIndex = 550;
			mcResp5.nextSlideIndex = 600;
			mcResp5.textContent = "This is a response to a Multiple Choice Test.";
			mcResp5.soundClip = false;
			mcResp5.nextButtonRequired = true;

			basicSlides.Add(basicSlide1);
			basicSlides.Add(basicSlide2);
			basicSlides.Add(slideResp1);
			basicSlides.Add(slideResp2);
			basicSlides.Add(slideResp3);
			basicSlides.Add(slideResp4);
			basicSlides.Add(mcResp1);
			basicSlides.Add(mcResp2);
			basicSlides.Add(mcResp3);
			basicSlides.Add(mcResp4);
			basicSlides.Add(mcResp5);
		}

		return basicSlides.ToArray();
	}

	private AARSlideMovie[] GenerateSlideMovies()
	{
		List<AARSlideMovie> movieSlides = new List<AARSlideMovie>();

		for (int i = 1; i < 4; i++) 
		{
			AARSlideMovie movieSlide1 = new AARSlideMovie();
			movieSlide1.AAR = i;
			movieSlide1.slideIndex = 300;
			movieSlide1.nextSlideIndex = 400;
			movieSlide1.movieClip = "TestMovie";
			movieSlides.Add(movieSlide1);
		}

		return movieSlides.ToArray();
	}

	private AARSlideQuizMultipleChoice[] GenerateMultiChoiceQuizzss()
	{
		List<AARSlideQuizMultipleChoice> multiChoiceQuizzes = new List<AARSlideQuizMultipleChoice>();

		for (int i = 1; i < 4; i++) {
			
			AARSlideQuizMultipleChoice multiChoiceSlide1 = new AARSlideQuizMultipleChoice();
			Choice choice1 = new Choice();
			choice1.choice = "Test choice number one.";
			choice1.nextIndexIfSelected = 510;
			
			Choice choice2 = new Choice();
			choice2.choice = "Test choice number two.";
			choice2.nextIndexIfSelected = 520;
			
			Choice choice3 = new Choice();
			choice3.choice = "Test choice number three.";
			choice3.nextIndexIfSelected = 530;
			
			Choice choice4 = new Choice();
			choice4.choice = "Test choice number four.";
			choice4.nextIndexIfSelected = 540;
			
			Choice choice5 = new Choice();
			choice5.choice = "Test choice number five.";
			choice5.nextIndexIfSelected = 550;
			
			multiChoiceSlide1.choices = new Choice[] 
			{ choice1, choice2, choice3, choice4, choice5 };
			multiChoiceSlide1.AAR = i;
			multiChoiceSlide1.titleContent = "Multiple Choice Test";
			multiChoiceSlide1.textContent = "This is a sample multiple choice quiz...";
			multiChoiceSlide1.slideIndex = 400;		

			multiChoiceQuizzes.Add (multiChoiceSlide1);
		}

		return multiChoiceQuizzes.ToArray();
	}

	private AARSlideQuizSlider[] GenerateQuizSliders()
	{
		List<AARSlideQuizSlider> sliderQuizzes = new List<AARSlideQuizSlider>();

		for (int i = 1; i < 4; i++) {
			AARSlideQuizSlider quizSlider = new AARSlideQuizSlider();

			quizSlider.AAR = i;
			quizSlider.highAmount = 100;
			quizSlider.lowAmount = 0;
			quizSlider.slideIndex = 600;
			quizSlider.textContent = "This is a test slider quiz!!";
			quizSlider.titleContent = "The Slider";

			quizSlider.quiz = Quiz.AAR1_CATAMARAN;
			quizSlider.answerVar = Quiz.AAR1_CATAMARAN_SLIDER;

			NumberTest numberTest1 = new NumberTest();
			numberTest1.nextSlideIndexIfTrue = 810;
			numberTest1.type = NumberTest.TypeOfTest.EQUAL;
			numberTest1.normalComparisonNumber = 10;

			NumberTest numberTest2 = new NumberTest();
			numberTest2.nextSlideIndexIfTrue = 820;
			numberTest2.type = NumberTest.TypeOfTest.LESS_THAN;
			numberTest2.normalComparisonNumber = 10;

			NumberTest numberTest3 = new NumberTest();
			numberTest3.nextSlideIndexIfTrue = 830;
			numberTest3.type = NumberTest.TypeOfTest.BETWEEN;
			numberTest3.betweenHighNumber = 20;
			numberTest3.betweenLowNumber = 11;

			NumberTest numberTest4 = new NumberTest();
			numberTest4.type = NumberTest.TypeOfTest.GREATER_THAN;
			numberTest4.nextSlideIndexIfTrue = 840;
			numberTest4.normalComparisonNumber = 20;

			quizSlider.tests = new NumberTest[] { numberTest1, numberTest2, numberTest3, numberTest4 };

			Choice choice1 = new Choice();
			choice1.choice = "Chocolate is better.";

			Choice choice2 = new Choice();
			choice2.choice = "Vanilla is better.";

			quizSlider.choices = new Choice[] { choice1, choice2 };

			sliderQuizzes.Add(quizSlider);
		}

		return sliderQuizzes.ToArray();
	}

}
