using UnityEngine;
using System.Collections;

public class ScaleText : MonoBehaviour {

	public dfLabel[] labels;
	public dfButton[] buttons;

	public void Scale( float scalePercent )
	{
		for( var i = 0; i < labels.Length; i++ )
		{
			labels[i].TextScale *= scalePercent;
		}

		for( var i = 0; i < buttons.Length; i++ )
		{
			buttons[i].TextScale *= scalePercent;
		}
	}
}
