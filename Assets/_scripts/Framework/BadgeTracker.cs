using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

public class BadgeTracker 
{
	private const string GOLD_MEDAL = "_GOLD";
	private const string SILVER_MEDAL = "_SILVER";
	private const string BRONZE_MEDAL = "_BRONZE";
	private const string NO_MEDAL = "_NO_MEDAL";

	private const int Ep1Median = 600;
	private const int Ep2Median = 400;
	private const int Ep3Median = 500;

	public enum Badge
	{
		NONE,
		EP1,
		EP2,
		EP3
	}

	public enum Medal
	{
		NO_MEDAL,
		BRONZE,
		SILVER,
		GOLD
	}

	
	public class BadgeScore
	{
		public Badge badge;
		public int correctAnswers = 0;
		public int totalAnswers = 0;

		private int calculatedMedian;
		public int CalculatedMedian {
			get { 
				if(totalAnswers == 0)
					return 0;

				int medianScore = Mathf.FloorToInt(0.75f * totalAnswers);
				calculatedMedian = medianScore * 100;

				return calculatedMedian;
			}
		}
	}

	//Static Helpers

	public static int GetFixedEpisodeMedian(int aar)
	{
		switch(aar)
		{
		case 1:
			return Ep1Median;
		case 2:
			return Ep2Median;
		case 3:
			return Ep3Median;
		}
		Debug.LogError ("Invalid AAR Passed!");
		return 0;
	}

	public static int GetCulmulativeMedian(int aar)
	{
		switch(aar)
		{
		case 1:
			return (Ep1Median);
		case 2:
			return (Ep1Median + Ep2Median);
		case 3:
			return (Ep1Median + Ep2Median + Ep3Median);
		}
		Debug.LogError ("Invalid AAR Passed!");
		return 0;
	}

	public static Medal GetMedal(BadgeScore score)
	{
		Debug.Log("Checking for Medal " + score.badge.ToString() + " - Score: " + score.correctAnswers);

		if(score.correctAnswers == 0)
			return Medal.NO_MEDAL;

		switch(score.badge) {
		case Badge.EP1:
			if(score.correctAnswers > (Ep1Median / 100))
				return Medal.GOLD;

			if(score.correctAnswers > 3)
				return Medal.SILVER;

			if(score.correctAnswers > 0)
				return Medal.BRONZE;

			return Medal.NO_MEDAL;
		case  Badge.EP2:
			if(score.correctAnswers > (Ep2Median / 100))
				return Medal.GOLD;

			if(score.correctAnswers > 2)
				return Medal.SILVER;

			if(score.correctAnswers > 0)
				return Medal.BRONZE;

			return Medal.NO_MEDAL;
		case Badge.EP3:
			if(score.correctAnswers > (Ep3Median / 100))
				return Medal.GOLD;

			if(score.correctAnswers > 2)
				return Medal.SILVER;

			if(score.correctAnswers > 0)
				return Medal.BRONZE;

			return Medal.NO_MEDAL;
		}

//		float percentageCorrect = (float) score.correctAnswers / (float) score.totalAnswers;
//		
//		Debug.Log ("Percentage of Questions Correct: " + percentageCorrect);
//		
//		if(percentageCorrect > 0.0f && percentageCorrect <= 0.40f)
//			return Medal.BRONZE;
//		
//		if(percentageCorrect > 0.40f && percentageCorrect <= 0.75f)
//			return Medal.SILVER;
		
		Debug.LogError("Invalid Medal!");
		return Medal.GOLD;
	}

	public static string GetMedalString(BadgeScore score)
	{
		switch(GetMedal(score)) {
		case Medal.BRONZE:
			return "a Bronze";
		case Medal.SILVER:
			return "a Silver";
		case Medal.GOLD:
			return "a Gold";
		}

		return "no";
	}

	public static string GetBadgeSpriteString(BadgeScore badgeScore)
	{
		string sprite = "";

		if(GetMedal(badgeScore) == Medal.NO_MEDAL) {
			return "EMPTY_MEDAL";
		}

		switch(badgeScore.badge) {
		case Badge.EP1:
			sprite += Badge.EP1.ToString();
			break;
		case Badge.EP2:
			sprite += Badge.EP2.ToString();
			break;
		case Badge.EP3: 
			sprite += Badge.EP3.ToString();
			break;
		}
		
		switch(GetMedal(badgeScore)) {
		case Medal.NO_MEDAL:
			sprite += NO_MEDAL;
			break;
		case Medal.BRONZE:
			sprite += BRONZE_MEDAL;
			break;
		case Medal.SILVER:
			sprite += SILVER_MEDAL;
			break;
		case Medal.GOLD:
			sprite += GOLD_MEDAL;
			break;
		}
		
		Debug.Log ("Picking Sprite: " + sprite);
		return sprite;
	}

	//Singleton for tracking Scores

	private static BadgeTracker instance;
	public static BadgeTracker Instance
	{
		get {
			if(instance == null) { 
				instance = new BadgeTracker();
				instance.badgeScores = InitializeBadgeScores();
			}
			return instance;
		}
	}
	
	protected List<BadgeScore> badgeScores;
	private static List<BadgeScore> InitializeBadgeScores() {

		List<BadgeScore> retBadgeScores = new List<BadgeScore>();

		for (int i = 0; i < Enum.GetNames(typeof(Badge)).Length; i++) {
			BadgeScore badgeScore = new BadgeScore();
			badgeScore.badge = (Badge) i;
			retBadgeScores.Add(badgeScore);
		}

		return retBadgeScores;
	}

	public string ExportBadgeScores()
	{
		string json =  JsonConvert.SerializeObject(badgeScores);
		Debug.Log("Exporting JSON: " + json);
		return json;
	}

	public void LoadBadgeScores(string json)
	{
		Debug.Log("Loading Badge Scores: " + json);
		if(json.Length == 0) {
			InitializeBadgeScores();
		} else {
			badgeScores = JsonConvert.DeserializeObject<List<BadgeScore>>(json);
		}
	}

	public void ResetBadgeScores()
	{
		badgeScores = InitializeBadgeScores();
	}

	public void RegisterBadgeAnswer(int aar, bool correct)
	{
		Debug.Log ("Correct Answer Recorded!" + aar);
		//Hardcoded for now due to time...
		BadgeScore score = badgeScores[aar];
		score.totalAnswers++;

		if(correct)
			score.correctAnswers++;

		Debug.Log (score.badge.ToString() + " Score: " + score.totalAnswers + " | " + score.correctAnswers);

		//TODO: Report to XML here.
		Debug.Log(ExportBadgeScores());
	}

	public BadgeScore GetBadgeScore(Badge badge) 
	{
		BadgeScore retScore = new BadgeScore();

		foreach(BadgeScore badgeScore in badgeScores) { 
			if(badgeScore.badge == badge) {
				retScore = badgeScore;
				continue;
			}
		}

		return retScore;
	}

}
 