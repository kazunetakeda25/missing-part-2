using UnityEngine;
using System.Collections;

public class QuizDataFaker : MonoBehaviour {

	private void Awake()
	{
		if(Debug.isDebugBuild == false)
			Destroy(this.gameObject);
		FakeValues();
	}

	private void Start()
	{

	}

	private void FakeValues()
	{
		SessionManager.Instance.SaveQuizAnswer(Quiz.GAME_1_2_AVERAGE_FACEBOOK_USE, 1);
		SessionManager.Instance.SaveQuizAnswer(Quiz.GAME_1_2A_AVERAGE_FACEBOOK_USE_SLIDER, 12.0f);
		SessionManager.Instance.SaveQuizAnswer(Quiz.GAME_1_4_FLAHERTY_RACKETEERING, 1);
		SessionManager.Instance.SaveQuizAnswer(Quiz.GAME_1_6_PARKS, 1);
		SessionManager.Instance.SaveQuizAnswer(Quiz.GAME_1_6A_PARKS_SLIDER, 88.0f);
		SessionManager.Instance.SaveQuizAnswer(Quiz.GAME_2_1A_STOCK_PRICE, 3);
		SessionManager.Instance.SaveQuizAnswer(Quiz.GAME_2_4_NYC_POPULATION, 1);
		SessionManager.Instance.SaveQuizAnswer(Quiz.GAME_2_4B_MARLIN_LENGTH, 18.0f);
		SessionManager.Instance.SaveQuizAnswer(Quiz.GAME_2_HUNTING_ME, 1.0f);
		SessionManager.Instance.SaveQuizAnswer(Quiz.GAME_2_HUNTING_THEM, 1.0f);
		SessionManager.Instance.SaveQuizAnswer(Quiz.GAME_3_3_PALLET_COUNT, 1);
		SessionManager.Instance.SaveQuizAnswer(Quiz.GAME_3_4_AMMO_COUNT, 0999.0f);
		SessionManager.Instance.SaveQuizAnswer(Quiz.GAME_3_8_TRAVIS_TRIAL, 4);
	}
	
}
