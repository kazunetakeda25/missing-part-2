using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Used to allow UI components to be displayed "in level" in full 3D by
/// "attaching" them to another GameObject
/// </summary>
[AddComponentMenu( "Daikon Forge/Examples/3D/Follow Object (3D)" )]
public class dfFollowObject3D : MonoBehaviour 
{

	public Transform attachedTo;

	private dfControl control;

	public void OnEnable()
	{
		control = GetComponent<dfControl>();
		updatePosition3D();
	}

	public void Update()
	{
		updatePosition3D();
	}

	public void OnDrawGizmos()
	{

		if( control == null )
			control = GetComponent<dfControl>();

		var size = ( (Vector3)control.Size ) * control.PixelsToUnits();

		Gizmos.matrix = Matrix4x4.TRS( attachedTo.position, attachedTo.rotation, attachedTo.localScale );

		Gizmos.color = Color.yellow;
		Gizmos.DrawWireCube( Vector3.zero, size );

	}

	public void OnDrawGizmosSelected()
	{
		OnDrawGizmos();
	}

	private void updatePosition3D()
	{

		if( attachedTo == null )
			return;

		control.transform.position = attachedTo.position;
		control.transform.rotation = attachedTo.rotation;
		control.transform.localScale = attachedTo.localScale;

	}

}
