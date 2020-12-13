using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class FidelityRuiner : MonoBehaviour {

	private void StartBV()
	{
		Debug.Log ("hit IT");
		if(Settings.Instance.Fidelity)
			this.GetComponent<AudioSource>().Play();
	}

	private void EndBV()
	{
		this.GetComponent<AudioSource>().Stop();
	}

}
