using UnityEngine;
using System.Collections;

namespace MissingComplete
{
	public class GameInitializer : MonoBehaviour 
	{

		//This script resets all variables to get them ready for a new or loaded game.

		private void Awake()
		{
			BadgeTracker.Instance.ResetBadgeScores();
		}
	}
}
