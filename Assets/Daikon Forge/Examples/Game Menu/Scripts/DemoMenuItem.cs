using UnityEngine;

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu( "Daikon Forge/Examples/Game Menu/Menu Item" )]
[Serializable]
public class DemoMenuItem : DemoItemBase, IComparable<DemoMenuItem>
{

	#region Public fields 

	public DemoPanelBase Submenu;

	#endregion

	#region IComparable<DemoMenuItem> Members

	public int CompareTo( DemoMenuItem other )
	{
		// Since RenderOrder is intimately related to ZOrder, this has the 
		// effect of sorting any list of DemoMenuItem instances by ZOrder.
		// This is especially useful for re-ordering items in a ScrollPanel,
		// for instance. Just change the ZOrder value of the dfControl to re-order.
		return this.Control.RenderOrder.CompareTo( other.Control.RenderOrder );
	}

	#endregion

}
