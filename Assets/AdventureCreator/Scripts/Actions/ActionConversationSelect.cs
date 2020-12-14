using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]

public class ActionConversationSelect : AC.Action {

	public AC.DialogueOption dialog;

	public ActionConversationSelect ()
	{
		this.isDisplayed = true;
		title = "Dialogue: Start Conversation selection with Daikon";
	}
	
	override public float Run ()
	{
		dialog.Interact ();
		
		return 0f;
	}
	#if UNITY_EDITOR
	override public void ShowGUI ()
	{

		dialog = (AC.DialogueOption) EditorGUILayout.ObjectField ("Conversation:", dialog, typeof (AC.DialogueOption), true);
		AfterRunningOption ();

	}
	#endif
}
