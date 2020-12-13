using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AAR2Gen))]
public class AARDebugger : MonoBehaviour 
{
	private AAR2Gen aar;
	public AARNGUI gui;

	[System.Serializable]
	public class DebugQuiz
	{
		public Quiz quiz;
		public QuizType type;
		public int minVal;
		public int maxVal;
		public int startVal;
	}

	public DebugQuiz[] quizzes;

	private int currentDebugIndex;
	private SessionManager sessionManager;

	private void Awake()
	{
		aar = this.gameObject.GetComponent<AAR2Gen>();
		sessionManager = SessionManager.Instance;
	}

	public void GatherDebugInfo()
	{
		currentDebugIndex = 0;
		CheckScoreToggle();
	}

	private void CheckScoreToggle()
	{
		gui.SetupBasicSlide("Images/PlaceholderImage", Narrator.Generic);
		gui.BodyLabel.text = "Set Scoring";
		gui.SetupRadios(new string[]{ "on", "off" });
		gui.onNextHit += OnScoreSet;
	}

	private void OnScoreSet()
	{
		gui.onNextHit -= OnScoreSet;
		bool scoring = gui.CurrentRadioIndex == 0 ? true : false;

		//Settings.Instance.SetGameIVs(false, false, false, scoring);
		RunDebugSlide();
	}

	public void OnSkipAllRandomValues()
	{
		foreach(DebugQuiz quiz in quizzes) {

			int randomVal = 0;

			if(quiz.type == QuizType.Slider) {
				randomVal = Random.Range(quiz.minVal, quiz.maxVal);
			}

			if(quiz.type == QuizType.MultipleChoice) {
				int length = QuizDataInterperter.RetrieveAnswerStrings(quiz.quiz).Length;
				randomVal = Random.Range(0, length);
			}

			SessionManager.Instance.SaveQuizAnswer(quiz.quiz, randomVal);
		}

		DebugInfoDone();
	}

	public void OnSkipAll1Values()
	{
		foreach(DebugQuiz quiz in quizzes) {
			SessionManager.Instance.SaveQuizAnswer(quiz.quiz, 1);
		}

		DebugInfoDone();
	}

	private void SetQuizVal(Quiz quiz, float val)
	{
		SessionManager.Instance.SaveQuizAnswer(quiz, val);
	}

	private void SetQuizVal(Quiz quiz, int val)
	{
		SessionManager.Instance.SaveQuizAnswer(quiz, val);
	}

	private void RunDebugSlide()
	{
		switch(quizzes[currentDebugIndex].type) {
		case QuizType.MultipleChoice:
			SetupForMultipleChoice();
			break;
		case QuizType.Slider:
			SetupForSlider();
			break;
		case QuizType.TrueFalse:
			SetupForBool();
			break;
		}
	}

	private void SetupForBool()
	{
		//Not used
	}

	private void SetupForMultipleChoice()
	{
		DebugQuiz currentQuiz = quizzes[currentDebugIndex];
		Debug.Log(gui.name);
		gui.SetupBasicSlide("Images/PlaceholderImage", Narrator.Represenative);
		gui.SetTitle("VARIABLE " + currentDebugIndex.ToString());
		gui.BodyLabel.text = currentQuiz.quiz.ToString();
		gui.SetupRadios(QuizDataInterperter.RetrieveAnswerStrings(currentQuiz.quiz));
		gui.onNextHit += RadioListenForNextButton;
	}

	private void RadioListenForNextButton()
	{
		Debug.Log("Radio Next Button Hit");
		gui.onNextHit -= RadioListenForNextButton;
		SetQuizVal(quizzes[currentDebugIndex].quiz, gui.CurrentRadioIndex);

		if((currentDebugIndex + 1) >= quizzes.Length) {
			aar.InitializeAAR();
			return;
		}

		currentDebugIndex++;
		RunDebugSlide();
	}

	private void SetupForSlider()
	{
		DebugQuiz currentQuiz = quizzes[currentDebugIndex];
		gui.SetupBasicSlide("Images/PlaceholderImage", Narrator.Generic);
		gui.SetTitle("VARIABLE " + currentDebugIndex.ToString());
		gui.BodyLabel.text = currentQuiz.quiz.ToString();
		gui.SetupSlider(currentQuiz.minVal, currentQuiz.maxVal, currentQuiz.startVal);
		gui.onNextHit += SliderListenForNextButton;
	}

	private void SliderListenForNextButton()
	{

		Debug.Log("Slider Next Button Hit");
		gui.onNextHit -= SliderListenForNextButton;
		float sliderVal = gui.GetSliderValueAdjusted();
		SetQuizVal(quizzes[currentDebugIndex].quiz, sliderVal);

		if((currentDebugIndex + 1) >= quizzes.Length) {
			aar.InitializeAAR();
			return;
		}

		currentDebugIndex++;
		RunDebugSlide();
	}

	private void NextButtonHit()
	{
		//Store data on current slide.

		currentDebugIndex++;
		RunDebugSlide();
	}

	public void DebugInfoDone()
	{
		gui.ClearGUI();
		aar.InitializeAAR();
	}

}
