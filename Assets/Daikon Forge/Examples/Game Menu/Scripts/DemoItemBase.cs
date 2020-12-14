using UnityEngine;

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu( "Daikon Forge/Examples/Game Menu/Demo Item Base Class" )]
[Serializable]
public class DemoItemBase : MonoBehaviour, IComparable<DemoItemBase>
{

	#region Serialized values 

	public string Tooltip;

	#endregion

	#region Private variables

	protected dfControl owner;
	protected dfControl hilite;

	#endregion

	#region Public properties

	public dfControl Control
	{
		get
		{
			if( owner == null )
			{
				owner = GetComponent<dfControl>();
			}
			return owner;
		}
	}

	#endregion

	#region Public methods

	public void Initialize()
	{
		hilite = GetComponentsInChildren<dfControl>().FirstOrDefault( i => i.name == "hilite" );
	}

	public virtual void Focus()
	{
		Control.Focus();
	}

	#endregion

	#region dfControl events

	protected virtual void OnKeyPress( dfControl sender, dfKeyEventArgs args )
	{
		if( args.KeyCode == KeyCode.Return )
		{
			Control.DoClick();
		}
	}

	protected virtual void OnMouseEnter()
	{
		Control.Focus();
	}

	protected virtual void OnEnterFocus( dfControl sender, dfFocusEventArgs args )
	{
		hilite.Show();
		hilite.GetComponent<dfTweenComponentBase>().Play();
	}

	protected virtual void OnLeaveFocus( dfControl sender, dfFocusEventArgs args )
	{
		hilite.Hide();
		hilite.GetComponent<dfTweenComponentBase>().Stop();
	}

	#endregion

	#region IComparable<DemoMenuItem> Members

	public int CompareTo( DemoItemBase other )
	{
		// Since RenderOrder is intimately related to ZOrder, this has the 
		// effect of sorting any list of DemoMenuItem instances by ZOrder.
		// This is especially useful for re-ordering items in a ScrollPanel,
		// for instance. Just change the ZOrder value of the dfControl to re-order.
		return this.Control.RenderOrder.CompareTo( other.Control.RenderOrder );
	}

	#endregion

}
