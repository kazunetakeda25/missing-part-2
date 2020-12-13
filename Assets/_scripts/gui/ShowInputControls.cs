using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// To show the GUI and recieve its input, all that is needed is this class and RecieveGuiInput.cs in the scene.
/// The user input methods will fade out automatically when the user enters a value.
/// 
/// The following methods should be called to show/hide the controls:
/// 
/// Full Screens:
/// 	ShowMainMenu()
/// 	ShowCreateSession()
/// 
/// Dialog:
/// 	ShowDialogBox( position, height, width )
///		AddDialog( "Dialog", audioLength )
/// 	RemoveCurrentDialog()
/// 	HideDialogBox()
/// 
/// Text Messages:
/// 	ShowTextMessage( "message" )
/// 
/// Hints:
/// 	ShowHints( "message" )
/// 
/// User Input: 
/// 	ShowMultipleChoice( ["1", "2"] )
/// 	ShowSlider( 1, 10[, 2] )
/// 	ShowSliderWithIncrements( 1, 10, 1)
/// </summary>
public class ShowInputControls : MonoBehaviour {
	
	#region Variables
	
	public GameObject 
		uiRootPrefab,
		sliderContainer,
		slider,
		sliderWithIncrements,
		blackOverlay,
		genericContainer;
	
	public dfControl
		multipleChoiceOption,
		mainMenu,
		bulletCount;
	
	public dfPanel 
		uiContainer, 
		createSession,
		aboutPage;
	
	public dfScrollPanel 
		dialogContainer, 
		multipleChoiceContainer,
		textMessageContainer;
	
	public dfLabel 
		dialogLabel,
		textMessage,
		hints,
		genericLabel;
	
	public AudioClip
		textMessageSound;

	public float 
		downscaler = 0.00075f,
		downscaler4by3 = 0.0001f,
		font = 1,
		font4by3 = 1,
		font16by9 = 1;
	
	public bool 
		IS4BY3 = false,
		USE_TEMP_RESOLUTION = false,
		TESTING_MULT_CHOICE = false, 
		TESTING_SLIDER_W_INCREMENTS = false,
		TESTING_SLIDER = false,
		TESTING_MAIN_MENU = false,
		TESTING_DIALOG = false,
		TESTING_TEXTS = false,
		TESTING_HINTS = false,
		TESTING_BULLETS = false;
	
	
	private GameObject 
		uiRoot, 
		dialogContainerInstance;
	
	private dfPanel
		aboutPageInstance;

	private dfLabel
		labelInstance;
	
	private dfScrollPanel 
		dialogInstance,
		textMessagesInstance;
	
	private bool 
		typewriterEffectHappening = false,
		typewriterBreak = false;
	
	private int
		screenWidth,
		screenHeight;
	
	#endregion
	
	void Start()
	{
		screenWidth = USE_TEMP_RESOLUTION ? 1024 : Screen.width;
		screenHeight = USE_TEMP_RESOLUTION ? 768 : Screen.height;
		
		
		// Set uiroot and uicontainer references
		uiRoot = (GameObject) Instantiate( uiRootPrefab );
		uiContainer = uiRoot.transform.GetChild(1).GetComponent<dfPanel>();
		//foreach( Transform baseObj in uiRoot.transform )
		//	if( baseObj.gameObject.tag == "UIContainer")
		//		uiContainer = baseObj.GetComponent<dfPanel>();
		float ratio = (float)screenWidth / (float)screenHeight;
		
		
		// Start Ed's stuff
		if (ratio <= 1.4f && ratio >= 1.3f) {
			IS4BY3 = true;
			downscaler4by3 = (float) screenWidth / 640f;
			if(downscaler4by3 > 1)
			{
				downscaler4by3 *= 1.2f;
			}
			font = font4by3 * downscaler4by3;
		} else {
			IS4BY3 = false;
			downscaler = (float) screenWidth / 800f;
			if(downscaler > 1)
				downscaler *= 1.1f;
			font = font16by9 * downscaler;
		}
		// End eds stuff
		

		uiRoot.GetComponent<dfGUIManager> ().SetWidth (Screen.width);
		uiRoot.GetComponent<dfGUIManager> ().SetHeight (Screen.height);
		if (uiContainer != null)
		{
			uiContainer.Width = screenWidth;
			uiContainer.Height = screenHeight;
			uiContainer.RelativePosition = Vector3.zero;
		}
		
		#region Testing
		if( TESTING_MULT_CHOICE )
		{
			string[] choices = new string[3];
			choices[0] = "Racketeering [color #ffaaaa](illegal business activities including extortion)";
			choices[1] = "Insider trading [color #ffaaaa](buying or selling securities, including "+
				"stocks based on nonpublic information from inside a company)";
			choices[2] = "Insider trading and money laundering [color #ffaaaa](transforming money from "+
				"criminal activities into legal funds, usually in banks)";
			ShowMultipleChoice( choices, new Vector3(0,0,0), Screen.width * .8f );
		}
		else if( TESTING_SLIDER_W_INCREMENTS )
		{
			ShowSliderWithIncrements(1, 7, 1);
		}
		else if( TESTING_MAIN_MENU )
		{
			ShowMainMenu();
		}
		else if( TESTING_SLIDER )
		{
			ShowSlider( 1, 5 ,1);
			ShowSliderText( "Hello" );
		}
		else if( TESTING_DIALOG )
		{
			ShowDialogBox( new Vector3(0,0,0), Screen.height * 0.7f, Screen.width * 0.8f );
			AddDialog("First Dialog line First Dialog line First Dialog line First Dialog line", 10f);
		}
		else if( TESTING_TEXTS )
		{
			StartCoroutine( TestingTexts() );
		}
		else if( TESTING_HINTS )
		{
			float width = Screen.height * 0.6f;
			ShowHints( "This is a hint, use it when the player enables it from the creation menu.",
			          new Vector3(200 , -200, 0),
			          width );
		}
		else if( TESTING_BULLETS )
		{
			ShowBulletCount( new Vector3(10, 10, 0) , "asdf");
		}
		#endregion
	}
	
	#region Testing
	IEnumerator TestingTexts()
	{
		yield return new WaitForSeconds( 1.0f );
		ShowTextMessage("Hey there, this is a text message!");
		yield return new WaitForSeconds( 2.0f );
		ShowTextMessage("Can you pick up some lettuce for tonight's salad? Thanks.");
		yield return new WaitForSeconds( 3.0f );
		ShowTextMessage("Also some eggs!");
		yield return new WaitForSeconds( 0.3f );
		ShowTextMessage(":)");
	}
	#endregion
	
	#region Ask user for input
	
	/// <summary>
	/// Shows slider with smooth incrementation between min and max.
	/// </summary>
	/// <returns>The slider.</returns>
	/// <param name="min">Minimum.</param>
	/// <param name="max">Max.</param>
	public GameObject ShowSlider( int min, int max, int anchorVal )
	{
		GameObject container = (GameObject) Instantiate( slider );
		container.transform.parent = uiContainer.transform;
		
		int stepSize = (max - min) / 1000 >= 1 ? (max - min) / 1000 : 1;
		
		SliderBehavior newSlider = GameObject.Find("SliderBehavior").GetComponent<SliderBehavior>();
		newSlider.SetupSlider( min, max, stepSize,anchorVal );
		newSlider.SetWidth( screenWidth * 0.9f );
		newSlider.sliderContainer.Position =  
			new Vector3( screenWidth * 0.05f, -( screenHeight - (screenHeight * 0.02f) - newSlider.sliderContainer.Height ), 0);
		
		
		// Pass onclick reference
		newSlider.SendDataOnclick( GetComponent<RecieveGuiInput>(), "RecieveSliderInput");
		
		
		// Fade the slider in
		container.GetComponent<dfControl>().IsVisible = false;
		FadeRecipient fade = container.gameObject.AddComponent<FadeRecipient>();
		fade.DestroyObjectOnFadeOut( container.gameObject );
		fade.Fade();
		
		return container.gameObject;
	}
	
	/// <summary>
	/// Shows the slider's label that is hidden by default.
	/// </summary>
	/// <param name="text">Label text.</param>
	public void ShowSliderText( string text )
	{
		SliderBehavior slider = GameObject.Find("SliderBehavior").GetComponent<SliderBehavior>();
		slider.SetText( text );
	}
	
	/// <summary>
	/// Shows slider with smooth incrementation between min and max.
	/// </summary>
	/// <returns>The slider.</returns>
	/// <param name="min">Minimum.</param>
	/// <param name="max">Max.</param>
	/// <param name="stepSize">Size of the increments</param>
	public GameObject ShowSlider( int min, int max, float stepSize )
	{
		GameObject container = (GameObject) Instantiate( slider );
		container.transform.parent = uiContainer.transform;
		
		stepSize = stepSize >= 1 ? stepSize : 1;
		
		SliderBehavior newSlider = GameObject.Find("SliderBehavior").GetComponent<SliderBehavior>();
		newSlider.SetupSlider( min, max, (int) Mathf.Round( stepSize ) ,4);
		newSlider.SetWidth( screenWidth * 0.9f );
		newSlider.sliderContainer.Position =  
			new Vector3( screenWidth * 0.05f, -( screenHeight - (screenHeight * 0.02f) - newSlider.sliderContainer.Height ), 0);
		
		
		// Pass onclick reference
		newSlider.SendDataOnclick( GetComponent<RecieveGuiInput>(), "RecieveSliderInput");
		
		
		// Fade the slider in
		container.GetComponent<dfControl>().IsVisible = false;
		FadeRecipient fade = container.gameObject.AddComponent<FadeRecipient>();
		fade.DestroyObjectOnFadeOut( container.gameObject );
		fade.Fade();
		
		return container.gameObject;
	}
	
	/// <summary>
	/// Shows the slider with increments.
	/// </summary>
	/// <returns>The slider with increments.</returns>
	/// <param name="min">Minimum.</param>
	/// <param name="max">Max.</param>
	/// <param name="increment">Increment.</param>
	public GameObject ShowSliderWithIncrements( int min, int max, int stepSize )
	{
		GameObject container = (GameObject) Instantiate( sliderWithIncrements );
		container.transform.parent = uiContainer.transform;
		
		stepSize = stepSize >= 1 ? stepSize : 1;
		
		SliderBehavior newSlider = GameObject.Find("SliderBehavior").GetComponent<SliderBehavior>();
		newSlider.SetupSlider( min, max, (int) Mathf.Round( stepSize ), 4);
		newSlider.SetWidth( screenWidth * 0.9f );
		newSlider.sliderContainer.Position =  
			new Vector3( screenWidth * 0.05f, -( screenHeight - (screenHeight * 0.02f) - newSlider.sliderContainer.Height ), 0);
		
		
		// Show the increment slots
		GameObject.Find("Increment_slots").GetComponent<EmptyIncrementSlots>().ShowEmptySlots( true );
		
		
		// Pass onclick reference
		newSlider.SendDataOnclick( GetComponent<RecieveGuiInput>(), "RecieveSliderInput");
		
		
		// Fade the slider in
		container.GetComponent<dfControl>().IsVisible = false;
		FadeRecipient fade = container.gameObject.AddComponent<FadeRecipient>();
		fade.DestroyObjectOnFadeOut( container.gameObject );
		fade.Fade();
		
		
		return container.gameObject;
	}
	
	/// <summary>
	/// Shows the bullet count interface.
	/// </summary>
	/// <returns>The bullet count GameObject.</returns>
	/// <param name="pos">Position.</param>
	public GameObject ShowBulletCount( Vector3 pos , string message)
	{
		dfControl newBulletCount = (dfControl)Instantiate( bulletCount );
		newBulletCount.transform.parent = uiContainer.transform;
		newBulletCount.Position = pos;
		
		AttachOnClickMethod( newBulletCount.GetComponent<BulletCount>().acceptButton, "GetBulletAmount" );
		
		return newBulletCount.gameObject;
	}
	
	/// <summary>
	/// Shows a dialog with clickable choices.
	/// </summary>
	/// <param name="choices">Choices the user can select from.</param>
	/// <returns>The multiple choice.</returns>
	public GameObject ShowMultipleChoice( string[] choices )
	{
		dfScrollPanel container = (dfScrollPanel) Instantiate( multipleChoiceContainer );
		container.transform.parent = uiContainer.transform;
		
		// Add choices
		for( int i = 0; i < choices.Length; i++)
		{
			dfControl choice = (dfControl) Instantiate( multipleChoiceOption );
			choice.transform.parent = container.transform;
			choice.Tooltip = choices[i];
			
			// Add the OnClick event bindings
			AttachOnClickMethod( choice.GetComponent<dfControl>(), "RecieveMultipleChoiceAnswer" );
		}
		
		// Center the options vertically
		container.Position = new Vector3( 
		                                 screenWidth / 2 - container.Width / 2, 
		                                 -(screenHeight / 2 - (multipleChoiceOption.Height + container.FlowPadding.bottom) * choices.Length / 2), 
		                                 0 );
		
		// Fade the slider in
		container.IsVisible = false;
		FadeRecipient fade = container.gameObject.AddComponent<FadeRecipient>();
		fade.DestroyObjectOnFadeOut( container.gameObject );
		fade.Fade();
		
		return container.gameObject;
	}
	/// <summary>
	/// Shows a dialog with clickable choices.
	/// </summary>
	/// <param name="choices">Choices the user can select from.</param>
	/// <param name="pos">Position.</param>
	/// <param name="width">Width.</param>
	/// <returns>The multiple choice.</returns>
	public GameObject ShowMultipleChoice( string[] choices, Vector3 pos, float width )
	{
		dfScrollPanel container = (dfScrollPanel) Instantiate( multipleChoiceContainer );
		container.transform.parent = uiContainer.transform;
		container.Position = pos;
		container.Width = width;
		
		// Add choices
		for( int i = 0; i < choices.Length; i++)
		{
			dfControl choice = (dfControl) Instantiate( multipleChoiceOption );
			choice.transform.parent = container.transform;
			choice.Tooltip = choices[i];
			choice.Width = container.Width;

			foreach( Transform child in choice.transform )
			{
				if( child.GetComponent<dfButton>() )
					child.GetComponent<dfButton>().Width = width;
				else
					child.GetComponent<dfControl>().Width = width - 40;
			}
			
			// Add the OnClick event bindings
			AttachOnClickMethod( choice.GetComponent<dfControl>(), "RecieveMultipleChoiceAnswer" );
		}
		
		// Fade the slider in
		container.IsVisible = false;
		FadeRecipient fade = container.gameObject.AddComponent<FadeRecipient>();
		fade.DestroyObjectOnFadeOut( container.gameObject );
		fade.Fade();
		
		return container.gameObject;
	}

	/// <summary>
	/// Shows a label.
	/// </summary>
	/// <returns>The label.</returns>
	/// <param name="pos">Position.</param>
	/// <param name="width">Width.</param>
	public GameObject ShowLabel( Vector3 pos, float width )
	{
		labelInstance = (dfLabel)Instantiate( genericLabel );
		labelInstance.transform.parent = uiContainer.transform;
		labelInstance.Position = pos;
		labelInstance.Width = width;

		labelInstance.IsVisible = false;
		FadeRecipient fade = labelInstance.gameObject.AddComponent<FadeRecipient>();
		fade.DestroyObjectOnFadeOut( labelInstance.gameObject );
		fade.Fade();

		return labelInstance.gameObject;
	}

	/// <summary>
	/// Hides the label.
	/// </summary>
	/// <returns>The label.</returns>
	public GameObject HideLabel()
	{
		if( labelInstance )
			labelInstance.GetComponent<FadeRecipient>().Fade();

		return labelInstance.gameObject;
	}
	
	#endregion
	
	#region Dialog
	
	/// <summary>
	/// Creates the dialog box at a screen position relative to the top-left of the screen.
	/// </summary>
	/// <returns>The dialog box.</returns>
	/// <param name="pos">Position.</param>
	/// <param name="height">Height.</param>
	/// <param name="width">Width.</param>
	public GameObject ShowDialogBox( Vector3 pos, float height, float width )
	{
		dialogContainerInstance = (GameObject) Instantiate( genericContainer );
		dialogContainerInstance.transform.parent = uiContainer.transform;
		
		dfControl dfContainer = dialogContainerInstance.GetComponent<dfControl>();
		dfContainer.RelativePosition = pos;
		dfContainer.Height = height;
		dfContainer.Width = width;
		
		// Set the outer container (Graphic element)
		dfControl outerContainer = dialogContainerInstance.GetComponent<dfControl>();
		outerContainer.RelativePosition = pos;
		outerContainer.Height = height;
		outerContainer.Width = width;
		
		// Create the dialog container (Layout element)
		dialogInstance = (dfScrollPanel) Instantiate( dialogContainer );
		dialogInstance.transform.parent = outerContainer.transform;
		dialogInstance.Width = outerContainer.Width;
		dialogInstance.Height = outerContainer.Height;
		dialogInstance.RelativePosition = pos; // pos - new Vector3(0,pos.y * 0.05f);
		
		// Fade dialog box in
		outerContainer.IsVisible = false;
		FadeRecipient fade = outerContainer.gameObject.AddComponent<FadeRecipient>();
		fade.DestroyObjectOnFadeOut( outerContainer.gameObject );
		fade.Fade();
		
		AttachOnClickMethod( dialogInstance.GetComponent<dfControl>(), "SkipTypewriterEffect" );
		return outerContainer.gameObject;
	}
	
	/// <summary>
	/// Adds dialog to container by fading it in.
	/// </summary>
	/// <param name="dialog">Dialog.</param>
	public void AddDialog( string dialog )
	{
		float scaler = IS4BY3 ? downscaler4by3 : downscaler;
		
		dfLabel label = (dfLabel) Instantiate( dialogLabel );
		label.transform.parent = dialogInstance.transform;
		
		label.Width = dialogInstance.Width - dialogInstance.ScrollPadding.left * 2;
		label.TextScale = font;//Screen.width * scaler;

		
		// Fade in with all text showing
		label.Text = dialog;
		label.IsVisible = false;
		FadeRecipient fade = label.gameObject.AddComponent<FadeRecipient>();
		fade.Fade();
	}
	
	/// <summary>
	/// Adds dialog to container with typewriter effect.
	/// </summary>
	/// <param name="dialog">Dialog.</param>
	public void AddDialog( string dialog, float typeEffectLength )
	{
		float scaler = IS4BY3 ? downscaler4by3 : downscaler;
		
		dfLabel label = (dfLabel) Instantiate( dialogLabel );
		label.transform.parent = dialogInstance.transform;
		label.RelativePosition = Vector3.zero;
		
		label.Width = dialogInstance.Width - dialogInstance.ScrollPadding.left * 2;
		label.TextScale = font;// Screen.width * scaler;

		
		// Display text letter by letter
		StartCoroutine( TypewriterEffect( label, dialog, typeEffectLength ) );
	}
	
	/// <summary>
	/// Adds dialog to container by fading it in.
	/// </summary>
	/// <param name="dialog">Dialog.</param>
	/// <param name="characterName">Character name.</param>
	public void AddDialog( string dialog, string characterName )
	{
		float scaler = IS4BY3 ? downscaler4by3 : downscaler;
		
		dfLabel label = (dfLabel) Instantiate( dialogLabel );
		label.transform.parent = dialogInstance.transform;
		
		label.Width = dialogInstance.Width - dialogInstance.ScrollPadding.left * 2;
		label.TextScale = font;//Screen.width * scaler;

		
		// Fade in with all text showing
		label.Text = characterName + ":\r\n" + dialog;
		label.IsVisible = false;
		FadeRecipient fade = label.gameObject.AddComponent<FadeRecipient>();
		fade.Fade();
	}
	
	/// <summary>
	/// Adds dialog to container with typewriter effect.
	/// </summary>
	/// <param name="dialog">Dialog.</param>
	/// <param name="characterName">Character name.</param>
	/// <param name="typeEffectLength">Type effect length.</param>
	public void AddDialog( string dialog, string characterName, float typeEffectLength )
	{
		float scaler = IS4BY3 ? downscaler4by3 : downscaler;
		
		dfLabel label = (dfLabel) Instantiate( dialogLabel );
		label.transform.parent = dialogInstance.transform;
		label.RelativePosition = Vector3.zero;
		
		label.Width = dialogInstance.Width - dialogInstance.ScrollPadding.left * 2;
		label.TextScale = font;// Screen.width * scaler;

		
		// Display text letter by letter
		StartCoroutine( TypewriterEffect( label, dialog, characterName, typeEffectLength ) );
	}
	
	/// <summary>
	/// Display the dialog letter by letter.
	/// </summary>
	/// <param name="label">Dialog container.</param>
	/// <param name="dialog">Dialog.</param>
	/// <param name="typeEffectLength">Length of the effect.</param>
	private IEnumerator TypewriterEffect( dfLabel label, string dialog, float typeEffectLength )
	{
		label.Text = "";
		typewriterEffectHappening = true;
		
		// Start scroll the message
		float delay = 1.0f / (float)dialog.Length * typeEffectLength;
		
		for (int i = 0; i < dialog.Length; i++)
		{
			if( !label )
				break;
			
			if( typewriterBreak )
			{
				label.Text = dialog;
				typewriterBreak = false;
				break;
			}
			
			label.Text += dialog[i];
			yield return new WaitForSeconds( delay );
		}
		
		typewriterEffectHappening = false;
	}
	
	/// <summary>
	/// Display the dialog letter by letter.
	/// </summary>
	/// <param name="label">Dialog container.</param>
	/// <param name="dialog">Dialog.</param>
	/// <param name="typeEffectLength">Length of the effect.</param>
	private IEnumerator TypewriterEffect( dfLabel label, string dialog, string characterName, float typeEffectLength )
	{
		label.Text = characterName + ":\r\n";
		typewriterEffectHappening = true;
		
		// Start scroll the message
		float delay = 1.0f / (float)dialog.Length * typeEffectLength;
		
		for (int i = 0; i < dialog.Length; i++)
		{
			if( !label )
				break;
			
			if( typewriterBreak )
			{
				label.Text = dialog;
				typewriterBreak = false;
				break;
			}
			
			label.Text += dialog[i];
			yield return new WaitForSeconds( delay );
		}
		
		typewriterEffectHappening = false;
	}
	
	/// <summary>
	/// Skips the typewriter effect and shows the entire dialog.
	/// </summary>
	public void SkipTypewriterEffect()
	{
		if( typewriterEffectHappening )
			typewriterBreak = true;
	}
	
	/// <summary>
	/// Removes the currently displayed dialog.
	/// </summary>
	public void RemoveCurrentDialog()
	{
		foreach (Transform oldDialog in dialogInstance.transform) 
		{
			Destroy (oldDialog.gameObject);
		}
	}
	
	public void HideDialogBox()
	{
		
		FadeRecipient fade;
		
		if( !dialogContainerInstance.GetComponent<FadeRecipient>() )
			fade = dialogContainerInstance.AddComponent<FadeRecipient>();
		else 
			fade = dialogContainerInstance.GetComponent<FadeRecipient>();
		
		fade.DestroyObjectOnFadeOut( dialogContainerInstance );
		fade.Fade();
	}
	
	#endregion
	
	#region Text messages
	
	/// <summary>
	/// Animates the text message in.
	/// </summary>
	/// <returns>Position of the text messages.</returns>
	/// <param name="message">Message.</param>
	public Vector3 ShowTextMessage( string message )
	{
		if( !textMessagesInstance )
		{
			textMessagesInstance = (dfScrollPanel) Instantiate( textMessageContainer );
			textMessagesInstance.transform.parent = uiContainer.transform;
			textMessagesInstance.gameObject.AddComponent<AudioSource>();
			textMessagesInstance.Position = new Vector3(screenHeight * 0.9f, -(screenHeight * 0.6f), 0);
			textMessagesInstance.Width = screenWidth * 0.2f;
		}
		
		// Add text message to container
		dfLabel textMsg = (dfLabel) Instantiate( textMessage );
		textMsg.transform.parent = textMessagesInstance.transform;
		textMsg.Text = message;
		textMsg.ZOrder = textMessagesInstance.transform.childCount - 1;
		textMsg.Tooltip = System.DateTime.Now.ToString("d");
		
		// Scaling
		textMsg.Width = textMessagesInstance.Width;
		textMsg.TextScale = screenWidth * 0.0013f;
		foreach( Transform date in textMsg.transform )
			date.GetComponent<dfLabel>().TextScale = textMsg.TextScale * 0.7f;
		
		// Fade the text message in
		textMsg.IsVisible = false;
		FadeRecipient fade = textMsg.gameObject.AddComponent<FadeRecipient>();
		// Override standard white fade in so the text is black
		fade.fadeInColor = new Color32(0,0,0,255); 
		fade.Fade();
		
		
		// Play sound
		textMessagesInstance.GetComponent<AudioSource>().PlayOneShot( textMessageSound );
		
		
		// Start coroutine to fade out + delete text after certain amount of time
		StartCoroutine( TextMessageDeleteTimer( textMsg.gameObject, 10.0f ) );
		
		return textMessagesInstance.Position;
	}
	
	/// <summary>
	/// Animates the text message in.
	/// </summary>
	/// <returns>Position of the text messages.</returns>
	/// <param name="message">Message.</param>
	/// <param name="timeUntilFadeout">Time until fadeout.</param>
	public Vector3 ShowTextMessage( string message, float timeUntilFadeout )
	{
		if( !textMessagesInstance )
		{
			textMessagesInstance = (dfScrollPanel) Instantiate( textMessageContainer );
			textMessagesInstance.transform.parent = uiContainer.transform;
			textMessagesInstance.gameObject.AddComponent<AudioSource>();
			textMessagesInstance.Position = new Vector3(screenHeight * 0.9f, -(screenHeight * 0.6f), 0);
			textMessagesInstance.Width = screenWidth * 0.2f;
		}
		
		// Add text message to container
		dfLabel textMsg = (dfLabel) Instantiate( textMessage );
		textMsg.transform.parent = textMessagesInstance.transform;
		textMsg.Text = message;
		textMsg.ZOrder = textMessagesInstance.transform.childCount - 1;
		textMsg.Tooltip = System.DateTime.Now.ToString("d");
		
		// Scaling
		textMsg.Width = textMessagesInstance.Width;
		textMsg.TextScale = screenWidth * 0.0013f;
		foreach( Transform date in textMsg.transform )
			date.GetComponent<dfLabel>().TextScale = textMsg.TextScale * 0.7f;
		
		// Fade the text message in
		textMsg.IsVisible = false;
		FadeRecipient fade = textMsg.gameObject.AddComponent<FadeRecipient>();
		// Override standard white fade in so the text is black
		fade.fadeInColor = new Color32(0,0,0,255); 
		fade.Fade();
		
		
		// Play sound
		textMessagesInstance.GetComponent<AudioSource>().PlayOneShot( textMessageSound );
		
		
		// Start coroutine to fade out + delete text after certain amount of time
		StartCoroutine( TextMessageDeleteTimer( textMsg.gameObject, timeUntilFadeout ) );
		
		return textMessagesInstance.Position;
	}
	
	/// <summary>
	/// Deletes specified text message after a certain amount of time.
	/// </summary>
	/// <returns>The message delete timer.</returns>
	/// <param name="textMessage">Text message.</param>
	/// <param name="waitTime">Wait time.</param>
	IEnumerator TextMessageDeleteTimer( GameObject textMessage, float waitTime )
	{
		yield return new WaitForSeconds( waitTime );
		
		// Fade out and delete the text message
		FadeRecipient fade = textMessage.GetComponent<FadeRecipient>();
		fade.DestroyObjectOnFadeOut( textMessage );
		fade.Fade();
		
		// If there are no more text messages, delete the container
		yield return new WaitForSeconds( fade.animationLength + 1.0f );
		if( textMessagesInstance && textMessagesInstance.transform.childCount < 1 )
			Destroy( textMessagesInstance.gameObject );
	}
	
	#endregion
	
	#region Full pages
	
	/// <summary>
	/// Shows the main menu.
	/// </summary>
	public void ShowMainMenu()
	{
		dfControl container = (dfControl) Instantiate( mainMenu );
		container.transform.parent = uiContainer.transform;
		container.Width = screenWidth;
		container.Height = screenHeight;
		container.Position = Vector3.zero;
		
		// Give the clickable items an event handler
		foreach( Transform childrenContainer in container.transform )
			foreach( Transform menuItem in childrenContainer.transform )
				if( menuItem.GetComponent<HighlightOnMouseOver>() )
					AttachOnClickMethod( menuItem.GetComponent<dfControl>(), "MainMenuItemClicked" );
		
		// Fade in
		container.IsVisible = false;
		FadeRecipient fade = container.gameObject.AddComponent<FadeRecipient>();
		fade.DestroyObjectOnFadeOut( container.gameObject );
		fade.Fade();
	}
	
	/// <summary>
	/// Shows the create menu.
	/// </summary>
	public void ShowCreateSession()
	{
		dfPanel create = (dfPanel)Instantiate( createSession );
		create.transform.parent = uiContainer.transform;
		create.RelativePosition = Vector3.zero;
		create.Width = screenWidth;
		create.Height = screenHeight;
		
		// Originally designed at 1080p, scale text down to retain design
		// create.GetComponent<ScaleText>().Scale( screenWidth * 0.0005f );
		
		// Fade in
		create.IsVisible = false;
		create.gameObject.AddComponent<FadeRecipient>().Fade();
	}
	
	/// <summary>
	/// Shows the about page.
	/// </summary>
	public void ShowAboutPage()
	{
		
		if( aboutPageInstance )
			return;
		
		FadeRecipient fade;
		aboutPageInstance = (dfPanel)Instantiate( aboutPage );
		aboutPageInstance.transform.parent = uiContainer.transform;
		aboutPageInstance.RelativePosition = Vector3.zero;
		aboutPageInstance.ZOrder = uiContainer.transform.childCount;
		aboutPageInstance.Width = screenWidth;
		aboutPageInstance.Height = screenHeight;
		aboutPageInstance.GetComponent<ScaleText>().Scale( screenWidth * 0.0005f );
		
		// Fade in
		aboutPageInstance.IsVisible = false;
		
		if( aboutPageInstance.gameObject.GetComponent<FadeRecipient>() ) 
			fade = aboutPageInstance.gameObject.GetComponent<FadeRecipient>();
		else
			fade = aboutPageInstance.gameObject.AddComponent<FadeRecipient>();
		
		fade.DestroyObjectOnFadeOut( aboutPageInstance.gameObject );
		fade.Fade();
	}
	
	#endregion
	
	/// <summary>
	/// Shows a small hints box in the center of the screen.
	/// </summary>
	/// <param name="hintText">Hint text.</param>
	public void ShowHints( string hintText, Vector3 pos, float width )
	{
		dfLabel thisHint = (dfLabel)Instantiate( hints );
		thisHint.transform.parent = uiContainer.transform;
		thisHint.GetComponent<Hints>().SetHintText( hintText );
		
		// thisHint.Position = new Vector3(0, -(screenHeight / 2 - thisHint.Height / 2), 0);
		thisHint.Position = pos;
		thisHint.Width = width;
		
		thisHint.IsVisible = false;
		thisHint.GetComponent<FadeRecipient>().animationLength = 0;
		thisHint.GetComponent<FadeRecipient>().Fade();
	}
	
	/// <summary>
	/// Fades in a black overlay.
	/// </summary>
	public void FadeInBlack()
	{
		return;
		GameObject black = (GameObject) Instantiate( blackOverlay );
		black.transform.parent = uiContainer.transform;
		black.transform.localPosition = Vector3.zero;
		black.name = "BlackOverlay";
		
		// Fade in
		black.GetComponent<dfControl>().IsVisible = false;
		FadeRecipient fade = black.GetComponent<FadeRecipient>(); 
		fade.animationLength = 1;
		fade.DestroyObjectOnFadeOut( black );
		fade.Fade();
	}
	
	/// <summary>
	/// Fades out the black overlay.
	/// </summary>
	public void FadeOutBlack()
	{
		GameObject bOverLay = GameObject.Find ("BlackOverlay");

		if(bOverLay)
			bOverLay.GetComponent<FadeRecipient>().Fade();
	}
	
	/// <summary>
	/// Calls supplied method from the RecieveGuiInput component when the element is clicked.
	/// </summary>
	/// <param name="control">dfControl of element.</param>
	/// <param name="methodName">Method name.</param>
	void AttachOnClickMethod( dfControl control, string methodName )
	{
		dfEventBinding clickToSendInfo = control.gameObject.AddComponent<dfEventBinding>();
		
		clickToSendInfo.DataSource = new dfComponentMemberInfo()
		{
			Component = control,
			MemberName = "Click"
		};
		
		clickToSendInfo.DataTarget = new dfComponentMemberInfo()
		{
			Component = GetComponent<RecieveGuiInput>(),
			MemberName = methodName
		};
	}
	
}
