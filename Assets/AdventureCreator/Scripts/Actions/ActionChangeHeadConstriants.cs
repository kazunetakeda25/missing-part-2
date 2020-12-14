using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ActionChangeHeadConstriants : AC.Action {

	public float minY;
	public float maxY;

	public ActionChangeHeadConstriants ()
	{
		this.isDisplayed = true;
		title = "Change FPS Head Constraints";
	}
	
	
	override public float Run ()
	{
		GameObject fpsGO = GameObject.Find("FirstPersonCamera");
		FirstPersonCamera fps = fpsGO.GetComponent<FirstPersonCamera>();
		fps.SetMinMax(new Vector2(minY, maxY));

		return 0f;
	}
	
	
	#if UNITY_EDITOR
	
	override public void ShowGUI ()
	{
		minY = EditorGUILayout.FloatField("MinY: ", minY);
		maxY = EditorGUILayout.FloatField("MinY: ", maxY); 

		AfterRunningOption ();
	}
	
	#endif
}
