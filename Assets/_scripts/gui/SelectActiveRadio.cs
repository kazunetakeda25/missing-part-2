using UnityEngine;
using System.Collections;

public class SelectActiveRadio : MonoBehaviour {

	public string activeRadioValue = "";

	private string 
		activeRadio = "interface_radio_selected_small", 
		inactiveRadio = "interface_radio_small";

	void Start()
	{
		foreach( Transform child in transform )
			AttachOnClickMethod( child.GetComponent<dfControl>(), "SetActiveRadio" );
	}

	/// <summary>
	/// Sets the clicked radio button to active.
	/// </summary>
	/// <param name="control">Control.</param>
	/// <param name="mouseEvent">Mouse event.</param>
	public void SetActiveRadio( dfControl control, dfMouseEventArgs mouseEvent )
	{
		// Set all buttons to inactive
		foreach( Transform sibling in control.transform.parent )
		{
			foreach( Transform siblingChild in sibling )
			{
				if( siblingChild.GetComponent<dfButton>() )
					siblingChild.GetComponent<dfButton>().BackgroundSprite = inactiveRadio;
			}
		}
		
		
		// Get the active button
		foreach( Transform child in control.transform )
		{
			// Record value
			activeRadioValue = control.Tooltip;
			
			if( child.GetComponent<dfButton>() )
				child.GetComponent<dfButton>().BackgroundSprite = activeRadio;
		}
		
		Debug.Log (activeRadioValue);
	}

	/// <summary>
	/// Calls supplied method when element is clicked.
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
			Component = this,
			MemberName = methodName
		};
	}
}
