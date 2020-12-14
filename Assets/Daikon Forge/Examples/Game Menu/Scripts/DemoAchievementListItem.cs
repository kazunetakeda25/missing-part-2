using System;
using System.Collections;

using UnityEngine;

[AddComponentMenu( "Daikon Forge/Examples/Game Menu/Achievement List Item" )]
public class DemoAchievementListItem : MonoBehaviour
{

	#region Private variables 

	private dfLabel NameLabel;
	private dfLabel DescriptionLabel;
	private dfLabel ProgressLabel;
	private dfSprite Icon;
	private dfProgressBar ProgressBar;
	private dfPanel Container;

	#endregion

	#region Public methods 

	public void Bind( DemoAchievementInfo data )
	{
		NameLabel.Text = data.AchievementName;
		DescriptionLabel.Text = data.Description;
		ProgressBar.Value = data.Progress;
		ProgressLabel.Text = data.FormattedProgress;
	}

	public void Expand()
	{

		var grid = Container.Parent as dfScrollPanel;
		var width = grid.Width - grid.FlowPadding.horizontal - grid.ScrollPadding.horizontal;

		Container.Anchor = dfAnchorStyle.Left | dfAnchorStyle.Right;
		Container.Width = width;
		Icon.RelativePosition = new Vector3( 10, ( Container.Height - Icon.Height ) * 0.5f );

		DescriptionLabel.Show();
		DescriptionLabel.RelativePosition = Icon.RelativePosition + new Vector3( Icon.Width + 10, 0 );
		DescriptionLabel.Width = Container.Width - DescriptionLabel.RelativePosition.x - 10;


	}

	public void Collapse()
	{

		Container.Anchor = dfAnchorStyle.None;
		Container.Width = 128;

		Icon.RelativePosition = ( Container.Size - Icon.Size ) * 0.5f;

		DescriptionLabel.Hide();

	}

	#endregion

	#region Unity events 

	void OnEnable() 
	{

		Container = GetComponent<dfPanel>();
		if( Container == null )
		{
			this.enabled = false;
		}

		NameLabel = Container.Find<dfLabel>( "Name" );
		DescriptionLabel = Container.Find<dfLabel>( "Description" );
		ProgressLabel = Container.Find<dfLabel>( "ProgressLabel" );
		Icon = Container.Find<dfSprite>( "Icon" );
		ProgressBar = Container.Find<dfProgressBar>( "ProgressBar" );

		Container.BackgroundSprite = "frame-style6";
		NameLabel.BackgroundSprite = null;
		ProgressBar.Opacity = 0.5f;

	}

	#endregion

	#region dfControl events 

	void OnMouseEnter()
	{
		Container.Focus();
	}

	void OnEnterFocus()
	{
		Container.BackgroundSprite = "frame-style7";
		NameLabel.BackgroundSprite = "heading-style1";
		Icon.Opacity = 1f;
		ProgressBar.Opacity = 1f;
	}

	void OnLeaveFocus()
	{
		Container.BackgroundSprite = "frame-style6";
		NameLabel.BackgroundSprite = null;
		Icon.Opacity = 0.75f;
		ProgressBar.Opacity = 0.5f;
	}

	#endregion

}
