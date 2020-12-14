using UnityEngine;
using System.Collections;
using DG.Tweening;

public class CrosshairFader : MonoBehaviour 
{
	private static CrosshairFader instance;
	public static CrosshairFader Instance { get { return instance; } }

	private void Awake()
	{
		instance = this;
		Hide ();
	}


	[SerializeField] bool showing = false;

	private void LateUpdate()
	{
		if(showing == true) {
			if(AC.KickStarter.stateHandler.gameState == AC.GameState.Normal) {
				this.GetComponent<Renderer>().enabled = true;
			} else {
				this.GetComponent<Renderer>().enabled = false;
			}
		}
	}

	public void Show()
	{
		if(showing == true) {
			return;
		}

		showing = true;
		Debug.Log ("Show");
		this.GetComponent<Renderer>().material.DOFade(1.0f, 0.5f);
	}

	public void Hide()
	{
		if(showing == false) {
			return;
		}

		showing = false;
		this.GetComponent<Renderer>().material.DOFade(0.0f, 0.5f);
	}
}
