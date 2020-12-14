/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"AutoLipsync.cs"
 * 
 *	This script provides simple lipsyncing for talking characters, "Half Life 1"-style.
 *	The Transform defined in jawBone will rotate according to the sound that the gameObject is emitting.
 * 
 */

using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[RequireComponent (typeof (AudioSource))]
public class AutoLipSync : MonoBehaviour
{
	
	public Transform jawBone;

	public enum Coord { W, X, Y, Z };
	public Coord coordinateToAffect;
	
	public float rotationFactor = 10f;
	
	private float volume;
	private float bin = 0.04f;
	private int width = 64;
	private float output;

	private float[] array;
	private Quaternion jawRotation;
	private AudioSource _audio;
	
	
	private void Awake ()
	{
		_audio = GetComponent <AudioSource>();
		array = new float[width];	
	}
	
	
	private void FixedUpdate ()
	{
		if (_audio.isPlaying)
		{
			_audio.GetOutputData(array, 0);
			float num3 = 0f;
			for (int i = 0; i < width; i++)
			{
			    float num4 = Mathf.Abs(array[i]);
			    num3 += num4;
			}
			num3 /= (float) width;
			
			// Only record changes big enough
			if (Mathf.Abs (num3 - volume) > bin)
				volume = num3;

			volume = Mathf.Clamp01 (volume * 2);
			volume *= 0.3f;
			
			output = Mathf.Lerp (output, volume, Time.deltaTime * Mathf.Abs (rotationFactor));
		}
		else
		{
			output = 0f;
		}
	}
	
	
	private void LateUpdate ()
	{
		jawRotation = jawBone.localRotation;
		
		if (coordinateToAffect == Coord.W)
		{
			if (rotationFactor < 0)
			{
				jawRotation.w += output;
			}
			else
			{
				jawRotation.w -= output;
			}
		}
		else if (coordinateToAffect == Coord.X)
		{
			if (rotationFactor < 0)
			{
				jawRotation.x += output;
			}
			else
			{
				jawRotation.x -= output;
			}
		}
		else if (coordinateToAffect == Coord.Y)
		{
			if (rotationFactor < 0)
			{
				jawRotation.y += output;
			}
			else
			{
				jawRotation.y -= output;
			}
		}
		else if (coordinateToAffect == Coord.Z)
		{
			if (rotationFactor < 0)
			{
				jawRotation.z += output;
			}
			else
			{
				jawRotation.z -= output;
			}
		}
		
		jawBone.localRotation = jawRotation;
	}
	
}