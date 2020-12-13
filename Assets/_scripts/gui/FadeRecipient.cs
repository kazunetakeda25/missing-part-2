using UnityEngine;
using System.Collections;

public class FadeRecipient : MonoBehaviour {
	
	public Color32 fadeInColor = new Color32(255,255,255,255);
	public float animationLength = .5f;
	public bool sendToBackOnFadeOut = false;
	
	private dfTweenColor32 fadeIn, fadeOut;
	private bool isVisible;
	private GameObject objToDestroy;
	
	void Awake()
	{
		// Create fade in tween
		fadeIn = gameObject.AddComponent<dfTweenColor32>();
		fadeIn.Target = new dfComponentMemberInfo()
		{
			Component = GetComponent<dfControl>(),
			MemberName = "Color"
		};
		fadeIn.StartValue = new Color32(0,0,0,0);
		fadeIn.EndValue = fadeInColor;
		fadeIn.Length = animationLength;


		// Create fade out tween
		fadeOut = gameObject.AddComponent<dfTweenColor32>();
		fadeOut.Target = new dfComponentMemberInfo()
		{
			Component = GetComponent<dfControl>(),
			MemberName = "Color"
		};
		fadeOut.StartValue = new Color32(255,255,255,255);
		fadeOut.EndValue = new Color32(0,0,0,0);
		fadeOut.Length = animationLength;
	}

	/// <summary>
	/// Fade in or out depending on the visibility of this object's dfControl.
	/// </summary>
	public void Fade()
	{
		if( gameObject.GetComponent<dfControl>().IsVisible )
			StartCoroutine( DoFade(false) ); // Fade Out
		else
			StartCoroutine( DoFade(true) ); // Fade In
	}
	
	private IEnumerator DoFade(bool isFadingIn)
	{
		if( isFadingIn )
		{
			if( sendToBackOnFadeOut ) 
				GetComponent<dfControl>().ZOrder = gameObject.transform.parent.childCount - 1;

			fadeIn.Play();
		}
		else
		{
			fadeOut.Play();

			// Wait for animation to finish before doing stuff
			yield return new WaitForSeconds( animationLength );

			if( sendToBackOnFadeOut ) 
				GetComponent<dfControl>().ZOrder = 0;

			if( objToDestroy != null )
				Destroy( objToDestroy );
		}
		
		gameObject.GetComponent<dfControl>().IsVisible = isFadingIn;
			
	}

	/// <summary>
	/// Destroys supplied GameObject when done fading out.
	/// </summary>
	/// <param name="obj">Object to destroy.</param>
	public void DestroyObjectOnFadeOut( GameObject obj )
	{
		objToDestroy = obj;
	}
}