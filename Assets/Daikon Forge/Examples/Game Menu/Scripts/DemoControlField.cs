using UnityEngine;

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu( "Daikon Forge/Examples/Game Menu/Control Field" )]
[Serializable]
public class DemoControlField : DemoItemBase
{

	void OnGotFocus( dfControl sender, dfFocusEventArgs args )
	{

		if( sender == Control )
		{

			var control =
				GetComponentsInChildren<dfControl>()
				.Where( c => c != Control && c.CanFocus )
				.OrderBy( c => c.RenderOrder )
				.FirstOrDefault();

			if( control != null )
			{
				control.Focus();
			}

		}

	}

}
