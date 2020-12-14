/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"SortingArea.cs"
 * 
 *	This script is a container class for individual regions of a SortingMap.
 * 
 */

using UnityEngine;

[System.Serializable]
public class SortingArea
{

	public float z;
	public int order;
	public string layer;
	public Color color;
	public int scale = 100;


	public SortingArea (SortingArea lastArea)
	{
		z = lastArea.z + 1f;
		order = lastArea.order + 1;
		layer = "";
		scale = lastArea.scale;
		color = GetRandomColor ();
	}


	public SortingArea (SortingArea area1, SortingArea area2)
	{
		z = (area1.z + area2.z) / 2f;

		float _avOrder = (float) area1.order + (float) area2.order;
		order = (int) (_avOrder / 2f);

		float _avScale = (float) area1.scale + (float) 	area2.scale;
		scale = (int) (_avScale / 2f);

		layer = "";
		color = GetRandomColor ();
	}


	public SortingArea (float _z, int _order)
	{
		z = _z;
		order = _order;
		layer = "";
		scale = 100;
		color = GetRandomColor ();
	}


	private Color GetRandomColor ()
	{
		return new Color (Random.Range (0f, 1f),Random.Range (0f, 1f), Random.Range (0f, 1f));
	}

}