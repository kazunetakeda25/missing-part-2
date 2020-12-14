using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ActionDaikonText : AC.Action {
	public ACCommunicator displayCommunicator;
	//public bool isPlayer;
	//public ACChar speaker;
	public string messageText;
	//public int lineID;
	public float timer =10.0f;
	public bool isBackground = false;
	//public AnimationClip headClip;
	//public AnimationClip mouthClip;
	//public float Height;
	//public float Width;
	//public Vector3 position;
	
	//private Dialog dialog;
	private AC.StateHandler stateHandler;
	private AC.SpeechManager speechManager;
	private AC.Options options;
	
	
	public ActionDaikonText ()
	{
		this.isDisplayed = true;
		title = "Dialogue: Daikon Text Message";
		//lineID = -1;
	}
	
	
	override public float Run ()
	{
		displayCommunicator = ACCommunicator.Instance;
		stateHandler = AC.KickStarter.stateHandler;
		options = AC.KickStarter.options;
		
		if (displayCommunicator && stateHandler && options)
		{
			if (!isRunning)
			{
				isRunning = true;				
				string _text = messageText;				
				if (_text != "")
				{
					displayCommunicator.ShowTextMessage(_text,timer);
					stateHandler.gameState = AC.GameState.Normal;
					if (!isBackground)
					{
						return defaultPauseTime;
					}
				}
				
				return 0f;
			}
			else
			{
				if (!displayCommunicator.isMessageAlive)
				{
					isRunning = false;
					stateHandler.gameState = AC.GameState.Cutscene;
					return 0f;
				}
				else
				{
					return defaultPauseTime;
				}
			}
		}
		
		return 0f;
	}
	
	#if UNITY_EDITOR
	
	override public void ShowGUI ()
	{
		messageText = EditorGUILayout.TextField ("Line text:", messageText);
		timer = EditorGUILayout.FloatField ("DisplayTime:", timer);
		isBackground = EditorGUILayout.Toggle ("Play in background?", isBackground);
		
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
