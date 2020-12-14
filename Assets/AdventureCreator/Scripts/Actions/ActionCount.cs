using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ActionCount : AC.Action {
	
	public ACCommunicator displayCommunicator;
	public string messageText; 
	public Quiz quizType = Quiz.NONE;
	public Vector2 position;

	public ActionCount()
	{
		this.isDisplayed = true;
		title = "Counter: Use Daikon Counter";
	}
	override public float Run ()
	{
		displayCommunicator = ACCommunicator.Instance;
		if (displayCommunicator) 
		{
			if (!isRunning) 
			{
				isRunning = true;
				AC.KickStarter.stateHandler.gameState = AC.GameState.Normal;
				Vector3 newposition = new Vector3( Screen.width * position.x/100f, -Screen.height * position.y /100f);
				displayCommunicator.ShowCounter(newposition, messageText, quizType);
				return 0.2f;
			}
			else
			{

				if(!displayCommunicator.isMessageAlive)
				{
					isRunning = false;
					return 0.0f;
				}
				else
				{
					return defaultPauseTime;
				}
			}
		}
		return 0.0f;
	}
#if UNITY_EDITOR
	
	override public void ShowGUI ()
	{		
		messageText = EditorGUILayout.TextField ("line Display", messageText);
		position = EditorGUILayout.Vector2Field ("Screen Position", position);
		quizType = (Quiz)EditorGUILayout.EnumPopup ("Quiz Type: ", quizType);
		AfterRunningOption ();
	}
	override public string SetLabel ()
	{
		string labelAdd = "";
		
		if (messageText != "")
		{
			string shortMessage = messageText;
			if (shortMessage != null && shortMessage.Length > 30)
			{
				shortMessage = shortMessage.Substring (0, 28) + "..";
			}
			
			labelAdd = " (" + shortMessage + ")";
		}
		
		return labelAdd;
	}
#endif
}
