using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu( "Daikon Forge/Examples/Touch/Drag Source Events" )]
public class TouchDragSourceEvents : MonoBehaviour
{

	private dfLabel _label;
	private bool isDragging = false;

	// Called by Unity just before any of the Update methods is called the first time.
	public void Start()
	{
		// Obtain a reference to the dfLabel instance attached to this object
		this._label = GetComponent<dfLabel>();
	}

	public void OnGUI()
	{

		if( !isDragging )
			return;

		var pos = Input.mousePosition;
		pos.y = Screen.height - pos.y;

		var rect = new Rect( pos.x - 100, pos.y - 50, 200, 100 );
		GUI.Box( rect, _label.name );

	}

	public void OnDragEnd( dfControl control, dfDragEventArgs dragEvent )
	{

		if( dragEvent.State == dfDragDropState.Dropped )
		{
			_label.Text = "Dropped on " + dragEvent.Target.name;
		}
		else
		{
			_label.Text = "Drag Ended: " + dragEvent.State;
		}

		isDragging = false;

	}

	public void OnDragStart( dfControl control, dfDragEventArgs dragEvent )
	{

		_label.Text = "Dragging...";

		dragEvent.Data = this.name;
		dragEvent.State = dfDragDropState.Dragging;
		dragEvent.Use();

		isDragging = true;

	}

}
