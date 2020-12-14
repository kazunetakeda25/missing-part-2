/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"JointBreaker.cs"
 * 
 *	This script is used by the PickUp script to
 *	clean up FixedJoints after they've broken
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	public class JointBreaker : MonoBehaviour
	{

		private void OnJointBreak (float breakForce)
		{
			GetComponent <FixedJoint>().connectedBody.GetComponent <Moveable_PickUp>().UnsetFixedJoint ();
			Destroy (this.gameObject);
		}

	}

}