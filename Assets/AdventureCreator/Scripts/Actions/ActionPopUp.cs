using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ActionPopUp : AC.Action {

	public bool ignorePriming;
	public string content;
	public bool hintComplete;
	public int variableID;
	public int variableNumber;
	public bool useVariable;
	public AC.VariablesManager varMan;
	
	//private bool isRunning;
	
	public ActionPopUp()
	{
		this.isDisplayed = true;
		title = "Hint Pop Up: For Priming";
	}
	override public float Run ()
	{
		if(Settings.Instance.Priming == false && ignorePriming == false) {
			return 0.0f;
		}

		//First Loop, before we've shown the hint.
		//Show Hint
		if(isRunning == false && hintComplete == false) {
			Debug.Log (Time.deltaTime);
			isRunning = true;
			HintPopUp.Instance.ShowHint(this, content);
			return 0.2f;
		}

		AC.KickStarter.stateHandler.gameState = AC.GameState.Normal;

		//Is Running is true
		//The negative function here is isRunning == false && hintComplete == true
		//This results in the return value below returning and AC to stop running this loop,
		//moving onto the next Conversation node.

		return 0.0f;
	}

	public void DismissHint()
	{
		hintComplete = true;
		isRunning = false;
	}
	
	#if UNITY_EDITOR
	
	override public void ShowGUI ()
	{
		if (!varMan) 
		{
			varMan = AC.KickStarter.variablesManager;
		}
		
		//conversation = (ConversationDaikon) EditorGUILayout.ObjectField ("Conversation:", conversation, typeof (ConversationDaikon), true);
		content = EditorGUILayout.TextField("Content", content);
		ignorePriming = EditorGUILayout.Toggle("Ignore Priming", ignorePriming);
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
		
		if (content != "")
		{
			string shortMessage = content;
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
