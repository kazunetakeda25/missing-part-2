/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"NavMeshBase.cs"
 * 
 *	A base class for NavigationMesh and NavMeshSegment
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	public class NavMeshBase : MonoBehaviour
	{

		public bool disableRenderer = true;
		#if UNITY_5
		public bool ignoreCollisions = true;
		#endif


		protected void BaseAwake ()
		{
			if (disableRenderer)
			{
				Hide ();
			}
			#if !UNITY_5
			if (GetComponent <Collider>())
			{
				GetComponent <Collider>().isTrigger = true;
			}
			#endif
		}


		public void Hide ()
		{
			if (GetComponent <MeshRenderer>())
			{
				GetComponent <MeshRenderer>().enabled = false;
			}
		}


		public void Show ()
		{
			if (GetComponent <MeshRenderer>())
			{
				GetComponent <MeshRenderer>().enabled = true;
				GetComponent <MeshRenderer>().receiveShadows = false;

				if (GetComponent <MeshFilter>() && GetComponent <MeshCollider>() && GetComponent <MeshCollider>().sharedMesh)
				{
					GetComponent <MeshFilter>().mesh = GetComponent <MeshCollider>().sharedMesh;
				}
			}
		}

	}

}
