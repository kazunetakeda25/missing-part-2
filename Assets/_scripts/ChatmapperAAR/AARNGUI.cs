using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class AARNGUI : MonoBehaviour 
{
	public delegate void OnNextHit(); 
	public event OnNextHit onNextHit;

	public enum SlideType
	{
		DISABLED,
		NORMAL_BODY,
		IMAGE_BODY
	}
	
	public enum SlideLogo
	{
		None,
		Anchoring,
		Projection,
		LastOne
	}

	public UILabel titleLabel;

	private UILabel bodyLabel;
	public UILabel BodyLabel
	{
		get { 
			if(bodyLabel == null) {
				Debug.LogWarning("Body Label Unassigned!");
				return mainBodyLabel;
			}
			return bodyLabel;
		}
	}
	[SerializeField] private UILabel mainBodyLabel;
	[SerializeField] private UILabel imageBodyLabel;

	[SerializeField] private UITexture image;
	[SerializeField] private UITexture logo;
	[SerializeField] private GameObject NextButton;

	[SerializeField] private GameObject fireworkRoot;
	[SerializeField] private GameObject[] fireworks;
	private List<ParticleSystem> fx = new List<ParticleSystem>();

	private MoviePlayer moviePlayer;
	private bool moviePlaying;

	[SerializeField] private SliderNumber sliderNumber;
	[SerializeField] private UISlider slider;
	public AARRadio[] radios;
	private int currentRadioIndex = -1;
	public int CurrentRadioIndex { get { return (int) currentRadioIndex; } }
	private float maxSliderValue;
	private float minSliderValue;

	private bool sliderChanged = true;

	[SerializeField] private GameObject rootTitle;
	[SerializeField] private GameObject rootLogo;
	[SerializeField] private GameObject rootNormalBody;
	[SerializeField] private GameObject rootQuizImage;
	[SerializeField] private GameObject rootImageBody;
	[SerializeField] private GameObject rootRadios;
	[SerializeField] private GameObject rootSlider;
	[SerializeField] private GameObject rootMedals;

	public UISprite medal1;
	public UISprite medal2;
	public UISprite medal3;

	public WheelSpinner wheel;

	public Texture2D anchorIcon;
	public Texture2D projectionIcon;
	public Texture2D represenativeIcon;

	private bool updateRadios = false;
	private int radioCount = 0;

	private bool delayingNextButton = false;
	private bool audioHasStartedPlaying = false;

	public void OnRadioHit(int buttonIndex)
	{
		Debug.Log("Radio: " + buttonIndex);
		currentRadioIndex = buttonIndex;

		if(AreAllButtonsUnChecked()) {
			NGUITools.SetActive(NextButton, false);
		} else {
			NGUITools.SetActive(NextButton, true);
		}
	}

	private bool AreAllButtonsUnChecked()
	{

		foreach(AARRadio radio in radios) {
			if(radio.toggle.value == true)
				return false;
		}

		return true;
	}

	public void PlayAudio(string audioFile, bool delayNextButton)
	{
		this.GetComponent<AudioSource>().clip = Resources.Load(audioFile) as AudioClip;
	
		if(this.GetComponent<AudioSource>().clip == null) {
			ShowNextButton();
			return;
		}

		this.GetComponent<AudioSource>().Play();

		if(delayNextButton) {
			audioHasStartedPlaying = false;
			delayingNextButton = true;
		}
	}

	public void PlayMovie(string[] paths)
	{
		ClearGUI();
		GameObject moviePlayerGO = new GameObject("AAR Movie Player");
		moviePlayer = moviePlayerGO.AddComponent<MoviePlayer>();

		moviePlayer.SetupMovie(paths, MoviePlayer.Type.OVERLAY, 0.75f);

		moviePlayer.PlayMovie();
		moviePlaying = true;
	}

	private void Update()
	{
		if(MissingComplete.AARQuitMenu.Instance.IsPaused == true) {
			return;
		}

		if(audioHasStartedPlaying == false && this.GetComponent<AudioSource>().isPlaying == true) {
			audioHasStartedPlaying = true;
		}

		if(delayingNextButton && audioHasStartedPlaying) {

			if(this.GetComponent<AudioSource>().isPlaying == false) {
				ShowNextButton();
			}

			if(this.GetComponent<AudioSource>().time > 5.0f) {
				ShowNextButton();
			}
		}

		if(moviePlaying && moviePlayer == null)
		{
			moviePlaying = false;
			OnNextButtonHit();
		}
	}

	private void LateUpdate()
	{
		if(updateRadios == true) {
			for(int i = 0; i < radioCount; i++) {
				NGUITools.SetActive(radios[i].gameObject, true);
			}
			updateRadios = false;
		}
	}

	public void ClearGUI()
	{
		ClearCurrentGUIElements();
	}

	public void ShowNextButton()
	{
		Debug.Log("next");
		delayingNextButton = false;
		NGUITools.SetActive(NextButton, true);
	}

	public void OnNextButtonHit()
	{
		NGUITools.SetActive(NextButton, false);

		this.GetComponent<AudioSource>().Stop();

		audioHasStartedPlaying = false;
		delayingNextButton = false;

		foreach(ParticleSystem storedFX in fx) {

			if(storedFX.GetComponent<AudioSource>()) {
				storedFX.GetComponent<AudioSource>().Stop();
			}

			GameObject.Destroy(storedFX.gameObject);
		}

		fx.Clear();

		if(onNextHit != null)
			onNextHit();
	}

	public void SetupBasicSlide(string imagePath, Narrator narrator)
	{
		ClearGUI();

		SlideType type = (imagePath.Length == 0) ? SlideType.NORMAL_BODY : SlideType.IMAGE_BODY;

		SetSlideStyle(type, imagePath);
		SetLogo(narrator);
	}

	public void SetupRadios(string[] radioDatas)
	{

		currentRadioIndex = -1;

		updateRadios = true;
		radioCount = radioDatas.Length;

		for(int i = 0; i < radioDatas.Length; i++) {

			radios[i].label.text = radioDatas[i];
			radios[i].toggle.value = false;

		}
	}

	public void PlayFireworks()
	{
		if(Settings.Instance.Scoring == true) {

			foreach(GameObject fxGO in fireworks) {
				GameObject specialEffect = (GameObject) GameObject.Instantiate(fxGO, fireworkRoot.transform.position, fireworkRoot.transform.rotation);
				specialEffect.transform.parent = fireworkRoot.transform;

				if(specialEffect.GetComponent<AudioSource>() != null)
					specialEffect.GetComponent<AudioSource>().PlayDelayed(0.3f);

				if(specialEffect.GetComponent<ParticleSystem>() != null) {
					fx.Add(specialEffect.GetComponent<ParticleSystem>());
					specialEffect.GetComponent<ParticleSystem>().Play();
				}

				fxGO.GetComponent<ParticleSystem>().Play();
			}
		}
	}

	public void SetupSlider(float minVal, float maxVal, float startVal)
	{

		this.maxSliderValue = maxVal;
		this.minSliderValue = minVal;

		sliderNumber.SetMinMaxVals(minVal, maxVal);
		float range = maxVal - minVal;
		float adjustedStartVal = startVal - minVal;
		Debug.Log("Range: " + adjustedStartVal / range);
		slider.value = adjustedStartVal / range;
		NGUITools.SetActive(rootSlider, true);

		slider.onDragFinished += OnSliderReleased;
	}

	public void OnSliderReleased()
	{
		if(NGUITools.GetActive(NextButton) == false) {
			ShowNextButton();
		}
	}

	public float GetSliderValueAdjusted()
	{
		float currentValue = (maxSliderValue - minSliderValue) * slider.value;
		currentValue += minSliderValue;
		return currentValue;
	}

	public void ShowMedals()
	{
		NGUITools.SetActive(rootMedals, true);
	}

	public void SetTitle(string title)
	{
		NGUITools.SetActive(titleLabel.gameObject, true);
		titleLabel.text = title;
	}

	public void TurnOnMedals()
	{
		NGUITools.SetActive(rootMedals, true);
	}

	private void ClearCurrentGUIElements()
	{
		//NGUITools.SetActive(rootRadios, false);
		NGUITools.SetActive(rootLogo, false);
		NGUITools.SetActive(titleLabel.gameObject, false);
		NGUITools.SetActive(rootNormalBody, false);
		NGUITools.SetActive(rootQuizImage, false);
		NGUITools.SetActive(rootImageBody, false);
		NGUITools.SetActive(rootSlider, false);
		NGUITools.SetActive(rootMedals, false);

		foreach(AARRadio radio in radios) {
			radio.toggle.value = false;
			NGUITools.SetActive(radio.gameObject, false);
		}
	}

	private void SetSlideStyle(SlideType type, string imagePath)
	{
		bodyLabel = null;

		//Debug.Log("Setting Slide Up For: " + type.ToString());

		switch(type)
		{
		case SlideType.DISABLED:
			return;
		case SlideType.NORMAL_BODY:
			NGUITools.SetActive(rootNormalBody, true);
			bodyLabel = mainBodyLabel;
			break;
		case SlideType.IMAGE_BODY:
			NGUITools.SetActive(rootQuizImage, true);
			NGUITools.SetActive(rootImageBody, true);
			bodyLabel = imageBodyLabel;
			image.mainTexture = Resources.Load(imagePath) as Texture2D;
			break;
		}
	}

	private void SetLogo(Narrator narrator) {
		NGUITools.SetActive(rootLogo, true);

		switch(narrator) {
		case Narrator.Anchoring:
			logo.mainTexture = anchorIcon;
			break;
		case Narrator.Projection:
			logo.mainTexture = projectionIcon;
			break;
		case Narrator.Represenative:
			logo.mainTexture = represenativeIcon;
			break;
		case Narrator.Computer:
		case Narrator.Generic:
		case Narrator.SME:
		case Narrator.SME2:
			NGUITools.SetActive(rootLogo, false);
			break;
		}
	}

}
