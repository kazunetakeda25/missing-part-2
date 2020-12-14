using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

[AddComponentMenu( "Daikon Forge/Examples/Game Menu/Panel Base Class" )]
public abstract class DemoPanelBase : MonoBehaviour
{

	protected static Stack<DemoPanelBase> panelStack = new Stack<DemoPanelBase>();

	#region Private variables

	protected dfControl owner;

	#endregion

	#region Public methods 

	public virtual void Show()
	{

		stopAllTweens();

		owner.Show();
		owner.BringToFront();

		var showTween = GetComponents<dfTweenPlayableBase>().First( i => i.TweenName == "Show" );
		showTween.Play();

	}

	public virtual void Hide()
	{

		stopAllTweens();

		owner.Unfocus();
		owner.IsEnabled = false;

		var tweenGroup = GetComponents<dfTweenPlayableBase>().FirstOrDefault( i => i.TweenName == "Hide" );
		if( tweenGroup != null )
		{
			tweenGroup.Play();
		}

	}

	public virtual void GoBack()
	{

		if( panelStack.Count == 0 )
			return;

		this.Hide();

		var prevPanel = panelStack.Pop();
		prevPanel.Show();

	}

	public virtual void Focus()
	{

		owner.Show();
		owner.Enable();

		if( !owner.ContainsFocus )
		{
			owner.Focus();
		}

	}

	#endregion

	#region Unity events

	protected virtual void Awake() { }
	protected virtual void OnEnabled() { }

	protected virtual void Start()
	{
		initialize();
	}

	protected virtual void Update()
	{
	}

	#endregion

	#region Component events

	protected virtual void TweenCompleted( dfTweenPlayableBase tween )
	{

		if( tween.TweenName == "Show" || tween.TweenName == "Submenu Return" )
		{
			Focus();
		}

		if( tween.TweenName == "Hide" )
		{
			owner.Hide();
		}

	}

	protected virtual void EnterFocus( dfControl sender, dfFocusEventArgs args )
	{
		sender.GetRootContainer().BringToFront();
	}

	protected virtual void OnKeyDown( dfControl sender, dfKeyEventArgs args )
	{

		if( args.Used )
			return;

		var goBack =
			args.KeyCode == KeyCode.Escape ||
			args.KeyCode == KeyCode.Backspace ||
			args.KeyCode == KeyCode.Joystick1Button2;

		if( goBack )
		{
			GoBack();
		}

	}

	#endregion

	#region Private utility methods 

	protected virtual void initialize()
	{

		// All menus start out invisible
		owner = GetComponent<dfControl>();
		owner.Opacity = 0f;
		owner.Hide();

	}

	private void stopAllTweens()
	{

		var tweenGroups = GetComponents<dfTweenGroup>();
		for( int i = 0; i < tweenGroups.Length; i++ )
		{
			tweenGroups[ i ].Stop();
		}

	}

	protected dfControl findControl( string name )
	{

		return
			GetComponents<dfControl>()
			.Concat( GetComponentsInChildren<dfControl>() )
			.Where( component => component.name == name )
			.FirstOrDefault();

	}

	#endregion

}
