//using UnityEngine;
//using System.Collections;
//
//public class AARManagerInputCatch : MonoBehaviour {
//
//	private AARRoot root;
//	private AARManager aarManager;
//
//	private int? selectedRadioButton = 0;
//
//	private void Awake()
//	{
//		aarManager = this.gameObject.GetComponent<AARManager>();
//		root = aarManager.Root;
//	}
//
//	public void OnPrevButtonClicked() { }
//	
//	public void OnNextButtonClicked( dfControl control, dfMouseEventArgs mouseEvent ) 
//	{
//		Debug.Log ("On Next Button Clicked!  " + control.name + mouseEvent.Clicks);
//		root.nextButton.IsEnabled = false;
//		root.nextButton.IsVisible = false;
//		root.nextButton.IsInteractive = false;
//		
//		//	 	dfEventBinding[] bindings = root.nextButton.gameObject.GetComponentsInChildren<dfEventBinding>();
//		//
//		//		for (int i = 0; i < bindings.Length; i++) {
//		//			Debug.LogWarning("Deleting Binding");
//		//			Destroy(bindings[i]);
//		//		}
//		
//		root.radioContainer.IsEnabled = false;
//		
//		//Quiz Slider is a two-part question, so when the next button is hit, we want to show the Slider to allow for input.
//		if(aarManager.CurrentSlide is AARSlideQuizSlider)
//		{
//			if(selectedRadioButton != null)
//				SavePreSliderRadioValue();
//			
//			ShowSlider();
//			return;
//		}
//		
//		if(showQuizResponseText)
//		{
//			NextSlide();
//			return;
//		}
//		
//		if(aarManager.CurrentSlide is AARSlideQuizMultipleChoice)
//		{
//			ProcessMultipleChoiceAnswer();
//			return;
//		}
//		
//		ClearSlide();
//		
//		NextSlide();
//	}
//	
//	public void OnSliderAcceptButtonHit(dfControl control, dfMouseEventArgs mouseEvent) 
//	{
//		AARSlideQuizSlider mySlide = aarManager.CurrentSlide as AARSlideQuizSlider;
//		SaveSliderValue();
//		
//		root.acceptButton.IsInteractive = false;
//		root.slider.IsInteractive = false;
//		showQuizResponseText = true;
//		
//		ShowSlide((int) NumberTester.GetNextSlide(mySlide));
//	}
//
//	public void OnRadioHit ( dfControl control, dfMouseEventArgs mouseEvent )
//	{
//		if Debug.Log ("Radio - " + control.name + " hit!");
//		
//		switch(control.name)
//		{
//		case "radio0":
//			selectedRadioButton = 0;
//			break;
//		case "radio1":
//			selectedRadioButton = 1;
//			break;
//		case "radio2":
//			selectedRadioButton = 2;
//			break;
//		case "radio3":
//			selectedRadioButton = 3;
//			break;
//		case "radio4":
//			selectedRadioButton = 4;
//			break;
//		}
//		
//		CreateNextButton();
//	}
//}
