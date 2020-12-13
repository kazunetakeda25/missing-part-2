using UnityEngine;
using System.Collections;

public class TextboxNumbersOnly : MonoBehaviour {

	private dfTextbox textbox;
	private string valueOfTextbox;

	void Start()
	{
		textbox = gameObject.GetComponent<dfTextbox>();
		valueOfTextbox = textbox.Text;
	}

	public void OnTextChanged( dfControl control, System.String value )
	{
		#if !UNITY_EDITOR

		if( value == "" )
		{
			valueOfTextbox = "";
			textbox.Text = valueOfTextbox;
			return;
		}


		int newValue = 0;
		int.TryParse(value.ToString(), out newValue);

		if( newValue == 0 )
		{
			textbox.Text = valueOfTextbox;
			textbox.CursorIndex = valueOfTextbox.Length;
		}
		else
		{
			textbox.Text = newValue.ToString();
			valueOfTextbox = newValue.ToString();
		}

		#endif
	}
}
