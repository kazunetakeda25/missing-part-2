using UnityEngine;

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class DemoAchievementInfo
{

	public string AchievementName;
	public string Description;

	public bool Unlocked;

	public int Needed;
	public int Current;

	public float Progress 
	{ 
		get 
		{
			return (float)Current / (float)Needed; 
		} 
	}

	public string FormattedProgress
	{
		get
		{
			return string.Format( "{0} / {1}", Current, Needed );
		}
	}

}
