using UnityEngine;
using System.Collections;

public class Hints : MonoBehaviour {

	private FadeRecipient fade;

	void Awake() 
	{
		fade = gameObject.GetComponent<FadeRecipient>();
		fade.DestroyObjectOnFadeOut( gameObject );
	}
	
	public void HideHints()
	{
		fade.Fade();
	}

	public void SetHintText( string text )
	{
		gameObject.GetComponent<dfLabel>().Text = text;
	}
}
