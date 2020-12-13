using UnityEngine;
using System.Collections;

public class IntroScrawl : MonoBehaviour 
{

	public void OnNextClicked( dfControl control, dfMouseEventArgs mouseEvent )
	{
		Debug.Log ("Next clicked.");
		SessionManager.Instance.GotoNextLevel();
		GameObject.Destroy(control.gameObject);
	}


}
