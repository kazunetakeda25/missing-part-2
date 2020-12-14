using UnityEngine;
using System.Collections;

public class DisableAC : MonoBehaviour 
{

	
	// Update is called once per frame
	void Update () {
		if(Input.GetKey(KeyCode.P))
			AC.KickStarter.stateHandler.TurnOffAC();
	}
}
