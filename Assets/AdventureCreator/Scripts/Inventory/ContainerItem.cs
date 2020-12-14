/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ContainerItem.cs"
 * 
 *	This script is a container class for inventory items stored in a Container.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	[System.Serializable]
	public class ContainerItem
	{

		public int linkedID;
		public int count;
		public int id;


		public ContainerItem (int _linkedID, int[] idArray)
		{
			count = 1;
			linkedID = _linkedID;
			id = 0;
			
			// Update id based on array
			foreach (int _id in idArray)
			{
				if (id == _id)
					id ++;
			}
		}


		public ContainerItem (int _linkedID, int _count, int[] idArray)
		{
			count = _count;
			linkedID = _linkedID;
			id = 0;
			
			// Update id based on array
			foreach (int _id in idArray)
			{
				if (id == _id)
					id ++;
			}
		}


		public ContainerItem (int _linkedID, int _count, int _id)
		{
			linkedID = _linkedID;
			count = _count;
			id = _id;
		}

	}

}