using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ActionUnloadSavedGame : AC.Action {
	
	public ActionUnloadSavedGame()
	{
		this.isDisplayed = true;
		title = "Unload Save Game: Do after quitting.";
	}
	
	
	override public float Run ()
	{
		MissingComplete.SaveGameManager.Instance.UnloadSavedGame();
		return 0f;
	}
	
	
	#if UNITY_EDITOR
	
	override public void ShowGUI ()
	{
		AfterRunningOption ();
	}
	
	#endif
}
