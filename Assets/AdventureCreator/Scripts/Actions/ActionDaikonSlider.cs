using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ActionDaikonSlider : AC.Action {
	
	public ACCommunicator displayCommunicator;
	public string messageText;
	public Quiz quizType = Quiz.NONE;
	public int low;
	public int high;
	public int anchorValue;
	public bool useVariable;
	public AC.VariablesManager varMan;
	public int variableID;
	public int variableNumber;

	//private bool isRunning;

	public ActionDaikonSlider()
	{
		this.isDisplayed = true;
		title = "Slider: Use Daikon Slider";
	}
	override public float Run ()
	{
		
		displayCommunicator = ACCommunicator.Instance;

		//AC.KickStarter.stateHandler.cursorIsOff = false;
		AC.KickStarter.stateHandler.gameState = AC.GameState.Normal;
		if (displayCommunicator) 
		{
			if (!isRunning) 
			{
				isRunning = true;
				if (high != 0 || low != 0) {
					if(messageText == "")
						displayCommunicator.ShowSlider (low, high,quizType);
					else 
					{
						Debug.Log(quizType);
						if(quizType == Quiz.GAME_2_HUNTING_ME|| quizType == Quiz.GAME_2_HUNTING_THEM)
						{
							Debug.Log("slider with increment");
							displayCommunicator.ShowSliderIncrement(low,high,messageText,quizType);
						}
						else
						{
							Debug.Log("slider");
							displayCommunicator.ShowSlider (low, high,messageText,anchorValue,quizType);
						}
					}

					return defaultPauseTime;
				}
				return 0;
			} 
			else 
			{
				if (!displayCommunicator.isMessageAlive)
				{
					if(useVariable)
					{

						AC.GVar gvar = AC.RuntimeVariables.GetVariable(variableID);
						gvar.Download();
						if (gvar.type == AC.VariableType.Integer) {
							Debug.Log("setting value " + displayCommunicator.GetValue());
							gvar.SetValue (displayCommunicator.GetValue (), AC.SetVarMethod.SetValue);
						}


					}
					isRunning = false;
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
		if (!varMan) 
		{
			varMan = AC.KickStarter.variablesManager;
		}

		//conversation = (ConversationDaikon) EditorGUILayout.ObjectField ("Conversation:", conversation, typeof (ConversationDaikon), true);
		low = EditorGUILayout.IntField ("Low", low);
		high = EditorGUILayout.IntField ("High", high);
		messageText = EditorGUILayout.TextField ("line Display", messageText);
		quizType = (Quiz)EditorGUILayout.EnumPopup ("Quiz Type: ", quizType);
		useVariable = EditorGUILayout.Toggle ("Use Variable", useVariable);
		anchorValue = EditorGUILayout.IntField ("Anchor Value", anchorValue);
		if (varMan) {
			if (varMan.vars.Count > 0 && useVariable) {

				List<string> labelList = new List<string> ();

				int i = 0;
				variableNumber = -1;
				foreach (AC.GVar _var in varMan.vars) {
					labelList.Add (_var.label);

					// If a GlobalVar variable has been removed, make sure selected variable is still valid
					if (_var.id == variableID) {
							variableNumber = i;
					}

					i ++;
				}

				if (variableNumber == -1) {
					// Wasn't found (variable was deleted?), so revert to zero
					Debug.LogWarning ("Previously chosen variable no longer exists!");
					variableNumber = 0;
					variableID = 0;
				}
				variableNumber = EditorGUILayout.Popup (variableNumber, labelList.ToArray ());
				variableID = varMan.vars [variableNumber].id;
			}
			//added this for jazz
		}
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
