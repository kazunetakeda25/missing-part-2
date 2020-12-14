/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"ActionTimescale.cs"
 * 
 *	This action modifies the speed at which the game runs at.
 *	It can be used for slow-motion effects during both cutscenes and gameplay.
 * 
 */

using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	[System.Serializable]
	public class ActionTimescale : Action
	{
		
		public float timeScale;
		public bool useTimeCurve = false;
		public AnimationCurve timeCurve;
		
		
		public ActionTimescale ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Engine;
			title = "Change timescale";
			description = "Changes the timescale to a value between 0 and 1. This allows for slow-motion effects.";
		}
		
		
		override public float Run ()
		{
			if (!isRunning)
			{
				isRunning = true;

				if (useTimeCurve)
				{
					if (timeCurve != null)
					{
						KickStarter.playerInput.SetTimeCurve (timeCurve);
						if (willWait)
						{
							return defaultPauseTime;
						}
					}
				}
				else if (timeScale > 0f)
				{
					KickStarter.playerInput.SetTimeScale (timeScale);
				}
			}
			else
			{
				if (KickStarter.playerInput.HasTimeCurve ())
				{
					return defaultPauseTime;
				}
				else
				{
					isRunning = false;
				}
			}
			return 0f;
		}

		
		#if UNITY_EDITOR

		override public void ShowGUI ()
		{
			useTimeCurve = EditorGUILayout.Toggle ("Use time curve?", useTimeCurve);
			if (useTimeCurve)
			{
				if (timeCurve == null)
				{
					timeCurve = AnimationCurve.Linear (0f, 0.1f, 1f, 1f);
				}

				timeCurve = EditorGUILayout.CurveField ("Time curve:", timeCurve);
				willWait = EditorGUILayout.Toggle ("Wait until finish?", willWait);
			}
			else
			{
				timeScale = EditorGUILayout.Slider ("Timescale:", timeScale, 0f, 1f);
			}
			
			AfterRunningOption ();
		}
		
		#endif

	}

}