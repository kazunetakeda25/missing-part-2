using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ActionDisable : AC.Action {

	public enum VisState { Disable, Enable };
	public GameObject obToAffect;
	public bool affectChildren;
	public VisState visState = 0;
	
	
	public ActionDisable ()
	{
		this.isDisplayed = true;
		title = "Object: Disable";
	}
	
	
	override public float Run ()
	{
		
		bool state = false;
		if (visState == VisState.Enable)
		{
			state = true;
		}
		
		if (obToAffect)
		{
			obToAffect.SetActive(state);
			
		}
		
		return 0f;
	}
	
	
	#if UNITY_EDITOR
	
	override public void ShowGUI ()
	{
		obToAffect = (GameObject) EditorGUILayout.ObjectField ("Object to affect:", obToAffect, typeof(GameObject), true);
		
		visState = (VisState) EditorGUILayout.EnumPopup ("Visibility:", visState);
		
		affectChildren = EditorGUILayout.Toggle ("Affect children?", affectChildren);
		
		AfterRunningOption ();
	}
	
	
	override public string SetLabel ()
	{
		string labelAdd = "";
		
		if (obToAffect)
			labelAdd = " (" + obToAffect.name + ")";
		
		return labelAdd;
	}
	
	#endif
}
