/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"Container.cs"
 * 
 *	This script is used to store a set of
 *	Inventory items in the scene, to be
 *	either taken or added to by the player.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{

	public class Container : MonoBehaviour
	{

		public List<ContainerItem> items = new List<ContainerItem>();


		public void Interact ()
		{
			KickStarter.playerInput.activeContainer = this;
		}


		public void Add (int _id, int amount)
		{
			// Raise "count" by 1 for appropriate ID
			foreach (ContainerItem containerItem in items)
			{
				if (containerItem.linkedID == _id)
				{
					if (KickStarter.inventoryManager.CanCarryMultiple (containerItem.linkedID))
					{
						containerItem.count += amount;
					}
					return;
				}
			}

			// Not already carrying the item
			foreach (InvItem assetItem in KickStarter.inventoryManager.items)
			{
				if (assetItem.id == _id)
				{
					if (!KickStarter.inventoryManager.CanCarryMultiple (_id))
					{
						amount = 1;
					}

					items.Add (new ContainerItem (_id, amount, GetIDArray ()));
				}
			}
		}
		
		
		public void Remove (int _id, int amount)
		{
			// Reduce "count" by 1 for appropriate ID
			
			foreach (ContainerItem item in items)
			{
				if (item.linkedID == _id)
				{
					if (item.count > 0)
					{
						item.count -= amount;
					}
					if (item.count < 1)
					{
						items.Remove (item);
					}
					return;
				}
			}
		}


		public int GetCount (int _id)
		{
			foreach (ContainerItem item in items)
			{
				if (item.linkedID == _id)
				{
					return (item.count);
				}
			}
			
			return 0;
		}


		public void InsertAt (InvItem _item, int _index)
		{
			ContainerItem newContainerItem = new ContainerItem (_item.id, GetIDArray ());
			newContainerItem.count = _item.count;

			if (items.Count <= _index)
			{
				items.Add (newContainerItem);
			}
			else
			{
				items.Insert (_index, newContainerItem);
			}
		}


		public int[] GetIDArray ()
		{
			// Returns a list of id's in the list
			
			List<int> idArray = new List<int>();
			
			foreach (ContainerItem item in items)
			{
				idArray.Add (item.id);
			}
			
			idArray.Sort ();
			return idArray.ToArray ();
		}

	}

}