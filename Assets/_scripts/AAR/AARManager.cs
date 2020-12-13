using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

[RequireComponent(typeof(AudioSource))]
public class AARManager : MonoBehaviour
{
	private const bool DEBUG = true;
	private const float DEBUG_SKIP_COOLDOWN = 0.2f;
	private const int MAX_CHAR_COUNT = 600;

	private const string BASE_IMG_PATH = "IMAGE/";
	private const string BASE_AUDIO_PATH = "AUDIO/";
	private const string BASE_GUI_PATH = "GUI/";
	private const string BASE_JSON_DATA = "JSON/";

	private const string BASE_AAR_PREFAB_PATH = "AAR Root";

	private const string MEDAL_STRING = "Your score of {0} against a median score of {1} has earned you a {2} medal for this episode.";
	private const string MEDAL_CULM_STRING = "\nThis brings your total score to {0} compared to a median score of {1}.";

	public int aar;
	public AARHistory history = new AARHistory();

	private List<AARSlide> slides;
	private int startIndex;

	private int autoFormatCount = 0;

	private AARRoot root;
	private AARSlide currentSlide;
	private GameObject aarContainer;
	private GameObject quizContainer;

	private List<dfButton> activeRadioButtons = new List<dfButton>();
	private int? selectedRadioButton = null;

	private AudioClip currentSoundClip;
	private bool audioPlayingForThisSlide = false;

	private bool showQuizResponseText = false;

	private float cooldownTimer;


	//Exposed Methods

	public void OnPrevButtonClicked() { }

	public void OnNextButtonClicked( dfControl control, dfMouseEventArgs mouseEvent ) 
	{
		Debug.Log ("On Next Button Clicked!  " + control.name + mouseEvent.Clicks);
		root.nextButton.IsEnabled = false;
		root.nextButton.IsVisible = false;
		root.nextButton.IsInteractive = false;

		ReportEvent.NextButtonClicked(currentSlide.slideIndex);

		root.radioContainer.IsEnabled = false;

		//Quiz Slider is a two-part question, so when the next button is hit, we want to show the Slider to allow for input.
		if(currentSlide is AARSlideQuizSlider)
		{
			if(selectedRadioButton != null)
				SavePreSliderRadioValue();

			ShowSlider();
			return;
		}

		if(showQuizResponseText)
		{
			NextSlide();
			return;
		}

		if(currentSlide is AARSlideQuizMultipleChoice)
		{
			ProcessMultipleChoiceAnswer();
			return;
		}

		ClearSlide();
		
		NextSlide();
	}

	public void OnSliderAcceptButtonHit(dfControl control, dfMouseEventArgs mouseEvent) 
	{
		AARSlideQuizSlider mySlide = currentSlide as AARSlideQuizSlider;
		SaveSliderValue();

		root.acceptButton.IsInteractive = false;
		root.slider.IsInteractive = false;
		showQuizResponseText = true;

		ShowSlide((int) NumberTester.GetNextSlide(mySlide));
	}

	public void NextSlide()
	{
		if(DEBUG) Debug.Log ("Showing Next Slide...");
		ShowSlide(currentSlide.nextSlideIndex);
	}

	public void OnRadioHit ( dfControl control, dfMouseEventArgs mouseEvent )
	{
		if(DEBUG) Debug.Log ("Radio - " + control.name + " hit!");

		switch(control.name)
		{
		case "radio0":
			selectedRadioButton = 0;
			break;
		case "radio1":
			selectedRadioButton = 1;
			break;
		case "radio2":
			selectedRadioButton = 2;
			break;
		case "radio3":
			selectedRadioButton = 3;
			break;
		case "radio4":
			selectedRadioButton = 4;
			break;
		}

		CreateNextButton();
	}

	private void Start() { StartAAREpisode(); }

	private void Update()
	{
		if(audioPlayingForThisSlide == true)
			CheckAudio();

		cooldownTimer -= Time.deltaTime;

		if(Debug.isDebugBuild && Input.GetMouseButtonUp(0) && cooldownTimer <= 0)
			this.GetComponent<AudioSource>().Stop();
	}

	private void CheckAudio()
	{
		if(this.GetComponent<AudioSource>().isPlaying == false)
		{
			GetComponent<AudioSource>().clip = null;
			audioPlayingForThisSlide = false;

			if(currentSlide is AARSlideQuizSlider)
				return;

			if(currentSlide is AARSlideQuizMultipleChoice)
				return;

			if(currentSlide.nextButtonRequired == false)
				NextSlide();
			else
				CreateNextButton();
		}
	}

	private void StartAAREpisode()
	{
		Debug.Log ("Start AAR Episode: " + aar);
		SlideContainer slideContainer = JsonConvert.DeserializeObject<SlideContainer>(FetchAARData());

		slides = AARSlideSorter.FilterContainer(slideContainer, aar);
		startIndex = AARSlideSorter.FindStartIndex(slides);

		CreateAARRoot();

		ShowSlide(startIndex);
	}

	private string FetchAARData()
	{

		switch(aar)
		{
		case 1:
			TextAsset json1 = Resources.Load (BASE_JSON_DATA + "AAR1Data") as TextAsset;
			return json1.text;
		case 2:
			TextAsset json2 = Resources.Load (BASE_JSON_DATA + "AAR2Data") as TextAsset;
			return json2.text;
		case 3:
			TextAsset json3 = Resources.Load (BASE_JSON_DATA + "AAR3Data") as TextAsset;
			return json3.text;
		}

		Debug.LogWarning ("Invalid AAR, loading AAR Placeholder");
		return AARJSONGenerator.JSON_EXAMPLE_OUTPUT;
	}

	private void ShowSlide(int index)
	{
		//-1 Means end the AAR
		if(index == -1)
		{
			EndAAR();
			return;
		}

		cooldownTimer = DEBUG_SKIP_COOLDOWN;
		history.GoForward(index);

		currentSlide = AARSlideSorter.FindSlide(slides, index);

		if(currentSlide is AARPathfinder)
		{
			ProcessPathfinder();
			return;
		}

		if(DEBUG) Debug.Log ("Showing Slide: " + currentSlide.slideIndex + " - with content: " + currentSlide.textContent);

		if(currentSlide is AARSlideMovie) {
			ShowMovie();
			return;
		}

		if(CreateNewSlideCheck() == true)
			ClearSlide();

		Debug.Log ("Current Badge: " + currentSlide.badge);
		if(currentSlide.badge != BadgeTracker.Badge.NONE) {
			BadgeTracker.Instance.RegisterBadgeAnswer(currentSlide.AAR, currentSlide.fireWorks);
		}

		if(currentSlide.fireWorks) {
			SessionManager.Instance.RewardPoints();

			if(Settings.Instance.Scoring) {
				foreach(ParticleSystem fireworkFX in root.fireworks) {
					if(fireworkFX.GetComponent<AudioSource>() != null)
						fireworkFX.GetComponent<AudioSource>().PlayDelayed(0.3f);

					fireworkFX.Play();
				}
			}
		}

		CreateTitle();

		if(currentSlide is AARSlideQuizMultipleChoice)
		{
			CreateMultipleChoiceQuiz();
			return;
		}

		if(currentSlide is AARSlideQuizSlider)
		{
			CreateSliderQuiz();
			return;
		}

		if(currentSlide is SpecialSlide)
		{
			ShowSpecialSlide();
			return;
		}

		CreateBasicTextPanel();
	}

	private bool CreateNewSlideCheck()
	{
		if(currentSlide is AARSlideQuizMultipleChoice)
			return true;

		if(currentSlide is AARSlideQuizSlider)
			return true;

		if(showQuizResponseText == true)
		{
			showQuizResponseText = false;
			return true;
		}

		if(autoFormatCount >= root.layoutLabel.Length)
		{
			autoFormatCount = 0;
			return true;
		}

		return false;
	}

	private void ClearSlide()
	{
		Debug.Log ("Clearing Slide");
		autoFormatCount = 0;
		activeRadioButtons.Clear();
		GameObject.Destroy(root.gameObject);
		CreateAARRoot();
	}

	private void CreateAARRoot()
	{
		GameObject aarRootGO = GameObject.Instantiate(Resources.Load (BASE_GUI_PATH + BASE_AAR_PREFAB_PATH)) as GameObject;
		root = aarRootGO.GetComponentInChildren<AARRoot>();
		aarContainer = root.aarRoot;
	}

	private void EndAAR()
	{
		history.ClearHistory();
		SessionManager.Instance.GotoNextLevel();
	}

	private void CreateTitle()
	{
		if(currentSlide.titleContent == null || 
		   currentSlide.titleContent == "")
			return;

		if(DEBUG) Debug.Log ("Create Title: " + currentSlide.titleContent);

		EnableDFControl(root.title);

		string formattedString = "";

		bool isMultipleChoice = QuizDataInterperter.IsMultipleChoice(currentSlide.answerVar);

		if(currentSlide.answerVar == Quiz.NONE) {
			formattedString = currentSlide.titleContent;
		} else if(isMultipleChoice) {
			Quiz myQuiz = currentSlide.answerVar;
			int myQuizAnswer =  SessionManager.Instance.RetrieveMultipleChoiceAnswer(myQuiz);
			string s = QuizDataInterperter.RetrieveAnswerString(myQuiz, myQuizAnswer);
			formattedString = String.Format(currentSlide.titleContent, s);
		} else {
			float i = SessionManager.Instance.RetrieveSliderAnswer(currentSlide.answerVar);
			formattedString = String.Format(currentSlide.titleContent, i);
		}


		
		root.title.Text = formattedString;
	}

	private void CreateBasicTextPanel()
	{
		Debug.Log("Creating Basic Text Slide: " + currentSlide.textContent);
		
		SetAudio();

		if(currentSlide.textContent.Length > MAX_CHAR_COUNT)
			Debug.LogError("Warning text content exceeds maximum character count! @Index: " + currentSlide.slideIndex);
	
		SetAutoLabel(currentSlide.textContent);

		//TypewriterEffect typeWriter = labelGO.AddComponent<TypewriterEffect>();
		//typeWriter.animationLength = this.audio.clip == null ? 0.01f : this.audio.clip.length;
	}

	private void SetAutoLabel(string textContent)
	{
		dfLabel label = root.layoutLabel[autoFormatCount];
		autoFormatCount ++;
		EnableDFControl(label);
		
		label.Text = textContent;
	}

	private void DisplayQuizResponseText()
	{
		showQuizResponseText = false;
		SetAudio();
		EnableDFControl(root.bottomText);
		root.bottomText.Text = currentSlide.textContent;

		//TypewriterEffect typeWriter = root.bottomText.gameObject.AddComponent<TypewriterEffect>();
		//typeWriter.animationLength = this.audio.clip == null ? 0.01f : this.audio.clip.length;
	}

	private void CreateImagePanel()
	{
		autoFormatCount ++;
		
		EnableDFControl(root.layoutImage);

		dfSprite sprite  = root.layoutImage;

		sprite.SpriteName = "interface_blank";

		NextSlide();
	}
	
	private void ShowMovie()
	{
		ClearSlide();

		AARSlideMovie movieSlide = currentSlide as AARSlideMovie;
		GameObject movieManagerGO = new GameObject ("AAR Movie Manager");
		AARMovieManager movieManager = movieManagerGO.AddComponent<AARMovieManager>();
		movieManager.Init(GatherAllSequentialMovies(), this);
	}

	private string[] GatherAllSequentialMovies()
	{
		List<string> movieTitles = new List<string>();

		bool nextSlideIsMovie = true;
		int lastIndex = 0;
		int testedIndex = currentSlide.slideIndex;

		while(nextSlideIsMovie == true) {
			AARSlide testedSlide = AARSlideSorter.FindSlide(slides, testedIndex);
			AARSlideMovie movieSlide = testedSlide as AARSlideMovie;
			movieTitles.Add(movieSlide.movieClip);
			Debug.Log ("Adding clip: " + movieSlide.movieClip + " to Q.");

			nextSlideIsMovie = IsNextSlideMovie(testedSlide.nextSlideIndex);
			lastIndex = testedSlide.slideIndex;
			testedIndex = testedSlide.nextSlideIndex;
		}

		currentSlide = AARSlideSorter.FindSlide(slides, lastIndex);

		return movieTitles.ToArray();
	}

	private bool IsNextSlideMovie(int nextSlide)
	{
		if(AARSlideSorter.FindSlide(slides, nextSlide) is AARSlideMovie)
			return true;

		return false;
	}

	private void CreateMultipleChoiceQuiz()
	{
		if(DEBUG) Debug.Log ("Creating Multiple Choice Quiz: " + currentSlide.textContent);

		SetAudio();

		AARSlideQuizMultipleChoice mySlide = currentSlide as AARSlideQuizMultipleChoice;
		CreateQuizContainer(mySlide.imagePath, mySlide.choices);
	}

	private void ProcessMultipleChoiceAnswer()
	{
		AARSlideQuizMultipleChoice mySlide = currentSlide as AARSlideQuizMultipleChoice;
		Debug.Log ("Processing Multiple Choice Answer: " + mySlide.choices[(int) selectedRadioButton].choice);
		Debug.Log (mySlide.quiz);
		SessionManager.Instance.SaveQuizAnswer(mySlide.quiz, (int) selectedRadioButton);
		showQuizResponseText = true;
		ShowSlide(mySlide.choices[(int) selectedRadioButton].nextIndexIfSelected);

		selectedRadioButton = null;
	}

	private void CreateSliderQuiz()
	{
		if(DEBUG) Debug.Log ("Creating Slider Quiz: " + currentSlide.textContent);
		AARSlideQuizSlider mySlide = currentSlide as AARSlideQuizSlider;
		CreateQuizContainer(mySlide.imagePath, mySlide.choices);

		SetAudio();
	}

	private void SavePreSliderRadioValue()
	{
		AARSlideQuizSlider mySlide = currentSlide as AARSlideQuizSlider;
		Debug.Log ("Saving Quiz: " + mySlide.quiz.ToString() + " value as: " + selectedRadioButton);
		SessionManager.Instance.SaveQuizAnswer(mySlide.quiz, (int) selectedRadioButton);

		selectedRadioButton = null;
	}

	private void ShowSlider()
	{
		Debug.Log ("Generating Slider Control!!");
		EnableDFControl(root.sliderPanel);

		AARSlideQuizSlider mySlide = currentSlide as AARSlideQuizSlider;
		if(mySlide.highAmount == mySlide.lowAmount){
			Debug.LogError("Sliders cannot have the same Max and Min Values!!  Cancelling Slider Creation.");
			return;
		}
		root.slider.MaxValue = mySlide.highAmount;
		root.slider.MinValue = mySlide.lowAmount;
		//root.slider.StepSize = (float) Mathf.CeilToInt(Mathf.Abs(mySlide.lowAmount - mySlide.highAmount) / 100);
		root.slider.StepSize = 1;

		root.sliderBehavior.ResetSlider();

		if(mySlide.SliderLabel != null)
			root.slideLabel.Text = mySlide.SliderLabel;
		else
			root.slideLabel.IsVisible = false;

		AddEventBindingToButton(root.acceptButton.gameObject, "OnSliderAcceptButtonHit");
		root.acceptButton.IsInteractive = false;
		root.acceptButton.IsEnabled = false;
		root.acceptButton.IsVisible = false;
	}

	private void SaveSliderValue()
	{
		AARSlideQuizSlider mySlide = currentSlide as AARSlideQuizSlider;
		Debug.Log ("Saving Slider Var in " + mySlide.answerVar.ToString());
		Debug.Log (" as " + root.slider.Value);
		SessionManager.Instance.SaveQuizAnswer(mySlide.answerVar, root.slider.Value);
	}

	private void CreateQuizContainer(string imagePath, Choice[] choices)
	{
		if(imagePath == null) imagePath = "";

		dfLabel myQuestionLabel = (imagePath.Length == 0) ? root.questionText : root.questionTextImage;
		EnableDFControl(myQuestionLabel);
		myQuestionLabel.Text = currentSlide.textContent;

		if(imagePath.Length > 0) {
			EnableDFControl(root.questionImage);
			root.questionImage.SpriteName = imagePath;
		}

		EnableDFControl(root.radioContainer);
		
		for (int i = 0; i < choices.Length; i++) {
			GameObject radioGO = GameObject.Instantiate(Resources.Load (BASE_GUI_PATH + "Btn_radio")) as GameObject;
			radioGO.transform.parent = root.radioContainer.transform;

			dfButton button = radioGO.GetComponentInChildren<dfButton>();
			radioGO.name = "radio" + i.ToString();
			activeRadioButtons.Add(button);
			
			dfLabel radioLabel = radioGO.GetComponentInChildren<dfLabel>();
			radioLabel.Text = choices[i].choice;
			
			AddEventBindingToButton(radioGO, "OnRadioHit");
		}
	}

	private void ProcessPathfinder()
	{
		AARPathfinder myPathfinder = currentSlide as AARPathfinder;
		int nextSlide = (int) NumberTester.GetNextSlide(myPathfinder);
		Debug.Log ("Pathfinder has determined that slide - " + nextSlide + " is next... Loading.");
		ShowSlide(nextSlide);
	}

	//GUI Tools

	private void CreateNextButton()
	{
		ReportEvent.NextButtonDisplayed(currentSlide.slideIndex);

		dfControl nextButtonToUse = root.nextButton;

		if(currentSlide is SpecialSlide)
		{
			SpecialSlide mySlide = currentSlide as SpecialSlide;
			if(mySlide.useUpperNextButton)
				nextButtonToUse = root.upperNextButton;
		}

		EnableDFControl(nextButtonToUse);
		AddEventBindingToButton(nextButtonToUse.gameObject, "OnNextButtonClicked");
	}

	private void SetAudio()
	{
		this.GetComponent<AudioSource>().clip = Resources.Load(BASE_AUDIO_PATH + currentSlide.slideIndex.ToString()) as AudioClip;
		Debug.Log ("Attempting to load: " + BASE_AUDIO_PATH + currentSlide.slideIndex.ToString());
		if(this.GetComponent<AudioSource>().clip == null)
			this.GetComponent<AudioSource>().clip = Resources.Load(BASE_AUDIO_PATH + "Placeholder") as AudioClip;
		
		audioPlayingForThisSlide = true;
		this.GetComponent<AudioSource>().Play();
	}

	private void AddEventBindingToButton(GameObject button, string methodToRun)
	{
		dfEventBinding eventBinding = button.gameObject.GetComponentInChildren<dfEventBinding>();
		eventBinding.Unbind();
		eventBinding.DataTarget = new dfComponentMemberInfo();
		eventBinding.DataSource = new dfComponentMemberInfo();
		
		eventBinding.DataSource.Component = button.GetComponentInChildren<dfControl>();
		eventBinding.DataSource.MemberName = "Click";
		
		eventBinding.DataTarget.Component = this;
		eventBinding.DataTarget.MemberName = methodToRun;
		eventBinding.Bind();
	}

	private void EnableDFControl(dfControl dfObject)
	{
		dfObject.IsEnabled = true;
		dfObject.IsVisible = true;
		dfObject.IsInteractive = true;
	}

	private void ShowSpecialSlide()
	{
		SpecialSlide mySlide = currentSlide as SpecialSlide;
		switch(mySlide.type)
		{
		case SpecialSlide.Type.FULL_IMAGE:
			ShowFullImageSlide(mySlide);
			break;
		case SpecialSlide.Type.SPINNER:
			ShowSpinnerSlide(mySlide);
			break;
		case SpecialSlide.Type.BADGE_PRESENTATION:
			ShowBadgePresentation(mySlide);
			break;
		}
		
	}

	private void ShowFullImageSlide(SpecialSlide slide)
	{
		Debug.Log ("Showing Full Slide Image!");
		root.fullImageSlide.bottomLabel.Text = slide.content[0];
		root.fullImageSlide.sprite.SpriteName = slide.imagePath;
		EnableDFControl(root.fullImageSlide.panel);

		SetAudio();
	}

	private void ShowBadgePresentation(SpecialSlide slide)
	{
		Debug.Log ("Showing Badge Presentation");

		if(Settings.Instance.Scoring == false) {
			root.title.Text = "";
			NextSlide();
			return;
		}

		//Player Score
		//Median Score
		//Badge Type

		BadgeTracker.BadgeScore score;
		
		switch(slide.AAR) {
		case 1:
			score = BadgeTracker.Instance.GetBadgeScore(BadgeTracker.Badge.EP1);
			break;
		case 2:
			score = BadgeTracker.Instance.GetBadgeScore(BadgeTracker.Badge.EP2);
			break;
		case 3:
			score = BadgeTracker.Instance.GetBadgeScore(BadgeTracker.Badge.EP3);
			break;
		default:
			Debug.LogError("Invalid AAR!");
			return;
		}

		//For Debug Testing Medal Screen
//		score = new BadgeTracker.BadgeScore();
//		score.badge = BadgeTracker.Badge.EP1;
//		score.correctAnswers = 8;
//		score.totalAnswers = 10;

		if(score.correctAnswers == 0)
		{
			root.title.Text = "";
			NextSlide();
			return;
		}
		int epScore = score.correctAnswers * 100;
		int medianScore = BadgeTracker.GetFixedEpisodeMedian(slide.AAR);
		string medalString = BadgeTracker.GetMedalString(score);

		int totalMedianScore = BadgeTracker.GetCulmulativeMedian(slide.AAR);
		int totalScore = GetTotalScore(slide.AAR);

		root.fullImageSlide.bottomLabel.Text = String.Format(MEDAL_STRING, epScore, medianScore, medalString);
		root.fullImageSlide.bottomLabel.Text += String.Format(MEDAL_CULM_STRING, totalScore, totalMedianScore);

		root.fullImageSlide.medalsSprite.SpriteName = BadgeTracker.GetBadgeSpriteString(score);
		EnableDFControl(root.fullImageSlide.panel);

		root.fullImageSlide.medalsSprite.GetComponent<AudioSource>().Play();

		ReportEvent.ReportMedalEarned(score);

		CreateNextButton();
	}

	private int GetTotalScore(int aar)
	{
		int totalScore = 0;

		if(aar >= 1)
			totalScore += BadgeTracker.Instance.GetBadgeScore(BadgeTracker.Badge.EP1).correctAnswers;

		if(aar >= 2)
			totalScore += BadgeTracker.Instance.GetBadgeScore(BadgeTracker.Badge.EP2).correctAnswers;

		if(aar >= 3)
			totalScore += BadgeTracker.Instance.GetBadgeScore(BadgeTracker.Badge.EP3).correctAnswers;

		return totalScore * 100;
	}

	private void ShowSpinnerSlide(SpecialSlide slide)
	{
		Debug.Log ("Showing Spinner Slide");
		if(slide.soundClip)
			SetAudio();

		SetAutoLabel(slide.content[0]);

		root.wheelSpinner.BringInWheel(SpinnerDoneSpinning);
	}

	public void SpinnerDoneSpinning()
	{
		CreateNextButton();
	}
}
