using UnityEngine;
using System.Collections;

[RequireComponent(typeof(dfPanel))]
public class DisablePanelOnPause : MonoBehaviour 
{
	private bool disabled = false;
	[SerializeField] private dfPanel myPanel;

	private void Awake()
	{
		myPanel = this.gameObject.GetComponent<dfPanel> ();
	}

	// Update is called once per frame
	void Update () 
	{
		if (disabled == false) {
			if (AC.KickStarter.stateHandler.gameState == AC.GameState.Paused) {
				disabled = true;
				myPanel.IsEnabled = false;
				Debug.Log ("disabled");
			}
		}

		if (disabled == true) {
			if (AC.KickStarter.stateHandler.gameState != AC.GameState.Paused) {
				disabled = false;
				myPanel.IsEnabled = true;
				Debug.Log ("enabled");
			}
		}
	}
}
