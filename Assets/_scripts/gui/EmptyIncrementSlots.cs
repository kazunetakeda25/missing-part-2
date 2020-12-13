using UnityEngine;
using System.Collections;

public class EmptyIncrementSlots : MonoBehaviour {

	public dfSlider slider;
	public dfSprite incrementMarker;

	private dfScrollPanel slotContainer;

	public void ShowEmptySlots( bool showValueAboveIncrement ) {

		slotContainer = gameObject.GetComponent<dfScrollPanel>();

		float numSteps = Mathf.Floor( (slider.MaxValue - slider.MinValue) / slider.StepSize );


		// Set padding for each slot
		slotContainer.FlowPadding = new RectOffset(
		    slotContainer.FlowPadding.left,
			(int) Mathf.Floor( ( slider.Width - incrementMarker.Width * numSteps ) / (numSteps ) ), 
			slotContainer.FlowPadding.top, 
			slotContainer.FlowPadding.bottom 
		);


		// Add the required number of increments
		for( int i = 0; i < numSteps + 1; i++ )
		{
			dfSprite slot = (dfSprite) Instantiate(incrementMarker);
			slot.transform.parent = transform;
			slot.name = "Increment - " + ( slider.StepSize * i + slider.MinValue ).ToString();
			slot.Tooltip = showValueAboveIncrement ? ( slider.StepSize * i + slider.MinValue ).ToString() : "";
			slot.ZOrder = transform.childCount;
		}
	}
	
}
