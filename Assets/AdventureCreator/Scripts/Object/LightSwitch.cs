/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"LightSwitch.cs"
 * 
 *	This can be used, via the Object: Send Message Action,
 *	to turn it's attached light component on and off.
 * 
 */

using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Light))]
public class LightSwitch : MonoBehaviour
{
	
	public bool enableOnStart = false;
	
	
	private void Awake ()
	{
		Switch (enableOnStart);
	}
	
	
	public void TurnOn ()
	{
		Switch (true);
	}
	
	
	public void TurnOff ()
	{
		Switch (false);
	}


	private void Switch (bool turnOn)
	{
		GetComponent <Light>().enabled = turnOn;
	}
	
}
