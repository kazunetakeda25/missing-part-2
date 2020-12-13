using UnityEngine;
using System.Collections;

public class HighlightOnMouseOver : MonoBehaviour {
	
	public float animationLength = .2f;
	[Range(0,255)]
	public byte startingOpacity = 50;
	
	private dfTweenColor32 fadeIn, fadeOut;
	private bool isVisible;

	
	void Awake()
	{
		fadeIn = gameObject.AddComponent<dfTweenColor32>();
		fadeIn.Target = new dfComponentMemberInfo()
		{
			Component = GetComponent<dfControl>(),
			MemberName = "Color"
		};
		fadeIn.StartValue = new Color32(255,255,255,startingOpacity);
		fadeIn.EndValue = new Color32(255,255,255,255);
		fadeIn.Length = animationLength;
		
		
		fadeOut = gameObject.AddComponent<dfTweenColor32>();
		fadeOut.Target = new dfComponentMemberInfo()
		{
			Component = GetComponent<dfControl>(),
			MemberName = "Color"
		};
		fadeOut.StartValue = new Color32(255,255,255,255);
		fadeOut.EndValue = new Color32(255,255,255,startingOpacity);
		fadeOut.Length = animationLength;

		// Starting color is low opacity
		GetComponent<dfControl>().Color = fadeIn.StartValue;
	}


	public void OnMouseEnter()
	{
		fadeIn.Play();
	}

	public void OnMouseLeave()
	{
		fadeOut.Play();
	}

}