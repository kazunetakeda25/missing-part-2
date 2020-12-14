using UnityEngine;

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu( "Daikon Forge/Examples/Game Menu/Option Panel" )]
[Serializable]
public class DemoOptionPanel : DemoPanelBase
{

	#region Private variables

	private dfLabel tooltip;
	private List<DemoItemBase> items;

	#endregion

	protected override void initialize()
	{

		base.initialize();

		tooltip = findControl( "Tooltip" ) as dfLabel;
		if( tooltip != null )
		{
			tooltip.Text = "";
		}

		initializeOptionItems();

	}

	public override void Focus()
	{

		owner.Show();
		owner.Enable();

		if( items.Count > 0 )
		{
			items[ 0 ].Focus();
		}
		else
		{

			var control =
				GetComponentsInChildren<dfControl>()
				.Where( c => c.CanFocus )
				.OrderBy( c => c.RenderOrder )
				.FirstOrDefault();

			if( control != null )
			{
				control.Focus();
			}

		}

	}

	public override void GoBack()
	{

		if( panelStack.Count == 0 )
			return;

		this.Hide();

		panelStack.Pop().Focus();

	}

	private void initializeOptionItems()
	{

		items = GetComponentsInChildren<DemoItemBase>().ToList();
		items.Sort();

		var length = items.Count;
		for( int i = 0; i < length; i++ )
		{

			var item = items[ i ];
			var itemIndex = i;

			// Perform item-specific initialization
			item.Initialize();

			// Attach keyboard navigation events
			item.Control.KeyDown += ( dfControl sender, dfKeyEventArgs args ) =>
			{

				if( args.Used ) return;

				var key = args.KeyCode;
				if( key == KeyCode.Tab )
				{
					if( args.Shift )
						key = KeyCode.UpArrow;
					else
						key = KeyCode.DownArrow;
				}

				if( key == KeyCode.DownArrow )
				{
					selectNext( itemIndex );
					args.Use();
				}
				else if( key == KeyCode.UpArrow )
				{
					selectPrevious( itemIndex );
					args.Use();
				}

			};

			if( tooltip != null )
			{

				// Display the item's tooltip on focus
				item.Control.EnterFocus += ( dfControl sender, dfFocusEventArgs args ) =>
				{
					tooltip.Text = item.Tooltip;
				};

			}

		}

	}

	private void selectPrevious( int index )
	{
		while( --index >= 0 )
		{
			if( items[ index ].Control.IsEnabled )
			{
				items[ index ].Focus();
				return;
			}
		}
	}

	private void selectNext( int index )
	{
		while( ++index < items.Count )
		{
			if( items[ index ].Control.IsEnabled )
			{
				items[ index ].Focus();
				return;
			}
		}
	}

}
