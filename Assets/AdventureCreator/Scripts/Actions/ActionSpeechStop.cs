/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"ActionSpeechStop.cs"
 * 
 *	This Action forces off all playing speech
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
	public class ActionSpeechStop : Action
	{

		public bool forceMenus;


		public ActionSpeechStop ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Dialogue;
			title = "Stop speech";
			description = "Ends any currently-playing speech instantly.";
		}
		
		
		override public float Run ()
		{
			KickStarter.dialog.KillDialog (true, forceMenus);

			return 0f;
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI ()
		{
			forceMenus = EditorGUILayout.Toggle ("Force off subtitles?", forceMenus);

			AfterRunningOption ();
		}
		
		
		public override string SetLabel ()
		{
			return "";
		}
		
		#endif
		
	}

}