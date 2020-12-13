using UnityEngine;
using System.Collections;

public class ControlAppearenceDelay : MonoBehaviour {

	public float delay;


	private void Update()
	{
		delay -= Time.deltaTime;

		if(delay <= 0)
			ShowButton();
	}

	private void ShowButton()
	{
		dfControl control = gameObject.GetComponent<dfControl>();
		control.IsVisible = true;
		control.IsEnabled = true;
		GameObject.Destroy(this);
	}

}
