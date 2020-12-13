using UnityEngine;
using System.Collections;

//Disable this when you drop it in a scene.

public class ACInteractionDelay : MonoBehaviour 
{
	public AC.Interaction interaction;
	public float delay;

	private void Update()
	{
		delay -= Time.deltaTime;

		if (delay <= 0) {
			if(AC.KickStarter.stateHandler.gameState == AC.GameState.Normal) {
				Activate();
			}
		}
	}

	public void Activate()
	{
		interaction.Interact();
		this.enabled = false;
	}

}
