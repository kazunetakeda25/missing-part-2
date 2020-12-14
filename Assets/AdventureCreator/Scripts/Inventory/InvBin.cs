/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"InvBin.cs"
 * 
 *	This script is a container class for inventory item categories.
 * 
 */


using UnityEngine;
using System.Collections;

[System.Serializable]
public class InvBin
{

	public string label;
	public int id;


	public InvBin ()
	{
		label = "";
		id = 0;
	}
	
	
	public InvBin (int[] idArray)
	{
		id = 0;

		foreach (int _id in idArray)
		{
			if (id == _id)
			{
				id ++;
			}
		}

		label = "Category " + (id + 1).ToString ();
	}

}
