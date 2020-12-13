using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class SceneNavigator 
{
	private const string TRANSITION_LEVEL = "LEVEL_TRANSITIONER";
	private const string FADER_PATH = "Faders/ScreenFaderPrefab_default";

	private const float FADE_IN_TIME = 3.0f;
	private const float FADE_OUT_TIME = 3.0f;

	private int currentScene = 0;

	private enum TransitionState
	{
		Idle,
		FadingOut,
		FadingIn,
		TransitionLevel
	}

	private TransitionState state = TransitionState.Idle;

	private List<Episode> sceneOrder;
	private DefaultScreenFader fader;

	public SceneNavigator()
	{
		sceneOrder = new List<Episode>(Enum.GetValues(typeof(Episode)) as Episode[]);
		//Debug.Log ("Count: " + sceneOrder.Count);
		CreateFader();
	}

	public void SetCurrentScene(Episode episode)
	{
		currentScene = (int)episode;
	}
	
	public void GotoNextScene()
	{
		Debug.Log ("Current Scene: " + currentScene);
		ReportEvent.EpisodeCompleted(sceneOrder[currentScene]);
		currentScene++;
		FadeOut();
	}
	
	public void TransitionComplete() { LoadCurrentLevel(); }

	private void CreateFader()
	{
		GameObject faderGO = GameObject.Instantiate(Resources.Load (FADER_PATH)) as GameObject;
		GameObject.DontDestroyOnLoad(faderGO);
		fader = faderGO.GetComponent<DefaultScreenFader>();
		fader.fadeColor = Color.black;
	}

	private void FadeOut()
	{
		Debug.Log ("1");
		fader.FadeFinish += FadeOutComplete;
		fader.Fade (ScreenFaderComponents.Enumerators.FadeDirection.In, FADE_IN_TIME);
	}

	public void FadeOutComplete(object obj, ScreenFaderComponents.Events.FadeEventArgs args) 
	{ 
		Debug.Log ("2");
		fader.FadeFinish -= FadeOutComplete;
		LoadTransitionLevel();
	}

	private void LoadTransitionLevel()
	{
		SceneManager.LoadScene(TRANSITION_LEVEL);
	}

	private void LoadCurrentLevel()
	{
		Debug.Log ("3");
		if(currentScene >= Enum.GetValues(typeof(Episode)).Length)
		{
			Debug.Log ("Quitting App");
			Application.Quit();
			return;
		}

		SceneManager.LoadScene(sceneOrder[currentScene].ToString());
		ReportEvent.EpisodeStarted(sceneOrder[currentScene]);
		fader.Fade (ScreenFaderComponents.Enumerators.FadeDirection.Out, FADE_OUT_TIME);
	}

}
