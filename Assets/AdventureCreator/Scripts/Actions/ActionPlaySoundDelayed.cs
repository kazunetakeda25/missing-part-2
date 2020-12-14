using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ActionPlaySoundDelayed : AC.Action {

	public AudioClip clip;
	public AudioSource source;
	public float delay = 0.0f;
	
	//private bool isRunning;

    public ActionPlaySoundDelayed()
	{
		this.isDisplayed = true;
		this.category = AC.ActionCategory.Sound;
		title = "Play Sound with a delay timer.";
	}
	override public float Run ()
	{
		source.clip = this.clip;
		source.PlayDelayed(delay);
		return 0f;
	}
	
	#if UNITY_EDITOR
	
	override public void ShowGUI (List<AC.ActionParameter> parameters)
	{
		clip = (AudioClip) EditorGUILayout.ObjectField("Clip To Play: ", clip, typeof(AudioClip), false);
		source = (AudioSource) EditorGUILayout.ObjectField("Audio Source: ", source, typeof(AudioSource), true);
		delay = EditorGUILayout.Slider ("Delay time:", delay, 0f, 10f);
	}
	
	#endif
}
