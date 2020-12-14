using UnityEngine;

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu( "Daikon Forge/Examples/Game Menu/List Field" )]
[Serializable]
public class DemoListField : DemoItemBase
{

	public List<string> Items = new List<string>();
	public List<string> ItemData = new List<string>();

	public int SelectedIndex = 0;

	void Start()
	{
		showSelection();
		updateButtons();
	}

	protected override void OnEnterFocus( dfControl sender, dfFocusEventArgs args )
	{
		base.OnEnterFocus( sender, args );
		updateButtons();
	}

	protected override void OnLeaveFocus( dfControl sender, dfFocusEventArgs args )
	{
		base.OnLeaveFocus( sender, args );
		updateButtons();
	}

	void OnMouseWheel( dfControl sender, dfMouseEventArgs args )
	{

		if( args.Used )
			return;

		args.Use();

		if( args.WheelDelta > 0 )
			SelectPrevious();
		else
			SelectNext();

	}

	void OnClick( dfControl sender, dfMouseEventArgs args )
	{

		var prevButton = this.Control.Find( "Prev Button" );
		var nextButton = this.Control.Find( "Next Button" );

		if( args.Source == prevButton )
		{
			SelectPrevious();
		}
		else if( args.Source == nextButton )
		{
			SelectNext();
		}

	}

	void OnKeyDown( dfControl sender, dfKeyEventArgs args )
	{

		if( args.KeyCode == KeyCode.LeftArrow )
		{
			SelectPrevious();
		}
		else if( args.KeyCode == KeyCode.RightArrow )
		{
			SelectNext();
		}

	}

	private void SelectPrevious()
	{

		var lastSelectedIndex = SelectedIndex;

		SelectedIndex = Mathf.Max( 0, SelectedIndex - 1 );

		if( lastSelectedIndex != SelectedIndex )
		{
			showSelection();
			SendMessage( "OnSelectedItemChanged", this, SendMessageOptions.DontRequireReceiver );
		}

	}

	private void SelectNext()
	{

		var lastSelectedIndex = SelectedIndex;

		SelectedIndex = Mathf.Min( SelectedIndex + 1, Items.Count - 1 );

		if( lastSelectedIndex != SelectedIndex )
		{
			showSelection();
			SendMessage( "OnSelectedItemChanged", this, SendMessageOptions.DontRequireReceiver );
		}

	}

	private void updateButtons()
	{

		var prevButton = this.Control.Find( "Prev Button" );
		if( prevButton != null )
		{
			prevButton.IsVisible = Control.ContainsFocus && SelectedIndex > 0;
		}

		var nextButton = this.Control.Find( "Next Button" );
		if( nextButton != null )
		{
			nextButton.IsVisible = Control.ContainsFocus && SelectedIndex < Items.Count - 1;
		}

	}

	void showSelection()
	{

		var label = this.Control.Find( "Item" ) as dfLabel;
		if( label != null )
		{
			label.Text = SelectedIndex >= 0 && SelectedIndex <= Items.Count - 1 ? Items[ SelectedIndex ] : "";
		}

		updateButtons();

	}

}
