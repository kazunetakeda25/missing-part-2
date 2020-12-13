using UnityEngine;
using System.Collections;
[RequireComponent(typeof(UILabel))]
public class SliderNumber : MonoBehaviour 
{
	public UILabel label;
	public UISlider mySlider;

	private int minVal;
	private int maxVal;

	public void SetMinMaxVals(float minVal, float maxVal)
	{
		SetMinMaxVals(Mathf.RoundToInt(minVal), Mathf.RoundToInt(maxVal));
	}

	public void SetMinMaxVals(int minVal, int maxVal)
	{
		this.minVal = minVal;
		this.maxVal = maxVal;
	}

	private void Awake()
	{
		label = this.GetComponent<UILabel>();
	}

	private void LateUpdate()
	{
		int currentValue = Mathf.RoundToInt((maxVal - minVal) * mySlider.value);
		currentValue += minVal;
		label.text = currentValue.ToString();
	}
}
