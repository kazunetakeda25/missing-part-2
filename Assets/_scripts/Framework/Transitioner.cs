using UnityEngine;
using System.Collections;

public class Transitioner : MonoBehaviour {
	
	public float transitionDelay;

	private void Awake()
	{
		if(AC.KickStarter.stateHandler != null) {
			GameObject.Destroy(AC.KickStarter.stateHandler.gameObject);
		}

		if(GameObject.FindGameObjectWithTag("Player") != null) {
			GameObject.Destroy(GameObject.FindGameObjectWithTag("Player"));
		}

		Time.timeScale = 1.0f;
	}

	private void Update()
	{
		transitionDelay -= Time.deltaTime;
		if(transitionDelay <= 0)
		{
			SessionManager.Instance.TransitionComplete();
			GameObject.Destroy(this.gameObject);
		}
	}

}
