using UnityEngine;
using System.Collections;

public class MakeRoomInParent : MonoBehaviour {

	void Start () 
	{

		// There should only be 3 elements in the container
		if( transform.parent.childCount > 3 )
		{
			foreach( Transform sibling in transform.parent )
			{
				if( sibling == transform )
					continue;

				Destroy( sibling.gameObject );
			}
		}

	}
}
