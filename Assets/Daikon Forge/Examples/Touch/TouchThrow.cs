using UnityEngine;
using System.Collections;

[AddComponentMenu( "Daikon Forge/Examples/Touch/Touch Throw" )]
public class TouchThrow : MonoBehaviour
{

	private dfControl control;
	private dfGUIManager manager;

	private Vector2 momentum;
	private Vector3 lastPosition;

	private bool animating = false;
	private bool dragging = false;

	public void Start()
	{
		this.control = GetComponent<dfControl>();
		this.manager = control.GetManager();
	}

	public void Update()
	{
		
		var screenSize = control.GetManager().GetScreenSize();
		var position = (Vector2)control.RelativePosition;
		var newPosition = position;

		if( animating )
		{

			if( position.x + momentum.x < 0 || position.x + momentum.x + control.Width > screenSize.x )
				momentum.x *= -1;

			if( position.y + momentum.y < 0 || position.y + momentum.y + control.Height > screenSize.y )
				momentum.y *= -1;

			newPosition += momentum;
			momentum *= ( 1f - Time.fixedDeltaTime );

		}

		newPosition = Vector2.Max( Vector2.zero, newPosition );
		newPosition = Vector2.Min( screenSize - control.Size, newPosition );

		if( Vector2.Distance( newPosition, position ) > float.Epsilon )
		{
			control.RelativePosition = newPosition;
		}

	}

	public void OnMultiTouch( dfControl control, dfTouchEventArgs touchData )
	{

		this.momentum = Vector2.zero;

		control.Color = Color.yellow;

		// Determine movement for each finger
		var touch1 = touchData.Touches[ 0 ];
		var touch2 = touchData.Touches[ 1 ];
		var dir1 = ( touch1.deltaPosition * ( Time.deltaTime / touch1.deltaTime ) ).Scale( 1, -1 );
		var dir2 = ( touch2.deltaPosition * ( Time.deltaTime / touch2.deltaTime ) ).Scale( 1, -1 );

		// Calculate size delta
		var pos1 = screenToGUI( touch1.position );
		var pos2 = screenToGUI( touch2.position );
		var currDist = pos1 - pos2;
		var prevDist = ( pos1 - dir1 ) - ( pos2 - dir2 );
		var delta = currDist.magnitude - prevDist.magnitude;

		if( Mathf.Abs( delta ) > float.Epsilon )
		{

			// Save relative position to restore after control's Size has changed
			var min = Vector3.Min( pos1, pos2 );
			var offset = min - control.RelativePosition;

			// Adjust the control's size according to the touch delta
			control.Size += Vector2.one * delta;

			// Put control back in relative position
			control.RelativePosition = min + offset;

		}

	}

	private Vector2 screenToGUI( Vector2 position )
	{
		position.y = manager.GetScreenSize().y - position.y;
		return position;
	}

	public void OnMouseMove( dfControl control, dfMouseEventArgs args )
	{

		if( animating || !dragging )
			return;

		this.momentum = ( momentum + args.MoveDelta.Scale( 1, -1 ) ) * 0.5f;

		args.Use();

		if( args.Buttons.IsSet( dfMouseButtons.Left ) )
		{

			var ray = args.Ray;
			var distance = 0f;
			var direction = Camera.main.transform.TransformDirection( Vector3.back );
			var plane = new Plane( direction, lastPosition );
			plane.Raycast( ray, out distance );

			var pos = ( ray.origin + ray.direction * distance ).Quantize( control.PixelsToUnits() );
			var offset = pos - lastPosition;

			var transformPos = ( control.transform.position + offset ).Quantize( control.PixelsToUnits() );
			control.transform.position = transformPos;

			lastPosition = pos;

		}

	}

	public void OnMouseEnter( dfControl control, dfMouseEventArgs args )
	{
		control.Color = Color.white; 
	}

	public void OnMouseDown( dfControl control, dfMouseEventArgs args )
	{

		control.BringToFront();

		animating = false;
		momentum = Vector2.zero;

		dragging = true;
		args.Use();

		var plane = new Plane( control.transform.TransformDirection( Vector3.back ), control.transform.position );
		var ray = args.Ray;

		var distance = 0f;
		plane.Raycast( args.Ray, out distance );

		lastPosition = ( ray.origin + ray.direction * distance );

	}

	public void OnMouseUp()
	{
		animating = true;
		dragging = false;
		control.Color = Color.white;
	}

}
