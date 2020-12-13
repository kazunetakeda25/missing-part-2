using UnityEngine;
using System.Collections;

public class SliderBehavior : MonoBehaviour {

	public dfControl 
		sliderContainer,
		acceptButton,
		sliderValue;

	public dfSlider
		slider;

	public dfLabel
		sliderLabel;

	void Start()
	{
		InitSlider();
	}

	public void SliderChanged(dfControl control, float value)
	{
		slider.ValueChanged -= SliderChanged;
		if( sliderValue )
			sliderValue.IsVisible = true;
		acceptButton.IsEnabled = true;
		acceptButton.IsVisible = true;
		acceptButton.IsInteractive = true;	
	}

	/// <summary>
	/// Enables the accept button by fading out the overlay.
	/// </summary>
	/// <param name="control">Control.</param>
	public void EnableAcceptButton( dfControl control, System.Boolean value )
	{
		FadeRecipient fade = control.gameObject.AddComponent<FadeRecipient>();
		fade.DestroyObjectOnFadeOut( control.gameObject );
		fade.fadeInColor = control.Color;
		fade.Fade();
	}

	private void InitSlider()
	{
		if( sliderValue )
			sliderValue.IsVisible = false;
		slider.ValueChanged += SliderChanged;
	}

	public void ResetSlider()
	{
		InitSlider();
	}

	public void SetPosition( Vector3 pos )
	{
		sliderContainer.Position = pos;
	}

	public void SetWidth( float width )
	{
		sliderContainer.Width = Mathf.Round( width );
	}

	public void SetupSlider( int min, int max, int stepSize, int sliderStartPosition )
	{
		slider.MinValue = min;
		slider.MaxValue = max;
		slider.StepSize = stepSize;
		slider.Value = sliderStartPosition;
	}

	public void SetText( string label )
	{
		sliderLabel.IsVisible = true;
		sliderLabel.Text = label;
	}

	public void SendDataOnclick( RecieveGuiInput recieve, string methodName )
	{
		dfEventBinding clickToSendInfo = acceptButton.gameObject.AddComponent<dfEventBinding>();
		clickToSendInfo.DataSource = new dfComponentMemberInfo()
		{
			Component = acceptButton,
			MemberName = "Click"
		};
		
		clickToSendInfo.DataTarget = new dfComponentMemberInfo()
		{
			Component = recieve,
			MemberName = methodName
		};
	}

}
