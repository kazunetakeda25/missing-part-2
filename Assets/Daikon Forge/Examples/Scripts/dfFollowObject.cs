using UnityEngine;
using System.Collections;

[AddComponentMenu( "Daikon Forge/Examples/General/Follow Object" )]
public class dfFollowObject : MonoBehaviour
{

	public GameObject attach;
	public Vector3 offset;
	public float hideDistance = 20;
	public float fadeDistance = 15;
	public bool constantScale = false;

	private dfControl myControl;

	void OnEnable()
	{

		if( attach == null )
		{
			Debug.LogError( "Configuration incomplete: " + this.name );
		}

		myControl = GetComponent<dfControl>();
		if( myControl == null )
		{
			Debug.LogError( "No dfControl component on this GameObject: " + gameObject.name );
			this.enabled = false;
		}

	}

	void Update()
	{

		if( myControl == null || attach == null )
			return;

		var targetPosition = attach.transform.position;
		var mainCamera = Camera.main;

		// If the object is not in the frustum, then hide the control
		var frustum = GeometryUtility.CalculateFrustumPlanes( mainCamera );
		if( !GeometryUtility.TestPlanesAABB( frustum, attach.GetComponent<Collider>().bounds ) )
		{
			myControl.enabled = false;
			return;
		}
		else
		{
			myControl.enabled = true;
		}

		var cameraDistance = Vector3.Distance( mainCamera.transform.position, targetPosition );
		if( cameraDistance > hideDistance )
		{
			// Hide the control after a given distance
			myControl.Opacity = 0f;
			return;
		}
		else if( cameraDistance > fadeDistance )
		{
			// Apply fade 
			myControl.Opacity = 1.0f - ( cameraDistance - fadeDistance ) / ( hideDistance - fadeDistance );
		}
		else
		{
			// Fully visible
			myControl.Opacity = 1.0f;
		}

		// Calculate 3D point of attachment
		var offsetPoint = attach.transform.position + offset;

		// Obtain a reference to the dfGUIManager rendering the control
		var manager = myControl.GetManager();

		// Convert world point to resolution-independant screen point
		var screenPoint = manager.WorldPointToGUI( offsetPoint );

		// Calulate resolution adjustment
		if( !manager.PixelPerfectMode )
		{
			if( constantScale )
				myControl.transform.localScale = Vector3.one * ( manager.FixedHeight / mainCamera.pixelHeight );
			else
				myControl.transform.localScale = Vector3.one;
		}

		// Center control over the followed object
		screenPoint.x -= ( myControl.Width / 2 ) * myControl.transform.localScale.x;
		screenPoint.y -= myControl.Height * myControl.transform.localScale.y;

		// Position control on screen
		myControl.RelativePosition = screenPoint;

	}

}
