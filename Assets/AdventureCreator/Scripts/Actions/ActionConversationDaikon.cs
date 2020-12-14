using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ActionConversationDaikon : AC.Action {

	public ConversationDaikon conversation;
	
	public ActionConversationDaikon ()
	{
		this.isDisplayed = true;
		title = "Dialogue: Start Conversation with Daikon";
	}
	
	
	override public float Run ()
	{
		conversation.Interact ();
		return 0f;
	}
	
	#if UNITY_EDITOR
	
	override public void ShowGUI ()
	{
		conversation = (ConversationDaikon) EditorGUILayout.ObjectField ("Conversation:", conversation, typeof (ConversationDaikon), true);
		AfterRunningOption ();
	}
	
	override public string SetLabel ()
	{
		string labelAdd = "";
		
		if (conversation)
		{
			labelAdd = " (" + conversation + ")";
		}
		
		return labelAdd;
	}
	
	#endif
}
