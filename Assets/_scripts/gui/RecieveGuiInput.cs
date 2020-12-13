using UnityEngine;
using System.Collections;

public class RecieveGuiInput : MonoBehaviour {
	
	public ACCommunicator acComm;
	public dfPanel createSession;

	private ShowInputControls showInputControls;

	void Start()
	{
		showInputControls = GetComponent<ShowInputControls>();
	}

	/// <summary>
	/// Handle clicks on the main menu links.
	/// </summary>
	/// <param name="control">dfControl.</param>
	/// <param name="mouseEvent">Mouse event.</param>
	public void MainMenuItemClicked( dfControl control, dfMouseEventArgs mouseEvent )
	{
		// Can't send twice when double clicking
		control.IsEnabled = false;

		string userInput = control.name;
		Debug.Log ("Button " + control.name + "clicked.");
		
		switch( userInput )
		{
			case "Create":
				showInputControls.ShowCreateSession();
				FadeOut( control.transform.parent.parent.gameObject, true );
				break;
				
			case "Load":
				break;
				
			case "Review":
				break;
				
			case "About":
				showInputControls.ShowAboutPage();
				control.IsEnabled = true;
				break;
				
			case "Quit":
				Application.Quit();
				break;
		}

	}
	
	/// <summary>
	/// Recieves the text of the multiple choice interface.
	/// </summary>
	/// <param name="control">dfControl.</param>
	/// <param name="mouseEvent">Mouse event.</param>
	public void RecieveMultipleChoiceAnswer( dfControl control, dfMouseEventArgs mouseEvent )
	{
		// Can't send twice when double clicking
		control.IsEnabled = false;

		string userInput = control.Tooltip;
		int buttonNum = control.ZOrder;

		Debug.Log( userInput + " -- " + buttonNum);

		if( acComm )
			acComm.Answer (buttonNum);

		FadeOut( control.transform.parent.gameObject, true );
	}


	public void GetBulletAmount( dfControl control, dfMouseEventArgs mouseEvent )
	{
		// Can't send twice when double clicking
		control.IsEnabled = false;
		
		string userInput = control.Tooltip;
		
		Debug.Log( userInput );
		if( acComm )
			acComm.GetCounterValue (int.Parse (userInput));

		FadeOut( control.transform.parent.gameObject, true );
	}

	/// <summary>
	/// Recieves the slider input.
	/// </summary>
	/// <param name="control">Control.</param>
	/// <param name="mouseEvent">Mouse event.</param>
	public void RecieveSliderInput( dfControl control, dfMouseEventArgs mouseEvent )
	{
		// Can't send twice when double clicking
		control.IsEnabled = false;

		SliderBehavior slider = GameObject.Find("SliderBehavior").GetComponent<SliderBehavior>();

		string userInput = slider.slider.Value.ToString();

		if( acComm )
			acComm.AnswerS (int.Parse (userInput));

		FadeOut( slider.sliderContainer.gameObject, true );
	}

	/// <summary>
	/// Skips the typewriter effect and shows the entire dialog.
	/// </summary>
	public void SkipTypewriterEffect()
	{
		showInputControls.SkipTypewriterEffect();
	}

	/// <summary>
	/// Fades the GameObject out.
	/// </summary>
	/// <param name="objToFade">Object to fade.</param>
	/// <param name="destroyOnFadeout">If set to <c>true</c> destroy on fadeout.</param>
	void FadeOut( GameObject objToFade, bool destroyOnFadeout )
	{
		FadeRecipient fade;

		if( objToFade.GetComponent<FadeRecipient>() )
			fade = objToFade.GetComponent<FadeRecipient>();
		else
			fade = objToFade.AddComponent<FadeRecipient>();


		if( destroyOnFadeout )
			fade.DestroyObjectOnFadeOut( objToFade );

		fade.Fade();
	}

}
