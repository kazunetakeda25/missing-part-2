using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ActionCheckpointSave : AC.Action {

	public int checkpointToSave;
	
	public ActionCheckpointSave ()
	{
		this.isDisplayed = true;
		title = "Save System: Checkpoint Save";
	}
	
	
	override public float Run ()
	{
		if(MissingComplete.SaveGameManager.Instance == null) {
			return 0f;
		}

		if(SessionManager.Instance.HasInit == false) {
			return 0f;
		}

		MissingComplete.SaveGameManager.Instance.GetCurrentSaveGame().checkPoint = checkpointToSave;
		MissingComplete.SaveGameManager.Instance.SaveCurrentGame();
		
		return 0f;
	}
	
	
	#if UNITY_EDITOR
	
	override public void ShowGUI ()
	{
		checkpointToSave = EditorGUILayout.IntField("Checkpoint To Save: ", checkpointToSave); 

		AfterRunningOption ();
	}
	
	#endif
}
