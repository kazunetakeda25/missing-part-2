using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class AARDriver : MonoBehaviour 
{
	private const string MEDAL_STRING = "Your score of {0} against a median score of {1} has earned you {2} medal for this episode.";
	private const string MEDAL_CULM_STRING = "\n\nThis brings your total score to {0} compared to a median score of {1}.";
	private const string EMPTY_MEDAL = "EMPTY_MEDAL";

	[SerializeField] private AAR2Gen aar;
	[SerializeField] private AARNGUI gui;

	private AAR2Gen.AARNode currentNode;
	private int currentAAR;

	public void DisplaySlide(AAR2Gen.AARNode node) 
	{
		currentNode = node;

		switch(node.slideType) {
		case AARSlideType.Basic:
		case AARSlideType.BasicImage:
			RunBasicSlide();
			break;
		case AARSlideType.Movie:
			RunMovieSlide();
			break;
		case AARSlideType.QuizRadios:
		case AARSlideType.QuizRadiosImage:
			RunRadiosQuizSlide();
			break;
		case AARSlideType.QuizSlider:
		case AARSlideType.QuizSliderImage:
			RunSliderQuizSlide();
			break;
		case AARSlideType.Spinner:
			RunSpinnerSlide();
			break;
		}
	}

	private void RunSpinnerSlide()
	{
		Debug.Log("Setting Up Spinner Slide");

		gui.SetupBasicSlide(currentNode.image, currentNode.narrator);
		gui.BodyLabel.text = currentNode.parsedBody;
		gui.wheel.BringInWheel(OnSlideDisplayDone);

		if(currentNode.audioFile != "" || currentNode.audioFile.Length > 0) {
			gui.PlayAudio(currentNode.audioFile, true);
		}
	}

	private void RunBasicSlide()
	{
		Debug.Log("Setting Up Basic Slide: ");

		if(currentNode.parsedBody.Length == 0) {
			Debug.Log("No Text here, go ahead and skip!");
			aar.NextSlide(DetermineNextLink());
			return;
		}

		gui.SetupBasicSlide(currentNode.image, currentNode.narrator);
		SetupBodyAndTitle();

		if(currentNode.audioFile != "" || currentNode.audioFile.Length > 0) {
			gui.PlayAudio(currentNode.audioFile, true);
		} else {
			if(	SlideTypeRequiresUserInput(currentNode.slideType) == false) {
				gui.ShowNextButton();
			}
		}

		if(currentNode.fireworks == true) {
			gui.PlayFireworks();
			SessionManager.Instance.RewardPoints();
			//Currently we compare against fixed Median value, so we don't need to register false answers
			//We will probably have to add a new tag to the CM file in the future to track this.
			BadgeTracker.Instance.RegisterBadgeAnswer(aar.GetConversationID(), true);
		}

		gui.onNextHit += OnSlideDisplayDone;
	}

	private bool SlideTypeRequiresUserInput(AARSlideType type)
	{
		switch(type) {
		case AARSlideType.Basic:
		case AARSlideType.BasicImage:
		case AARSlideType.Movie:
		case AARSlideType.Spinner:
			return false;
		}

		return true;
	}

	private void SetupBodyAndTitle()
	{
		string titleString = GetFormattedString(currentNode.parsedTitle, currentNode.titleVars);
		gui.SetTitle(titleString);

		string bodyString = GetFormattedString(currentNode.parsedBody, currentNode.bodyVars);
		gui.BodyLabel.text = bodyString;
	}

	private string GetFormattedString(string baseString, string[] vars) 
	{
		string[] stringVarValues = new string[vars.Length];

		for(int i = 0; i < vars.Length; i++) {
			stringVarValues[i] = SessionManager.Instance.RetrieveCleanStringQuizAnswer(vars[i]);
		}

		return string.Format(baseString, stringVarValues);
	}

	private void RunMovieSlide()
	{
		//Debug.Log("Setting up Movie: " + currentNode.video);
		currentNode = currentNode;

		List<string> moviePaths = new List<string>();

		moviePaths.Add(currentNode.video);

		gui.PlayMovie(moviePaths.ToArray());
		gui.onNextHit += OnSlideDisplayDone;
	}

	private void RunRadiosQuizSlide()
	{
		//Debug.Log("Setting Up Radios");

		List<string> radioLabels = new List<string>();

		for(int i = 0; i < currentNode.outgoingLinks.Length; i++) {
			radioLabels.Add(currentNode.outgoingLinks[i].text);
		}

		gui.SetupBasicSlide(currentNode.image, currentNode.narrator);
		gui.SetupRadios(radioLabels.ToArray());
		SetupBodyAndTitle();
		gui.PlayAudio(currentNode.audioFile, false);

		gui.onNextHit += ListenForRadioDone;
	}

	private void RunSliderQuizSlide()
	{
		Debug.Log("Setting Up Slider");

		gui.SetupBasicSlide(currentNode.image, currentNode.narrator);
		gui.SetupSlider(currentNode.quizSliderRanges[0], currentNode.quizSliderRanges[2], currentNode.quizSliderRanges[1]);
		SetupBodyAndTitle();
		gui.PlayAudio(currentNode.audioFile, false);

		gui.onNextHit += ListenForSliderDone;
	}

	public void PrepareMedalsSlide()
	{
		ShowBadgePresentation();
	}

	public void ListenForRadioDone()
	{
		int answer = gui.CurrentRadioIndex;
		SessionManager.Instance.SaveQuizAnswer(currentNode.quizVariable, answer);
		gui.onNextHit -= ListenForRadioDone;
		aar.NextSlide(DetermineNextLink());
	}

	public void ListenForSliderDone()
	{
		float answer = gui.GetSliderValueAdjusted();
		SessionManager.Instance.SaveQuizAnswer(currentNode.quizVariable, answer);
		gui.onNextHit -= ListenForSliderDone;
		aar.NextSlide(DetermineNextLink());
	}

	public void OnSlideDisplayDone()
	{
		gui.onNextHit -= OnSlideDisplayDone;
		aar.NextSlide(DetermineNextLink());
	}

	private int DetermineNextLink()
	{
		if(currentNode.outgoingLinks.Length == 0) {
			//No Outgoing Links, end AAR.
			return -1;
		}

		if(currentNode.slideType == AARSlideType.QuizRadios || currentNode.slideType == AARSlideType.QuizRadiosImage) {
			AAR2Gen.AARNode linkNode = aar.GetNodeByID(currentNode.outgoingLinks[gui.CurrentRadioIndex].link);
			return linkNode.outgoingLinks[0].link;
		}

		if(currentNode.outgoingLinks.Length == 1) {
			return currentNode.outgoingLinks[0].link;
		}

		AAR2Gen.AARNode.OutgoingLink[] outgoingLinks = currentNode.outgoingLinks;
		var destinationNodes = new List<AAR2Gen.AARNode>();

		for(int i = 0; i < outgoingLinks.Length; i++) {
			destinationNodes.Add(aar.GetNodeByID(outgoingLinks[i].link));
			//Debug.Log("Adding Destination Node: " + destinationNodes[i].ID);
		}

		destinationNodes.Sort((x, y) => x.conditionPriority.CompareTo(y.conditionPriority));
		destinationNodes.Reverse();

		for(int i = 0; i < destinationNodes.Count; i++) {

			if(destinationNodes[i].myConditions == null) {
				//There are no conditions, so we definitely want this one!
				return destinationNodes[i].ID;
			}

			bool allValuesAreTrue = false;

			foreach(AAR2Gen.AARNode.Condition condition in destinationNodes[i].myConditions) {
				int l;
				bool isLeftNumeric = int.TryParse(condition.left, out l);

				if(isLeftNumeric == false) {
					Debug.Log("Left Side is *not* an integer.  Retrieving Quiz Variable.");
					string answerNumber = SessionManager.Instance.RetrieveStringQuizAnswer(condition.left);
					float parsedAnswerFloat;
					float.TryParse(answerNumber, out parsedAnswerFloat);
					l = Mathf.RoundToInt(parsedAnswerFloat);
					Debug.Log("Left Side is now parsed to: " + l.ToString());
				}

				int r;
				bool isRightnumeric = int.TryParse(condition.right, out r);

				if(isRightnumeric == false) {
					Debug.Log("Right Side is *not* an integer.  Retrieving Quiz Variable.");
					string answerNumber = SessionManager.Instance.RetrieveStringQuizAnswer(condition.right);
					float parsedAnswerFloat;
					float.TryParse(answerNumber, out parsedAnswerFloat);
					r = Mathf.RoundToInt(parsedAnswerFloat);
					Debug.Log("Right Side is now parsed to: " + r.ToString());
				}

				//Debug.Log("Left: " + l);
				//Debug.Log("Right: " + r);

				//This is inverted for some reason.
				if(condition.condition(l, r) == true) {
					//Debug.Log("Condition: is true.");
					allValuesAreTrue = true;
				} else {
					//Debug.Log("Condition: is false.");
					allValuesAreTrue = false;
				}
			}

			if(allValuesAreTrue == true)
				return destinationNodes[i].ID;
		}

		Debug.LogError("Invalid Value!!");
		return 0;
	}

	private void ShowBadgePresentation()
	{

		if(Settings.Instance.Scoring == false) {
			Debug.Log("Ending AAR...");
			aar.WrapAAR();
			return;
		}

		Debug.Log("Setting Medals Page");
		gui.SetupBasicSlide("Images/Blank", Narrator.Generic);

		BadgeTracker.BadgeScore score;

		switch(aar.GetConversationID()) {
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

//		if(score.correctAnswers == 0)
//		{
//			aar.WrapAAR();
//			return;
//		}

		int epScore = score.correctAnswers * 100;
		int medianScore = BadgeTracker.GetFixedEpisodeMedian(aar.GetConversationID());
		string medalString = BadgeTracker.GetMedalString(score);

		int totalMedianScore = BadgeTracker.GetCulmulativeMedian(aar.GetConversationID());
		int totalScore = GetTotalScore(aar.GetConversationID());

		gui.SetTitle(GetEpisodeResultsString());

		gui.BodyLabel.text = String.Format(MEDAL_STRING, epScore, medianScore, medalString);
		if(aar.GetConversationID() > 1) {
			gui.BodyLabel.text += String.Format(MEDAL_CULM_STRING, totalScore, totalMedianScore);
		}

		string ep1BadgeString = BadgeTracker.GetBadgeSpriteString(BadgeTracker.Instance.GetBadgeScore(BadgeTracker.Badge.EP1));
		string ep2BadgeString = BadgeTracker.GetBadgeSpriteString(BadgeTracker.Instance.GetBadgeScore(BadgeTracker.Badge.EP2));
		string ep3BadgeString = BadgeTracker.GetBadgeSpriteString(BadgeTracker.Instance.GetBadgeScore(BadgeTracker.Badge.EP3));

		switch(aar.GetConversationID()) {
		case 1:
			gui.medal1.spriteName = ep1BadgeString;
			gui.medal2.spriteName = EMPTY_MEDAL;
			gui.medal3.spriteName = EMPTY_MEDAL;
			break;
		case 2:
			gui.medal1.spriteName = ep1BadgeString;
			gui.medal2.spriteName = ep2BadgeString;
			gui.medal3.spriteName = EMPTY_MEDAL;
			break;
		case 3:
			gui.medal1.spriteName = ep1BadgeString;
			gui.medal2.spriteName = ep2BadgeString;
			gui.medal3.spriteName = ep3BadgeString;
			break;
		}

		gui.medal1.GetComponent<AudioSource>().Play();

		ReportEvent.ReportMedalEarned(score);
		gui.ShowMedals();
		gui.ShowNextButton();
		gui.onNextHit += OnSlideMedalsComplete;
	}

	private string GetEpisodeResultsString()
	{
		switch(aar.GetConversationID()) {
		case 1:
			return "Episode One Results";
		case 2:
			return "Episode Two Results";
		case 3:
			return "Episode Three Results";
		}

		return "Bad Title";
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

	public void OnSlideMedalsComplete()
	{
		gui.onNextHit -= OnSlideMedalsComplete;
		if(MissingComplete.SaveGameManager.Instance != null) {
			MissingComplete.SaveGameManager.Instance.GetCurrentSaveGame().aarCheckpoint = 0;
		}
		aar.WrapAAR();
	}

}
