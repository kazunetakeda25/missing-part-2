/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"ActionTemplate.cs"
 * 
 *	This is a blank action template.
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
	public class ActionTemplate : Action
	{
		
		// Declare variables here
		
		
		public ActionTemplate ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Custom;
			title = "Template";
			description = "This is a blank Action template.";
		}
		
		
		override public float Run ()
		{
			/* 
			 * This function is called when the action is performed.
			 * 
			 * The float to return is the time that the game
			 * should wait before moving on to the next action.
			 * Return 0f to make the action instantenous.
			 * 
			 * For actions that take longer than one frame,
			 * you can return "defaultPauseTime" to make the game
			 * re-run this function a short time later. You can
			 * use the isRunning boolean to check if the action is
			 * being run for the first time, eg: 
			 */
			
			if (!isRunning)
			{
				isRunning = true;
				return defaultPauseTime;
			}
			else
			{
				isRunning = false;
				return 0f;
			}
		}
		
		
		#if UNITY_EDITOR

		override public void ShowGUI ()
		{
			// Action-specific Inspector GUI code here
			
			AfterRunningOption ();
		}
		

		public override string SetLabel ()
		{
			// Return a string used to describe the specific action's job.
			
			string labelAdd = "";
			return labelAdd;
		}

		#endif
		
	}

}