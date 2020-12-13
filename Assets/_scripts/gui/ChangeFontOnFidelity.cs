using UnityEngine;
using System.Collections;

public class ChangeFontOnFidelity : MonoBehaviour 
{

	public dfFontBase fidelityFont;

	void Start () 
	{
		if( ACCommunicator.FidelityActive )
		{
			gameObject.GetComponent<dfLabel>().Font = fidelityFont;
			Debug.Log (this.gameObject.name );

			if(this.gameObject.name == "Label")
				gameObject.GetComponent<dfLabel>().enabled = false;
		}
	}
}
