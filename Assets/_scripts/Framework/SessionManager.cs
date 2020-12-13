using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

public class SessionManager : MonoBehaviour
{
	public class Candidate
	{
		public string subjectID;
		public Sex sex;
		public int episode4Score = 0;
		public int episode5Score = 0;
		public int episode6Score = 0;
		public int totalScore = 0;
		public Episode currentEpisode = Episode.NONE;
	}

	private static SessionManager instance;
	public static SessionManager Instance
	{
		get 
		{ 
			if (instance == null)
			{
				GameObject sessionManagerGO = new GameObject("Session Manager");
				instance = sessionManagerGO.AddComponent<SessionManager>();
				instance.Init();
			}
			return instance; 
		}
	}


	private Candidate candidate;
	private ScoreKeeper scoreKeeper;
	private SceneNavigator sceneNavigator;

	private bool hasInit = false;
	public bool HasInit { get { return hasInit; } }

	private void Init()
	{
		scoreKeeper = new ScoreKeeper();
		sceneNavigator = new SceneNavigator();
		hasInit = true;
	}

	public void StartNewSession(string subjectID, Sex sex)
	{
		candidate = new Candidate();
		candidate.sex = sex;
		candidate.subjectID = subjectID;

		SessionDataManager.Instance.BeginSession(candidate);
	}

	public class SerializedGameData
	{
		public Candidate candidate;
		public ScoreKeeper.QuizAnswerBool[] quizBools;
		public ScoreKeeper.QuizAnswerMultipleChoice[] quizMCs;
		public ScoreKeeper.QuizAnswerSlider[] quizSliders;
		public int playerScore;
	}

	//Quiz Interaction
	public void SaveQuizAnswer(string quiz, int value) { scoreKeeper.StoreQuizAnswer(quiz, value); }
	public void SaveQuizAnswer(string quiz, float value) { scoreKeeper.StoreQuizAnswer(quiz, value); }
	public void SaveQuizAnswer(string quiz, bool value) { scoreKeeper.StoreQuizAnswer(quiz, value); }

	public void SaveQuizAnswer(Quiz quiz, int value) { scoreKeeper.StoreQuizAnswer(quiz, value); }
	public void SaveQuizAnswer(Quiz quiz, float value) { scoreKeeper.StoreQuizAnswer(quiz, value); }
	public void SaveQuizAnswer(Quiz quiz, bool value) { scoreKeeper.StoreQuizAnswer(quiz, value); }

	public float RetrieveSliderAnswer(Quiz quiz) { return scoreKeeper.RetrieveFloatAnswer(quiz); }
	public int RetrieveMultipleChoiceAnswer(Quiz quiz) { return scoreKeeper.RetrieveMultipleChoiceAnswer(quiz); }
	public bool RetrieveBoolAnswer(Quiz quiz) { return scoreKeeper.RetrieveBoolAnswer(quiz); }

	public string RetrieveStringQuizAnswer(string quiz) { return scoreKeeper.RetrieveAnswerWithString(quiz); }
	public string RetrieveCleanStringQuizAnswer(string quiz) { return scoreKeeper.RetrieveCleanAnswerString(quiz); }

	public List<ScoreKeeper.QuizAnswer> RetrieveStoredQuizAnswers() { return scoreKeeper.StoredQuizAnswers; }

	//Scene Interaction
	public int GetScore() { return scoreKeeper.Score; }
	//TODO: 
	public void SetScore(int savedScore) { scoreKeeper.SetScore(savedScore); }

	public void RewardPoints() { scoreKeeper.RewardPoints(); }

	public void SetStartScene(Episode ep) { sceneNavigator.SetCurrentScene(ep); } 
	public void GotoNextLevel() { sceneNavigator.GotoNextScene(); }
	public void TransitionComplete() { sceneNavigator.TransitionComplete(); }

	public string GetGameDataJSON()
	{
		string sessionJSON = "";

		SerializedGameData gameData = new SerializedGameData();
		gameData.candidate = candidate;
		gameData.playerScore = GetScore();
		ScoreKeeper.QuizAnswer[] answers = RetrieveStoredQuizAnswers().ToArray();

		var bools = new List<ScoreKeeper.QuizAnswerBool>();
		var mcs = new List<ScoreKeeper.QuizAnswerMultipleChoice>();
		var sliders = new List<ScoreKeeper.QuizAnswerSlider>();

		for(int i = 0; i < answers.Length; i++) {

			if(answers[i] is ScoreKeeper.QuizAnswerBool) {
				ScoreKeeper.QuizAnswerBool answer = answers[i] as ScoreKeeper.QuizAnswerBool;
				bools.Add(answer);
			}

			if(answers[i] is ScoreKeeper.QuizAnswerMultipleChoice) {
				ScoreKeeper.QuizAnswerMultipleChoice answer = answers[i] as ScoreKeeper.QuizAnswerMultipleChoice;
				mcs.Add(answer);
			}

			if(answers[i] is ScoreKeeper.QuizAnswerSlider) {
				ScoreKeeper.QuizAnswerSlider answer = answers[i] as ScoreKeeper.QuizAnswerSlider;
				sliders.Add(answer);
			}
		}

		gameData.quizBools = bools.ToArray();
		gameData.quizMCs = mcs.ToArray();
		gameData.quizSliders = sliders.ToArray();

		sessionJSON = JsonConvert.SerializeObject(gameData);

		return sessionJSON;
	}

	public void SetGameDataJSON(string json)
	{
		Debug.Log("Loading JSON: " + json);
		SerializedGameData gameData = JsonConvert.DeserializeObject<SerializedGameData>(json);
		candidate = gameData.candidate;

		if(gameData.quizBools != null) {
			foreach(ScoreKeeper.QuizAnswerBool answer in gameData.quizBools) {
				SaveQuizAnswer(answer.quiz, answer.answerBool);
			}
		}

		if(gameData.quizMCs != null) {
			foreach(ScoreKeeper.QuizAnswerMultipleChoice answer in gameData.quizMCs) {
				SaveQuizAnswer(answer.quiz, answer.multipleChoiceAnswer);
			}

		}

		if(gameData.quizSliders != null) {
			foreach(ScoreKeeper.QuizAnswerSlider answer in gameData.quizSliders) {
				SaveQuizAnswer(answer.quiz, answer.sliderValue);
			}
		}

		SetScore(gameData.playerScore);

		hasInit = true;
	}

	public void Reset()
	{
		hasInit = false;
		SetScore(0);
		candidate = null;
		Init();
	}

	private void Awake()
	{
		GameObject.DontDestroyOnLoad(this.gameObject);
	}

	private void OnApplicationQuit()
	{
		SessionDataManager.Instance.CloseSession();
	}
}
