using UnityEngine;
using System.Collections;

public class MedalSetter : MonoBehaviour {

	public BadgeTracker.Badge badge;

	private void Start()
	{
		if(Settings.Instance.Scoring == false)
			GameObject.Destroy(this.gameObject);

		BadgeTracker.BadgeScore score = BadgeTracker.Instance.GetBadgeScore(badge);

//		score = new BadgeTracker.BadgeScore();
//		score.badge = BadgeTracker.Badge.EP1;
//		score.totalAnswers = 10;
//		score.correctAnswers = 2;

		string spriteString = BadgeTracker.GetBadgeSpriteString(score);

		dfSprite mySprite = this.gameObject.GetComponent<dfSprite>();

		if(spriteString != null)
			mySprite.SpriteName = spriteString;
	}

}
