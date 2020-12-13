using UnityEngine;
using System.Collections;

public class AARRoot : MonoBehaviour 
{
	[System.Serializable]
	public class FullImageSlide
	{
		public dfPanel panel;
		public dfSprite sprite;
		public dfSprite medalsSprite;
		public dfLabel bottomLabel;
	}

	public GameObject aarRoot;

	public ParticleSystem[] fireworks;

	public dfLabel title;

	public dfPanel upperNextButton;
	public dfPanel nextButton;
	public GameObject prevButton;

	public dfSprite layoutImage;
	public dfLabel[] layoutLabel;

	public dfLabel questionText;
	public dfLabel questionTextImage;
	public dfSprite questionImage;
	public dfLabel bottomText;
	public dfScrollPanel radioContainer;

	public dfPanel sliderPanel;
	public dfSlider slider;
	public dfLabel slideLabel;
	public SliderBehavior sliderBehavior;
	public dfButton acceptButton;

	public FullImageSlide fullImageSlide;

	public WheelSpinner wheelSpinner;
}
