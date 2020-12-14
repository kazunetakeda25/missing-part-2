using UnityEngine;

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu( "Daikon Forge/Examples/Game Menu/Menu Panel" )]
[Serializable]
public class DemoMenuPanel : DemoPanelBase
{

	#region Public fields 

	public dfLabel Tooltip;
	public bool AutoShow = false;

	#endregion

	#region Private variables

	private DemoMenuItem selected = null;
	private List<DemoMenuItem> items; 

	#endregion

	#region Private utility methods 

	public override void Focus()
	{

		owner.Show();
		owner.Enable();

		if( selected == null || !selected.Control.IsEnabled )
		{
			selected = items.FirstOrDefault( i => i.Control.IsEnabled );
		}

		if( selected != null )
		{
			selected.Focus();
		}

	}

	protected override void initialize()
	{

		base.initialize();

		if( Tooltip == null )
		{ 
			Tooltip = findControl( "Tooltip" ) as dfLabel;
		}

		if( Tooltip != null )
		{
			Tooltip.Text = "";
		}

		#region Initialize menu items 

		items = GetComponentsInChildren<DemoMenuItem>().ToList();
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

				if( args.Used )
					return;

				if( args.KeyCode == KeyCode.DownArrow )
				{
					selectNext( itemIndex );
					args.Use();
				}
				else if( args.KeyCode == KeyCode.UpArrow )
				{
					selectPrevious( itemIndex );
					args.Use();
				}
				else if( args.KeyCode == KeyCode.Space || args.KeyCode == KeyCode.Return )
				{
					var target = item.Submenu;
					if( target != null )
					{
						showSubmenu( target );
					}
					args.Use();
				}

			};

			// Display the item's tooltip on focus
			item.Control.EnterFocus += ( dfControl sender, dfFocusEventArgs args ) =>
			{
				selected = item;
				if( Tooltip != null )
				{
					Tooltip.Text = item.Tooltip;
				}
			};

			// Display the item's submenu on click
			item.Control.Click += ( dfControl sender, dfMouseEventArgs args ) =>
			{

				if( args.Used )
					return;

				var target = item.Submenu;
				if( target != null )
				{
					showSubmenu( target );
				}

				args.Use();

			};

		}

		#endregion

		if( AutoShow )
		{
			this.Show();
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

	private void showSubmenu( DemoPanelBase target )
	{

		owner.Unfocus();
		owner.Disable();

		if( !panelStack.Contains( target ) )
		{
			panelStack.Push( this );
		}
		else
		{
			panelStack.Pop();
		}

		target.Show();

	}

	#endregion

}
