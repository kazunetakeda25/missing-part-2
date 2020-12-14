using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ActionPopUpTutorial : AC.Action {

	public bool hintComplete;
	public string content;
	
	public ActionPopUpTutorial()
	{
		this.isDisplayed = true;
		title = "Hint Pop Up: Image";
	}
	override public float Run ()
	{

		//First Loop, before we've shown the hint.
		//Show Hint
		//if(isRunning == false && hintComplete == false) {
		//	Debug.Log (Time.deltaTime);
		//	isRunning = true;
		//	PopUpTutorial.Instance.ShowTutorial(this, content);
		//	return 0.2f;
		//}

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
		//conversation = (ConversationDaikon) EditorGUILayout.ObjectField ("Conversation:", conversation, typeof (ConversationDaikon), true);
		content = EditorGUILayout.TextField("Content", content);
	}
#endif
}
